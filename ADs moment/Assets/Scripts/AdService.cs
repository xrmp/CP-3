using UnityEngine;
using UnityEngine.Advertisements;
using System;

public class AdService : MonoBehaviour, IUnityAdsInitializationListener,
    IUnityAdsLoadListener, IUnityAdsShowListener
{
    [Header("Unity Ads Settings")]
    [SerializeField] private bool testMode = true;

    [SerializeField] private string androidGameId = "YOUR_ANDROID_GAME_ID";
    [SerializeField] private string iosGameId = "YOUR_IOS_GAME_ID";

    [SerializeField] private string bannerPlacement = "Banner_Android";
    [SerializeField] private string rewardedPlacement = "Rewarded_Android";
    [SerializeField] private string interstitialPlacement = "Interstitial_Android";

    [SerializeField] private string bannerPlacementIOS = "Banner_iOS";
    [SerializeField] private string rewardedPlacementIOS = "Rewarded_iOS";
    [SerializeField] private string interstitialPlacementIOS = "Interstitial_iOS";

    private string currentGameId;
    private string currentBannerId;
    private string currentRewardedId;
    private string currentInterstitialId;

    [SerializeField] private BannerPosition bannerPosition = BannerPosition.BOTTOM_CENTER;

    public Action OnRewardedAdCompleted;
    public Action OnRewardedAdFailed;
    public Action OnInterstitialAdClosed;

    public static AdService Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAds()
    {
#if UNITY_ANDROID
        currentGameId = androidGameId;
        currentBannerId = bannerPlacement;
        currentRewardedId = rewardedPlacement;
        currentInterstitialId = interstitialPlacement;
#elif UNITY_IOS
            currentGameId = iosGameId;
            currentBannerId = bannerPlacementIOS;
            currentRewardedId = rewardedPlacementIOS;
            currentInterstitialId = interstitialPlacementIOS;
#else
            currentGameId = androidGameId; 
            currentGameId = androidGameId; 
            currentBannerId = bannerPlacement;
            currentRewardedId = rewardedPlacement;
            currentInterstitialId = interstitialPlacement;
#endif

        if (string.IsNullOrEmpty(currentGameId))
        {
            Debug.LogError("❌ AdService: Game ID is not set!");
            return;
        }

        Advertisement.Initialize(currentGameId, testMode, this);
        Debug.Log($"🚀 AdService initializing with Game ID: {currentGameId}, Test Mode: {testMode}");
    }

    // =============================================
    // 🎯 BANNER ADS
    // =============================================
    public void LoadBanner()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("⚠️ Ads not initialized yet");
            return;
        }

        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        Advertisement.Banner.SetPosition(bannerPosition);
        Advertisement.Banner.Load(currentBannerId, options);
    }

    public void ShowBanner()
    {
        if (!Advertisement.isInitialized) return;

        BannerOptions options = new BannerOptions
        {
            showCallback = OnBannerShown,
            hideCallback = OnBannerHidden,
            clickCallback = OnBannerClicked
        };

        Advertisement.Banner.Show(currentBannerId, options);
        Debug.Log("📢 Banner show requested");
    }

    public void HideBanner()
    {
        Advertisement.Banner.Hide();
    }

    public void DestroyBanner()
    {
        Advertisement.Banner.Hide(true);
    }

    // Banner callbacks
    void OnBannerLoaded() => Debug.Log("✅ Banner loaded");
    void OnBannerError(string message) => Debug.LogError($"❌ Banner error: {message}");
    void OnBannerShown() => Debug.Log("📢 Banner shown");
    void OnBannerHidden() => Debug.Log("📪 Banner hidden");
    void OnBannerClicked() => Debug.Log("👆 Banner clicked");

    // =============================================
    // 🎁 REWARDED ADS
    // =============================================
    public void LoadRewardedAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("⚠️ Ads not initialized yet");
            return;
        }

        Advertisement.Load(currentRewardedId, this);
        Debug.Log("🔄 Loading rewarded ad...");
    }

    public void ShowRewardedAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("⚠️ Ads not initialized yet");
            OnRewardedAdFailed?.Invoke();
            return;
        }

        Advertisement.Show(currentRewardedId, this);
        Debug.Log("🎁 Showing rewarded ad...");
    }

    // =============================================
    // 🎬 INTERSTITIAL ADS
    // =============================================
    public void LoadInterstitialAd()
    {
        if (!Advertisement.isInitialized) return;
        Advertisement.Load(currentInterstitialId, this);
        Debug.Log("🔄 Loading interstitial ad...");
    }

    public void ShowInterstitialAd()
    {
        if (!Advertisement.isInitialized)
        {
            OnInterstitialAdClosed?.Invoke();
            return;
        }

        Advertisement.Show(currentInterstitialId, this);
        Debug.Log("🎬 Showing interstitial ad...");
    }

    // =============================================
    // 📞 AD LISTENERS
    // =============================================

    // Initialization listener
    public void OnInitializationComplete()
    {
        Debug.Log("✅ Unity Ads initialized successfully!");

        // Preload ads
        LoadBanner();
        LoadRewardedAd();
        LoadInterstitialAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"❌ Unity Ads initialization failed: {error} - {message}");
    }

    // Load listener
    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log($"✅ Ad loaded: {placementId}");
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"❌ Failed to load ad {placementId}: {error} - {message}");

        if (placementId == currentRewardedId)
            OnRewardedAdFailed?.Invoke();
    }

    // Show listener
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"❌ Failed to show ad {placementId}: {error} - {message}");

        if (placementId == currentRewardedId)
            OnRewardedAdFailed?.Invoke();
        else if (placementId == currentInterstitialId)
            OnInterstitialAdClosed?.Invoke();
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Debug.Log($"▶️ Ad started: {placementId}");
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log($"👆 Ad clicked: {placementId}");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"✅ Ad completed: {placementId}, State: {showCompletionState}");

        if (placementId == currentRewardedId)
        {
            if (showCompletionState == UnityAdsShowCompletionState.COMPLETED)
            {
                Debug.Log("🎁 REWARD GRANTED!");
                OnRewardedAdCompleted?.Invoke();
            }
            else
            {
                OnRewardedAdFailed?.Invoke();
            }

            // Reload for next time
            LoadRewardedAd();
        }
        else if (placementId == currentInterstitialId)
        {
            OnInterstitialAdClosed?.Invoke();
            LoadInterstitialAd();
        }
    }
}