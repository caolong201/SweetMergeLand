using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using USimpFramework.Utility;
using USimpFramework.Animation.DOTweenExtension;
using USimpFramework.UI;

namespace TheKingOfMergeCity
{
    public class UIAreaCompleted : MonoBehaviour
    {
        public event System.Action onLoadAreaCompleted;
        
        [SerializeField] CanvasGroup boardCanvasGroup;
        [SerializeField] FloatingObject decoTrans;
        [SerializeField] Transform contentTrans;
        [SerializeField] Image areaIconImage;
        [SerializeField] CanvasGroup containerAreaCanvasGroup;
        [SerializeField] Image shineImg;
        [SerializeField] Image radialShineImg1;
        [SerializeField] Image radialShineImg2;
        [SerializeField] Transform textAreaUnlockedTrans;
        [SerializeField] Button unlockButton;
        [SerializeField] ParticleSystem candyVfx;
        [SerializeField] GameObject goNewAreaUnlock;
        [SerializeField] GameObject goAllAreaCompleted;
        [Header("Setting")]
        [SerializeField] float rotateSpeed;

        int areaId;

        public void Show(int areaId)
        {
            this.areaId = areaId;

            var configAreas = ConfigManager.Instance.configArea.areaItems;
            bool isLastArea = areaId >= configAreas.Count;

            goNewAreaUnlock.SetActive(!isLastArea);
            goAllAreaCompleted.SetActive(isLastArea);

            transform.SetAsLastSibling();
            transform.localScale = Vector3.one;
            gameObject.SetActive(true);
            candyVfx.Play();

            StartCoroutine(CR_Show());

            if (!isLastArea)
            {
                var configArea = ConfigManager.Instance.configArea.areaItems.Find(c => c.id == areaId);
                if (configArea == null)
                {
                    Debug.LogError("Invalid area id " + areaId);
                    return;
                }

                areaIconImage.sprite = configArea.iconSprite;
            }

            IEnumerator CR_Show()
            {
                shineImg.SetAlpha(0);
                radialShineImg1.SetAlpha(0);
                radialShineImg2.SetAlpha(0);

                boardCanvasGroup.DOFade(1, 0.5f).SetEase(Ease.Linear).From(0);
                var tween = contentTrans.DOScale(1f, 0.5f).SetEase(Ease.OutBack).From(0.7f);
                unlockButton.gameObject.SetActive(false);
                containerAreaCanvasGroup.gameObject.SetActive(false);

                yield return tween.WaitForCompletion();

                shineImg.DOFade(1, 0.5f).SetEase(Ease.Linear).SetDelay(0.3f);
                radialShineImg1.DOFade(1, 0.5f).SetEase(Ease.Linear).SetDelay(0.3f);
                radialShineImg2.DOFade(1, 0.5f).SetEase(Ease.Linear).SetDelay(0.3f);

                if (isLastArea)
                {

                }
                else
                {
                    containerAreaCanvasGroup.gameObject.SetActive(true);
                    containerAreaCanvasGroup.DOFade(1f, 0.4f).SetEase(Ease.Linear).SetDelay(0.1f).From(0f);
                    var tween2 = containerAreaCanvasGroup.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).From(1.4f).SetDelay(0.1f);

                    yield return tween2.WaitForCompletion();

                    unlockButton.gameObject.SetActive(true);
                    unlockButton.transform.DOScaleX(1f, 0.5f).SetEase(Ease.OutBack).From(0);
                }
            }
        }

        void Update()
        {
            radialShineImg1.transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
            radialShineImg2.transform.Rotate(Vector3.forward, -rotateSpeed * Time.deltaTime);
        }

        public void PressUnlock()
        {
            UserManager.Instance.SetCurrentSelectAreaId(areaId);
            UserManager.Instance.isPlayingDecoBuildingFromInGameScene = false;

            Hide();

            UIManager.Instance.ShowLoading(true, () =>
            {
                //Switch to new scene
                UIManager.Instance.HideAllView();
                HomeManager.Instance.LoadArea(areaId, () =>
                {
                    UIManager.Instance.ShowLoading(false);
                    onLoadAreaCompleted?.Invoke();
                });
            });
        }

        public void PressClose()
        {
            Hide();

            var userManager = UserManager.Instance;
            
            if (userManager.isPendingPlayPlayerLevelUp)
            {
                HomeManager.Instance.ShowLevelUpReward();
            }
            else
            {
                if (userManager.isPlayingDecoBuildingFromInGameScene)
                {
                    userManager.isPlayingDecoBuildingFromInGameScene = false;
                    BootManager.Instance.LoadScene(SceneConstants.IN_GAME_SCENE_NAME, true);
                }
            }
        }

        public void Hide()
        {
            shineImg.SetAlpha(0);
            radialShineImg1.SetAlpha(0);
            radialShineImg2.SetAlpha(0);

            candyVfx.Stop();
            transform.DOPopOut(0.5f);
        }
    }
}
