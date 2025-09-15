
using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

using USimpFramework.Animation.DOTweenExtension;

namespace TheKingOfMergeCity
{
    using Config;
    
    public class UIRouletteRewardPopup : MonoBehaviour
    {
        [SerializeField] UIRewardItem _uiRewardItem;
        public UIRewardItem uiRewardItem => _uiRewardItem;
        [SerializeField] Button claimButton;

        [Header("Animation")]
        [SerializeField] float showDuration;
        [SerializeField] Transform boardTrans;
        [SerializeField] CanvasGroup canvasGroup;

        public ConfigRewardItem configReward { get; private set; }


        public void Show(ConfigRewardItem configReward)
        {
            this.configReward = configReward;
            gameObject.SetActive(true);
            claimButton.interactable = true;
            uiRewardItem.Setup(configReward);
            boardTrans.DOPopIn(showDuration);
            canvasGroup.DOFade(1, showDuration).From(0).SetEase(Ease.Linear);   
        }

        public void Hide()
        {
            boardTrans.DOPopOut(showDuration);
            canvasGroup.DOFade(0, showDuration).From(1).SetEase(Ease.Linear).OnComplete(() =>
            {
                gameObject.SetActive(false);
            });

        }

       
    }
}
