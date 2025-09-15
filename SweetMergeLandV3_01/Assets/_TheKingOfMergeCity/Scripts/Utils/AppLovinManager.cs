using System.Collections;
using System.Collections.Generic;
using USimpFramework.Utility;
using UnityEngine;
using System;

namespace TheKingOfMergeCity
{
    public class ApplovinManager : SimpleSingleton<ApplovinManager>
    { 

        public enum AdType
        {
            Banner,
            Rewarded,
            Interstitial
        }

        [Serializable]
        public struct AdSetting
        {
            public AdType adType;
            public string adUnitId;
            public string testAdUnitId;
            public bool enableTestAd;

            /// <summary>The ad id based on the config enable test ad or not  </summary>
            public string adId => enableTestAd ? testAdUnitId : adUnitId;
        }

        [Serializable]
        public struct BannerAdUISetting
        {
            public MaxSdkBase.AdViewPosition alignment;
            public bool screenAdaptive;
            public bool useCustomPosition;
            public Vector2 customPosition;
            public Color color;
            public bool overrideWidth;
            public float width;
            

        }

        [SerializeField] List<AdSetting> androidAdSettings;
        [SerializeField] List<AdSetting> iosAdSetting;
        [SerializeField] bool enableMediationDebugger = true;
        [SerializeField] bool enableCreativeDebugger;

        [Header("Banner Setting")]
        [SerializeField] BannerAdUISetting bannerAdUISetting = new BannerAdUISetting() 
        { 
            color = new Color32(255, 255, 255, 255), 
            alignment = MaxSdk.AdViewPosition.BottomCenter, 
            screenAdaptive = true 
        };

        string rewardedAdUnitId;
        string bannerAdUnitId;

        int retryAttempt;

        void Start()
        {
            MaxSdk.InitializeSdk();
            MaxSdk.SetCreativeDebuggerEnabled(enableCreativeDebugger);
            
            
            if (Application.isEditor)
            {
                rewardedAdUnitId = androidAdSettings.Find(s => s.adType == AdType.Rewarded).testAdUnitId;
                bannerAdUnitId = androidAdSettings.Find(s => s.adType == AdType.Banner).testAdUnitId;
            
        }
            else
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    var rewardedAdSetting = androidAdSettings.Find(s => s.adType == AdType.Rewarded);
                    rewardedAdUnitId = rewardedAdSetting.adId;

                    var bannerAdSetting = androidAdSettings.Find(s => s.adType == AdType.Banner);
                    bannerAdUnitId = bannerAdSetting.adId;

                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    var rewardedAdSetting = iosAdSetting.Find(s => s.adType == AdType.Rewarded);
                    rewardedAdUnitId = rewardedAdSetting.adId;

                    var bannerAdSetting = iosAdSetting.Find(s => s.adType == AdType.Banner);
                    bannerAdUnitId = bannerAdSetting.adId;
                }
            }

            MaxSdkCallbacks.OnSdkInitializedEvent += config =>
            {
                Debug.Log($"{nameof(ApplovinManager)} | Applovin SDK is initialized");

                if (enableMediationDebugger)
                {
                    MaxSdk.ShowMediationDebugger();
                }

                if (enableCreativeDebugger)
                {
                    MaxSdk.ShowCreativeDebugger();
                }

                InitializeRewardedAds();
                InitializeBannerAds();
            };
        }

        void OnDestroy()
        {
            UnRegisterRewaredAdEvents();
            UnRegisterBannerAdEvents();
        }

        public void ShowRewardedAd(Action<bool> onResult)
        {
            if (!MaxSdk.IsRewardedAdReady(rewardedAdUnitId))
            {
                Debug.LogError($"{nameof(ApplovinManager)} | Rewarded ad with Id {rewardedAdUnitId} is not ready");
                onResult?.Invoke(false);
                return;
            }

            Debug.Log($"{nameof(ApplovinManager)} | Show Rewarded Ad with ad ID {rewardedAdUnitId}");

            MaxSdk.ShowRewardedAd(rewardedAdUnitId);

            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += m_OnAdDisplayedFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += m_OnRewardedAdReceivedRewardEvent;

            void m_OnAdDisplayedFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
            {
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= m_OnAdDisplayedFailedEvent;
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= m_OnRewardedAdReceivedRewardEvent;
                onResult?.Invoke(false);
            }

            void m_OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
            {
                MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= m_OnRewardedAdReceivedRewardEvent;
                MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= m_OnAdDisplayedFailedEvent;
                
                // The rewarded ad displayed and the user should receive the reward.
                Debug.Log($"{nameof(ApplovinManager)} | On Rewarded received for ad ID: " + adUnitId);
                
                onResult?.Invoke(true);
            }
        }

        #region Rewarded Ad
        void InitializeRewardedAds()
        {
            RegisterRewardedAdEvents();

            // Load the first rewarded ad
            LoadRewardedAd();
        }

        void RegisterRewardedAdEvents()
        {
            // Attach callback
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent += OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += OnRewardedAdFailedToDisplayEvent;
        }

        void UnRegisterRewaredAdEvents()
        {
            // Attach callback
            MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= OnRewardedAdLoadedEvent;
            MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= OnRewardedAdLoadFailedEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent -= OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.Rewarded.OnAdClickedEvent -= OnRewardedAdClickedEvent;
            MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= OnRewardedAdRevenuePaidEvent;
            MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= OnRewardedAdFailedToDisplayEvent;
        }

        void LoadRewardedAd()
        {
            Debug.Log($"{nameof(ApplovinManager)} | Loading ad with Id {rewardedAdUnitId}...");
            MaxSdk.LoadRewardedAd(rewardedAdUnitId);
        }

        void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is ready for you to show. MaxSdk.IsRewardedAdReady(adUnitId) now returns 'true'.
            Debug.Log($"{nameof(ApplovinManager)} | New rewarded ad with id {adUnitId} loaded sucessfully!");


            // Reset retry attempt
            retryAttempt = 0;
        }

        void OnRewardedAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
        {
            // Rewarded ad failed to load
            // AppLovin recommends that you retry with exponentially higher delays, up to a maximum delay (in this case 64 seconds).

            Debug.LogError($"{nameof(ApplovinManager)} | Ad loaded failed, error {errorInfo.AdLoadFailureInfo}");

            retryAttempt++;
            double retryDelay = Mathf.Pow(2, Mathf.Min(6, retryAttempt));

            Invoke(nameof(LoadRewardedAd), (float)retryDelay);
        }

        void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {

        }

        void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad failed to display. AppLovin recommends that you load the next ad.
            LoadRewardedAd();
        }

        void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        void OnRewardedAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Rewarded ad is hidden. Pre-load the next ad
            LoadRewardedAd();
        }

        void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
        {
            // The rewarded ad displayed and the user should receive the reward.
            Debug.Log($"{nameof(ApplovinManager)} | On Rewarded received for ad ID: " + adUnitId);
        }

        void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            // Ad revenue paid. Use this callback to track user revenue.
        }
        #endregion

        #region Banner Ad
        public void ShowBannerAd()
        {
            MaxSdk.ShowBanner(bannerAdUnitId);
        }

        public void HideBannerAd()
        {
            MaxSdk.HideBanner(bannerAdUnitId);
        }


        void InitializeBannerAds()
        {
            // Banners are automatically sized to 320?50 on phones and 728?90 on tablets
            // You may call the utility method MaxSdkUtils.isTablet() to help with view sizing adjustments
            Debug.Log($"AppLovinManager | Create Banner Ad...");

            if (bannerAdUISetting.useCustomPosition)
            {
                MaxSdk.CreateBanner(bannerAdUnitId, bannerAdUISetting.customPosition.x, bannerAdUISetting.customPosition.y);
            }
            else
            {
                var adViewConfiguration = new MaxSdk.AdViewConfiguration(bannerAdUISetting.alignment)
                {
                    IsAdaptive = bannerAdUISetting.screenAdaptive
                };
                MaxSdk.CreateBanner(bannerAdUnitId, adViewConfiguration);
            }

            // Set background color for banners to be fully functional
            MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, bannerAdUISetting.color);

            RegisterBannerAdEvents();

        }

        void RegisterBannerAdEvents()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent += OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent += OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += OnBannerAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent += OnBannerAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent += OnBannerAdCollapsedEvent;
        }

        void UnRegisterBannerAdEvents()
        {
            MaxSdkCallbacks.Banner.OnAdLoadedEvent -= OnBannerAdLoadedEvent;
            MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= OnBannerAdLoadFailedEvent;
            MaxSdkCallbacks.Banner.OnAdClickedEvent -= OnBannerAdClickedEvent;
            MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= OnBannerAdRevenuePaidEvent;
            MaxSdkCallbacks.Banner.OnAdExpandedEvent -= OnBannerAdExpandedEvent;
            MaxSdkCallbacks.Banner.OnAdCollapsedEvent -= OnBannerAdCollapsedEvent;
        }

        void OnBannerAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }


        void OnBannerAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        void OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }


        void OnBannerAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo) { }

        void OnBannerAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo adInfo)
        {
            //Some Ad network dont have banner Ad, so you dont need show the error log on the console
            //Debug.LogError($"AppLovinManager | Banner Ad failed to load! Ad id {adUnitId}, error detail: {adInfo.AdLoadFailureInfo}, Network {adInfo.MediatedNetworkErrorMessage}");
        }

        void OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
        {
            Debug.Log($"AppLovinManager | Banner Ad loaded succesfully! Ad Id {adUnitId}");
        }

        #endregion
    }
}
