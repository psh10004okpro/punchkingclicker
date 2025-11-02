using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace PunchKing
{
    /// <summary>
    /// 플로팅 텍스트 풀
    /// 데미지/골드/경험치 등 표시용 텍스트 오브젝트 풀링
    /// </summary>
    public class FloatingTextPool : MonoBehaviour
    {
        public static FloatingTextPool Instance { get; private set; }

        [Header("프리팹")]
        public GameObject floatingTextPrefab;

        [Header("풀 설정")]
        public int poolSize = 30;
        public Transform poolParent;

        [Header("애니메이션 설정")]
        public float moveSpeed = 2f;
        public float lifetime = 1.5f;
        public float fadeStartTime = 0.8f;

        private Queue<FloatingText> textPool = new Queue<FloatingText>();
        private List<FloatingText> activeTexts = new List<FloatingText>();

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (poolParent == null)
            {
                poolParent = new GameObject("FloatingTextPool").transform;
                poolParent.SetParent(transform);
            }

            InitializePool();
        }

        /// <summary>
        /// 풀 초기화
        /// </summary>
        void InitializePool()
        {
            // 프리팹이 없으면 기본 생성
            if (floatingTextPrefab == null)
            {
                floatingTextPrefab = CreateDefaultFloatingText();
            }

            for (int i = 0; i < poolSize; i++)
            {
                CreateFloatingTextObject();
            }
        }

        /// <summary>
        /// 기본 플로팅 텍스트 프리팹 생성
        /// </summary>
        GameObject CreateDefaultFloatingText()
        {
            GameObject prefab = new GameObject("FloatingText");

            Canvas canvas = prefab.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 100;

            RectTransform rectTransform = prefab.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 100);
            rectTransform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            TextMeshProUGUI text = prefab.AddComponent<TextMeshProUGUI>();
            text.fontSize = 48;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.enableWordWrapping = false;

            // 외곽선 추가
            text.outlineWidth = 0.2f;
            text.outlineColor = Color.black;

            FloatingText floatingText = prefab.AddComponent<FloatingText>();

            return prefab;
        }

        /// <summary>
        /// 플로팅 텍스트 오브젝트 생성
        /// </summary>
        void CreateFloatingTextObject()
        {
            GameObject obj = Instantiate(floatingTextPrefab, poolParent);
            FloatingText floatingText = obj.GetComponent<FloatingText>();

            if (floatingText == null)
            {
                floatingText = obj.AddComponent<FloatingText>();
            }

            floatingText.pool = this;
            obj.SetActive(false);
            textPool.Enqueue(floatingText);
        }

        /// <summary>
        /// 풀에서 텍스트 가져오기
        /// </summary>
        FloatingText GetFromPool()
        {
            if (textPool.Count == 0)
            {
                CreateFloatingTextObject();
            }

            FloatingText text = textPool.Dequeue();
            text.gameObject.SetActive(true);
            activeTexts.Add(text);
            return text;
        }

        /// <summary>
        /// 풀로 반환
        /// </summary>
        public void ReturnToPool(FloatingText text)
        {
            if (text == null) return;

            text.gameObject.SetActive(false);
            text.transform.SetParent(poolParent);
            activeTexts.Remove(text);
            textPool.Enqueue(text);
        }

        /// <summary>
        /// 플로팅 텍스트 표시 (기본)
        /// </summary>
        public void Show(string message, Vector3 position, Color color)
        {
            FloatingText text = GetFromPool();
            text.Initialize(message, position, color, moveSpeed, lifetime, fadeStartTime);
        }

        /// <summary>
        /// 데미지 텍스트 표시
        /// </summary>
        public void ShowDamage(BigNumber damage, Vector3 position, bool isCritical = false)
        {
            string message = damage.ToKoreanString();
            Color color = isCritical ? new Color(1f, 0.2f, 0.2f) : new Color(1f, 1f, 1f);
            float size = isCritical ? 1.5f : 1f;

            if (isCritical)
            {
                message = "크리티컬!\n" + message;
            }

            FloatingText text = GetFromPool();
            text.Initialize(message, position, color, moveSpeed * size, lifetime, fadeStartTime);

            if (isCritical)
            {
                text.transform.localScale *= 1.3f;
            }
        }

        /// <summary>
        /// 골드 텍스트 표시
        /// </summary>
        public void ShowGold(BigNumber amount, Vector3 position)
        {
            string message = "+" + amount.ToKoreanString() + " 골드";
            Color color = new Color(1f, 0.84f, 0f);

            FloatingText text = GetFromPool();
            text.Initialize(message, position, color, moveSpeed, lifetime, fadeStartTime);
        }

        /// <summary>
        /// 경험치 텍스트 표시
        /// </summary>
        public void ShowExp(long amount, Vector3 position)
        {
            string message = "+" + amount + " EXP";
            Color color = new Color(0.3f, 0.8f, 1f);

            FloatingText text = GetFromPool();
            text.Initialize(message, position, color, moveSpeed, lifetime, fadeStartTime);
        }

        /// <summary>
        /// 커스텀 텍스트 표시
        /// </summary>
        public void ShowCustom(string message, Vector3 position, Color color, float scale = 1f)
        {
            FloatingText text = GetFromPool();
            text.Initialize(message, position, color, moveSpeed, lifetime, fadeStartTime);
            text.transform.localScale *= scale;
        }

        void Update()
        {
            // 활성 텍스트 업데이트
            for (int i = activeTexts.Count - 1; i >= 0; i--)
            {
                if (activeTexts[i] != null && !activeTexts[i].gameObject.activeSelf)
                {
                    activeTexts.RemoveAt(i);
                }
            }
        }
    }

    /// <summary>
    /// 플로팅 텍스트 컴포넌트
    /// </summary>
    public class FloatingText : MonoBehaviour
    {
        [HideInInspector] public FloatingTextPool pool;

        private TextMeshProUGUI textMesh;
        private CanvasGroup canvasGroup;
        private float moveSpeed;
        private float lifetime;
        private float fadeStartTime;
        private float elapsedTime;
        private Vector3 velocity;

        void Awake()
        {
            textMesh = GetComponentInChildren<TextMeshProUGUI>();
            if (textMesh == null)
            {
                textMesh = GetComponent<TextMeshProUGUI>();
            }

            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize(string message, Vector3 position, Color color, float speed, float life, float fadeStart)
        {
            if (textMesh == null)
            {
                Awake();
            }

            textMesh.text = message;
            textMesh.color = color;
            transform.position = position;
            moveSpeed = speed;
            lifetime = life;
            fadeStartTime = fadeStart;
            elapsedTime = 0f;

            canvasGroup.alpha = 1f;

            // 랜덤한 방향으로 약간 이동
            float randomAngle = Random.Range(-30f, 30f);
            velocity = Quaternion.Euler(0f, 0f, randomAngle) * Vector3.up * moveSpeed;
        }

        void Update()
        {
            elapsedTime += Time.deltaTime;

            // 이동
            transform.position += velocity * Time.deltaTime;

            // 페이드 아웃
            if (elapsedTime >= fadeStartTime)
            {
                float fadeProgress = (elapsedTime - fadeStartTime) / (lifetime - fadeStartTime);
                canvasGroup.alpha = 1f - fadeProgress;
            }

            // 생명 종료
            if (elapsedTime >= lifetime)
            {
                pool?.ReturnToPool(this);
            }
        }
    }
}
