using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace TheKingOfMergeCity.Config
{
    using Enum;

    [Serializable]
    public class ConfigExtenalPuzzleItem
    {
        [SerializeField] string _puzzleId;
        public string puzzleId => _puzzleId;

        [Tooltip("Level of puzzle, starting from 0")]
        [SerializeField] int _level;
        public int level => _level;

        [SerializeField] ExternalItemRewardSetting _externalItemRewardSetting;
        public ExternalItemRewardSetting externalItemRewardSetting => _externalItemRewardSetting;
    }

    [Serializable]
    public class ExternalItemRewardSetting
    {
        [SerializeField] ExternalItemRewardCondition _rewardConditionType;
        public ExternalItemRewardCondition rewardConditionType => _rewardConditionType;

        [Header("By Area Unlocked")]
        [SerializeField] int _areaId; //Also using for Deco
        public int areaId => _areaId;

        [Header("By Deco Built")]
        [SerializeField] string _decoId;
        public string decoId => _decoId;

        [Header("By Level Upgraded")]
        [SerializeField] int _rewardedAtLevel;
        public int rewardedAtLevel => _rewardedAtLevel;

        [Header("By Currency cost")]
        [SerializeField] CurrencyType _currencyType;
        public CurrencyType currencyType => _currencyType;

        [SerializeField] int _costAmount;
        public int costAmout => _costAmount;
    }

    public class ConfigExternalRewardPuzzle : ScriptableObject
    {
        [SerializeField] List<ConfigExtenalPuzzleItem> _items;
        public IReadOnlyList<ConfigExtenalPuzzleItem> readonlyItems => _items;

        public List<ConfigExtenalPuzzleItem> GetConfigExternalRewardPuzzles(ExternalItemRewardCondition condition)
        {
            return _items.FindAll(s => s.externalItemRewardSetting. rewardConditionType == condition);
        }

        public List<ConfigExtenalPuzzleItem> GetConfigExternalRewardByUnlockedArea(int areaId)
        {
            return _items.FindAll(s => s.externalItemRewardSetting.rewardConditionType == ExternalItemRewardCondition.ByAreaUnlocked && s.externalItemRewardSetting.areaId == areaId);
        }

    }
}
