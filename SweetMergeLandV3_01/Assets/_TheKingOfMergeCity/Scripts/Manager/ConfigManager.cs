using UnityEngine;
using USimpFramework.Utility;

namespace TheKingOfMergeCity
{
    using Config;
 
    public sealed class ConfigManager : SimpleSingleton<ConfigManager>
    {
        [SerializeField] ConfigPuzzle _configPuzzle;
        public ConfigPuzzle configPuzzle => _configPuzzle;

        [SerializeField] ConfigLevel _configLevel;
        public ConfigLevel configLevel => _configLevel;

        [SerializeField] ConfigCharacter _configCharacter;
        public ConfigCharacter configCharacter => _configCharacter;

        [SerializeField] ConfigCurrency _configCurrency;
        public ConfigCurrency configCurrency => _configCurrency;

        [SerializeField] ConfigGlobal _configGlobal;
        public ConfigGlobal configGlobal => _configGlobal;

        [SerializeField] ConfigArea _configArea;
        public ConfigArea configArea => _configArea;

        [SerializeField] ConfigTutorial _configTutorial;
        public ConfigTutorial configTutorial => _configTutorial;

        [SerializeField] ConfigExternalRewardPuzzle _configExternalRewardPuzzle;
        public ConfigExternalRewardPuzzle configExternalRewardPuzzle => _configExternalRewardPuzzle;

        [SerializeField] ConfigPlayerLevel _configPlayerLevel;
        public ConfigPlayerLevel configPlayerLevel => _configPlayerLevel;

        [SerializeField] ConfigPuzzleInventory _configPuzzleInventory;
        public ConfigPuzzleInventory configPuzzleInventory => _configPuzzleInventory;

        [SerializeField] ConfigDailyReward _configDailyReward;
        public ConfigDailyReward configDailyReward => _configDailyReward;

        [SerializeField] ConfigRoulette _configRoulette;
        public ConfigRoulette configRoulette => _configRoulette;

        [SerializeField] ConfigFeature _configFeature;
        public ConfigFeature configFeature => _configFeature;
    }
}
