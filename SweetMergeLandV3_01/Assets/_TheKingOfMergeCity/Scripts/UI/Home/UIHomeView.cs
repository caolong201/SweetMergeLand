using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using USimpFramework.UI;
using USimpFramework.Utility;
using TMPro;

using USimpFramework.Animation.DOTweenExtension;
namespace TheKingOfMergeCity
{
    using Enum;
    
    public class UIHomeView : UIViewBase
    {
        [Header("Home")]
        [SerializeField] UITopBar _uiTopBar;
        [SerializeField] UIAreaCompleted uiAreaCompleted;
        [SerializeField] RectTransform groupBottomTrans;
        [SerializeField] RectTransform groupBottomMenuTrans;
        [SerializeField] RectTransform groupTopTrans;
        [SerializeField] RectTransform groupButtonLeftTrans;

        [SerializeField] UIMenuTabView uiMenuTabView;
        [SerializeField] TMP_Text areaNameText;
        [SerializeField] TMP_Text areaProgressText;
        [SerializeField] Slider areaProgressSlider;
        [SerializeField] Transform notiTrans;
        [SerializeField] UIStoryCharacterSpeechBubble uiNextDecoInfo;

        [SerializeField] Button dailyRewardButton;
        [SerializeField] Transform dailyRewardNotiTrans;

        [SerializeField] Button rouletteButton;

        [SerializeField] Button _playButton;
        public Button playButton => _playButton;

        [Header("Area selection")]
        [SerializeField] UIAreaSelectPopup uiAreaSelectPopup;
        
        public UITopBar uiTopBar => _uiTopBar;

        Vector2 originalGroupBottomPos;
        Vector2 originalGroupBottomMenuPos;
        Vector2 originalGroupTopPos;
        Vector2 originalGroupButtonLeftPos;

        float selectedWidth;
        float unSelectedWidth;
        
        void Start()
        {
            notiTrans.DOScale(1.3f, 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
            dailyRewardNotiTrans.DOScale(1.3f, 1f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        }

        void OnDestroy()
        {
            notiTrans.DOKill();
            dailyRewardNotiTrans.DOKill();
        }

        public override void Show(bool withTransition = true, System.Action onCompleted = null)
        {
            base.Show(withTransition, onCompleted);

            if (originalGroupBottomPos == Vector2.zero)
            {
                originalGroupBottomPos = groupBottomTrans.anchoredPosition;
            }

            if (originalGroupBottomMenuPos == Vector2.zero)
            {
                originalGroupBottomMenuPos = groupBottomMenuTrans.anchoredPosition;
            }

            if (originalGroupTopPos == Vector2.zero)
            {
                originalGroupTopPos = groupTopTrans.anchoredPosition;
            }

            if (originalGroupButtonLeftPos == Vector2.zero)
            {
                originalGroupButtonLeftPos = groupButtonLeftTrans.anchoredPosition;
            }

            OnLoadAreaCompleted();
            OnCurrencyBalanceChanged(CurrencyType.Star);
            OnPlayerLevelUp();
            
            //Delay a bit to let the layout group to calculate the size of each element (so we don't have to calculate it ourselves)
            DOVirtual.DelayedCall(0.05f, () =>
            {
                if (selectedWidth > 0)
                    return;
                
                selectedWidth = (uiMenuTabView.selectedButton.transform as RectTransform).GetWidth();
                unSelectedWidth = (uiMenuTabView.tabButtons[0].transform as RectTransform).GetWidth();
                uiMenuTabView.selectedButton.SetWidthLayout(selectedWidth);
                
                //Then disable the child control width to control the size of each element ourselves
                uiMenuTabView.GetComponent<HorizontalLayoutGroup>().childControlWidth = false;
            });

            //Register event listener
            var homeManager = HomeManager.Instance;

            homeManager.onAreaCompleted -= OnAreaCompleted;
            homeManager.onAreaCompleted += OnAreaCompleted;

            homeManager.onReturnToIngame -= PressGoToInGame;
            homeManager.onReturnToIngame += PressGoToInGame;

            homeManager.onPlayerLevelUp -= OnPlayerLevelUp;
            homeManager.onPlayerLevelUp += OnPlayerLevelUp;

            uiMenuTabView.onSelectedButtonChanged -= OnSelectTabButtonChanged;
            uiMenuTabView.onSelectedButtonChanged += OnSelectTabButtonChanged;

            foreach (var decoItem in HomeManager.Instance.readonlyDecoItems)
            {
                decoItem.onDecoItemStateChanged -= OnDecoItemStateChanged;
                decoItem.onDecoItemStateChanged += OnDecoItemStateChanged;
            }

            var userManager = UserManager.Instance;
            userManager.onCurrencyBalanceChanged -= OnCurrencyBalanceChanged;
            userManager.onCurrencyBalanceChanged += OnCurrencyBalanceChanged;

            userManager.onDailyRewardClaimed -= OnDailyRewardClaimed;
            userManager.onDailyRewardClaimed += OnDailyRewardClaimed;

        }

        void OnDailyRewardClaimed()
        {
            dailyRewardNotiTrans.gameObject.SetActive(false);
        }

        void OnPlayerLevelUp()
        {
            var userManager = UserManager.Instance;
           
            dailyRewardButton.gameObject.SetActive(false);
            dailyRewardNotiTrans.gameObject.SetActive(false);

            if (userManager.currentPlayerLevel >= ConfigManager.Instance.configDailyReward.unlockAtPlayerLevel)
            {
                dailyRewardNotiTrans.gameObject.SetActive(userManager.hasPendingClaimReward);

                if (userManager.isPlayingDecoBuildingFromInGameScene)
                {
                }
                else
                {
                    dailyRewardButton.transform.localScale = Vector3.zero;
                    dailyRewardButton.transform.DOPopIn(0.5f, 1);
                }
            }
            
            rouletteButton.gameObject.SetActive(false);
            if (userManager.currentPlayerLevel >= ConfigManager.Instance.configRoulette.unlockAtPlayerLevel)
            {

                if (userManager.isPlayingDecoBuildingFromInGameScene)
                {
                }
                else
                {
                    rouletteButton.transform.localScale = Vector3.zero;
                    rouletteButton.transform.DOPopIn(0.5f, 1);
                }
            }
        }

        void OnCurrencyBalanceChanged(CurrencyType currencyType)
        {
            if (currencyType == CurrencyType.Star)
            {
                CheckNoti();
            }
        }

        void OnDecoItemStateChanged(DecoItemController decoItem, DecoItemState oldState)
        {
            var state = decoItem.decoItemState;
            if (state == DecoItemState.Completed)
            {
                //Todo:Should wait for deco complete unlock and update with transition
                UpdateAreaProgress();
                UpdateNextBuildDecoInfo(false);
            }
        }

        void RegisterDecoStateChanged()
        {
            foreach (var decoItem in HomeManager.Instance.readonlyDecoItems)
            {
                decoItem.onDecoItemStateChanged -= OnDecoItemStateChanged;
                decoItem.onDecoItemStateChanged += OnDecoItemStateChanged;
            }
        }

        void OnLoadAreaCompleted()
        {
            uiMenuTabView.Setup();
            uiAreaSelectPopup.ScrollToTop();

            UpdateAreaProgress();
            UpdateNextBuildDecoInfo(true);
        }

        void UpdateAreaProgress()
        {
            var userManager = UserManager.Instance;
            var configAreaItem = HomeManager.Instance.currentConfigArea;
            var data = userManager.currentSelectAreaData;
            areaNameText.text = "Area " + (userManager.currentSelectAreaId + 1);
            areaProgressSlider.value = (float)data.completedDecoIds.Count / configAreaItem.decoItems.Count;
            areaProgressText.text = $"{data.completedDecoIds.Count}/{configAreaItem.decoItems.Count}";
        }

        void UpdateNextBuildDecoInfo(bool showSpeechBubble)
        {
            var userManager = UserManager.Instance;
            var nextConfigDeco = userManager.GetNextUnlockConfigDeco(userManager.currentSelectAreaId, out var isAreaCompleted);
            if (isAreaCompleted || nextConfigDeco == null)
            {
                uiNextDecoInfo.Hide();
            }
            else
            {
                string speech = nextConfigDeco.description.ToLower();
                speech = "Please " + speech;
                if (showSpeechBubble)
                {
                    uiNextDecoInfo.Show(speech);
                }
                else
                {
                    uiNextDecoInfo.UpdateSpeech(speech);
                }
            }
        }

        void OnSelectTabButtonChanged(UIMenuTabButton oldTabButton)
        {
            if (oldTabButton != null)
                oldTabButton.SetWidthLayout(unSelectedWidth,true);
            
            uiMenuTabView.selectedButton.SetWidthLayout(selectedWidth,true);

            int index = uiMenuTabView.tabButtons.IndexOf(uiMenuTabView.selectedButton);

            if (index == 1) //Area
            {
                uiAreaSelectPopup.Show();
            }
            else if (index == 0) //In-home
            {

            }
        }

        void OnDisable()
        {
            var homeManager = HomeManager.Instance;
            if (homeManager != null)
            {
                homeManager.onAreaCompleted -= OnAreaCompleted;
                homeManager.onReturnToIngame -= PressGoToInGame;
                homeManager.onPlayerLevelUp -= OnPlayerLevelUp;

                foreach (var decoItem in homeManager.readonlyDecoItems)
                {
                    decoItem.onDecoItemStateChanged -= OnDecoItemStateChanged;
                }
            }

            uiMenuTabView.onSelectedButtonChanged -= OnSelectTabButtonChanged;
            uiAreaSelectPopup.onLoadAreaCompleted -= OnLoadAreaCompleted;
            uiAreaCompleted.onLoadAreaCompleted -= OnLoadAreaCompleted;

            var userManager = UserManager.Instance;
            if (userManager != null)
            {
                userManager.onCurrencyBalanceChanged -= OnCurrencyBalanceChanged;
                userManager.onDailyRewardClaimed -= OnDailyRewardClaimed;
            }
        }
       
        public void ShowGroupInteractableButton(bool isShowing)
        {
            float moveDuration = 0.8f;

            if (isShowing)
            {
                groupBottomTrans.DOAnchorPos(originalGroupBottomPos, moveDuration).SetEase(Ease.OutBack);
                groupTopTrans.DOAnchorPos(originalGroupTopPos, moveDuration).SetEase(Ease.OutBack);
                groupBottomMenuTrans.DOAnchorPos(originalGroupBottomMenuPos, moveDuration).SetEase(Ease.OutQuad);
                groupButtonLeftTrans.DOAnchorPos(originalGroupButtonLeftPos, moveDuration).SetEase(Ease.OutBack);
            }
            else
            {
                var endPos = new Vector2(originalGroupBottomPos.x, UIManager.Instance.minRectPoint.y);
                groupBottomTrans.DOAnchorPos(endPos, moveDuration).SetEase(Ease.InBack);
                groupBottomMenuTrans.DOAnchorPos(endPos, moveDuration).SetEase(Ease.OutQuad);

                endPos = new Vector2(originalGroupTopPos.x, UIManager.Instance.maxRectPoint.y);
                groupTopTrans.DOAnchorPos(endPos, moveDuration).SetEase(Ease.InBack);

                endPos = new Vector2(UIManager.Instance.minRectPoint.x, originalGroupButtonLeftPos.y);
                groupButtonLeftTrans.DOAnchorPos(endPos, moveDuration).SetEase(Ease.InBack);
            }
        }

        public void CheckNoti()
        {
            UserManager.Instance.CheckCanBuildDeco(out var areaId, out var decoId);
            notiTrans.gameObject.SetActive(areaId >= 0 && !string.IsNullOrEmpty(decoId));
        }
        
        void OnAreaCompleted()
        {
            uiAreaCompleted.Show(HomeManager.Instance.currentConfigArea.id + 1);
        }

        #region Testing only
        // void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.A))
        //     {
        //         uiAreaCompleted.Show(0 + 1);
        //     }
        // }
        #endregion

        public void PressGoToInGame()
        {
            BootManager.Instance.LoadScene(SceneConstants.IN_GAME_SCENE_NAME, true);
        }

        public void PressBuildDeco()
        {
            UIManager.Instance.ShowPopup<UIDecoBuildPopup>();

            var uiDecoBuildPopup = UIManager.Instance.currentPopup as UIDecoBuildPopup;
            var data = UserManager.Instance.currentSelectAreaData;
            var configDecoItems = HomeManager.Instance.currentConfigArea.decoItems;
            var nextBuildingDecoId = configDecoItems[Mathf.Min(configDecoItems.Count - 1, data.completedDecoIds.Count)].id;
            uiDecoBuildPopup.UpdateState(data.areaId,  nextBuildingDecoId);
        }

        public void PressDecoCost(UIDecoCost uiDecoCost)
        {
            if (UserManager.Instance.isPlayingDecoBuildingFromInGameScene)
                return;

            UIManager.Instance.ShowPopup<UIAreaDecorationPoup>();
        }

        public void PressDailyReward()
        {
            UIManager.Instance.ShowPopup<UIDailyRewardPopup>();
        }

        public void PressRoulette()
        {
            UIManager.Instance.ShowPopup<UIRoulettePopup>();
        }

       
    }
}
