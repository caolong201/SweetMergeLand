using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using USimpFramework.EditorExtension;
using System;   

namespace TheKingOfMergeCity.Config
{
    using Enum;

    [Serializable]
    public class ConfigPlayerLevelItem
    {
        [SerializeField] int _exp;
        public int exp => _exp;

        [SerializeField] List<ConfigRewardCurrencyItem> _currencyRewards;
        public List<ConfigRewardCurrencyItem> currencyRewards => _currencyRewards;

        [SerializeField] List<string> _puzzleRewardIds;
        public List<string> puzzleRewards => _puzzleRewardIds;

        public ConfigPlayerLevelItem(int exp)
        {
            _exp = exp;
            _currencyRewards = new();
            _puzzleRewardIds = new();
        }

    }

    public class ConfigPlayerLevel : ScriptableObject
    {
        [SerializeField] List<ConfigPlayerLevelItem> _items;
        public IReadOnlyList<ConfigPlayerLevelItem> items => _items;

#if UNITY_EDITOR
        [SerializeField] TextAsset csvFile;

        [SimpleInspectorButton("Import from CSV")]
        void ImportFromCsv()
        {
            int startingIndex = 1;
            string[] rows = csvFile.text.Split("\n", StringSplitOptions.RemoveEmptyEntries);

            _items.Clear();

            for (int i = startingIndex; i < rows.Length; i++)
            {
                string[] cols = rows[i].Replace("\r", "").Split(",");
                var newItem = new ConfigPlayerLevelItem(int.Parse(cols[1]));

                if (cols[2].Length > 0)
                {
                    var currencyRewardsStr = cols[2].Split(";");
                    foreach (var currencyRewardStr in currencyRewardsStr)
                    {
                        var format = currencyRewardStr.Split(":");
                        newItem.currencyRewards.Add(new ConfigRewardCurrencyItem(System.Enum.Parse<CurrencyType>(format[0], true), int.Parse(format[1])));
                    }

                }

                if (cols[3].Length > 0)
                {
                    var externalPuzzleItemRewardsStr = cols[3].Split(";");
                    foreach (var externalStr in externalPuzzleItemRewardsStr)
                    {
                        newItem.puzzleRewards.Add(externalStr);
                    }
                }
                
                _items.Add(newItem);
            }

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        }

        
        
#endif
    }
}
