using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AdService))]
public class AdServiceEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();
        
    AdService adService = (AdService)target;
        
    GUILayout.Space(20);
    GUILayout.Label("🧪 TEST CONTROLS", EditorStyles.boldLabel);
        
    if (GUILayout.Button("1. Initialize Ads"))
    {
      adService.SendMessage("InitializeAds");
    }
        
    GUILayout.Space(5);
        
    if (GUILayout.Button("2. Load Banner"))
    {
      adService.LoadBanner();
    }
        
    if (GUILayout.Button("3. Show Banner"))
    {
      adService.ShowBanner();
    }
        
    if (GUILayout.Button("4. Hide Banner"))
    {
      adService.HideBanner();
    }
        
    GUILayout.Space(5);
        
    if (GUILayout.Button("Load Rewarded Ad"))
    {
      adService.LoadRewardedAd();
    }
        
    if (GUILayout.Button("Show Rewarded Ad"))
    {
      adService.ShowRewardedAd();
    }
        
    GUILayout.Space(5);
        
    if (GUILayout.Button("Load Interstitial"))
    {
      adService.LoadInterstitialAd();
    }
        
    if (GUILayout.Button("Show Interstitial"))
    {
      adService.ShowInterstitialAd();
    }
        
    GUILayout.Space(10);
    GUILayout.Label($"📱 Platform: {GetPlatform()}", EditorStyles.boldLabel);
  }
    
  string GetPlatform()
  {
#if UNITY_ANDROID
    return "Android";
#elif UNITY_IOS
            return "iOS";
#else
            return "Editor (Android fallback)";
#endif
  }
}