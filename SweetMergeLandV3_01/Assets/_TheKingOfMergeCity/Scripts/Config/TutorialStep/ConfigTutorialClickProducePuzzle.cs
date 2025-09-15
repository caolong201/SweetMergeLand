using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKingOfMergeCity.Config
{
    using Model;

    [CreateAssetMenu (menuName = "Config/Tutorial/Click produce puzzle")]
    public class ConfigTutorialClickProducePuzzle : ConfigTutorialStep
    {
        [SerializeField] BoardPosition _clickPosition;
        public BoardPosition clickPosition => _clickPosition;

        [SerializeField] bool _autoDetectPosition;
        public bool autoDetectPosition => _autoDetectPosition;


        [Tooltip("The hole mask size in pixel")]
        [SerializeField] Vector2 _holeSize = new Vector2(200, 520);
        public Vector2 holeSize => _holeSize;

        [SerializeField] List<ConfigOrderItem> _forceProduceItems;
        public List<ConfigOrderItem> forceProduceItem => _forceProduceItems;

    }
}
