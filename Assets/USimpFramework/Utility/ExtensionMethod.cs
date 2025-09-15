using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Text;
using UnityEngine.UI;

namespace USimpFramework.Utility
{
    public static class CameraExtensions
    {
        static readonly Vector2 vpTopLeft = new Vector2(0, 1);
        static readonly Vector2 vpTopRight = new Vector2(1, 1);
        static readonly Vector2 vpBottomLeft = new Vector2(0, 0);
        static readonly Vector2 vpBottomRight = new Vector2(1, 0);

        public static Boundary GetWorldBoundary(this Camera camera)
        {
            if (camera == null)
                return new();

            return new Boundary()
            {
                topLeft = camera.ViewportToWorldPoint(vpTopLeft),
                topRight = camera.ViewportToWorldPoint(vpTopRight),
                bottomLeft = camera.ViewportToWorldPoint(vpBottomLeft),
                bottomRight = camera.ViewportToWorldPoint(vpBottomRight)
            };

        }

        ///<summary>Only works if camera is orthorgraphic</summary>
        public static Vector2 Get2DSize(this Camera camera)
        {
            if (camera == null || !camera.orthographic)
                return Vector2.zero;

            float height = camera.orthographicSize * 2;
            return new Vector2((float)Screen.width/ Screen.height * height, height);
        }
    }

    public static class MathfUtils
    {
        /// <summary>Round the value up to digits number</summary>
        public static float Round(float value, int digits)
        {
            int fraction = (int)Mathf.Pow(10, digits);
            return (float)Mathf.RoundToInt(value * fraction) / fraction;
        }

        ///<summary>Floor the float value up to digits number </summary>
        public static float Floor(float value, int digits)
        {
            if (digits == 0)
                return Mathf.Floor(value);
            var power = Mathf.Pow(10, digits);
            return Mathf.Floor(value * power) / power;
        }
    }

    public static class GlobalUtils
    {
        public static void Swap<T>(ref T a, ref T b)
        {
            var temp = a;
            a = b;
            b = temp;
        }
    }

    public static class JsonExtension
    {
        public static string ToJson(this object obj)
        {
            if (obj == null)
                return string.Empty;

            return JsonConvert.SerializeObject(obj);

        }
    }

    /// <summary> Extensions for String type</summary>
    public static class StringExtensions
    {
        ///<summary>To UpperCase First Char </summary>
        public static string ToUpperCaseFirstChar(this string str)
        {
            return str[0].ToString().ToUpper() + str[1..];
        }

        ///<summary>To Sentence With Spaces </summary>
        public static string ToSentenceWithSpaces(this string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return "";
            var newText = new StringBuilder(str.Length * 2);
            newText.Append(str[0]);
            for (int i = 1; i < str.Length; i++)
            {
                if (char.IsUpper(str[i]) && str[i - 1] != ' ')
                    newText.Append(' ');
                newText.Append(str[i]);
            }
            return newText.ToString();
        }
    }

    public static class ArrayExtension
    {

    }

    [System.Serializable]
    public struct Boundary
    {
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
    }

    [System.Serializable]
    public struct Offset
    {
        public float top;
        public float bottom;
        public float left;
        public float right;

    }

    public static class TransformExtensions
    {
        static StringBuilder path = new();

        public static string GetPathFromRoot(this Transform transform, Transform rootTransform)
        {
            var temp = transform;
            path.Clear();
            while (temp.name != rootTransform.name)
            {
                path.Insert(0, temp.name + "/");
                temp = temp.parent;
            }

            return path.ToString();
        }

        public static void ClearAllChilds(this Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }

        public static List<Transform> GetAllChilds(this Transform transform)
        {
            var result = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                result.Add(transform.GetChild(i));
            }

            return result;
        }
    }

    public static class RectTransformExtensions
    {
        public static Vector2 GetSize(this RectTransform source) => source.rect.size;

        public static float GetWidth(this RectTransform source) => source.rect.size.x;

        public static float GetHeight(this RectTransform source) => source.rect.size.y;

        /// <summary> Sets the sources RT size to the same as the toCopy's RT size.  </summary>
        public static void SetSize(this RectTransform source, RectTransform toCopy)
        {
            source.SetSize(toCopy.GetSize());
        }

        /// <summary>  Sets the sources RT size to the same as the newSize. </summary>
        public static void SetSize(this RectTransform source, Vector2 newSize)
        {
            source.SetSize(newSize.x, newSize.y);
        }

        /// <summary> Sets the sources RT size to the new width and height. </summary>
        public static void SetSize(this RectTransform source, float width, float height)
        {
            source.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            source.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        /// <summary> Sets the sources RT size to the new width. </summary>
        public static void SetWidth(this RectTransform source, float width)
        {
            source.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        /// <summary> Sets the sources RT size to the new width. </summary>
        public static void SetHeight(this RectTransform source, float height)
        {
            source.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        static readonly Vector3[] worldCorners = new Vector3[4];
        /// <summary> Get the world rectangle from local rectangle in rect transform</summary>
        public static Rect GetWorldRect(this RectTransform rectTransform)
        {
            // This returns the world space positions of the corners in the order // [0] bottom left, // [1] top left // [2] top right // [3] bottom right 
            rectTransform.GetWorldCorners(worldCorners);

            Vector2 min = worldCorners[0];
            Vector2 max = worldCorners[2];
            Vector2 size = max - min;
            return new Rect(min, size);
        }

        public static bool IsElementOverlap(this RectTransform rectTransform, RectTransform other)
        {
            var rect1 = rectTransform.GetWorldRect();
            var rect2 = other.GetWorldRect();
            return rect1.Overlaps(rect2);
        }
    }

    public static class ImageExtension
    {
        public static void SetAlpha(this Image image, float alpha)
        {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

}

