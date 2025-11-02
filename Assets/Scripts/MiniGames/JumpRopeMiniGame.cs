using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace PunchKing
{
    /// <summary>
    /// 줄넘기 미니게임
    /// 타이밍 맞춰 탭하여 점수 획득
    /// </summary>
    public class JumpRopeMiniGame : MonoBehaviour
    {
        [Header("UI")]
        public GameObject gamePanel;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI comboText;
        public Image timingIndicator;
        public TextMeshProUGUI countdownText;
        public Button jumpButton;

        [Header("설정")]
        public float gameDuration = 30f;
        public float ropeSpeed = 1f;
        public float perfectZone = 0.1f;  // 완벽한 타이밍 범위
        public float goodZone = 0.25f;     // 좋은 타이밍 범위

        private int score = 0;
        private int combo = 0;
        private int maxCombo = 0;
        private float gameTimer = 0f;
        private float ropePhase = 0f;
        private bool isPlaying = false;
        private bool waitingForJump = false;

        void Awake()
        {
            if (jumpButton != null)
            {
                jumpButton.onClick.AddListener(OnJumpClicked);
            }
        }

        /// <summary>
        /// 게임 시작
        /// </summary>
        public void StartGame()
        {
            score = 0;
            combo = 0;
            maxCombo = 0;
            gameTimer = gameDuration;
            ropePhase = 0f;
            isPlaying = true;
            waitingForJump = false;

            if (gamePanel != null)
                gamePanel.SetActive(true);

            UpdateUI();

            StartCoroutine(GameLoop());
        }

        /// <summary>
        /// 게임 루프
        /// </summary>
        IEnumerator GameLoop()
        {
            while (gameTimer > 0 && isPlaying)
            {
                gameTimer -= Time.deltaTime;
                ropePhase += Time.deltaTime * ropeSpeed;

                // 줄넘기 타이밍 (1초마다)
                if (ropePhase >= 1f)
                {
                    ropePhase = 0f;
                    waitingForJump = true;
                }

                // 타이밍을 놓친 경우
                if (waitingForJump && ropePhase > goodZone)
                {
                    MissedJump();
                    waitingForJump = false;
                }

                UpdateTimingIndicator();
                UpdateUI();

                yield return null;
            }

            EndGame();
        }

        /// <summary>
        /// 점프 버튼 클릭
        /// </summary>
        void OnJumpClicked()
        {
            if (!isPlaying || !waitingForJump) return;

            // 타이밍 평가
            if (ropePhase <= perfectZone)
            {
                // 퍼펙트!
                PerfectJump();
            }
            else if (ropePhase <= goodZone)
            {
                // 굿!
                GoodJump();
            }
            else
            {
                // 미스
                MissedJump();
            }

            waitingForJump = false;
        }

        /// <summary>
        /// 퍼펙트 점프
        /// </summary>
        void PerfectJump()
        {
            combo++;
            int points = 100 + (combo * 10);  // 콤보 보너스
            score += points;

            if (combo > maxCombo)
                maxCombo = combo;

            // 이펙트
            ShowJudgement("PERFECT!", Color.yellow);
            AudioManager.Instance?.PlaySound("Click");
        }

        /// <summary>
        /// 좋은 점프
        /// </summary>
        void GoodJump()
        {
            combo++;
            int points = 50 + (combo * 5);
            score += points;

            if (combo > maxCombo)
                maxCombo = combo;

            ShowJudgement("GOOD", Color.green);
            AudioManager.Instance?.PlaySound("Click");
        }

        /// <summary>
        /// 미스
        /// </summary>
        void MissedJump()
        {
            combo = 0;
            ShowJudgement("MISS", Color.red);
        }

        /// <summary>
        /// 판정 표시
        /// </summary>
        void ShowJudgement(string text, Color color)
        {
            // TODO: 판정 텍스트 애니메이션
            Debug.Log(text);
        }

        /// <summary>
        /// 타이밍 인디케이터 업데이트
        /// </summary>
        void UpdateTimingIndicator()
        {
            if (timingIndicator == null) return;

            // 줄넘기 위치 표시
            timingIndicator.fillAmount = ropePhase;

            // 타이밍 존 색상
            if (ropePhase <= perfectZone)
            {
                timingIndicator.color = Color.yellow;
            }
            else if (ropePhase <= goodZone)
            {
                timingIndicator.color = Color.green;
            }
            else
            {
                timingIndicator.color = Color.red;
            }
        }

        /// <summary>
        /// UI 업데이트
        /// </summary>
        void UpdateUI()
        {
            if (scoreText != null)
                scoreText.text = $"점수: {score}";

            if (comboText != null)
            {
                comboText.text = combo > 0 ? $"콤보: {combo}" : "";
            }

            if (countdownText != null)
                countdownText.text = $"{gameTimer:F1}초";
        }

        /// <summary>
        /// 게임 종료
        /// </summary>
        void EndGame()
        {
            isPlaying = false;

            // 보상 지급
            MiniGameManager.Instance?.OnMiniGameFinished("줄넘기", score);

            // 패널 닫기
            if (gamePanel != null)
            {
                gamePanel.SetActive(false);
            }

            // 결과 표시
            ShowResults();
        }

        /// <summary>
        /// 결과 표시
        /// </summary>
        void ShowResults()
        {
            string results = $"줄넘기 결과\n\n";
            results += $"점수: {score}\n";
            results += $"최대 콤보: {maxCombo}\n\n";
            results += $"보상: {MiniGameManager.Instance?.CalculateReward("줄넘기", score).ToKoreanString()} 골드";

            Debug.Log(results);
            // TODO: 결과 팝업 UI 표시
        }

        /// <summary>
        /// 게임 중단
        /// </summary>
        public void QuitGame()
        {
            isPlaying = false;
            StopAllCoroutines();

            if (gamePanel != null)
                gamePanel.SetActive(false);
        }

#if UNITY_STANDALONE || UNITY_EDITOR
        void Update()
        {
            // PC: 스페이스바로 점프
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnJumpClicked();
            }
        }
#endif
    }
}
