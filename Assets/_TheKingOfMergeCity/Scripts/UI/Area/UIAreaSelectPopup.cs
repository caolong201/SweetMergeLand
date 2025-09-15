using System.Collections.Generic;
using UnityEngine;
using USimpFramework.UI;
using System.Linq;
using UnityEngine.UI;

namespace TheKingOfMergeCity
{
    public class UIAreaSelectPopup : MonoBehaviour
    {
        public event System.Action onLoadAreaCompleted;

        [SerializeField] UIAreaItem uiAreaItemPrefab;
        [SerializeField] ScrollRect scrollRect; 
        List<UIAreaItem> uiAreaItems = new();

        void Start()
        {
            uiAreaItemPrefab.gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
         
            var configAreas = ConfigManager.Instance.configArea.areaItems;
            //Order By config rea
            var sortedConfigAreas = configAreas
                .OrderByDescending(c => c.id)
                .OrderByDescending(c =>
                {
                    var a = UserManager.Instance.areaDatas.Find(s => s.areaId == c.id);
                    return a != null && a.completedDecoIds.Count < c.decoItems.Count;
                })
                .OrderByDescending(c => UserManager.Instance.areaDatas.Find(s => s.areaId == c.id) != null)
                .ToList();
           
            while (uiAreaItems.Count < sortedConfigAreas.Count)
            {
                var ui = Instantiate(uiAreaItemPrefab, uiAreaItemPrefab.transform.parent);
                uiAreaItems.Add(ui);
            }

            uiAreaItems.ForEach(ui => ui.gameObject.SetActive(false));

            for (int i = 0; i < sortedConfigAreas.Count; i++)
            {
                var uiAreaItem = uiAreaItems[i];
                var configAreaItem = sortedConfigAreas[i];
                uiAreaItem.Setup(configAreaItem, UserManager.Instance.areaDatas.Find(s => s.areaId == configAreaItem.id));
                uiAreaItem.goSeparator.SetActive(i < configAreas.Count - 1);
            }
        }

        public void ScrollToTop()
        {
            scrollRect.verticalNormalizedPosition = 1;
        }

        public void Hide()
        {
            
        }

        public void PressUIAreaItem(UIAreaItem uiAreaItem)
        {
            if (uiAreaItem.areaItemState == Enum.DecoItemState.Lock)
                return;

            UserManager.Instance.SetCurrentSelectAreaId(uiAreaItem.config.id);
            UIManager.Instance.ShowLoading(true, () =>
            {
                UIManager.Instance.HideAllView();
                HomeManager.Instance.LoadArea(uiAreaItem.config.id, () =>
                {
                    UIManager.Instance.ShowLoading(false);
                    onLoadAreaCompleted?.Invoke();
                });
            });
        }
    }
}

