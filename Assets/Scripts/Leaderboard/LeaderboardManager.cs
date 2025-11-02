using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PunchKing
{
    /// <summary>
    /// 리더보드 매니저
    /// 로컬 및 클라우드 랭킹 관리
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }

        [Header("리더보드 설정")]
        public int maxLocalEntries = 100;
        public bool useCloudLeaderboard = false;

        [Header("플레이어 정보")]
        public string playerName = "Player";

        private List<LeaderboardEntry> localLeaderboard = new List<LeaderboardEntry>();
        private List<LeaderboardEntry> globalLeaderboard = new List<LeaderboardEntry>();

        private const string PLAYER_NAME_KEY = "PlayerName";
        private const string LOCAL_LEADERBOARD_KEY = "LocalLeaderboard";

        /// <summary>
        /// 리더보드 항목
        /// </summary>
        [System.Serializable]
        public class LeaderboardEntry
        {
            public string playerName;
            public long score;           // 스테이지 번호
            public long totalGold;       // 총 획득 골드
            public int prestigeCount;    // 프레스티지 횟수
            public long totalDamage;     // 총 데미지
            public DateTime timestamp;
            public bool isLocalPlayer;

            public LeaderboardEntry(string name, long stage, long gold, int prestige, long damage)
            {
                playerName = name;
                score = stage;
                totalGold = gold;
                prestigeCount = prestige;
                totalDamage = damage;
                timestamp = DateTime.Now;
                isLocalPlayer = false;
            }

            public string GetScoreString()
            {
                return $"스테이지 {score}";
            }

            public string GetStatsString()
            {
                return $"골드: {new BigNumber(totalGold).ToKoreanString()} | 프레스티지: {prestigeCount}회";
            }
        }

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
            }
        }

        void Start()
        {
            LoadPlayerName();
            LoadLocalLeaderboard();
            GenerateDummyData();
        }

        /// <summary>
        /// 플레이어 이름 설정
        /// </summary>
        public void SetPlayerName(string name)
        {
            playerName = name;
            PlayerPrefs.SetString(PLAYER_NAME_KEY, name);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 플레이어 이름 로드
        /// </summary>
        void LoadPlayerName()
        {
            playerName = PlayerPrefs.GetString(PLAYER_NAME_KEY, "Player");
        }

        /// <summary>
        /// 현재 플레이어 점수 제출
        /// </summary>
        public void SubmitScore()
        {
            if (GameManager.Instance == null) return;

            long currentStage = GameManager.Instance.currentStage;
            long totalGold = GetTotalGoldEarned();
            int prestigeCount = PrestigeManager.Instance != null ? PrestigeManager.Instance.timesPrestiged : 0;
            long totalDamage = GetTotalDamageDealt();

            LeaderboardEntry newEntry = new LeaderboardEntry(
                playerName,
                currentStage,
                totalGold,
                prestigeCount,
                totalDamage
            );
            newEntry.isLocalPlayer = true;

            // 로컬 리더보드에 추가
            AddToLocalLeaderboard(newEntry);

            // 클라우드 리더보드에 제출 (활성화된 경우)
            if (useCloudLeaderboard)
            {
                SubmitToCloudLeaderboard(newEntry);
            }

            Debug.Log($"[Leaderboard] 점수 제출: {playerName} - 스테이지 {currentStage}");
        }

        /// <summary>
        /// 로컬 리더보드에 추가
        /// </summary>
        void AddToLocalLeaderboard(LeaderboardEntry entry)
        {
            // 기존 플레이어 항목 제거
            localLeaderboard.RemoveAll(e => e.playerName == entry.playerName);

            // 새 항목 추가
            localLeaderboard.Add(entry);

            // 점수 순으로 정렬
            localLeaderboard = localLeaderboard.OrderByDescending(e => e.score).ToList();

            // 최대 항목 수 제한
            if (localLeaderboard.Count > maxLocalEntries)
            {
                localLeaderboard = localLeaderboard.Take(maxLocalEntries).ToList();
            }

            SaveLocalLeaderboard();
        }

        /// <summary>
        /// 클라우드 리더보드에 제출
        /// </summary>
        void SubmitToCloudLeaderboard(LeaderboardEntry entry)
        {
#if UNITY_SERVICES_ENABLED
            // Unity Gaming Services (UGS) Leaderboards 사용
            // 실제 구현 시 UGS 패키지 필요
            Debug.Log($"[Leaderboard] 클라우드 제출: {entry.playerName} - {entry.score}");

            // 예시:
            // LeaderboardsService.Instance.AddPlayerScoreAsync(
            //     "punch_king_global",
            //     entry.score
            // );
#else
            Debug.Log("[Leaderboard] Unity Gaming Services가 활성화되지 않았습니다.");
#endif
        }

        /// <summary>
        /// 로컬 리더보드 가져오기
        /// </summary>
        public List<LeaderboardEntry> GetLocalLeaderboard(int count = 10)
        {
            return localLeaderboard.Take(count).ToList();
        }

        /// <summary>
        /// 글로벌 리더보드 가져오기
        /// </summary>
        public List<LeaderboardEntry> GetGlobalLeaderboard(int count = 10)
        {
            if (useCloudLeaderboard)
            {
                // 클라우드에서 가져오기
                FetchGlobalLeaderboard(count);
                return globalLeaderboard.Take(count).ToList();
            }
            else
            {
                // 로컬 데이터 반환
                return GetLocalLeaderboard(count);
            }
        }

        /// <summary>
        /// 글로벌 리더보드 가져오기 (클라우드)
        /// </summary>
        async void FetchGlobalLeaderboard(int count)
        {
#if UNITY_SERVICES_ENABLED
            // Unity Gaming Services에서 가져오기
            // var results = await LeaderboardsService.Instance.GetScoresAsync("punch_king_global", new GetScoresOptions { Limit = count });
            // globalLeaderboard = ConvertToLeaderboardEntries(results);
            Debug.Log($"[Leaderboard] 글로벌 리더보드 불러오기 (상위 {count}명)");
#else
            await System.Threading.Tasks.Task.CompletedTask;
#endif
        }

        /// <summary>
        /// 플레이어 순위 가져오기
        /// </summary>
        public int GetPlayerRank()
        {
            for (int i = 0; i < localLeaderboard.Count; i++)
            {
                if (localLeaderboard[i].isLocalPlayer)
                {
                    return i + 1;
                }
            }
            return -1;
        }

        /// <summary>
        /// 총 골드 획득량 계산
        /// </summary>
        long GetTotalGoldEarned()
        {
            // AchievementManager에서 추적 중인 총 골드 사용
            if (AchievementManager.Instance != null)
            {
                return AchievementManager.Instance.GetProgress(AchievementData.AchievementType.TotalGold);
            }
            return 0;
        }

        /// <summary>
        /// 총 데미지 계산
        /// </summary>
        long GetTotalDamageDealt()
        {
            // 총 클릭 수 * 평균 데미지로 근사치 계산
            if (AchievementManager.Instance != null && GameManager.Instance != null)
            {
                long totalClicks = AchievementManager.Instance.GetProgress(AchievementData.AchievementType.TotalClicks);
                // 간단한 추정치
                return totalClicks * 100;
            }
            return 0;
        }

        /// <summary>
        /// 로컬 리더보드 저장
        /// </summary>
        void SaveLocalLeaderboard()
        {
            string json = JsonUtility.ToJson(new LeaderboardWrapper { entries = localLeaderboard });
            PlayerPrefs.SetString(LOCAL_LEADERBOARD_KEY, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// 로컬 리더보드 로드
        /// </summary>
        void LoadLocalLeaderboard()
        {
            string json = PlayerPrefs.GetString(LOCAL_LEADERBOARD_KEY, "");
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    LeaderboardWrapper wrapper = JsonUtility.FromJson<LeaderboardWrapper>(json);
                    localLeaderboard = wrapper.entries;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[Leaderboard] 로드 실패: {e.Message}");
                    localLeaderboard = new List<LeaderboardEntry>();
                }
            }
        }

        /// <summary>
        /// 더미 데이터 생성 (테스트용)
        /// </summary>
        void GenerateDummyData()
        {
            if (localLeaderboard.Count > 0) return;

            string[] dummyNames = {
                "펀치킹", "철권파이터", "강철주먹", "챔피언", "복서조",
                "권왕", "타이슨", "알리", "메이웨더", "파퀴아오",
                "최강권사", "격투천재", "주먹황제", "전설의파이터", "불패신화"
            };

            System.Random random = new System.Random();

            for (int i = 0; i < 15; i++)
            {
                string name = dummyNames[random.Next(dummyNames.Length)] + random.Next(100, 999);
                long stage = random.Next(50, 500);
                long gold = (long)(random.NextDouble() * 1000000000000);
                int prestige = random.Next(0, 20);
                long damage = (long)(random.NextDouble() * 999999999);

                localLeaderboard.Add(new LeaderboardEntry(name, stage, gold, prestige, damage));
            }

            // 정렬
            localLeaderboard = localLeaderboard.OrderByDescending(e => e.score).ToList();

            SaveLocalLeaderboard();
        }

        /// <summary>
        /// JSON 직렬화용 래퍼
        /// </summary>
        [System.Serializable]
        class LeaderboardWrapper
        {
            public List<LeaderboardEntry> entries;
        }

#if UNITY_EDITOR
        [ContextMenu("Submit Test Score")]
        void SubmitTestScore()
        {
            SubmitScore();
        }

        [ContextMenu("Clear Leaderboard")]
        void ClearLeaderboard()
        {
            localLeaderboard.Clear();
            PlayerPrefs.DeleteKey(LOCAL_LEADERBOARD_KEY);
            Debug.Log("[Leaderboard] 리더보드 초기화 완료");
        }

        [ContextMenu("Generate Dummy Data")]
        void GenerateDummyDataManual()
        {
            localLeaderboard.Clear();
            GenerateDummyData();
            Debug.Log("[Leaderboard] 더미 데이터 생성 완료");
        }
#endif
    }
}
