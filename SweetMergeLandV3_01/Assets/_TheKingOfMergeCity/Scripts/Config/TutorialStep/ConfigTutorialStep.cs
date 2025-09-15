using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKingOfMergeCity.Config
{

    public class ConfigTutorialStep : ScriptableObject
    {
        [SerializeField] string _id;
        public string id => _id;

        [TextArea]
        [SerializeField] string _description;
        public string description => _description;

        [SerializeField] Vector2 _offsetBetweenUITutorialInfoAndCenter;
        public Vector2 offsetBetweenUITutorialInfoAndCenter => _offsetBetweenUITutorialInfoAndCenter;

        [SerializeField] bool _showDescription = true;
        public bool showDescription => _showDescription;

        [SerializeField] bool _showDescriptionWhenPlay = true;
        public bool showDescriptionWhenPlay => _showDescriptionWhenPlay;
    }
}
