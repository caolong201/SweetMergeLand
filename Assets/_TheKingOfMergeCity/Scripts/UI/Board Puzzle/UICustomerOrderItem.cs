using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using USimpFramework.Animation.DOTweenExtension;
using USimpFramework.UI;
using USimpFramework.UIGachaEffect;
using USimpFramework.Utility;
using DG.Tweening;

namespace TheKingOfMergeCity
{
    using Config;
    using Enum;
    using Model;
    using TMPro;

    public class UICustomerOrderItem : MonoBehaviour
    {
        public event System.Action<UICustomerOrderItem> onStartServe;

        [SerializeField] Image customerImage;
        [SerializeField] UIFoodOrderItem uiFoodOrderItemPrefab;
        [SerializeField] UIRewardItem uiRewardItemPrefab;
        [SerializeField] Transform contentTrans;
        [SerializeField] Transform trayTrans;
        [SerializeField] Transform rewardContainerTrans;
        [SerializeField] Transform spawnStarTrans;
        [SerializeField] Transform customerRootTrans;
        [SerializeField] ParticleSystem pendingServeFx;
        [SerializeField] ElasticScale customerScalePendingServe;
        [SerializeField] FloatingObject customerFloat;
    
        [SerializeField] Button _serveButton;
        public Button serveButton => _serveButton;

        [SerializeField] TextMeshProUGUI pendingServeText;
        [SerializeField] CanvasGroup pendingServeTextGroup;

        #region This for pooling
        List<UIFoodOrderItem> uiFoodOrderItems = new();
        List<UIRewardItem> uiRewardItems = new();
        #endregion
 
        public IReadOnlyList<UIPuzzleNormalItemController> readonlyPuzzleItemHasCompletedOrder => puzzleItemsHasCompletedOrder;

        public bool hasPendingServe { get; private set; }

        public bool isPlayingServeTransition { get; private set; }

        public UserCustomerDataItem data { get; private set; }

        List<UIPuzzleNormalItemController> puzzleItemsHasCompletedOrder = new(); //This need to clear after pooling
        bool isAllOrderCompleted => uiFoodOrderItems.TrueForAll(ui => ui.gameObject.activeSelf && ui.isCompleted);

        /// <summary>
        /// Start is called once, but do not put any pooling state object in here, because Start() is called after Setup() is called
        /// </summary>
        void Start()
        {
            uiFoodOrderItemPrefab.gameObject.SetActive(false);
            uiRewardItemPrefab.gameObject.SetActive(false);
           
        }

        
        #region Testing Purpose
        /*void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                customerScalePendingServe.Play();
            }
        }*/
        #endregion

        /// <summary>
        /// Setup is called when spawn customer (from pool)
        /// </summary>
        /// <param name="data"></param>
        public void Setup(UserCustomerDataItem data)
        {
            ResetData();

            this.data = data;

            //Update customer image
            var configCharacter = ConfigManager.Instance.configCharacter.items.Find(s => s.id == data.customerId);
            if (configCharacter == null)
            {
                Debug.LogError($"Update customer failed! Cannot find character with id {data.customerId}");
            }
            else
            {
                customerImage.sprite = configCharacter.iconSprite;
            }

            //Spawn food orders
            //Debug.Log($"<color=yellow>Customer {transform.GetInstanceID()} setup food orders count {uiFoodOrderItems.Count}, data orders count {data.orders.Count} </color>");
            
            while (uiFoodOrderItems.Count < data.orders.Count)
            {
                var ui = Instantiate(uiFoodOrderItemPrefab, uiFoodOrderItemPrefab.transform.parent);
                uiFoodOrderItems.Add(ui);
            }

            uiFoodOrderItems.ForEach(ui => ui.gameObject.SetActive(false));

            for (int i = 0; i < data.orders.Count; i++)
            {
                var ui = uiFoodOrderItems[i];
                var configOrder = data.orders[i];
                ui.Setup(configOrder.puzzleId, configOrder.level, false);
            }

            //Spawn reward when completed or the order
            while (uiRewardItems.Count < data.rewards.Count)
            {
                var ui = Instantiate(uiRewardItemPrefab, uiRewardItemPrefab.transform.parent);
                uiRewardItems.Add(ui);
            }

            uiRewardItems.ForEach(ui => ui.gameObject.SetActive(false));

            for (int i = 0; i < data.rewards.Count; i++)
            {
                var reward = data.rewards[i];
                uiRewardItems[i].Setup(new ConfigRewardItem(RewardType.Currency, reward.currencyType, reward.amount));
            }

        }

        void ResetData()
        {
            customerFloat.enabled = false;
            puzzleItemsHasCompletedOrder.Clear();
            hasPendingServe = false;
            serveButton.gameObject.SetActive(false);

            if (pendingServeTextGroup != null)
            {
                pendingServeTextGroup.DOKill();
                pendingServeTextGroup.alpha = 1f;
                pendingServeTextGroup.gameObject.SetActive(false);
            }
            else if (pendingServeText != null)
            {
                pendingServeText.gameObject.SetActive(false);
            }

            if (pendingServeFx != null)
            {
                pendingServeFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                pendingServeFx.gameObject.SetActive(false);
            }


        }

        public void SmoothTransition(float duration = 0.2f)
        {
            contentTrans.SetParent(transform.parent.parent);
            contentTrans.SetAsFirstSibling();

            DOVirtual.DelayedCall(0.1f, () =>
            {
                if (!gameObject)
                    return;

                contentTrans.SetParent(transform);
                contentTrans.DOLocalMove(Vector3.zero, duration).SetEase(Ease.OutSine);
            });
        }

        public void CheckCompleteAllOrders()
        {
            var unblockNormalItems = InGameManager.Instance.puzzlesController.GetUnblockNormalItems();
            foreach (var puzzleItem in unblockNormalItems)
            {
                var uiFoodOrder = uiFoodOrderItems.Find(ui => ui.gameObject.activeSelf && ui.itemId == puzzleItem.config.id && ui.level == puzzleItem.data.level);
                if (uiFoodOrder != null)
                {
                    //Check if this food or is completed before meaning that the comparing puzzle item is has the same match condition of the existing one of puzzleItemHasOrder
                    uiFoodOrder.isCompleted = true;
                    puzzleItem.SetCompletedOrder(true);
                    puzzleItemsHasCompletedOrder.Add(puzzleItem);
                }
            }

            if (isAllOrderCompleted)
            {
                SetPendingServe(true);
            }

            //Check star coin to show the build button
            UserManager.Instance.CheckCanBuildDeco(out _, out _);
        }

        public bool HasOrderOfThisItem(string puzzleItemId)
        {
            return data.orders.Exists(a => a.puzzleId == puzzleItemId);
        }

        public void CheckCompleteOrder(UIPuzzleNormalItemController puzzleItem, int oldLevel)
        {
            bool hasAdded = false;
            foreach (var uiFoodOrderItem in uiFoodOrderItems)
            {
                if (!uiFoodOrderItem.gameObject.activeSelf)
                    continue;

                //Important Need to handle: What if this puzzle is serve to other/destroy when merge to another/ upgrade level
                if (uiFoodOrderItem.level == puzzleItem.data.level && uiFoodOrderItem.itemId == puzzleItem.config.id)
                {
                    uiFoodOrderItem.isCompleted = true;
                    puzzleItem.SetCompletedOrder(true);
                    puzzleItemsHasCompletedOrder.Add(puzzleItem);
                    hasAdded = true;
                    break;
                }
                else //Check remove this puzzle
                {
                    RemoveOrderedPuzzle(puzzleItem, oldLevel);
                }
            }

            //Debug.Log($"Customer {transform.GetInstanceID()} check complete order for {puzzleItem.config.id}, " +
            //    $"old level {oldLevel}, level {puzzleItem.data.level}, complete count: {uiFoodOrderItems.Count(ui => ui.isCompleted)}");

            if (isAllOrderCompleted)
            {
                if (hasAdded)
                    puzzleItem.SetPendingServe(true);
                SetPendingServe(true);
            }
            else
            {
                puzzleItem.SetPendingServe(false);
                SetPendingServe(false);
            }
        }

        public void PressServe()
        {
            if (!hasPendingServe)
                return;

            var uiInGameView = UIManager.Instance.currentView as UIInGameView;


            hasPendingServe = false;
            onStartServe?.Invoke(this);

            pendingServeFx.gameObject.SetActive(false);

            serveButton.transform.DOPopOut(0.4f);

            customerFloat.BackDefault();
            customerFloat.enabled = false;


            if (pendingServeFx != null)
            {
                pendingServeFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                pendingServeFx.gameObject.SetActive(false);
            }
            // also stop text
            if (pendingServeTextGroup != null)
            {
                pendingServeTextGroup.DOKill();
                pendingServeTextGroup.gameObject.SetActive(false);
            }
            else if (pendingServeText != null)
            {
                pendingServeText.DOKill();
                pendingServeText.gameObject.SetActive(false);
            }



            var configRewards = new List<ConfigRewardItem>();
            var userManager = UserManager.Instance;
            var puzzlesController = InGameManager.Instance.puzzlesController;
            var customersController = InGameManager.Instance.customersController;

            //Claim reward
            foreach (var ui in uiRewardItems)
            {
                if (!ui.gameObject.activeSelf)
                    continue;
                configRewards.Add(new ConfigRewardItem(RewardType.Currency, ui.config.currencyType, ui.config.amount));
            }
            
            userManager.ClaimRewards(configRewards);

            //Remove this customer from data
            userManager.RemoveServingCustomer(data.customerId);

            //------Transition--------
            StartCoroutine(CR_ServeTransition());
            //--------End transition

            puzzleItemsHasCompletedOrder.Clear();

            IEnumerator CR_ServeTransition()
            {
                //Disable all the interaction except the playing board puzzle
                InGameManager.Instance.isPlayingServeCustomer = true;

                List<UIPuzzleNormalItemController> actualRemovedItems = new();
                List<UIPuzzleNormalItemController> remain = new();
                //Get the puzzle correspond to the item
                foreach (var puzzleItem in puzzleItemsHasCompletedOrder)
                {
                    if (!actualRemovedItems.Exists(i => i.config.id == puzzleItem.config.id && i.data.level == puzzleItem.data.level))
                    {
                        actualRemovedItems.Add(puzzleItem);
                    }
                    else
                    {
                        remain.Add(puzzleItem);
                    }
                    uiInGameView.CheckHideSelector(puzzleItem.transform);
                }

                const float flyDuration = 0.45f;
                foreach (var puzzleItem in actualRemovedItems)
                {
                    //Check other custtomer
                    foreach (var customer in customersController.readonlyServingCustomers)
                    {
                        customer.RemoveOrderedPuzzle(puzzleItem, puzzleItem.data.level);
                    }

                    var foodOrder = uiFoodOrderItems.Find(ui => ui.itemId == puzzleItem.config.id && ui.level == puzzleItem.data.level);

                    puzzlesController.RemovePuzzle(puzzleItem);

                    //Fly the puzzle item in the board from the corresponding food order
                    puzzleItem.HideTickAndBackground();
                    puzzleItem.transform.SetParent(foodOrder.transform);

                    //Debug.Log($"PUzzle item {puzzleItem.config.id}-{puzzleItem.level}, parent {puzzleItem.transform.parent.name}");
                    puzzleItem.Move(foodOrder.transform.position, false, false, Ease.OutSine, flyDuration, () =>
                    {
                        foodOrder.ShowIconImage(false);
                        foodOrder.isCompleted = false;
                        puzzleItem.transform.DOScale(0, 0.3f).SetEase(Ease.Linear).OnComplete(() =>
                        {
                            SimpleObjectPool.Despawn(puzzleItem, moveToPoolContainer: false);
                        });
                    });
                }

                foreach (var puzzleItem in remain)
                {
                    puzzleItem.SetCompletedOrder(false);
                    puzzleItem.SetPendingServe(false);
                }

                //Wait until the flying puzzle reach the order
                yield return new WaitForSeconds(flyDuration + 0.55f);

                var uiTopBar = uiInGameView.uiTopBar;
                var uiStarCoin = uiTopBar.starCoinImage.transform;
                int starCoinAmount = configRewards.Find(r => r.currencyType == CurrencyType.Star).amount;

                //Todo: Improve UI gacha effect to handle each star coin collide with the ui top bar

                //Fly the coin gacha effect to UI top bar (coin star)
                /*UIGachaEffect.Instance.PlayGachaEffect("star_coin", starCoinAmount, spawnStarTrans.position, uiStarCoin.position, onFirstItemCompleted: () =>
               {
                   uiTopBar.IncreaseCurrency(CurrencyType.Star);
               });*/

                uiTopBar.PlayCurrencyGachaEffect(CurrencyType.Star, starCoinAmount, spawnStarTrans.position);

                yield return new WaitForSeconds(1.5f);

                //Remove this customer (exit), tray and currency container
                HideFromQueue(() =>
                 {
                     //Finally set the interaction
                     InGameManager.Instance.isPlayingServeCustomer = false;


                     //Show new customer in the queue
                     //By the time this customer id hiding complete, smooth transition other customer
                     foreach (var customer in customersController.readonlyServingCustomers)
                     {
                         customer.SmoothTransition();
                     }

                     var nextCustomer = customersController.SpawnNextCustomer();
                     if (nextCustomer != null)
                     {
                         nextCustomer.ShowFromQueue();
                     }
                 });
            }
        }

        /// <summary>
        /// Remove puzzle item which has been ordered in the puzzleItemHasOrder list of each customer
        /// </summary>
        /// <param name="puzzleItem">the need to remove puzzle item</param>
        /// <param name="oldLevel">level to check in that puzzle item, in case that the input puzzle item has upgraded, therefore the current level is changed</param>
        public void RemoveOrderedPuzzle(UIPuzzleNormalItemController puzzleItem, int oldLevel)
        {
            if (!puzzleItemsHasCompletedOrder.Contains(puzzleItem))
                return;

            //Debug.Log($"customer {transform.GetInstanceID()} remove order, id:{puzzleItem.config.id}, level {puzzleItem.level}, instance id {puzzleItem.transform.GetInstanceID()}, ");
            puzzleItemsHasCompletedOrder.Remove(puzzleItem);

            //Update UI food order on this customer
            var uiFoodOrder = uiFoodOrderItems.Find(ui => ui.gameObject.activeSelf && ui.itemId == puzzleItem.config.id && ui.level == oldLevel);

            if (uiFoodOrder != null && !puzzleItemsHasCompletedOrder.Exists(p => p.config.id == uiFoodOrder.itemId && p.data.level == uiFoodOrder.level))
                uiFoodOrder.isCompleted = false;

            if (!uiFoodOrderItems.TrueForAll(ui => ui.isCompleted))
            {
                //Check has pending serve
                SetPendingServe(false);

                puzzleItem.SetCompletedOrder(false);
                puzzleItem.SetPendingServe(false);
            }
        }

        void HideFromQueue(System.Action onCompleted = null)
        {
            customerRootTrans.DOPopOut(0.3f).OnComplete(() =>
            {
                trayTrans.DOPopOut(0.3f);
                rewardContainerTrans.DOPopOut(0.3f).OnComplete(() =>
                {
                    SimpleObjectPool.Despawn(this);
                    onCompleted?.Invoke();
                });
            });
        }

        void ShowFromQueue()
        {
            customerRootTrans.localScale = Vector3.zero;
            trayTrans.localScale = Vector3.zero;
            rewardContainerTrans.localScale = Vector3.zero;
            customerRootTrans.DOPopIn(0.5f).OnComplete(() =>
            {               
                CheckCompleteAllOrders();

                if (hasPendingServe)
                {
                    InGameManager.Instance.customersController.SortCustomerByFullCompletedOrder(true);
                }
                
                //Check complete step
                TutorialManager.Instance.CheckCompleteStep<ConfigServeCustomer>();
            });
            trayTrans.DOPopIn(0.5f);
            rewardContainerTrans.DOPopIn(0.5f);
        }

        void SetPendingServe(bool pendingServe)
        {
            //Debug.Log($"Customer {transform.GetInstanceID()} set pending serve, new {pendingServe}, old {hasPendingServe}");

            if (hasPendingServe == pendingServe)
                return;

            hasPendingServe = pendingServe;

            if (hasPendingServe)
            {
                //Scale elastic to highlight
                customerScalePendingServe.Play();
              
                pendingServeFx.gameObject.SetActive(true);
                pendingServeFx.Play();
                serveButton.transform.localScale = Vector3.zero;
                serveButton.transform.DOPopIn(0.2f).OnComplete(() =>
                {
                    serveButton.image.DOColor(new Color32(176, 176, 176, 255), 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
                    serveButton.transform.DOScale(1.2f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
                });



                if (pendingServeTextGroup != null)
                {
                    pendingServeTextGroup.gameObject.SetActive(true);
                    pendingServeTextGroup.alpha = 1f;
                    pendingServeTextGroup.DOFade(0.15f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
                }
                else if (pendingServeText != null)
                {
                    pendingServeText.gameObject.SetActive(true);
                  
                    pendingServeText.DOFade(0.15f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
                }


                puzzleItemsHasCompletedOrder.ForEach(h => h.SetPendingServe(true));
                customerFloat.enabled = true;

                //Check tutorial
                if (!UserManager.Instance.finishTutorial)
                {
                    if (TutorialManager.Instance.currentStep.config is ConfigServeCustomer)
                    {
                        var uiTutorialPopup = UIManager.Instance.currentPopup as UITutorialPopup;
                        uiTutorialPopup.ShowEffect();
                    }
                }
            }
            else
            {
                pendingServeFx.gameObject.SetActive(false);
                serveButton.image.DOKill();
                serveButton.transform.DOKill();
                serveButton.transform.DOPopOut(0.2f);
                puzzleItemsHasCompletedOrder.ForEach(h => h.SetPendingServe(false));
                customerFloat.BackDefault();
                customerFloat.enabled = false;


                if (pendingServeFx != null)
                {
                    pendingServeFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    pendingServeFx.gameObject.SetActive(false);
                }

                if (pendingServeTextGroup != null)
                {
                    pendingServeTextGroup.DOKill();
                    pendingServeTextGroup.alpha = 1f;
                    pendingServeTextGroup.gameObject.SetActive(false);
                }
                else if (pendingServeText != null)
                {
                    pendingServeText.DOKill();
                    pendingServeText.gameObject.SetActive(false);
                }
            }

           
        }

       
        void OnDestroy()
        {
            contentTrans.DOKill();
            trayTrans.DOKill();
            customerRootTrans.DOKill();
            serveButton.DOKill();
            serveButton.image.DOKill();
            serveButton.transform.DOKill();
            rewardContainerTrans.DOKill();


            if (pendingServeTextGroup != null) pendingServeTextGroup.DOKill();
            if (pendingServeText != null) pendingServeText.DOKill();
        }
    }
}
