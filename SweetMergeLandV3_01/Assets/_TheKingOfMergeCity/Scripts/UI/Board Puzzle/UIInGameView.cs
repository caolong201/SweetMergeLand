using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using USimpFramework.UI;
using USimpFramework.Utility;
using TMPro;
using DG.Tweening;
using Coffee.UIExtensions;

using UnityEngine.UI.Extensions.CasualGame;

namespace TheKingOfMergeCity
{
    using Enum;
    using Config;

    public class UIInGameView : UIViewBase
    {
        [SerializeField] Transform _puzzleItemContainerTrans;
        public Transform puzzleItemContainerTrans => _puzzleItemContainerTrans;

        [SerializeField] Transform _groupRowTrans;
        public Transform groupRowTrans => _groupRowTrans;

        [SerializeField] Transform _customerOrderContainerTrans;
        public Transform customerOrderContainerTrans => _customerOrderContainerTrans;

        [SerializeField] TMP_Text boardFullText;
        [SerializeField] UITopBar _uiTopBar;
        public UITopBar uiTopBar => _uiTopBar;

        [SerializeField] Image imageSelector;

        [SerializeField] Image tileBgImagePrefab;

        [SerializeField] ParticleSystem mergeItemGlowFx;

        [SerializeField] Transform groupButtonTrans;

        [SerializeField] Button _buildButton;
        public Button buildButton => _buildButton;

        [Header("Provider external puzzle items")]
        [SerializeField] Button _itemProviderButton;
        public Button itemProviderButton => _itemProviderButton;

        [SerializeField] TMP_Text rewardItemCountText;
        [SerializeField] Image rewardItemImage;

        Transform imageSelectorOriginalParentTrans;

        [SerializeField] UIParticle giftScatterVfxPrefab;
        [SerializeField] UIParticle itemMergedVfx;

        [SerializeField] Button _inventoryButton;
        public Button inventoryButton => _inventoryButton;
    
        void Start()
        {
            boardFullText.gameObject.SetActive(false);
            imageSelector.gameObject.SetActive(false);
            imageSelectorOriginalParentTrans = imageSelector.transform.parent;
            tileBgImagePrefab.gameObject.SetActive(false);
            CreateBackground();
        }


        int canBuildAreaId = -1;
        string canBuildDecoId;

        void OnEnable()
        {
            groupButtonTrans.gameObject.SetActive(false);
       
            var userManager = UserManager.Instance;
            userManager.onCurrencyBalanceChanged += OnCurrencyBalanceChanged;
            userManager.onDecoCanBuild += OnDecoCanBuild;
            userManager.CheckCanBuildDeco(out var areaId, out string decoId);
            
            if (areaId >= 0)
                OnDecoCanBuild(areaId, decoId);
            else
            {
                buildButton.gameObject.SetActive(false);
                if (!buildButton.gameObject.activeSelf && !itemProviderButton.gameObject.activeSelf)
                    groupButtonTrans.gameObject.SetActive(false);
            }

            BootManager.Instance.onBeforeSceneLoaded -= OnBeforeSceneLoaded;
            BootManager.Instance.onBeforeSceneLoaded += OnBeforeSceneLoaded;
            
            
            //Reupdate the external reward puzzle items
            UpdateExtenalRewardPuzzleItems();
        }
        void OnBeforeSceneLoaded(string sceneName)
        {
            if (sceneName != SceneConstants.IN_GAME_SCENE_NAME)
                ShowSelectorIndicator(false, imageSelectorOriginalParentTrans);
        }


        void UpdateExtenalRewardPuzzleItems()
        {
            var externalPuzzleItems = UserManager.Instance.externalPuzzleItems;

            if (externalPuzzleItems.Count == 0)
            {
                itemProviderButton.gameObject.SetActive(false);

                if (!buildButton.gameObject.activeSelf && !itemProviderButton.gameObject.activeSelf)
                    groupButtonTrans.gameObject.SetActive(false);
            }
            else
            {
                groupButtonTrans.gameObject.SetActive(true);
                itemProviderButton.gameObject.SetActive(true);

                rewardItemCountText.text = externalPuzzleItems.Count.ToString();
                var firstConfigReward = externalPuzzleItems[0];
                var configPuzzle = ConfigManager.Instance.configPuzzle.configItems.Find(c => c.id == firstConfigReward.puzzleId);
                if (configPuzzle == null)
                    throw new UnityException($"Something wrong! Invalid puzzle id {firstConfigReward.puzzleId}");

                rewardItemImage.sprite = configPuzzle.configPerLevel[firstConfigReward.level].itemSprite;

            }
        }

        void OnDecoCanBuild(int areaId, string decoId)
        {
            canBuildAreaId = areaId;
            canBuildDecoId = decoId;
            buildButton.gameObject.SetActive(true);
            buildButton.transform.parent.SetAsFirstSibling();
            groupButtonTrans.gameObject.SetActive(true);

        }

        void OnDisable()
        {
            UserManager.Instance.onCurrencyBalanceChanged -= OnCurrencyBalanceChanged;
            UserManager.Instance.onDecoCanBuild -= OnDecoCanBuild;
            BootManager.Instance.onBeforeSceneLoaded -= OnBeforeSceneLoaded;
        }

        void OnCurrencyBalanceChanged(CurrencyType currencyType)
        {
            if (currencyType == CurrencyType.Energy)
            {
                int balance = UserManager.Instance.GetCurrencyBalance(CurrencyType.Energy);
                if (balance == 0)
                {
                    UIManager.Instance.ShowPopup<UIBuyEnergyPopup>();
                }
            }
        }


        public void PressProviderItem()
        {
            //Find the first empty slot
            var puzzleControllers = InGameManager.Instance.puzzlesController;
            var firstEmptyTile = puzzleControllers.GetFirstEmptyTile();
            if (firstEmptyTile == null)
            {
                UIManager.Instance.ShowFloatingText("Dont have empty tile!");
                return;
            }

            var configRewardPuzzleItem = UserManager.Instance.externalPuzzleItems[0];

            var puzzleItem = puzzleControllers.SpawnPuzzleItem(configRewardPuzzleItem.puzzleId, configRewardPuzzleItem.level, 2, firstEmptyTile);

            //Set the item image from this button
            puzzleItem.itemImage.transform.position = itemProviderButton.transform.position;
            puzzleItem.itemImage.transform.DOLocalMove(Vector2.zero, 0.5f).SetEase(Ease.OutSine);

            //Remove from external data
            UserManager.Instance.RemoveExternalRewardPuzzle(configRewardPuzzleItem.puzzleId);

            UpdateExtenalRewardPuzzleItems();

            if (puzzleItem is UIPuzzleNormalItemController)
            {
                //Must recheck the order
                InGameManager.Instance.customersController.CheckCompleteOrder(puzzleItem as UIPuzzleNormalItemController, -1);
            }
        }
        

        public void ShowMergeItemGlowFx(Vector2 position, bool isShow)
        {
            if (isShow)
            {
                mergeItemGlowFx.gameObject.SetActive(true);
                mergeItemGlowFx.transform.position = position;
                mergeItemGlowFx.Play();
            }
            else
            {
                mergeItemGlowFx.gameObject.SetActive(false);
            }
        }

        public void ShowBreakBlockFx(Vector2 position)
        {
            var fx = SimpleObjectPool.Spawn(giftScatterVfxPrefab, giftScatterVfxPrefab.transform.parent);
            fx.transform.position = position;
            fx.Play();

            DOVirtual.DelayedCall(1f, () => SimpleObjectPool.Despawn(fx, moveToPoolContainer:false));
        }

        public void ShowItemMergeFx(Vector2 position, bool withChildren)
        {
            itemMergedVfx.Stop();
            itemMergedVfx.transform.position = position;
            itemMergedVfx.Play();
        }

        void CreateBackground()
        {
            bool isDark = true;
            var configPuzzle = ConfigManager.Instance.configPuzzle;
            var boardSize = configPuzzle.boardSize;
            for (int irow = 0; irow < boardSize.y; irow++)
            {
                for (int icol = 0; icol < boardSize.x; icol++)
                {
                    var bgIns = Instantiate(tileBgImagePrefab, tileBgImagePrefab.transform.parent);
                    bgIns.sprite = isDark ? configPuzzle.darkTileSprite : configPuzzle.lightTileSprite;
                    bgIns.gameObject.SetActive(true);
                    isDark = !isDark;
                }
            }
        }

        public void ShowBoardFullText(string description, Vector2 position)
        {
            var text = SimpleObjectPool.Spawn(boardFullText, boardFullText.transform.parent);
            text.text = description;
            var textTrans = text.transform;
            textTrans.position = position;
            var color = text.color;
            color.a = 1;
            text.color = color;

            textTrans.DOKill();
          
            var sequence = DOTween.Sequence();
            sequence.Append(textTrans.DOScale(1.3f, 0.1f));
            sequence.Append(textTrans.DOScale(1, 0.1f));
            sequence.AppendInterval(0.8f);
            sequence.Append(text.DOFade(0, 0.3f).OnStart(() => textTrans.DOMoveY(position.y + 30f, 0.3f)));
            sequence.OnComplete(() =>
            {
                SimpleObjectPool.Despawn(text, moveToPoolContainer:false);
            });
        }

        public void ShowSelectorIndicator(bool isShow, Transform parentTrans)
        {
            imageSelector.transform.SetParent(parentTrans);
            if (isShow)
            {
                imageSelector.gameObject.SetActive(true);
                var rect = imageSelector.rectTransform;
                rect.anchoredPosition = Vector2.zero;
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.sizeDelta = new Vector2(30, 30);

                imageSelector.transform.localScale *= 0.9f;
                imageSelector.transform.DOKill();
                imageSelector.transform.DOScale(1.2f, 0.1f).OnComplete(() =>
                imageSelector.transform.DOScale(1, 0.1f));
               
            }
            else
            {
                imageSelector.gameObject.SetActive(false);
            }
        }
        
        public void CheckHideSelector(Transform compareTrans)
        {
            if (imageSelector.transform.parent == compareTrans)
            {
                ShowSelectorIndicator(false, imageSelectorOriginalParentTrans);
            }
        }

        public void PressBuildDeco()
        {
            if (InGameManager.Instance.isPlayingServeCustomer)
                return;

            var tutorialManager = TutorialManager.Instance;
            var uiManager = UIManager.Instance;

            tutorialManager.CheckCompleteStep<ConfigTutorialClickBuildDeco>();

            uiManager.ShowPopup<UIDecoBuildPopup>(onCompleted: () =>
            {
                //Check tutorial
                if (!UserManager.Instance.finishTutorial && TutorialManager.Instance.currentStep.config is ConfigTutorialClickStartInBuildPopup)
                {
                    uiManager.GetPopup<UITutorialPopup>().ShowEffect();
                }
            });
           
            var uiDecoBuildPopup = uiManager.currentPopup as UIDecoBuildPopup;
            uiDecoBuildPopup.UpdateState(canBuildAreaId, canBuildDecoId);
        }

        public void PressBackToHome()
        {
            if (!UserManager.Instance.finishTutorial || InGameManager.Instance.isPlayingServeCustomer)
                return;
            
            ApplovinManager.Instance.HideBannerAd();

            BootManager.Instance.LoadScene(SceneConstants.HOME_SCENE_NAME, true);
        }

        public void PressInventory()
        {
            if (!UserManager.Instance.finishTutorial)
                return;

            UIManager.Instance.ShowPopup<UIPuzzleInventoryPopup>();
        }
    }
}
