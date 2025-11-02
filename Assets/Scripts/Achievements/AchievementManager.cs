using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace PunchKing
{
    /// <summary>
    /// 업적 시스템 관리
    /// 업적 진행도 추적 및 보상 지급
    /// </summary>
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        [Header("업적 목록")]
        public List<AchievementData> allAchievements = new List<AchievementData>();

        [Header("이벤트")]
        public UnityEvent<AchievementData> OnAchievementUnlocked;

        // 업적 진행도 저장
        private Dictionary<string, long> achievementProgress = new Dictionary<string, long>();

        // 완료된 업적
        private HashSet<string> completedAchievements = new HashSet<string>();

        // 통계 추적
        [HideInInspector] public long totalClicks = 0;
        [HideInInspector] public long totalGoldEarned = 0;
        [HideInInspector] public int highestStage = 0;
        [HideInInspector] public int bossesDefeated = 0;
        [HideInInspector] public int skillsUsed = 0;
        [HideInInspector] public int criticalHits = 0;
        [HideInInspector] public int questsCompleted = 0;
        [HideInInspector] public int playTimeSeconds = 0;

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

            if (OnAchievementUnlocked == null)
                OnAchievementUnlocked = new UnityEvent<AchievementData>();
        }

        void Start()
        {
            SubscribeToGameEvents();
            LoadAchievementProgress();
            CheckAllAchievements();
        }

        void Update()
        {
            // 플레이 시간 추적
            playTimeSeconds += (int)Time.deltaTime;
        }

        /// <summary>
        /// 게임 이벤트 구독
        /// </summary>
        void SubscribeToGameEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPunch.AddListener(OnPunch);
                GameManager.Instance.OnGoldChanged.AddListener(OnGoldChanged);
                GameManager.Instance.OnStageChanged.AddListener(OnStageChanged);
                GameManager.Instance.OnDamageDealt.AddListener(OnDamageDealt);
            }

            if (PrestigeManager.Instance != null)
            {
                PrestigeManager.Instance.OnPrestige.AddListener(OnPrestige);
            }
        }

        /// <summary>
        /// 펀치 이벤트
        /// </summary>
        void OnPunch()
        {
            totalClicks++;
            UpdateProgress(AchievementData.AchievementType.TotalClicks, totalClicks);
        }

        /// <summary>
        /// 골드 변경 이벤트
        /// </summary>
        void OnGoldChanged(BigNumber gold)
        {
            totalGoldEarned = (long)gold.ToDouble();
            UpdateProgress(AchievementData.AchievementType.TotalGold, totalGoldEarned);
        }

        /// <summary>
        /// 스테이지 변경 이벤트
        /// </summary>
        void OnStageChanged(int stage)
        {
            if (stage > highestStage)
            {
                highestStage = stage;
                UpdateProgress(AchievementData.AchievementType.ReachStage, highestStage);
            }

            // 보스 스테이지
            if (stage % 10 == 0)
            {
                bossesDefeated++;
                UpdateProgress(AchievementData.AchievementType.DefeatBosses, bossesDefeated);
            }
        }

        /// <summary>
        /// 데미지 이벤트
        /// </summary>
        void OnDamageDealt(BigNumber damage, bool isCritical)
        {
            if (isCritical)
            {
                criticalHits++;
                UpdateProgress(AchievementData.AchievementType.CriticalHits, criticalHits);
            }
        }

        /// <summary>
        /// 프레스티지 이벤트
        /// </summary>
        void OnPrestige()
        {
            if (PrestigeManager.Instance != null)
            {
                UpdateProgress(AchievementData.AchievementType.PrestigeCount,
                    PrestigeManager.Instance.prestigeCount);
            }
        }

        /// <summary>
        /// 스킬 사용
        /// </summary>
        public void OnSkillUsed()
        {
            skillsUsed++;
            UpdateProgress(AchievementData.AchievementType.UseSkills, skillsUsed);
        }

        /// <summary>
        /// 퀘스트 완료
        /// </summary>
        public void OnQuestCompleted()
        {
            questsCompleted++;
            UpdateProgress(AchievementData.AchievementType.CompleteQuests, questsCompleted);
        }

        /// <summary>
        /// 코치 잠금 해제
        /// </summary>
        public void OnCoachUnlocked()
        {
            if (CoachSystem.Instance != null)
            {
                int unlockedCount = CoachSystem.Instance.allCoaches.Count(c => c.isUnlocked);
                UpdateProgress(AchievementData.AchievementType.UnlockCoaches, unlockedCount);
            }
        }

        /// <summary>
        /// 진행도 업데이트
        /// </summary>
        void UpdateProgress(AchievementData.AchievementType type, long value)
        {
            // 해당 타입의 모든 업적 확인
            var achievements = allAchievements.Where(a => a.type == type);

            foreach (var achievement in achievements)
            {
                // 이미 완료된 업적은 스킵
                if (completedAchievements.Contains(achievement.achievementId))
                    continue;

                // 진행도 업데이트
                achievementProgress[achievement.achievementId] = value;

                // 완료 체크
                if (achievement.IsCompleted(value))
                {
                    UnlockAchievement(achievement);
                }
            }
        }

        /// <summary>
        /// 업적 잠금 해제
        /// </summary>
        void UnlockAchievement(AchievementData achievement)
        {
            if (completedAchievements.Contains(achievement.achievementId))
                return;

            completedAchievements.Add(achievement.achievementId);

            // 보상 지급
            GiveReward(achievement);

            // 이벤트 발생
            OnAchievementUnlocked?.Invoke(achievement);

            // UI 알림
            UIManager.Instance?.ShowBuffNotification(
                "🏆 업적 달성!",
                $"{achievement.achievementName}\n{achievement.GetRewardDescription()}",
                3f
            );

            // 사운드
            AudioManager.Instance?.PlaySound("AchievementUnlock");

            Debug.Log($"업적 달성: {achievement.achievementName}");
        }

        /// <summary>
        /// 보상 지급
        /// </summary>
        void GiveReward(AchievementData achievement)
        {
            switch (achievement.rewardType)
            {
                case AchievementData.RewardType.Gold:
                    GameManager.Instance?.AddGold(new BigNumber(achievement.rewardAmount));
                    break;

                case AchievementData.RewardType.PrestigeCurrency:
                    if (PrestigeManager.Instance != null)
                    {
                        PrestigeManager.Instance.prestigeCurrency =
                            PrestigeManager.Instance.prestigeCurrency.Add(new BigNumber(achievement.rewardAmount));
                    }
                    break;

                case AchievementData.RewardType.DamageMultiplier:
                    // 영구 데미지 보너스 (프레스티지 시스템에 추가 가능)
                    break;

                case AchievementData.RewardType.GoldMultiplier:
                    // 영구 골드 보너스
                    break;

                case AchievementData.RewardType.CritChance:
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.critChance += (float)achievement.rewardAmount;
                    }
                    break;
            }
        }

        /// <summary>
        /// 업적 진행도 가져오기
        /// </summary>
        public long GetProgress(AchievementData achievement)
        {
            if (achievementProgress.ContainsKey(achievement.achievementId))
                return achievementProgress[achievement.achievementId];
            return 0;
        }

        /// <summary>
        /// 업적 완료 여부
        /// </summary>
        public bool IsCompleted(AchievementData achievement)
        {
            return completedAchievements.Contains(achievement.achievementId);
        }

        /// <summary>
        /// 모든 업적 체크
        /// </summary>
        void CheckAllAchievements()
        {
            UpdateProgress(AchievementData.AchievementType.TotalClicks, totalClicks);
            UpdateProgress(AchievementData.AchievementType.TotalGold, totalGoldEarned);
            UpdateProgress(AchievementData.AchievementType.ReachStage, highestStage);
            UpdateProgress(AchievementData.AchievementType.DefeatBosses, bossesDefeated);
            UpdateProgress(AchievementData.AchievementType.UseSkills, skillsUsed);
            UpdateProgress(AchievementData.AchievementType.CriticalHits, criticalHits);
            UpdateProgress(AchievementData.AchievementType.CompleteQuests, questsCompleted);
            UpdateProgress(AchievementData.AchievementType.PlayTime, playTimeSeconds);

            if (PrestigeManager.Instance != null)
            {
                UpdateProgress(AchievementData.AchievementType.PrestigeCount,
                    PrestigeManager.Instance.prestigeCount);
            }
        }

        /// <summary>
        /// 진행도 저장
        /// </summary>
        public void SaveAchievementProgress()
        {
            // SaveManager를 통해 저장 (SaveData에 추가 필요)
            PlayerPrefs.SetInt("TotalClicks", (int)totalClicks);
            PlayerPrefs.SetInt("TotalGold", (int)totalGoldEarned);
            PlayerPrefs.SetInt("HighestStage", highestStage);
            PlayerPrefs.SetInt("BossesDefeated", bossesDefeated);
            PlayerPrefs.SetInt("SkillsUsed", skillsUsed);
            PlayerPrefs.SetInt("CriticalHits", criticalHits);
            PlayerPrefs.SetInt("QuestsCompleted", questsCompleted);
            PlayerPrefs.SetInt("PlayTime", playTimeSeconds);

            // 완료된 업적 저장
            string completed = string.Join(",", completedAchievements);
            PlayerPrefs.SetString("CompletedAchievements", completed);

            PlayerPrefs.Save();
        }

        /// <summary>
        /// 진행도 로드
        /// </summary>
        void LoadAchievementProgress()
        {
            totalClicks = PlayerPrefs.GetInt("TotalClicks", 0);
            totalGoldEarned = PlayerPrefs.GetInt("TotalGold", 0);
            highestStage = PlayerPrefs.GetInt("HighestStage", 0);
            bossesDefeated = PlayerPrefs.GetInt("BossesDefeated", 0);
            skillsUsed = PlayerPrefs.GetInt("SkillsUsed", 0);
            criticalHits = PlayerPrefs.GetInt("CriticalHits", 0);
            questsCompleted = PlayerPrefs.GetInt("QuestsCompleted", 0);
            playTimeSeconds = PlayerPrefs.GetInt("PlayTime", 0);

            // 완료된 업적 로드
            string completed = PlayerPrefs.GetString("CompletedAchievements", "");
            if (!string.IsNullOrEmpty(completed))
            {
                completedAchievements = new HashSet<string>(completed.Split(','));
            }
        }

        /// <summary>
        /// 완료 퍼센트
        /// </summary>
        public float GetCompletionPercentage()
        {
            if (allAchievements.Count == 0) return 0f;
            return (float)completedAchievements.Count / allAchievements.Count * 100f;
        }

        /// <summary>
        /// 해당 타입의 현재 진행도 가져오기
        /// </summary>
        public long GetProgress(AchievementData.AchievementType type)
        {
            switch (type)
            {
                case AchievementData.AchievementType.TotalClicks:
                    return totalClicks;
                case AchievementData.AchievementType.TotalGold:
                    return totalGoldEarned;
                case AchievementData.AchievementType.ReachStage:
                    return highestStage;
                case AchievementData.AchievementType.DefeatBosses:
                    return bossesDefeated;
                case AchievementData.AchievementType.PrestigeCount:
                    return PrestigeManager.Instance != null ? PrestigeManager.Instance.prestigeCount : 0;
                case AchievementData.AchievementType.UseSkills:
                    return skillsUsed;
                case AchievementData.AchievementType.CriticalHits:
                    return criticalHits;
                case AchievementData.AchievementType.CompleteQuests:
                    return questsCompleted;
                case AchievementData.AchievementType.PlayTime:
                    return playTimeSeconds;
                case AchievementData.AchievementType.UpgradeLevel:
                    return UpgradeManager.Instance != null ? GetTotalUpgradeLevels() : 0;
                case AchievementData.AchievementType.UnlockCoaches:
                    return CoachSystem.Instance != null ? CoachSystem.Instance.allCoaches.Count(c => c.isUnlocked) : 0;
                case AchievementData.AchievementType.DailyLogins:
                    return DailyRewardSystem.Instance != null ? DailyRewardSystem.Instance.currentStreak : 0;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 총 업그레이드 레벨 계산
        /// </summary>
        long GetTotalUpgradeLevels()
        {
            if (UpgradeManager.Instance == null) return 0;
            long total = 0;
            foreach (var kvp in UpgradeManager.Instance.upgradeLevels)
            {
                total += kvp.Value;
            }
            return total;
        }

        /// <summary>
        /// 업적 ID로 완료 여부 확인
        /// </summary>
        public bool IsAchievementCompleted(string achievementId)
        {
            return completedAchievements.Contains(achievementId);
        }

        /// <summary>
        /// 타입별 업적 목록 가져오기
        /// </summary>
        public List<AchievementData> GetAchievementsByType(AchievementData.AchievementType type)
        {
            return allAchievements.Where(a => a.type == type).OrderBy(a => a.tier).ToList();
        }

        /// <summary>
        /// 모든 업적 목록 (AchievementPanel용 호환성 프로퍼티)
        /// </summary>
        public List<AchievementData> achievements => allAchievements;

#if UNITY_EDITOR
        [ContextMenu("Unlock All Achievements")]
        void EditorUnlockAll()
        {
            foreach (var achievement in allAchievements)
            {
                if (!completedAchievements.Contains(achievement.achievementId))
                {
                    UnlockAchievement(achievement);
                }
            }
        }

        [ContextMenu("Reset All Achievements")]
        void EditorResetAll()
        {
            completedAchievements.Clear();
            achievementProgress.Clear();
            PlayerPrefs.DeleteKey("CompletedAchievements");
            Debug.Log("모든 업적 리셋됨");
        }
#endif
    }
}
