using UnityEngine;

namespace PunchKing
{
    /// <summary>
    /// 업그레이드 데이터를 정의하는 ScriptableObject
    /// Unity 에디터에서 쉽게 업그레이드를 생성하고 관리할 수 있음
    /// </summary>
    [CreateAssetMenu(fileName = "New Upgrade", menuName = "PunchKing/Upgrade Data", order = 1)]
    public class UpgradeData : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("업그레이드 이름 (예: 펀치력 증가)")]
        public string upgradeName = "업그레이드";

        [Tooltip("업그레이드 설명")]
        [TextArea(2, 4)]
        public string description = "클릭당 펀치력을 증가시킵니다.";

        [Tooltip("업그레이드 아이콘")]
        public Sprite icon;

        [Tooltip("업그레이드 타입")]
        public UpgradeType type;

        [Header("비용 설정")]
        [Tooltip("초기 비용")]
        public double baseCost = 10;

        [Tooltip("레벨당 비용 증가 배율 (1.15 = 15% 증가)")]
        public float costMultiplier = 1.15f;

        [Tooltip("최대 레벨 (0 = 무제한)")]
        public int maxLevel = 0;

        [Header("효과 설정")]
        [Tooltip("기본 효과 값")]
        public float baseEffect = 1f;

        [Tooltip("레벨당 효과 증가량")]
        public float effectPerLevel = 1f;

        [Tooltip("효과 타입 (덧셈/곱셈)")]
        public EffectCalculationType effectType = EffectCalculationType.Additive;

        [Header("잠금 조건")]
        [Tooltip("이 업그레이드를 잠금 해제하는데 필요한 스테이지")]
        public int requiredStage = 1;

        [Tooltip("이 업그레이드를 잠금 해제하는데 필요한 프레스티지 횟수")]
        public int requiredPrestige = 0;

        /// <summary>
        /// 업그레이드 종류
        /// </summary>
        public enum UpgradeType
        {
            PunchPower,         // 펀치력 증가
            AutoClicker,        // 자동 클릭 추가
            CriticalChance,     // 크리티컬 확률 증가
            CriticalDamage,     // 크리티컬 데미지 증가
            GoldMultiplier,     // 골드 획득 배율 증가
            OfflineEarnings,    // 오프라인 수익 증가
            ClickDamage,        // 클릭 데미지 배율
            GoldPerClick        // 클릭당 골드
        }

        /// <summary>
        /// 효과 계산 방식
        /// </summary>
        public enum EffectCalculationType
        {
            Additive,   // 덧셈 (레벨 × 효과량)
            Multiplicative  // 곱셈 (1 + 레벨 × 효과량)
        }

        /// <summary>
        /// 특정 레벨의 비용 계산
        /// </summary>
        public BigNumber GetCost(int currentLevel)
        {
            if (currentLevel < 0)
                currentLevel = 0;

            double cost = baseCost * Mathf.Pow(costMultiplier, currentLevel);
            return new BigNumber(cost);
        }

        /// <summary>
        /// 특정 레벨의 효과 값 계산
        /// </summary>
        public float GetEffect(int level)
        {
            if (level <= 0)
                return 0f;

            switch (effectType)
            {
                case EffectCalculationType.Additive:
                    // 덧셈: baseEffect + (level × effectPerLevel)
                    return baseEffect + (level * effectPerLevel);

                case EffectCalculationType.Multiplicative:
                    // 곱셈: 1 + (level × effectPerLevel)
                    return 1f + (level * effectPerLevel);

                default:
                    return baseEffect;
            }
        }

        /// <summary>
        /// 레벨 제한 확인
        /// </summary>
        public bool IsMaxLevel(int currentLevel)
        {
            if (maxLevel <= 0)
                return false;  // 무제한

            return currentLevel >= maxLevel;
        }

        /// <summary>
        /// 잠금 해제 가능 여부
        /// </summary>
        public bool IsUnlocked(int currentStage, int prestigeCount)
        {
            return currentStage >= requiredStage && prestigeCount >= requiredPrestige;
        }

        /// <summary>
        /// 업그레이드 정보를 텍스트로 반환
        /// </summary>
        public string GetInfoText(int currentLevel)
        {
            string info = $"<b>{upgradeName}</b>\n";
            info += $"{description}\n\n";

            if (currentLevel > 0)
            {
                info += $"현재 레벨: {currentLevel}\n";
                info += $"현재 효과: {GetEffect(currentLevel):F1}\n";
            }

            if (!IsMaxLevel(currentLevel))
            {
                info += $"다음 비용: {GetCost(currentLevel).ToKoreanString()}\n";
                info += $"다음 효과: {GetEffect(currentLevel + 1):F1}";
            }
            else
            {
                info += "<color=yellow>최대 레벨 달성!</color>";
            }

            return info;
        }

        /// <summary>
        /// 업그레이드 타입 설명
        /// </summary>
        public string GetTypeDescription()
        {
            switch (type)
            {
                case UpgradeType.PunchPower:
                    return "클릭 데미지를 증가시킵니다";
                case UpgradeType.AutoClicker:
                    return "자동으로 초당 골드를 생성합니다";
                case UpgradeType.CriticalChance:
                    return "크리티컬 확률을 증가시킵니다";
                case UpgradeType.CriticalDamage:
                    return "크리티컬 데미지 배율을 증가시킵니다";
                case UpgradeType.GoldMultiplier:
                    return "골드 획득량을 증가시킵니다";
                case UpgradeType.OfflineEarnings:
                    return "오프라인 수익을 증가시킵니다";
                case UpgradeType.ClickDamage:
                    return "클릭 데미지 배율을 증가시킵니다";
                case UpgradeType.GoldPerClick:
                    return "클릭당 골드를 획득합니다";
                default:
                    return "알 수 없는 효과";
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// 에디터에서 데이터 검증
        /// </summary>
        private void OnValidate()
        {
            // 비용은 양수여야 함
            if (baseCost < 0)
                baseCost = 0;

            // 비용 배율은 1 이상이어야 함
            if (costMultiplier < 1f)
                costMultiplier = 1f;

            // 최대 레벨은 0 이상
            if (maxLevel < 0)
                maxLevel = 0;

            // 필요 스테이지는 1 이상
            if (requiredStage < 1)
                requiredStage = 1;

            // 필요 프레스티지는 0 이상
            if (requiredPrestige < 0)
                requiredPrestige = 0;
        }
#endif
    }
}
