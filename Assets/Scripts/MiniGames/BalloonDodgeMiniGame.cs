using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace PunchKing
{
    /// <summary>
    /// 풍선 피하기 미니게임
    /// 떨어지는 풍선을 피하며 생존 시간으로 점수 획득
    /// </summary>
    public class BalloonDodgeMiniGame : MonoBehaviour
    {
        [Header("UI")]
        public GameObject gamePanel;
        public RectTransform gameArea;
        public RectTransform player;
        public GameObject balloonPrefab;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI livesText;

        [Header("설정")]
        public int startingLives = 3;
        public float balloonSpawnInterval = 1f;
        public float balloonSpeed = 200f;
        public float playerSpeed = 500f;

        private int lives = 0;
        private float score = 0f;
        private bool isPlaying = false;
        private Vector2 playerPosition;
        private List<RectTransform> activeBalloons = new List<RectTransform>();
        private float spawnTimer = 0f;

        void Awake()
        {
            if (player != null)
            {
                playerPosition = player.anchoredPosition;
            }
        }

        /// <summary>
        /// 게임 시작
        /// </summary>
        public void StartGame()
        {
            lives = startingLives;
            score = 0f;
            isPlaying = true;
            spawnTimer = 0f;

            // 기존 풍선 제거
            ClearBalloons();

            // 플레이어 위치 초기화
            if (player != null)
            {
                player.anchoredPosition = new Vector2(0, -gameArea.rect.height / 2 + 50);
                playerPosition = player.anchoredPosition;
            }

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
            while (lives > 0 && isPlaying)
            {
                // 점수 증가 (생존 시간)
                score += Time.deltaTime * 10f;

                // 풍선 생성
                spawnTimer -= Time.deltaTime;
                if (spawnTimer <= 0)
                {
                    SpawnBalloon();
                    spawnTimer = balloonSpawnInterval;

                    // 난이도 증가 (점점 빨리 생성)
                    if (balloonSpawnInterval > 0.3f)
                    {
                        balloonSpawnInterval -= 0.01f;
                    }
                }

                // 풍선 이동 및 충돌 체크
                UpdateBalloons();

                // 플레이어 이동 (모바일)
                HandleInput();

                UpdateUI();

                yield return null;
            }

            EndGame();
        }

        /// <summary>
        /// 풍선 생성
        /// </summary>
        void SpawnBalloon()
        {
            if (balloonPrefab == null || gameArea == null) return;

            GameObject balloon = Instantiate(balloonPrefab, gameArea);
            RectTransform balloonRect = balloon.GetComponent<RectTransform>();

            // 랜덤 X 위치
            float randomX = Random.Range(-gameArea.rect.width / 2, gameArea.rect.width / 2);
            balloonRect.anchoredPosition = new Vector2(randomX, gameArea.rect.height / 2);

            activeBalloons.Add(balloonRect);
        }

        /// <summary>
        /// 풍선 업데이트
        /// </summary>
        void UpdateBalloons()
        {
            for (int i = activeBalloons.Count - 1; i >= 0; i--)
            {
                RectTransform balloon = activeBalloons[i];
                if (balloon == null)
                {
                    activeBalloons.RemoveAt(i);
                    continue;
                }

                // 아래로 이동
                balloon.anchoredPosition += Vector2.down * balloonSpeed * Time.deltaTime;

                // 화면 밖으로 나가면 제거
                if (balloon.anchoredPosition.y < -gameArea.rect.height / 2 - 100)
                {
                    Destroy(balloon.gameObject);
                    activeBalloons.RemoveAt(i);
                    continue;
                }

                // 플레이어 충돌 체크
                if (CheckCollision(player, balloon))
                {
                    OnHit();
                    Destroy(balloon.gameObject);
                    activeBalloons.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 충돌 체크
        /// </summary>
        bool CheckCollision(RectTransform a, RectTransform b)
        {
            if (a == null || b == null) return false;

            float distance = Vector2.Distance(a.anchoredPosition, b.anchoredPosition);
            float collisionRadius = 50f;  // 충돌 반경

            return distance < collisionRadius;
        }

        /// <summary>
        /// 입력 처리
        /// </summary>
        void HandleInput()
        {
            if (player == null || gameArea == null) return;

#if UNITY_STANDALONE || UNITY_EDITOR
            // PC: 마우스 위치 추적
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                gameArea, Input.mousePosition, null, out mousePos
            );
            playerPosition.x = mousePos.x;
#elif UNITY_ANDROID || UNITY_IOS
            // 모바일: 터치 드래그
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector2 touchPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    gameArea, touch.position, null, out touchPos
                );
                playerPosition.x = touchPos.x;
            }
#endif

            // X 범위 제한
            float halfWidth = gameArea.rect.width / 2 - 30;
            playerPosition.x = Mathf.Clamp(playerPosition.x, -halfWidth, halfWidth);

            // 플레이어 이동
            player.anchoredPosition = Vector2.Lerp(
                player.anchoredPosition,
                playerPosition,
                Time.deltaTime * 10f
            );
        }

        /// <summary>
        /// 피격
        /// </summary>
        void OnHit()
        {
            lives--;

            // 카메라 셰이크
            CameraShake.Instance?.Shake(0.2f, 0.3f);

            // 사운드
            AudioManager.Instance?.PlaySound("SandbagHit");

            // 생명력 0이면 게임 오버
            if (lives <= 0)
            {
                isPlaying = false;
            }
        }

        /// <summary>
        /// UI 업데이트
        /// </summary>
        void UpdateUI()
        {
            if (scoreText != null)
                scoreText.text = $"점수: {(int)score}";

            if (livesText != null)
            {
                string hearts = "";
                for (int i = 0; i < lives; i++)
                {
                    hearts += "❤️";
                }
                livesText.text = hearts;
            }
        }

        /// <summary>
        /// 기존 풍선 제거
        /// </summary>
        void ClearBalloons()
        {
            foreach (var balloon in activeBalloons)
            {
                if (balloon != null)
                    Destroy(balloon.gameObject);
            }
            activeBalloons.Clear();
        }

        /// <summary>
        /// 게임 종료
        /// </summary>
        void EndGame()
        {
            isPlaying = false;

            // 보상 지급
            MiniGameManager.Instance?.OnMiniGameFinished("풍선 피하기", (int)score);

            // 풍선 제거
            ClearBalloons();

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
            string results = $"풍선 피하기 결과\n\n";
            results += $"점수: {(int)score}\n\n";
            results += $"보상: {MiniGameManager.Instance?.CalculateReward("풍선 피하기", (int)score).ToKoreanString()} 골드";

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
            ClearBalloons();

            if (gamePanel != null)
                gamePanel.SetActive(false);
        }
    }
}
