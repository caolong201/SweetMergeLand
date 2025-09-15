using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using USimpFramework.Utility;
using DG.Tweening;

namespace TheKingOfMergeCity
{
    using Enum;

    public class UIDecoCost : MonoBehaviour
    {
        [SerializeField] TMP_Text costText;
        [SerializeField] Transform notiTrans;
        [SerializeField] Image shineImage;
        [SerializeField] Image backgroundImage;
        [SerializeField] Image chopImage;
        [SerializeField] Outline backgroundOutline;
        [SerializeField] Shadow chopShadow;

        [Header("Settings")]
        [SerializeField] Color unlockColor;
        [SerializeField] Color readyToBuildColor;
        [SerializeField] Color outlineUnlockColor;
        [SerializeField] Color outlineReadyToBuildColor;


        int costAmount;

        void Start()
        {
            notiTrans.DOScale(1.3f, 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        }

        void OnDestroy()
        {
            notiTrans.DOKill();
        }

        void Update()
        {
            shineImage.transform.Rotate(Vector3.forward, -15 * Time.deltaTime);
        }

        private void OnEnable()
        {
            OnStarCostChanged(UserManager.Instance.GetCurrencyBalance(CurrencyType.Star));
        }

        void OnStarCostChanged(int currentProgress)
        {
            costText.text = $"{currentProgress:N0}/{costAmount}";
            notiTrans.gameObject.SetActive(currentProgress >= costAmount);
            if (currentProgress >= costAmount)
            {
                shineImage.DOFade(1, 0.5f).SetEase(Ease.Linear).From(0);
                backgroundImage.color = readyToBuildColor;
                chopImage.color = readyToBuildColor;
                backgroundOutline.effectColor = outlineReadyToBuildColor;
                chopShadow.effectColor = outlineReadyToBuildColor;
            }
            else
            {
                shineImage.DOKill();
                shineImage.SetAlpha(0);
                backgroundImage.color = unlockColor;
                chopImage.color = unlockColor;
                backgroundOutline.effectColor = outlineUnlockColor;
                chopShadow.effectColor = outlineUnlockColor;
                
            }
        }

        public void Setup( int costAmount)
        {
            this.costAmount = costAmount;
            shineImage.SetAlpha(0);
            transform.localScale = Vector3.one;
        }
    }
}
