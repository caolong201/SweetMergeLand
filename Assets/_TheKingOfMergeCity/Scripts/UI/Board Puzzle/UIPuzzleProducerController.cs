using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using USimpFramework.UI;

namespace TheKingOfMergeCity
{
    using Enum;
    using Model;
    using Tutorial;
    using Config;

    public class UIPuzzleProducerController : UIPuzzleItemController
    {
        [SerializeField] Image energyImage;
        [SerializeField] ParticleSystem shineVfx;

        void Start()
        {
            energyImage.DOColor(new Color32(154,154,154,255),1f).SetEase(Ease.Linear).SetLoops(-1,LoopType.Yoyo);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            energyImage.DOKill();
        }

        protected override void ResetData()
        {
            base.ResetData();
            shineVfx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void OnEnable()
        {
            if (isMaxLevel)
            {
                shineVfx.Play();
            }
        }

        public override void Setup(ConfigPuzzleItem config, ConfigPuzzleType configPuzzleType, UserPuzzleDataItem data, Vector2 spawnPosition, Vector2 itemSize)
        {
            base.Setup(config, configPuzzleType, data, spawnPosition, itemSize);

            if (isMaxLevel)
                shineVfx.Play();
        }


        public bool canProduce => isMaxLevel;

        public override void OnPointerClick(PointerEventData eventData)
        {
            var userManager = UserManager.Instance;
            var inGameManager = InGameManager.Instance;
            var uiManager = UIManager.Instance;

            if (!userManager.finishTutorial)
            {
                var currentStep = TutorialManager.Instance.currentStep;
                if (currentStep.config is ConfigServeCustomer configServeCustomer)
                {
                    if (!configServeCustomer.allowClickProducer)
                        return;
                }
                else
                {
                    if (currentStep.config is not ConfigTutorialClickProducePuzzle)
                        return;
                }
            }

            base.OnPointerClick(eventData);

            if (!isMaxLevel || isDragging)
                return;

            //Produce single item
            var puzzleController = inGameManager.puzzlesController;
            if (puzzleController.IsBoardFullItem())
            {
                var uiIngame = UIManager.Instance.currentView as UIInGameView;
                uiIngame.ShowBoardFullText("Board is full!", transform.position + Vector3.up * 100f);
                return;
            }

            //Check consume energy
            int energy = userManager.GetCurrencyBalance(CurrencyType.Energy);
            if (energy < configPuzzleType.energyCostPerProduce)
            {
                var uiIngame = uiManager.currentView as UIInGameView;
                uiIngame.ShowBoardFullText("Dont have enough energy!", transform.position + Vector3.up * 100f);
                return;
            }

            userManager.AddCurrencyAmount(CurrencyType.Energy, -configPuzzleType.energyCostPerProduce, true,true);

           
            //Get nearest empty tile from this board position
            var nearestEmptyTile = puzzleController.GetNearestEmptyTileFromBoardPosition(data.boardPosition);
            if (nearestEmptyTile == null)
            {
                Debug.LogError("Something wrong, item producer " + config.id);
                return;
            }

            string itemId = "";
            int itemLevel = 0;

            //Spawn new item
            if (userManager.finishTutorial)
            {
                itemId = InGameManager.Instance.puzzlesController.GetProduceItemId(config);
            }
            else
            {
                var currentStep = TutorialManager.Instance.currentStep;
                if (currentStep is ClickProducePuzzleTutorialStep clickProducePuzzleTutorialStep )
                {
                    var configItem = clickProducePuzzleTutorialStep.GetCurrentConfigItem();
                    itemId = configItem.itemId;
                    itemLevel = configItem.level;
                }
                else
                {
                    itemId = InGameManager.Instance.puzzlesController.GetProduceItemId(config);
                }
            }

            //Check tutorial
            TutorialManager.Instance.CheckCompleteStep<ConfigTutorialClickProducePuzzle>();

            var uiPuzzle = puzzleController.SpawnPuzzleItem(itemId, itemLevel, 2, nearestEmptyTile); 
            uiPuzzle.transform.position = transform.position;
            uiPuzzle.transform.localScale *= 1.5f;
            uiPuzzle.transform.DOScale(1f, 0.5f).SetEase(Ease.OutSine);
            uiPuzzle.BringToFront();
            uiPuzzle.MoveItemToTile(nearestEmptyTile,Ease.OutSine, 0.5f, () =>
            {
                //Todo: Play vfx

            });
        }

        public override void Upgrade()
        {
            base.Upgrade();

            if (isMaxLevel)
            {
                shineVfx.Play();
            }
        }

        public override void OnLevelUpdated()
        {
            base.OnLevelUpdated();
            
            energyImage.gameObject.SetActive(isMaxLevel);
        }
    }
}
