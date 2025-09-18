using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using USimpFramework.Utility;

namespace USimpFramework.UI
{
    public class UIManager : SimpleSingleton<UIManager>
    {
        [SerializeField] Canvas canvas;

        [SerializeField] Transform uiViewContainer;
        [SerializeField] Transform uiPopupContainer;
        [SerializeField] Transform uiFloatingTextContainer;


        public UIViewBase currentView { get; private set; }
        public UIPopupBase currentPopup { get; private set; }

        Dictionary<string, UIPopupBase> uiPopupDic = new();

        Dictionary<string, UIViewBase> uiViewDic = new();

         public Vector2 minRectPoint { get; private set; }
        public Vector2 maxRectPoint { get; private set; }

        void Start()
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,new Vector2(0, 0), null, out var temp1);
            minRectPoint = temp1;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform, new Vector2(Screen.width, Screen.height), null, out  var temp2);
            maxRectPoint = temp2;
        }

        UIViewBase prevView;
        UIPopupBase prevPopup;

        #region View Methods
        /// <summary>
        /// Show new view and hide the current view
        /// </summary>
        /// <typeparam name="T">the type of the UI View</typeparam>
        public T ShowView<T>(bool withTransition = true, System.Action onCompleted = null) where T : UIViewBase
        {
            var viewPrefab = UIConfig.uiViewPrefabs.Find(s => s is T);

            if (viewPrefab == null)
            {
                Debug.LogError("Show view failed! Cannot find " + typeof(T).Name);
                return null;
            }

            if (currentView != null)
            {
                prevView = currentView;
                currentView.Hide(withTransition);
            }

            if (!uiViewDic.TryGetValue(viewPrefab.name, out var viewInstance))
            {
                viewInstance = Instantiate(viewPrefab, uiViewContainer);
                uiViewDic.Add(viewPrefab.name, viewInstance);
            }

            currentView = viewInstance;
            currentView.Show(withTransition, onCompleted); //Show the newest view
            return currentView as T;

        }


        public void HideView(UIViewBase view, bool withTransition = true, System.Action onCompleted = null)
        {
            if (!uiViewDic.ContainsKey(view.GetType().Name))
            {
                Debug.LogError("Hide view failed! Cannot find: " + view.name);
                return;
            }

            view.Hide(withTransition, onCompleted);
        }

        public void HideView<T>(bool enablePrevious = false, bool withTransition = true, System.Action onCompleted = null) where T : UIViewBase
        {
            if (uiViewDic.TryGetValue(typeof(T).Name, out var view))
            {
                currentView = prevView;
                view.Hide(withTransition, onCompleted);

                if (enablePrevious)
                {
                    prevView.Show(withTransition);
                }
            }
            else
            {
                Debug.LogError("Hide view failed! Cannot find: " + typeof(T).Name);
                return;
            }
        }

        public void HideAllView(bool withTransition = true)
        {
            foreach (var view in uiViewDic.Values)
                view.Hide(withTransition);
        }

        public T GetView<T>() where T : UIViewBase
        {
            uiViewDic.TryGetValue(typeof(T).Name, out var uiView);
            return uiView as T;
        }
        #endregion

        #region Popup Methods
        /// <summary>
        /// Show Popup on top of other popup, not hiding the current popup
        /// </summary>
        /// <typeparam name="T">The type of the current popup script</typeparam>
        public T ShowPopup<T>(bool withTransition = true, System.Action onCompleted = null) where T : UIPopupBase
        {
            var popupPrefab = UIConfig.uiPopupPrefabs.Find(s => s is T);

            if (popupPrefab == null)
            {
                Debug.LogError("Show popup failed! Cannot find " + typeof(T).Name);
                return null;
            }


            if (currentPopup != null)
            {
                prevPopup = currentPopup;
            }

            if (!uiPopupDic.TryGetValue(popupPrefab.name, out var popupInstance))
            {
                popupInstance = Instantiate(popupPrefab, uiPopupContainer);
                uiPopupDic.Add(popupPrefab.name, popupInstance);
            }

            currentPopup = popupInstance;
            currentPopup.transform.SetAsLastSibling();
            currentPopup.Show(withTransition, onCompleted);
            return currentPopup as T;

        }

        public void HidePopup(UIPopupBase popup, bool withTransition = true, System.Action onCompleted = null)
        {
            if (!uiPopupDic.ContainsKey(popup.GetType().Name))
            {
                Debug.LogWarning("Hide popup failed! Cannot find: " + popup.name);
                return;
            }

            popup.Hide(withTransition, onCompleted);
        }

        public void HidePopup<T>(bool withTransition = true, System.Action onCompleted = null) where T : UIPopupBase
        {
            if (uiPopupDic.TryGetValue(typeof(T).Name, out var popup))
            {
                popup.Hide(withTransition, onCompleted);
            }
            else
            {
                Debug.LogWarning("Hide popup failed! Cannot find: " + typeof(T).Name);
                return;
            }
        }

        public void HideAllPopup(bool withTransition = true)
        {
            foreach (var popup in uiPopupDic.Values)
                popup.Hide(withTransition);
        }

        public T GetPopup<T>() where T : UIPopupBase
        {
            uiPopupDic.TryGetValue(typeof(T).Name, out var popup);
            return popup as T;
        }
        #endregion

        #region System Methods
        /// <summary>
        /// Show the loading popup at the last popup child element, to block other ui element
        /// </summary>
        /// <param name="isShow"></param>
        public void ShowLoading(bool isShow, System.Action onCompleted = null)
        {
            if (isShow)
            {
                ShowPopup<UILoadingPopup>(true, onCompleted);
            }
            else
            {
                HidePopup<UILoadingPopup>(true, onCompleted);
            }
        }

        public void MessageBox(string title, string content, bool hasConfirmButton = false, System.Action onPressConfirm = null)
        {
            ShowPopup<UIMessageBoxPopup>();
            var popup = currentPopup as UIMessageBoxPopup;
            popup.Setup(title, content, hasConfirmButton, onPressConfirm);
        }


        Queue<UIFloatingTextController> floatingTextPool = new();

        public void ShowFloatingText(string content)
        {
            UIFloatingTextController floatingText = null;

            if (floatingTextPool.Count == 0)
            {
                floatingText = Instantiate(UIConfig.uiFloatingTextController, uiFloatingTextContainer);
            }
            else
                floatingText = floatingTextPool.Dequeue();

            //Spawn at the bottom of the screen's device
            var bottom = Camera.main.ViewportToScreenPoint(new Vector2(0.5f, 0));

            floatingText.transform.position = bottom + Vector3.up * 250f;
            floatingText.gameObject.SetActive(true);
            floatingText.Show(content, () =>
            {
                floatingText.gameObject.SetActive(false);
                floatingTextPool.Enqueue(floatingText);
            });
        }

        
        /// <summary> Set interaction of all user event (button click, touch drag,...)  </summary>
        public void SetInteraction(bool isInteracted)
        {
            var eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
                return;
            eventSystem.enabled = isInteracted;
        }
        
        #endregion
    }
}



