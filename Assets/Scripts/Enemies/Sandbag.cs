using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PunchKing
{
    /// <summary>
    /// 샌드백 (일반 적)
    /// 플레이어가 클릭하여 공격하는 타겟
    /// </summary>
    public class Sandbag : MonoBehaviour
    {
        [Header("체력")]
        public BigNumber maxHealth;
        public BigNumber currentHealth;

        [Header("UI 레퍼런스")]
        public Slider healthBar;
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI stageText;

        [Header("애니메이션")]
        public Animator animator;

        [Header("이펙트")]
        public ParticleSystem destroyParticles;

        private int stageNumber;

        /// <summary>
        /// 샌드백 초기화
        /// </summary>
        public void Initialize(BigNumber health, int stage)
        {
            maxHealth = health.Clone();
            currentHealth = health.Clone();
            stageNumber = stage;

            UpdateUI();

            // 애니메이터 초기화
            if (animator != null)
            {
                animator.SetTrigger("Spawn");
            }
        }

        /// <summary>
        /// 데미지 받기
        /// </summary>
        public void TakeDamage(BigNumber damage)
        {
            currentHealth = currentHealth.Subtract(damage);

            // 최소 0
            if (currentHealth.IsLessThan(new BigNumber(0)) || currentHealth.IsZero())
            {
                currentHealth = new BigNumber(0);
                OnDestroyed();
                return;
            }

            // 타격 애니메이션
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }

            // UI 업데이트
            UpdateUI();

            // 사운드
            AudioManager.Instance?.PlaySound("SandbagHit");
        }

        /// <summary>
        /// UI 업데이트
        /// </summary>
        void UpdateUI()
        {
            // 체력 바
            if (healthBar != null)
            {
                float healthPercent = (float)(currentHealth.ToDouble() / maxHealth.ToDouble());
                healthBar.value = Mathf.Clamp01(healthPercent);
            }

            // 체력 텍스트
            if (healthText != null)
            {
                healthText.text = $"{currentHealth.ToKoreanString()} / {maxHealth.ToKoreanString()}";
            }

            // 스테이지 텍스트
            if (stageText != null)
            {
                stageText.text = $"Stage {stageNumber}";
            }
        }

        /// <summary>
        /// 샌드백 파괴
        /// </summary>
        void OnDestroyed()
        {
            // 파괴 이펙트
            if (destroyParticles != null)
            {
                ParticleSystem particles = Instantiate(destroyParticles, transform.position, Quaternion.identity);
                Destroy(particles.gameObject, 3f);
            }

            // 파괴 애니메이션
            if (animator != null)
            {
                animator.SetTrigger("Destroy");
            }

            // 사운드
            AudioManager.Instance?.PlaySound("SandbagDestroy");

            // 게임 매니저에 알림
            GameManager.Instance?.OnEnemyDestroyed();

            // 오브젝트 제거 (애니메이션 후)
            Destroy(gameObject, 0.5f);
        }

        /// <summary>
        /// 클릭 이벤트 (모바일 터치 포함)
        /// </summary>
        void OnMouseDown()
        {
            Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickPosition.z = 0;
            GameManager.Instance?.OnSandbagClicked(clickPosition);
        }

#if UNITY_ANDROID || UNITY_IOS
        /// <summary>
        /// 터치 입력 처리 (모바일)
        /// </summary>
        void Update()
        {
            if (Input.touchCount > 0)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        Ray ray = Camera.main.ScreenPointToRay(touch.position);
                        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

                        if (hit.collider != null && hit.collider.gameObject == gameObject)
                        {
                            Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                            touchPosition.z = 0;
                            GameManager.Instance?.OnSandbagClicked(touchPosition);
                        }
                    }
                }
            }
        }
#endif
    }
}
