using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

using UnityEngine;
using USimpFramework.Utility;
using DG.Tweening;

using UnityEngine.SceneManagement;

using USimpFramework.UI;

using Random = UnityEngine.Random;

namespace TheKingOfMergeCity
{
    public class BootManager : SimpleSingleton<BootManager>
    {
        public Action onProgressChanged;
        public Action<string> onBeforeSceneLoaded;
        public Action<string> onAfterSceneLoaded;
        
        public float progress { get; private set; }
        
        IEnumerator Start()
        {
            TimeUtils.InitServerStartTime(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            Application.targetFrameRate = 60;
            Input.simulateMouseWithTouches = true;
            Input.multiTouchEnabled = false;

            UserManager.Instance.Init();

            UIManager.Instance.ShowView<UIBootView>();
            
            float fakeProgress = Random.Range(0.2f, 0.75f);
            var tween = DOTween.To(() => progress, x =>
            {
                progress = x;
                onProgressChanged?.Invoke();
            }, fakeProgress, 0.5f).SetEase(Ease.Linear);
            
            yield return tween.WaitForCompletion();
            
            //Wait for some seconds
            yield return new WaitForSeconds(Random.Range(0.5f, 0.75f));

            var tween2 = DOTween.To(() => progress, x =>
            {
                progress = x;
                onProgressChanged?.Invoke();
            }, 1, 0.5f).SetEase(Ease.Linear);

            yield return tween2.WaitForCompletion();            
            
            //Check if has not finish the tutorial
            if (!UserManager.Instance.finishTutorial)
            {
                //Load in game scene
                LoadScene(SceneConstants.IN_GAME_SCENE_NAME,false);
            }
            else
            {
                //Load home scene
                LoadScene(SceneConstants.HOME_SCENE_NAME, false);
            }
        }

        public async void LoadScene(string sceneName, bool showLoadingScreen)
        {
           var scene = SceneManager.GetSceneByName(sceneName);
           if (!string.IsNullOrEmpty(scene.name))
           {
               throw new UnityException($"Scene {sceneName} not exists in build");
           }

           onBeforeSceneLoaded?.Invoke(sceneName);
           if (showLoadingScreen)
           {
               UIManager.Instance.ShowLoading(true, async () =>
               {
                   var op = SceneManager.LoadSceneAsync(sceneName);
                   while (!op.isDone)
                       await Task.Yield();

                   UIManager.Instance.ShowLoading(false);
                   onAfterSceneLoaded?.Invoke(sceneName);
               });
           }
           else
           {
               var op = SceneManager.LoadSceneAsync(sceneName);
               while (!op.isDone)
                   await Task.Yield();
               
               onAfterSceneLoaded?.Invoke(sceneName); 
           }
        }
        
    }
}
