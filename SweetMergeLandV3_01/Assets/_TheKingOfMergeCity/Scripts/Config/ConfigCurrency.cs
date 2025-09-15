using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace TheKingOfMergeCity.Config
{
    using Enum;

    [Serializable]
    public class ConfigCurrencyItem
    {
        [SerializeField] CurrencyType _type;
        public CurrencyType type => _type;

        [SerializeField] Sprite _iconSprite;
        public Sprite iconSprite => _iconSprite;
    }

    public class ConfigCurrency : ScriptableObject
    {
        [SerializeField] List<ConfigCurrencyItem> _items;
        public List<ConfigCurrencyItem> items => _items;
    }
}
