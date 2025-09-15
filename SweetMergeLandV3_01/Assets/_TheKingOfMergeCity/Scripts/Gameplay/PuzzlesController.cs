using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using USimpFramework.Utility;
using USimpFramework.UI;
using DG.Tweening;
using System;

namespace TheKingOfMergeCity
{
    using Config;
    using Model;
    using System.Linq;

    [DefaultExecutionOrder(100)]
    public class PuzzlesController : MonoBehaviour
    {
        public event Action<UIPuzzleItemController> onPuzzleMerged;
        
        public Dictionary<string, ConfigPuzzleItem> availableProducerConfigDic { get; private set; } = new();
        
        List<UIPuzzleItemController> mergeSuggestionItems = new();
        List<GameObject> tileRows = new();
        List<Tween> highlightTweens = new();

        Transform groupPuzzleItemTrans;
        TileInfo[][] board;

        /// <summary> Tile size of the board, in pixel  </summary>
        Vector2 tileSize;
        Vector2 firstScreenPos;
        Vector2Int boardSize;

        float nextTimeSuggesstion;
        bool needShowMergeItemGlowFx;
        bool beginDragItem;

        void Start()
        {
            nextTimeSuggesstion = Time.time + ConfigManager.Instance.configGlobal.showSuggestionInterval;
        }

        //[SerializeField] BoardPosition test;
        void Update()
        {
            #region Testing purpose
            /*if (Input.GetKeyDown(KeyCode.A))
            {
                var neighborTiles = GetNeighborTiles(test);
                string log = "";
                foreach (var tile in neighborTiles)
                {
                    log += $"({tile.boardPosition.y},{tile.boardPosition.x}) ";
                }
                Debug.Log(log);
            }*/
            #endregion

            if (!beginDragItem && Time.time >= nextTimeSuggesstion)
            {
                ShowSuggestion();
            }
        }

        public void LoadLevel(UserBoardData userBoardData, Transform groupPuzzleItemTrans, Transform groupRowtrans)
        {
            this.groupPuzzleItemTrans = groupPuzzleItemTrans;

            var configManager = ConfigManager.Instance;
            var configPuzzle = configManager.configPuzzle;


            //Clamp group puzzle size first
            var rectTrans = groupRowtrans as RectTransform;
            var size = rectTrans.GetSize();
            size.x = Mathf.Clamp(size.x, configPuzzle.boardSizePixelMin.x, configPuzzle.boardSizePixelMax.x);
            size.y = Mathf.Clamp(size.y, configPuzzle.boardSizePixelMin.y, configPuzzle.boardSizePixelMax.y);

            rectTrans.SetSize(size);
            (groupPuzzleItemTrans as RectTransform).SetSize(size);


            GenerateBoard(groupRowtrans);

            Dictionary<string, ConfigPuzzleItem> configPuzzleItemDic = new();

            var puzzleDatas = userBoardData.puzzleDatas;

            //Load level
            foreach (var data in puzzleDatas)
            {
                var boardPosition = data.boardPosition;
                var tileInfo = board[boardPosition.y][boardPosition.x];

                if (!configPuzzleItemDic.TryGetValue(data.puzzleId, out var configPuzzleItem))
                {
                    configPuzzleItem = ConfigManager.Instance.configPuzzle.configItems.Find(c => c.id == data.puzzleId);
                    configPuzzleItemDic[data.puzzleId] = configPuzzleItem;
                }

                var configPuzzleType = configPuzzle.configPuzzlePerTypes.Find(s => s.puzzleType == configPuzzleItem.puzzleType);
                var puzzleItem = SpawnPuzzleItem(configPuzzleItem, configPuzzleType, data, tileInfo);

                if (puzzleItem is UIPuzzleProducerController producer && !puzzleItem.isBlock && producer.canProduce)
                {
                    if (!availableProducerConfigDic.ContainsKey(puzzleItem.config.id))
                        availableProducerConfigDic.Add(puzzleItem.config.id, configPuzzleItemDic[puzzleItem.config.id]);
                }
            }
        }

        public string GetProduceItemId(ConfigPuzzleItem configProducer)
        {
            if (configProducer.puzzleType != Enum.PuzzleType.Producer)
                return string.Empty;

            //Recalculate the weight between produced item
            var settingPerProduceItems = configProducer.settingPerProducedItems;
            var probalities = new List<int>();
            int lastProb = 0;
            int sum = configProducer.settingPerProducedItems.Sum(s => s.probability);
            if (settingPerProduceItems[^1].probability == 0)
            {
                lastProb = 100 - sum;
            }
            int max = 0;
            Dictionary<string, int> itemIdPropDic = new();

            for (int i = 0; i < settingPerProduceItems.Count; i++)
            {
                var setting = settingPerProduceItems[i];
                int configProb = i == settingPerProduceItems.Count - 1 ? lastProb : setting.probability;

                //Check for any customers has any of this item id
                if (InGameManager.Instance.customersController.readonlyServingCustomers.Any(c => c.HasOrderOfThisItem(setting.id)))
                {
                    itemIdPropDic.Add(setting.id, configProb);
                    max += configProb;
                }
            }

           /* string log = "Prob of " + configProducer.id;
            foreach (var pair in itemIdPropDic)
            {
                log += $"{pair.Key}: {pair.Value} ";
            }
            log += $" max {max}";
            Debug.Log(log);*/

            int randomProb = UnityEngine.Random.Range(0, max);
            int total = 0;
            foreach (var keyValue in itemIdPropDic)
            {
                total += keyValue.Value;
                if (randomProb <= total)
                    return keyValue.Key;
            }

            return itemIdPropDic.Keys.ToList()[^1];
        }


        public List<UIPuzzleItemController> GetAllPuzzles()
        {
            var result = new List<UIPuzzleItemController>();
            for (int i = 0; i < boardSize.y; i++)
            {
                for (int j = 0; j < boardSize.x; j++)
                {
                    if (board[i][j].puzzleItem != null)
                        result.Add(board[i][j].puzzleItem);
                }
            } 
            return result;
        }
        
        public bool IsBoardFullItem()
        {
            for (int i = 0; i < boardSize.y; i++)
            {
                for (int j = 0; j < boardSize.x; j++)
                {
                    if (board[i][j].isEmpty)
                        return false;
                }
            }
            return true;
        }

        public List<UIPuzzleNormalItemController> GetUnblockNormalItems()
        {
            var result = new List<UIPuzzleNormalItemController>();
            for (int i = 0; i < boardSize.y; i++)
            {
                for (int j = 0; j < boardSize.x; j++)
                {
                    var tile = board[i][j];
                    if (tile.puzzleItem == null)
                        continue;
                    var puzzle = tile.puzzleItem;
                    if (puzzle is not UIPuzzleNormalItemController controller || controller.isBlock)
                        continue;
                    result.Add(controller);
                }
            }
            return result;
        }

        public void RemovePuzzle(UIPuzzleItemController puzzle)
        {
            //Save data here
            var boardPosition = puzzle.data.boardPosition;

            //Save data here
            UserManager.Instance.RemovePuzzleAt(boardPosition);

            RemoveItemListener(puzzle);
            board[boardPosition.y][boardPosition.x].puzzleItem = null;
        }

        public bool CanMerge(UIPuzzleItemController fromItem, UIPuzzleItemController toItem)
        {
            bool isFromItemValid = fromItem != null && fromItem.data.blockingLevel >= 1;
            bool isToItemValid = toItem != null && toItem.data.blockingLevel >= 1;
            return isFromItemValid && isToItemValid && fromItem != toItem && toItem.config.id == fromItem.config.id && toItem.data.level == fromItem.data.level;
        }

        /// <summary>
        /// Get neighbor tiles, with one unit distance along horizontal or vertical axis 
        /// </summary>
        /// <returns></returns>
        List<TileInfo> GetNeighborTiles(BoardPosition boardPosition)
        {
            var result = new List<TileInfo>();
            var irow = boardPosition.y;
            var icol = boardPosition.x;

            var curRow = irow - 1;
            var curCol = icol;

            if (IsPositionValid(curRow, curCol))
                result.Add(board[curRow][curCol]);

            curRow = irow + 1;
            curCol = icol;

            if (IsPositionValid(curRow, curCol))
                result.Add(board[curRow][curCol]);

            curRow = irow;
            curCol = icol - 1;

            if (IsPositionValid(curRow, curCol))
                result.Add(board[curRow][curCol]);

            curRow = irow;
            curCol = icol + 1;

            if (IsPositionValid(curRow, curCol))
                result.Add(board[curRow][curCol]);

            return result;
        }


        public Vector2 BoardToScreenPos(BoardPosition boardPosition)
        {
            if (!IsPositionValid(boardPosition.x, boardPosition.y))
            {
                Debug.LogError($"board position {boardPosition} invalid!");
                return Vector2.zero;
            }

            return board[boardPosition.y][boardPosition.x].worldPosition;
        }

        public UIPuzzleItemController GetPuzzleItem(BoardPosition boardPosition)
        {
            return board[boardPosition.y][boardPosition.x].puzzleItem;
        }

        bool IsPositionValid(int irow, int icol)
        {
            return icol >= 0 && icol < boardSize.x && irow >= 0 && irow < boardSize.y;
        }

        struct Cell
        {
            public int row, col, dist;
            public Cell(int row, int col, int dist)
            {
                this.row = row;
                this.col = col;
                this.dist = dist;
            }
        }

        //up, down, left, right, and 4 diagonal
        static readonly int[,] directions = { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 }, { 1, 1 }, { 1, -1 }, { -1, 1 }, { -1, -1 } };

        public TileInfo GetTileInfo(BoardPosition boardPosition)
        {
            return board[boardPosition.y][boardPosition.x];
        }

        public TileInfo GetNearestEmptyTileFromBoardPosition(BoardPosition boardPosition)
        {
            int maxRow = boardSize.y;
            int maxCol = boardSize.x;

            bool[,] visited = new bool[maxRow, maxCol];
            Queue<Cell> cellQueue = new();
            cellQueue.Enqueue(new Cell(boardPosition.y, boardPosition.x, 0));
            visited[boardPosition.y, boardPosition.x] = true;

            while (cellQueue.Count > 0)
            {
                var cur = cellQueue.Dequeue();

                if (board[cur.row][cur.col].isEmpty)
                    return board[cur.row][cur.col];

                //Explore 8 directions starting from the input position
                for (int i = 0; i < 8; i++)
                {
                    int newRow = cur.row + directions[i, 0];
                    int newCol = cur.col + directions[i, 1];

                    if (newRow >= 0 && newRow < maxRow && newCol >= 0 && newCol < maxCol && !visited[newRow, newCol])
                    {
                        cellQueue.Enqueue(new Cell(newRow, newCol, cur.dist + 1));
                        visited[newRow, newCol] = true;
                    }
                }
            }

            return null;
        }

        public UIPuzzleItemController GetNearestMergableItem(BoardPosition boardPosition, UIPuzzleItemController comparingItem)
        {
            int maxRow = boardSize.y;
            int maxCol = boardSize.x;

            bool[][] visited = new bool[maxRow][];
            for (int index = 0; index < maxRow; index++)
            {
                visited[index] = new bool[maxCol];
            }
            Queue<Cell> cellQueue = new();
            cellQueue.Enqueue(new Cell(boardPosition.y, boardPosition.x, 0));
            visited[boardPosition.y][boardPosition.x] = true;
            while (cellQueue.Count > 0)
            {
                var cur = cellQueue.Dequeue();
                var item = board[cur.row][cur.col].puzzleItem;

                if (CanMerge(item, comparingItem))
                    return item;

                //Explore 8 directions starting from the input position
                for (int i = 0; i < 8; i++)
                {
                    int newRow = cur.row + directions[i, 0];
                    int newCol = cur.col + directions[i, 1];

                    if (newRow >= 0 && newRow < maxRow && newCol >= 0 && newCol < maxCol && !visited[newRow][newCol] && item != null && item.data.blockingLevel >= 1)
                    {
                        cellQueue.Enqueue(new Cell(newRow, newCol, cur.dist + 1));
                        visited[newRow][newCol] = true;
                    }
                }
            }

            return null;
        }

        TileInfo GetNearestTile(Vector2 worldPosition)
        {
            //We need to convert this world position to screen position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(groupPuzzleItemTrans as RectTransform, worldPosition, null, out var localPoint);

            int icol = Mathf.RoundToInt((localPoint.x - firstScreenPos.x) / tileSize.x);
            int irow = Mathf.RoundToInt((firstScreenPos.y - localPoint.y) / tileSize.y);

            return !IsPositionValid(irow, icol) ? null : board[irow][icol];
        }

        public TileInfo GetFirstEmptyTile()
        {
            for (int i = 0; i < boardSize.y; i++)
            {
                for (int j = 0; j < boardSize.x; j++)
                {
                    if (board[i][j].isEmpty)
                        return board[i][j];
                }
            }

            return null;
        }

        void OnHoldItem(UIPuzzleItemController puzzleItem)
        {
            var nearestTileItem = GetNearestTile(puzzleItem.transform.position);

            //Check for is puzzle overlay inventory bag
            if (nearestTileItem == null)
            {
                if (IsPuzzleOverlayInventoryBag(puzzleItem))
                {
                    if (!needShowMergeItemGlowFx)
                    {
                        needShowMergeItemGlowFx = true;

                        var uiInGameView = UIManager.Instance.currentView as UIInGameView;
                        uiInGameView.ShowMergeItemGlowFx(uiInGameView.inventoryButton.transform.position, true);
                    }
                }
                else
                {
                    needShowMergeItemGlowFx = false;
                    var uiInGameView = UIManager.Instance.currentView as UIInGameView;
                    uiInGameView.ShowMergeItemGlowFx(Vector3.zero, false);
                }
                
                return;
            }

            //Check for mergable item in side the board
            var nearestItem = nearestTileItem.puzzleItem;
            if (CanMerge(puzzleItem, nearestItem) && nearestItem != null & nearestItem.data.blockingLevel > 1)
            {
                if (!needShowMergeItemGlowFx)
                {
                    needShowMergeItemGlowFx = true;

                    var uiInGameView = UIManager.Instance.currentView as UIInGameView;
                    uiInGameView.ShowMergeItemGlowFx(nearestItem.transform.position, true);

                }
            }
            else
            {
                needShowMergeItemGlowFx = false;
                var uiInGameView = UIManager.Instance.currentView as UIInGameView;
                uiInGameView.ShowMergeItemGlowFx(Vector3.zero, false);
            }
        }

     
        bool IsPuzzleOverlayInventoryBag(UIPuzzleItemController puzzleItem)
        {
            var uiIngame = UIManager.Instance.currentView as UIInGameView;
            var inventoryButton = uiIngame.inventoryButton;

            if (!inventoryButton.gameObject.activeSelf)
                return false;

            return (puzzleItem.transform as RectTransform).IsElementOverlap(inventoryButton.transform as RectTransform);

        }

        void PutPuzzleToBag(UIPuzzleItemController puzzleItem, out bool isSuccess)
        {
            //Pop the inventory button a little bit
            var uiIngameView = UIManager.Instance.currentView as UIInGameView;

            UserManager.Instance.EquipPuzzleInventorySlot(puzzleItem.data.puzzleId, puzzleItem.data.level, out isSuccess);

            if (!isSuccess)
                return;

            RemovePuzzle(puzzleItem);

            //Check for customer when remove dropped item
            if (puzzleItem is UIPuzzleNormalItemController droppedNormalItem)
            {
                InGameManager.Instance.customersController.RemoveOrder(droppedNormalItem);
            }

            SimpleObjectPool.Despawn(puzzleItem, moveToPoolContainer:false);

            var seq = DOTween.Sequence();
            seq.Append(uiIngameView.inventoryButton.transform.DOScale(1.2f, 0.15f).SetEase(Ease.Linear));
            seq.Append(uiIngameView.inventoryButton.transform.DOScale(1f, 0.15f).SetEase(Ease.Linear));
            
        }

        public List<UIPuzzleProducerController> GetAvailableProducerItems()
        {
            var result = new List<UIPuzzleProducerController>();

            for (int i = 0; i < boardSize.y; i++)
            {
                for (int j = 0; j < boardSize.x; j++)
                {
                    var item = board[i][j].puzzleItem;
                    if (item != null && item is UIPuzzleProducerController producer)
                    {
                        if (!producer.isBlock && producer.canProduce)
                            result.Add(producer);
                    }
                }
            }

            return result;
        }

        public UIPuzzleItemController SpawnPuzzleItem(string puzzleItemId, int level, int blockingLevel, TileInfo tileInfo)
        {
            var configPuzzle = ConfigManager.Instance.configPuzzle;
            var configPuzzleItem = configPuzzle.configItems.Find(s => s.id == puzzleItemId);
            var configPuzzleType = configPuzzle.configPuzzlePerTypes.Find(s => s.puzzleType == configPuzzleItem.puzzleType);
            var data = UserManager.Instance.AddNewPuzzle(puzzleItemId, level, blockingLevel, tileInfo.boardPosition);

            return SpawnPuzzleItem(configPuzzleItem, configPuzzleType, data, tileInfo);
        }

        UIPuzzleItemController SpawnPuzzleItem(ConfigPuzzleItem configPuzzleItem, ConfigPuzzleType configPuzzleType, UserPuzzleDataItem data, TileInfo tileInfo)
        {
            var uiPuzzle = SimpleObjectPool.Spawn(configPuzzleType.itemPrefab, groupPuzzleItemTrans);
            uiPuzzle.Setup(configPuzzleItem, configPuzzleType, data, tileInfo.worldPosition, tileSize);
            tileInfo.puzzleItem = uiPuzzle;
            RegisterItemListener(uiPuzzle);
            return uiPuzzle;
        }
       
        void RegisterItemListener(UIPuzzleItemController puzzleItem)
        {
            puzzleItem.onBeginDragItem -= OnBeginDragItem;
            puzzleItem.onBeginDragItem += OnBeginDragItem;

            puzzleItem.onDropItem -= OnDropPuzzleItem;
            puzzleItem.onDropItem += OnDropPuzzleItem;

            puzzleItem.onHoldItem -= OnHoldItem;
            puzzleItem.onHoldItem += OnHoldItem;
        }

        void RemoveItemListener(UIPuzzleItemController puzzleItem)
        {
            puzzleItem.onDropItem -= OnDropPuzzleItem;
            puzzleItem.onHoldItem -= OnHoldItem;
        }

        void OnBeginDragItem(UIPuzzleItemController puzzleItem)
        {
            beginDragItem = true;
            HideSuggesstion();
        }

        void OnDropPuzzleItem(UIPuzzleItemController droppedItem)
        {
            nextTimeSuggesstion = Time.time + ConfigManager.Instance.configGlobal.showSuggestionInterval;
            beginDragItem = false;

            var uiIngameView = UIManager.Instance.currentView as UIInGameView;
            uiIngameView.ShowMergeItemGlowFx(Vector3.zero, false);

            var inGameManager = InGameManager.Instance;
            var userManager = UserManager.Instance;

            //Check to move to nearest tile
            var puzzlePos = droppedItem.itemPosition;

            var nearestTile = GetNearestTile(puzzlePos);
            var droppedItemBoardPos = droppedItem.data.boardPosition;

            if (IsPuzzleOverlayInventoryBag(droppedItem))
            {
                PutPuzzleToBag(droppedItem, out bool isPutSuccess);

                if (isPutSuccess)
                    return;
            }

            if (nearestTile == null || nearestTile == board[droppedItemBoardPos.y][droppedItemBoardPos.x])
            {
                droppedItem.BackToOriginal();
                return;
            }

            //If is empty move to that tile
            if (nearestTile.isEmpty)
            {
                //Check tutorial;

                if (!userManager.finishTutorial)
                {
                    droppedItem.BackToOriginal();
                }
                else
                {
                    var tile = GetTileInfo(droppedItemBoardPos);
                    tile.puzzleItem = null;
                    droppedItem.MoveItemToTile(nearestTile);
                    userManager.boardData.SaveData();
                }
            }
            else
            {
                var nearestTileItem = nearestTile.puzzleItem;
                bool canMergePuzzle = nearestTileItem.data.blockingLevel >= 1 && nearestTileItem.data.level == droppedItem.data.level &&
                    !nearestTileItem.isMaxLevel && droppedItem.config.id == nearestTileItem.config.id; //Half block and tile == level

                //Check tutorial
                if (!userManager.finishTutorial)
                {
                    var config = TutorialManager.Instance.currentStep.config;

                    if (config is ConfigServeCustomer configServeCustomer)
                    {
                        if (!configServeCustomer.allowMovePuzzle)
                        {
                            canMergePuzzle = false;
                        }
                    }
                    else if (config is not ConfigTutorialPuzzleMove)
                    {
                        canMergePuzzle = false;
                    }
                }

                if (canMergePuzzle)
                {
                    RemovePuzzle(droppedItem);
                    SimpleObjectPool.Despawn(droppedItem, moveToPoolContainer: false);

                    //Check for customer when remove dropped item
                    if (droppedItem is UIPuzzleNormalItemController droppedNormalItem)
                    {
                        inGameManager.customersController.RemoveOrder(droppedNormalItem);
                    }

                    if (nearestTileItem.data.blockingLevel == 1) //Half block
                        nearestTileItem.BreakBlock();

                    int oldPuzzleLevel = nearestTileItem.data.level;

                    //Upgrade the puzzle
                    nearestTileItem.Upgrade();

                    //Also break neighbor fulled block
                    var fullBlockedNeighborTiles = GetNeighborTiles(nearestTile.boardPosition).FindAll(s => !s.isEmpty && s.puzzleItem.data.blockingLevel == 0);
                    foreach (var tile in fullBlockedNeighborTiles)
                    {
                        tile.puzzleItem.BreakBlock();
                    }

                    //Also check customer order here
                    if (nearestTileItem is UIPuzzleNormalItemController normalItem)
                    {
                        inGameManager.customersController.CheckCompleteOrder(normalItem, oldPuzzleLevel);
                    }

                    //Check producer item
                    if (nearestTileItem is UIPuzzleProducerController producerItem && !producerItem.isBlock && producerItem.canProduce)
                    {
                        if (!availableProducerConfigDic.ContainsKey(producerItem.config.id))
                            availableProducerConfigDic.Add(producerItem.config.id, ConfigManager.Instance.configPuzzle.configItems.Find(s => s.id == producerItem.config.id));
                    }

                    userManager.boardData.SaveData();
                    onPuzzleMerged?.Invoke(nearestTileItem);
                }
                else
                {
                    //Swap this tile with current tile
                    if (!nearestTileItem.isBlock)
                    {
                        droppedItem.SwapItemInTile(nearestTile);
                        userManager.boardData.SaveData();
                    }
                    else
                    {
                        droppedItem.BackToOriginal();
                    }
                }
            }
        }
        void GenerateBoard(Transform groupRowTrans)
        {
            var configPuzzle = ConfigManager.Instance.configPuzzle;
            boardSize = configPuzzle.boardSize;
            board = new TileInfo[boardSize.y][];
            bool isDark = true;
            var rowPrefab = configPuzzle.rowPrefab;
            var tileImagePrefab = configPuzzle.tileImagePrefab;

            Image[][] tempImgs = new Image[boardSize.y][];

            for (int irow = 0; irow < boardSize.y; irow++)
            {
                //Spawn the row first
                board[irow] = new TileInfo[boardSize.x];
                var rowIns = Instantiate(rowPrefab, groupRowTrans);
                rowIns.SetActive(true);

                tempImgs[irow] = new Image[boardSize.x];

                for (int icol = 0; icol < boardSize.x; icol++)
                {
                    var tileImage = Instantiate(tileImagePrefab, rowIns.transform);
                    tileImage.sprite = isDark ? configPuzzle.darkTileSprite : configPuzzle.lightTileSprite;
                    tileImage.gameObject.SetActive(true);
                    board[irow][icol] = new TileInfo(new BoardPosition() { x = icol, y = irow });
                    tempImgs[irow][icol] = tileImage;

                    isDark = !isDark;
                }

                tileRows.Add(rowIns);
            }

            //Force calculate all the board position
            LayoutRebuilder.ForceRebuildLayoutImmediate(groupRowTrans as RectTransform);

            for (int irow = 0; irow < boardSize.y; irow++)
            {
                for (int icol = 0; icol < boardSize.x; icol++)
                {
                    board[irow][icol].worldPosition = tempImgs[irow][icol].transform.position;
                }
            }

            tileSize = tempImgs[0][0].rectTransform.GetSize();

            RectTransformUtility.ScreenPointToLocalPointInRectangle(groupRowTrans as RectTransform, tempImgs[0][0].rectTransform.position, null, out var point);
            firstScreenPos = point;
        }

        void ShowSuggestion()
        {
            nextTimeSuggesstion = Time.time + ConfigManager.Instance.configGlobal.showSuggestionInterval;

            //Maybe only find the suggesstion when spawning new item/merge item
            if (mergeSuggestionItems.Count < 2)
            {
                //Find 2 merge suggestion item
                //Debug.Log("Find new item");
                for (int i = 0; i < boardSize.y; i++)
                {
                    for (int j = 0; j < boardSize.x; j++)
                    {
                        var item = board[i][j].puzzleItem;
                        if (item == null || item.isBlock || item.isMaxLevel)
                            continue;

                        if (item is UIPuzzleNormalItemController normalPuzzle)
                        {
                            if (normalPuzzle.hasOrder || normalPuzzle.pendingServe)
                                continue;
                        }

                        mergeSuggestionItems.Add(item);

                        if (mergeSuggestionItems.Count == 1)
                        {
                            //Use BFS search to search for the mergable item
                            var nearestMergableItem = GetNearestMergableItem(board[i][j].boardPosition, item);
                            if (nearestMergableItem != null)
                            {
                                mergeSuggestionItems.Add(nearestMergableItem);
                                break;
                            }
                            else
                                mergeSuggestionItems.Clear();
                        }
                    }

                    if (mergeSuggestionItems.Count == 2)
                        break;
                }
            }

            if (mergeSuggestionItems.Count < 2)
                mergeSuggestionItems.Clear();
            else
            {
                highlightTweens.ForEach(h => h.Kill());

                for (int i = 0; i < mergeSuggestionItems.Count; i++)
                {
                    var itemTrans = mergeSuggestionItems[i].transform;
                    mergeSuggestionItems[i].StopAllCoroutines();
                    highlightTweens.Add(itemTrans.DOScale(1.2f, 0.2f).SetEase(Ease.Linear).SetLoops(4, LoopType.Yoyo).From(1));
                }
            }
        }

        void HideSuggesstion()
        {
            highlightTweens.ForEach(h => h.Kill());
            highlightTweens.Clear();
            mergeSuggestionItems.ForEach(m => m.transform.localScale = Vector3.one);
            mergeSuggestionItems.Clear();
        }

        public class TileInfo
        {
            public Vector2 worldPosition;
            public readonly BoardPosition boardPosition;
            public UIPuzzleItemController puzzleItem;

            public TileInfo(BoardPosition boardPosition)
            {
                this.boardPosition = boardPosition;
                puzzleItem = null;
            }

            public bool isEmpty => puzzleItem == null;
        }

        void OnDestroy()
        {
            //Clear all the ui puzzle, because this is spawn on the UIIngameView, which is DDOL by default
            for (int irow = 0; irow < boardSize.y; irow++)
            {
                for (int icol = 0; icol < boardSize.x; icol++)
                {
                    var puzzle = board[irow][icol].puzzleItem;
                    if (puzzle != null)
                    {
                        Destroy(puzzle.gameObject);
                    }
                }
            }

            //Clear all the row
            foreach (var row in tileRows)
            {
                if (row != null)
                {
                    Destroy(row);
                }
            }

            SimpleObjectPool.ClearAll();
        }
    }
}
