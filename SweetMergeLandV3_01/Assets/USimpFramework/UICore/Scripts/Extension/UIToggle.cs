using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace USimpFramework.UI.Extensions
{
    [RequireComponent(typeof(Toggle))]
    public class UIToggle : MonoBehaviour
    {
        [SerializeField] Image backgroundImage;
        [SerializeField] Image handleImage;
        [SerializeField] Sprite backgroundOnSprite;
        [SerializeField] Sprite backgroundOffSprite;
        [SerializeField] Color colorOnSprite;
        [SerializeField] Color colorOffSprite;

        public bool isOn
        {
            get => toggle.isOn;
            set => toggle.isOn = value;
        }

        Toggle _toggle;
        Toggle toggle
        {
            get
            {
                if (_toggle == null)
                    _toggle = GetComponent<Toggle>();

                return _toggle;
            }
        }


        void Awake()
        {
            toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        void OnToggleValueChanged(bool value)
        {
            if (backgroundImage != null)
                backgroundImage.sprite = value ? backgroundOnSprite : backgroundOffSprite;

            if (handleImage != null)
            {
                var rectTransform = handleImage.rectTransform;
                if (value)
                {
                    handleImage.color = colorOnSprite;
                    rectTransform.anchorMin = new Vector2(1, 0);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.anchoredPosition = Vector2.zero;

                }
                else
                {
                    handleImage.color = colorOffSprite;
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = new Vector2(0, 1f);
                    rectTransform.anchoredPosition = Vector2.zero;
                }
            }
        }
    }
}
