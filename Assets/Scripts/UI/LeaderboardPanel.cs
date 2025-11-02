using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace PunchKing
{
    /// <summary>
    /// 리더보드 UI 패널
    /// 로컬/글로벌 랭킹 표시
    /// </summary>
    public class LeaderboardPanel : MonoBehaviour
    {
        [Header("탭")]
        public Toggle tabLocal;
        public Toggle tabGlobal;

        [Header("리더보드 리스트")]
        public Transform leaderboardContainer;
        public GameObject leaderboardEntryPrefab;

        [Header("플레이어 정보")]
        public TextMeshProUGUI playerRankText;
        public TextMeshProUGUI playerNameText;
        public TextMeshProUGUI playerScoreText;
        public Button submitScoreButton;

        [Header("설정")]
        public int displayCount = 10;

        private List<LeaderboardEntryUI> entryUIs = new List<LeaderboardEntryUI>();
        private bool showingGlobal = false;

        /// <summary>
        /// 리더보드 항목 UI
        /// </summary>
        [System.Serializable]
        public class LeaderboardEntryUI
        {
            public GameObject gameObject;
            public TextMeshProUGUI rankText;
            public TextMeshProUGUI nameText;
            public TextMeshProUGUI scoreText;
            public TextMeshProUGUI statsText;
            public Image background;
            public Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            public Color highlightColor = new Color(0.3f, 0.5f, 0.3f, 0.9f);
            public Color top3Color = new Color(0.5f, 0.4f, 0.2f, 0.9f);
        }

        void Start()
        {
            SetupTabs();

            if (submitScoreButton != null)
            {
                submitScoreButton.onClick.AddListener(OnSubmitScore);
            }
        }

        void OnEnable()
        {
            RefreshLeaderboard();
            UpdatePlayerInfo();
        }

        /// <summary>
        /// 탭 설정
        /// </summary>
        void SetupTabs()
        {
            if (tabLocal != null)
                tabLocal.onValueChanged.AddListener((isOn) => { if (isOn) OnTabChanged(false); });

            if (tabGlobal != null)
                tabGlobal.onValueChanged.AddListener((isOn) => { if (isOn) OnTabChanged(true); });
        }

        /// <summary>
        /// 탭 변경
        /// </summary>
        void OnTabChanged(bool global)
        {
            showingGlobal = global;
            RefreshLeaderboard();
        }

        /// <summary>
        /// 리더보드 갱신
        /// </summary>
        void RefreshLeaderboard()
        {
            if (LeaderboardManager.Instance == null) return;

            ClearLeaderboard();

            List<LeaderboardManager.LeaderboardEntry> entries;

            if (showingGlobal)
            {
                entries = LeaderboardManager.Instance.GetGlobalLeaderboard(displayCount);
            }
            else
            {
                entries = LeaderboardManager.Instance.GetLocalLeaderboard(displayCount);
            }

            for (int i = 0; i < entries.Count; i++)
            {
                CreateLeaderboardEntry(entries[i], i + 1);
            }
        }

        /// <summary>
        /// 리더보드 항목 생성
        /// </summary>
        void CreateLeaderboardEntry(LeaderboardManager.LeaderboardEntry data, int rank)
        {
            if (leaderboardEntryPrefab == null || leaderboardContainer == null) return;

            GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
            LeaderboardEntryUI entry = new LeaderboardEntryUI
            {
                gameObject = entryObj
            };

            // UI 컴포넌트 참조
            entry.rankText = entryObj.transform.Find("RankText")?.GetComponent<TextMeshProUGUI>();
            entry.nameText = entryObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            entry.scoreText = entryObj.transform.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
            entry.statsText = entryObj.transform.Find("StatsText")?.GetComponent<TextMeshProUGUI>();
            entry.background = entryObj.GetComponent<Image>();

            // 데이터 설정
            if (entry.rankText != null)
            {
                entry.rankText.text = GetRankString(rank);
            }

            if (entry.nameText != null)
            {
                entry.nameText.text = data.playerName;
            }

            if (entry.scoreText != null)
            {
                entry.scoreText.text = data.GetScoreString();
            }

            if (entry.statsText != null)
            {
                entry.statsText.text = data.GetStatsString();
            }

            // 배경색 설정
            if (entry.background != null)
            {
                if (data.isLocalPlayer)
                {
                    entry.background.color = entry.highlightColor;
                }
                else if (rank <= 3)
                {
                    entry.background.color = entry.top3Color;
                }
                else
                {
                    entry.background.color = entry.normalColor;
                }
            }

            entryUIs.Add(entry);
        }

        /// <summary>
        /// 순위 문자열 반환
        /// </summary>
        string GetRankString(int rank)
        {
            switch (rank)
            {
                case 1:
                    return "🥇 1";
                case 2:
                    return "🥈 2";
                case 3:
                    return "🥉 3";
                default:
                    return rank.ToString();
            }
        }

        /// <summary>
        /// 플레이어 정보 업데이트
        /// </summary>
        void UpdatePlayerInfo()
        {
            if (LeaderboardManager.Instance == null || GameManager.Instance == null) return;

            // 플레이어 이름
            if (playerNameText != null)
            {
                playerNameText.text = LeaderboardManager.Instance.playerName;
            }

            // 플레이어 점수
            if (playerScoreText != null)
            {
                long currentStage = GameManager.Instance.currentStage;
                playerScoreText.text = $"스테이지 {currentStage}";
            }

            // 플레이어 순위
            if (playerRankText != null)
            {
                int rank = LeaderboardManager.Instance.GetPlayerRank();
                if (rank > 0)
                {
                    playerRankText.text = $"순위: {GetRankString(rank)}";
                }
                else
                {
                    playerRankText.text = "순위: -";
                }
            }
        }

        /// <summary>
        /// 점수 제출
        /// </summary>
        void OnSubmitScore()
        {
            if (LeaderboardManager.Instance == null) return;

            LeaderboardManager.Instance.SubmitScore();

            // UI 갱신
            RefreshLeaderboard();
            UpdatePlayerInfo();

            // 알림
            UIManager.Instance?.ShowBuffNotification(
                "점수 제출!",
                "리더보드에 점수가 제출되었습니다.",
                2f
            );

            // 오디오
            AudioManager.Instance?.PlaySFX("Success");
        }

        /// <summary>
        /// 리더보드 초기화
        /// </summary>
        void ClearLeaderboard()
        {
            foreach (var entry in entryUIs)
            {
                if (entry.gameObject != null)
                    Destroy(entry.gameObject);
            }
            entryUIs.Clear();
        }

        /// <summary>
        /// 패널 열기
        /// </summary>
        public void Open()
        {
            gameObject.SetActive(true);
            RefreshLeaderboard();
            UpdatePlayerInfo();
        }

        /// <summary>
        /// 패널 닫기
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 플레이어 이름 변경
        /// </summary>
        public void ChangePlayerName(string newName)
        {
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.SetPlayerName(newName);
                UpdatePlayerInfo();
            }
        }
    }
}
