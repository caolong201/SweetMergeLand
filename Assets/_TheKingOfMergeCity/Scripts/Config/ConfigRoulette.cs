using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace TheKingOfMergeCity.Config
{
    using Enum;

    [System.Serializable]
    public class ConfigRouletteReward : ConfigRewardItem
    {
        [JsonProperty("weight")]
        [SerializeField] int _weight;
        [JsonIgnore] public int weight => _weight;

        [JsonProperty("isRandomPuzzleId")]
        [SerializeField] bool _isRandomPuzzleId;
        [JsonIgnore] public bool isRandomPuzzleId => _isRandomPuzzleId; 

        public ConfigRouletteReward()
        {

        }

        public ConfigRouletteReward(RewardType rewardType, CurrencyType currencyType, int amount, int weight) : base(rewardType, currencyType, amount)
        {
            _weight = weight;
        }

        public ConfigRouletteReward(RewardType rewardType, string puzzleId, int level, int amount, int weight) : base(rewardType, puzzleId, level, amount)
        {
            _weight = weight;
        }
    }


    public class ConfigRoulette : ScriptableObject
    {
        [SerializeField] int _unlockAtPlayerLevel;
        public int unlockAtPlayerLevel => _unlockAtPlayerLevel;

        [SerializeField] List<ConfigRouletteReward> _rewards;
        public List<ConfigRouletteReward> rewards => _rewards;

        [Tooltip("Watch ad spin count")]
        [SerializeField] int _watchAdSpinCount;
        public int watchAdSpinCount => _watchAdSpinCount;

        [Tooltip("Spin reset in seconds")]
        [SerializeField] int _resetCooldownSecond;
        public int resetCooldownSecond => _resetCooldownSecond;

        [SerializeField] int _puzzleIdResetCooldownSeconds = 60;
        public int puzzleIdResetCooldownSeconds => _puzzleIdResetCooldownSeconds;

        [Header("Animation")]
        [SerializeField] Vector2Int _spinCountRange;
        public Vector2Int spinCountRange => _spinCountRange;

        [SerializeField] float _angularSpeed = 30f;
        public float angularSpeed => _angularSpeed;

        [SerializeField] float _rotateHandle = -16f;
        public float rotateHandle => _rotateHandle;

        [SerializeField] float _handleRotateSpeed = 30;
        public float handleRotateSpeed => _handleRotateSpeed;

        [SerializeField] float _acceleration;
        public float acceleration => _acceleration;

        [SerializeField] float _angleFactor = 10;
        public float angleFactor => _angleFactor;
    }
}
