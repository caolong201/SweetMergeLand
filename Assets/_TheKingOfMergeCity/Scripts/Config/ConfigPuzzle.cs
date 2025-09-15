using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;

namespace TheKingOfMergeCity.Config
{
    using Enum;

    [Serializable]
    public class ConfigPuzzleDetailItem
    {
        [SerializeField] Sprite _itemSprite;
        public Sprite itemSprite => _itemSprite;

        [SerializeField] float _sizeScale = 0;
        public float sizeScale => _sizeScale;
    }

    [Serializable]
    public class ConfigBlockingLevelItem
    {
        [SerializeField] List<Sprite> _blockingSprites;
        public List<Sprite> blockingSprites => _blockingSprites;

        public Sprite GetRandomSprite()
        {
            return _blockingSprites[Random.Range(0, _blockingSprites.Count)];
        }
    }

    [Serializable]
    public class ConfigPuzzleItem
    {
        [Serializable]
        public struct SettingPerProducedItem
        {
            public string id;
            [Range(0, 100)]
            public int probability;
        }

        [SerializeField] string _id;
        public string id => _id;

        [SerializeField] PuzzleType _puzzleType;
        public PuzzleType puzzleType => _puzzleType;

        [Tooltip("Config per level, level starting from 0")]
        [SerializeField] List<ConfigPuzzleDetailItem> _configPerLevel;
        public List<ConfigPuzzleDetailItem> configPerLevel => _configPerLevel;

        [Header("PuzzleType/Producer")]
        [Tooltip("Probability must be ordered descending, the last one MUST have 0 value to calculate the correct probility")]
        [SerializeField] List<SettingPerProducedItem> _settingPerProducedItems;
        public List<SettingPerProducedItem> settingPerProducedItems => _settingPerProducedItems;

        public string GetProduceItemId()
        {
            int prob = Random.Range(0, 101);
            int total = 0;
            for (int i = 0; i < _settingPerProducedItems.Count - 1; i++)
            {
                var setting = _settingPerProducedItems[i];
                total += setting.probability;
                if (prob <= total)
                    return setting.id;
            }

            return _settingPerProducedItems[^1].id;
        }
    }

    [Serializable]
    public class ConfigPuzzleType
    {
        [SerializeField] PuzzleType _puzzleType;
        public PuzzleType puzzleType => _puzzleType;

        [SerializeField] UIPuzzleItemController _itemPrefab;
        public UIPuzzleItemController itemPrefab => _itemPrefab;

        [Header("PuzzleType/Producer")]
        [SerializeField] int _energyCostPerProduce = 1;
        public int energyCostPerProduce => _energyCostPerProduce;
    }

    [CreateAssetMenu(menuName = "Config/Puzzle")]
    public class ConfigPuzzle : ScriptableObject
    {
        [Header("Puzzle Item Spawn")]

        [Tooltip("The board size in unit")]
        [SerializeField] Vector2Int _boardSize;
        public Vector2Int boardSize => _boardSize;

        [Tooltip("The board size in pixel, the board will be stretched per device resolution, but will be clamped")]
        [SerializeField] Vector2 _boardSizePixelMin;
        public Vector2 boardSizePixelMin => _boardSizePixelMin;

        [Tooltip("The board size in pixel, the board will be stretched per device resolution, but will be clamped")]
        [SerializeField] Vector2 _boardSizePixelMax;
        public Vector2 boardSizePixelMax => _boardSizePixelMax;

        [SerializeField] Sprite _darkTileSprite;
        public Sprite darkTileSprite => _darkTileSprite;

        [SerializeField] Sprite _lightTileSprite;
        public Sprite lightTileSprite => _lightTileSprite;

        [SerializeField] GameObject _rowPrefab;
        public GameObject rowPrefab => _rowPrefab;

        [SerializeField] Image _tileImagePrefab;
        public Image tileImagePrefab => _tileImagePrefab;


        [SerializeField] List<ConfigPuzzleType> _configPuzzlePerTypes;
        public List<ConfigPuzzleType> configPuzzlePerTypes => _configPuzzlePerTypes;

        [SerializeField] List<ConfigBlockingLevelItem> _configBlockingItems;
        public List<ConfigBlockingLevelItem> configBlockingItems => _configBlockingItems;

        [SerializeField] List<ConfigPuzzleItem> _configItems;
        public List<ConfigPuzzleItem> configItems => _configItems;

    }
}
