using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using USimpFramework.UI;

namespace TheKingOfMergeCity
{
    public class UIScalePopup : UIPopupBase
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected RectTransform boardTrans;
        [SerializeField] protected float duration = 0.5f;

        public override void Show(bool withTransition = true, System.Action onCompleted = null)
        {
            gameObject.SetActive(true);
            boardTrans.DOScale(1, duration).SetEase(Ease.OutBack).From(0.7f);
            canvasGroup.DOFade(1, duration).SetEase(Ease.Linear).From(0f).OnComplete(() => onCompleted?.Invoke());
            canvasGroup.blocksRaycasts = true;
        }

        public override void Hide(bool withTransition = true, System.Action onCompleted =null)
        {
            boardTrans.DOScale(0.7f, duration).SetEase(Ease.InBack).From(1);
            canvasGroup.DOFade(0, duration).SetEase(Ease.Linear).From(1f).OnComplete(() =>
            {
                gameObject.SetActive(false);
                onCompleted?.Invoke();
            });
             
            canvasGroup.blocksRaycasts = false;
        }

       
    }
}
