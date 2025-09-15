using System;
using UnityEngine;

namespace USimpFramework.EditorExtension
{

    [AttributeUsage(AttributeTargets.Method, AllowMultiple =false, Inherited = true)]
    public class SimpleInspectorButtonAttribute : Attribute
    {  
        public string buttonText { get; }

        public SimpleInspectorButtonAttribute(string buttonText = null)
        {
            this.buttonText = buttonText;
        }
    }
}
