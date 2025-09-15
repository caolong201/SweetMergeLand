using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKingOfMergeCity.Config
{
    using Enum;

    public class ConfigDailyReward : ScriptableObject
    {
        [SerializeField] int _unlockAtPlayerLevel;
        public int unlockAtPlayerLevel => _unlockAtPlayerLevel;

        [SerializeField] List<ConfigDailyRewardItemPack> _dailyRewardItems;
        public List<ConfigDailyRewardItemPack> dailyRewardItems => _dailyRewardItems;

        
    }
}
