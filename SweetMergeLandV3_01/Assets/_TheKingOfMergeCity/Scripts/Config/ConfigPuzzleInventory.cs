using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKingOfMergeCity.Config
{
    [System.Serializable]
    public class ConfigPuzzleInventorySlotItem
    {
        [SerializeField] int _unlockCost;
        public int unlockCost => _unlockCost;
    }


    public class ConfigPuzzleInventory : ScriptableObject
    {
        [SerializeField] List<ConfigPuzzleInventorySlotItem> _configSlots;
        public List<ConfigPuzzleInventorySlotItem> configSlots => _configSlots;

        [SerializeField] int _slotPerRow;
        public int slotPerRow => _slotPerRow;

        [SerializeField] int _unlockInventoryAtLevel; //Maybe we can use this
        public int unlockInventoryAtLevel => _unlockInventoryAtLevel;
    }
}
