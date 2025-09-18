using UnityEngine;

namespace TheKingOfMergeCity.Config
{
    [CreateAssetMenu(menuName = "Config/Tutorial/Click Buy Energy")]
    public class ConfigTutorialClickBuyEnergy : ConfigTutorialStep
    {
        [Tooltip("The hole mask size in pixel")]
        [SerializeField] Vector2 _holeSize = new Vector2(200, 200);
        public Vector2 holeSize => _holeSize;
    }
}
