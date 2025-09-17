using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;
using USimpFramework.USimpType;
using USimpFramework.Utility;

using Random = UnityEngine.Random;

namespace TheKingOfMergeCity
{
    using Model;
    using Enum;
    using Config;
    using System.Linq;

    public sealed class UserManager : SimpleSingleton<UserManager>
    {
        public event Action<CurrencyType> onCurrencyBalanceChanged;
        public event Action<int, string> onDecoCanBuild;
        public event Action onPlayerLevelUp;
        public event Action onDailyRewardClaimed;


        public event Action onInventoryChanged;
        public int currentBoardLevel { get; private set; } = 0;

        public PlayerPrefsTypeCollection<UserCurrencyItem> userCurrencies { get; private set; } = new("userCurrencies");

        public PlayerPrefsType<UserBoardData> boardData { get; private set; } = new("boardData");

        public PlayerPrefsType<bool> finishTutorial { get; private set; } = new("finishTutorial");

        public PlayerPrefsType<long> lastTimeActiveMs { get; private set; } = new("lastTimeActiveMs");

        public PlayerPrefsType<int> currentSelectAreaId { get; private set; } = new(0, "currentSelectAreaId");

        public PlayerPrefsTypeCollection<UserAreaData> areaDatas { get; private set; } = new("areaDatas");

        public PlayerPrefsTypeCollection<UserPuzzleDataItemBase> externalPuzzleItems { get; private set; } = new("externalPuzzleItems");

        /// <summary> This only store the unlocked slot </summary>
        public PlayerPrefsTypeCollection<UserPuzzleInventoryDataItem> puzzleInventoryItems { get; private set; } = new("puzzleInventoryItems");

        public PlayerPrefsType<UserDailyReward> dailyRewardData { get; private set; } = new(new UserDailyReward()
        {
            currentClaimIndex = -1
        }, "dailyRewardData");

        public PlayerPrefsType<UserRoulette> rouletteData { get; private set; }

        public long nextTimeAddEnergyMs { get; private set; }

        public UserAreaData currentSelectAreaData { get; private set; }

        public int currentPlayerLevel { get; private set; }

        public bool hasPendingClaimReward { get; private set; }

        public bool isMaxLevel { get; private set; }
        
        /// <summary>Must check this if you want to perform animation before the value is updated  </summary>
        public bool isPlayingDecoBuildingFromInGameScene { get; set; }
        
        /// <summary>Must check this if you want to perform the player leveling up animation before the value is updated</summary>
        public bool isPendingPlayPlayerLevelUp { get; set; }
        
        public int oldEnergy { get; private set; }
        public int oldLevel { get; private set; }
        public int oldExp { get; private set; }

        public bool isInitialized { get; private set; }

        long nextTimeActiveMs;
        bool startRegenEnergy;

        IgnoreSaveDataToken ignoreWhenHasNotFinishTutorial;


        void NotifyInventoryChanged()
        {
            try
            {
                onInventoryChanged?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError("Error while invoking onInventoryChanged: " + ex);
            }
        }

            public void Init()
        {
            //Just for testing
            currentBoardLevel = 4;

            finishTutorial.LoadData();
                
            puzzleInventoryItems.LoadData();

            if (ConfigManager.Instance.configGlobal.skipTutorial)
                finishTutorial.value = true;

            ignoreWhenHasNotFinishTutorial = new IgnoreSaveDataToken()
            {
                isIgnore = !finishTutorial
            };

            var configManager = ConfigManager.Instance;

            //Init currencies
            userCurrencies.SetIgnoreSaveDataToken(ignoreWhenHasNotFinishTutorial);
            userCurrencies.LoadData();

            if (userCurrencies.Count == 0)
            {
                var configGlobal = configManager.configGlobal;
                userCurrencies.Add(new UserCurrencyItem(CurrencyType.Energy, configGlobal.startingEnergy));
                userCurrencies.SaveData();
            }

            lastTimeActiveMs.SetIgnoreSaveDataToken(ignoreWhenHasNotFinishTutorial);
            lastTimeActiveMs.LoadData();

            //Init area datas
            areaDatas.SetIgnoreSaveDataToken(ignoreWhenHasNotFinishTutorial);
            areaDatas.LoadData();

            if (areaDatas.Count == 0)
            {
                areaDatas.Add(new UserAreaData(0));
                areaDatas.SaveData();
            }

            currentSelectAreaId.SetIgnoreSaveDataToken(ignoreWhenHasNotFinishTutorial);
            currentSelectAreaId.LoadData();
            currentSelectAreaData = areaDatas.Find(u => u.areaId == currentSelectAreaId);

            //Load board data
            boardData.SetIgnoreSaveDataToken(ignoreWhenHasNotFinishTutorial);
            boardData.LoadData();
            if (boardData.value.puzzleDatas.Count == 0)
            {
                //Generate new data
                var configCurrentLevel = configManager.configLevel.items[currentBoardLevel];

                foreach (var config in configCurrentLevel.items)
                {
                    AddNewPuzzle(config.puzzleId, config.level, config.blockingLevel, config.position, false);
                }

                foreach (var config in configCurrentLevel.customerOrders)
                {
                    var data = AddNewCustomer(config, false);
                }

                if (!finishTutorial)
                {
                    boardData.value.customerDatas[0].isInQueue = true;
                }

                boardData.SaveData();
            }

            externalPuzzleItems.SetIgnoreSaveDataToken(ignoreWhenHasNotFinishTutorial);
            externalPuzzleItems.LoadData();

            //Should be placed in boost manager
            int totalEnergyRegenAmount = Mathf.RoundToInt((1.0f * TimeUtils.GetServerUtcNowMs() - lastTimeActiveMs) / 1000 / ConfigManager.Instance.configGlobal.energyRegenInterval);
            if (GetCurrencyBalance(CurrencyType.Energy) < ConfigManager.Instance.configGlobal.maxEnergy)
            {
                AddCurrencyAmount(CurrencyType.Energy, totalEnergyRegenAmount, true, true, false);
            }

            oldExp = GetCurrencyBalance(CurrencyType.Exp);
            currentPlayerLevel = GetPlayerLevelByExp(oldExp, out var maxLevel);
            oldLevel = currentPlayerLevel;
            isMaxLevel = maxLevel;

            //Init inventory slots
            puzzleInventoryItems.LoadData();
            var configPuzzleInventory = configManager.configPuzzleInventory;
            var unlockedSlots = configPuzzleInventory.configSlots.FindAll(c => c.unlockCost <= 0);

            if (puzzleInventoryItems.Count < unlockedSlots.Count) //First init the game
            {
                while (puzzleInventoryItems.Count < unlockedSlots.Count)
                {
                    AddPuzzleInventorySlot(puzzleInventoryItems.Count, "", -1, false);
                }

                puzzleInventoryItems.SaveData();


                NotifyInventoryChanged();
            }

            InitDailyReward();
            InitRoulette();

            isInitialized = true;
        }
        
        void Update()
        {
            if (!isInitialized)
                return;

            //Save last active Ms
            var currentTimeMs = TimeUtils.GetServerUtcNowMs();

            if (currentTimeMs >= nextTimeActiveMs)
            {
                lastTimeActiveMs.SetValueWithoutNotify(currentTimeMs);
                lastTimeActiveMs.SaveData();

                nextTimeActiveMs = currentTimeMs + 60 * 1000;
            }

            //Regen energy
            if (startRegenEnergy)
            {
                if (currentTimeMs >= nextTimeAddEnergyMs)
                {
                    AddCurrencyAmount(CurrencyType.Energy, 1, true, true, false);
                    nextTimeAddEnergyMs = currentTimeMs + (long)ConfigManager.Instance.configGlobal.energyRegenInterval * 1000;
                }
            }

            //Check reset roulette spin (DO NOT save data here)
            var configRoulette = ConfigManager.Instance.configRoulette;
            rouletteData.value.CheckReset(configRoulette.watchAdSpinCount, configRoulette.resetCooldownSecond);
            if (Input.GetKeyDown(KeyCode.R))
            {
                AddCurrencyAmount(CurrencyType.Gem, 100, true, true);
      
            }
        }

        public void SaveAll()
        {
            finishTutorial.value = true;
            finishTutorial.SaveData();

            ignoreWhenHasNotFinishTutorial.isIgnore = false;

            userCurrencies.SaveData();
            boardData.SaveData();
            areaDatas.SaveData();
            lastTimeActiveMs.SaveData();
            currentSelectAreaId.SaveData();
            externalPuzzleItems.SaveData();
            rouletteData.SaveData();
        }


        #region Player Api
        public int GetPlayerLevelByExp(int exp, out bool isMaxLevel)
        {
            isMaxLevel = false;

            if (exp <= 0)
                return 1;

            var configPlayerLevel = ConfigManager.Instance.configPlayerLevel;
            for (int i = 2; i < configPlayerLevel.items.Count; i++)
            {
                if (exp < configPlayerLevel.items[i].exp)
                {
                    return i - 1;
                }
            }

            isMaxLevel = true;
            return configPlayerLevel.items.Count - 1;
        }

        public int GetExpByLevel(int level, out bool isMaxLevel)
        {
            isMaxLevel = false;
            if (level < 0)
                return 0;

            var configPlayerLevel = ConfigManager.Instance.configPlayerLevel;

            if (level >= configPlayerLevel.items.Count - 1)
            {
                isMaxLevel = true;
                return configPlayerLevel.items[^1].exp;
            }

            return configPlayerLevel.items[level].exp;
        }

        #endregion

        #region Area Api

        /// <summary>
        /// Check if any decoration in any area scenes can be built? 
        /// </summary>
        /// <param name="areaId">The area id, -1 if is invalid or if all deco item in that area is built</param>
        /// <param name="decoId">The building deco id in that area, empty if is invalid</param>
        public void CheckCanBuildDeco(out int areaId, out string decoId)
        {
            areaId = -1;
            decoId = "";

            //Get current active area data
            if (areaDatas.Count == 0)
                return;

            var lastArea = areaDatas.dataCollection[^1]; //Todo:Should be the current selected area?

            var configArea = ConfigManager.Instance.configArea.areaItems.Find(a => a.id == lastArea.areaId);
            if (configArea == null)
                throw new UnityException("Check deco can build failed! Invalid Area: " + lastArea.areaId);

            if (lastArea.completedDecoIds.Count == configArea.decoItems.Count)//All the deco of this area has been built
                return;

            //Get next deco item need to build
            var configDecoItem = configArea.decoItems[lastArea.completedDecoIds.Count];
            var coinStar = GetCurrencyBalance(CurrencyType.Star);

            //Check if this item able to build
            if (coinStar >= configDecoItem.buildingCost)
            {
                areaId = lastArea.areaId;
                decoId = configDecoItem.id;
                onDecoCanBuild?.Invoke(areaId, decoId);
            }
        }

        public ConfigDecoItem GetNextUnlockConfigDeco(int areaId, out bool isAreaCompleted)
        {
            isAreaCompleted = false;
            if (areaId < 0)
            {
                Debug.LogError("Invalid area id: " + areaId);
                return null;
            }

            var configAreaItem = ConfigManager.Instance.configArea.areaItems.Find(a => a.id == areaId);
            if (configAreaItem == null)
            {
                Debug.LogError("Invalid area config with id: " + areaId);
                return null;
            }

            var areaData = areaDatas.Find(a => a.areaId == areaId);
            if (areaData == null) //Invalid area data
            {
                Debug.LogError("Invalid area data with id: " + areaId);
                return null;
            }

            if (areaData.completedDecoIds.Count == configAreaItem.decoItems.Count)
            {
                isAreaCompleted = true;
                return null;
            }

            return configAreaItem.decoItems[areaData.completedDecoIds.Count];
        }


        public void SetCurrentSelectAreaId(int areaId)
        {
            if (currentSelectAreaId == areaId)
                return;

            currentSelectAreaId.value = areaId;
            currentSelectAreaId.SaveData();
            currentSelectAreaData = areaDatas.Find(s => s.areaId == areaId);
        }

        public void BuildDecoration(int areaId, string decorationId, out bool isSuccess)
        {
            isSuccess = false;
            int starCoin = GetCurrencyBalance(CurrencyType.Star);
            var configManager = ConfigManager.Instance;
            var configArea = configManager.configArea.areaItems.Find(c => c.id == areaId);
            var configDeco = configArea.decoItems.Find(c => c.id == decorationId);

            if (starCoin < configDeco.buildingCost)
                return;

            //Consume star
            AddCurrencyAmount(CurrencyType.Star, -configDeco.buildingCost, false);

            //Add energy
            AddCurrencyAmount(CurrencyType.Energy, ConfigManager.Instance.configGlobal.energyRewardAfterBuild, false, allowOverMax: true);

            //Add exp
            AddCurrencyAmount(CurrencyType.Exp, configDeco.expReward, false);

            currentSelectAreaData.completedDecoIds.Add(decorationId);
            areaDatas.SaveData();
            userCurrencies.SaveData();

            //Check if it's last deco
            int index = configArea.decoItems.FindIndex(d => d.id == decorationId);
            if (index == configArea.decoItems.Count - 1)
            {
                if (configArea.id < ConfigManager.Instance.configArea.areaItems.Count - 1)
                {
                    UnlockArea(configArea.id + 1);
                }
            }

            //Check if has any deco build condition
            var decoBuildRewardPuzzleItems = configManager.configExternalRewardPuzzle.GetConfigExternalRewardPuzzles(ExternalItemRewardCondition.ByDecoBuilt);
            if (decoBuildRewardPuzzleItems.Count > 0)
            {
                foreach (var configItem in decoBuildRewardPuzzleItems)
                {
                    AddExternalRewardPuzzle(configItem.puzzleId, configItem.level);
                }
            }

            isSuccess = true;
        }

        public void UnlockArea(int areaId)
        {
            if (areaDatas.dataCollection.Exists(u => u.areaId == areaId)) //This area has been unlocked
                return;

            areaDatas.Add(new UserAreaData(areaId));
            areaDatas.SaveData();


            //Check if has any unlocked area condition
            var areaUnlockedRewardPuzleItems = ConfigManager.Instance.configExternalRewardPuzzle.GetConfigExternalRewardByUnlockedArea(areaId);
            if (areaUnlockedRewardPuzleItems.Count > 0)
            {
                foreach (var configItem in areaUnlockedRewardPuzleItems)
                {
                    AddExternalRewardPuzzle(configItem.puzzleId, configItem.level);
                }
            }

        }


        #endregion

        #region Currency Api

        public void AddCurrencyAmount(CurrencyType currencyType, int amount, bool autoSave, bool notifyOnChanged = false, bool allowOverMax = true)
        {
            if (currencyType == CurrencyType.Exp && isMaxLevel)
                return;

            var userCurrency = userCurrencies.Find(u => u.currencyType == currencyType);
            if (userCurrency == null)
            {
                userCurrency = new(currencyType, 0);
                userCurrencies.Add(userCurrency);
            }
            if (currencyType == CurrencyType.Energy)
            {
                oldEnergy = userCurrency.balance;
            }

            if (currencyType == CurrencyType.Exp)
            {
                oldExp = userCurrency.balance;
            }

            userCurrency.balance = Mathf.Max(userCurrency.balance + amount, 0);

            if (currencyType == CurrencyType.Energy)
            {
                var configGlobal = ConfigManager.Instance.configGlobal;

                if (!startRegenEnergy)
                {
                    if (userCurrency.balance < configGlobal.maxEnergy)
                    {
                        //Start regen energy
                        startRegenEnergy = true;
                        nextTimeAddEnergyMs = TimeUtils.GetServerUtcNowMs() + (long)configGlobal.energyRegenInterval * 1000;
                    }
                    else
                    {
                        //Claim max energy (if not allow)

                        if (allowOverMax)
                        {

                        }
                        else
                        {
                            Debug.Log("Claim max energy!");
                            userCurrency.balance = configGlobal.maxEnergy;
                        }
                        startRegenEnergy = false;
                    }
                }
            }
            else if (currencyType == CurrencyType.Exp)
            {
                var configPlayerLevel = ConfigManager.Instance.configPlayerLevel;
                if (userCurrency.balance >= configPlayerLevel.items[currentPlayerLevel].exp)
                {
                    oldLevel = currentPlayerLevel;
                    currentPlayerLevel = GetPlayerLevelByExp(userCurrency.balance, out bool maxLevel);
                    isMaxLevel = maxLevel;
                    if (notifyOnChanged)
                    {
                        onPlayerLevelUp?.Invoke();
                    }
                }
            }

            if (autoSave)
                userCurrencies.SaveData();

            if (notifyOnChanged)
            {
                onCurrencyBalanceChanged?.Invoke(currencyType);
            }
        }


        public void ClaimRewards(List<ConfigRewardItem> rewards)
        {
            bool needSaveCurrencies = false;
            bool needSaveExternalPuzzle = false;

            foreach (var reward in rewards)
            {
                if (reward.rewardType == RewardType.Currency)
                {
                    AddCurrencyAmount(reward.currencyType, reward.amount, false);
                    needSaveCurrencies = true;
                }

                if (reward.rewardType == RewardType.PuzzleItem)
                {
                    AddExternalRewardPuzzle(reward.puzzleId, reward.puzzleLevel, false);
                    needSaveExternalPuzzle = true;
                }
            }

            if (needSaveCurrencies)
            {
                userCurrencies.SaveData();
            }

            if (needSaveExternalPuzzle)
                externalPuzzleItems.SaveData();
        }

        public int GetCurrencyBalance(CurrencyType currencyType)
        {
            var userCurrency = userCurrencies.Find(u => u.currencyType == currencyType);
            return userCurrency == null ? 0 : userCurrency.balance;
        }
        #endregion

        #region Customer Api
        public UserCustomerDataItem AddNewCustomer(ConfigCustomerOrderItem config, bool autoSave = true)
        {
            int id = boardData.value.customerDatas.Count;
            var result = new UserCustomerDataItem(config.customerId);
            foreach (var configOrder in config.orders)
            {
                result.orders.Add(new UserFoodOrderDataItem(configOrder.itemId, configOrder.level));
            }

            result.rewards.AddRange(config.currencyRewards);

            boardData.value.customerDatas.Add(result);

            if (autoSave)
                boardData.SaveData();

            return result;
        }

        public void RemoveServingCustomer(int customerId, bool autoSave = true)
        {

            var customerData = boardData.value.customerDatas.Find(s => s.customerId == customerId);
            if (customerData == null)
                throw new UnityException($"Remove customer failed! Customer id {customerId} is not exists!");

            boardData.value.customerDatas.Remove(customerData);
            if (autoSave)
                boardData.SaveData();
        }


        #endregion

        #region Puzzle Api
        public UserPuzzleDataItem AddNewPuzzle(string puzzleId, int level, int blockingLevel, BoardPosition boardPosition, bool autoSave = true)
        {
            var result = new UserPuzzleDataItem(puzzleId, level, blockingLevel, boardPosition);
            boardData.value.puzzleDatas.Add(result);

            if (autoSave)
                boardData.SaveData();

            return result;
        }

        UserPuzzleDataItemBase AddExternalRewardPuzzle(string puzzleId, int level, bool autoSave = true)
        {
            var result = new UserPuzzleDataItemBase(puzzleId, level);
            externalPuzzleItems.Add(result);
            if (autoSave)
                externalPuzzleItems.SaveData();
            return result;
        }

        public void RemoveExternalRewardPuzzle(string puzzleId, bool autoSave = true)
        {
            var data = externalPuzzleItems.Find(p => p.puzzleId == puzzleId);
            if (data == null)
                throw new UnityException($"Remove puzzle failed! Invalid id {puzzleId}");

            externalPuzzleItems.Remove(data);

            if (autoSave)
                externalPuzzleItems.SaveData();
        }

        public void RemovePuzzleAt(BoardPosition boardPosition, bool autoSave = true)
        {
            var puzzleData = boardData.value.puzzleDatas.Find(d => (d.boardPosition.x == boardPosition.x) && (d.boardPosition.y == boardPosition.y));
            if (puzzleData == null)
            {
                Debug.Log("Remove puzzle failed! Cannot have puzzle at" + boardPosition);
                return;
            }

            boardData.value.puzzleDatas.Remove(puzzleData);
            if (autoSave)
                boardData.SaveData();
        }


        #endregion

        #region Inventory Puzzle Api
        void AddPuzzleInventorySlot(int slotId, string puzzleId, int puzzleLevel, bool autoSave)
        {
            var data = new UserPuzzleInventoryDataItem(puzzleId, puzzleLevel, slotId);
            puzzleInventoryItems.Add(data);

            if (autoSave)
                puzzleInventoryItems.SaveData();
        }

        public void EquipPuzzleInventorySlot(string puzzleId, int puzzleLevel, out bool isSuccess)
        {
            isSuccess = false;

            var configPuzzleInventory = ConfigManager.Instance.configPuzzleInventory;

            if (puzzleInventoryItems.Count == configPuzzleInventory.configSlots.Count)
                return;

            //Find an empty slot
            var emptySlot = puzzleInventoryItems.Find(c => string.IsNullOrEmpty(c.puzzleId));

            if (emptySlot == null)
                return;

            emptySlot.SetPuzzle(puzzleId, puzzleLevel);
            puzzleInventoryItems.SaveData();
            NotifyInventoryChanged();
            isSuccess = true;
        }

        public void UnEquipPuzzleInventorySlot(int slotId)
        {
            var slot = puzzleInventoryItems.Find(c => c.slotId == slotId);

            if (slot == null)
                return;


            slot.SetPuzzle("", -1);

            NotifyInventoryChanged();

            puzzleInventoryItems.SaveData();
        }

        public bool UnlockSlot(int slotId)
        {
            var configSlots = ConfigManager.Instance.configPuzzleInventory.configSlots;

            if (slotId < 0 || slotId >= configSlots.Count)
            {
                Debug.LogError(("Unlock slot failed! Invalid slot id"));
            }

            if (puzzleInventoryItems.dataCollection.Exists(c => c.slotId == slotId))
            {
                Debug.LogError("Unlock slot failed! Slot has already unlocked");
                return false;
            }

            //Check gem
            var gemBalance = GetCurrencyBalance(CurrencyType.Gem);
            var configSlot = ConfigManager.Instance.configPuzzleInventory.configSlots[slotId];

            if (gemBalance < configSlot.unlockCost)
            {
                return false;
            }

            //Consume gem   
            AddCurrencyAmount(CurrencyType.Gem, -configSlot.unlockCost, true, true);

            AddPuzzleInventorySlot(slotId, "", -1, true);
            return true;
        }

        #endregion

        #region Daily Reward Api
        void InitDailyReward()
        {
            var configDailyReward = ConfigManager.Instance.configDailyReward;

            //Init daily reward
            dailyRewardData.LoadData();
            var today0hUtcMs = TimeUtils.Today0hUtcMs();
            var dailyReward = dailyRewardData.value;
            if (dailyReward.lastClaimMs <= today0hUtcMs)
            {
                var currentPack = configDailyReward.dailyRewardItems[dailyReward.currentDailyRewardPackId];
                if (dailyReward.currentClaimIndex == currentPack.rewards.Count - 1)
                {
                    dailyReward.currentClaimIndex = -1;
                    dailyReward.currentDailyRewardPackId++;

                    if (dailyReward.currentDailyRewardPackId == configDailyReward.dailyRewardItems.Count)
                    {
                        dailyReward.currentDailyRewardPackId = 0;
                    }

                    dailyRewardData.SaveData();
                }

                hasPendingClaimReward = true;
            }
        }

        public bool ClaimDailyReward()
        {
            var dailyReward = dailyRewardData.value;

            if (dailyReward.lastClaimMs > TimeUtils.Today0hUtcMs())
            {
                Debug.LogError("Something wrong. Today has already claimed reward");
                return false;
            }

            var configDailyReward = ConfigManager.Instance.configDailyReward;
            int next = dailyReward.currentClaimIndex + 1;

            var configDailyRewardPack = configDailyReward.dailyRewardItems[dailyReward.currentDailyRewardPackId];
            var configDailyRewardItem = configDailyRewardPack.rewards[next];

            ClaimRewards(new List<ConfigRewardItem>()
            {
                configDailyRewardItem
            });


            dailyReward.currentClaimIndex = next;
            dailyReward.lastClaimMs = TimeUtils.GetServerUtcNowMs();

            dailyRewardData.SaveData();

            hasPendingClaimReward = false;
            onDailyRewardClaimed?.Invoke();

            return true;
        }

        #endregion

        #region Roulette Api
        void InitRoulette()
        {
            var serverUtcNowMs = TimeUtils.GetServerUtcNowMs();
            var configRoulette = ConfigManager.Instance.configRoulette;

            if (rouletteData == null)
            {
                rouletteData = new(new UserRoulette(configRoulette.watchAdSpinCount, configRoulette.resetCooldownSecond), "rouletteData");
                rouletteData.SetIgnoreSaveDataToken(ignoreWhenHasNotFinishTutorial);
            }

            rouletteData.LoadData();

            bool needSave = rouletteData.value.CheckReset(configRoulette.watchAdSpinCount, configRoulette.resetCooldownSecond) 
                || CheckResetRouletteConfigRewards(false);
            if (needSave)
                rouletteData.SaveData();
        }

        public ConfigRouletteReward MakeRouletteSpin()
        {
            var configRoulette = ConfigManager.Instance.configRoulette;
            if (rouletteData.value.CheckReset(configRoulette.watchAdSpinCount, configRoulette.resetCooldownSecond))
            {

            }
            else
            {
                if (rouletteData.value.isCooldown)
                {
                    Debug.LogError("Something wrong!, roulette data is coolodown");
                    return null;
                }
            }

            rouletteData.value.AddValue(-1);
            rouletteData.SaveData();

            //Start making a roulette spin
            var rewards = rouletteData.value.configRewards;
            var totalWeight = rewards.Sum(c => c.weight);
            int randomWeight = Random.Range(0, totalWeight + 1);
            ConfigRouletteReward finalReward = null;

            foreach (var reward in rewards)
            {
                if (randomWeight < reward.weight)
                {
                    finalReward = reward;
                    break;
                }
                randomWeight -= reward.weight;
            }

            if (finalReward == null)
                finalReward = rewards[^1];
            
            //Also claim reward here
            ClaimRewards(new List<ConfigRewardItem>
            {
                finalReward
            });

            return finalReward;
        }

        public bool CheckResetRouletteConfigRewards(bool autoSave)
        {
            var nowMs = TimeUtils.GetServerUtcNowMs();
            if (nowMs >= rouletteData.value.tsNextTimeResetPuzzleId)
            {
                var configRoulette = ConfigManager.Instance.configRoulette;
                var configPuzzle = ConfigManager.Instance.configPuzzle;
                
                rouletteData.value.configRewards.Clear();

                var availableNormalPuzzleIds = configPuzzle.configItems.FindAll(c => c.puzzleType == PuzzleType.Normal);

                foreach (var reward in configRoulette.rewards)
                {
                    if (reward.rewardType == RewardType.PuzzleItem && reward.isRandomPuzzleId)
                    {
                      
                        var randomPuzzleId = availableNormalPuzzleIds[Random.Range(0, availableNormalPuzzleIds.Count)].id;
                        rouletteData.value.configRewards.Add(new ConfigRouletteReward(RewardType.PuzzleItem, randomPuzzleId, reward.puzzleLevel, reward.amount, reward.weight));
                    }
                    else
                    {
                        rouletteData.value.configRewards.Add(reward);
                    }
                }

                rouletteData.value.tsNextTimeResetPuzzleId = TimeUtils.GetServerUtcNowMs() + configRoulette.puzzleIdResetCooldownSeconds * 1000;

                if (autoSave)
                    rouletteData.SaveData();

                return true;
            }

            return false;
        }
        #endregion
    }
}
