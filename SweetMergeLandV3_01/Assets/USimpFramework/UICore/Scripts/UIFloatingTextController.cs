using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace USimpFramework.UI
{

    public class UIFloatingTextController : MonoBehaviour
    {
        [SerializeField] protected TMP_Text text;

        public virtual void Setup(string content)
        {
            text.text = content;
        }

        public virtual void Show(string content, System.Action onCompleted)
        {
            text.text = content;

        }
    }
}
