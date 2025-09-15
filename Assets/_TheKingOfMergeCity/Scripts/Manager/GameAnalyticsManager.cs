using GameAnalyticsSDK;
using UnityEngine;
using USimpFramework.Utility;

namespace TheKingOfMergeCity
{
    public class GameAnalyticsManager : SimpleSingleton<GameAnalyticsManager>
    {
        [SerializeField] string decoBuildCompleteSchema;
        [SerializeField] string deocBuildStartSchema;

        void Start()
        {
            GameAnalytics.Initialize();
        }

        public void SendDecoBuildCompleteEvent(int areaId, int buildDecoId)
        {
            GameAnalytics.NewDesignEvent(string.Format(decoBuildCompleteSchema, areaId + 1, buildDecoId + 1));
        }

        public void SendDecoBuildStartEvent(int areaId, int buildDecoId)
        {
            GameAnalytics.NewDesignEvent(string.Format(deocBuildStartSchema, areaId + 1, buildDecoId + 1));
        }
    }
}
