using UnityEngine;
using System;

namespace PunchKing
{
    /// <summary>
    /// 광고 매니저
    /// Unity Ads 통합 및 보상형 광고
    /// </summary>
    public class AdManager : MonoBehaviour
    {
        public static AdManager Instance { get; private set; }

        [Header("Unity Ads 설정")]
        public string androidGameId = "1234567";
        public string iosGameId = "7654321";
        public bool testMode = true;

        [Header("광고 배치 ID")]
        public string rewardedAdPlacement = "Rewarded_Android";
        public string interstitialAdPlacement = "Interstitial_Android";
        public string bannerAdPlacement = "Banner_Android";

        [Header("보상 설정")]
        public RewardType defaultRewardType = RewardType.Gold;
        public float goldRewardMultiplier = 2f;
        public int rewardedAdCooldownSeconds = 30;

        [Header("이벤트")]
        public UnityEngine.Events.UnityEvent OnRewardedAdSuccess;
        public UnityEngine.Events.UnityEvent OnRewardedAdFailed;
        public UnityEngine.Events.UnityEvent OnInterstitialShown;

        private bool isInitialized = false;
        private bool isShowingAd = false;
        private DateTime lastRewardedAdTime = DateTime.MinValue;
        private Action currentRewardCallback;

        public enum RewardType
        {
            Gold,              // 골드 2배 지급
            DoubleOfflineGold, // 오프라인 골드 2배
            FreeSkillUse,      // 무료 스킬 사용
            TimeSkip,          // 시간 스킵 (쿨다운 감소)
            ExtraLife          // 보스전 생명 추가
        }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            InitializeAds();
        }

        /// <summary>
        /// 광고 SDK 초기화
        /// </summary>
        void InitializeAds()
        {
            // 광고 제거 구매 여부 확인
            if (ShopManager.Instance != null && ShopManager.Instance.adsRemoved)
            {
                Debug.Log("[AdManager] 광고가 제거되었습니다.");
                return;
            }

#if UNITY_ADS
            string gameId = GetGameId();
            Advertisement.Initialize(gameId, testMode, this);
            Debug.Log($"[AdManager] Unity Ads 초기화 시작 (Game ID: {gameId}, Test Mode: {testMode})");
#else
            Debug.Log("[AdManager] Unity Ads 패키지가 설치되지 않았습니다. 테스트 모드로 실행됩니다.");
            isInitialized = true;
#endif
        }

        /// <summary>
        /// 플랫폼별 Game ID 반환
        /// </summary>
        string GetGameId()
        {
#if UNITY_IOS
            return iosGameId;
#elif UNITY_ANDROID
            return androidGameId;
#else
            return androidGameId;
#endif
        }

        /// <summary>
        /// 보상형 광고 표시
        /// </summary>
        public void ShowRewardedAd(RewardType rewardType, Action onSuccess = null)
        {
            // 광고 제거 구매 여부 확인
            if (ShopManager.Instance != null && ShopManager.Instance.adsRemoved)
            {
                Debug.Log("[AdManager] 광고 제거됨 - 보상만 지급");
                GiveReward(rewardType);
                onSuccess?.Invoke();
                return;
            }

            // 쿨다운 체크
            if (!CanShowRewardedAd())
            {
                float remainingSeconds = rewardedAdCooldownSeconds - (float)(DateTime.Now - lastRewardedAdTime).TotalSeconds;
                UIManager.Instance?.ShowBuffNotification(
                    "광고 시청 대기",
                    $"{remainingSeconds:F0}초 후 다시 시청할 수 있습니다.",
                    2f
                );
                return;
            }

            // 이미 광고 표시 중
            if (isShowingAd)
            {
                Debug.LogWarning("[AdManager] 이미 광고가 표시 중입니다.");
                return;
            }

            currentRewardCallback = onSuccess;

#if UNITY_ADS
            if (Advertisement.IsReady(rewardedAdPlacement))
            {
                isShowingAd = true;
                var options = new ShowOptions { resultCallback = (result) => HandleRewardedAdResult(result, rewardType) };
                Advertisement.Show(rewardedAdPlacement, options);
            }
            else
            {
                Debug.LogWarning("[AdManager] 보상형 광고가 준비되지 않았습니다.");
                OnRewardedAdFailed?.Invoke();
            }
#else
            // 테스트 모드: 즉시 보상 지급
            Debug.Log($"[AdManager] 테스트 모드 - 보상형 광고 시뮬레이션: {rewardType}");
            isShowingAd = true;
            Invoke(nameof(SimulateRewardedAdSuccess), 1f);
            void SimulateRewardedAdSuccess()
            {
                HandleRewardedAdResult(ShowResult.Finished, rewardType);
            }
#endif
        }

        /// <summary>
        /// 보상형 광고 결과 처리
        /// </summary>
        void HandleRewardedAdResult(ShowResult result, RewardType rewardType)
        {
            isShowingAd = false;

            switch (result)
            {
                case ShowResult.Finished:
                    Debug.Log($"[AdManager] 보상형 광고 시청 완료: {rewardType}");
                    GiveReward(rewardType);
                    lastRewardedAdTime = DateTime.Now;
                    OnRewardedAdSuccess?.Invoke();
                    currentRewardCallback?.Invoke();
                    break;

                case ShowResult.Skipped:
                    Debug.Log("[AdManager] 광고 건너뜀");
                    UIManager.Instance?.ShowBuffNotification("광고 건너뜀", "광고를 끝까지 시청해야 보상을 받을 수 있습니다.", 2f);
                    OnRewardedAdFailed?.Invoke();
                    break;

                case ShowResult.Failed:
                    Debug.LogError("[AdManager] 광고 표시 실패");
                    UIManager.Instance?.ShowBuffNotification("광고 오류", "광고를 불러올 수 없습니다.", 2f);
                    OnRewardedAdFailed?.Invoke();
                    break;
            }

            currentRewardCallback = null;
        }

        /// <summary>
        /// 보상 지급
        /// </summary>
        void GiveReward(RewardType rewardType)
        {
            if (GameManager.Instance == null) return;

            switch (rewardType)
            {
                case RewardType.Gold:
                    // 현재 골드의 2배 지급
                    BigNumber currentGold = GameManager.Instance.gold;
                    BigNumber bonusGold = currentGold.Multiply(goldRewardMultiplier);
                    GameManager.Instance.AddGold(bonusGold);
                    UIManager.Instance?.ShowBuffNotification(
                        "💰 광고 보상!",
                        $"{bonusGold.ToKoreanString()} 골드 획득!",
                        3f
                    );
                    break;

                case RewardType.DoubleOfflineGold:
                    // 1시간 치 오프라인 골드 지급
                    BigNumber offlineGold = CalculateOfflineGold(3600);
                    GameManager.Instance.AddGold(offlineGold);
                    UIManager.Instance?.ShowBuffNotification(
                        "😴 오프라인 보상!",
                        $"{offlineGold.ToKoreanString()} 골드 획득! (1시간 치)",
                        3f
                    );
                    break;

                case RewardType.FreeSkillUse:
                    // 모든 스킬 쿨다운 리셋
                    SkillManager.Instance?.ResetAllCooldowns();
                    UIManager.Instance?.ShowBuffNotification(
                        "⚡ 스킬 리셋!",
                        "모든 스킬 쿨다운이 초기화되었습니다!",
                        3f
                    );
                    break;

                case RewardType.TimeSkip:
                    // 모든 타이머 30% 감소
                    SkillManager.Instance?.ReduceAllCooldowns(0.3f);
                    UIManager.Instance?.ShowBuffNotification(
                        "⏰ 시간 스킵!",
                        "모든 쿨다운이 30% 감소했습니다!",
                        3f
                    );
                    break;

                case RewardType.ExtraLife:
                    // 보스전에서 사용할 수 있는 추가 생명 (GameManager에 변수 추가 필요)
                    UIManager.Instance?.ShowBuffNotification(
                        "❤️ 추가 생명!",
                        "다음 보스전에서 추가 생명을 얻습니다!",
                        3f
                    );
                    break;
            }

            // 오디오
            AudioManager.Instance?.PlaySFX("Reward");
        }

        /// <summary>
        /// 오프라인 골드 계산
        /// </summary>
        BigNumber CalculateOfflineGold(int seconds)
        {
            if (GameManager.Instance == null) return BigNumber.Zero;

            // GameManager의 초당 골드 계산 로직 사용
            BigNumber goldPerSecond = GameManager.Instance.CalculateGoldPerSecond();
            return goldPerSecond.Multiply(seconds);
        }

        /// <summary>
        /// 전면 광고 표시
        /// </summary>
        public void ShowInterstitialAd()
        {
            // 광고 제거 구매 여부 확인
            if (ShopManager.Instance != null && ShopManager.Instance.adsRemoved)
            {
                Debug.Log("[AdManager] 광고가 제거되었습니다.");
                return;
            }

#if UNITY_ADS
            if (Advertisement.IsReady(interstitialAdPlacement))
            {
                Advertisement.Show(interstitialAdPlacement);
                OnInterstitialShown?.Invoke();
                Debug.Log("[AdManager] 전면 광고 표시");
            }
            else
            {
                Debug.LogWarning("[AdManager] 전면 광고가 준비되지 않았습니다.");
            }
#else
            Debug.Log("[AdManager] 테스트 모드 - 전면 광고 시뮬레이션");
#endif
        }

        /// <summary>
        /// 배너 광고 표시
        /// </summary>
        public void ShowBannerAd()
        {
            // 광고 제거 구매 여부 확인
            if (ShopManager.Instance != null && ShopManager.Instance.adsRemoved)
            {
                Debug.Log("[AdManager] 광고가 제거되었습니다.");
                return;
            }

#if UNITY_ADS
            Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
            Advertisement.Banner.Show(bannerAdPlacement);
            Debug.Log("[AdManager] 배너 광고 표시");
#else
            Debug.Log("[AdManager] 테스트 모드 - 배너 광고 시뮬레이션");
#endif
        }

        /// <summary>
        /// 배너 광고 숨기기
        /// </summary>
        public void HideBannerAd()
        {
#if UNITY_ADS
            Advertisement.Banner.Hide();
#endif
        }

        /// <summary>
        /// 보상형 광고 표시 가능 여부
        /// </summary>
        public bool CanShowRewardedAd()
        {
            if (isShowingAd) return false;

            // 쿨다운 체크
            double elapsedSeconds = (DateTime.Now - lastRewardedAdTime).TotalSeconds;
            return elapsedSeconds >= rewardedAdCooldownSeconds;
        }

        /// <summary>
        /// 보상형 광고 준비 여부
        /// </summary>
        public bool IsRewardedAdReady()
        {
#if UNITY_ADS
            return Advertisement.IsReady(rewardedAdPlacement);
#else
            return true; // 테스트 모드는 항상 준비됨
#endif
        }

        /// <summary>
        /// 남은 쿨다운 시간 (초)
        /// </summary>
        public float GetRewardedAdCooldown()
        {
            if (!CanShowRewardedAd())
            {
                return rewardedAdCooldownSeconds - (float)(DateTime.Now - lastRewardedAdTime).TotalSeconds;
            }
            return 0f;
        }

#if UNITY_EDITOR
        [ContextMenu("Test Show Rewarded Ad")]
        void TestShowRewardedAd()
        {
            ShowRewardedAd(RewardType.Gold);
        }

        [ContextMenu("Reset Ad Cooldown")]
        void ResetAdCooldown()
        {
            lastRewardedAdTime = DateTime.MinValue;
            Debug.Log("[AdManager] 광고 쿨다운 초기화");
        }
#endif

        // Unity Ads 콜백 인터페이스 구현을 위한 더미 정의
        enum ShowResult
        {
            Finished,
            Skipped,
            Failed
        }
    }
}
