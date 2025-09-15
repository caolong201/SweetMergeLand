using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using USimpFramework.UI;
using TMPro;

namespace TheKingOfMergeCity
{
    public class UIBootView : UIViewBase
    {
        [SerializeField] Image bgImage;
        [SerializeField] Slider progressSlider;
        [SerializeField] TMP_Text progressText;
        
        void Start()
        {
            BootManager.Instance.onProgressChanged += OnProgressChanged;

            progressSlider.onValueChanged.AddListener(val =>
            {
                progressText.text = $"Loading {Mathf.RoundToInt(progressSlider.value * 100)}%...";
            });           
            progressSlider.value = 0;
        }

        void OnDestroy()
        {
            BootManager.Instance.onProgressChanged -= OnProgressChanged;    
        }
        
        void OnProgressChanged()
        {
            progressSlider.value = BootManager.Instance.progress;
        }
    }
}
