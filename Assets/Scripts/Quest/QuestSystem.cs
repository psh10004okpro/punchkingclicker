using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace PunchKing
{
    /// <summary>
    /// 퀘스트/미션 시스템
    /// 일일 퀘스트 및 특별 미션 관리
    /// </summary>
    public class QuestSystem : MonoBehaviour
    {
        public static QuestSystem Instance { get; private set; }

        [System.Serializable]
        public class Quest
        {
            public string questId;
            public string questName;
            public string description;
            public QuestType type;
            public int targetValue;
            public int currentProgress;
            public bool isCompleted;
            public bool isDaily;  // 일일 퀘스트 여부

            // 보상
            public int goldReward;
            public int prestigeReward;

            public enum QuestType
            {
                ClickTimes,         // N번 클릭
                ReachStage,         // 스테이지 도달
                DefeatBosses,       // 보스 처치
                UseSkills,          // 스킬 사용
                UpgradeTimes,       // 업그레이드 구매
                EarnGold,           // 골드 획득
                PlayMiniGame        // 미니게임 플레이
            }
        }

        [Header("퀘스트 목록")]
        public List<Quest> dailyQuests = new List<Quest>();
        public List<Quest> specialQuests = new List<Quest>();

        [Header("이벤트")]
        public UnityEvent<Quest> OnQuestCompleted;

        private System.DateTime lastDailyResetDate;

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

            if (OnQuestCompleted == null)
                OnQuestCompleted = new UnityEvent<Quest>();
        }

        void Start()
        {
            LoadQuestData();
            CheckDailyReset();
            InitializeDefaultQuests();
            SubscribeToGameEvents();
        }

        /// <summary>
        /// 기본 퀘스트 초기화
        /// </summary>
        void InitializeDefaultQuests()
        {
            if (dailyQuests.Count == 0)
            {
                // 일일 퀘스트 3개 생성
                dailyQuests.Add(new Quest
                {
                    questId = "daily_click",
                    questName = "매일 클릭하기",
                    description = "100번 클릭하기",
                    type = Quest.QuestType.ClickTimes,
                    targetValue = 100,
                    goldReward = 500,
                    isDaily = true
                });

                dailyQuests.Add(new Quest
                {
                    questId = "daily_stage",
                    questName = "스테이지 진행",
                    description = "스테이지 10 도달",
                    type = Quest.QuestType.ReachStage,
                    targetValue = 10,
                    goldReward = 1000,
                    isDaily = true
                });

                dailyQuests.Add(new Quest
                {
                    questId = "daily_skill",
                    questName = "스킬 사용",
                    description = "스킬 5번 사용",
                    type = Quest.QuestType.UseSkills,
                    targetValue = 5,
                    goldReward = 750,
                    isDaily = true
                });
            }
        }

        /// <summary>
        /// 게임 이벤트 구독
        /// </summary>
        void SubscribeToGameEvents()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPunch.AddListener(OnPunch);
                GameManager.Instance.OnStageChanged.AddListener(OnStageChanged);
            }

            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnUpgradePurchased.AddListener((_, __) => OnUpgradePurchased());
            }

            if (MiniGameManager.Instance != null)
            {
                MiniGameManager.Instance.OnMiniGameCompleted.AddListener((_, __) => OnMiniGamePlayed());
            }
        }

        /// <summary>
        /// 클릭 이벤트
        /// </summary>
        void OnPunch()
        {
            UpdateQuestProgress(Quest.QuestType.ClickTimes, 1);
        }

        /// <summary>
        /// 스테이지 변경
        /// </summary>
        void OnStageChanged(int stage)
        {
            UpdateQuestProgress(Quest.QuestType.ReachStage, stage);

            if (stage % 10 == 0)
            {
                UpdateQuestProgress(Quest.QuestType.DefeatBosses, 1);
            }
        }

        /// <summary>
        /// 업그레이드 구매
        /// </summary>
        void OnUpgradePurchased()
        {
            UpdateQuestProgress(Quest.QuestType.UpgradeTimes, 1);
        }

        /// <summary>
        /// 미니게임 플레이
        /// </summary>
        void OnMiniGamePlayed()
        {
            UpdateQuestProgress(Quest.QuestType.PlayMiniGame, 1);
        }

        /// <summary>
        /// 스킬 사용 (수동 호출)
        /// </summary>
        public void OnSkillUsed()
        {
            UpdateQuestProgress(Quest.QuestType.UseSkills, 1);
        }

        /// <summary>
        /// 퀘스트 진행도 업데이트
        /// </summary>
        void UpdateQuestProgress(Quest.QuestType type, int increment)
        {
            // 일일 퀘스트
            foreach (var quest in dailyQuests)
            {
                if (quest.type == type && !quest.isCompleted)
                {
                    quest.currentProgress += increment;

                    if (quest.currentProgress >= quest.targetValue)
                    {
                        CompleteQuest(quest);
                    }
                }
            }

            // 특별 퀘스트
            foreach (var quest in specialQuests)
            {
                if (quest.type == type && !quest.isCompleted)
                {
                    quest.currentProgress += increment;

                    if (quest.currentProgress >= quest.targetValue)
                    {
                        CompleteQuest(quest);
                    }
                }
            }
        }

        /// <summary>
        /// 퀘스트 완료
        /// </summary>
        void CompleteQuest(Quest quest)
        {
            if (quest.isCompleted) return;

            quest.isCompleted = true;

            // 보상 지급
            if (quest.goldReward > 0)
            {
                GameManager.Instance?.AddGold(new BigNumber(quest.goldReward));
            }

            if (quest.prestigeReward > 0 && PrestigeManager.Instance != null)
            {
                PrestigeManager.Instance.prestigeCurrency =
                    PrestigeManager.Instance.prestigeCurrency.Add(new BigNumber(quest.prestigeReward));
            }

            // 업적 카운터
            AchievementManager.Instance?.OnQuestCompleted();

            // 이벤트 발생
            OnQuestCompleted?.Invoke(quest);

            // UI 알림
            UIManager.Instance?.ShowBuffNotification(
                "✅ 퀘스트 완료!",
                $"{quest.questName}\n💰 {quest.goldReward} 골드",
                3f
            );

            Debug.Log($"퀘스트 완료: {quest.questName}");

            SaveQuestData();
        }

        /// <summary>
        /// 일일 퀘스트 리셋 확인
        /// </summary>
        void CheckDailyReset()
        {
            System.DateTime now = System.DateTime.Now;
            System.DateTime today = now.Date;

            if (lastDailyResetDate.Date != today)
            {
                // 새로운 날, 일일 퀘스트 리셋
                ResetDailyQuests();
                lastDailyResetDate = now;
                SaveQuestData();
            }
        }

        /// <summary>
        /// 일일 퀘스트 리셋
        /// </summary>
        void ResetDailyQuests()
        {
            foreach (var quest in dailyQuests)
            {
                quest.currentProgress = 0;
                quest.isCompleted = false;
            }

            Debug.Log("일일 퀘스트 리셋됨");
        }

        /// <summary>
        /// 퀘스트 데이터 저장
        /// </summary>
        void SaveQuestData()
        {
            PlayerPrefs.SetString("LastDailyReset", lastDailyResetDate.ToString("yyyy-MM-dd"));

            // 일일 퀘스트 진행도 저장
            for (int i = 0; i < dailyQuests.Count; i++)
            {
                PlayerPrefs.SetInt($"DailyQuest_{i}_Progress", dailyQuests[i].currentProgress);
                PlayerPrefs.SetInt($"DailyQuest_{i}_Completed", dailyQuests[i].isCompleted ? 1 : 0);
            }

            PlayerPrefs.Save();
        }

        /// <summary>
        /// 퀘스트 데이터 로드
        /// </summary>
        void LoadQuestData()
        {
            string lastResetStr = PlayerPrefs.GetString("LastDailyReset", "");
            if (!string.IsNullOrEmpty(lastResetStr))
            {
                System.DateTime.TryParse(lastResetStr, out lastDailyResetDate);
            }
            else
            {
                lastDailyResetDate = System.DateTime.MinValue;
            }

            // 일일 퀘스트 진행도 로드
            for (int i = 0; i < dailyQuests.Count; i++)
            {
                dailyQuests[i].currentProgress = PlayerPrefs.GetInt($"DailyQuest_{i}_Progress", 0);
                dailyQuests[i].isCompleted = PlayerPrefs.GetInt($"DailyQuest_{i}_Completed", 0) == 1;
            }
        }

        /// <summary>
        /// 완료되지 않은 퀘스트 수
        /// </summary>
        public int GetIncompleteQuestCount()
        {
            return dailyQuests.Count(q => !q.isCompleted) + specialQuests.Count(q => !q.isCompleted);
        }

#if UNITY_EDITOR
        [ContextMenu("Complete All Quests")]
        void EditorCompleteAll()
        {
            foreach (var quest in dailyQuests)
            {
                if (!quest.isCompleted)
                {
                    quest.currentProgress = quest.targetValue;
                    CompleteQuest(quest);
                }
            }
        }

        [ContextMenu("Reset Daily Quests")]
        void EditorResetDaily()
        {
            ResetDailyQuests();
            SaveQuestData();
            Debug.Log("일일 퀘스트 리셋됨");
        }
#endif
    }
}
