using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PunchKing
{
    /// <summary>
    /// 보스 적
    /// 10스테이지마다 등장하는 강력한 적
    /// 샌드백보다 10배 높은 체력
    /// </summary>
    public class Boss : MonoBehaviour
    {
        [Header("체력")]
        public BigNumber maxHealth;
        public BigNumber currentHealth;

        [Header("UI 레퍼런스")]
        public Slider healthBar;
        public TextMeshProUGUI healthText;
        public TextMeshProUGUI bossNameText;
        public TextMeshProUGUI stageText;

        [Header("애니메이션")]
        public Animator animator;

        [Header("이펙트")]
        public ParticleSystem destroyParticles;
        public ParticleSystem hitParticles;

        [Header("보스 정보")]
        public string bossName = "강력한 보스";
        public Color bossColor = Color.red;

        private int stageNumber;

        /// <summary>
        /// 보스 초기화
        /// </summary>
        public void Initialize(BigNumber health, int stage)
        {
            maxHealth = health.Clone();
            currentHealth = health.Clone();
            stageNumber = stage;

            // 보스 이름 설정 (스테이지에 따라)
            SetBossName(stage);

            UpdateUI();

            // 등장 애니메이션
            if (animator != null)
            {
                animator.SetTrigger("BossSpawn");
            }

            // 등장 사운드
            AudioManager.Instance?.PlaySound("BossAppear");

            // 카메라 셰이크
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.3f, 0.5f);
            }
        }

        /// <summary>
        /// 스테이지에 따른 보스 이름 설정
        /// </summary>
        void SetBossName(int stage)
        {
            int bossNumber = stage / 10;

            string[] bossNames = new string[]
            {
                "챔피언 샌드백",
                "헤비급 샌드백",
                "타이탄 샌드백",
                "레전드 샌드백",
                "불멸의 샌드백",
                "신화의 샌드백",
                "우주의 샌드백",
                "절대자 샌드백"
            };

            if (bossNumber < bossNames.Length)
            {
                bossName = bossNames[bossNumber];
            }
            else
            {
                bossName = $"레벨 {bossNumber} 보스";
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
                animator.SetTrigger("BossHit");
            }

            // 타격 이펙트
            if (hitParticles != null)
            {
                ParticleSystem particles = Instantiate(hitParticles, transform.position, Quaternion.identity);
                Destroy(particles.gameObject, 2f);
            }

            // UI 업데이트
            UpdateUI();

            // 사운드
            AudioManager.Instance?.PlaySound("BossHit");

            // 체력이 50% 이하일 때 분노 모드
            float healthPercent = (float)(currentHealth.ToDouble() / maxHealth.ToDouble());
            if (healthPercent < 0.5f && animator != null)
            {
                animator.SetBool("IsEnraged", true);
            }
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

                // 보스 체력바는 빨간색
                if (healthBar.fillRect != null)
                {
                    healthBar.fillRect.GetComponent<Image>().color = bossColor;
                }
            }

            // 체력 텍스트
            if (healthText != null)
            {
                healthText.text = $"{currentHealth.ToKoreanString()} / {maxHealth.ToKoreanString()}";
            }

            // 보스 이름
            if (bossNameText != null)
            {
                bossNameText.text = bossName;
                bossNameText.color = bossColor;
            }

            // 스테이지 텍스트
            if (stageText != null)
            {
                stageText.text = $"BOSS Stage {stageNumber}";
            }
        }

        /// <summary>
        /// 보스 파괴
        /// </summary>
        void OnDestroyed()
        {
            // 파괴 이펙트 (더 화려하게)
            if (destroyParticles != null)
            {
                ParticleSystem particles = Instantiate(destroyParticles, transform.position, Quaternion.identity);
                Destroy(particles.gameObject, 5f);
            }

            // 파괴 애니메이션
            if (animator != null)
            {
                animator.SetTrigger("BossDestroy");
            }

            // 사운드
            AudioManager.Instance?.PlaySound("BossDefeat");

            // 강한 카메라 셰이크
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.7f, 1f);
            }

            // 승리 UI 표시
            UIManager.Instance?.ShowBossDefeatPopup(bossName);

            // 게임 매니저에 알림
            GameManager.Instance?.OnEnemyDestroyed();

            // 오브젝트 제거 (애니메이션 후)
            Destroy(gameObject, 1f);
        }

        /// <summary>
        /// 클릭 이벤트
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
