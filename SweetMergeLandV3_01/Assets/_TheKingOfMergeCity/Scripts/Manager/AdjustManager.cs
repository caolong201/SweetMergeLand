using System.Collections;
using System.Collections.Generic;
using AdjustSdk;
using UnityEngine;
using USimpFramework.Utility;

namespace TheKingOfMergeCity
{
    public class AdjustManager : SimpleSingleton<AdjustManager>
    {
        static readonly string ADJUST_KEY = "gfat4yp3ek1s";

        void Start()
        {
            AdjustConfig adjustConfig = new(ADJUST_KEY, AdjustEnvironment.Production, true);

            adjustConfig.LogLevel = AdjustLogLevel.Info;
            adjustConfig.IsSendingInBackgroundEnabled = true;

            new GameObject("Adjust").AddComponent<Adjust>();

            Adjust.InitSdk(adjustConfig);

        }
    }
}
