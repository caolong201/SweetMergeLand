using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using USimpFramework.UI;
using TMPro;
using System.Linq;
using USimpFramework.UIGachaEffect;
using DG.Tweening;


namespace TheKingOfMergeCity
{
    using Config;
    using Enum;

    public class UIAreaDecorationPoup : UIScalePopup
    {
        public struct DecoData
        {
            public bool isCompleted;
            public ConfigDecoItem config;
        }

        [SerializeField] TMP_Text areaNameText;
        [SerializeField] TMP_Text areaProgressText;
        [SerializeField] Slider areaProgressSlider;
        [SerializeField] UIDecorationInfo uiDecorationInfoPrefab;
        [SerializeField] Button closeButton;

        List<UIDecorationInfo> uiDecorationInfos = new();

        void Awake()
        {
            uiDecorationInfoPrefab.gameObject.SetActive(false);
        }

        void UpdateDecos()
        {
            var unlockDecos = HomeManager.Instance.readonlyDecoItems.Where(deco => deco.decoItemState == DecoItemState.Unlock).ToList();
            while (uiDecorationInfos.Count < unlockDecos.Count)
            {
                var ui = Instantiate(uiDecorationInfoPrefab, uiDecorationInfoPrefab.transform.parent);
                uiDecorationInfos.Add(ui);
            }

            uiDecorationInfos.ForEach(ui => ui.gameObject.SetActive(false));

            for (int i = 0; i < unlockDecos.Count; i++)
            {
                uiDecorationInfos[i].Setup(unlockDecos[i].config);
            }
        }

        void UpdateAreaProgress()
        {
            var currentUserArea = UserManager.Instance.currentSelectAreaData;
            var currentConfigArea = HomeManager.Instance.currentConfigArea;

            areaProgressSlider.value = (float)currentUserArea.completedDecoIds.Count / currentConfigArea.decoItems.Count;
            areaProgressText.text = $"{currentUserArea.completedDecoIds.Count} / {currentConfigArea.decoItems.Count}";
            areaNameText.text = currentConfigArea.displayName;
        }

        private void OnEnable()
        {

            var uiTopBar = (UIManager.Instance.currentView as UIHomeView).uiTopBar;
            uiTopBar.GetComponent<Canvas>().overrideSorting = true;
            closeButton.interactable = true;

            UpdateDecos();
            UpdateAreaProgress();

        }

        private void OnDisable()
        {
            var uiTopBar = (UIManager.Instance.currentView as UIHomeView).uiTopBar;
            uiTopBar.GetComponent<Canvas>().overrideSorting = false;
        }

        public void PressClose()
        {
            UIManager.Instance.HidePopup(this);
        }


        public void PressDoItButton(UIDecorationInfo ui)
        {
            //Check consume
            var currentSelectAreaId = UserManager.Instance.currentSelectAreaId;
            UserManager.Instance.BuildDecoration(currentSelectAreaId, ui.config.id, out var isSuccess);
            if (!isSuccess)
            {
                Debug.LogError("Insusficient amount!");
                return;
            }

            //Play parabola effect
            closeButton.interactable = false;
            ui.doItButton.transition = Selectable.Transition.None;
            ui.doItButton.interactable = false;

            var uiHomeView = UIManager.Instance.currentView as UIHomeView;

            var uiTopBar = uiHomeView.uiTopBar;

            uiHomeView.ShowGroupInteractableButton(false);
            uiHomeView.CheckNoti();
            
            //Decrease currency amount here
            uiTopBar.IncreaseCurrency(CurrencyType.Star, 3f);

            //Play parabola effect
            UIGachaEffect.Instance.PlayParabolaEffect("star_coin", uiTopBar.starCoinImage.transform.position, ui.doItButton.transform.position, ui.config.buildingCost, () =>
            {
                var doItButtonTrans = ui.doItButton.transform;
                doItButtonTrans.DOKill();
                doItButtonTrans.DOScale(1.2f, 0.2f).SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo).From(1);
            }, () =>
            {
                DOVirtual.DelayedCall(0.1f, () =>
                {
                    UIManager.Instance.HidePopup(this);
                    DOVirtual.DelayedCall(0.8f, () =>
                    {
                        HomeManager.Instance.BuildDecoration(ui.config.id);
                    });
                });
            });
            
            //Analytics
            GameAnalyticsManager.Instance.SendDecoBuildCompleteEvent(currentSelectAreaId, HomeManager.Instance.currentConfigArea.decoItems.FindIndex(c => c.id == ui.config.id));
        }
    }
}
