using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace PunchKing
{
    /// <summary>
    /// 게임의 메인 매니저
    /// 게임 상태, 진행 흐름, 클릭 처리 등을 관리
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("게임 상태")]
        public int currentStage = 1;
        public BigNumber totalGold = new BigNumber(0);
        public BigNumber goldPerSecond = new BigNumber(0);

        [Header("플레이어 스탯")]
        public BigNumber punchPower = new BigNumber(1);
        public float critChance = 0.05f;  // 5% 크리티컬 확률
        public float critMultiplier = 2.0f;  // 2배 크리티컬 데미지
        public int clickCount = 0;
        public int totalKillCount = 0;

        [Header("레퍼런스")]
        public Transform sandbagSpawnPoint;
        public GameObject sandbagPrefab;
        public GameObject bossPrefab;
        public ParticleSystem hitParticles;
        public ParticleSystem criticalHitParticles;

        [Header("이벤트")]
        public UnityEvent<BigNumber> OnGoldChanged;
        public UnityEvent<int> OnStageChanged;
        public UnityEvent OnPunch;
        public UnityEvent<BigNumber, bool> OnDamageDealt;  // 데미지, 크리티컬 여부

        [Header("게임 설정")]
        [SerializeField] private float autoSaveInterval = 30f;  // 30초마다 자동 저장
        [SerializeField] private int bossStageInterval = 10;  // 10스테이지마다 보스

        private GameObject currentEnemy;
        private Sandbag currentSandbag;
        private Boss currentBoss;
        private Coroutine autoSaveCoroutine;
        private Coroutine goldGenerationCoroutine;

        void Awake()
        {
            // 싱글톤 패턴
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

            // 모바일 최적화 설정
            Application.targetFrameRate = 60;
            Input.multiTouchEnabled = true;
            QualitySettings.vSyncCount = 0;  // VSync 끄고 수동으로 60fps 제어

            InitializeEvents();
        }

        void Start()
        {
            InitializeGame();
        }

        void InitializeEvents()
        {
            if (OnGoldChanged == null) OnGoldChanged = new UnityEvent<BigNumber>();
            if (OnStageChanged == null) OnStageChanged = new UnityEvent<int>();
            if (OnPunch == null) OnPunch = new UnityEvent();
            if (OnDamageDealt == null) OnDamageDealt = new UnityEvent<BigNumber, bool>();
        }

        void InitializeGame()
        {
            // 저장된 데이터 로드
            bool loadSuccess = SaveManager.Instance.LoadGame();

            if (!loadSuccess)
            {
                // 새 게임 시작
                ResetGameToDefault();
            }

            // 첫 샌드백/보스 생성
            SpawnEnemy();

            // 자동 골드 생성 시작
            if (goldGenerationCoroutine != null)
                StopCoroutine(goldGenerationCoroutine);
            goldGenerationCoroutine = StartCoroutine(AutoGoldGeneration());

            // 자동 저장 시작
            if (autoSaveCoroutine != null)
                StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = StartCoroutine(AutoSave());

            // Unity 6 GPU Resident Drawer 최적화
#if UNITY_6_0_OR_NEWER
            QualitySettings.enableGPUResidentDrawer = true;
#endif

            // UI 업데이트
            OnGoldChanged?.Invoke(totalGold);
            OnStageChanged?.Invoke(currentStage);
        }

        void ResetGameToDefault()
        {
            currentStage = 1;
            totalGold = new BigNumber(0);
            goldPerSecond = new BigNumber(0);
            punchPower = new BigNumber(1);
            critChance = 0.05f;
            critMultiplier = 2.0f;
            clickCount = 0;
            totalKillCount = 0;
        }

        /// <summary>
        /// 샌드백이 클릭되었을 때 호출
        /// </summary>
        public void OnSandbagClicked(Vector3 clickPosition)
        {
            if (currentEnemy == null) return;

            // 데미지 계산
            BigNumber damage = CalculateDamage();
            bool isCritical = UnityEngine.Random.value < critChance;

            if (isCritical)
            {
                damage = damage.Multiply(critMultiplier);
                ShowCriticalEffect(clickPosition);
            }
            else
            {
                ShowHitEffect(clickPosition);
            }

            // 데미지 적용
            if (currentSandbag != null)
            {
                currentSandbag.TakeDamage(damage);
            }
            else if (currentBoss != null)
            {
                currentBoss.TakeDamage(damage);
            }

            // UI 업데이트
            UIManager.Instance?.ShowDamagePopup(clickPosition, damage, isCritical);

            // 클릭 카운트 증가
            clickCount++;

            // 이벤트 발생
            OnPunch?.Invoke();
            OnDamageDealt?.Invoke(damage, isCritical);

            // 햅틱 피드백 (모바일)
#if UNITY_IOS || UNITY_ANDROID
            if (isCritical)
            {
                Handheld.Vibrate();  // 크리티컬은 진동 강하게
            }
#endif
        }

        /// <summary>
        /// 데미지 계산 (모든 보너스 적용)
        /// </summary>
        BigNumber CalculateDamage()
        {
            BigNumber baseDamage = punchPower.Clone();

            // 업그레이드 보너스 적용
            if (UpgradeManager.Instance != null)
            {
                baseDamage = UpgradeManager.Instance.ApplyUpgradeBonuses(baseDamage);
            }

            // 프레스티지 보너스
            if (PrestigeManager.Instance != null)
            {
                baseDamage = PrestigeManager.Instance.ApplyPrestigeBonus(baseDamage);
            }

            // 스킬 버프 적용
            if (SkillManager.Instance != null)
            {
                baseDamage = SkillManager.Instance.ApplyActiveBuffs(baseDamage);
            }

            // 코치 보너스
            if (CoachSystem.Instance != null)
            {
                baseDamage = CoachSystem.Instance.ApplyCoachBonus(baseDamage);
            }

            return baseDamage;
        }

        /// <summary>
        /// 샌드백/보스가 파괴되었을 때 호출
        /// </summary>
        public void OnEnemyDestroyed()
        {
            // 스테이지 보상 지급
            BigNumber reward = CalculateStageReward();
            AddGold(reward);

            // 킬 카운트 증가
            totalKillCount++;

            // 다음 스테이지로
            currentStage++;
            OnStageChanged?.Invoke(currentStage);

            // 카메라 셰이크
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.2f, 0.3f);
            }

            // 다음 적 생성
            SpawnEnemy();
        }

        /// <summary>
        /// 적 생성 (샌드백 또는 보스)
        /// </summary>
        void SpawnEnemy()
        {
            // 기존 적 제거
            if (currentEnemy != null)
                Destroy(currentEnemy);

            currentSandbag = null;
            currentBoss = null;

            // 보스 스테이지인지 확인
            bool isBossStage = (currentStage % bossStageInterval == 0);

            if (isBossStage && bossPrefab != null)
            {
                // 보스 생성
                currentEnemy = Instantiate(bossPrefab, sandbagSpawnPoint);
                currentBoss = currentEnemy.GetComponent<Boss>();

                if (currentBoss != null)
                {
                    BigNumber bossHealth = CalculateBossHealth();
                    currentBoss.Initialize(bossHealth, currentStage);
                }
            }
            else
            {
                // 샌드백 생성
                currentEnemy = Instantiate(sandbagPrefab, sandbagSpawnPoint);
                currentSandbag = currentEnemy.GetComponent<Sandbag>();

                if (currentSandbag != null)
                {
                    BigNumber sandbagHealth = CalculateSandbagHealth();
                    currentSandbag.Initialize(sandbagHealth, currentStage);
                }
            }
        }

        /// <summary>
        /// 샌드백 체력 계산
        /// </summary>
        BigNumber CalculateSandbagHealth()
        {
            // 기본 체력: 100
            // 지수적 증가: 100 * 1.15^(stage-1)
            BigNumber baseHealth = new BigNumber(100);
            double multiplier = Mathf.Pow(1.15f, currentStage - 1);
            return baseHealth.Multiply(multiplier);
        }

        /// <summary>
        /// 보스 체력 계산 (샌드백의 10배)
        /// </summary>
        BigNumber CalculateBossHealth()
        {
            return CalculateSandbagHealth().Multiply(10);
        }

        /// <summary>
        /// 스테이지 보상 골드 계산
        /// </summary>
        BigNumber CalculateStageReward()
        {
            // 기본 보상: 10골드
            // 스테이지에 따라 증가: 10 * 1.1^(stage-1)
            BigNumber baseReward = new BigNumber(10);
            double multiplier = Mathf.Pow(1.1f, currentStage - 1);
            BigNumber reward = baseReward.Multiply(multiplier);

            // 보스는 5배 보상
            if (currentStage % bossStageInterval == 0)
            {
                reward = reward.Multiply(5);
            }

            // 스킬 골드 버프 적용
            if (SkillManager.Instance != null)
            {
                reward = SkillManager.Instance.ApplyGoldBuffs(reward);
            }

            return reward;
        }

        /// <summary>
        /// 골드 추가
        /// </summary>
        public void AddGold(BigNumber amount)
        {
            totalGold = totalGold.Add(amount);
            OnGoldChanged?.Invoke(totalGold);
        }

        /// <summary>
        /// 골드 사용 (업그레이드 등)
        /// </summary>
        public bool SpendGold(BigNumber amount)
        {
            if (totalGold.IsGreaterThanOrEqual(amount))
            {
                totalGold = totalGold.Subtract(amount);
                OnGoldChanged?.Invoke(totalGold);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 초당 골드 생성 업데이트
        /// </summary>
        public void UpdateGoldPerSecond()
        {
            goldPerSecond = new BigNumber(0);

            if (UpgradeManager.Instance != null)
            {
                goldPerSecond = UpgradeManager.Instance.GetAutoClickerGoldPerSecond();
            }
        }

        /// <summary>
        /// 자동 골드 생성 코루틴
        /// </summary>
        IEnumerator AutoGoldGeneration()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                if (!goldPerSecond.IsZero())
                {
                    AddGold(goldPerSecond);
                }
            }
        }

        /// <summary>
        /// 자동 저장 코루틴
        /// </summary>
        IEnumerator AutoSave()
        {
            while (true)
            {
                yield return new WaitForSeconds(autoSaveInterval);
                SaveManager.Instance?.SaveGame();
                Debug.Log("게임 자동 저장됨");
            }
        }

        /// <summary>
        /// 일반 타격 이펙트
        /// </summary>
        void ShowHitEffect(Vector3 position)
        {
            if (hitParticles != null)
            {
                ParticleSystem particles = Instantiate(hitParticles, position, Quaternion.identity);
                Destroy(particles.gameObject, 2f);
            }
        }

        /// <summary>
        /// 크리티컬 타격 이펙트
        /// </summary>
        void ShowCriticalEffect(Vector3 position)
        {
            if (criticalHitParticles != null)
            {
                ParticleSystem particles = Instantiate(criticalHitParticles, position, Quaternion.identity);
                Destroy(particles.gameObject, 2f);
            }

            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.Shake(0.15f, 0.2f);
            }
        }

        /// <summary>
        /// 게임 종료 시
        /// </summary>
        void OnApplicationQuit()
        {
            SaveManager.Instance?.SaveGame();
        }

        /// <summary>
        /// 게임이 백그라운드로 갈 때
        /// </summary>
        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                // 백그라운드로 갈 때 저장
                SaveManager.Instance?.SaveGame();
            }
        }

        /// <summary>
        /// 게임 리셋 (프레스티지용)
        /// </summary>
        public void ResetGameForPrestige()
        {
            // 스테이지와 골드만 리셋
            currentStage = 1;
            totalGold = new BigNumber(0);
            clickCount = 0;

            // 업그레이드 리셋
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.ResetUpgrades();
            }

            // 처음 샌드백 생성
            SpawnEnemy();

            // UI 업데이트
            OnGoldChanged?.Invoke(totalGold);
            OnStageChanged?.Invoke(currentStage);
        }

        /// <summary>
        /// DPS 계산 (디버그/통계용)
        /// </summary>
        public BigNumber GetCurrentDPS()
        {
            // 클릭 DPS는 계산하기 어려우므로 골드/초만 반환
            return goldPerSecond;
        }

#if UNITY_EDITOR
        // 에디터 디버그 기능
        [ContextMenu("Add 1000 Gold")]
        void DebugAddGold()
        {
            AddGold(new BigNumber(1000));
        }

        [ContextMenu("Skip 10 Stages")]
        void DebugSkipStages()
        {
            currentStage += 10;
            OnStageChanged?.Invoke(currentStage);
            SpawnEnemy();
        }
#endif
    }
}
