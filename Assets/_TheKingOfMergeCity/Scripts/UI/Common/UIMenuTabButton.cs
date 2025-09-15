using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using USimpFramework.UI.Extensions;

namespace TheKingOfMergeCity
{
    [RequireComponent(typeof(Button))]
    public class UIMenuTabButton : UITabButton
    {
        [Header("Setting")]
        [SerializeField] Color selectedColor;
        [SerializeField] Color unSelectedColor;
        [SerializeField] float selectedHeight;
        [SerializeField] float unSelectedHeight;
        [SerializeField] int contentPaddingTop = -71;
        [SerializeField] VerticalLayoutGroup contentLayoutGroup;
        [SerializeField] bool _isLock;
        public bool isLock => _isLock;
        
        [Header("References")]
        [SerializeField] TMP_Text contentText;
        [SerializeField] LayoutElement layoutElement;
        [SerializeField] Image backgroundImage;
        [SerializeField] Image iconImage;
        [SerializeField] GameObject goLock;
        
        public void SetWidthLayout(float width, bool withTransition = false)
        {
            var rectTransform = transform as RectTransform;
            var sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x = width;

            if (withTransition)
            {
                rectTransform.DOSizeDelta(sizeDelta, 0.1f).SetEase(Ease.OutQuad);
            }
            else
            {
                rectTransform.sizeDelta = sizeDelta;
            }
        }

        void Start()
        {
            goLock.SetActive(isLock);
        }
        
        protected override void OnSelected()
        {
            button.interactable = !isSelected; 
            
            KillTweens();
            
            if (isSelected)
            {
                button.interactable = false;
                contentText.gameObject.SetActive(true);
                backgroundImage.color = selectedColor;
                var sizeDelta = iconImage.rectTransform.sizeDelta;
                sizeDelta.y = selectedHeight;
                iconImage.rectTransform.DOSizeDelta(sizeDelta, 0.25f).SetEase(Ease.OutQuad);
                contentLayoutGroup.padding.top = contentPaddingTop;
            }
            else
            {
                button.interactable = true;
                contentText.gameObject.SetActive(false);
                backgroundImage.color = unSelectedColor;
                if (hasChanged)
                {
                    var sizeDelta = iconImage.rectTransform.sizeDelta;
                    sizeDelta.y = unSelectedHeight;
                    iconImage.rectTransform.DOSizeDelta(sizeDelta, 0.25f).SetEase(Ease.OutQuad);
                    contentLayoutGroup.padding.top = 0;
                }
            }
        }

        ///<summary>Called before selecting tab </summary>
        public override void ResetData()
        {
            base.ResetData();

            var sizeDelta = iconImage.rectTransform.sizeDelta;
            sizeDelta.y = unSelectedHeight;
            iconImage.rectTransform.sizeDelta = sizeDelta;

        }

        void KillTweens()
        {
            iconImage.rectTransform.DOKill();
            layoutElement.DOKill();
        }

        void OnDestroy()
        {
            KillTweens();
        }
    }
}
