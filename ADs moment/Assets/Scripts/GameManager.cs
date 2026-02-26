using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    public Button showBannerButton;
    public Button hideBannerButton;
    public Button showRewardedButton;
    public Button showInterstitialButton;
    public Text statusText;

    [Header("Game Settings")]
    public int playerCoins = 100;
    public Text coinsText;

    void Start()
    {
        UpdateUI();

        // Subscribe to ad events
        if (AdService.Instance != null)
        {
            AdService.Instance.OnRewardedAdCompleted += OnRewardedAdCompleted;
            AdService.Instance.OnRewardedAdFailed += OnRewardedAdFailed;
            AdService.Instance.OnInterstitialAdClosed += OnInterstitialAdClosed;
        }

        // Setup buttons
        showBannerButton.onClick.AddListener(() => AdService.Instance?.ShowBanner());
        hideBannerButton.onClick.AddListener(() => AdService.Instance?.HideBanner());
        showRewardedButton.onClick.AddListener(() => AdService.Instance?.ShowRewardedAd());
        showInterstitialButton.onClick.AddListener(() => AdService.Instance?.ShowInterstitialAd());

        // Auto-load banner
        Invoke(nameof(DelayedBannerLoad), 2f);
    }

    void DelayedBannerLoad()
    {
        AdService.Instance?.LoadBanner();
    }

    void OnRewardedAdCompleted()
    {
        // Give reward
        playerCoins += 50;
        statusText.text = "🎁 Reward granted: +50 coins!";
        UpdateUI();
    }

    void OnRewardedAdFailed()
    {
        statusText.text = "❌ Ad failed. Try again later.";
    }

    void OnInterstitialAdClosed()
    {
        statusText.text = "↩️ Returned from interstitial ad";
    }

    void UpdateUI()
    {
        if (coinsText != null)
            coinsText.text = $"Coins: {playerCoins}";
    }
}