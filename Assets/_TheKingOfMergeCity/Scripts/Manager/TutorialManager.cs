using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using USimpFramework.Utility;
using USimpFramework.UI;
using System;

namespace TheKingOfMergeCity
{ 
    using Tutorial;
    using Config;
    public class TutorialManager : SimpleSingleton<TutorialManager>
    {
        public event Action onStepPlayed;
        public event Action onStepCompleted;
        public TutorialStep currentStep { get; private set; }

        List<TutorialStep> steps = new();
        public void SetStep(TutorialStep step)
        {
            currentStep = step;
        }


        #region Solution
        //Todo: How to handle that the data for complete process will always happen immediately when complete the step
        //Making the PreCompleteStep() -> save all the data, and handle logic immdiately,
        //CompleteStep() -> Wait for all association transition to complete, then complete -> check for showing the next step
        #endregion

        protected override void Awake()
        {
            base.Awake();

            BootManager.Instance.onAfterSceneLoaded += OnSceneChanged;
        }

        void Start()
        {
            if (UserManager.Instance.finishTutorial)
                return;

            foreach (var config in ConfigManager.Instance.configTutorial.readonlySteps)
            {
                TutorialStep newStep;
                if (config is ConfigTutorialClickProducePuzzle)
                    newStep = new ClickProducePuzzleTutorialStep(config);
                else
                {
                    newStep = new NormalTutorialStep(config);
                }

                steps.Add(newStep);
            }

            if (steps.Count > 0)
                currentStep = steps[0];

        }

        void OnDestroy()
        {
            if (InGameManager.Instance != null)
                InGameManager.Instance.puzzlesController.onPuzzleMerged -= OnPuzzleMerged;

            BootManager.Instance.onAfterSceneLoaded -= OnSceneChanged;
        }
        
        void OnSceneChanged(string sceneName)
        {
            if (sceneName == SceneConstants.IN_GAME_SCENE_NAME && !UserManager.Instance.finishTutorial)
            {
                InGameManager.Instance.puzzlesController.onPuzzleMerged += OnPuzzleMerged;
            }
        }
        
        void OnPuzzleMerged(UIPuzzleItemController puzzleItem)
        {
            var config = currentStep.config;

            if (config is ConfigTutorialPuzzleMove configTutorialPuzzleMove)
            {
                var puzzleController = InGameManager.Instance.puzzlesController;
                var endItem = puzzleController.GetPuzzleItem(configTutorialPuzzleMove.endPosition);
                endItem.SetRaycastTarget(true);
                if (configTutorialPuzzleMove.completeCondition == ConfigTutorialPuzzleMove.CompleteCondition.ByEndPosition)
                {
                    if (endItem == puzzleItem)
                    {
                        CompleteStep();
                    }
                }
                else if (configTutorialPuzzleMove.completeCondition == ConfigTutorialPuzzleMove.CompleteCondition.ByItemId)
                {
                    if (puzzleItem.config.id == configTutorialPuzzleMove.puzzleId)
                    {
                        CompleteStep();
                    }
                }
            }
        }

        public void PlayStep()
        {
            var config = currentStep.config;
            //them
            var uiPopup = UIManager.Instance.GetPopup<UITutorialPopup>();
            if (uiPopup == null) return;

            if (config is ConfigTutorialClickBuyEnergy buyEnergyStep)
            {
                uiPopup.ShowEffect();
            }
            //Check for each type of config tutorial step
            if (config is ConfigTutorialPuzzleMove configTutorialPuzzleMove)
            {
                var secondItem = InGameManager.Instance.puzzlesController.GetPuzzleItem(configTutorialPuzzleMove.endPosition);
                if (secondItem != null)
                {
                    secondItem.SetRaycastTarget(false);
                }
            }
            else if (config is ConfigTutorialClickProducePuzzle configClickProducePuzzle)
            {
                var puzzleController = InGameManager.Instance.puzzlesController;
                var item = puzzleController.GetPuzzleItem(configClickProducePuzzle.clickPosition);
                if (item == null || item is not UIPuzzleProducerController producerItem || !producerItem.canProduce)
                {
                    Debug.LogError($"This board position {configClickProducePuzzle.clickPosition} can have producer item!");
                    return;
                }

                producerItem.SetDraggable(false);
            }

            var uiTutorialPopup = UIManager.Instance.GetPopup<UITutorialPopup>();

            if (uiTutorialPopup == null)
               throw new UnityException("Something went wrong! Tutoiral Popup is no spawn yet!");

            if (config.showDescriptionWhenPlay)
            {
                uiTutorialPopup.ShowEffect();
            }
            
            if (config is ConfigServeCustomer configServeCustomer)
            {
                if (configServeCustomer.allowMovePuzzle)
                {
                    uiTutorialPopup.ShowBlockTouch(false);
                    uiTutorialPopup.ShowMaskBg(false);
                }
            }
           

            onStepPlayed?.Invoke();
        }

        public void CheckCompleteStep<T>() where T: ConfigTutorialStep
        {
            if (UserManager.Instance.finishTutorial)
                return;

            if (typeof(T) != currentStep.config.GetType())
                return;
            //them
            if (currentStep.config is ConfigTutorialClickBuyEnergy)
            {
                var uiPopup = UIManager.Instance.GetPopup<UITutorialPopup>();
                if (uiPopup != null)
                    uiPopup.HideEffect();
            }

            if (currentStep is ClickProducePuzzleTutorialStep producePuzzleTutorialStep)
            {
                producePuzzleTutorialStep.clickCount++;
                var config = producePuzzleTutorialStep.config as ConfigTutorialClickProducePuzzle;
               
                if (producePuzzleTutorialStep.clickCount == config.forceProduceItem.Count)
                {
                    var uiProducer = InGameManager.Instance.puzzlesController.GetPuzzleItem(config.clickPosition);
                    if (uiProducer != null)
                    {
                        uiProducer.SetDraggable(true);
                    }

                    CompleteStep();

                }
            }
            else if (currentStep.config is ConfigServeCustomer configServeCustomer)
            {
                CompleteStep();
            }
            else if (currentStep.config is ConfigTutorialClickBuildDeco)
            {
                CompleteStep();
            }
            else if (currentStep.config is ConfigTutorialClickStartInBuildPopup)
            {
                CompleteStep();
            }
        }


        void CompleteStep()
        {
            //Debug.Log("Complete step: " + currentStep.config.id);
            int stepIndex = steps.IndexOf(currentStep);
            if (stepIndex == steps.Count - 1)
            {
                FinishTutorial();
            }
            else
            {
                var uiTutorialPopup = UIManager.Instance.currentPopup as UITutorialPopup;
                uiTutorialPopup.HideEffect();
                
                onStepCompleted?.Invoke();

                //Go to next step
                currentStep = steps[stepIndex + 1];
                PlayStep();
            }
        }

        void FinishTutorial()
        {
            //Save all related data
            UserManager.Instance.SaveAll();

            //Send event the first object of the area to unlock
            GameAnalyticsManager.Instance.SendDecoBuildStartEvent(0, 0);


            InGameManager.Instance.puzzlesController.onPuzzleMerged -= OnPuzzleMerged;
            UIManager.Instance.HidePopup<UITutorialPopup>();
        }

        void Update()
        {
            #region Testing show tutorial popup
            /*if (Input.GetKeyDown(KeyCode.S))
            {
                UIManager.Instance.ShowPopup<UITutorialPopup>();
            }
            else if (Input.GetKeyDown(KeyCode.H))
            {
                UIManager.Instance.HidePopup<UITutorialPopup>();
            }*/
            #endregion
        }
    }
}

namespace TheKingOfMergeCity.Tutorial
{
    using Config;

    public abstract class TutorialStep
    {
        public ConfigTutorialStep config { get; private set; }

        public TutorialStep(ConfigTutorialStep configTutorialStep)
        {
            config = configTutorialStep;
        }
    }

    public class NormalTutorialStep : TutorialStep
    {
        public NormalTutorialStep(ConfigTutorialStep configTutorialStep) : base(configTutorialStep)
        {

        }
    }


    public class ClickProducePuzzleTutorialStep : TutorialStep
    {
        public int clickCount { get; set; } = 0;

        public ClickProducePuzzleTutorialStep(ConfigTutorialStep configTutorialStep) : base(configTutorialStep)
        {

        }

        public ConfigOrderItem GetCurrentConfigItem()
        {
            return (config as ConfigTutorialClickProducePuzzle).forceProduceItem[clickCount];
        }
    }
}
