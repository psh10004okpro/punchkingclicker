using UnityEngine;
using System.Collections.Generic;

namespace PunchKing
{
    /// <summary>
    /// 코치 시스템
    /// 각 코치는 고유한 보너스와 스킬 제공
    /// </summary>
    public class CoachSystem : MonoBehaviour
    {
        public static CoachSystem Instance { get; private set; }

        /// <summary>
        /// 코치 데이터
        /// </summary>
        [System.Serializable]
        public class Coach
        {
            public int id;
            public string coachName;
            public string description;
            public Sprite portrait;
            public BigNumber unlockCost;
            public float damageBonus;      // 데미지 보너스 (%)
            public float goldBonus;        // 골드 보너스 (%)
            public float critBonus;        // 크리티컬 보너스 (%)
            public bool isUnlocked;
        }

        [Header("코치 목록")]
        public List<Coach> allCoaches = new List<Coach>();

        private int activeCoachId = -1;  // -1 = 코치 없음

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

            InitializeCoaches();
        }

        /// <summary>
        /// 코치 초기화
        /// </summary>
        void InitializeCoaches()
        {
            // 기본 코치 5명 설정
            if (allCoaches.Count == 0)
            {
                allCoaches.Add(new Coach
                {
                    id = 0,
                    coachName = "이소용",
                    description = "전설의 복서. 데미지 +50%",
                    unlockCost = new BigNumber(1000),
                    damageBonus = 0.5f,
                    goldBonus = 0f,
                    critBonus = 0f,
                    isUnlocked = false
                });

                allCoaches.Add(new Coach
                {
                    id = 1,
                    coachName = "김트레이너",
                    description = "골드 마스터. 골드 획득 +30%",
                    unlockCost = new BigNumber(5000),
                    damageBonus = 0f,
                    goldBonus = 0.3f,
                    critBonus = 0f,
                    isUnlocked = false
                });

                allCoaches.Add(new Coach
                {
                    id = 2,
                    coachName = "박챔피언",
                    description = "크리티컬 전문가. 크리티컬 확률 +15%",
                    unlockCost = new BigNumber(10000),
                    damageBonus = 0f,
                    goldBonus = 0f,
                    critBonus = 0.15f,
                    isUnlocked = false
                });

                allCoaches.Add(new Coach
                {
                    id = 3,
                    coachName = "최마스터",
                    description = "균형잡힌 코치. 모든 보너스 +20%",
                    unlockCost = new BigNumber(50000),
                    damageBonus = 0.2f,
                    goldBonus = 0.2f,
                    critBonus = 0.1f,
                    isUnlocked = false
                });

                allCoaches.Add(new Coach
                {
                    id = 4,
                    coachName = "전설의 권왕",
                    description = "최강의 코치. 모든 보너스 +100%",
                    unlockCost = new BigNumber(1000000),
                    damageBonus = 1f,
                    goldBonus = 1f,
                    critBonus = 0.25f,
                    isUnlocked = false
                });
            }
        }

        /// <summary>
        /// 코치 잠금 해제
        /// </summary>
        public bool UnlockCoach(int coachId)
        {
            Coach coach = allCoaches.Find(c => c.id == coachId);
            if (coach == null)
            {
                Debug.LogError($"코치 ID {coachId}를 찾을 수 없습니다");
                return false;
            }

            if (coach.isUnlocked)
            {
                Debug.Log($"{coach.coachName}은(는) 이미 잠금 해제되었습니다");
                return false;
            }

            // 골드 확인 및 지불
            if (!GameManager.Instance.SpendGold(coach.unlockCost))
            {
                Debug.Log("골드가 부족합니다");
                return false;
            }

            // 잠금 해제
            coach.isUnlocked = true;

            // 사운드
            AudioManager.Instance?.PlaySound("CoachUnlock");

            Debug.Log($"{coach.coachName} 잠금 해제!");

            return true;
        }

        /// <summary>
        /// 코치 활성화
        /// </summary>
        public void SetActiveCoach(int coachId)
        {
            Coach coach = allCoaches.Find(c => c.id == coachId);

            if (coach == null || !coach.isUnlocked)
            {
                Debug.LogWarning($"코치 ID {coachId}를 활성화할 수 없습니다");
                return;
            }

            activeCoachId = coachId;

            // 코치 보너스 적용
            ApplyCoachBonuses();

            Debug.Log($"{coach.coachName} 활성화!");
        }

        /// <summary>
        /// 코치 보너스 적용
        /// </summary>
        void ApplyCoachBonuses()
        {
            if (activeCoachId < 0) return;

            Coach activeCoach = allCoaches.Find(c => c.id == activeCoachId);
            if (activeCoach == null) return;

            // 크리티컬 확률 보너스
            if (activeCoach.critBonus > 0 && GameManager.Instance != null)
            {
                GameManager.Instance.critChance += activeCoach.critBonus;
            }
        }

        /// <summary>
        /// 데미지에 코치 보너스 적용
        /// </summary>
        public BigNumber ApplyCoachBonus(BigNumber baseDamage)
        {
            if (activeCoachId < 0) return baseDamage;

            Coach activeCoach = allCoaches.Find(c => c.id == activeCoachId);
            if (activeCoach == null) return baseDamage;

            if (activeCoach.damageBonus > 0)
            {
                float multiplier = 1f + activeCoach.damageBonus;
                return baseDamage.Multiply(multiplier);
            }

            return baseDamage;
        }

        /// <summary>
        /// 골드에 코치 보너스 적용
        /// </summary>
        public BigNumber ApplyGoldBonus(BigNumber baseGold)
        {
            if (activeCoachId < 0) return baseGold;

            Coach activeCoach = allCoaches.Find(c => c.id == activeCoachId);
            if (activeCoach == null) return baseGold;

            if (activeCoach.goldBonus > 0)
            {
                float multiplier = 1f + activeCoach.goldBonus;
                return baseGold.Multiply(multiplier);
            }

            return baseGold;
        }

        /// <summary>
        /// 활성 코치 ID 반환 (저장용)
        /// </summary>
        public int GetActiveCoachId()
        {
            return activeCoachId;
        }

        /// <summary>
        /// 잠금 해제된 코치 배열 반환 (저장용)
        /// </summary>
        public bool[] GetUnlockedCoaches()
        {
            bool[] unlocked = new bool[allCoaches.Count];
            for (int i = 0; i < allCoaches.Count; i++)
            {
                unlocked[i] = allCoaches[i].isUnlocked;
            }
            return unlocked;
        }

        /// <summary>
        /// 저장 데이터 로드
        /// </summary>
        public void LoadCoachData(int savedActiveCoachId, bool[] unlockedArray)
        {
            if (unlockedArray != null && unlockedArray.Length == allCoaches.Count)
            {
                for (int i = 0; i < allCoaches.Count; i++)
                {
                    allCoaches[i].isUnlocked = unlockedArray[i];
                }
            }

            activeCoachId = savedActiveCoachId;
            ApplyCoachBonuses();
        }

#if UNITY_EDITOR
        [ContextMenu("Unlock All Coaches")]
        void EditorUnlockAllCoaches()
        {
            foreach (var coach in allCoaches)
            {
                coach.isUnlocked = true;
            }
            Debug.Log("모든 코치 잠금 해제됨");
        }
#endif
    }
}
