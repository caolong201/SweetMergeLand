using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKingOfMergeCity.Config
{
    public class ConfigTutorial : ScriptableObject
    {
        [SerializeField] List<ConfigTutorialStep> _steps;
        public IReadOnlyList<ConfigTutorialStep> readonlySteps => _steps;


        public ConfigTutorialStep GetStep(string id)
        {
            return _steps.Find(s => s.id == id);
        }
    }
}
