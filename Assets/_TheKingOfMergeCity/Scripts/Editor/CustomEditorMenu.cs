using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TheKingOfMergeCity
{
    public class CustomditorMenu : MonoBehaviour
    {

        static readonly string schema = "Assets/_TheKingOfMergeCity/Scenes/{0}.unity";

        [MenuItem("My Menu/Start Game")]
        private static void StartGame()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.isDirty)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
            }

            EditorSceneManager.OpenScene(string.Format(schema, SceneConstants.BOOT_SCENE_NAME));
            EditorApplication.isPlaying = true;
        }
        [MenuItem("My Menu/Choose Scene/InGame")]
        private static void ChooseSceneInGame()
        {
            EditorSceneManager.OpenScene(string.Format(schema, SceneConstants.IN_GAME_SCENE_NAME));
        }

        [MenuItem("My Menu/Choose Scene/Home")]
        private static void ChooseSceneHome()
        {
            EditorSceneManager.OpenScene(string.Format(schema, SceneConstants.HOME_SCENE_NAME));
        }

    }
}
