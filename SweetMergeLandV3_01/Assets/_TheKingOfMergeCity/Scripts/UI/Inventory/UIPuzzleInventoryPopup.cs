using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using USimpFramework.UI;
using TMPro;
using USimpFramework.Utility;
using DG.Tweening;

namespace TheKingOfMergeCity
{
    public class UIPuzzleInventoryPopup : UIScalePopup
    {
        [SerializeField] UIPuzzleInventorySlotItem uiPuzzleInventorySlotItemPrefab;
        [SerializeField] Transform uiRowSlotPrefab;

        List<UIPuzzleInventorySlotItem> uiPuzzleInventorySlotItems = new();
        List<Transform> rowSlots = new();

        void OnEnable()
        {
            var configPuzzleInventory = ConfigManager.Instance.configPuzzleInventory;
          
            //Spawn row slots first
            if (rowSlots.Count == 0)
            {
                uiPuzzleInventorySlotItemPrefab.gameObject.SetActive(false);
                uiRowSlotPrefab.gameObject.SetActive(false);

                int rowCount = Mathf.CeilToInt(configPuzzleInventory.configSlots.Count / configPuzzleInventory.slotPerRow);
                for (int i = 0; i < rowCount; i++)
                {
                    var ui = Instantiate(uiRowSlotPrefab, uiRowSlotPrefab.parent);
                    ui.gameObject.SetActive(true);
                    rowSlots.Add(ui);
                }
            }


            //Init slot datas
            var slotDataItems = UserManager.Instance.puzzleInventoryItems;
            while (uiPuzzleInventorySlotItems.Count < slotDataItems.Count)
            {
                var ui = Instantiate(uiPuzzleInventorySlotItemPrefab, uiPuzzleInventorySlotItemPrefab.transform.parent);
                uiPuzzleInventorySlotItems.Add(ui);
            }

            uiPuzzleInventorySlotItems.ForEach(ui => ui.gameObject.SetActive(false));

            for (int i = 0; i < slotDataItems.Count; i++)
            {
                var data = slotDataItems[i];

                //Calculate the parent and set the parent
                int rowIndex = data.slotId / configPuzzleInventory.slotPerRow;
                var ui = uiPuzzleInventorySlotItems[i];
                ui.SetData(data);
                ui.transform.SetParent(rowSlots[rowIndex]);
            }

            //Spawn the the last locked slot (if have)
            SpawnNextUnlock();
        }

        void SpawnNextUnlock()
        {
            var configPuzzleInventory = ConfigManager.Instance.configPuzzleInventory;
            var slotDataItems = UserManager.Instance.puzzleInventoryItems;

            //Spawn the the last locked slot (if have)
            if (uiPuzzleInventorySlotItems.Count < configPuzzleInventory.configSlots.Count)
            {
                var nextSlotId = slotDataItems.Count;

                int rowIndex = nextSlotId / configPuzzleInventory.slotPerRow;
                var ui = Instantiate(uiPuzzleInventorySlotItemPrefab, rowSlots[rowIndex]);

                ui.SetLock(configPuzzleInventory.configSlots[nextSlotId].unlockCost, nextSlotId);
                uiPuzzleInventorySlotItems.Add(ui);
            }
        }

        public void PressClose()
        {
            UIManager.Instance.HidePopup(this);
        }

        public void PressUIPuzzleInventorySlot(UIPuzzleInventorySlotItem ui)
        {
            if (ui.isLocked)
                return;

            var puzzlesController = InGameManager.Instance.puzzlesController;
            var firstEmptyTile = puzzlesController.GetFirstEmptyTile();
            if (firstEmptyTile == null)
            {
                ui.ScaleUp();
                UIManager.Instance.ShowFloatingText("Board is full");
                return;
            }
           
            var puzzleItem = puzzlesController.SpawnPuzzleItem(ui.data.puzzleId, ui.data.level, 2, firstEmptyTile);

            puzzleItem.OnPointerClick(null);
            puzzleItem.transform.position = firstEmptyTile.worldPosition;

            if (puzzleItem is UIPuzzleNormalItemController normalItem)
            {
                //Also must check complete order when put back the puzzle
                InGameManager.Instance.customersController.CheckCompleteOrder(normalItem, -1);
            }
           
            UserManager.Instance.UnEquipPuzzleInventorySlot(ui.data.slotId);

            ui.UpdateVisual();
        }

        public void PressUnlockNewSlot(UIPuzzleInventorySlotItem ui)
        {
            if (!ui.isLocked)
                return;
            
            UIManager.Instance.MessageBox("CONRFIRM", "Do want to use gem to unlock this slot", true, () =>
            {
                bool isSuccess = UserManager.Instance.UnlockSlot(ui.tempSlotId);

                if (isSuccess)
                {
                    //Update the newly unlocked ui
                    var data = UserManager.Instance.puzzleInventoryItems.Find(c => c.slotId == ui.tempSlotId);
                    if (data == null)
                        throw new UnityException("Something wrong! Cannot update data with id:  " + ui.tempSlotId);

                    ui.SetLock(0, -1);
                    ui.SetData(data);

                    SpawnNextUnlock();
                }
                else
                {
                    UIManager.Instance.ShowFloatingText("Insufficient Gem");
                }
            });
        }
        
    }
}
