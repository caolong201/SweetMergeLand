using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using USimpFramework.UIGachaEffect;
using USimpFramework.Utility;
using USimpFramework.UI;
using USimpFramework.Animation.DOTweenExtension;

using Coffee.UIExtensions;
using DG.Tweening;


namespace TheKingOfMergeCity
{
    using Enum;
   

    public class UIDailyRewardPopup : UIScalePopup
    {
        [SerializeField] UIDailyRewardItem normalItemPrefab;
        [SerializeField] UIDailyRewardItem specialItemPrefab;
        [SerializeField] Transform rowNormalItemPrefab;
        [SerializeField] Button claimButton;
        [SerializeField] Image flyingPuzzleImagePrefab;
        [SerializeField] UIParticle vfxPendingClaim;
        
        List<UIDailyRewardItem> uiDailyRewardItems = new();
        List<Transform> rowNormals = new();
        int curNormalPerRow = 0;

        void Start()
        {
            normalItemPrefab.gameObject.SetActive(false);
            specialItemPrefab.gameObject.SetActive(false);
            rowNormalItemPrefab.gameObject.SetActive(false);
            flyingPuzzleImagePrefab.gameObject.SetActive(false);

            var dailyReward = UserManager.Instance.dailyRewardData.value;
            bool hasPendingReward = UserManager.Instance.hasPendingClaimReward;
            var configPack = ConfigManager.Instance.configDailyReward.dailyRewardItems[dailyReward.currentDailyRewardPackId];

            int count = 0;
            while (uiDailyRewardItems.Count < configPack.rewards.Count)
            {
                var configReward = configPack.rewards[count++];
                var uiPrefab = configReward.isSpecial ? specialItemPrefab : normalItemPrefab;
                var uiIns = Instantiate(uiPrefab, uiPrefab.transform.parent);
                uiDailyRewardItems.Add(uiIns);
            }

            uiDailyRewardItems.ForEach(ui => ui.gameObject.SetActive(false));

            for (int i = 0; i < configPack.rewards.Count; i++)
            {
                //Todo: It's not guarantee that the ui list index is the same as config pack rewards
                var configReward = configPack.rewards[i];
                var uiDailyReward = uiDailyRewardItems[i];

                if (!configReward.isSpecial)
                {
                    //Set up the row prefab
                    if (curNormalPerRow == 0 || curNormalPerRow % 3 == 0)
                    {
                        var rowIns = Instantiate(rowNormalItemPrefab, rowNormalItemPrefab.transform.parent);
                        rowIns.gameObject.SetActive(true);
                        rowIns.SetSiblingIndex(curNormalPerRow / 3);
                        rowNormals.Add(rowIns);
                    }
                    uiDailyReward.transform.SetParent(rowNormals[curNormalPerRow / 3]);
                    curNormalPerRow++;
                }

                uiDailyReward.Setup(configReward, i + 1);
                int next = hasPendingReward ? dailyReward.currentClaimIndex + 1 : -1;
                var claimState = i <= dailyReward.currentClaimIndex ? RewardClaimState.Claimed : (i == next ? RewardClaimState.PendingClaim : RewardClaimState.None);
                uiDailyReward.SetClaimState(claimState, false);

                if (claimState == RewardClaimState.PendingClaim)
                {
                    vfxPendingClaim.transform.SetParent(uiDailyReward.transform);
                    vfxPendingClaim.transform.localPosition = Vector2.zero;
                    vfxPendingClaim.transform.SetAsFirstSibling();
                    vfxPendingClaim.Play();
                }
            }

            claimButton.gameObject.SetActive(hasPendingReward);

        }

        public void PressClose()
        {
            UIManager.Instance.HidePopup(this);
        }

        public void PressClaim()
        {
            //Validate
            var dailyReward = UserManager.Instance.dailyRewardData.value;
            var uiDailyRewardItem = uiDailyRewardItems.Find(c => c.rewardClaimState == RewardClaimState.PendingClaim);
    
            if (uiDailyRewardItem == null)
                return;
            
            int oldIndex = dailyReward.currentClaimIndex;
            var result = UserManager.Instance.ClaimDailyReward();
            if (!result)
                return;

            claimButton.gameObject.SetActive(false);

            //Show effect for reward claimed
            uiDailyRewardItem.SetClaimState(RewardClaimState.Claimed, true);

            vfxPendingClaim.StopEmission();

            var configReward = uiDailyRewardItem.config;

            if (configReward.rewardType == RewardType.Currency)
            {
                var uiTopBar = (UIManager.Instance.currentView as UIHomeView).uiTopBar;

                float sizeScale = configReward.currencyType == CurrencyType.Energy ? 1 : 1.5f;

                uiTopBar.PlayCurrencyGachaEffect(configReward.currencyType, configReward.amount, uiDailyRewardItem.transform.position, sizeScale, () =>
                {
                    UIManager.Instance.HidePopup(this);
                });
            }
            else if (configReward.rewardType == RewardType.PuzzleItem)
            {

                //Fly the puzzle item to the play button
                var flyingPuzzle = SimpleObjectPool.Spawn(flyingPuzzleImagePrefab, UIGachaEffect.Instance.transform);
                flyingPuzzle.sprite = uiDailyRewardItem.rewardItemImage.sprite;
                flyingPuzzle.rectTransform.SetSize(uiDailyRewardItem.rewardItemImage.rectTransform.GetSize());
                flyingPuzzle.gameObject.SetActive(true);
                flyingPuzzle.transform.position = uiDailyRewardItem.rewardItemImage.transform.position;

                float moveDuration = 0.4f;
                float moveToPlayButtonDuration = 0.6f;

                var seq = DOTween.Sequence();
                var startPos = flyingPuzzle.transform.position + new Vector3(80, 80);
                var playButton = (UIManager.Instance.currentView as UIHomeView).playButton;
                UIManager.Instance.SetInteraction(false);

                DOVirtual.DelayedCall(0.2f, () => UIManager.Instance.HidePopup(this));

                seq.Append(flyingPuzzle.transform.DOMove(startPos, moveDuration).SetEase(Ease.OutQuad));
                seq.Append(flyingPuzzle.transform.DOMove(playButton.transform.position, moveToPlayButtonDuration).SetEase(Ease.InQuad).OnStart(() =>
                {
                    flyingPuzzle.transform.DOScale(0.2f, moveToPlayButtonDuration).SetEase(Ease.InQuad);
                }));
                seq.OnComplete(() =>
                {
                    playButton.transform.DOJellyPop(1.2f,0.2f);
                    SimpleObjectPool.Despawn(flyingPuzzle, moveToPoolContainer: false);
                    UIManager.Instance.SetInteraction(true);
                });
                
            }
        }
    }
}
