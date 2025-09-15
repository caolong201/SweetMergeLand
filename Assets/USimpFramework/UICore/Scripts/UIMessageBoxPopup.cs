using System.Collections;
using System.Collections.Generic;

using TheKingOfMergeCity;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace USimpFramework.UI
{
    public class UIMessageBoxPopup : UIScalePopup
    {
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text contentText;
        [SerializeField] Button confirmButton;

        public void Setup(string title, string content, bool hasConfirmButton  = false, System.Action onPressConfirm = null)
        {
            titleText.text = title;
            contentText.text = content;

            confirmButton.onClick.RemoveAllListeners();
            confirmButton.interactable = true;
            if (hasConfirmButton)
            {
                confirmButton.gameObject.SetActive(true);
                confirmButton.onClick.AddListener(() =>
                {
                    confirmButton.interactable = false;
                    onPressConfirm?.Invoke();
                    
                    UIManager.Instance.HidePopup(this);
                });
            }
            else
            {
                confirmButton.gameObject.SetActive(false);
            }
        }

        public void PressClose()
        {
            UIManager.Instance.HidePopup(this);
        }
    }
}
