using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace USimpFramework.Utility
{
    [System.Serializable]
    public class RangeFloatValue
    {
        public enum ValueType
        {
            Constant = 0,
            RandomBetweenTwoConstants = 1
        }

        public ValueType valueType;

        public float constantValue;

        public float minRandomValue;
        public float maxRandomValue;

        public float Value
        {
            get
            {
                switch (valueType)
                {
                    case ValueType.Constant: return constantValue;
                    case ValueType.RandomBetweenTwoConstants: return Random.Range(minRandomValue, maxRandomValue);
                }

                return 0;
            }
        }
    }
}

#if UNITY_EDITOR
namespace USimpFramework.Utility.Editor
{
    using UnityEditor;

    [CustomPropertyDrawer(typeof(RangeFloatValue))]
    internal class RangeFloatValueDrawer : PropertyDrawer
    {
        RangeFloatValue.ValueType valueTypeEnum;

        const float lineSpace = 3f;

        float lineHeight => EditorGUIUtility.singleLineHeight + lineSpace;

        SerializedProperty valueTypeProperty;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lineCount = 1;

            return lineHeight * lineCount + EditorGUIUtility.standardVerticalSpacing;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GetSerializedProperty(property);
            float expandWidth = position.width;

            // Using BeginProperty / EndProperty on the parent property means that  prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            //Draw label
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            if ((int)valueTypeEnum != valueTypeProperty.enumValueIndex) //This is for first initialization
            {
                valueTypeEnum = (RangeFloatValue.ValueType)valueTypeProperty.enumValueIndex;
            }

            valueTypeEnum = (RangeFloatValue.ValueType)EditorGUI.EnumPopup(new Rect(expandWidth, position.y, 20f, 20f), valueTypeEnum);
            valueTypeProperty.enumValueIndex = (int)valueTypeEnum;

            var startPosX = EditorGUIUtility.labelWidth - ((EditorGUI.indentLevel - 1) * 15 - 5);

            switch (valueTypeEnum)
            {
                case RangeFloatValue.ValueType.Constant:
                    {
                        var constantFieldWidth = expandWidth / 2;
                        constantFieldWidth = Mathf.Clamp(constantFieldWidth, 20, 320); //Todo: research for stretching the width like the way EditorGUI.PropertyField does
                        var constantFieldRect = new Rect(startPosX, position.y, constantFieldWidth, EditorGUIUtility.singleLineHeight);

                        EditorGUI.PropertyField(constantFieldRect, property.FindPropertyRelative("constantValue"), GUIContent.none);
                        break;
                    }
                case RangeFloatValue.ValueType.RandomBetweenTwoConstants: //Todo: research for stretching the width like the way EditorGUI.PropertyField does
                    {
                        var randomConstantsFieldWidth = expandWidth / 4;
                        randomConstantsFieldWidth = Mathf.Clamp(randomConstantsFieldWidth, 20, 320);
                        var randomConstantsFieldRect = new Rect(startPosX, position.y, randomConstantsFieldWidth, EditorGUIUtility.singleLineHeight);

                        EditorGUI.PropertyField(randomConstantsFieldRect, property.FindPropertyRelative("minRandomValue"), GUIContent.none);

                        randomConstantsFieldRect.x += (randomConstantsFieldWidth + 10f);

                        EditorGUI.PropertyField(randomConstantsFieldRect, property.FindPropertyRelative("maxRandomValue"), GUIContent.none);
                        break;
                    }
                default:
                    {
                        EditorGUI.HelpBox(new Rect(startPosX, position.y, 200f, EditorGUIUtility.singleLineHeight), "Currently not available", MessageType.Error);
                        break;
                    }

            }

            //property.serializedObject.ApplyModifiedProperties();

            EditorGUI.EndProperty();
        }

        void GetSerializedProperty(SerializedProperty property)
        {
            valueTypeProperty = property.FindPropertyRelative("valueType");
        }
    }
}
#endif