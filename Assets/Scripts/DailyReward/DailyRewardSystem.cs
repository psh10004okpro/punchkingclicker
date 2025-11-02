using UnityEngine;
using System;
using System.Collections.Generic;

namespace PunchKing
{
    /// <summary>
    /// 일일 보상 시스템
    /// 연속 로그인 보상 제공
    /// </summary>
    public class DailyRewardSystem : MonoBehaviour
    {
        public static DailyRewardSystem Instance { get; private set; }

        [System.Serializable]
        public class DailyReward
        {
            public int day;
            public string rewardName;
            public Sprite icon;
            public RewardType type;
            public double amount;

            public enum RewardType
            {
                Gold,
                PrestigeCurrency,
                DamageBoost,        // 일시적 데미지 부스트
                GoldBoost,          // 일시적 골드 부스트
                SkillCooldownReset  // 모든 스킬 쿨다운 리셋
            }
        }

        [Header("보상 목록")]
        public List<DailyReward> rewards = new List<DailyReward>();

        [Header("설정")]
        public int maxDays = 7;  // 7일 주기

        private int currentStreak = 0;
        private DateTime lastClaimDate;
        private bool canClaimToday = false;

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
            LoadDailyRewardData();
            CheckDailyReward();
            InitializeDefaultRewards();
        }

        /// <summary>
        /// 기본 보상 초기화
        /// </summary>
        void InitializeDefaultRewards()
        {
            if (rewards.Count > 0) return;

            rewards = new List<DailyReward>
            {
                new DailyReward { day = 1, rewardName = "골드", type = DailyReward.RewardType.Gold, amount = 1000 },
                new DailyReward { day = 2, rewardName = "골드", type = DailyReward.RewardType.Gold, amount = 2000 },
                new DailyReward { day = 3, rewardName = "데미지 부스트", type = DailyReward.RewardType.DamageBoost, amount = 2 },
                new DailyReward { day = 4, rewardName = "골드", type = DailyReward.RewardType.Gold, amount = 5000 },
                new DailyReward { day = 5, rewardName = "골드 부스트", type = DailyReward.RewardType.GoldBoost, amount = 2 },
                new DailyReward { day = 6, rewardName = "골드", type = DailyReward.RewardType.Gold, amount = 10000 },
                new DailyReward { day = 7, rewardName = "프레스티지 화폐", type = DailyReward.RewardType.PrestigeCurrency, amount = 1 }
            };
        }

        /// <summary>
        /// 일일 보상 체크
        /// </summary>
        void CheckDailyReward()
        {
            DateTime now = DateTime.Now;
            DateTime today = now.Date;

            // 오늘 이미 받았는지 확인
            if (lastClaimDate.Date == today)
            {
                canClaimToday = false;
                return;
            }

            // 연속 로그인 확인
            DateTime yesterday = today.AddDays(-1);
            if (lastClaimDate.Date == yesterday)
            {
                // 연속 로그인 유지
                canClaimToday = true;
            }
            else if (lastClaimDate.Date < yesterday)
            {
                // 연속 끊김
                currentStreak = 0;
                canClaimToday = true;
            }
        }

        /// <summary>
        /// 보상 수령
        /// </summary>
        public bool ClaimReward()
        {
            if (!canClaimToday)
            {
                Debug.Log("오늘은 이미 보상을 받았습니다!");
                return false;
            }

            // 스트릭 증가
            currentStreak++;
            if (currentStreak > maxDays)
            {
                currentStreak = 1; // 주기 리셋
            }

            // 보상 지급
            DailyReward reward = GetTodayReward();
            if (reward != null)
            {
                GiveReward(reward);
            }

            // 데이터 저장
            lastClaimDate = DateTime.Now;
            canClaimToday = false;
            SaveDailyRewardData();

            // UI 알림
            UIManager.Instance?.ShowBuffNotification(
                "📅 일일 보상!",
                $"{currentStreak}일차: {reward.rewardName}",
                3f
            );

            return true;
        }

        /// <summary>
        /// 오늘의 보상 가져오기
        /// </summary>
        public DailyReward GetTodayReward()
        {
            int day = (currentStreak % maxDays);
            if (day == 0) day = maxDays;

            return rewards.Find(r => r.day == day);
        }

        /// <summary>
        /// 보상 지급
        /// </summary>
        void GiveReward(DailyReward reward)
        {
            switch (reward.type)
            {
                case DailyReward.RewardType.Gold:
                    GameManager.Instance?.AddGold(new BigNumber(reward.amount));
                    break;

                case DailyReward.RewardType.PrestigeCurrency:
                    if (PrestigeManager.Instance != null)
                    {
                        PrestigeManager.Instance.prestigeCurrency =
                            PrestigeManager.Instance.prestigeCurrency.Add(new BigNumber(reward.amount));
                    }
                    break;

                case DailyReward.RewardType.DamageBoost:
                    // 1시간 동안 데미지 2배 (버프 시스템 필요)
                    StartCoroutine(ApplyTemporaryBoost("Damage", reward.amount, 3600));
                    break;

                case DailyReward.RewardType.GoldBoost:
                    // 1시간 동안 골드 2배
                    StartCoroutine(ApplyTemporaryBoost("Gold", reward.amount, 3600));
                    break;

                case DailyReward.RewardType.SkillCooldownReset:
                    ResetAllSkillCooldowns();
                    break;
            }

            Debug.Log($"보상 지급: {reward.rewardName}");
        }

        /// <summary>
        /// 일시적 부스트 적용
        /// </summary>
        System.Collections.IEnumerator ApplyTemporaryBoost(string boostType, double multiplier, int duration)
        {
            // 부스트 활성화
            Debug.Log($"{boostType} 부스트 활성화: x{multiplier} ({duration}초)");

            // TODO: 실제 부스트 적용 로직

            yield return new WaitForSeconds(duration);

            // 부스트 종료
            Debug.Log($"{boostType} 부스트 종료");
        }

        /// <summary>
        /// 모든 스킬 쿨다운 리셋
        /// </summary>
        void ResetAllSkillCooldowns()
        {
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.oneTwoPunch.currentCooldown = 0;
                SkillManager.Instance.headbutt.currentCooldown = 0;
                SkillManager.Instance.comboRush.currentCooldown = 0;
                SkillManager.Instance.goldRush.currentCooldown = 0;
                Debug.Log("모든 스킬 쿨다운 리셋!");
            }
        }

        /// <summary>
        /// 데이터 저장
        /// </summary>
        void SaveDailyRewardData()
        {
            PlayerPrefs.SetInt("DailyReward_Streak", currentStreak);
            PlayerPrefs.SetString("DailyReward_LastClaim", lastClaimDate.ToString("yyyy-MM-dd"));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 데이터 로드
        /// </summary>
        void LoadDailyRewardData()
        {
            currentStreak = PlayerPrefs.GetInt("DailyReward_Streak", 0);

            string lastClaimStr = PlayerPrefs.GetString("DailyReward_LastClaim", "");
            if (!string.IsNullOrEmpty(lastClaimStr))
            {
                DateTime.TryParse(lastClaimStr, out lastClaimDate);
            }
            else
            {
                lastClaimDate = DateTime.MinValue;
            }
        }

        /// <summary>
        /// 현재 연속일 가져오기
        /// </summary>
        public int GetCurrentStreak()
        {
            return currentStreak;
        }

        /// <summary>
        /// 오늘 수령 가능 여부
        /// </summary>
        public bool CanClaimToday()
        {
            return canClaimToday;
        }

        /// <summary>
        /// 다음 보상까지 남은 시간
        /// </summary>
        public TimeSpan GetTimeUntilNextReward()
        {
            DateTime now = DateTime.Now;
            DateTime tomorrow = now.Date.AddDays(1);
            return tomorrow - now;
        }

#if UNITY_EDITOR
        [ContextMenu("Force Claim Today")]
        void EditorForceClaim()
        {
            canClaimToday = true;
            ClaimReward();
        }

        [ContextMenu("Reset Daily Rewards")]
        void EditorReset()
        {
            currentStreak = 0;
            lastClaimDate = DateTime.MinValue;
            canClaimToday = true;
            PlayerPrefs.DeleteKey("DailyReward_Streak");
            PlayerPrefs.DeleteKey("DailyReward_LastClaim");
            Debug.Log("일일 보상 리셋됨");
        }
#endif
    }
}
