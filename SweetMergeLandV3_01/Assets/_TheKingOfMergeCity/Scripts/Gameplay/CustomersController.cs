using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using USimpFramework.UI;
using USimpFramework.Utility;

namespace TheKingOfMergeCity
{
    using Config;
    using Enum;
    using Model;

    [DefaultExecutionOrder(100)]
    public class CustomersController : MonoBehaviour
    {
        [SerializeField] UICustomerOrderItem uiCustomerOrderItemPrefab;

        List<UICustomerOrderItem> servingCustomers = new();
        public IReadOnlyList<UICustomerOrderItem> readonlyServingCustomers => servingCustomers;

        Transform parentContainer;

        public void StartGame()
        {
            if (!UserManager.Instance.finishTutorial)
                return;

            //Spawn normal customer into scene
            int actualCount = ConfigManager.Instance.configLevel.maxAppearCustomer - servingCustomers.Count;
            for (int i = 0; i < actualCount; i++)
            {
                SpawnNextCustomer();
            }

            foreach (var customer in readonlyServingCustomers)
            {
                customer.CheckCompleteAllOrders();
            }
            
            SortCustomerByFullCompletedOrder();
        }

        public void LoadCustomerOrders(UserBoardData boardData, Transform parentContainer)
        {
            this.parentContainer = parentContainer;
            var currentServingCustomers = boardData.customerDatas.FindAll(c => c.isInQueue);
            for (int i = 0; i < currentServingCustomers.Count; i++)
            {
                var currentServingCustomer = currentServingCustomers[i];
                SpawnCustomerFromData(currentServingCustomer);
            }
        }

        void OnStartServeCustomer(UICustomerOrderItem customer)
        {
            //Remove this customer (exit customer)
            customer.onStartServe -= OnStartServeCustomer;
            servingCustomers.Remove(customer);
            //Data will be saved immediately (without waiting for any transition)
        }

        UICustomerOrderItem SpawnCustomerFromData(UserCustomerDataItem data)
        {
            var uiCustomer = SimpleObjectPool.Spawn(uiCustomerOrderItemPrefab, parentContainer);
            uiCustomer.Setup(data);
            uiCustomer.onStartServe -= OnStartServeCustomer;
            uiCustomer.onStartServe += OnStartServeCustomer;
            servingCustomers.Add(uiCustomer);
            return uiCustomer;
        }

        public UICustomerOrderItem SpawnNextCustomer()
        {
            var allCustomer = UserManager.Instance.boardData.value.customerDatas;
            var hiddenCustomers = allCustomer.FindAll(c => !c.isInQueue);
            UserCustomerDataItem data;
            if (hiddenCustomers.Count == 0)
            {
                //Add new random customer
                var randomConfig = GenerateRandomCustomerOrder(2, 4); //Todo: should be balanced by the current board data (how many empty tiles)
                data = UserManager.Instance.AddNewCustomer(randomConfig);
            }
            else
            {
                data = hiddenCustomers[0];
            }

            data.isInQueue = true;
            UserManager.Instance.boardData.SaveData();

            return SpawnCustomerFromData(data);
        }

        ConfigCustomerOrderItem GenerateRandomCustomerOrder(int minItemLevel, int maxItemLevel)
        {
            var orderCount = Random.Range(2, 4);
            var configManager = ConfigManager.Instance;
            var configCharacter = configManager.configCharacter;
            var configItems = configManager.configPuzzle.configItems;
            var result = new ConfigCustomerOrderItem(configCharacter.items[Random.Range(0, configCharacter.items.Count)].id);
            var itemIndices = new List<int>();

            foreach (var configProducer in InGameManager.Instance.puzzlesController.availableProducerConfigDic)
            {
                foreach (var setting in configProducer.Value.settingPerProducedItems)
                {
                    int index = configItems.FindIndex(s => s.id == setting.id);
                    if (index == -1)
                    {
                        Debug.LogError("Something wrong! Invalid id" + setting.id);
                    }

                    if (!itemIndices.Contains(index))
                        itemIndices.Add(index);
                }
            }

            for (int i = 0; i < orderCount; i++)
            {
                int randomItemLevel = Random.Range(minItemLevel, maxItemLevel);
                int randomItemIndex = itemIndices[Random.Range(0, itemIndices.Count)]; //Todo: random with the same probability as the config producer

                result.orders.Add(new ConfigOrderItem(configItems[randomItemIndex].id, randomItemLevel));
                itemIndices.Remove(randomItemIndex);
            }

            int totalStarReward = 0;
            foreach (var order in result.orders)
            {
                totalStarReward += order.level * configManager.configGlobal.starRewardPerItemLevel;
            }

            result.currencyRewards.Add(new ConfigRewardCurrencyItem(CurrencyType.Star, totalStarReward));

            return result;
        }

        /// <summary>
        /// Get customer that are pending serve
        /// </summary>
        /// <returns>The first customer that are pending serve, null if it cannot find any customer</returns>
        public UICustomerOrderItem GetPendingServeCustomer()
        {
            return servingCustomers.Find(c => c.hasPendingServe);
        }


        public void RemoveOrder(UIPuzzleNormalItemController puzzleItem)
        {
            servingCustomers.ForEach(c => c.RemoveOrderedPuzzle(puzzleItem, puzzleItem.data.level));
        }

        /// <summary>
        /// Check complete order and removed ordered puzzle item in each customer
        /// </summary>
        /// <param name="puzzleItem"></param>
        /// <param name="oldLevel">The old level of this puzzle item, just in case if this puzzle item has been merge upgraded, therefore, level of the puzzle item is changed</param>
        public void CheckCompleteOrder(UIPuzzleNormalItemController puzzleItem, int oldLevel)
        {
            servingCustomers.ForEach(customer =>
            {
                customer.CheckCompleteOrder(puzzleItem, oldLevel);
            });
            
            SortCustomerByFullCompletedOrder(true);
        }

        public bool HasFoodOrder(UIPuzzleNormalItemController puzzleItem)
        {
            return servingCustomers
                .Any(customer => customer.readonlyPuzzleItemHasCompletedOrder
                    .Any(itemHasOrder => puzzleItem == itemHasOrder));
        }

        public bool HasPendingServeCustomer(UIPuzzleNormalItemController puzzleItem)
        {
            return servingCustomers
                .Any(customer => customer.readonlyPuzzleItemHasCompletedOrder
                    .Any(itemHasOrder => puzzleItem == itemHasOrder && customer.hasPendingServe));
        }

        public void SortCustomerByFullCompletedOrder(bool withTransition = false)
        {
            if (servingCustomers.Count < 1)
                return;
            
            //Check for has already sorted first
            var sortedCustomers = servingCustomers.OrderByDescending(c => c.hasPendingServe).ToList();
            bool needSort = false;
            
            for (int i = 0; i < sortedCustomers.Count; i++)
            {
                if (sortedCustomers[i].transform.GetInstanceID() != servingCustomers[i].transform.GetInstanceID())
                {
                    needSort = true;    
                    break;
                }
            }

            if (needSort)
            {
                servingCustomers = sortedCustomers;
                
                var uiIngameView = UIManager.Instance.currentView as UIInGameView;
            
                int offset = uiIngameView.buildButton.gameObject.activeSelf ? 1 : 0;
                for (int i = 0; i < servingCustomers.Count; i++)
                {
                    servingCustomers[i].transform.SetSiblingIndex(i + offset);
                }

                if (withTransition)
                {
                    foreach (var customer in servingCustomers)
                    {
                        customer.SmoothTransition(0.3f);
                    }
                }
            }
        }


        void OnDestroy()
        {
            //DEstroy all the customer, because those are spawned on UIIngameView, which are DDOL by default
            for (int i = 0; i < servingCustomers.Count; i++)
            {
                var servingCustomer = servingCustomers[i];
                if (servingCustomer != null)
                    Destroy(servingCustomer.gameObject);
            }
        }
    }
}
