using UnityEngine;

namespace PunchKing
{
    /// <summary>
    /// 업적 데이터 ScriptableObject
    /// 다양한 업적 조건 및 보상 정의
    /// </summary>
    [CreateAssetMenu(fileName = "New Achievement", menuName = "PunchKing/Achievement Data", order = 2)]
    public class AchievementData : ScriptableObject
    {
        [Header("기본 정보")]
        public string achievementId;
        public string achievementName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("조건")]
        public AchievementType type;
        public long targetValue;  // 목표 값
        public int tier = 1;      // 티어 (같은 업적의 단계)

        [Header("보상")]
        public RewardType rewardType;
        public double rewardAmount;

        [Header("UI")]
        public bool isSecret = false;  // 비밀 업적

        /// <summary>
        /// 업적 타입
        /// </summary>
        public enum AchievementType
        {
            TotalClicks,        // 총 클릭 횟수
            TotalGold,          // 총 획득 골드
            ReachStage,         // 스테이지 도달
            DefeatBosses,       // 보스 격파 횟수
            PrestigeCount,      // 프레스티지 횟수
            UpgradeLevel,       // 특정 업그레이드 레벨
            UseSkills,          // 스킬 사용 횟수
            CriticalHits,       // 크리티컬 타격 횟수
            UnlockCoaches,      // 코치 잠금 해제
            PlayTime,           // 플레이 시간 (초)
            DailyLogins,        // 연속 로그인 일수
            CompleteQuests      // 퀘스트 완료 횟수
        }

        /// <summary>
        /// 보상 타입
        /// </summary>
        public enum RewardType
        {
            Gold,               // 골드
            PrestigeCurrency,   // 프레스티지 화폐
            DamageMultiplier,   // 영구 데미지 배율
            GoldMultiplier,     // 영구 골드 배율
            CritChance          // 크리티컬 확률 증가
        }

        /// <summary>
        /// 업적 진행도 계산 (0~1)
        /// </summary>
        public float GetProgress(long currentValue)
        {
            if (targetValue <= 0) return 1f;
            return Mathf.Clamp01((float)currentValue / targetValue);
        }

        /// <summary>
        /// 업적 완료 여부
        /// </summary>
        public bool IsCompleted(long currentValue)
        {
            return currentValue >= targetValue;
        }

        /// <summary>
        /// 업적 설명 텍스트 (진행도 포함)
        /// </summary>
        public string GetDescriptionWithProgress(long currentValue)
        {
            if (isSecret && !IsCompleted(currentValue))
            {
                return "??? 비밀 업적";
            }

            string progress = $"{FormatNumber(currentValue)} / {FormatNumber(targetValue)}";
            return $"{description}\n진행도: {progress}";
        }

        /// <summary>
        /// 보상 설명
        /// </summary>
        public string GetRewardDescription()
        {
            switch (rewardType)
            {
                case RewardType.Gold:
                    return $"💰 {new BigNumber(rewardAmount).ToKoreanString()} 골드";
                case RewardType.PrestigeCurrency:
                    return $"💎 {rewardAmount} 프레스티지 화폐";
                case RewardType.DamageMultiplier:
                    return $"⚔️ 데미지 +{rewardAmount * 100}% (영구)";
                case RewardType.GoldMultiplier:
                    return $"💰 골드 +{rewardAmount * 100}% (영구)";
                case RewardType.CritChance:
                    return $"🎯 크리티컬 +{rewardAmount * 100}% (영구)";
                default:
                    return "보상";
            }
        }

        /// <summary>
        /// 숫자 포맷
        /// </summary>
        string FormatNumber(long value)
        {
            if (value < 1000)
                return value.ToString();
            else if (value < 1000000)
                return $"{value / 1000f:F1}K";
            else if (value < 1000000000)
                return $"{value / 1000000f:F1}M";
            else
                return $"{value / 1000000000f:F1}B";
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // ID 자동 생성
            if (string.IsNullOrEmpty(achievementId))
            {
                achievementId = $"{type}_{tier}_{UnityEngine.Random.Range(1000, 9999)}";
            }

            // 목표값 양수 확인
            if (targetValue < 0)
                targetValue = 0;

            // 보상 양수 확인
            if (rewardAmount < 0)
                rewardAmount = 0;
        }
#endif
    }
}
