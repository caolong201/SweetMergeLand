using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using USimpFramework.EditorExtension;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
#endif

namespace USimpFramework.UI.Extensions
{
    public class UITabView<TButton> : MonoBehaviour where TButton: UITabButton
    {
        public event System.Action<TButton> onSelectedButtonChanged;

        [SerializeField] List<TButton> _tabButtons;
        public List<TButton> tabButtons => _tabButtons;

        [Tooltip("starting index button when first show the tab view")]
        [SerializeField] int startingIndex;

        public  TButton selectedButton { get; protected set; }

        /// <summary> Must call setup outside of this class </summary>
        public virtual void Setup()
        {
            if (tabButtons.Count == 0)
                return;

            ResetData();
            SelectTab(tabButtons[startingIndex]);
        }
       
        public virtual void SelectTab(TButton button)
        {
            var old = selectedButton;
    
            if (selectedButton != null)
                selectedButton.isSelected = false;

            selectedButton = button;
            selectedButton.isSelected = true;

            onSelectedButtonChanged?.Invoke(old);
        }

        ///<summary>Called before selecting tab </summary>
        protected virtual void ResetData()
        {
            tabButtons.ForEach(b => b.ResetData());
        }

#if UNITY_EDITOR
        [SimpleInspectorButton("Setup Button Click Event")]
        protected void SetupButtonClickEvent()
        {
            foreach (var button in tabButtons)
            {
                UnityEventTools.AddObjectPersistentListener(button.button.onClick, SelectTab, button);
                EditorUtility.SetDirty(button.button);
            }
        }
        #endif
    }
}
