using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;

namespace USimpFramework.UI.Editor
{
    public class UIManagerEditorWindow : EditorWindow
    {
        enum UIElementType
        {
            None = 0,
            View = 1,
            Popup = 2
        }

        static readonly string UI_CONFIG_PATH = "Assets/USimpFramework/UICore/Resources/UIConfig.asset";

        static readonly string UI_VIEW_SCRIPT_TEMPLATE_PATH = "Assets/USimpFramework/UICore/ScriptTemplates/80-UI C# Script__UI View Script-NewUIViewScript.cs.txt";
        static readonly string UI_POPUP_SCRIPT_TEMPLATE_PATH = "Assets/USimpFramework/UICore/ScriptTemplates/80-UI C# Script__UI Popup Script-NewUIPopupScript.cs.txt";

        static readonly string UI_SCRIPT_DIRECTORY_DATA_KEY = "UI_SCRIPT_DIRECTORY_KEY";
        static readonly string UI_PREFAB_DIRECTOY_DATA_KEY = "UI_PREFAB_DIRECTORY_KEY";
        static readonly string UI_PREFAB_NAME_DATA_KEY = "UI_PREFAB_NAME_KEY";
        static readonly string UI_SCRIPT_NAMESPACE_DATA_KEY = "UI_SCRIPT_NAMESPACE_KEY";

        [MenuItem("Tools/USimpFramework/UI/Generate UI ")]
        public static void ShowGenerateUIWindow()
        {
            var generateUIWindow = GetWindow<UIManagerEditorWindow>("UI Generate Editor");
            generateUIWindow.Show();
            uiConfig = AssetDatabase.LoadAssetAtPath<UIConfig>(UI_CONFIG_PATH);
        }

        //Make it static to save the last data
        static string uiPrefabDirectory = "Assets/Prefabs/UI";
        static string uiScriptDirectory = "Assets/Scripts/UI";
        static UIConfig uiConfig;
        static string uiPrefabName = "";
        static string scriptNamespace = "";

        bool useTemplate = true;
        UIElementType uiElementType = UIElementType.View;

        void OnGUI()
        {
            uiPrefabName = EditorGUILayout.TextField("UI Name", uiPrefabName);

            scriptNamespace = EditorGUILayout.TextField("Script Namespace", scriptNamespace);

            useTemplate = EditorGUILayout.Toggle("Use template", useTemplate);

            uiElementType = (UIElementType)EditorGUILayout.EnumPopup("UI Element Type", uiElementType);

            if (useTemplate)
            {
                if (GUILayout.Button("Locate UI Config"))
                {
                    if (uiConfig == null)
                        AssetDatabase.LoadAssetAtPath<UIConfig>(UI_CONFIG_PATH);
                    Selection.activeObject = uiConfig;
                }
            }

            uiPrefabDirectory = DrawFileDirectory("Prefab Directory", uiPrefabDirectory);
            uiScriptDirectory = DrawFileDirectory("Script Directory", uiScriptDirectory);

            if (GUILayout.Button("Generate UI"))
            {
                if (string.IsNullOrEmpty(uiPrefabDirectory) || string.IsNullOrEmpty(uiScriptDirectory) || string.IsNullOrEmpty(uiPrefabName) || uiElementType == UIElementType.None || string.IsNullOrEmpty(scriptNamespace))
                {
                    Debug.LogError("Invalid arguments! Please check again!");
                    return;
                }
                Debug.Log("Generating UI...");

                //Creating prefab at path
                var prefab = useTemplate ? (uiElementType == UIElementType.Popup ? UIConfig.popupTemplate : UIConfig.viewTemplate) : new GameObject();

                var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                //We need to unpack instance of prefab to avoid creating prefab variant
                PrefabUtility.UnpackPrefabInstance(instance, PrefabUnpackMode.Completely, InteractionMode.UserAction);

                string prefabLocalPath = $"{uiPrefabDirectory}/{uiPrefabName}.prefab";
                prefabLocalPath = AssetDatabase.GenerateUniqueAssetPath(prefabLocalPath);
                var asset = PrefabUtility.SaveAsPrefabAsset(instance, prefabLocalPath);

                DestroyImmediate(instance);

                asset.name = uiPrefabName;
                asset.layer = LayerMask.NameToLayer("UI");

                //Create a file script at path, now just add the component
                if (uiElementType == UIElementType.Popup)
                {
                    CreateScriptAsset(UI_POPUP_SCRIPT_TEMPLATE_PATH, uiPrefabName + ".cs");

                }
                else if (uiElementType == UIElementType.View)
                {
                    CreateScriptAsset(UI_VIEW_SCRIPT_TEMPLATE_PATH, uiPrefabName + ".cs");
                }

                EditorPrefs.SetString(UI_PREFAB_NAME_DATA_KEY, uiPrefabName);
                EditorPrefs.SetString(UI_SCRIPT_DIRECTORY_DATA_KEY, uiScriptDirectory);
                EditorPrefs.SetString(UI_PREFAB_DIRECTOY_DATA_KEY, uiPrefabDirectory);
                EditorPrefs.SetString(UI_SCRIPT_NAMESPACE_DATA_KEY, scriptNamespace);

                PrefabUtility.SavePrefabAsset(asset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log($"Create {uiPrefabName} success!");
            }
        }

        string DrawFileDirectory(string title, string path)
        {

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(title, GUILayout.MaxWidth(100));
            EditorGUILayout.LabelField(path, GUILayout.MaxWidth(400));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Browse"))
            {
                path = EditorUtility.OpenFolderPanel("Select", "", path);

                if (!string.IsNullOrEmpty(path) && path.IndexOf("Asset") > 0)
                    path = path.Substring(path.IndexOf("Assets"));

            }

            EditorGUILayout.EndHorizontal();

            return path;
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void CreateAssetWhenReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall -= CreateAssetWhenReady;
                EditorApplication.delayCall += CreateAssetWhenReady;
                return;
            }

            EditorApplication.delayCall -= OnChanged;
            EditorApplication.delayCall += OnChanged;
        }

        static void OnChanged()
        {
            if (EditorPrefs.HasKey(UI_PREFAB_NAME_DATA_KEY) && EditorPrefs.HasKey(UI_SCRIPT_DIRECTORY_DATA_KEY) && EditorPrefs.HasKey(UI_PREFAB_DIRECTOY_DATA_KEY) && EditorPrefs.HasKey(UI_SCRIPT_NAMESPACE_DATA_KEY))
            {
                uiPrefabName = EditorPrefs.GetString(UI_PREFAB_NAME_DATA_KEY);
                uiScriptDirectory = EditorPrefs.GetString(UI_SCRIPT_DIRECTORY_DATA_KEY);
                uiPrefabDirectory = EditorPrefs.GetString(UI_PREFAB_DIRECTOY_DATA_KEY);
                scriptNamespace = EditorPrefs.GetString(UI_SCRIPT_NAMESPACE_DATA_KEY);

                //Add component to the prefab
                var uiPrefabPath = $"{uiPrefabDirectory}/{uiPrefabName}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(uiPrefabPath);
                string typeAssemblyName = $"{scriptNamespace}.{uiPrefabName}, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
                var type = System.Type.GetType(typeAssemblyName);

                var element = prefab.AddComponent(type);
                if (element is UIViewBase)
                {
                    UIConfig.uiViewPrefabs.Add(element as UIViewBase);
                }
                else if (element is UIPopupBase)
                {
                    UIConfig.uiPopupPrefabs.Add(element as UIPopupBase);
                }
                UIConfig.SaveAsset();

                EditorPrefs.DeleteKey(UI_PREFAB_DIRECTOY_DATA_KEY);
                EditorPrefs.DeleteKey(UI_PREFAB_NAME_DATA_KEY);
                EditorPrefs.DeleteKey(UI_SCRIPT_DIRECTORY_DATA_KEY);
                EditorPrefs.DeleteKey(UI_SCRIPT_NAMESPACE_DATA_KEY);

                //Move asset to new path
                var oldPath = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
                var newPath = $"{uiScriptDirectory}/{uiPrefabName}.cs";

                AssetDatabase.MoveAsset(oldPath, newPath);


            }

            EditorApplication.delayCall -= OnChanged;

        }

        static void CreateScriptAsset(string templatePath, string destName)
        {
#if UNITY_2019_1_OR_NEWER
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, destName);
#else
	typeof(UnityEditor.ProjectWindowUtil)
		.GetMethod("CreateScriptAsset", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
		.Invoke(null, new object[] { templatePath, destName });
#endif
        }


    }
}
