using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

using USimpFramework.UI;
using USimpFramework.Utility;

using DG.Tweening;


namespace TheKingOfMergeCity
{
    using Config;
    using Enum;

    public class HomeManager : SimpleSingleton<HomeManager>
    {
        public event System.Action onAreaCompleted;
        public event System.Action onReturnToIngame;
        
        /// <summary> Use this event instead of UserManger.onPlayerLevelUp to wait for some transition to complete </summary>
        public event System.Action onPlayerLevelUp;

        [SerializeField] WorkerController _workerPrefab;
        public WorkerController workerPrefab => _workerPrefab;

        [Header("VFX")]
        [SerializeField] ParticleSystem buildingVfxPrefab;
        [SerializeField] ParticleSystem buildingCompleteVfxPrefab;
        
        SceneInstance sceneInstance;

        List<DecoItemController> decoItems = new();
        public IReadOnlyList<DecoItemController> readonlyDecoItems => decoItems;

        public ConfigAreaItem currentConfigArea { get; private set; }

        AreaVFXController areaVFXController;

        void Start()
        {

            var userManager = UserManager.Instance;
            
            //Load area scene
            LoadArea(userManager.currentSelectAreaId, () =>
            {
                //Only check when loading this home scene
                if (userManager.hasPendingClaimReward &&  
                    userManager.currentPlayerLevel >= ConfigManager.Instance.configDailyReward.unlockAtPlayerLevel
                    && !userManager.isPlayingDecoBuildingFromInGameScene)
                {
                    UIManager.Instance.ShowPopup<UIDailyRewardPopup>();
                }
                
            });
        }

        void Update()
        {
            #region Just For testing
            if (Input.GetKeyDown(KeyCode.P))
                UIManager.Instance.ShowPopup<UIPlayerLevelUpPopup>();
            #endregion
        }

        public async void LoadArea(int areaId, System.Action onCompleted = null)
        {
            //Unload the old scene
            SimpleObjectPool.ClearAll();

            if (sceneInstance.Scene.IsValid())
            {
               await Addressables.UnloadSceneAsync(sceneInstance).Task;
            }

            currentConfigArea = ConfigManager.Instance.configArea.areaItems.Find(c => c.id == areaId);
            if (currentConfigArea == null)
                throw new UnityException("Load area failed! Invalid area " + UserManager.Instance.currentSelectAreaId);

            var asyncOp = Addressables.LoadSceneAsync(currentConfigArea.sceneAddress, LoadSceneMode.Additive);
            await asyncOp.Task;
            sceneInstance = asyncOp.Result;
            SceneManager.SetActiveScene(sceneInstance.Scene);

            //Setup decoration items
            SetupDecorationItems();
            
            areaVFXController = FindAnyObjectByType<AreaVFXController>();

            UIManager.Instance.ShowView<UIHomeView>();

            if (UserManager.Instance.isPendingPlayPlayerLevelUp)
            {
                ShowLevelUpReward();
            }

            onCompleted?.Invoke();
        }

        void SetupDecorationItems()
        {
            decoItems.Clear();
            var decoItemArr = FindObjectsByType<DecoItemController>(FindObjectsSortMode.None);
            foreach (var item in decoItemArr)
            {
                item.onDecoItemStateChanged -= OnDecoItemStateChanged;
                item.onDecoItemStateChanged += OnDecoItemStateChanged;

                decoItems.Add(item);
            }

            //Setup deco item first
            for (int i = 0; i < decoItems.Count; i++)
            {
                var decoItem = decoItems[i];
                decoItem.Setup();
            }

            //Then update state
            for (int i = 0; i < decoItems.Count; i++)
            {
                decoItems[i].UpdateState();
            }
        }

        void OnDestroy()
        {
            decoItems.ForEach(deco => deco.onDecoItemStateChanged -= OnDecoItemStateChanged);
            SimpleObjectPool.ClearAll();
        }

        /// <summary> This is called even first starting the game, or while playing ingame</summary>
        void OnDecoItemStateChanged(DecoItemController decoItem, DecoItemState oldState)
        {
            StartCoroutine(CR_OnDecoItemStateChanged());

            IEnumerator CR_OnDecoItemStateChanged()
            {
                var state = decoItem.decoItemState;
                var userManager = UserManager.Instance;
                var uiManager = UIManager.Instance;
                
                if (state == DecoItemState.Completed)
                {
                    bool isPlayingDecoBuilding = userManager.isPlayingDecoBuildingFromInGameScene;

                    //Wait for some transition to complete
                    if (oldState == DecoItemState.Unlock)
                    {
                        //Play some effects
                        bool isPlayingEffect = true;
                        var uiHomeView = uiManager.currentView as UIHomeView;
                        var uiTopbar = uiHomeView.uiTopBar;
                        var decoScreenPoint = Camera.main.WorldToScreenPoint(decoItem.GetFirstDecoChild().position);
                        uiTopbar.PlayCurrencyGachaEffect(CurrencyType.Energy,
                            ConfigManager.Instance.configGlobal.energyRewardAfterBuild, decoScreenPoint,1f, () =>
                            {
                                isPlayingEffect = false;
                            });

                        uiTopbar.PlayCurrencyGachaEffect(CurrencyType.Exp, decoItem.config.expReward, decoScreenPoint);
                        
                        yield return new WaitForSeconds(0.3f);
                        uiHomeView.ShowGroupInteractableButton(true);
                        yield return new WaitWhile(() => isPlayingEffect);
                        uiManager.SetInteraction(true);
                    }

                    //Check if is last deco
                    int index = currentConfigArea.decoItems.FindIndex(d => d.id == decoItem.config.id);
                    
                    //Check level up (for newly completed deco item)
                    if (oldState == DecoItemState.Unlock && userManager.currentPlayerLevel > userManager.oldLevel)
                    {
                        Debug.Log("<color=yellow>Level up!!!</color>");
                        userManager.isPendingPlayPlayerLevelUp = true;

                        if (index == currentConfigArea.decoItems.Count - 1) //Area completed
                        {
                            
                        }
                        else
                        {
                            ShowLevelUpReward();
                        }
                        
                        onPlayerLevelUp?.Invoke();
                    }
                    
                    if (index == currentConfigArea.decoItems.Count - 1)
                    {
                        //Go to next unlock area
                        if (oldState == DecoItemState.Unlock) //Check if this is from old unlock state
                        {
                            onAreaCompleted?.Invoke();
                        }
                    }
                    else
                    {
                        //Unlock new deco items
                        var unlockedDecoIds = decoItem.config.nextUnlockDecoIds;
                        foreach (var id in unlockedDecoIds)
                        {
                            if (userManager.currentSelectAreaData.completedDecoIds.Contains(id))
                                continue;

                            var needUnlockDeco = decoItems.Find(d => d.decoId == id);
                            if (needUnlockDeco == null)
                            {
                                Debug.LogError("Cannot unlock deco! Invalid Id " + id);
                                continue;
                            }
                            needUnlockDeco.Unlock();
                        }

                        if (oldState == DecoItemState.Unlock && isPlayingDecoBuilding && !userManager.isPendingPlayPlayerLevelUp)
                        {
                            //Return to puzzle scene
                            userManager.isPlayingDecoBuildingFromInGameScene = false;
                            onReturnToIngame?.Invoke();
                        }
                    }
                }
            }
        }

        public void ShowLevelUpReward()
        {            
            var level = UserManager.Instance.currentPlayerLevel;
            var configFeatures = ConfigManager.Instance.configFeature.items.FindAll(c => c.unlockAtPlayerLevel == level);

            var configRewards = new List<ConfigRewardItem>();
            #region Un comment this when loading reward
            /* var rewards = ConfigManager.Instance.configPlayerLevel.items[level].currencyRewards;
             foreach (var reward in rewards)
             {
                 configRewards.Add(new ConfigRewardItem(RewardType.Currency, reward.currencyType, reward.amount));
             }*/
            #endregion

            var popup = UIManager.Instance.ShowPopup<UIPlayerLevelUpPopup>();
            popup.Setup(configFeatures, configRewards);
        }
        

        public void PlayBuildingDecoFlow(DecoItemController decoItemController, float delay = 0)
        {
            DOVirtual.DelayedCall(delay, () =>
            {
                var uiHomeView = UIManager.Instance.currentView as UIHomeView;
                uiHomeView.ShowGroupInteractableButton(false);
                decoItemController.Build();
            });
        }

        public void PlayBuildingDecoFlow(string decoId, float delay = 0)
        {
            var decoItem = decoItems.Find(d => d.decoId == decoId);
            PlayBuildingDecoFlow(decoItem, delay);
        }

        public void PlayBuildingVfx(Vector3 position)
        {
            areaVFXController.PlayBuildingVfx(position);
        }

        public void PlayBuildingCompletedVfx(Vector3 position)
        {
            areaVFXController.PlayBuildingCompletedVfx(position);
        }

        public void BuildDecoration(string decoId)
        {
            var decoItem = decoItems.Find(d => d.decoId == decoId);
            decoItem.Build();

        }
    }
}
