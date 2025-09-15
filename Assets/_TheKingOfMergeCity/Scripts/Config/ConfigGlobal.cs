using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;

namespace TheKingOfMergeCity.Config
{
    using Enum;

    public class ConfigGlobal : ScriptableObject
    {
        [SerializeField] bool _skipTutorial;
        public bool skipTutorial => _skipTutorial;
        
        [SerializeField] int _startingEnergy = 100;
        public int startingEnergy => _startingEnergy;

        [Tooltip("Energy regen interval (in seconds)")]
        [SerializeField] float _energyRegenInterval = 120;
        public float energyRegenInterval => _energyRegenInterval;

        [SerializeField] int _maxEnergy = 30;
        public int maxEnergy => _maxEnergy;

        [Tooltip("Show suggestion interval, in seconds")]
        [SerializeField] float _showSuggestionInterval = 5f;
        public float showSuggestionInterval => _showSuggestionInterval;

        [SerializeField] int _starRewardPerItemLevel = 10;
        public int starRewardPerItemLevel => _starRewardPerItemLevel;

        [SerializeField] int _energyRewardAfterBuild = 20;
        public int energyRewardAfterBuild => _energyRewardAfterBuild;

        [SerializeField] int _energyRewardAfterWatchAd = 100;
        public int energyRewardAfterWatchAd => _energyRewardAfterWatchAd;

        [SerializeField] Material _grayMat;
        public Material grayMat => _grayMat;
    }
}
