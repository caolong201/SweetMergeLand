using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheKingOfMergeCity
{
    using Model;
    using Config;
    using DG.Tweening;

    public class UIPuzzleInventorySlotItem : MonoBehaviour
    {
        [SerializeField] Image puzzleItemImage;
        [SerializeField] GameObject goLocked;
        [SerializeField] GameObject goUnlocked;
        [SerializeField] TMP_Text unlockCostText;
        
        public bool isLocked => unlockCost > 0;

        /// <summary> Temp slot id when this slot is locked  </summary>
        public int tempSlotId { get; private set; }
        
        public int unlockCost { get; private set; }

        public UserPuzzleInventoryDataItem data { get; private set; }

        public void UpdateVisual()
        {
            if (string.IsNullOrEmpty(data.puzzleId))
            {
                puzzleItemImage.gameObject.SetActive(false);
            }
            else
            {
                var config = ConfigManager.Instance.configPuzzle.configItems.Find(c => data.puzzleId == c.id);
                var configPerLevel = config.configPerLevel[data.level];
                puzzleItemImage.gameObject.SetActive(true);
                puzzleItemImage.sprite = configPerLevel.itemSprite;
            }
        }

        /// <summary>
        /// Set data for slot
        /// </summary>
        /// <param name="data">If slot.puzzleId is empty, mean this is empty</param>
        public void SetData(UserPuzzleInventoryDataItem data)
        {
            this.data = data;

            UpdateVisual();

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Set Lock for slot
        /// </summary>
        /// <param name="unlockCost">if unlock cost <= 0, mean this slot is unlocked</param>
        public void SetLock(int unlockCost, int tempSlotId)
        {
            this.tempSlotId = tempSlotId;
            this.unlockCost = unlockCost;

            goLocked.SetActive(false);
            goUnlocked.SetActive(false);

            if (unlockCost <= 0)
            {
                goUnlocked.SetActive(true);
            }
            else
            {
                goLocked.SetActive(true);
                unlockCostText.text = unlockCost.ToString("N0");
            }

            gameObject.SetActive(true);
        }

        public void ScaleUp()
        {
            transform.DOKill();
            var seq = DOTween.Sequence();
            seq.Append(transform.DOScale(1.2f, 0.15f).SetEase(Ease.Linear));
            seq.Append(transform.DOScale(1f, 0.15f).SetEase(Ease.Linear));
        }
    }
}

