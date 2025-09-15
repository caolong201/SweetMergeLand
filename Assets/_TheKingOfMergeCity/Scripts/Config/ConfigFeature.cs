using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TheKingOfMergeCity.Config
{
    using Enum;

    [System.Serializable]
    public class ConfigFeatureItem
    {
        [SerializeField] FeatureType _featureType;
        public FeatureType featureType => _featureType;

        [SerializeField] int _unlockAtPlayerLevel;
        public int unlockAtPlayerLevel => _unlockAtPlayerLevel;
        
        [SerializeField] string _displayName;
        public string displayName => _displayName;

        [SerializeField] Sprite _iconSprite;
        public Sprite iconSprite => _iconSprite;
    }
    
    public class ConfigFeature : ScriptableObject
    {
        [SerializeField] List<ConfigFeatureItem> _items;
        public List<ConfigFeatureItem> items => _items;

        public ConfigFeatureItem GetConfigByFeatureType(FeatureType featureType)
        {
            return _items.Find(c => c.featureType == featureType);
        }
    }
}
