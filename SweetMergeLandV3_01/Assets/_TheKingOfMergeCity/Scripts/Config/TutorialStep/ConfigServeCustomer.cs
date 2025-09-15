using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKingOfMergeCity.Config
{
    [CreateAssetMenu(menuName = "Config/Tutorial/Serve Customer")]
    public class ConfigServeCustomer : ConfigTutorialStep //This step not will be trigger the UI right away, instead it wait until the user has pending serve
    {
        [SerializeField] bool _autoDetectServeCustomer = true;
        public bool autoDetectServeCustomer => _autoDetectServeCustomer;

        [Tooltip("The hole mask size in pixel")]
        [SerializeField] Vector2 _holeSize = new Vector2(200, 520);
        public Vector2 holeSize => _holeSize;

        [SerializeField] bool _allowMovePuzzle = false;
        public bool allowMovePuzzle => _allowMovePuzzle;

        [SerializeField] bool _allowClickProducer = true;
        public bool allowClickProducer => _allowClickProducer;
    }
}
