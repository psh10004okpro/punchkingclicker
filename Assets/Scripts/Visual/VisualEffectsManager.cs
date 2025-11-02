using UnityEngine;
using System.Collections.Generic;

namespace PunchKing
{
    /// <summary>
    /// 비주얼 이펙트 매니저
    /// 파티클, 이펙트, 애니메이션 관리
    /// </summary>
    public class VisualEffectsManager : MonoBehaviour
    {
        public static VisualEffectsManager Instance { get; private set; }

        [Header("파티클 프리팹")]
        public ParticleSystem punchHitEffect;
        public ParticleSystem criticalHitEffect;
        public ParticleSystem goldRainEffect;
        public ParticleSystem levelUpEffect;
        public ParticleSystem prestigeEffect;
        public ParticleSystem skillActivateEffect;

        [Header("오브젝트 풀 설정")]
        public int poolSize = 20;

        private Dictionary<string, Queue<ParticleSystem>> particlePools = new Dictionary<string, Queue<ParticleSystem>>();
        private Transform poolParent;

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

            // 풀 부모 오브젝트 생성
            poolParent = new GameObject("ParticlePool").transform;
            poolParent.SetParent(transform);

            InitializePools();
        }

        /// <summary>
        /// 파티클 풀 초기화
        /// </summary>
        void InitializePools()
        {
            CreatePool("PunchHit", punchHitEffect);
            CreatePool("CriticalHit", criticalHitEffect);
            CreatePool("GoldRain", goldRainEffect);
            CreatePool("LevelUp", levelUpEffect);
            CreatePool("Prestige", prestigeEffect);
            CreatePool("SkillActivate", skillActivateEffect);
        }

        /// <summary>
        /// 파티클 풀 생성
        /// </summary>
        void CreatePool(string poolName, ParticleSystem prefab)
        {
            if (prefab == null)
            {
                // 프리팹이 없으면 런타임에 기본 파티클 생성
                prefab = CreateDefaultParticle(poolName);
            }

            particlePools[poolName] = new Queue<ParticleSystem>();

            for (int i = 0; i < poolSize; i++)
            {
                ParticleSystem particle = Instantiate(prefab, poolParent);
                particle.gameObject.SetActive(false);
                particlePools[poolName].Enqueue(particle);
            }
        }

        /// <summary>
        /// 기본 파티클 생성 (프리팹이 없을 때)
        /// </summary>
        ParticleSystem CreateDefaultParticle(string effectName)
        {
            GameObject obj = new GameObject($"Default_{effectName}");
            ParticleSystem ps = obj.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 0.5f;
            main.startLifetime = 0.5f;
            main.startSpeed = 5f;
            main.startSize = 0.3f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            // 이펙트 타입별 색상
            switch (effectName)
            {
                case "PunchHit":
                    main.startColor = new Color(1f, 0.8f, 0.3f); // 노란색
                    break;
                case "CriticalHit":
                    main.startColor = new Color(1f, 0.2f, 0.2f); // 빨간색
                    main.startSize = 0.5f;
                    break;
                case "GoldRain":
                    main.startColor = new Color(1f, 0.84f, 0f); // 골드
                    break;
                case "LevelUp":
                    main.startColor = new Color(0.3f, 0.8f, 1f); // 파란색
                    break;
                case "Prestige":
                    main.startColor = new Color(0.8f, 0.3f, 1f); // 보라색
                    break;
                case "SkillActivate":
                    main.startColor = new Color(0.3f, 1f, 0.3f); // 녹색
                    break;
            }

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 10, 20)
            });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;

            return ps;
        }

        /// <summary>
        /// 파티클 풀에서 가져오기
        /// </summary>
        ParticleSystem GetParticleFromPool(string poolName)
        {
            if (!particlePools.ContainsKey(poolName) || particlePools[poolName].Count == 0)
            {
                Debug.LogWarning($"[VisualEffects] 풀 '{poolName}'이(가) 비어있습니다.");
                return null;
            }

            ParticleSystem particle = particlePools[poolName].Dequeue();
            particle.gameObject.SetActive(true);
            return particle;
        }

        /// <summary>
        /// 파티클을 풀로 반환
        /// </summary>
        void ReturnParticleToPool(string poolName, ParticleSystem particle)
        {
            if (particle == null) return;

            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            particle.gameObject.SetActive(false);
            particle.transform.SetParent(poolParent);

            if (particlePools.ContainsKey(poolName))
            {
                particlePools[poolName].Enqueue(particle);
            }
        }

        /// <summary>
        /// 파티클 재생
        /// </summary>
        void PlayParticle(string poolName, Vector3 position, float duration = 2f)
        {
            ParticleSystem particle = GetParticleFromPool(poolName);
            if (particle == null) return;

            particle.transform.position = position;
            particle.Play();

            StartCoroutine(ReturnAfterDelay(poolName, particle, duration));
        }

        System.Collections.IEnumerator ReturnAfterDelay(string poolName, ParticleSystem particle, float delay)
        {
            yield return new WaitForSeconds(delay);
            ReturnParticleToPool(poolName, particle);
        }

        #region 공개 메서드

        /// <summary>
        /// 펀치 히트 이펙트
        /// </summary>
        public void PlayPunchHitEffect(Vector3 position)
        {
            PlayParticle("PunchHit", position, 0.5f);
        }

        /// <summary>
        /// 크리티컬 히트 이펙트
        /// </summary>
        public void PlayCriticalHitEffect(Vector3 position)
        {
            PlayParticle("CriticalHit", position, 1f);

            // 추가 화면 셰이크
            CameraShake.Instance?.Shake(0.3f, 0.2f);
        }

        /// <summary>
        /// 골드 획득 이펙트
        /// </summary>
        public void PlayGoldRainEffect(Vector3 position)
        {
            PlayParticle("GoldRain", position, 1.5f);
        }

        /// <summary>
        /// 레벨업 이펙트
        /// </summary>
        public void PlayLevelUpEffect(Vector3 position)
        {
            PlayParticle("LevelUp", position, 2f);
        }

        /// <summary>
        /// 프레스티지 이펙트
        /// </summary>
        public void PlayPrestigeEffect(Vector3 position)
        {
            PlayParticle("Prestige", position, 3f);
        }

        /// <summary>
        /// 스킬 활성화 이펙트
        /// </summary>
        public void PlaySkillActivateEffect(Vector3 position)
        {
            PlayParticle("SkillActivate", position, 1f);
        }

        /// <summary>
        /// 텍스트 플로팅 이펙트 (데미지/골드 표시)
        /// </summary>
        public void ShowFloatingText(string text, Vector3 position, Color color, float fontSize = 24f)
        {
            // DamagePopup을 사용하거나 새로운 FloatingText 컴포넌트 사용
            if (UIManager.Instance != null)
            {
                // UIManager에 FloatingText 기능이 있다면 사용
                // 없으면 간단한 텍스트 표시
                Debug.Log($"[FloatingText] {text} at {position}");
            }
        }

        #endregion

#if UNITY_EDITOR
        [ContextMenu("Test Punch Effect")]
        void TestPunchEffect()
        {
            PlayPunchHitEffect(Vector3.zero);
        }

        [ContextMenu("Test Critical Effect")]
        void TestCriticalEffect()
        {
            PlayCriticalHitEffect(Vector3.zero);
        }

        [ContextMenu("Test Gold Rain")]
        void TestGoldRain()
        {
            PlayGoldRainEffect(Vector3.zero);
        }
#endif
    }
}
