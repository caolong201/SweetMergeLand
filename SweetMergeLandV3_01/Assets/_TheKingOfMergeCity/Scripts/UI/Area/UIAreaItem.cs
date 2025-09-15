using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TheKingOfMergeCity
{
    using Config;
    using Model;
    using Enum;

    public class UIAreaItem : MonoBehaviour
    {
        [SerializeField] TMP_Text areaNameText;
        [SerializeField] Image areaImage;
        [SerializeField] Slider areaProgressSlider;
        [SerializeField] TMP_Text areaProgressText;
        [SerializeField] GameObject goLock;
        [SerializeField] GameObject goCompleted;
        [SerializeField] Button viewAreaButton;
        [SerializeField] GameObject _goSeparator;
        public GameObject goSeparator => _goSeparator;

        public ConfigAreaItem config { get; private set; }

        public DecoItemState areaItemState { get; private set; }

        public void Setup(ConfigAreaItem config, UserAreaData data)
        {
            this.config = config;

            areaNameText.text = config.displayName;
            areaImage.sprite = config.iconSprite;
            goLock.SetActive(false);
            goCompleted.SetActive(false);
            areaProgressSlider.gameObject.SetActive(false);
            areaImage.material = null;
            viewAreaButton.gameObject.SetActive(false);

            if (data != null)
            {
                if (data.completedDecoIds.Count == config.decoItems.Count) //Completed
                {
                    goCompleted.SetActive(true);
                    areaItemState = DecoItemState.Completed; //Todo: play complete transition
                }
                else //Progress
                {
                    areaProgressSlider.gameObject.SetActive(true);
                    areaProgressSlider.value = (float)data.completedDecoIds.Count / config.decoItems.Count;
                    areaProgressText.text = $"{data.completedDecoIds.Count}/{config.decoItems.Count}";
                    areaItemState = DecoItemState.Unlock; //Todo: Play unlock transition
                }

                if (UserManager.Instance.currentSelectAreaData != data)
                {
                    viewAreaButton.gameObject.SetActive(true);
                }
            }
            else
            {
                goLock.SetActive(true);
                areaImage.material = ConfigManager.Instance.configGlobal.grayMat;
                //areaImage.sprite = config.iconSpriteDisable;
                areaItemState = DecoItemState.Lock;
            }

            gameObject.SetActive(true);
        }
    }
}
