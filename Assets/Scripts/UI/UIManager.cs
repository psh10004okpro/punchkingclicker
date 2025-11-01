using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace PunchKing
{
    /// <summary>
    /// UI 전체 관리
    /// 모바일 Safe Area, 데미지 팝업, 패널 관리 등
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("상단 UI")]
        public TextMeshProUGUI goldText;
        public TextMeshProUGUI stageText;
        public TextMeshProUGUI dpsText;

        [Header("패널")]
        public GameObject upgradePanel;
        public GameObject skillPanel;
        public GameObject settingsPanel;
        public GameObject prestigePanel;

        [Header("데미지 팝업")]
        public GameObject damagePopupPrefab;
        public Transform damagePopupContainer;
        private Queue<DamagePopup> damagePopupPool = new Queue<DamagePopup>();
        private int poolSize = 50;

        [Header("알림")]
        public GameObject notificationPrefab;
        public Transform notificationContainer;

        [Header("Safe Area")]
        public RectTransform safeAreaPanel;

        [Header("팝업")]
        public GameObject offlineEarningsPopup;
        public TextMeshProUGUI offlineEarningsText;
        public GameObject bossDefeatPopup;
        public TextMeshProUGUI bossDefeatText;

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

            ApplySafeArea();
            InitializeDamagePopupPool();
        }

        void Start()
        {
            // 게임 매니저 이벤트 구독
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged.AddListener(UpdateGoldDisplay);
                GameManager.Instance.OnStageChanged.AddListener(UpdateStageDisplay);
            }

            // 초기 UI 업데이트
            UpdateUI();

            // DPS 업데이트 코루틴
            StartCoroutine(UpdateDPSDisplay());
        }

        /// <summary>
        /// Safe Area 적용 (노치 대응)
        /// </summary>
        void ApplySafeArea()
        {
            if (safeAreaPanel == null) return;

            Rect safeArea = Screen.safeArea;

            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            safeAreaPanel.anchorMin = anchorMin;
            safeAreaPanel.anchorMax = anchorMax;

            Debug.Log($"Safe Area 적용: {anchorMin} ~ {anchorMax}");
        }

        /// <summary>
        /// 데미지 팝업 풀 초기화
        /// </summary>
        void InitializeDamagePopupPool()
        {
            if (damagePopupPrefab == null || damagePopupContainer == null)
            {
                Debug.LogWarning("데미지 팝업 프리팹 또는 컨테이너가 없습니다");
                return;
            }

            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(damagePopupPrefab, damagePopupContainer);
                DamagePopup popup = obj.GetComponent<DamagePopup>();
                popup.gameObject.SetActive(false);
                damagePopupPool.Enqueue(popup);
            }
        }

        /// <summary>
        /// 골드 표시 업데이트
        /// </summary>
        public void UpdateGoldDisplay(BigNumber gold)
        {
            if (goldText != null)
            {
                goldText.text = gold.ToKoreanString();
            }
        }

        /// <summary>
        /// 스테이지 표시 업데이트
        /// </summary>
        public void UpdateStageDisplay(int stage)
        {
            if (stageText != null)
            {
                stageText.text = $"Stage {stage}";
            }
        }

        /// <summary>
        /// DPS 표시 업데이트 (1초마다)
        /// </summary>
        IEnumerator UpdateDPSDisplay()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                if (dpsText != null && GameManager.Instance != null)
                {
                    BigNumber goldPerSec = GameManager.Instance.goldPerSecond;
                    dpsText.text = $"골드/초: {goldPerSec.ToKoreanString()}";
                }
            }
        }

        /// <summary>
        /// 전체 UI 업데이트
        /// </summary>
        public void UpdateUI()
        {
            if (GameManager.Instance != null)
            {
                UpdateGoldDisplay(GameManager.Instance.totalGold);
                UpdateStageDisplay(GameManager.Instance.currentStage);
            }
        }

        /// <summary>
        /// 데미지 팝업 표시
        /// </summary>
        public void ShowDamagePopup(Vector3 worldPosition, BigNumber damage, bool isCritical)
        {
            if (damagePopupPool.Count == 0)
            {
                // 풀이 부족하면 추가 생성
                GameObject obj = Instantiate(damagePopupPrefab, damagePopupContainer);
                DamagePopup popup = obj.GetComponent<DamagePopup>();
                damagePopupPool.Enqueue(popup);
            }

            DamagePopup damagePopup = damagePopupPool.Dequeue();
            damagePopup.gameObject.SetActive(true);

            // 월드 좌표를 스크린 좌표로 변환
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            damagePopup.transform.position = screenPos;

            // 데미지 설정 및 애니메이션 시작
            damagePopup.Setup(damage, isCritical);
            damagePopup.Play(() =>
            {
                damagePopup.gameObject.SetActive(false);
                damagePopupPool.Enqueue(damagePopup);
            });
        }

        /// <summary>
        /// 버프 알림 표시
        /// </summary>
        public void ShowBuffNotification(string title, string description, float duration)
        {
            if (notificationPrefab != null && notificationContainer != null)
            {
                GameObject notification = Instantiate(notificationPrefab, notificationContainer);
                TextMeshProUGUI[] texts = notification.GetComponentsInChildren<TextMeshProUGUI>();

                if (texts.Length >= 2)
                {
                    texts[0].text = title;
                    texts[1].text = description;
                }

                Destroy(notification, 3f);
            }
        }

        /// <summary>
        /// 오프라인 수익 팝업
        /// </summary>
        public void ShowOfflineEarningsPopup(BigNumber earnings, long seconds)
        {
            if (offlineEarningsPopup == null) return;

            offlineEarningsPopup.SetActive(true);

            if (offlineEarningsText != null)
            {
                int hours = (int)(seconds / 3600);
                int minutes = (int)((seconds % 3600) / 60);

                string timeText = hours > 0 ? $"{hours}시간 {minutes}분" : $"{minutes}분";
                offlineEarningsText.text = $"오프라인 동안 {timeText}이 지났습니다!\n\n{earnings.ToKoreanString()} 골드를 획득했습니다!";
            }
        }

        /// <summary>
        /// 오프라인 수익 팝업 닫기
        /// </summary>
        public void CloseOfflineEarningsPopup()
        {
            if (offlineEarningsPopup != null)
            {
                offlineEarningsPopup.SetActive(false);
            }
        }

        /// <summary>
        /// 보스 격파 팝업
        /// </summary>
        public void ShowBossDefeatPopup(string bossName)
        {
            if (bossDefeatPopup == null) return;

            bossDefeatPopup.SetActive(true);

            if (bossDefeatText != null)
            {
                bossDefeatText.text = $"{bossName} 격파!";
            }

            // 3초 후 자동 닫기
            StartCoroutine(CloseBossDefeatPopupAfterDelay(3f));
        }

        IEnumerator CloseBossDefeatPopupAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (bossDefeatPopup != null)
            {
                bossDefeatPopup.SetActive(false);
            }
        }

        /// <summary>
        /// 패널 열기/닫기
        /// </summary>
        public void TogglePanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(!panel.activeSelf);
            }
        }
    }
}
