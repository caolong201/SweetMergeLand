using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace TheKingOfMergeCity
{
    public class UIPlayerLevelUpRewardItem : MonoBehaviour
    {
        [SerializeField] Image imageIcon;
        [SerializeField] TMP_Text rewardAmountText;
        [SerializeField] TMP_Text unlockText;

        public bool isFeatureUnlocked { get; private set; }

        public void Setup(Sprite iconSprite, int amount)
        {
            imageIcon.sprite = iconSprite;

            isFeatureUnlocked = amount < 0;

            rewardAmountText.gameObject.SetActive(false);
            unlockText.gameObject.SetActive(false);

            if (isFeatureUnlocked)
            {
                unlockText.gameObject.SetActive(true);
            }
            else
            {
                rewardAmountText.text = amount.ToString();
            }

            gameObject.SetActive(true);
        }
    }
}
