using UnityEngine;
using TMPro;
using System;

namespace PunchKing
{
    /// <summary>
    /// 데미지 팝업 표시
    /// 타격 시 화면에 데미지 숫자가 떠오르는 효과
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class DamagePopup : MonoBehaviour
    {
        [Header("UI 레퍼런스")]
        public TextMeshProUGUI damageText;
        public CanvasGroup canvasGroup;

        [Header("애니메이션 설정")]
        [SerializeField] private float moveSpeed = 100f;
        [SerializeField] private float fadeSpeed = 1f;
        [SerializeField] private float lifetime = 1f;

        [Header("크리티컬 설정")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color criticalColor = Color.yellow;
        [SerializeField] private float normalSize = 32f;
        [SerializeField] private float criticalSize = 48f;

        private RectTransform rectTransform;
        private float timer;
        private Action onComplete;
        private Vector3 moveDirection;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            if (damageText == null)
            {
                damageText = GetComponent<TextMeshProUGUI>();
            }
        }

        /// <summary>
        /// 데미지 팝업 설정
        /// </summary>
        public void Setup(BigNumber damage, bool isCritical)
        {
            if (damageText == null) return;

            // 데미지 텍스트
            damageText.text = damage.ToKoreanString();

            // 크리티컬 여부에 따른 스타일
            if (isCritical)
            {
                damageText.color = criticalColor;
                damageText.fontSize = criticalSize;
                damageText.fontStyle = FontStyles.Bold;
            }
            else
            {
                damageText.color = normalColor;
                damageText.fontSize = normalSize;
                damageText.fontStyle = FontStyles.Normal;
            }

            // 초기화
            canvasGroup.alpha = 1f;
            timer = 0f;

            // 랜덤한 방향으로 움직임 (약간 위쪽으로)
            float randomAngle = UnityEngine.Random.Range(-30f, 30f);
            moveDirection = Quaternion.Euler(0, 0, randomAngle) * Vector3.up;
        }

        /// <summary>
        /// 애니메이션 시작
        /// </summary>
        public void Play(Action onCompleteCallback)
        {
            onComplete = onCompleteCallback;
            timer = 0f;
        }

        void Update()
        {
            if (timer >= lifetime)
            {
                return;
            }

            timer += Time.deltaTime;

            // 위로 이동
            rectTransform.position += moveDirection * moveSpeed * Time.deltaTime;

            // 페이드 아웃
            float alpha = 1f - (timer / lifetime);
            canvasGroup.alpha = alpha;

            // 애니메이션 완료
            if (timer >= lifetime)
            {
                onComplete?.Invoke();
            }
        }
    }
}
