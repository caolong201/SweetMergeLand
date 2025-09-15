using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TheKingOfMergeCity
{
    public class InvertedMaskRaycast : MonoBehaviour, ICanvasRaycastFilter
    {
        [SerializeField] MaskableGraphic targetGraphic;

        public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
        {
            // Skip if deactived.
            if (!isActiveAndEnabled || !targetGraphic || !targetGraphic.isActiveAndEnabled)
            {
                return true;
            }

            // check inside
            if (eventCamera)
            {
                return !RectTransformUtility.RectangleContainsScreenPoint(targetGraphic.rectTransform, sp, eventCamera);
            }
            else
            {
                return !RectTransformUtility.RectangleContainsScreenPoint(targetGraphic.rectTransform, sp);
            }
        }

      

    }
}
