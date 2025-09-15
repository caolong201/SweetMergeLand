using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using USimpFramework.Utility;
using USimpFramework.UI;
using System;

namespace TheKingOfMergeCity
{
    using Enum;

    [DefaultExecutionOrder(30)]
    public sealed class InGameManager : SimpleSingleton<InGameManager>
    {
        public Action<GameState> onGameStateChanged;

        [SerializeField] PuzzlesController _puzzlesController;
        public PuzzlesController puzzlesController => _puzzlesController;

        [SerializeField] CustomersController _customersController;
        public CustomersController customersController => _customersController;

        [SerializeField] Camera _mainCamera;
        public Camera mainCamera => _mainCamera;

        public bool isPlayingServeCustomer { get; set; }

        GameState _gameState;
        public GameState gameState
        {
            get => _gameState;
            private set
            {
                if (_gameState == value)
                    return;

                var oldState = _gameState;
                _gameState = value;
                onGameStateChanged?.Invoke(oldState);
            }
        }

        public int maxAppearCustomer
        {
            get
            {
                if (UserManager.Instance.finishTutorial)
                    return ConfigManager.Instance.configLevel.maxAppearCustomer;

                return 1;
            }
        }

        protected override void Awake()
        {
            base.Awake();
        }

        void Start()
        {
           
            StartCoroutine(CR_StartGame());

            IEnumerator CR_StartGame()
            {
                yield return null;

                //Check first start to prevent not reseting level and customer

                UIManager.Instance.ShowView<UIInGameView>();
                var uiIngame = UIManager.Instance.currentView as UIInGameView;
                var boardData = UserManager.Instance.boardData;
                puzzlesController.LoadLevel(boardData, uiIngame.puzzleItemContainerTrans, uiIngame.groupRowTrans);

                //For tutorial, only allow one customer can appear at a time
                customersController.LoadCustomerOrders(boardData, uiIngame.customerOrderContainerTrans);
                customersController.StartGame();

                //Check tutorial
                if (!UserManager.Instance.finishTutorial)
                {
                    UIManager.Instance.ShowPopup<UITutorialPopup>();

                    TutorialManager.Instance.PlayStep();
                }
                else
                {
                    StartCoroutine(CR_WaitToShowBannerAd());
                }
                
                gameState = GameState.Playing;
            }

            IEnumerator CR_WaitToShowBannerAd()
            {
                yield return new WaitForSeconds(4f);
                ApplovinManager.Instance.ShowBannerAd();
            }
        }
    }
}
