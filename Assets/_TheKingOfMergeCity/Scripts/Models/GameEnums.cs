using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKingOfMergeCity.Enum
{
    public enum GameState
    {
        Prepare  = 0,
        Playing = 1,
        GameWin = 2,
        GameOver = 3
    }

    public enum CurrencyType
    {
        None = 0,
        Coin = 1,
        Gem = 2,
        Exp = 3,
        Star = 4,
        Energy = 5
    }

    public enum RewardType
    {
        None = 0,
        PuzzleItem = 1,
        Currency =2,
    }

    public enum BlockingState
    {
        FullBlock = 1,
        HaflBlock = 2,
        Unblock = 3,
    }

    public enum PuzzleType
    {
        Normal = 0,
        Producer = 1
    }

    public enum DecoItemState
    {
        None = 0,
        Lock = 1,
        Unlock = 2,
        Completed = 3
    } 

    public enum MoveDirection
    {
        None = 0,
        X = 1,
        Y = 2,
        Z = 3
    }

    public enum ExternalItemRewardCondition
    {
        None = 0,
        ByAreaUnlocked = 1,
        ByDecoBuilt = 2,
        ByLevelUpgraded = 3,
        ByCurrencyCost = 4
    }

    public enum RewardClaimState
    {
        None = 0,
        PendingClaim = 1,
        Claimed = 2
    }

    public enum FeatureType
    {
        None = 0,
        DailyReward = 1,
        Roulette = 2,
        Shop = 3,
        SeasonPass = 4,

    }
}
