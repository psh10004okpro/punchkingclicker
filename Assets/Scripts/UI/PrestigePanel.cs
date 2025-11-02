using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PunchKing
{
    /// <summary>
    /// 프레스티지 UI 패널
    /// 프레스티지 정보 및 실행
    /// </summary>
    public class PrestigePanel : MonoBehaviour
    {
        [Header("정보 표시")]
        public TextMeshProUGUI prestigeCountText;
        public TextMeshProUGUI prestigeCurrencyText;
        public TextMeshProUGUI currentBonusText;
        public TextMeshProUGUI rewardText;
        public TextMeshProUGUI requirementText;

        [Header("버튼")]
        public Button prestigeButton;
        public Image buttonBackground;

        [Header("색상")]
        public Color canPrestigeColor = new Color(1f, 0.84f, 0f);
        public Color cannotPrestigeColor = Color.gray;

        void OnEnable()
        {
            UpdateUI();
        }

        void Start()
        {
            if (prestigeButton != null)
            {
                prestigeButton.onClick.AddListener(OnPrestigeClicked);
            }

            // 스테이지 변경 이벤트 구독
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStageChanged.AddListener((_) => UpdateUI());
            }
        }

        /// <summary>
        /// UI 업데이트
        /// </summary>
        void UpdateUI()
        {
            if (PrestigeManager.Instance == null) return;

            // 프레스티지 횟수
            if (prestigeCountText != null)
            {
                prestigeCountText.text = $"프레스티지 횟수: {PrestigeManager.Instance.prestigeCount}";
            }

            // 프레스티지 화폐
            if (prestigeCurrencyText != null)
            {
                prestigeCurrencyText.text = $"보유 화폐: {PrestigeManager.Instance.prestigeCurrency.ToKoreanString()}";
            }

            // 현재 보너스
            if (currentBonusText != null)
            {
                float bonus = (PrestigeManager.Instance.GetPrestigeMultiplier() - 1f) * 100f;
                currentBonusText.text = $"현재 데미지 보너스: +{bonus:F1}%";
            }

            // 프레스티지 가능 여부
            bool canPrestige = PrestigeManager.Instance.CanPrestige();
            BigNumber reward = PrestigeManager.Instance.CalculatePrestigeReward();

            if (canPrestige)
            {
                // 보상 표시
                if (rewardText != null)
                {
                    rewardText.text = $"<color=yellow>획득 화폐: {reward.ToKoreanString()}</color>";
                }

                // 요구사항 표시
                if (requirementText != null)
                {
                    requirementText.text = "프레스티지 가능!";
                    requirementText.color = Color.green;
                }

                // 버튼 활성화
                if (prestigeButton != null)
                {
                    prestigeButton.interactable = true;
                }

                if (buttonBackground != null)
                {
                    buttonBackground.color = canPrestigeColor;
                }
            }
            else
            {
                // 보상 표시 (미리보기)
                if (rewardText != null)
                {
                    rewardText.text = $"획득 예정: {reward.ToKoreanString()}";
                }

                // 요구사항 표시
                if (requirementText != null)
                {
                    int currentStage = GameManager.Instance?.currentStage ?? 1;
                    int required = 100; // minStageForPrestige
                    int remaining = required - currentStage;

                    requirementText.text = $"스테이지 {remaining} 부족";
                    requirementText.color = Color.red;
                }

                // 버튼 비활성화
                if (prestigeButton != null)
                {
                    prestigeButton.interactable = false;
                }

                if (buttonBackground != null)
                {
                    buttonBackground.color = cannotPrestigeColor;
                }
            }
        }

        /// <summary>
        /// 프레스티지 버튼 클릭
        /// </summary>
        void OnPrestigeClicked()
        {
            ShowPrestigeConfirmation();
        }

        /// <summary>
        /// 프레스티지 확인 팝업
        /// </summary>
        void ShowPrestigeConfirmation()
        {
            if (PrestigeManager.Instance == null) return;

            BigNumber reward = PrestigeManager.Instance.CalculatePrestigeReward();

            string message = $"프레스티지를 실행하시겠습니까?\n\n";
            message += $"획득 화폐: {reward.ToKoreanString()}\n\n";
            message += "경고: 스테이지, 골드, 업그레이드가 초기화됩니다!\n";
            message += "(코치, 스킨, 프레스티지 화폐는 유지)";

            // 확인 팝업 (임시로 바로 실행)
            PerformPrestige();
        }

        /// <summary>
        /// 프레스티지 실행
        /// </summary>
        void PerformPrestige()
        {
            if (PrestigeManager.Instance != null)
            {
                PrestigeManager.Instance.PerformPrestige();
                UpdateUI();

                // 프레스티지 효과 연출
                PlayPrestigeEffect();
            }
        }

        /// <summary>
        /// 프레스티지 효과
        /// </summary>
        void PlayPrestigeEffect()
        {
            // 화면 플래시 효과 등
            Debug.Log("프레스티지 실행!");
        }

        /// <summary>
        /// 패널 열기
        /// </summary>
        public void Open()
        {
            gameObject.SetActive(true);
            UpdateUI();
        }

        /// <summary>
        /// 패널 닫기
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }

        void Update()
        {
            // 0.5초마다 UI 업데이트
            if (Time.frameCount % 30 == 0)
            {
                UpdateUI();
            }
        }
    }
}
