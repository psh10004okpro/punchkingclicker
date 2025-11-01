using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace PunchKing
{
    /// <summary>
    /// 스킬 시스템 관리
    /// 원투펀치, 박치기, 콤보러쉬, 골드러쉬 등의 스킬 처리
    /// </summary>
    public class SkillManager : MonoBehaviour
    {
        public static SkillManager Instance { get; private set; }

        /// <summary>
        /// 스킬 데이터 구조
        /// </summary>
        [System.Serializable]
        public class Skill
        {
            public string skillName;
            public string description;
            public Sprite icon;
            public float cooldown;          // 쿨다운 시간 (초)
            public float duration;          // 지속 시간 (버프 스킬용)
            public float effectMultiplier;  // 효과 배율

            // UI 레퍼런스
            public Button skillButton;
            public Image cooldownOverlay;
            public TextMeshProUGUI cooldownText;

            // 런타임 데이터
            [HideInInspector] public float currentCooldown;
            [HideInInspector] public bool isActive;
        }

        [Header("스킬 목록")]
        public Skill oneTwoPunch;   // 원투펀치: 즉시 2회 타격
        public Skill headbutt;      // 박치기: 강력한 단일 타격
        public Skill comboRush;     // 콤보러쉬: 일정 시간 데미지 증가
        public Skill goldRush;      // 골드러쉬: 일정 시간 골드 획득 증가

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
        }

        void Start()
        {
            InitializeSkills();
        }

        void Update()
        {
            UpdateCooldowns(Time.deltaTime);
        }

        /// <summary>
        /// 스킬 초기화
        /// </summary>
        void InitializeSkills()
        {
            // 원투펀치 기본 설정
            if (oneTwoPunch.skillButton != null)
            {
                oneTwoPunch.skillName = "원투펀치";
                oneTwoPunch.description = "즉시 2회 타격";
                oneTwoPunch.cooldown = 5f;
                oneTwoPunch.skillButton.onClick.AddListener(() => ActivateSkill(oneTwoPunch));
            }

            // 박치기 기본 설정
            if (headbutt.skillButton != null)
            {
                headbutt.skillName = "박치기";
                headbutt.description = "강력한 단일 타격 (5배 데미지)";
                headbutt.cooldown = 10f;
                headbutt.effectMultiplier = 5f;
                headbutt.skillButton.onClick.AddListener(() => ActivateSkill(headbutt));
            }

            // 콤보러쉬 기본 설정
            if (comboRush.skillButton != null)
            {
                comboRush.skillName = "콤보러쉬";
                comboRush.description = "10초간 데미지 2배";
                comboRush.cooldown = 30f;
                comboRush.duration = 10f;
                comboRush.effectMultiplier = 2f;
                comboRush.skillButton.onClick.AddListener(() => ActivateSkill(comboRush));
            }

            // 골드러쉬 기본 설정
            if (goldRush.skillButton != null)
            {
                goldRush.skillName = "골드러쉬";
                goldRush.description = "10초간 골드 획득 3배";
                goldRush.cooldown = 60f;
                goldRush.duration = 10f;
                goldRush.effectMultiplier = 3f;
                goldRush.skillButton.onClick.AddListener(() => ActivateSkill(goldRush));
            }
        }

        /// <summary>
        /// 스킬 활성화
        /// </summary>
        public void ActivateSkill(Skill skill)
        {
            if (skill == null) return;

            // 쿨다운 중이면 사용 불가
            if (skill.currentCooldown > 0)
            {
                Debug.Log($"{skill.skillName}은(는) 아직 쿨다운 중입니다");
                return;
            }

            // 쿨다운 시작
            skill.currentCooldown = skill.cooldown;

            // 스킬 효과 실행
            if (skill == oneTwoPunch)
            {
                StartCoroutine(OneTwoPunchEffect());
            }
            else if (skill == headbutt)
            {
                StartCoroutine(HeadbuttEffect());
            }
            else if (skill == comboRush)
            {
                StartCoroutine(BuffSkill(comboRush));
            }
            else if (skill == goldRush)
            {
                StartCoroutine(BuffSkill(goldRush));
            }

            // 사운드 재생
            AudioManager.Instance?.PlaySound("SkillActivate");

            Debug.Log($"{skill.skillName} 활성화!");
        }

        /// <summary>
        /// 쿨다운 업데이트
        /// </summary>
        void UpdateCooldowns(float deltaTime)
        {
            UpdateSkillCooldown(oneTwoPunch, deltaTime);
            UpdateSkillCooldown(headbutt, deltaTime);
            UpdateSkillCooldown(comboRush, deltaTime);
            UpdateSkillCooldown(goldRush, deltaTime);
        }

        void UpdateSkillCooldown(Skill skill, float deltaTime)
        {
            if (skill == null) return;

            if (skill.currentCooldown > 0)
            {
                skill.currentCooldown -= deltaTime;

                // UI 업데이트
                if (skill.cooldownOverlay != null)
                {
                    float fillAmount = skill.currentCooldown / skill.cooldown;
                    skill.cooldownOverlay.fillAmount = fillAmount;
                }

                if (skill.cooldownText != null)
                {
                    skill.cooldownText.text = Mathf.Ceil(skill.currentCooldown).ToString();
                }

                if (skill.skillButton != null)
                {
                    skill.skillButton.interactable = false;
                }
            }
            else
            {
                // 쿨다운 완료
                if (skill.cooldownOverlay != null)
                    skill.cooldownOverlay.fillAmount = 0;

                if (skill.cooldownText != null)
                    skill.cooldownText.text = "";

                if (skill.skillButton != null)
                    skill.skillButton.interactable = true;
            }
        }

        /// <summary>
        /// 원투펀치 효과
        /// </summary>
        IEnumerator OneTwoPunchEffect()
        {
            Vector3 clickPos = GetClickPosition();

            // 첫 번째 펀치
            GameManager.Instance.OnSandbagClicked(clickPos);

            yield return new WaitForSeconds(0.15f);

            // 두 번째 펀치
            GameManager.Instance.OnSandbagClicked(clickPos);
        }

        /// <summary>
        /// 박치기 효과
        /// </summary>
        IEnumerator HeadbuttEffect()
        {
            BigNumber damage = GameManager.Instance.punchPower.Multiply(headbutt.effectMultiplier);

            // 모든 업그레이드 보너스 적용
            if (UpgradeManager.Instance != null)
            {
                damage = UpgradeManager.Instance.ApplyUpgradeBonuses(damage);
            }

            // 프레스티지 보너스
            if (PrestigeManager.Instance != null)
            {
                damage = PrestigeManager.Instance.ApplyPrestigeBonus(damage);
            }

            // 데미지 적용
            Vector3 clickPos = GetClickPosition();

            if (GameManager.Instance.currentStage % 10 == 0)
            {
                // 보스에게
                // (Boss 스크립트 참조 필요)
            }
            else
            {
                // 샌드백에게
                // (Sandbag 스크립트 참조 필요)
            }

            // UI 데미지 표시
            UIManager.Instance?.ShowDamagePopup(clickPos, damage, true);

            // 카메라 셰이크
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.5f, 0.7f);
            }

            yield return null;
        }

        /// <summary>
        /// 버프 스킬 (콤보러쉬, 골드러쉬)
        /// </summary>
        IEnumerator BuffSkill(Skill skill)
        {
            skill.isActive = true;
            float timer = skill.duration;

            // 버프 시작 UI 표시
            if (skill == comboRush)
            {
                UIManager.Instance?.ShowBuffNotification("콤보러쉬!", "데미지 2배!", skill.duration);
            }
            else if (skill == goldRush)
            {
                UIManager.Instance?.ShowBuffNotification("골드러쉬!", "골드 획득 3배!", skill.duration);
            }

            // 지속 시간 대기
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                yield return null;
            }

            skill.isActive = false;
        }

        /// <summary>
        /// 활성화된 버프를 데미지에 적용
        /// </summary>
        public BigNumber ApplyActiveBuffs(BigNumber baseDamage)
        {
            if (comboRush.isActive)
            {
                baseDamage = baseDamage.Multiply(comboRush.effectMultiplier);
            }

            return baseDamage;
        }

        /// <summary>
        /// 활성화된 버프를 골드에 적용
        /// </summary>
        public BigNumber ApplyGoldBuffs(BigNumber baseGold)
        {
            if (goldRush.isActive)
            {
                baseGold = baseGold.Multiply(goldRush.effectMultiplier);
            }

            return baseGold;
        }

        /// <summary>
        /// 스킬 쿨다운 배열 반환 (저장용)
        /// </summary>
        public float[] GetSkillCooldowns()
        {
            return new float[]
            {
                oneTwoPunch.currentCooldown,
                headbutt.currentCooldown,
                comboRush.currentCooldown,
                goldRush.currentCooldown
            };
        }

        /// <summary>
        /// 스킬 쿨다운 로드
        /// </summary>
        public void LoadSkillCooldowns(float[] cooldowns)
        {
            if (cooldowns == null || cooldowns.Length < 4) return;

            // 로드 시 쿨다운이 너무 오래 남았으면 0으로 리셋
            oneTwoPunch.currentCooldown = Mathf.Max(0, cooldowns[0]);
            headbutt.currentCooldown = Mathf.Max(0, cooldowns[1]);
            comboRush.currentCooldown = Mathf.Max(0, cooldowns[2]);
            goldRush.currentCooldown = Mathf.Max(0, cooldowns[3]);
        }

        /// <summary>
        /// 클릭 위치 계산 (샌드백 중앙)
        /// </summary>
        Vector3 GetClickPosition()
        {
            if (GameManager.Instance.sandbagSpawnPoint != null)
            {
                return GameManager.Instance.sandbagSpawnPoint.position;
            }

            return Vector3.zero;
        }

#if UNITY_EDITOR
        [ContextMenu("Reset All Cooldowns")]
        void EditorResetCooldowns()
        {
            oneTwoPunch.currentCooldown = 0;
            headbutt.currentCooldown = 0;
            comboRush.currentCooldown = 0;
            goldRush.currentCooldown = 0;
            Debug.Log("모든 스킬 쿨다운 리셋됨");
        }
#endif
    }
}
