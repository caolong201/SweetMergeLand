using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using USimpFramework.Animation.DOTweenExtension;
using USimpFramework.EditorExtension;
using USimpFramework.Utility;
namespace TheKingOfMergeCity
{
    using Enum;
    using Config;

    public class DecoItemController : MonoBehaviour
    {
        public System.Action<DecoItemController, DecoItemState> onDecoItemStateChanged;

        [SerializeField] string _decoId;
        public string decoId => _decoId;

        [SerializeField] GameObject worldObject;

        [SerializeField] GameObject previewWorldObject;

        [SerializeField] List<Transform> workerSpawnTrans;

        [Header("Dissolve effect")]
        [SerializeField] Transform topTrans;
        [SerializeField] Transform bottomTrans;
        [SerializeField] Transform dissoveControllerTarget;
        [SerializeField] float showUpDuration = 1f;
        [SerializeField] int showUpCount = 4;
        [SerializeField] MoveDirection moveDirection = MoveDirection.Y; 

        [Header("Scale effect")]
        [SerializeField] AnimationCurve scaleCurveX;
        [SerializeField] AnimationCurve scaleCurveY;
        [SerializeField] AnimationCurve scaleCurveZ;
        [SerializeField] bool scaleAtChild;

        [Header("Particle effect")]
        [SerializeField] bool playVfxAtChild;
        [SerializeField] List<ParticleSystem> buildingVfxs;
        [SerializeField] List<ParticleSystem> buildingCompletedVfxs;

        DecoItemState _decoItemState;
        public DecoItemState decoItemState
        {
            get => _decoItemState;
            private set
            {
                if (_decoItemState == value)
                    return;
                var oldState = _decoItemState;
                _decoItemState = value;
                OnDecoItemStateChanged(oldState);

            }
        }

        public ConfigDecoItem config { get; private set; }

        List<Vector3> originalChildScales = new();
        List<WorkerController> workers = new();
        List<Animator> childAnimators = new();

        void Start()
        {
            foreach (Transform child in previewWorldObject.transform)
            {
                originalChildScales.Add(child.localScale);
            }

            foreach (Transform child in worldObject.transform)
            {
                var animator = child.GetComponentsInChildren<Animator>();

                if (animator != null)
                {
                    childAnimators.AddRange(animator);
                }
            }
        }

       
        public void Setup()
        {
            config = HomeManager.Instance.currentConfigArea.decoItems.Find(d => d.id == decoId);

            childAnimators.ForEach(a => a.enabled = false);

            previewWorldObject.SetActive(false);

            dissoveControllerTarget.position = bottomTrans.position;
        }

        public void UpdateState()
        {
            var completedDecos = UserManager.Instance.currentSelectAreaData.completedDecoIds;
            var decoData = completedDecos.Find(s => s == decoId);
            decoItemState = DecoItemState.Lock;
           
            if (decoData != null)
            {
                //Check first, if the game is playing the building completed effect, we will hardcode that only check for the last completed decos
                if (decoData == completedDecos[^1])
                {
                    if (UserManager.Instance.isPlayingDecoBuildingFromInGameScene)
                    {
                        decoItemState = DecoItemState.Unlock;
                        HomeManager.Instance.PlayBuildingDecoFlow(decoId, 0.8f);
                    }
                    else
                    {
                        decoItemState = DecoItemState.Completed;
                    }
                }
                else
                {
                    decoItemState = DecoItemState.Completed;
                }
            }
            else
            {
                if (completedDecos.Count == 0)
                {
                    if (decoId == HomeManager.Instance.currentConfigArea.decoItems[0].id) //The first item of newly unlocked area will always be unlocked
                    {
                        decoItemState = DecoItemState.Unlock;
                    }
                }
            }
        }

        void OnDecoItemStateChanged(DecoItemState oldState)
        {
            worldObject.SetActive(false);
            previewWorldObject.SetActive(false);

            if (decoItemState == DecoItemState.Lock)
            {
                childAnimators.ForEach(a => a.enabled = false);
            }
            else if (decoItemState == DecoItemState.Completed)
            {
                worldObject.SetActive(true);
                childAnimators.ForEach(a => a.enabled = true);

            }
            else if (decoItemState == DecoItemState.Unlock)
            {
                previewWorldObject.SetActive(true);
            }
            
            onDecoItemStateChanged?.Invoke(this, oldState);
        }

        public void Unlock()
        {
            if (decoItemState == DecoItemState.Lock)
            {
                GameAnalyticsManager.Instance.SendDecoBuildStartEvent(UserManager.Instance.currentSelectAreaId,
                        HomeManager.Instance.currentConfigArea.decoItems.FindIndex(s => s.id == config.id));
            }
           
            previewWorldObject.SetActive(true);
            float duration = 0.4f;
            foreach (Transform child in previewWorldObject.transform)
            {
                child.DOScale(originalChildScales[child.GetSiblingIndex()], duration).SetEase(Ease.OutBack).From(0);
            }

            DOVirtual.DelayedCall(duration, () =>
            {
                worldObject.SetActive(true);
                decoItemState = DecoItemState.Unlock;
            });
        }

        [SimpleInspectorButton("Test Build")]
        void TestBuild()
        {
            //Reset first
            previewWorldObject.SetActive(true);
            worldObject.SetActive(true);
            dissoveControllerTarget.position = bottomTrans.position;

            Build();
        }

        [SimpleInspectorButton("Test Lock")]
        void TestLock()
        {
            previewWorldObject.SetActive(false);
            worldObject.SetActive(false);
        }

        [SimpleInspectorButton("Test Unlock")]
        void TestUnlock()
        {
            previewWorldObject.SetActive(true);

            worldObject.SetActive(false);
        }

        /// <summary>  Build deco, note that this is just the effect transition, user data will not wait until this transition is saved </summary>
        public void Build()
        {
            var waitBetweenShowUp = showUpDuration / showUpCount;
            worldObject.SetActive(true);
            var homeManager = HomeManager.Instance;

            StartCoroutine(CR_Build());

            IEnumerator CR_Build()
            {
                //Spawn worker first
                foreach (var spawnTrans in workerSpawnTrans)
                {
                    var worker = SimpleObjectPool.Spawn(HomeManager.Instance.workerPrefab, transform.parent);

                    worker.Setup(HomeManager.Instance.currentConfigArea.workerModel);
                    worker.transform.localScale = Vector3.zero;
                    worker.transform.SetPositionAndRotation(spawnTrans.position, spawnTrans.rotation);
                    worker.transform.DOPopIn(0.3f).OnComplete(() => worker.Build());
                    workers.Add(worker);
                }

                yield return new WaitForSeconds(0.7f);

                //Setup building vfx
                for (int i = 1; i <= showUpCount; i++)
                {
                    var pos = dissoveControllerTarget.position;
                    switch (moveDirection)
                    {
                        case MoveDirection.X: pos.x = Mathf.Lerp(bottomTrans.position.x, topTrans.position.x, (float)i / showUpCount); break;
                        case MoveDirection.Y: pos.y = Mathf.Lerp(bottomTrans.position.y, topTrans.position.y, (float)i / showUpCount); break;
                        case MoveDirection.Z: pos.z = Mathf.Lerp(bottomTrans.position.z, topTrans.position.z, (float)i / showUpCount); break;
                    }

                    dissoveControllerTarget.position = pos;

                    if (scaleAtChild)
                    {
                        foreach (Transform child in previewWorldObject.transform)
                        {
                            StartCoroutine(CR_Play(child.transform, waitBetweenShowUp, child.transform.localScale.x, scaleCurveX, scaleCurveY, scaleCurveZ));
                        }

                        foreach (Transform child in worldObject.transform)
                        {
                            StartCoroutine(CR_Play(child.transform, waitBetweenShowUp, child.transform.localScale.x, scaleCurveX, scaleCurveY, scaleCurveZ));
                        }
                    }
                    else
                    {
                        StartCoroutine(CR_Play(previewWorldObject.transform, waitBetweenShowUp, 1, scaleCurveX, scaleCurveY, scaleCurveZ));
                        StartCoroutine(CR_Play(worldObject.transform, waitBetweenShowUp, 1, scaleCurveX, scaleCurveY, scaleCurveZ));
                    }

                    if (buildingVfxs.Count > 0)
                    {
                        buildingVfxs.ForEach(b => b.Play());
                    }
                    else
                    {
                        if (playVfxAtChild)
                        {
                            foreach (Transform child in worldObject.transform)
                            {
                                homeManager.PlayBuildingVfx(child.position);
                            }
                        }
                        else
                        {
                            homeManager.PlayBuildingVfx(worldObject.transform.position);
                        }
                    }
                   
                    yield return new WaitForSeconds(waitBetweenShowUp);
                }

                if (buildingCompletedVfxs.Count > 0)
                {
                    buildingCompletedVfxs.ForEach(b =>
                    {
                        b.Play();
                    });
                }
                else
                {
                    if (playVfxAtChild)
                    {
                        foreach (Transform child in worldObject.transform)
                        {
                            homeManager.PlayBuildingCompletedVfx(child.position);
                        }
                    }
                    else
                    {
                        homeManager.PlayBuildingCompletedVfx(worldObject.transform.position);
                    }
                }

                //Hide worker
                foreach (var worker in workers)
                {
                    worker.transform.DOPopOut(0.3f).OnComplete(() => SimpleObjectPool.Despawn(worker));
                }

                workers.Clear();

                yield return new WaitForSeconds(0.5f);

                decoItemState = DecoItemState.Completed;
            }
        }

        public Transform GetFirstDecoChild()
        {
            return scaleAtChild ? worldObject.transform.GetChild(0) : worldObject.transform;
        }
        
        IEnumerator CR_Play(Transform target, float duration, float endScale, AnimationCurve curveX, AnimationCurve curveY, AnimationCurve curveZ, System.Action onCompleted = null)
        {
            float elapse = 0;
            var curScale = target.localScale;
            while (elapse < duration)
            {
                var fraction = elapse / duration;
                curScale.x = curveX.Evaluate(fraction) * endScale;
                curScale.y = curveY.Evaluate(fraction) * endScale;
                curScale.z = curveZ.Evaluate(fraction) * endScale;

                target.localScale = curScale;

                elapse += Time.deltaTime;
                yield return null;
            }

            target.localScale = Vector3.one * endScale;
            onCompleted?.Invoke();
        }


    }
}
