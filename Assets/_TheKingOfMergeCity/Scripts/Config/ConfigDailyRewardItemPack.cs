using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKingOfMergeCity.Config
{
    using Enum;
    
    [System.Serializable]
    public class ConfigDailyRewardItem: ConfigRewardItem
    {
        public ConfigDailyRewardItem(RewardType rewardType, CurrencyType currencyType, int amount) : base(rewardType, currencyType, amount)
        {
            
        }
        
        [JsonProperty("isSpecial")]
        [SerializeField] bool _isSpecial;
        [JsonIgnore] public bool isSpecial => _isSpecial;
    }

    [CreateAssetMenu(menuName = "Config/Daily Reward")]
    public class ConfigDailyRewardItemPack : ScriptableObject
    {
        [SerializeField] List<ConfigDailyRewardItem> _rewards;
        public List<ConfigDailyRewardItem> rewards => _rewards;
    }
}
