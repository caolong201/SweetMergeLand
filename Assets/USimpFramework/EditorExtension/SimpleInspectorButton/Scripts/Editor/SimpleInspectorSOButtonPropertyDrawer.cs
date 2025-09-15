using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace USimpFramework.EditorExtension
{
    [CustomEditor(typeof(ScriptableObject), true)]
    public class SimpleInspectorSOButtonPropertyDrawer : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector
            DrawDefaultInspector();

            // Get the target script
            ScriptableObject monoBehaviour = (ScriptableObject)target;

            // Get all methods in the target script
            MethodInfo[] methods = monoBehaviour.GetType().GetMethods(
                BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (MethodInfo method in methods)
            {
                // Check if the method has the ButtonAttribute
                SimpleInspectorButtonAttribute buttonAttribute = method.GetCustomAttribute<SimpleInspectorButtonAttribute>();
                if (buttonAttribute != null)
                {
                    string buttonText = buttonAttribute.buttonText ?? method.Name;

                    // Create a button in the inspector
                    if (GUILayout.Button(buttonText))
                    {
                        // Invoke the method
                        method.Invoke(monoBehaviour, null);
                    }
                }

            }

        }
    }
}
