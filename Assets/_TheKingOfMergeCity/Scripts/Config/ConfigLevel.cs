using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using TheKingOfMergeCity.Model;

namespace TheKingOfMergeCity.Config
{

    using Enum;
    using USimpFramework.EditorExtension;

    [Serializable]
    public class ConfigPuzzleInLevel
    {
        [Tooltip("Board position of the puzzle, x is col, y is row, starting from 0 and from the top left of the board")]
        [SerializeField] BoardPosition _position;
        public BoardPosition position => _position;

        [SerializeField] string _puzzleId;
        public string puzzleId => _puzzleId;

        [Tooltip("Level of puzzle, starting from 0")]
        [SerializeField] int _level;
        public int level => _level;

        [Tooltip("Level of blocking, starting from 0, if blocking level is 3, this mean is fulled unlock")]
        [SerializeField] int _blockingLevel;
        public int blockingLevel => _blockingLevel;

        public ConfigPuzzleInLevel(BoardPosition position, string puzzleId, int level, int blockingLevel)
        {
            _position = position;
            _puzzleId = puzzleId;
            _level = level;
            _blockingLevel = blockingLevel;
        }
    }

    [Serializable]
    public class ConfigLevelItem
    {
        [SerializeField] List<ConfigPuzzleInLevel> _items = new();
        public List<ConfigPuzzleInLevel> items => _items;

        [SerializeField] List<ConfigCustomerOrderItem> _customerOrders = new();
        public List<ConfigCustomerOrderItem> customerOrders => _customerOrders;

        [Tooltip("Spawn random customer after serving all of the customer's order")]
        [SerializeField] bool _spawnLoopCustomer;
        public bool spawnLoopCustomer => _spawnLoopCustomer;
    }

    [Serializable]
    public class ConfigCustomerOrderItem
    {

        [SerializeField] int _customerId;
        public int customerId => _customerId;

        [SerializeField] List<ConfigOrderItem> _orders = new();
        public List<ConfigOrderItem> orders => _orders;

        [SerializeField] List<ConfigRewardCurrencyItem> _currencyRewards = new();
        public List<ConfigRewardCurrencyItem> currencyRewards => _currencyRewards;

        public ConfigCustomerOrderItem(int customerId)
        {
            _customerId = customerId;
        }

    }

    [Serializable]
    public class ConfigOrderItem
    {
        [SerializeField] string _itemId;
        public string itemId => _itemId;

        [SerializeField] int _level;
        public int level => _level;

        public ConfigOrderItem(string itemId, int level)
        {
            _itemId = itemId;
            _level = level;
        }
    }
    

    [CreateAssetMenu(menuName = "Config/Level")]
    public class ConfigLevel : ScriptableObject
    {
        [SerializeField] List<ConfigLevelItem> _items;
        public List<ConfigLevelItem> items => _items;

        [SerializeField] int _maxAppearCustomer;
        public int maxAppearCustomer => _maxAppearCustomer;

#if UNITY_EDITOR

        [SimpleInspectorButton("Sample level")]
        void SampleLevel()
        {
            var newLevel = new ConfigLevelItem();
            var configPuzzle = UnityEditor.AssetDatabase.LoadAssetAtPath<ConfigPuzzle>("Assets/_TheKingOfMergeCity/Config/ConfigPuzzle.asset");
            var normalConfigPuzzles = configPuzzle.configItems.FindAll(c => c.puzzleType == PuzzleType.Normal);
            
            //Generate board
            var boardSize = configPuzzle.boardSize;
            int midRow = boardSize.y / 2;
            int midCol = boardSize.x / 2;
            for (int i = 0; i < boardSize.y; i++)
            {
                for (int j = 0; j < boardSize.x; j++)
                {
                    var randomConfig = normalConfigPuzzles[Random.Range(0, normalConfigPuzzles.Count)];
                    int puzzleLevel = Random.Range(0, randomConfig.configPerLevel.Count);
                    int blockingLevel = 0;
                    if (j == midCol && (i >= midRow - 1 && i <= midRow))
                    {
                        //Debug.Log($"({j}-{i})" );
                        puzzleLevel = 0;
                        blockingLevel = 2;
                    }
                    newLevel.items.Add(new ConfigPuzzleInLevel(new BoardPosition(j, i), randomConfig.id, puzzleLevel,blockingLevel));
                }
            }

            //Generate customer order //Todo: check for duplicate (itemId, level)
            int customerOrderCount = Random.Range(2, 6);
            for (int i = 0; i < customerOrderCount; i++)
            {
                var customerOrderItem = new ConfigCustomerOrderItem(Random.Range(0, 5));
                int orderCount = Random.Range(1, 4);
                customerOrderItem.currencyRewards.Add(new ConfigRewardCurrencyItem(CurrencyType.Star, 10 * orderCount));//Todo: How do we calculate the reward?
                for (int j = 0; j < orderCount; j++)
                {
                    var randomConfig = normalConfigPuzzles[Random.Range(0, normalConfigPuzzles.Count)];
                    customerOrderItem.orders.Add(new ConfigOrderItem(randomConfig.id, Random.Range(3, Mathf.Min(6, randomConfig.configPerLevel.Count-1))));
                }
                newLevel.customerOrders.Add(customerOrderItem);
            }

            items.Add(newLevel);
            
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);

        }

        [SerializeField] TextAsset csvFile;

        [SimpleInspectorButton("Import csv file")]
        void ImportCsvFile()
        {
            int startingRowIndex = 1;
            string[] rowStrs = csvFile.text.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            int currentLevel = int.Parse(rowStrs[startingRowIndex].Replace("\r", "").Split(",")[0]);
            items[currentLevel].customerOrders.Clear();

            for (int i = startingRowIndex; i < rowStrs.Length; i++)
            {
                string[] csvCols = rowStrs[i].Replace("\r", "").Split(",", StringSplitOptions.RemoveEmptyEntries);

                var level = int.Parse(csvCols[0]);

                var currentConfigLevel = items[currentLevel];

                var detail = new ConfigCustomerOrderItem(int.Parse(csvCols[2]));

                var ordersStr = csvCols[3].Split(";", StringSplitOptions.RemoveEmptyEntries);

                //Debug.Log("Customer order id: " + i + " Add new detail with customer model id:   " + detail.customerId);
                foreach (var orderStr in ordersStr)
                {
                    //Debug.Log("order str: " + orderStr);
                    var orderDetail = orderStr.Split(":");
                    detail.orders.Add(new ConfigOrderItem(orderDetail[0], int.Parse(orderDetail[1])));
                }

                detail.currencyRewards.Add(new ConfigRewardCurrencyItem(CurrencyType.Star, int.Parse(csvCols[4])));

                currentConfigLevel.customerOrders.Add(detail);

                if (level > currentLevel)
                {
                    //Debug.Log("Level: " + level);
                    currentLevel++;
                    items[currentLevel].customerOrders.Clear();
                }
            }

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        }
#endif

    }
}
