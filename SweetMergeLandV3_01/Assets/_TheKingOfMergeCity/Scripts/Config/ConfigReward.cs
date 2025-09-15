using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace TheKingOfMergeCity.Config
{
    using Enum;


    [Serializable]
    public class ConfigRewardItemBase
    {
        [JsonProperty("amount")]
        [SerializeField] protected int _amount;
        [JsonIgnore] public int amount => _amount;

        public ConfigRewardItemBase(int amount)
        {
            _amount = amount;
        }
    }

    [Serializable]
    public class ConfigRewardCurrencyItem : ConfigRewardItemBase
    {
        [JsonProperty("currencyType")]
        [SerializeField] CurrencyType _currencyType;
        [JsonIgnore] public CurrencyType currencyType => _currencyType;

        public ConfigRewardCurrencyItem(CurrencyType currencyType, int amount ) : base(amount)
        {
            _currencyType = currencyType;
        }
    }

    [Serializable]
    public class ConfigRewardItem
    {
        [JsonProperty("rewardType")]
        [SerializeField] RewardType _rewardType;
        [JsonIgnore] public RewardType rewardType => _rewardType;

        [Header("RewardType/Currency")]
        [JsonProperty("currencyType")]
        [SerializeField] CurrencyType _currencyType;
        [JsonIgnore] public CurrencyType currencyType => _currencyType;

        [Header("RewardType/Puzzle Item")]
        [JsonProperty("puzzleId")]
        [SerializeField] string _puzzleId;
        [JsonIgnore] public string puzzleId => _puzzleId;

        [JsonProperty("puzzleLevel")]
        [SerializeField] int _puzzleLevel;
        [JsonIgnore] public int puzzleLevel => _puzzleLevel;

        [JsonProperty("amount")]
        [SerializeField] int _amount;
        [JsonIgnore] public int amount => _amount;

        public ConfigRewardItem()
        {

        }

        public ConfigRewardItem(RewardType rewardType, CurrencyType currencyType, int amount)
        {
            _rewardType = rewardType;
            _currencyType = currencyType;
            _amount = amount;
        }

        public ConfigRewardItem(RewardType rewardType, string puzzleId, int puzzleLevel, int amount)
        {
            _rewardType = rewardType;
            _puzzleId = puzzleId;
            _puzzleLevel = puzzleLevel;
            _amount = amount;
        }

       [JsonIgnore] public Sprite iconSprite
        {
            get
            {
                if (rewardType == RewardType.Currency)
                {
                    return ConfigManager.Instance.configCurrency.items.Find(c => c.type == currencyType)?.iconSprite;
                }

                if (rewardType == RewardType.PuzzleItem)
                {
                    var configItems = ConfigManager.Instance.configPuzzle.configItems.Find(c => c.id == puzzleId);
                    if (configItems == null)
                        return null;

                    return configItems.configPerLevel[puzzleLevel].itemSprite;
                }

                return null;
            }
             
        }
    }
}
