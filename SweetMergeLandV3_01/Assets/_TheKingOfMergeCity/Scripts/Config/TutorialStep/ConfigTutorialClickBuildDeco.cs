using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKingOfMergeCity.Config
{
    //Step to complete: Click on build button -> Popup open -> click on Start -> Load scene -> wait for the playing effect complete -> complete tutorial


    [CreateAssetMenu(menuName = "Config/Tutorial/Click To Build Deco")]
    public class ConfigTutorialClickBuildDeco : ConfigTutorialStep
    {
        [Tooltip("The hole mask size in pixel")]
        [SerializeField] Vector2 _holeSize = new Vector2(200, 520);
        public Vector2 holeSize => _holeSize;


    }
}
