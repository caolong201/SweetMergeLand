using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKingOfMergeCity.Config
{
    using Model;

    [CreateAssetMenu(menuName = "Config/Tutorial/Puzzle move")]
    public class ConfigTutorialPuzzleMove : ConfigTutorialStep
    {
        public enum CompleteCondition
        {
            ByEndPosition = 0,
            ByItemId = 1,
        }

        [Header("Manual move")]
        [SerializeField] BoardPosition _startPosition;
        public BoardPosition startPosition => _startPosition;

        [SerializeField] BoardPosition _endPosition;
        public BoardPosition endPosition => _endPosition;

        [Tooltip("The hole mask size in pixel")]
        [SerializeField] Vector2 _holeSize = new Vector2(200, 520);
        public Vector2 holeSize => _holeSize;

        [Tooltip("Auto detect the Start item to merge to the End item, also move the hole masking to that 2 item")]
        [SerializeField] bool _autoDetectStartItem;
        public bool autoDetectStartItem => _autoDetectStartItem;

        [SerializeField] CompleteCondition _completeCondition;
        public CompleteCondition completeCondition => _completeCondition;

        [SerializeField] string _puzzleId;
        public string puzzleId => _puzzleId;
    }
}
