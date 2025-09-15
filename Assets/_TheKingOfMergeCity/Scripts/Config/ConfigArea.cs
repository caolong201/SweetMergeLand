using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using USimpFramework.EditorExtension;

namespace TheKingOfMergeCity.Config
{
    [Serializable]
    public class ConfigAreaItem
    {
        [SerializeField] int _id;
        public int id => _id;

        [SerializeField] string _displayName;
        public string displayName => _displayName;

        [SerializeField] Sprite _iconSprite;
        public Sprite iconSprite => _iconSprite;

        [SerializeField] Sprite _iconSpriteDisable;
        public Sprite iconSpriteDisable => _iconSpriteDisable;

        [SerializeField] string _sceneAddress;
        public string sceneAddress => _sceneAddress;

        [SerializeField] List<ConfigDecoItem> _decoItems;
        public List<ConfigDecoItem> decoItems => _decoItems;

        [SerializeField] GameObject _workerModel;
        public GameObject workerModel => _workerModel;

        public ConfigAreaItem(int id)
        {
            _id = id;
        }

    }

    [Serializable]
    public class ConfigDecoItem
    {
        [SerializeField] string _id;
        public string id => _id;

        [SerializeField] Sprite _iconSprite;
        public Sprite iconSprite => _iconSprite;

        [SerializeField] int _buildingCost;
        public int buildingCost => _buildingCost;

        [SerializeField] int _expReward;
        public int expReward => _expReward;

        [SerializeField] string _description;
        public string description => _description;

        [SerializeField] List<string> _nextUnlockDecoIds;
        public List<string> nextUnlockDecoIds => _nextUnlockDecoIds;


        public ConfigDecoItem(string id, int buildingCost, string description, int expReward, List<string> nextUnlockDecoIds)
        {
            _id = id;
            _buildingCost = buildingCost;
            _expReward = expReward;
            _description = description;
            _nextUnlockDecoIds = nextUnlockDecoIds;
        }

        public void SetData(string id, int buildingCost, string description, int expReward, List<string> nextUnlockDecoIds)
        {
            _id = id;
            _buildingCost = buildingCost;
            _expReward = expReward;
            _description = description;
            _nextUnlockDecoIds = nextUnlockDecoIds;
        }

    }

    public class ConfigArea : ScriptableObject
    {
        [SerializeField] List<ConfigAreaItem> _areaItems;
        public List<ConfigAreaItem> areaItems => _areaItems;


#if UNITY_EDITOR
        [SerializeField] TextAsset csvConfigAreaDecoItems;
        [SerializeField] TextAsset csvConfigAreas;

        [SimpleInspectorButton("Import csv config area deco items")]
        void ImportCsvConfigAreaItems()
        {
            string[] rows = csvConfigAreaDecoItems.text.Split("\n", StringSplitOptions.RemoveEmptyEntries);

            int startingIndex = 1;
            int currentAreaId = 0;
            int currentDecoIndex = 0;

            string[] rows2 = csvConfigAreas.text.Split("\n", StringSplitOptions.RemoveEmptyEntries);

            //Clear all deco items
            /*foreach (var configArea in areaItems)
            {
                foreach ()
            }*/

            for (int i = startingIndex; i < rows.Length; i++)
            {
                var cols = rows[i].Replace("\r", "").Split(",");
                int areaId = int.Parse(cols[0]);

                if (areaId > currentAreaId)
                {
                    currentAreaId++;
                    currentDecoIndex = 0;
                    if (currentAreaId > areaItems.Count)
                    {
                        areaItems.Add(new ConfigAreaItem(currentAreaId));
                    }
                }

                int buildingCost = int.Parse(cols[3]);
                string description = cols[2];
                string decoId = cols[1];

                int expReward = int.Parse(cols[4]);

                List<string> nextUnlockDecoIds = new();
                string[] format = cols[5].Split(";");
                foreach (var id in format)
                {
                    nextUnlockDecoIds.Add(id);
                }

                areaItems[currentAreaId].decoItems[currentDecoIndex].SetData(decoId, buildingCost, description, expReward, nextUnlockDecoIds);
                currentDecoIndex++;
            }

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
        }


#endif

    }
}
