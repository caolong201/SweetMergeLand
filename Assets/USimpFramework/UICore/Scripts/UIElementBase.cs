using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace USimpFramework.UI
{
    /// <summary>
    /// Base class for UIViewBase and UIPopupBase, this class will be child of the UIManager class in hierachy, therefore is will be not destroyed when loading scene
    /// UI always need to be called last when updating values
    /// </summary>
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    public abstract class UIElementBase : MonoBehaviour
    {
        public abstract void Show(bool withTransition = true, System.Action onCompleted = null);
        public abstract void Hide(bool withTransition = true, System.Action onCompleted = null);
    }
}
