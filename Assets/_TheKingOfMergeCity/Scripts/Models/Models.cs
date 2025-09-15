using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace TheKingOfMergeCity.Model
{
    using Enum;
    using Config;
    using USimpFramework.Utility;
    using UnityEngine;

    [Serializable]
    public class UserCurrencyItem
    {
        [JsonProperty("currencyType")]
        public CurrencyType currencyType { get; private set; }

        [JsonProperty("balance")]
        public int balance { get;  set; }

        public UserCurrencyItem(CurrencyType currencyType, int balance)
        {
            this.currencyType = currencyType;
            this.balance = balance;
        }

    }
    
    [Serializable]
    public class UserFoodOrderDataItem
    {
        [JsonProperty("puzzleId")]
        public string puzzleId { get; private set; }

        [JsonProperty("level")]
        public int level { get; private set; }

        public UserFoodOrderDataItem(string puzzleId, int level)
        {
            this.puzzleId = puzzleId;
            this.level = level;
        }

    }

    [Serializable]
    public class UserCustomerDataItem
    {
        [JsonProperty("customerId")]
        public int customerId { get; private set; }

        [JsonProperty("orders")]
        public List<UserFoodOrderDataItem> orders { get; private set; } = new();

        [JsonProperty("isInQueue")]
        public bool isInQueue { get; set; } = false;

        [JsonProperty("rewards")]
        public List<ConfigRewardCurrencyItem> rewards { get; private set; } = new(); //abstract cannot deserialize

        public UserCustomerDataItem(int customerId)
        {
            this.customerId = customerId;
        }
    }
    
    [Serializable]
    public class UserPuzzleDataItem :UserPuzzleDataItemBase
    {
        [JsonProperty("blockingLevel")]
        public int blockingLevel { get; set; }

        [JsonProperty("boardPosition")]
        public BoardPosition boardPosition { get; set; }

        public UserPuzzleDataItem(string puzzleId, int level, int blockingLevel, BoardPosition boardPosition) : base(puzzleId, level)
        {
            this.blockingLevel = blockingLevel;
            this.boardPosition = boardPosition;
        }
    }

    [Serializable]
    public class UserPuzzleDataItemBase
    {
        [JsonProperty("puzzleId")]
        public string puzzleId { get; protected set; }

        [JsonProperty("level")]
        public int level { get; set; }

        public UserPuzzleDataItemBase(string puzzleId, int level)
        {
            this.puzzleId = puzzleId;
            this.level = level;
        }
    }


    [Serializable]
    public class UserBoardData
    {
        [JsonProperty("level")]
        public int level { get; private set; }

        [JsonProperty("puzzleDatas")]
        public List<UserPuzzleDataItem> puzzleDatas { get; private set; } = new();

        [JsonProperty("customerDatas")]
        public List<UserCustomerDataItem> customerDatas { get; private set; } = new();
    }

    [Serializable]
    public class UserAreaData
    {
        [JsonProperty("areaId")]
        public int areaId { get; private set; }

        [JsonProperty("completedDecoIds")]
        public List<string> completedDecoIds { get; private set; } = new();

        public UserAreaData(int areaId)
        {
            this.areaId = areaId;
        }
    }

    [Serializable]
    public class UserPuzzleInventoryDataItem : UserPuzzleDataItemBase
    {
        [JsonProperty("slotId")]
        public int slotId { get; private set; }

        public UserPuzzleInventoryDataItem(string puzzleId, int puzzleLevel, int slotId) : base(puzzleId, puzzleLevel)
        {
            this.slotId = slotId;
        }

        public void SetPuzzle(string puzzleId, int puzzleLevel)
        {
            this.puzzleId = puzzleId;
            this.level = puzzleLevel;
        }
    }

    [Serializable]
    public class UserDailyReward
    {
        [JsonProperty("lastClaimMs")]
        public long lastClaimMs { get; set; }

        [JsonProperty("currentClaimIndex")]
        public int currentClaimIndex { get; set; }

        [JsonProperty("currentDailyRewardPackId")]
        public int currentDailyRewardPackId { get; set; }
    }

 
    [Serializable]
    public class UserRoulette : ConsumableItemCooldown
    {
        [JsonProperty("tsNextTimeResetPuzzleId")]
        public long tsNextTimeResetPuzzleId { get; set; }

        [JsonProperty("configRewards")]
        public List<ConfigRouletteReward> configRewards { get; private set; }

        public UserRoulette(float maxValue, int resetCooldownSeconds) : base(maxValue, resetCooldownSeconds)
        {
            configRewards = new();
        }
    }

    [Serializable]
    public class ConsumableItemCooldown
    {
        public event Action<ConsumableItemCooldown, float> onValueChanged;

        //Reset cooldown in seconds
        [JsonProperty("resetCooldownSecond")]
        public float resetCooldownSecond { get; set; }

        [JsonProperty("maxValue")]
        public float maxValue { get; set; }

        [JsonProperty("currentValue")]
        public float currentValue { get; set; }

        [JsonProperty("tsNextTimeReset")]
        public long tsNextTimeReset { get; set; }

        [JsonIgnore] public bool isCooldown => currentValue == 0;

        public ConsumableItemCooldown(float maxValue, int resetCooldownSecond)
        {
            currentValue = maxValue;
            this.maxValue = maxValue;
            this.resetCooldownSecond = resetCooldownSecond;
        }

        public void Reset(float maxValue, int resetCooldownSecond)
        {
            var oldValue = currentValue;
            currentValue = maxValue;
            this.maxValue = maxValue;
            this.resetCooldownSecond = resetCooldownSecond;
            onValueChanged?.Invoke(this, oldValue);
        }

        public bool CheckReset(float maxValue, int resetCooldownSecond)
        {
            if (!isCooldown)
                return false;

            if (TimeUtils.GetServerUtcNowMs() >= tsNextTimeReset)
            {
                Reset(maxValue, resetCooldownSecond);
                return true;
            }

            return false;
        }

        public bool AddValue(int amount)
        {
            //Check changed value
            var newValue = currentValue + amount;
            newValue = Mathf.Clamp(newValue, 0, maxValue);
            if (Mathf.Approximately(newValue, currentValue))
                return false;

            var oldValue = currentValue;
            currentValue = newValue;

            if (currentValue == 0)
                tsNextTimeReset = TimeUtils.GetServerUtcNowMs() + (long)resetCooldownSecond * 1000;

            onValueChanged?.Invoke(this, oldValue);
            return true;
        }

        public long GetRemainingCooldownResetMs()
        {
            return tsNextTimeReset - TimeUtils.GetServerUtcNowMs();
        }
    }

    [Serializable]
    public struct BoardPosition
    {
        public int x;
        public int y;

        public BoardPosition(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString()
        {
            return $"({y},{x})";
        }

        
    }
}
