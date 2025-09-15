using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace USimpFramework.UI
{
    public class UIConfig : ScriptableObject
    {
        static UIConfig _instance;
        static UIConfig instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<UIConfig>("UIConfig");

                return _instance;
            }
        }

        [Header("View")]
        [SerializeField] List<UIViewBase> _uiViewPrefabs;
        public static List<UIViewBase> uiViewPrefabs => instance._uiViewPrefabs;

        [Header("Popup")]
        [SerializeField] List<UIPopupBase> _uiPopupPrefabs;
        public static List<UIPopupBase> uiPopupPrefabs => instance._uiPopupPrefabs;

        [Header("Style template")]
        [SerializeField] GameObject _viewTemplate;
        public static GameObject viewTemplate => instance._viewTemplate;

        [SerializeField] GameObject _popupTemplate;
        public static GameObject popupTemplate => instance._popupTemplate;

        [SerializeField] UIFloatingTextController _uiFloatingTextController;
        public static UIFloatingTextController uiFloatingTextController => instance._uiFloatingTextController;

#if UNITY_EDITOR
        [Header("Script template")]
        [SerializeField] TextAsset _uiViewScriptTemplate;
        public static TextAsset uiViewScriptTemplate => instance._uiViewScriptTemplate;

        [SerializeField] TextAsset _uiPopupScriptTemplate;

        public static TextAsset uiPoupScriptTemplate => instance._uiPopupScriptTemplate;

        public static void SaveAsset()
        {
            UnityEditor.EditorUtility.SetDirty(instance);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(instance);
        }
#endif

    }
}
