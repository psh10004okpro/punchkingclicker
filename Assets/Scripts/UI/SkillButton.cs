using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PunchKing
{
    /// <summary>
    /// 스킬 버튼 UI 컴포넌트
    /// 쿨다운 표시 및 스킬 활성화
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SkillButton : MonoBehaviour
    {
        [Header("UI 레퍼런스")]
        public Image skillIcon;
        public TextMeshProUGUI skillNameText;
        public TextMeshProUGUI cooldownText;
        public Image cooldownOverlay;
        public Image glowEffect;
        public ParticleSystem activationParticles;

        [Header("키 바인딩 표시")]
        public TextMeshProUGUI keyBindText;

        [Header("색상")]
        public Color readyColor = Color.white;
        public Color cooldownColor = Color.gray;
        public Color activeColor = Color.yellow;

        private Button button;
        private SkillManager.Skill skillData;
        private bool isReady = true;

        void Awake()
        {
            button = GetComponent<Button>();
        }

        /// <summary>
        /// 스킬 데이터 설정
        /// </summary>
        public void Setup(SkillManager.Skill skill, string keyBind = "")
        {
            skillData = skill;

            // 아이콘
            if (skillIcon != null && skill.icon != null)
            {
                skillIcon.sprite = skill.icon;
            }

            // 이름
            if (skillNameText != null)
            {
                skillNameText.text = skill.skillName;
            }

            // 키 바인딩
            if (keyBindText != null && !string.IsNullOrEmpty(keyBind))
            {
                keyBindText.text = keyBind;
            }

            // 버튼 클릭 이벤트
            if (button != null && skill.skillButton == null)
            {
                button.onClick.AddListener(() => OnSkillClicked());
            }

            UpdateUI();
        }

        void Update()
        {
            if (skillData == null) return;

            UpdateUI();
            CheckKeyInput();
        }

        /// <summary>
        /// UI 업데이트
        /// </summary>
        void UpdateUI()
        {
            if (skillData == null) return;

            float currentCooldown = skillData.currentCooldown;
            float maxCooldown = skillData.cooldown;
            bool isActive = skillData.isActive;

            // 쿨다운 표시
            if (currentCooldown > 0)
            {
                isReady = false;

                // 쿨다운 오버레이
                if (cooldownOverlay != null)
                {
                    cooldownOverlay.fillAmount = currentCooldown / maxCooldown;
                }

                // 쿨다운 텍스트
                if (cooldownText != null)
                {
                    cooldownText.gameObject.SetActive(true);
                    cooldownText.text = Mathf.Ceil(currentCooldown).ToString("F0");
                }

                // 버튼 비활성화
                if (button != null)
                {
                    button.interactable = false;
                }

                // 색상 변경
                if (skillIcon != null)
                {
                    skillIcon.color = cooldownColor;
                }
            }
            else
            {
                isReady = true;

                // 쿨다운 오버레이 숨김
                if (cooldownOverlay != null)
                {
                    cooldownOverlay.fillAmount = 0;
                }

                // 쿨다운 텍스트 숨김
                if (cooldownText != null)
                {
                    cooldownText.gameObject.SetActive(false);
                }

                // 버튼 활성화
                if (button != null)
                {
                    button.interactable = true;
                }

                // 색상 변경
                if (skillIcon != null)
                {
                    skillIcon.color = isActive ? activeColor : readyColor;
                }

                // 준비 완료 효과
                if (glowEffect != null)
                {
                    glowEffect.gameObject.SetActive(!isActive);
                }
            }

            // 활성 상태 표시
            if (isActive && glowEffect != null)
            {
                glowEffect.gameObject.SetActive(true);
                glowEffect.color = activeColor;
            }
        }

        /// <summary>
        /// 스킬 클릭
        /// </summary>
        void OnSkillClicked()
        {
            if (skillData == null || !isReady) return;

            ActivateSkill();
        }

        /// <summary>
        /// 스킬 활성화
        /// </summary>
        void ActivateSkill()
        {
            if (SkillManager.Instance != null)
            {
                SkillManager.Instance.ActivateSkill(skillData);

                // 활성화 파티클
                if (activationParticles != null)
                {
                    activationParticles.Play();
                }

                // 활성화 애니메이션
                PlayActivationAnimation();
            }
        }

        /// <summary>
        /// 활성화 애니메이션
        /// </summary>
        void PlayActivationAnimation()
        {
            // 스케일 애니메이션 (Unity Animation 사용)
            transform.localScale = Vector3.one * 1.2f;
            StartCoroutine(ScaleBackCoroutine());
        }

        System.Collections.IEnumerator ScaleBackCoroutine()
        {
            float duration = 0.3f;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = Vector3.one;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = 1f - Mathf.Pow(1f - t, 3); // EaseOutCubic
                transform.localScale = Vector3.Lerp(startScale, targetScale, t);
                yield return null;
            }

            transform.localScale = targetScale;
        }

        /// <summary>
        /// 키보드 입력 체크 (PC용)
        /// </summary>
        void CheckKeyInput()
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            if (keyBindText != null && !string.IsNullOrEmpty(keyBindText.text))
            {
                string key = keyBindText.text.ToLower();

                if (Input.GetKeyDown(key))
                {
                    OnSkillClicked();
                }
            }
#endif
        }

        /// <summary>
        /// 툴팁 표시
        /// </summary>
        public void ShowTooltip()
        {
            if (skillData == null) return;

            string tooltip = $"<b>{skillData.skillName}</b>\n";
            tooltip += $"{skillData.description}\n\n";
            tooltip += $"쿨다운: {skillData.cooldown}초";

            if (skillData.duration > 0)
            {
                tooltip += $"\n지속 시간: {skillData.duration}초";
            }

            // 툴팁 표시 (UIManager 통해)
            // UIManager.Instance?.ShowTooltip(tooltip, transform.position);
        }

        /// <summary>
        /// 툴팁 숨김
        /// </summary>
        public void HideTooltip()
        {
            // UIManager.Instance?.HideTooltip();
        }
    }
}
