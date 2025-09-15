using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using ICommonUtils = USimpFramework.Utility.CommonUtils;

namespace USimpFramework.Animation.DOTweenExtension
{
    [AddComponentMenu("InteractiveSeven/DOTween Animation Setting Controller")]
    public sealed class DOTweenAnimationSettingController : MonoBehaviour
    {
        public enum EventToPlay
        {
            None = 0,
            Awake = 1,
            Start = 2,
            OnEnable = 3,
        }

        public enum PlayMode
        {
            /// <summary> Sequence mode will play tweens sequently, the completed event is the sequence completed event</summary>
            Sequence = 0,

            /// <summary>Parallel mode will play tweens simultaneously, the completed event is the returned tween index completed event</summary>
            Parallel = 1
        }

        public class SourceStateInfo
        {
            public Vector3 position;
            public Quaternion rotation;
            public Vector3 scale;
            public float colorAlpha;
            public Vector2 anchorPos;
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector2 pivot;
            public string textInput; // For text message
        }

        [Serializable]
        internal class DOTweenAnimationSettingCollection
        {
            public PlayMode playMode;
            public List<DOTweenAnimationSetting> animations;

            [Tooltip("The index of returned tween when in parallel mode, has no effect when in sequence mode")]
            public int returnedTweenIndex;

            Transform source;
            List<DOTweenAnimationSetting> executedAnimations = new();
            List<Tween> parallelTweens = new();
            Sequence currentSequence;


            public void Init(Transform source)
            {
                this.source = source;
            }

            public void Kill()
            {
                if (currentSequence == null)
                    return;

                if (currentSequence.IsActive() && currentSequence.IsPlaying())
                {
                    currentSequence.Kill();
                }
            }

            public Sequence Play()
            {
                var result = DOTween.Sequence();

                executedAnimations.Clear();
                parallelTweens.Clear();

                animations.ForEach(a => AddToExecutedAnimations(a.animationType, source.transform, a));

                bool tweenFound = false;
                for (int i = 0; i < executedAnimations.Count; i++)
                {
                    var animation = executedAnimations[i];
                    var tween = animation.GetTween();

                    if (tween == null)
                    {
                        Debug.LogWarning("Animation " + i + " is invalid!");
                        continue;
                    }

                    if (playMode == PlayMode.Sequence)
                    {
                        result.Append(tween);
                    }
                    else if (playMode == PlayMode.Parallel)
                    {
                        if (returnedTweenIndex == i)
                        {
                            result.Append(tween);
                            tweenFound = true;
                        }

                        parallelTweens.Add(tween);
                    }
                }

                if (playMode == PlayMode.Parallel && !tweenFound && executedAnimations.Count > 0)
                {
                    AddToExecutedAnimations(executedAnimations[0].animationType, source, executedAnimations[0]);
                }

                if (playMode == PlayMode.Parallel)
                {
                    for (int i = 0; i < parallelTweens.Count; i++)
                    {
                        if (i == returnedTweenIndex)
                            continue;
                        parallelTweens[i].Play();
                    }
                }

                currentSequence = result.Play();

                return result;
            }

            void AddToExecutedAnimations<T>(Transform sourceTransform, DOTweenAnimationSetting source) where T : DOTweenAnimationSetting
            {
                var newAnim = Activator.CreateInstance<T>();
                ICommonUtils.DeepCopy(newAnim, source);
                newAnim.Init(sourceTransform);
                executedAnimations.Add(newAnim);
            }

            void AddToExecutedAnimations(AnimationType animationType, Transform sourceTransform, DOTweenAnimationSetting source)
            {
                switch (animationType)
                {
                    case AnimationType.Move: AddToExecutedAnimations<DOTweenMoveSetting>(sourceTransform, source); break;
                    case AnimationType.LocalMove: AddToExecutedAnimations<DOTweenLocalMoveSetting>(sourceTransform, source); break;
                    case AnimationType.Fade:
                        {
                            if (sourceTransform.TryGetComponent<SpriteRenderer>(out _))
                            {
                                AddToExecutedAnimations<DOTweenSpriteSetting>(sourceTransform, source);
                            }
                            else if (sourceTransform.TryGetComponent<Image>(out _))
                            {
                                AddToExecutedAnimations<DOTweenImageSetting>(sourceTransform, source);
                            }
                            else if (sourceTransform.TryGetComponent<TMP_Text>(out _))
                            {
                                AddToExecutedAnimations<DOTweenTMPTextFadeSetting>(sourceTransform, source);
                            }
                            else if (sourceTransform.TryGetComponent<CanvasGroup>(out _))
                            {
                                AddToExecutedAnimations<DOTweenCanvasGroupSetting>(sourceTransform, source);
                            }

                            break;
                        }
                    case AnimationType.Scale: AddToExecutedAnimations<DOTweenScaleSetting>(sourceTransform, source); break;
                    case AnimationType.Jump: AddToExecutedAnimations<DOTweenJumpSetting>(sourceTransform, source); break;
                    case AnimationType.LocalJump: AddToExecutedAnimations<DOTweenLocalJumpSetting>(sourceTransform, source); break;
                    case AnimationType.AnchorPos: AddToExecutedAnimations<DOTweenAnchorPosSetting>(sourceTransform, source); break;
                    case AnimationType.AnchorMin: AddToExecutedAnimations<DOTweenAnchorMinSetting>(sourceTransform, source); break;
                    case AnimationType.AnchorMax: AddToExecutedAnimations<DOTweenAnchorMaxSetting>(sourceTransform, source); break;
                    case AnimationType.Pivot: AddToExecutedAnimations<DOTweenPivotSetting>(sourceTransform, source); break;
                    case AnimationType.Rotate: AddToExecutedAnimations<DOTweenRotateSetting>(sourceTransform, source); break;
                    case AnimationType.LocalRotate: AddToExecutedAnimations<DOTweenLocalRotateSetting>(sourceTransform, source); break;
                    case AnimationType.TMP_TextIncrease: AddToExecutedAnimations<DOTweenTMPTextIncrease>(sourceTransform, source); break;
                }
            }
        }

        [SerializeField] List<DOTweenAnimationSettingCollection> _animations;
        internal List<DOTweenAnimationSettingCollection> animations => _animations;

        [Tooltip("The mono behaviour's event to play animation on, set to None to manually play it")]
        public EventToPlay eventToPlayOnType;

        [SerializeField] UnityEvent _onCompleted;
        public UnityEvent onCompleted => _onCompleted;

        SourceStateInfo originalSourceStateInfo;

        /// <summary> The duration of the tween, is calculated after the tween is completed</summary>
        public float duration { get; private set; }

        void Awake()
        {
            if (eventToPlayOnType == EventToPlay.Awake)
                Play();
        }

        void Start()
        {
            if (eventToPlayOnType == EventToPlay.Start)
                Play();
        }

        void OnEnable()
        {
            if (eventToPlayOnType == EventToPlay.OnEnable)
                Play();
        }

        Coroutine playCR;

        public void Play()
        {
            if (originalSourceStateInfo == null)
            {
                SaveSourceStateInfo();
            }
            else
            {
                LoadSourceStateInfo();
            }

            animations.ForEach(a =>
            {
                a.Kill();
                a.Init(transform);
            });
           


            if (playCR != null)
                StopCoroutine(playCR);

            playCR = StartCoroutine(CR_PlayAnimations());

            IEnumerator CR_PlayAnimations()
            {
                for (int i = 0; i < animations.Count; i++)
                {
                    var animation = animations[i];
                    var seq = animation.Play();
                    duration += seq.Duration();
                    yield return new WaitForSeconds(seq.Duration());
                }

                onCompleted?.Invoke();
            }
        }

        void SaveSourceStateInfo()
        {
            originalSourceStateInfo = new SourceStateInfo()
            {
                position = transform.position,
                rotation = transform.rotation,
                scale = transform.localScale,
                colorAlpha = 1,
                anchorMin = new(0.5f, 0.5f),
                anchorMax = new(0.5f, 0.5f),
                pivot = new(0.5f, 0.5f)
            };

            if (transform.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                originalSourceStateInfo.colorAlpha = spriteRenderer.color.a;
            }
            else if (transform.TryGetComponent<Image>(out var image))
            {
                originalSourceStateInfo.colorAlpha = image.color.a;
            }
            else if (transform.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                originalSourceStateInfo.colorAlpha = canvasGroup.alpha;
            }


            if (transform is RectTransform)
            {
                var rectTrans = transform as RectTransform;
                originalSourceStateInfo.anchorPos = rectTrans.anchoredPosition;
                originalSourceStateInfo.anchorMin = rectTrans.anchorMin;
                originalSourceStateInfo.anchorMax = rectTrans.anchorMax;
                originalSourceStateInfo.pivot = rectTrans.pivot;
            }

            if (transform.TryGetComponent<TMP_Text>(out var text))
            {
                originalSourceStateInfo.textInput = text.text;
                originalSourceStateInfo.colorAlpha = text.color.a;

            }
        }

        void LoadSourceStateInfo()
        {
            var data = originalSourceStateInfo;
            transform.SetPositionAndRotation(data.position, data.rotation);
            transform.localScale = data.scale;

            if (transform.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                var color = spriteRenderer.color;
                color.a = data.colorAlpha;
                spriteRenderer.color = color;
            }
            else if (transform.TryGetComponent<Image>(out var image))
            {
                var color = image.color;
                color.a = data.colorAlpha;
                image.color = color;
            }
            else if (transform.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup.alpha = data.colorAlpha;
            }

            if (transform is RectTransform)
            {
                var rectTrans = transform as RectTransform;
                rectTrans.anchoredPosition = data.anchorPos;
                rectTrans.anchorMin = data.anchorMin;
                rectTrans.anchorMax = data.anchorMax;
                rectTrans.pivot = data.pivot;
            }

            if (transform.TryGetComponent<TMP_Text>(out var text))
            {
                text.text = originalSourceStateInfo.textInput;

                var color = text.color;
                color.a = data.colorAlpha;
                text.color = color;
            }
        }



    }

    public enum AnimationType
    {
        None = 0,
        Move = 1,
        LocalMove = 2,
        Scale = 3,
        Fade = 4,
        Jump = 5,
        LocalJump = 6,
        AnchorPos = 7,
        AnchorMin = 8,
        AnchorMax = 9,
        Pivot = 10,
        Rotate = 11,
        LocalRotate = 12,
        TMP_TextIncrease = 13
    }

    [Serializable]
    internal class DOTweenAnimationSetting
    {
        public AnimationType animationType;
        public Ease easeType = Ease.Linear;
        public AnimationCurve easeCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        public bool isFrom = false;

        public bool isValue = true;
        public float endValue;
        public int endValueInt;
        public Vector3 endValueVector3;
        public Vector2 endValueVector2;
        public bool isUniformScale = true;

        public float jumpPower;
        public int numJumps;

        public RotateMode rotateMode;

        public Transform target;

        public float duration;
        public bool isSpeedBased;
        public float delay;

        [Tooltip("the loops of the tween, not set -1 because this use Sequence, set to realy big value instead")]
        [Min(1)] public int loops = 1; 
        public LoopType loopType;

        public bool isValid => source != null;

        public virtual void Init(Transform source)
        {
            this.source = source;
        }

        /// <summary>  The source transform of this class, will be initilize when play </summary>
        public Transform source { get; private set; }

        public Tween GetTween()
        {
            if (!isValid)
                return null;

            var result = Create();

            if (easeType == Ease.INTERNAL_Custom)
            {
                result.SetEase(easeCurve);
            }
            else
            {
                result.SetEase(easeType);
            }

            result.SetSpeedBased(isSpeedBased).SetDelay(delay).SetLoops(loops, loopType).Pause();
            return result;
        }

        protected virtual Tween Create() { return null; }


    }

    [Serializable]
    internal class DOTweenSpriteSetting : DOTweenAnimationSetting
    {
        SpriteRenderer spriteRenderer;

        public new bool isValid => base.isValid && spriteRenderer != null;

        public override void Init(Transform source)
        {
            base.Init(source);
            spriteRenderer = source.GetComponent<SpriteRenderer>();
        }

        protected override Tween Create()
        {
            float finalValue = endValue;
            if (isFrom)
            {
                var color = spriteRenderer.color;
                finalValue = color.a;
                spriteRenderer.color = new Color(color.r, color.g, color.b, endValue);
            }

            Tween result = spriteRenderer.DOFade(finalValue, duration);

            return result;
        }
    }

    [Serializable]
    internal class DOTweenCanvasGroupSetting : DOTweenAnimationSetting
    {
        CanvasGroup canvasGroup;

        public new bool isValid => base.isValid && canvasGroup != null;

        public override void Init(Transform source)
        {
            base.Init(source);
            canvasGroup = source.GetComponent<CanvasGroup>();
        }

        protected override Tween Create()
        {
            float finalValue = endValue;
            if (isFrom)
            {
                finalValue = canvasGroup.alpha;
                canvasGroup.alpha = endValue;
            }

            Tween result = canvasGroup.DOFade(finalValue, duration);

            return result;
        }
    }

    [Serializable]
    internal class DOTweenTMPTextFadeSetting : DOTweenAnimationSetting
    {
        public TMP_Text text;

        public new bool isValid => base.isValid && text != null;

        public override void Init(Transform source)
        {
            base.Init(source);

            text = source.GetComponent<TMP_Text>();
        }

        protected override Tween Create()
        {
            float finalValue = endValue;

            if (isFrom)
            {
                var color = text.color;
                finalValue = color.a;
                text.color = new Color(color.r, color.g, color.b, endValue);

            }

            Tween result = text.DOFade(finalValue, duration);
            return result;
        }
    }

    [Serializable]
    internal class DOTweenImageSetting : DOTweenAnimationSetting
    {
        Image image;

        public new bool isValid => base.isValid && image != null;

        public override void Init(Transform source)
        {
            base.Init(source);
            image = source.GetComponent<Image>();
        }

        protected override Tween Create()
        {
            float finalValue = endValue;
            if (isFrom)
            {
                var color = image.color;
                finalValue = color.a;
                image.color = new Color(color.r, color.g, color.b, endValue);

            }

            Tween result = image.DOFade(finalValue, duration);

            return result;
        }
    }

    [Serializable]
    internal class DOTweenLocalMoveSetting : DOTweenAnimationSetting
    {
        protected override Tween Create()
        {
            Vector3 finalValue = endValueVector3;
            if (isFrom)
            {
                finalValue = source.position;
                source.position = endValueVector3;
            }

            var result = source.DOLocalMove(finalValue, duration);
            return result;
        }
    }

    [Serializable]
    internal class DOTweenMoveSetting : DOTweenAnimationSetting
    {
        protected override Tween Create()
        {
            if (!isValue && target == null)
            {
                Debug.LogWarning("Target is invalid when in target mode, the end position has been set to Vector3.zero");
            }

            Vector3 finalValue = isValue ? endValueVector3 : (target != null ? target.position : Vector3.zero);
            if (isFrom)
            {
                finalValue = source.position;
                source.position = isValue ? endValueVector3 : (target != null ? target.position : Vector3.zero);
            }

            var result = source.DOMove(finalValue, duration);
            return result;
        }
    }

    [Serializable]
    internal class DOTweenScaleSetting : DOTweenAnimationSetting
    {
        protected override Tween Create()
        {
            float finalValue = endValue;
            Vector3 finalValueVector3 = endValueVector3;
            if (isFrom)
            {
                if (isUniformScale)
                {
                    finalValue = source.localScale.x;
                    source.localScale = new Vector3(endValue, endValue, endValue);

                }
                else
                {
                    finalValueVector3 = source.localScale;
                    source.localScale = endValueVector3;
                }

            }

            if (isUniformScale)
            {
                return source.DOScale(finalValue, duration);
            }
            else
            {
                var result = source.DOScaleX(finalValueVector3.x, duration);
                source.DOScaleY(finalValueVector3.y, duration);
                source.DOScaleZ(finalValueVector3.z, duration);
                return result;
            }

        }
    }

    [Serializable]
    internal class DOTweenJumpSetting : DOTweenAnimationSetting
    {
        protected override Tween Create()
        {
            if (!isValue && target == null)
            {
                Debug.LogWarning("Target is invalid when in target mode, the end position has been set to Vector3.zero");
            }

            Vector3 finalValue = isValue ? endValueVector3 : (target != null ? target.position : Vector3.zero);
            if (isFrom)
            {
                finalValue = source.position;
                source.position = isValue ? endValueVector3 : (target != null ? target.position : Vector3.zero);
            }

            var result = source.DOLocalJump(finalValue, jumpPower, numJumps, duration);
            return result;
        }
    }

    [Serializable]
    internal class DOTweenLocalJumpSetting : DOTweenAnimationSetting
    {
        protected override Tween Create()
        {
            Vector3 finalValue = endValueVector3;
            if (isFrom)
            {
                finalValue = source.position;
                source.position = endValueVector3;
            }

            var result = source.DOLocalJump(finalValue, jumpPower, numJumps, duration);
            return result;
        }
    }

    [Serializable]
    internal class DOTweenAnchorPosSetting : DOTweenAnimationSetting
    {
        public new bool isValid => base.isValid && source is RectTransform;

        protected override Tween Create()
        {
            Vector2 finalValue = endValueVector2;
            var sourceRectTrans = source as RectTransform;
            if (isFrom)
            {
                finalValue = sourceRectTrans.anchoredPosition;
                sourceRectTrans.anchoredPosition = endValueVector2;
            }

            return sourceRectTrans.DOAnchorPos(finalValue, duration);
        }
    }

    [Serializable]
    internal class DOTweenAnchorMinSetting : DOTweenAnimationSetting
    {
        public new bool isValid => base.isValid && source is RectTransform;

        protected override Tween Create()
        {
            Vector2 finalValue = endValueVector2;
            var sourceRectTrans = source as RectTransform;
            if (isFrom)
            {
                finalValue = sourceRectTrans.anchorMin;
                sourceRectTrans.anchorMin = endValueVector2;
            }

            return sourceRectTrans.DOAnchorMin(finalValue, duration);
        }
    }

    [Serializable]
    internal class DOTweenAnchorMaxSetting : DOTweenAnimationSetting
    {
        public new bool isValid => base.isValid && source is RectTransform;

        protected override Tween Create()
        {
            Vector2 finalValue = endValueVector2;
            var sourceRectTrans = source as RectTransform;
            if (isFrom)
            {
                finalValue = sourceRectTrans.anchorMax;
                sourceRectTrans.anchorMax = endValueVector2;
            }

            return sourceRectTrans.DOAnchorMax(finalValue, duration);
        }
    }

    [Serializable]
    internal class DOTweenPivotSetting : DOTweenAnimationSetting
    {
        public new bool isValid => base.isValid && source is RectTransform;

        protected override Tween Create()
        {
            Vector2 finalValue = endValueVector2;
            var sourceRectTrans = source as RectTransform;
            if (isFrom)
            {
                finalValue = sourceRectTrans.pivot;
                sourceRectTrans.pivot = endValueVector2;
            }

            return sourceRectTrans.DOPivot(finalValue, duration);
        }
    }

    [Serializable]
    internal class DOTweenRotateSetting : DOTweenAnimationSetting
    {
        protected override Tween Create()
        {
            Vector3 finalValue = endValueVector3;
            if (isFrom)
            {
                finalValue = source.eulerAngles;
                source.eulerAngles = endValueVector3;
            }

            return source.DORotate(finalValue, duration, rotateMode);
        }
    }

    [Serializable]
    internal class DOTweenLocalRotateSetting : DOTweenAnimationSetting
    {
        protected override Tween Create()
        {
            Vector3 finalValue = endValueVector3;
            if (isFrom)
            {
                finalValue = source.localEulerAngles;
                source.eulerAngles = endValueVector3;
            }

            return source.DOLocalRotate(finalValue, duration, rotateMode);
        }
    }

    [Serializable]
    internal class DOTweenTMPTextIncrease : DOTweenAnimationSetting
    {
        TMP_Text text;

        public new bool isValid => base.isValid && text != null && int.TryParse(text.text, out _);

        public override void Init(Transform source)
        {
            base.Init(source);

            text = source.GetComponent<TMP_Text>();
        }

        protected override Tween Create()
        {
            int finalValue = endValueInt;

            if (int.TryParse(text.text, out var startValue))
            {
                if (isFrom)
                {
                    finalValue = startValue;
                    text.text = endValueInt.ToString("N0");
                }

                return text.DOIncrease(finalValue, duration);
            }

            return null;
        }
    }


    public static class DOTweenExtensionMethods
    {
        public static TweenerCore<int, int, NoOptions> DOIncrease(this TMP_Text text, int endValue, float duration)
        {
            if (int.TryParse(text.text, NumberStyles.Number, CultureInfo.CurrentCulture, out var startValue))
            {
                var tween = DOTween.To(() => startValue, x => text.text = x.ToString("N0"), endValue, duration).SetEase(Ease.Linear);
                return tween;
            }

            return null;
        }

        public static TweenerCore<float,float, NoOptions> DOIncrease(this TMP_Text text, float endValue, float duration, string format = "F2")
        {
            if (float.TryParse(text.text, NumberStyles.Number, CultureInfo.CurrentCulture, out var startValue))
            {
                var tween = DOTween.To(() => startValue, x => text.text = x.ToString(format), endValue, duration).SetEase(Ease.Linear);
            }

            return null;
        }

        public static TweenerCore<Vector3, Vector3, VectorOptions> DOScaleX(this RectTransform rectTransform, float endValue, Vector2 pivot, float duration)
        {
            var originPivot = rectTransform.pivot;
            rectTransform.pivot = pivot;
            var tween = rectTransform.DOScaleX(endValue, duration);
            tween.onComplete += () => rectTransform.pivot = originPivot;
            return tween;
        }

        public static TweenerCore<Vector3, Vector3, VectorOptions> DOPopIn(this Transform transform, float duration, float targetScale = 1)
        {
            transform.gameObject.SetActive(true);
            transform.localScale = Vector3.zero;
            var tween = transform.DOScale(targetScale, duration).SetEase(Ease.OutBack);
            return tween;
        }

        public static TweenerCore<Vector3, Vector3, VectorOptions> DOPopIn(this Transform transform, float duration, Vector3 targetScale)
        {
            transform.gameObject.SetActive(true);
            transform.localScale = Vector3.zero;
            var tween = transform.DOScale(targetScale, duration).SetEase(Ease.OutBack);
            return tween;
        }

        public static TweenerCore<Vector3, Vector3, VectorOptions> DOPopOut(this Transform transform, float duration, bool deactiveWhenComplete = true)
        {
            var tween = transform.DOScale(0, duration).SetEase(Ease.InBack);
            tween.OnComplete(() =>
            {
                if (deactiveWhenComplete)
                    transform.gameObject.SetActive(false);
            });
            return tween;
        }

        public static Tween DOJellyPop(this Transform transform, float scale, float duration)
        {
            var seq = DOTween.Sequence();
            seq.Append(transform.DOScale(scale, duration / 2).SetEase(Ease.Linear));
            seq.Append(transform.DOScale(1, duration / 2).SetEase(Ease.Linear));
            return seq;
        }
    }
}

//----------------------CUSTOM DRAWING EDITOR, CAN BE MOVED TO OTHER FILE EDITOR SCRIPT------------------
#if UNITY_EDITOR 
namespace USimpFramework.Animation.DOTweenExtension.Editor
{
    using UnityEditor;

    [CustomEditor(typeof(DOTweenAnimationSettingController))]
    [CanEditMultipleObjects]
    public class DOTweenAnimationSettingControllerInspector : Editor
    {

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var target = this.target as DOTweenAnimationSettingController;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Play"))
            {
                if (!Application.isPlaying)
                {
                    Debug.LogError("DOTween free version does not support in edit mode, please enter play mode to play!");
                    return;
                }


                target.Play();
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    [CustomPropertyDrawer(typeof(DOTweenAnimationSetting))]
    internal class DotweenAnimationDrawer : PropertyDrawer
    {
        const float lineSpace = 5f;

        SerializedProperty isFromProperty;
        SerializedProperty isValueProperty;
        SerializedProperty animationTypeProperty;
        SerializedProperty isUniformScaleProperty;
        SerializedProperty isSpeedBasedProperty;
        SerializedProperty easeTypeProperty;
        SerializedProperty loopsProperty;

        float lineHeight => EditorGUIUtility.singleLineHeight + lineSpace;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lineCount = 2;
            if (property.isExpanded)
            {
                GetSerializedProperty(property);

                var animationType = (AnimationType)animationTypeProperty.enumValueIndex;
                var targetObject = property.serializedObject.targetObject as MonoBehaviour;
                if (animationType == AnimationType.Fade &&
                    !targetObject.TryGetComponent<SpriteRenderer>(out _) && !targetObject.TryGetComponent<Image>(out _)
                    && !targetObject.TryGetComponent<CanvasGroup>(out _) && !targetObject.TryGetComponent<TMP_Text>(out _)) //Check invalid property
                {
                    lineCount += 2;
                }
                else if (animationType == AnimationType.AnchorMax || animationType == AnimationType.AnchorMin || animationType == AnimationType.AnchorPos || animationType == AnimationType.Pivot)
                {
                    if (targetObject.transform is not RectTransform)
                        lineCount += 2;
                    else
                        lineCount += 6;
                }
                else if (animationType == AnimationType.TMP_TextIncrease && !targetObject.TryGetComponent<TMP_Text>(out _))
                {
                    lineCount++;
                }
                else
                {
                    lineCount += 6;

                    switch ((AnimationType)animationTypeProperty.enumValueIndex)
                    {
                        case AnimationType.Scale: lineCount++; break;
                        case AnimationType.LocalJump: lineCount += 2; break;
                        case AnimationType.Jump: lineCount += 2; break;
                        case AnimationType.Rotate: lineCount++; break;
                        case AnimationType.LocalRotate: lineCount++; break;
                    }
                }

                switch ((Ease)easeTypeProperty.enumValueIndex)
                {
                    case Ease.INTERNAL_Custom:
                        {
                            lineCount++;
                            break;
                        }
                }

                if (loopsProperty.intValue >= 2)
                {
                    lineCount++;
                }

                return lineHeight * lineCount + EditorGUIUtility.standardVerticalSpacing;

            }

            return EditorGUIUtility.standardVerticalSpacing + 20;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);

            GetSerializedProperty(property);

            // Using BeginProperty / EndProperty on the parent property means that  prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            property.isExpanded = EditorGUI.Foldout(new Rect(90, position.y, 10, 15), property.isExpanded, GUIContent.none);

            if (property.isExpanded)
            {
                var rect = new Rect(85, position.y + lineHeight, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("animationType"), new GUIContent("Animation"));

                rect.y += lineHeight;

                //Check valid property
                var targetObject = property.serializedObject.targetObject as MonoBehaviour;
                var animationType = (AnimationType)animationTypeProperty.enumValueIndex;
                if (animationType == AnimationType.Fade)
                {
                    if (!targetObject.TryGetComponent<SpriteRenderer>(out _) && !targetObject.TryGetComponent<Image>(out _)
                        && !targetObject.TryGetComponent<CanvasGroup>(out _) && !targetObject.TryGetComponent<TMP_Text>(out _))
                    {
                        EditorGUI.HelpBox(rect, "Target object dont have any renderer component attach!", MessageType.Error);
                        return;
                    }
                }

                if (animationType == AnimationType.AnchorMax || animationType == AnimationType.AnchorMin || animationType == AnimationType.AnchorPos
                    || animationType == AnimationType.Pivot)
                {
                    if (targetObject.transform is not RectTransform)
                    {
                        EditorGUI.HelpBox(rect, "Target object must have the rect transform", MessageType.Error);
                        return;
                    }
                }

                if (animationType == AnimationType.TMP_TextIncrease && !targetObject.TryGetComponent<TMP_Text>(out _))
                {
                    EditorGUI.HelpBox(rect, "Target object must have TMP_Text component!", MessageType.Error);
                    return;
                }

                EditorGUI.PropertyField(rect, easeTypeProperty, new GUIContent("Ease"));
                switch ((Ease)easeTypeProperty.enumValueIndex)
                {
                    case Ease.INTERNAL_Custom:
                        {
                            rect.y += lineHeight;
                            EditorGUI.PropertyField(new Rect(rect.x + 20, rect.y, rect.width, rect.height), property.FindPropertyRelative("easeCurve"));
                            break;
                        }
                }

                rect.y += lineHeight;
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("loops"));

                if (loopsProperty.intValue >= 2)
                {
                    rect.y += lineHeight;
                    EditorGUI.PropertyField(new Rect(rect.x + 10, rect.y, rect.width, rect.height), property.FindPropertyRelative("loopType"));
                }

                rect.y += lineHeight;
                string durationLabel = isSpeedBasedProperty.boolValue ? "Speed" : "Duration";
                if (GUI.Button(new Rect(rect.x, rect.y, EditorGUIUtility.fieldWidth + 10, EditorGUIUtility.singleLineHeight), durationLabel))
                {
                    isSpeedBasedProperty.boolValue = !isSpeedBasedProperty.boolValue;
                }
                EditorGUI.PropertyField(new Rect(rect.x + 70, rect.y, rect.width, rect.height), property.FindPropertyRelative("duration"), GUIContent.none);

                rect.y += lineHeight;
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("delay"));

                rect.y += lineHeight;
                string fromToButtonLabel = isFromProperty.boolValue ? "From" : "To";
                if (GUI.Button(rect, fromToButtonLabel))
                {
                    isFromProperty.boolValue = !isFromProperty.boolValue;
                }

                rect.y += lineHeight;
                switch (animationType)
                {
                    case AnimationType.Move:
                        {
                            string isValueLabel = isValueProperty.boolValue ? "Value" : "Target";
                            if (GUI.Button(new Rect(rect.x, rect.y, EditorGUIUtility.fieldWidth, EditorGUIUtility.singleLineHeight), isValueLabel))
                            {
                                isValueProperty.boolValue = !isValueProperty.boolValue;
                            }

                            if (isValueProperty.boolValue)
                            {
                                EditorGUI.PropertyField(new Rect(rect.x + 70, rect.y, rect.width, rect.height), property.FindPropertyRelative("endValueVector3"), GUIContent.none);
                            }
                            else
                            {
                                EditorGUI.PropertyField(new Rect(rect.x + 70, rect.y, rect.width, rect.height), property.FindPropertyRelative("target"), GUIContent.none);
                            }
                            break;
                        }
                    case AnimationType.LocalMove:
                        {
                            EditorGUI.PropertyField(rect, property.FindPropertyRelative("endValueVector3"), GUIContent.none);
                            break;
                        }
                    case AnimationType.Fade:
                        {
                            EditorGUI.PropertyField(rect, property.FindPropertyRelative("endValue"), GUIContent.none);
                            break;
                        }
                    case AnimationType.TMP_TextIncrease:
                        {
                            EditorGUI.PropertyField(rect, property.FindPropertyRelative("endValueInt"), GUIContent.none);
                            break;
                        }
                    case AnimationType.Scale:
                        {
                            if (isUniformScaleProperty.boolValue)
                            {
                                EditorGUI.PropertyField(rect, property.FindPropertyRelative("endValue"), GUIContent.none);
                            }
                            else
                            {
                                EditorGUI.PropertyField(rect, property.FindPropertyRelative("endValueVector3"), GUIContent.none);
                            }

                            rect.y += lineHeight;

                            isUniformScaleProperty.boolValue = EditorGUI.Toggle(new Rect(rect.x, rect.y, 50, EditorGUIUtility.singleLineHeight), new GUIContent("Uniform"), isUniformScaleProperty.boolValue);
                            break;
                        }
                    case AnimationType.Jump:
                        {
                            string isValueLabel = isValueProperty.boolValue ? "Value" : "Target";
                            if (GUI.Button(new Rect(rect.x, rect.y, EditorGUIUtility.fieldWidth, EditorGUIUtility.singleLineHeight), isValueLabel))
                            {
                                isValueProperty.boolValue = !isValueProperty.boolValue;
                            }

                            if (isValueProperty.boolValue)
                            {
                                EditorGUI.PropertyField(new Rect(rect.x + 70, rect.y, rect.width, rect.height), property.FindPropertyRelative("endValueVector3"), GUIContent.none);
                            }
                            else
                            {
                                EditorGUI.PropertyField(new Rect(rect.x + 70, rect.y, rect.width, rect.height), property.FindPropertyRelative("target"), GUIContent.none);
                            }

                            rect.y += lineHeight;
                            EditorGUI.PropertyField(rect, property.FindPropertyRelative("jumpPower"));

                            rect.y += lineHeight;
                            EditorGUI.PropertyField(rect, property.FindPropertyRelative("numJumps"));
                            break;
                        }
                    case AnimationType.LocalJump:
                        {
                            EditorGUI.PropertyField(rect, property.FindPropertyRelative("endValueVector3"), GUIContent.none);

                            rect.y += lineHeight;
                            EditorGUI.PropertyField(rect, property.FindPropertyRelative("jumpPower"));

                            rect.y += lineHeight;
                            EditorGUI.PropertyField(rect, property.FindPropertyRelative("numJumps"));
                            break;

                        }
                    case AnimationType.AnchorPos:
                        {
                            EditorGUI.PropertyField(rect, property.FindPropertyRelative("endValueVector2"), GUIContent.none);
                            break;
                        }
                    case AnimationType.AnchorMin:
                        {
                            EditorGUI.PropertyField(rect, property.FindPropertyRelative("endValueVector2"), GUIContent.none);
                            break;
                        }
                    case AnimationType.AnchorMax:
                        {
                            EditorGUI.PropertyField(rect, property.FindPropertyRelative("endValueVector2"), GUIContent.none);
                            break;
                        }
                    case AnimationType.Pivot:
                        {
                            EditorGUI.PropertyField(rect, property.FindPropertyRelative("endValueVector2"), GUIContent.none);
                            break;
                        }
                    case AnimationType.Rotate:
                        {
                            EditorGUI.PropertyField(rect, property.FindPropertyRelative("endValueVector3"), GUIContent.none);

                            rect.y += lineHeight;

                            EditorGUI.PropertyField(rect, property.FindPropertyRelative("rotateMode"));

                            break;
                        }
                    case AnimationType.LocalRotate:
                        {
                            EditorGUI.PropertyField(rect, property.FindPropertyRelative("endValueVector3"), GUIContent.none);

                            rect.y += lineHeight;

                            EditorGUI.PropertyField(rect, property.FindPropertyRelative("rotateMode"));

                            break;
                        }
                }

            }

            EditorGUI.EndProperty();
        }

        void GetSerializedProperty(SerializedProperty property)
        {
            isFromProperty = property.FindPropertyRelative("isFrom");

            isValueProperty = property.FindPropertyRelative("isValue");

            animationTypeProperty = property.FindPropertyRelative("animationType");

            isUniformScaleProperty = property.FindPropertyRelative("isUniformScale");

            isSpeedBasedProperty = property.FindPropertyRelative("isSpeedBased");

            easeTypeProperty = property.FindPropertyRelative("easeType");

            loopsProperty = property.FindPropertyRelative("loops");
        }
    }
}
#endif