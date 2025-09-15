using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using USimpFramework.Animation.DOTweenExtension;

namespace USimpFramework.UI
{

    public class UIPopupBase : UIElementBase
    {
        [SerializeField] protected DOTweenAnimationSettingController showAnim;
        [SerializeField] protected DOTweenAnimationSettingController hideAnim;

        public override void Hide(bool withTransition = true, System.Action onCompleted = null)
        {
            if (!gameObject.activeSelf)
                return;

            if (withTransition)
            {
                if (hideAnim != null)
                {
                    hideAnim.Play();

                    hideAnim.onCompleted.RemoveAllListeners();
                    hideAnim.onCompleted.AddListener(OnHideComplete);
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
            else
            {
                gameObject.SetActive(false);
            }

            void OnHideComplete()
            {
                gameObject.SetActive(false);
                onCompleted?.Invoke();
            }
        }

        public override void Show(bool withTransition = true, System.Action onCompleted = null)
        {

            //if (gameObject.activeSelf)
            //    return;

            if (withTransition)
            {
                gameObject.SetActive(true);

                if (showAnim != null)
                {
                    showAnim.Play();
                    showAnim.onCompleted.RemoveAllListeners();
                    showAnim.onCompleted.AddListener(() => onCompleted?.Invoke());
                }
            }
            else
            {
                gameObject.SetActive(true);
            }
        }
    }
}
