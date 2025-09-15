using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace USimpFramework.UI.Extensions
{
    [RequireComponent(typeof(Button))]
    public class UITabButton : UIButtonBase
    {
        [SerializeField] protected GameObject _goRelatedContent;
        public GameObject goRelatedContent => _goRelatedContent;

        bool _isSelected;
        protected bool hasChanged;
        public bool isSelected
        {
            get => _isSelected;
            set
            {
                /*if (_isSelected == value)
                    return;*/

                hasChanged = _isSelected != value;
                
                _isSelected = value;
                OnSelected();
            }
        }

        public virtual void ResetData()
        {
            isSelected = false;
        }

        protected override void OnButtonInteractable()
        {
            
        }

        protected virtual void OnSelected()
        {
            button.interactable = !isSelected;

            if (goRelatedContent != null)
                goRelatedContent.SetActive(isSelected);

        }
    }
}
