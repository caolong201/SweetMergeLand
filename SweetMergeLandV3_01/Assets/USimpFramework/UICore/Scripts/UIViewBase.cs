using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace USimpFramework.UI
{
    public class UIViewBase : UIElementBase
    {

        public override void Hide(bool withTransition = true, System.Action onCompleted = null)
        {
            gameObject.SetActive(false);
        }

        public override void Show(bool withTransition = true, System.Action onCompleted = null)
        {
            gameObject.SetActive(true);
        }
    }
}
