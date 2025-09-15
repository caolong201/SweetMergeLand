using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;

using USimpFramework.UI;
using USimpFramework.UIGachaEffect;
using TMPro;
using DG.Tweening;

namespace TheKingOfMergeCity
{
    using Enum;
    using Config;

    public class UIDecoBuildPopup : UIScalePopup
    {
        [Header("References")]
        [SerializeField] Slider areaProgressSlider;
        [SerializeField] TMP_Text areaProgressText;
        [SerializeField] TMP_Text buildDescriptionText;
        [SerializeField] TMP_Text costText;
        [SerializeField] TMP_Text rewardText;

        [SerializeField] UIRewardItem uiRewardItemPrefab;

        [SerializeField] Image starCoinImage;
        [SerializeField] Button closeButton;
        
        [SerializeField] Button _startButton;
        public Button startButton => _startButton;

        [SerializeField] Button playButton;

        [SerializeField] Image tickImage;
        [SerializeField] Image decoIconImage;

        [Header("States")]
        [SerializeField] GameObject goBuilding;
        [SerializeField] GameObject goCompleteAll;
        [SerializeField] Color canBuildColor;
        [SerializeField] Color normalColor;


        List<UIRewardItem> uiRewardItems = new();

        int buildingCost;
        int areaId = -1;
        string buildDecoId;

        void Start()
        {
            uiRewardItemPrefab.gameObject.SetActive(false);
        }

        public void UpdateState(int areaId, string buildDecoId)
        {
            goBuilding.SetActive(false);
            goCompleteAll.SetActive(false);
            tickImage.gameObject.SetActive(false);

            this.areaId = areaId;

            var configArea = ConfigManager.Instance.configArea.areaItems.Find(c => c.id == areaId);
            if (configArea == null)
                throw new UnityException("Update state failed! Invalid config area: " + areaId);

            var areaData = UserManager.Instance.areaDatas.Find(a => a.areaId == areaId);
            if (areaData == null)
                throw new UnityException("Update state failed! Invalid area id: " + areaId);

            if (areaData.completedDecoIds.Count == configArea.decoItems.Count) //Is max
            {
                goCompleteAll.SetActive(true);
            }
            else
            {
                goBuilding.SetActive(true);

                this.buildDecoId = buildDecoId;

                var configDeco = configArea.decoItems.Find(c => c.id == buildDecoId);
                if (configDeco == null)
                    throw new UnityException("Update state failed! Invalid config deco: " + buildDecoId);

                decoIconImage.sprite = configDeco.iconSprite;
                buildingCost = configDeco.buildingCost;
                int starCoin = UserManager.Instance.GetCurrencyBalance(CurrencyType.Star);
                costText.text = $"{starCoin}/{buildingCost}";
                buildDescriptionText.text = configDeco.description;

                
                //Update rewards
                rewardText.text = ConfigManager.Instance.configGlobal.energyRewardAfterBuild.ToString();

                var configRewards = new List<ConfigRewardItem>();

                configRewards.Add(new ConfigRewardItem(RewardType.Currency, CurrencyType.Energy, ConfigManager.Instance.configGlobal.energyRewardAfterBuild));
                configRewards.Add(new ConfigRewardItem(RewardType.Currency, CurrencyType.Exp, configDeco.expReward));

                while (uiRewardItems.Count < configRewards.Count)
                {
                    var ui = Instantiate(uiRewardItemPrefab, uiRewardItemPrefab.transform.parent);
                    uiRewardItems.Add(ui);
                }

                uiRewardItems.ForEach(ui => ui.gameObject.SetActive(false));

                for (int i =0; i < configRewards.Count; i++)
                {
                    uiRewardItems[i].Setup(configRewards[i]);
                }

                if (starCoin >= buildingCost)
                {
                    startButton.gameObject.SetActive(true);
                    tickImage.gameObject.SetActive(true);
                    playButton.gameObject.SetActive(false);
                }
                else
                {
                    startButton.gameObject.SetActive(false);
                    tickImage.gameObject.SetActive(false);
                    playButton.gameObject.SetActive(true);
                }

                costText.color = starCoin > buildingCost ? canBuildColor : normalColor;

            }
            areaProgressSlider.value = (float)areaData.completedDecoIds.Count / configArea.decoItems.Count;
            areaProgressText.text = $"{areaData.completedDecoIds.Count}/{configArea.decoItems.Count}";
        }


        public override void Show(bool withTransition = true, System.Action onCompleted = null)
        {
            base.Show(withTransition, onCompleted);
            
            closeButton.interactable = true;
            startButton.interactable = true;
            playButton.interactable = true;
        }


        public void PressStart(Button button)
        {
            var userManager = UserManager.Instance;
            var uiManager = UIManager.Instance;
            userManager.BuildDecoration(areaId,buildDecoId, out bool isSuccess);

            if (!isSuccess)
            {
                Debug.LogError("Something wrong! Cannot build decoration!");
                uiManager.ShowFloatingText("Something wrong! Cannot build decoration!");
                return;
            }

            //Set select area id
            userManager.SetCurrentSelectAreaId(areaId);

            closeButton.interactable = false;
            button.interactable = false;

            UIManager.Instance.SetInteraction(false);

            //Base on the current scene, play building construction animation effect
            bool isHomeScene = HomeManager.Instance != null;
            
            if (isHomeScene) //Home scene
            {
                var uiHomeView = uiManager.currentView as UIHomeView;
                var uiTopBar = uiHomeView.uiTopBar;
                uiTopBar.IncreaseCurrency(CurrencyType.Star, 1f);
                uiHomeView.CheckNoti();
            }
            else
            {
                //Play building construction animation effect
                userManager.isPlayingDecoBuildingFromInGameScene = true;

                var uiInGameView = uiManager.currentView as UIInGameView;
                uiInGameView.CheckHideSelector(transform);

                var uiTopBar = (uiManager.currentView as UIInGameView).uiTopBar;
                uiTopBar.IncreaseCurrency(CurrencyType.Star, 1f);
            }
            
            //Decrease cost text
            int balance = userManager.GetCurrencyBalance(CurrencyType.Star);
            DOTween.To(() => balance, x => costText.text = $"{x}/{buildingCost}", balance, 1f).SetEase(Ease.Linear);

            UIGachaEffect.Instance.PlayParabolaEffect("star_coin", starCoinImage.transform.position, startButton.transform.position, buildingCost, () =>
            {
                var startButtonTrans = startButton.transform;
                startButtonTrans.DOKill();
                startButtonTrans.DOScale(1.2f, 0.2f).SetEase(Ease.Linear).SetLoops(2, LoopType.Yoyo).From(1);

            }, () =>
            {
                DOVirtual.DelayedCall(0.2f, () =>
                {
                    PressClose();
                    if (isHomeScene)
                    {
                        HomeManager.Instance.PlayBuildingDecoFlow(buildDecoId, 0.6f);
                    }
                    else
                    {
                        ApplovinManager.Instance.HideBannerAd();
                        BootManager.Instance.LoadScene(SceneConstants.HOME_SCENE_NAME,true);
                    }    
                });
            });


            //Check tutorial
            TutorialManager.Instance.CheckCompleteStep<ConfigTutorialClickStartInBuildPopup>();

            //Analytics
            var currentConfigArea = ConfigManager.Instance.configArea.areaItems.Find(c => c.id == areaId);
            GameAnalyticsManager.Instance.SendDecoBuildCompleteEvent(areaId,currentConfigArea.decoItems.FindIndex(c => c.id == buildDecoId));
        }

        public void PressPlay(Button button)
        {
            closeButton.interactable = false;
            button.interactable = false;

            UIManager.Instance.HidePopup(this);
            BootManager.Instance.LoadScene(SceneConstants.IN_GAME_SCENE_NAME,true);
        }

        public void PressClose()
        {
            UIManager.Instance.HidePopup(this);
        }
    }
}
