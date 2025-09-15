using USimpFramework.UI;
using USimpFramework.UI.Extensions;
using UnityEngine;
using DG.Tweening;

using UnityEngine.EventSystems;

namespace TheKingOfMergeCity
{
    public class UIMenuTabView : UITabView<UIMenuTabButton>
    {
        [Tooltip("Horizontal drag threshold in pixel ")]
        [SerializeField] float dragThreshold = 3f;

        bool isSwitchingTab;
        
        public override void SelectTab(UIMenuTabButton button)
        {
            if (button.isLock)
            {
                UIManager.Instance.ShowFloatingText("Coming soon!");
                return;
            }

            //Need to check for the first open the tab, select the starting tab with no transition
            if (selectedButton != null && selectedButton != button)
            {
                isSwitchingTab = true;

                //Move position of selected tab
                var prevContent = selectedButton.goRelatedContent;
                int prevIndex = tabButtons.IndexOf(selectedButton);
                var prevContentRectTransform = prevContent.transform as RectTransform;
                prevContentRectTransform.DOKill();

                var curContent = button.goRelatedContent;
                var curIndex = tabButtons.IndexOf(button);

                curContent.SetActive(true);
                var curContentRectTransform = curContent.transform as RectTransform;
                curContentRectTransform.DOKill();
                var anchorPos = curContentRectTransform.anchoredPosition;
                anchorPos.x = curIndex > prevIndex ? UIManager.Instance.maxRectPoint.x * 2 : UIManager.Instance.minRectPoint.x * 2;
                curContentRectTransform.DOAnchorPos(Vector2.zero, 0.2f).SetEase(Ease.OutQuad).From(anchorPos);
                anchorPos.x = curIndex > prevIndex ? UIManager.Instance.minRectPoint.x * 2 : UIManager.Instance.maxRectPoint.x * 2;

                prevContentRectTransform.DOAnchorPos(anchorPos, 0.2f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    prevContent.SetActive(false);
                    isSwitchingTab = false;
                });
            }
           
            base.SelectTab(button);
        }

        Vector2 firstDragScreenPos;
        Vector2 secondDragScreenPos;
        float dragXOffset;

        public void OnBeginDragTabContent()
        {
            firstDragScreenPos = Input.mousePosition;
        }

        public void OnDragTabContent()
        {
            if (isSwitchingTab)
                return;

            secondDragScreenPos = Input.mousePosition;
            dragXOffset = secondDragScreenPos.x - firstDragScreenPos.x;

            if (Mathf.Abs(dragXOffset) >= dragThreshold)
            {
                bool leftToRight = dragXOffset > 0;
                //Select tab
                var index = tabButtons.IndexOf(selectedButton) + (leftToRight ? -1 : 1);

                if (index > 0 && index < tabButtons.Count - 1) //In Range
                {
                    var nextButton = tabButtons[index];
                    if (!nextButton.isLock)
                    {
                        SelectTab(nextButton);
                    }
                }
            }
        }

        public void OnMouseUpTabContent()
        {
            dragXOffset = 0;
        }
    }
}
