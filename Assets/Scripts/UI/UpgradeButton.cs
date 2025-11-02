using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PunchKing
{
    /// <summary>
    /// 개별 업그레이드 버튼 UI
    /// 업그레이드 정보 표시 및 구매 처리
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UpgradeButton : MonoBehaviour
    {
        [Header("UI 레퍼런스")]
        public Image iconImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI costText;
        public TextMeshProUGUI effectText;
        public GameObject maxLevelIndicator;
        public GameObject lockedIndicator;

        [Header("버튼")]
        public Button purchaseButton;
        public Image buttonBackground;

        [Header("색상")]
        public Color affordableColor = new Color(0.2f, 0.8f, 0.3f);
        public Color unaffordableColor = new Color(0.5f, 0.5f, 0.5f);
        public Color maxLevelColor = new Color(1f, 0.84f, 0f);

        private UpgradeData upgradeData;
        private int currentLevel;

        void Awake()
        {
            if (purchaseButton == null)
                purchaseButton = GetComponent<Button>();

            purchaseButton.onClick.AddListener(OnPurchaseClicked);
        }

        /// <summary>
        /// 업그레이드 데이터 설정
        /// </summary>
        public void Setup(UpgradeData data, int level)
        {
            upgradeData = data;
            currentLevel = level;

            UpdateUI();
        }

        /// <summary>
        /// UI 업데이트
        /// </summary>
        public void UpdateUI()
        {
            if (upgradeData == null) return;

            // 아이콘
            if (iconImage != null && upgradeData.icon != null)
            {
                iconImage.sprite = upgradeData.icon;
            }

            // 이름
            if (nameText != null)
            {
                nameText.text = upgradeData.upgradeName;
            }

            // 설명
            if (descriptionText != null)
            {
                descriptionText.text = upgradeData.description;
            }

            // 레벨
            if (levelText != null)
            {
                if (upgradeData.maxLevel > 0)
                {
                    levelText.text = $"Lv.{currentLevel}/{upgradeData.maxLevel}";
                }
                else
                {
                    levelText.text = $"Lv.{currentLevel}";
                }
            }

            // 잠금 상태 확인
            bool isUnlocked = upgradeData.IsUnlocked(
                GameManager.Instance?.currentStage ?? 1,
                PrestigeManager.Instance?.prestigeCount ?? 0
            );

            // 최대 레벨 확인
            bool isMaxLevel = upgradeData.IsMaxLevel(currentLevel);

            // 잠김 표시
            if (lockedIndicator != null)
            {
                lockedIndicator.SetActive(!isUnlocked);
            }

            // 최대 레벨 표시
            if (maxLevelIndicator != null)
            {
                maxLevelIndicator.SetActive(isMaxLevel);
            }

            // 비용 및 효과
            if (!isMaxLevel && isUnlocked)
            {
                BigNumber cost = upgradeData.GetCost(currentLevel);
                float nextEffect = upgradeData.GetEffect(currentLevel + 1);

                if (costText != null)
                {
                    costText.text = $"💰 {cost.ToKoreanString()}";
                }

                if (effectText != null)
                {
                    float currentEffect = upgradeData.GetEffect(currentLevel);
                    effectText.text = $"효과: {currentEffect:F1} → {nextEffect:F1}";
                }

                // 구매 가능 여부에 따른 색상
                bool canAfford = GameManager.Instance != null &&
                                GameManager.Instance.totalGold.IsGreaterThanOrEqual(cost);

                if (buttonBackground != null)
                {
                    buttonBackground.color = canAfford ? affordableColor : unaffordableColor;
                }

                purchaseButton.interactable = canAfford;
            }
            else if (isMaxLevel)
            {
                if (costText != null)
                {
                    costText.text = "MAX";
                }

                if (effectText != null)
                {
                    float maxEffect = upgradeData.GetEffect(currentLevel);
                    effectText.text = $"최대 효과: {maxEffect:F1}";
                }

                if (buttonBackground != null)
                {
                    buttonBackground.color = maxLevelColor;
                }

                purchaseButton.interactable = false;
            }
            else // 잠김
            {
                if (costText != null)
                {
                    costText.text = "🔒 잠김";
                }

                if (effectText != null)
                {
                    effectText.text = $"필요: 스테이지 {upgradeData.requiredStage}";
                }

                purchaseButton.interactable = false;
            }
        }

        /// <summary>
        /// 구매 버튼 클릭
        /// </summary>
        void OnPurchaseClicked()
        {
            if (upgradeData == null || UpgradeManager.Instance == null) return;

            bool success = UpgradeManager.Instance.PurchaseUpgrade(upgradeData);

            if (success)
            {
                currentLevel++;
                UpdateUI();

                // 구매 성공 애니메이션
                PlayPurchaseAnimation();
            }
            else
            {
                // 구매 실패 애니메이션
                PlayFailAnimation();
            }
        }

        /// <summary>
        /// 구매 성공 애니메이션
        /// </summary>
        void PlayPurchaseAnimation()
        {
            // 간단한 스케일 애니메이션
            transform.localScale = Vector3.one * 1.1f;
            LeanTween.scale(gameObject, Vector3.one, 0.3f).setEaseOutBack();
        }

        /// <summary>
        /// 구매 실패 애니메이션 (흔들기)
        /// </summary>
        void PlayFailAnimation()
        {
            Vector3 originalPos = transform.localPosition;
            LeanTween.moveX(gameObject, originalPos.x + 10f, 0.1f)
                .setEaseShake()
                .setLoopPingPong(1)
                .setOnComplete(() => transform.localPosition = originalPos);
        }

        /// <summary>
        /// 매 프레임 UI 업데이트 (골드 변화 감지)
        /// </summary>
        void Update()
        {
            // 0.5초마다 업데이트 (성능 최적화)
            if (Time.frameCount % 30 == 0)
            {
                UpdateUI();
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터 디버그 - 강제 레벨업
        /// </summary>
        [ContextMenu("Force Level Up")]
        void EditorLevelUp()
        {
            currentLevel++;
            UpdateUI();
        }
#endif
    }
}
