using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKingOfMergeCity.Config
{
    [CreateAssetMenu(menuName = "Config/Tutorial/Click Start in build popup")]
    public class ConfigTutorialClickStartInBuildPopup : ConfigTutorialStep
    {
        [Tooltip("The hole mask size in pixel")]
        [SerializeField] Vector2 _holeSize = new Vector2(200, 520);
        public Vector2 holeSize => _holeSize;
    }
}
