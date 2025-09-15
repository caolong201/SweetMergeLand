using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheKingOfMergeCity
{
    using Config;
    using Enum;

    public class UIDecorationInfo : MonoBehaviour
    {
        [SerializeField] Image decoIconImage;
        [SerializeField] TMP_Text descriptionText;
        [SerializeField] TMP_Text costText;
        [SerializeField] Button _doItButton;
        public Button doItButton => _doItButton;            

        public ConfigDecoItem config { get; private set; }

        public void Setup(ConfigDecoItem config)
        {
            this.config = config;

            decoIconImage.sprite = config.iconSprite;
            descriptionText.text = config.description;
            costText.text = config.buildingCost.ToString("N0");
            doItButton.transition = Selectable.Transition.SpriteSwap;
            doItButton.interactable = UserManager.Instance.GetCurrencyBalance(CurrencyType.Star) >= config.buildingCost;
            gameObject.SetActive(true);
        }

    }
}
