using UnityEngine;
using UnityEngine.Events;

namespace PunchKing
{
    /// <summary>
    /// 프레스티지 시스템
    /// 스테이지 100 이상에서 프레스티지 가능
    /// 프레스티지 화폐를 얻고 영구 보너스 제공
    /// </summary>
    public class PrestigeManager : MonoBehaviour
    {
        public static PrestigeManager Instance { get; private set; }

        [Header("프레스티지 데이터")]
        public int prestigeCount = 0;
        public BigNumber prestigeCurrency = new BigNumber(0);

        [Header("프레스티지 설정")]
        [SerializeField] private int minStageForPrestige = 100;
        [SerializeField] private float prestigeBonusPerCurrency = 0.1f;  // 10% per currency

        [Header("이벤트")]
        public UnityEvent OnPrestige;

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

            if (OnPrestige == null)
                OnPrestige = new UnityEvent();
        }

        /// <summary>
        /// 프레스티지 가능 여부 확인
        /// </summary>
        public bool CanPrestige()
        {
            if (GameManager.Instance == null) return false;

            return GameManager.Instance.currentStage >= minStageForPrestige;
        }

        /// <summary>
        /// 프레스티지 보상 계산
        /// </summary>
        public BigNumber CalculatePrestigeReward()
        {
            if (GameManager.Instance == null) return new BigNumber(0);

            int currentStage = GameManager.Instance.currentStage;

            if (currentStage < minStageForPrestige)
                return new BigNumber(0);

            // 프레스티지 화폐 = floor(currentStage / 10)
            double reward = Mathf.Floor(currentStage / 10f);
            return new BigNumber(reward);
        }

        /// <summary>
        /// 프레스티지 실행
        /// </summary>
        public void PerformPrestige()
        {
            if (!CanPrestige())
            {
                Debug.LogWarning($"프레스티지 불가능! (최소 스테이지: {minStageForPrestige})");
                return;
            }

            // 프레스티지 보상 계산
            BigNumber reward = CalculatePrestigeReward();
            prestigeCurrency = prestigeCurrency.Add(reward);
            prestigeCount++;

            Debug.Log($"프레스티지 완료! 보상: {reward.ToKoreanString()} (총: {prestigeCurrency.ToKoreanString()})");

            // 게임 리셋
            ResetGameForPrestige();

            // 이벤트 발생
            OnPrestige?.Invoke();

            // 사운드
            AudioManager.Instance?.PlaySound("Prestige");

            // 저장
            SaveManager.Instance?.SaveGame();
        }

        /// <summary>
        /// 프레스티지 후 게임 리셋
        /// </summary>
        void ResetGameForPrestige()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetGameForPrestige();
            }

            // 업그레이드 리셋
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.ResetUpgrades();
            }

            // 코치와 스킨은 유지됨
        }

        /// <summary>
        /// 프레스티지 보너스를 데미지에 적용
        /// </summary>
        public BigNumber ApplyPrestigeBonus(BigNumber baseDamage)
        {
            if (prestigeCurrency.IsZero())
                return baseDamage;

            // 프레스티지 배수: 1 + (prestigeCurrency × 0.1)
            double multiplier = 1.0 + (prestigeCurrency.ToDouble() * prestigeBonusPerCurrency);
            return baseDamage.Multiply(multiplier);
        }

        /// <summary>
        /// 프레스티지 보너스 배율 가져오기
        /// </summary>
        public float GetPrestigeMultiplier()
        {
            if (prestigeCurrency.IsZero())
                return 1f;

            return (float)(1.0 + (prestigeCurrency.ToDouble() * prestigeBonusPerCurrency));
        }

        /// <summary>
        /// 프레스티지 정보 텍스트
        /// </summary>
        public string GetPrestigeInfoText()
        {
            string info = $"프레스티지 횟수: {prestigeCount}\n";
            info += $"프레스티지 화폐: {prestigeCurrency.ToKoreanString()}\n";
            info += $"데미지 보너스: +{(GetPrestigeMultiplier() - 1f) * 100f:F1}%\n\n";

            if (CanPrestige())
            {
                BigNumber reward = CalculatePrestigeReward();
                info += $"<color=green>프레스티지 가능!</color>\n";
                info += $"보상: {reward.ToKoreanString()} 프레스티지 화폐";
            }
            else
            {
                int remaining = minStageForPrestige - GameManager.Instance.currentStage;
                info += $"<color=yellow>프레스티지까지 {remaining} 스테이지 남음</color>";
            }

            return info;
        }

#if UNITY_EDITOR
        [ContextMenu("Force Prestige")]
        void EditorPrestige()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentStage = minStageForPrestige;
            }
            PerformPrestige();
            Debug.Log("강제 프레스티지 실행됨");
        }

        [ContextMenu("Add 10 Prestige Currency")]
        void EditorAddPrestigeCurrency()
        {
            prestigeCurrency = prestigeCurrency.Add(new BigNumber(10));
            Debug.Log($"프레스티지 화폐 +10 (총: {prestigeCurrency.ToKoreanString()})");
        }
#endif
    }
}
