using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PunchKing
{
    /// <summary>
    /// 보상형 광고 버튼 UI
    /// 광고 시청 및 쿨다운 표시
    /// </summary>
    public class RewardedAdButton : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        public Button adButton;
        public TextMeshProUGUI buttonText;
        public TextMeshProUGUI cooldownText;
        public Image cooldownOverlay;
        public GameObject availableIcon;

        [Header("보상 설정")]
        public AdManager.RewardType rewardType = AdManager.RewardType.Gold;
        public string buttonLabel = "광고 보고 골드 2배 받기";

        [Header("색상")]
        public Color availableColor = Color.green;
        public Color cooldownColor = Color.gray;

        private bool isAvailable = false;

        void Start()
        {
            if (adButton == null)
                adButton = GetComponent<Button>();

            if (adButton != null)
            {
                adButton.onClick.AddListener(OnButtonClicked);
            }

            if (buttonText != null)
            {
                buttonText.text = buttonLabel;
            }
        }

        void Update()
        {
            UpdateButtonState();
        }

        /// <summary>
        /// 버튼 상태 업데이트
        /// </summary>
        void UpdateButtonState()
        {
            if (AdManager.Instance == null) return;

            bool canShow = AdManager.Instance.CanShowRewardedAd();
            bool isReady = AdManager.Instance.IsRewardedAdReady();
            isAvailable = canShow && isReady;

            // 버튼 활성화 상태
            if (adButton != null)
            {
                adButton.interactable = isAvailable;
            }

            // 쿨다운 텍스트
            if (cooldownText != null)
            {
                if (!canShow)
                {
                    float remainingSeconds = AdManager.Instance.GetRewardedAdCooldown();
                    cooldownText.gameObject.SetActive(true);
                    cooldownText.text = $"대기: {remainingSeconds:F0}초";
                }
                else if (!isReady)
                {
                    cooldownText.gameObject.SetActive(true);
                    cooldownText.text = "로딩 중...";
                }
                else
                {
                    cooldownText.gameObject.SetActive(false);
                }
            }

            // 쿨다운 오버레이
            if (cooldownOverlay != null)
            {
                if (!canShow)
                {
                    float remainingSeconds = AdManager.Instance.GetRewardedAdCooldown();
                    float totalCooldown = AdManager.Instance.rewardedAdCooldownSeconds;
                    float fillAmount = 1f - (remainingSeconds / totalCooldown);
                    cooldownOverlay.fillAmount = fillAmount;
                    cooldownOverlay.gameObject.SetActive(true);
                }
                else
                {
                    cooldownOverlay.gameObject.SetActive(false);
                }
            }

            // 사용 가능 아이콘
            if (availableIcon != null)
            {
                availableIcon.SetActive(isAvailable);
            }

            // 버튼 색상
            if (buttonText != null)
            {
                buttonText.color = isAvailable ? availableColor : cooldownColor;
            }
        }

        /// <summary>
        /// 버튼 클릭
        /// </summary>
        void OnButtonClicked()
        {
            if (AdManager.Instance == null)
            {
                Debug.LogWarning("[RewardedAdButton] AdManager가 없습니다.");
                return;
            }

            if (!isAvailable)
            {
                // 사용 불가 메시지
                if (!AdManager.Instance.CanShowRewardedAd())
                {
                    float remainingSeconds = AdManager.Instance.GetRewardedAdCooldown();
                    UIManager.Instance?.ShowBuffNotification(
                        "광고 대기 중",
                        $"{remainingSeconds:F0}초 후 다시 시청할 수 있습니다.",
                        2f
                    );
                }
                else if (!AdManager.Instance.IsRewardedAdReady())
                {
                    UIManager.Instance?.ShowBuffNotification(
                        "광고 로딩 중",
                        "광고를 불러오고 있습니다. 잠시 후 다시 시도해주세요.",
                        2f
                    );
                }
                return;
            }

            // 광고 표시
            AdManager.Instance.ShowRewardedAd(rewardType, OnAdWatchedSuccess);
        }

        /// <summary>
        /// 광고 시청 성공 콜백
        /// </summary>
        void OnAdWatchedSuccess()
        {
            Debug.Log($"[RewardedAdButton] 광고 시청 완료: {rewardType}");

            // 파티클 효과 등 추가 가능
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("Reward");
            }
        }

        /// <summary>
        /// 보상 타입 변경
        /// </summary>
        public void SetRewardType(AdManager.RewardType type)
        {
            rewardType = type;
            UpdateButtonLabel();
        }

        /// <summary>
        /// 버튼 라벨 업데이트
        /// </summary>
        void UpdateButtonLabel()
        {
            if (buttonText == null) return;

            switch (rewardType)
            {
                case AdManager.RewardType.Gold:
                    buttonLabel = "광고 보고 골드 2배 받기";
                    break;
                case AdManager.RewardType.DoubleOfflineGold:
                    buttonLabel = "광고 보고 오프라인 골드 받기";
                    break;
                case AdManager.RewardType.FreeSkillUse:
                    buttonLabel = "광고 보고 스킬 리셋";
                    break;
                case AdManager.RewardType.TimeSkip:
                    buttonLabel = "광고 보고 쿨다운 감소";
                    break;
                case AdManager.RewardType.ExtraLife:
                    buttonLabel = "광고 보고 추가 생명";
                    break;
            }

            buttonText.text = buttonLabel;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Inspector에서 버튼 라벨 변경 시 자동 업데이트
        /// </summary>
        void OnValidate()
        {
            if (buttonText != null && !string.IsNullOrEmpty(buttonLabel))
            {
                buttonText.text = buttonLabel;
            }
        }
#endif
    }
}
