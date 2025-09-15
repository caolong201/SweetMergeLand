using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace USimpFramework.UI.Extensions
{
    [RequireComponent(typeof(Button))]
    public abstract class UIButtonBase : MonoBehaviour
    {
        Button _button;
        public Button button
        {
            get
            {
                if (_button == null)
                    _button = GetComponent<Button>();

                return _button;
            }
        }

        public bool interactable
        {
            get => button.interactable;
            set
            {
                button.interactable = value;
                OnButtonInteractable();
            }
        }

        protected abstract void OnButtonInteractable();
    }
}
