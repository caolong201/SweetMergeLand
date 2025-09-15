using System.Collections;
using System.Collections.Generic;

using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheKingOfMergeCity
{
    using Config;
    using Enum;

    public class UIDailyRewardItem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Image containerBackgroundImage;
        [SerializeField] Image _rewardItemImage;
        public Image rewardItemImage => _rewardItemImage;

        [SerializeField] TMP_Text rewardAmountText;
        [SerializeField] TMP_Text dayText;
        [SerializeField] Image imageTick;
        [SerializeField] GameObject goAvailable;
        [SerializeField] GameObject goUnAvailable;

        [Header("Setting")]
        [SerializeField] Sprite activeSprite;
        [SerializeField] Sprite inActiveSprite;

        public RewardClaimState rewardClaimState { get; private set; }

        public ConfigDailyRewardItem config { get; private set; }
        
        public void Setup(ConfigDailyRewardItem config, int day)
        {
            this.config = config;

            rewardItemImage.sprite = config.iconSprite;
            rewardAmountText.text = config.amount.ToString();
            dayText.text = "Day " + day;
            
            gameObject.SetActive(true);
        }


        public void SetClaimState(RewardClaimState newState, bool withTransition)
        {
            rewardClaimState = newState;

            imageTick.gameObject.SetActive(false);

            goAvailable.SetActive(false);
            goUnAvailable.SetActive(false);

            if (newState == RewardClaimState.None)
            {
                containerBackgroundImage.sprite = inActiveSprite;
                goUnAvailable.SetActive(true);
            }
            else if (newState == RewardClaimState.PendingClaim)
            {
                containerBackgroundImage.sprite = activeSprite;
                goAvailable.SetActive(true);
            }
            else if (newState == RewardClaimState.Claimed)
            {
                containerBackgroundImage.sprite = activeSprite;
                imageTick.gameObject.SetActive(true);
                goAvailable.SetActive(true);

                if (withTransition)
                {
                    imageTick.transform.DOScale(1f, 0.6f).SetEase(Ease.OutSine).From(1.4f);

                    transform.DOScale(1.3f, 0.1f).SetEase(Ease.Linear).OnComplete(() => transform.DOScale(1f, 0.1f).SetEase(Ease.Linear));
                }
            }
        }

       
    }
}
