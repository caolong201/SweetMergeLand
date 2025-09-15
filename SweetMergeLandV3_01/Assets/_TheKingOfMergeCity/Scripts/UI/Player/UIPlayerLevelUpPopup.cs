using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using USimpFramework.UI;
using TMPro;
using DG.Tweening;


namespace TheKingOfMergeCity
{
    using Config;
    using USimpFramework.Animation.DOTweenExtension;

    public class UIPlayerLevelUpPopup : UIScalePopup
    {
        [Header("References")]
        [SerializeField] UIPlayerLevelUpRewardItem uiPlayerLevelUpRewardItemPrefab;
        [SerializeField] TMP_Text levelText;
        [SerializeField] TMP_Text emptyText;
        [SerializeField] TMP_Text claimButtonText;
        
        
        [Header("Animation")]
        [SerializeField] List<Transform> starTrans;
        [SerializeField] Transform labelTrans;
        
        List<UIPlayerLevelUpRewardItem> uiRewardItems = new();

        void Start()
        {
            uiPlayerLevelUpRewardItemPrefab.gameObject.SetActive(false);
        }

        public void Setup(List<ConfigFeatureItem> configFeatures, List<ConfigRewardItem> configRewards)
        {
            if (configFeatures.Count == 0 && configRewards.Count == 0)
            {
                emptyText.gameObject.SetActive(true);
                claimButtonText.text = "Close";
                return;
            }
            else
            {
                emptyText.gameObject.SetActive(false);
                claimButtonText.text = "Claim";
            }

            foreach (var config in configFeatures)
            {
                var ui = GetOrCreateUI();
                ui.Setup(config.iconSprite, -1);
            }

        }

      
        UIPlayerLevelUpRewardItem GetOrCreateUI()
        {
            var ui = uiRewardItems.Find(c => !c.gameObject.activeSelf);
            if (ui == null)
            {
                ui = Instantiate(uiPlayerLevelUpRewardItemPrefab, uiPlayerLevelUpRewardItemPrefab.transform.parent);
                uiRewardItems.Add(ui);
            }

            return ui;
        }

        public override void Show(bool withTransition = true, Action onCompleted = null)
        {
            base.Show(withTransition, onCompleted);

            levelText.text = UserManager.Instance.currentPlayerLevel.ToString();

            PlayAnimation();
        }

        void PlayAnimation()
        {

            StartCoroutine(CR_PlayAnimation());

            IEnumerator CR_PlayAnimation()
            {
                foreach (var star in starTrans)
                {
                    star.gameObject.SetActive(false);
                }
               
                var tween = labelTrans.DOScaleX(1, 0.4f).SetEase(Ease.OutSine).From(0).SetDelay(0.2f);
                yield return tween.WaitForCompletion();

                var mid = starTrans[1];
                mid.DOScale(1, 0.2f).SetEase(Ease.InSine).From(1.6f).OnStart(() => mid.gameObject.SetActive(true));

                var left = starTrans[0];
                left.DOScale(1, 0.2f).SetEase(Ease.InSine).From(1.6f).SetDelay(0.2f).OnStart(() => left.gameObject.SetActive(true));

                var right = starTrans[2];
                var tween2 = right.DOScale(1, 0.2f).SetEase(Ease.InSine).From(1.6f).SetDelay(0.4f).OnStart(() => right.gameObject.SetActive(true));
            }

        }

        void OnDisable()
        {
            uiRewardItems.ForEach(ui => ui.gameObject.SetActive(false));
        }

        public void PressClaim()
        {
            //Todo: Fly the unlock item to the corresponding
            var userManager = UserManager.Instance;
            
            userManager.isPendingPlayPlayerLevelUp = false;
            UIManager.Instance.HidePopup(this);

            //Claim the reward



            if (userManager.isPlayingDecoBuildingFromInGameScene)
            {
                userManager.isPlayingDecoBuildingFromInGameScene = false;
                BootManager.Instance.LoadScene(SceneConstants.IN_GAME_SCENE_NAME, true);
            }
        }

    }
}
