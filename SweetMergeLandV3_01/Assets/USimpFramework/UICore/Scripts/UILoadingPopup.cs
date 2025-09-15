using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace USimpFramework.UI
{
    public class UILoadingPopup : UIPopupBase
    {
        [SerializeField] Image upImage;
        [SerializeField] Image downImage;
        [SerializeField] RectTransform girlContentTrans;

        void Start()
        { 

            upImage.rectTransform.anchoredPosition = new Vector2(UIManager.Instance.minRectPoint.x, upImage.rectTransform.anchoredPosition.y);
            downImage.rectTransform.anchoredPosition = new Vector2(UIManager.Instance.maxRectPoint.x, downImage.rectTransform.anchoredPosition.y);
        }

        public bool isShowCompleted { get; private set; }


        public override void Show(bool withTransition = true, System.Action onCompleted = null)
        {
            isShowCompleted = false;

            if (withTransition)
            {
                gameObject.SetActive(true);

                float duration = 0.4f;
                upImage.DOFillAmount(1, duration).SetEase(Ease.Linear).From(0);
                downImage.DOFillAmount(1, duration).SetEase(Ease.Linear).From(0);

                girlContentTrans.DOScale(1, duration).SetEase(Ease.OutBack).SetDelay(0.3f).From(Vector3.zero);

                DOVirtual.DelayedCall(duration + 0.3f + 0.3f, () =>
                {
                    isShowCompleted = true;
                    onCompleted?.Invoke();
                });

            }
            else
            {
                isShowCompleted = true;
                gameObject.SetActive(true);
                onCompleted?.Invoke();
            }
        }

        public override void Hide(bool withTransition = true, System.Action onCompleted = null)
        {
            if (withTransition)
            {
                StartCoroutine(CR_Hide());
            }
            else
            {
                gameObject.SetActive(false);
                onCompleted?.Invoke();
            }


            IEnumerator CR_Hide()
            {
                yield return new WaitUntil(() => isShowCompleted);

                float duration = 0.4f;

                upImage.DOFillAmount(0, duration).SetEase(Ease.Linear).SetDelay(0.3f);
                var tween = downImage.DOFillAmount(0, duration).SetEase(Ease.Linear).SetDelay(0.3f);
                girlContentTrans.DOScale(0, duration).SetEase(Ease.InBack);

                yield return tween.WaitForCompletion();
                gameObject.SetActive(false);
                onCompleted?.Invoke();
            }
        }

    }
}
