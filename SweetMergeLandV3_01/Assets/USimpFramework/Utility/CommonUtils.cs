using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace USimpFramework.Utility
{
    public static class CommonUtils 
    {
        public static void DeepCopy<T>(T des, T source, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            var fields = des.GetType().GetFields(bindingFlags);

            foreach (var field in fields)
            {
                var sourceValueField = field.GetValue(source);
                field.SetValue(des, sourceValueField);
            }
        }

        public static void DeepCopyAll<T>(T des, T source, BindingFlags fieldBinding = BindingFlags.Public| BindingFlags.Instance, BindingFlags propertyBinding = BindingFlags.Public | BindingFlags.Instance)
        {
            var type = des.GetType();
            var fields = type.GetFields(fieldBinding);

           foreach (var field in fields)
            {
                field.SetValue(des, field.GetValue(source));
            }

            var properties = type.GetProperties(propertyBinding);

            foreach (var property in properties)
            {
                property.SetValue(des, property.GetValue(source));
            }
        }

        public static void DeepCopy(object des, object src, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            var desType = des.GetType();
            var sourceType = src.GetType();
            if (des.GetType() != src.GetType())
                throw new InvalidOperationException($"{desType} is not match with {sourceType}");

            var fields = desType.GetFields(bindingFlags);
            foreach (var field in fields)
            {
                field.SetValue(des, field.GetValue(src));
            }
        }
    }

#if UNITY_EDITOR
    public static class SerializedPropertyExtensionMethods
    {
        public static T GetSerializedValue<T>(this SerializedProperty property)
        {
            object targetObject = property.serializedObject.targetObject;
            string[] propertyNames = property.propertyPath.Split('.');

            // Clear the property path from "Array" and "data[i]".
            if (propertyNames.Length >= 3 && propertyNames[propertyNames.Length - 2] == "Array")
                propertyNames = propertyNames.Take(propertyNames.Length - 2).ToArray();

            // Get the last object of the property path.
            foreach (string path in propertyNames)
            {
                targetObject = targetObject.GetType()
                    .GetField(path, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .GetValue(targetObject);
            }

            if (targetObject.GetType().GetInterfaces().Contains(typeof(IList<T>)))
            {
                int propertyIndex = int.Parse(property.propertyPath[property.propertyPath.Length - 2].ToString());

                return ((IList<T>)targetObject)[propertyIndex];
            }
            else return (T)targetObject;
        }
    }
#endif

    public static class FieldInfoExtensionMethods
    {
       public static void SetValue(this FieldInfo fieldInfo, object obj, string valueStr, Type type)
        {

            if (type == typeof(int))
            {
                fieldInfo.SetValue(obj, int.Parse(valueStr));
            }
            else if (type == typeof(uint))
            {
                fieldInfo.SetValue(obj, uint.Parse(valueStr));
            }
            else if (type == typeof(float))
            {
                fieldInfo.SetValue(obj, float.Parse(valueStr));
            }
            else if (type == typeof(decimal))
            {
                fieldInfo.SetValue(obj, decimal.Parse(valueStr));
            }
            else if (type == typeof(short))
            {
                fieldInfo.SetValue(obj, short.Parse(valueStr));
            }
            else if (type == typeof(ushort))
            {
                fieldInfo.SetValue(obj, ushort.Parse(valueStr));
            }
            else if (type == typeof(bool))
            {
                fieldInfo.SetValue(obj, bool.Parse(valueStr));
            }
            else if (type == typeof(long))
            {
                fieldInfo.SetValue(obj, long.Parse(valueStr));
            }
            else if (type == typeof(ulong))
            {
                fieldInfo.SetValue(obj, ulong.Parse(valueStr));
            }
            else if (type == typeof(double))
            {
                fieldInfo.SetValue(obj, double.Parse(valueStr));
            }
            else if (type == typeof(string))
            {
                fieldInfo.SetValue(obj, valueStr);
            }
            else
            {
                try
                {
                    //Debug.Log($"{valueStr}, {type}");
                    fieldInfo.SetValue(obj, JsonConvert.DeserializeObject(valueStr, type));
                }
                catch (Exception e)
                {
                    throw new InvalidCastException("Attempting to deserialize object with value as json string failed! Sub exception: " + e.Message);
                }
            }
               
        }

       public static object GetValue(this FieldInfo fieldInfo, object obj, Type type, out Type convertedType)
        {
            if (type.IsValueType || type == typeof(string))
            {
                convertedType = type;
                return fieldInfo.GetValue(obj);
            }

            convertedType = typeof(string);
            return JsonConvert.SerializeObject(fieldInfo.GetValue(obj));
        }
    }

    
}
