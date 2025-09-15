using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using USimpFramework.UI;
using TMPro;

namespace TheKingOfMergeCity
{
    using Enum;

    public class UIBuyEnergyPopup : UIScalePopup
    {
        [SerializeField] Image energyImage;
        [SerializeField] TMP_Text energyRewardText;

        void Start()
        {
            energyRewardText.text = "+" + ConfigManager.Instance.configGlobal.energyRewardAfterWatchAd;    
        }

        public void PressWatchAd()
        {
            ApplovinManager.Instance.ShowRewardedAd(isSuccess =>
            {
                if (!isSuccess)
                {
                    UIManager.Instance.ShowFloatingText("Cannot watch ad! Please check your internet connection!");
                    return;
                }


                int amount = ConfigManager.Instance.configGlobal.energyRewardAfterWatchAd;
                UserManager.Instance.AddCurrencyAmount(CurrencyType.Energy, amount, true, allowOverMax: true);

                UIManager.Instance.HidePopup(this);

                var uiIngameVIew = UIManager.Instance.currentView as UIInGameView;
                if (uiIngameVIew != null)
                {
                    var uiTopbar = uiIngameVIew.uiTopBar;
                    uiTopbar.PlayCurrencyGachaEffect(CurrencyType.Energy, amount, energyImage.transform.position);
                }
            });
           
        }
        
        public void PressClose()
        {
            UIManager.Instance.HidePopup(this);
        }
    }
}
