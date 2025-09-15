using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace USimpFramework.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UIFadeFloatingText : UIFloatingTextController
    {
        [SerializeField] float showUpDuration = 0.8f;
        [SerializeField] float upOffset = 120f;

        CanvasGroup canvasGroup;

        public override void Show(string content, Action onCompleted)
        {
            base.Show(content, onCompleted);

            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            var end = transform.position.y + upOffset;
            transform.DOMoveY(end, showUpDuration).SetEase(Ease.Linear).OnComplete(() => onCompleted?.Invoke());

            canvasGroup.alpha = 0;
           
            canvasGroup.DOFade(1, showUpDuration /2).SetEase(Ease.Linear).OnComplete(() =>  canvasGroup.DOFade(0,showUpDuration/2).SetEase(Ease.Linear));
            
        }
    }
}
