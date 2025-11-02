using UnityEngine;
using UnityEngine.Events;

namespace PunchKing
{
    /// <summary>
    /// 미니게임 메인 관리
    /// 모든 미니게임의 시작, 종료, 보상 관리
    /// </summary>
    public class MiniGameManager : MonoBehaviour
    {
        public static MiniGameManager Instance { get; private set; }

        [Header("미니게임 가용성")]
        public bool canPlayJumpRope = true;
        public bool canPlayBalloonDodge = true;

        [Header("쿨다운 설정")]
        public float miniGameCooldown = 3600f;  // 1시간

        [Header("이벤트")]
        public UnityEvent<string, int> OnMiniGameCompleted;  // 게임 이름, 점수

        private float lastJumpRopeTime = -999999f;
        private float lastBalloonDodgeTime = -999999f;

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

            if (OnMiniGameCompleted == null)
                OnMiniGameCompleted = new UnityEvent<string, int>();
        }

        /// <summary>
        /// 줄넘기 시작 가능 여부
        /// </summary>
        public bool CanPlayJumpRope()
        {
            return canPlayJumpRope && (Time.time - lastJumpRopeTime) >= miniGameCooldown;
        }

        /// <summary>
        /// 풍선 피하기 시작 가능 여부
        /// </summary>
        public bool CanPlayBalloonDodge()
        {
            return canPlayBalloonDodge && (Time.time - lastBalloonDodgeTime) >= miniGameCooldown;
        }

        /// <summary>
        /// 줄넘기 시작
        /// </summary>
        public void StartJumpRope()
        {
            if (!CanPlayJumpRope())
            {
                float remaining = miniGameCooldown - (Time.time - lastJumpRopeTime);
                Debug.Log($"줄넘기 쿨다운: {remaining:F0}초 남음");
                return;
            }

            lastJumpRopeTime = Time.time;

            // 줄넘기 씬 로드 또는 UI 활성화
            var jumpRope = FindObjectOfType<JumpRopeMiniGame>();
            if (jumpRope != null)
            {
                jumpRope.StartGame();
            }
            else
            {
                Debug.LogWarning("JumpRopeMiniGame을 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 풍선 피하기 시작
        /// </summary>
        public void StartBalloonDodge()
        {
            if (!CanPlayBalloonDodge())
            {
                float remaining = miniGameCooldown - (Time.time - lastBalloonDodgeTime);
                Debug.Log($"풍선 피하기 쿨다운: {remaining:F0}초 남음");
                return;
            }

            lastBalloonDodgeTime = Time.time;

            var balloonDodge = FindObjectOfType<BalloonDodgeMiniGame>();
            if (balloonDodge != null)
            {
                balloonDodge.StartGame();
            }
            else
            {
                Debug.LogWarning("BalloonDodgeMiniGame을 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 미니게임 완료 처리
        /// </summary>
        public void OnMiniGameFinished(string gameName, int score)
        {
            // 이벤트 발생
            OnMiniGameCompleted?.Invoke(gameName, score);

            // 보상 지급
            BigNumber reward = CalculateReward(gameName, score);
            GameManager.Instance?.AddGold(reward);

            // UI 알림
            UIManager.Instance?.ShowBuffNotification(
                $"🎮 {gameName} 완료!",
                $"점수: {score}\n보상: {reward.ToKoreanString()} 골드",
                3f
            );

            Debug.Log($"{gameName} 완료! 점수: {score}, 보상: {reward.ToKoreanString()}");
        }

        /// <summary>
        /// 보상 계산
        /// </summary>
        BigNumber CalculateReward(string gameName, int score)
        {
            // 기본 보상 = 점수 × 10
            BigNumber baseReward = new BigNumber(score * 10);

            // 현재 스테이지에 따라 보상 증가
            if (GameManager.Instance != null)
            {
                int stage = GameManager.Instance.currentStage;
                float stageMultiplier = 1f + (stage * 0.1f);
                baseReward = baseReward.Multiply(stageMultiplier);
            }

            return baseReward;
        }

        /// <summary>
        /// 줄넘기 남은 쿨다운
        /// </summary>
        public float GetJumpRopeCooldown()
        {
            return Mathf.Max(0, miniGameCooldown - (Time.time - lastJumpRopeTime));
        }

        /// <summary>
        /// 풍선 피하기 남은 쿨다운
        /// </summary>
        public float GetBalloonDodgeCooldown()
        {
            return Mathf.Max(0, miniGameCooldown - (Time.time - lastBalloonDodgeTime));
        }

#if UNITY_EDITOR
        [ContextMenu("Reset All Cooldowns")]
        void EditorResetCooldowns()
        {
            lastJumpRopeTime = -999999f;
            lastBalloonDodgeTime = -999999f;
            Debug.Log("미니게임 쿨다운 리셋됨");
        }
#endif
    }
}
