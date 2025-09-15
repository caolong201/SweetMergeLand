using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace TheKingOfMergeCity.Config
{
    [Serializable]
    public class ConfigCharacterItem
    {
        [SerializeField] int _id;
        public int id => _id;
        [SerializeField] Sprite _iconSprite;
        public Sprite iconSprite => _iconSprite;
    }

    public class ConfigCharacter : ScriptableObject
    {
        [SerializeField] List<ConfigCharacterItem> _items;
        public List<ConfigCharacterItem> items => _items;
    }
}
