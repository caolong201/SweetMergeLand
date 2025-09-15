using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

namespace TheKingOfMergeCity
{
    using Config;

    public class UIFoodOrderItem : MonoBehaviour
    {
        [SerializeField] Image itemIconImage;
        [SerializeField] Image imageTickCompleted;

        bool _isCompleted;
        public bool isCompleted
        {
            get => _isCompleted;
            set
            {
                _isCompleted = value;
                imageTickCompleted.gameObject.SetActive(value);
            }
        }

        //public ConfigOrderItem config { get; private set; }

        public string itemId { get; private set; }
        public int level { get; private set; }

        public void ShowIconImage(bool isShow)
        {
            itemIconImage.enabled = isShow;
        }

        public void Setup(string itemId, int level, bool isCompleted)
        {

            ResetData();

            this.itemId = itemId;
            this.level = level;
            this.isCompleted = isCompleted;

            var configFood = ConfigManager.Instance.configPuzzle.configItems.Find(s => s.id == itemId);
            if (configFood == null)
                throw new UnityException($"Invalid config with id {itemId} ");

            itemIconImage.sprite = configFood.configPerLevel[level].itemSprite;

            imageTickCompleted.gameObject.SetActive(isCompleted);
            gameObject.SetActive(true);
        }


        void ResetData()
        {
            ShowIconImage(true);
        }
    }
}
