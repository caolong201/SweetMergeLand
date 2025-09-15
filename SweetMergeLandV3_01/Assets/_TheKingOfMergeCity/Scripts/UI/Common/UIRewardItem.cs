using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheKingOfMergeCity
{
    using Config;

    public class UIRewardItem : MonoBehaviour
    {
        [SerializeField] Image rewardImage;
        [SerializeField] TMP_Text amountText;

        public ConfigRewardItem config { get; private set; }

        public void Setup(ConfigRewardItem config)
        {
            this.config = config;
            
            rewardImage.sprite = config.iconSprite;
            amountText.text = config.amount.ToString();
            gameObject.SetActive(true);
        }

        public Image GetRewardImage() => rewardImage;
    }
}
