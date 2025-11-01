using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace PunchKing
{
    /// <summary>
    /// 업그레이드 시스템 관리
    /// 업그레이드 구매, 레벨 관리, 보너스 계산
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }

        [Header("업그레이드 목록")]
        [Tooltip("게임에 사용될 모든 업그레이드 데이터")]
        public List<UpgradeData> allUpgrades = new List<UpgradeData>();

        [Header("이벤트")]
        public UnityEvent<UpgradeData, int> OnUpgradePurchased;  // 업그레이드, 새 레벨

        // 각 업그레이드의 현재 레벨 저장
        private Dictionary<UpgradeData, int> upgradeLevels = new Dictionary<UpgradeData, int>();

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

            InitializeUpgrades();

            if (OnUpgradePurchased == null)
                OnUpgradePurchased = new UnityEvent<UpgradeData, int>();
        }

        /// <summary>
        /// 업그레이드 초기화
        /// </summary>
        void InitializeUpgrades()
        {
            upgradeLevels.Clear();

            foreach (var upgrade in allUpgrades)
            {
                if (upgrade != null)
                {
                    upgradeLevels[upgrade] = 0;
                }
            }
        }

        /// <summary>
        /// 업그레이드 구매
        /// </summary>
        public bool PurchaseUpgrade(UpgradeData upgrade)
        {
            if (upgrade == null)
            {
                Debug.LogError("업그레이드 데이터가 null입니다");
                return false;
            }

            // 레벨 확인
            if (!upgradeLevels.ContainsKey(upgrade))
            {
                upgradeLevels[upgrade] = 0;
            }

            int currentLevel = upgradeLevels[upgrade];

            // 최대 레벨 확인
            if (upgrade.IsMaxLevel(currentLevel))
            {
                Debug.Log($"{upgrade.upgradeName}은(는) 이미 최대 레벨입니다");
                return false;
            }

            // 잠금 확인
            if (!upgrade.IsUnlocked(GameManager.Instance.currentStage,
                PrestigeManager.Instance?.prestigeCount ?? 0))
            {
                Debug.Log($"{upgrade.upgradeName}은(는) 아직 잠겨있습니다");
                return false;
            }

            // 비용 확인 및 지불
            BigNumber cost = upgrade.GetCost(currentLevel);
            if (!GameManager.Instance.SpendGold(cost))
            {
                Debug.Log("골드가 부족합니다");
                return false;
            }

            // 레벨 증가
            upgradeLevels[upgrade]++;
            int newLevel = upgradeLevels[upgrade];

            // 업그레이드 효과 적용
            ApplyUpgradeEffect(upgrade, newLevel);

            // 이벤트 발생
            OnUpgradePurchased?.Invoke(upgrade, newLevel);

            // 사운드 재생
            AudioManager.Instance?.PlaySound("UpgradePurchase");

            Debug.Log($"{upgrade.upgradeName} 레벨 {newLevel} 구매 완료!");

            return true;
        }

        /// <summary>
        /// 업그레이드 효과 적용
        /// </summary>
        void ApplyUpgradeEffect(UpgradeData upgrade, int newLevel)
        {
            float effect = upgrade.GetEffect(newLevel);

            switch (upgrade.type)
            {
                case UpgradeData.UpgradeType.PunchPower:
                    // 펀치력 증가는 ApplyUpgradeBonuses에서 계산됨
                    break;

                case UpgradeData.UpgradeType.AutoClicker:
                    // 자동 클릭 = 초당 골드 생성
                    GameManager.Instance.UpdateGoldPerSecond();
                    break;

                case UpgradeData.UpgradeType.CriticalChance:
                    // 크리티컬 확률 증가 (최대 75%)
                    GameManager.Instance.critChance = Mathf.Min(0.75f, effect / 100f);
                    break;

                case UpgradeData.UpgradeType.CriticalDamage:
                    // 크리티컬 데미지 배율
                    GameManager.Instance.critMultiplier = effect;
                    break;

                case UpgradeData.UpgradeType.GoldMultiplier:
                    // 골드 배수는 스테이지 보상 계산 시 적용
                    break;

                case UpgradeData.UpgradeType.OfflineEarnings:
                    // 오프라인 수익은 SaveManager에서 처리
                    break;
            }
        }

        /// <summary>
        /// 모든 업그레이드 보너스를 데미지에 적용
        /// </summary>
        public BigNumber ApplyUpgradeBonuses(BigNumber baseDamage)
        {
            BigNumber totalDamage = baseDamage.Clone();
            float totalMultiplier = 1f;

            foreach (var kvp in upgradeLevels)
            {
                UpgradeData upgrade = kvp.Key;
                int level = kvp.Value;

                if (level == 0) continue;

                float effect = upgrade.GetEffect(level);

                switch (upgrade.type)
                {
                    case UpgradeData.UpgradeType.PunchPower:
                        // 덧셈 방식
                        if (upgrade.effectType == UpgradeData.EffectCalculationType.Additive)
                        {
                            totalDamage = totalDamage.Add(new BigNumber(effect));
                        }
                        // 곱셈 방식
                        else
                        {
                            totalMultiplier *= effect;
                        }
                        break;

                    case UpgradeData.UpgradeType.ClickDamage:
                        totalMultiplier *= effect;
                        break;
                }
            }

            // 곱셈 보너스 적용
            if (totalMultiplier > 1f)
            {
                totalDamage = totalDamage.Multiply(totalMultiplier);
            }

            return totalDamage;
        }

        /// <summary>
        /// 자동 클릭 초당 골드 계산
        /// </summary>
        public BigNumber GetAutoClickerGoldPerSecond()
        {
            BigNumber goldPerSecond = new BigNumber(0);

            foreach (var kvp in upgradeLevels)
            {
                UpgradeData upgrade = kvp.Key;
                int level = kvp.Value;

                if (level == 0) continue;

                if (upgrade.type == UpgradeData.UpgradeType.AutoClicker)
                {
                    float effect = upgrade.GetEffect(level);
                    goldPerSecond = goldPerSecond.Add(new BigNumber(effect));
                }
            }

            // 골드 배수 적용
            float goldMultiplier = GetGoldMultiplier();
            if (goldMultiplier > 1f)
            {
                goldPerSecond = goldPerSecond.Multiply(goldMultiplier);
            }

            return goldPerSecond;
        }

        /// <summary>
        /// 골드 배수 계산
        /// </summary>
        public float GetGoldMultiplier()
        {
            float multiplier = 1f;

            foreach (var kvp in upgradeLevels)
            {
                UpgradeData upgrade = kvp.Key;
                int level = kvp.Value;

                if (level == 0) continue;

                if (upgrade.type == UpgradeData.UpgradeType.GoldMultiplier)
                {
                    float effect = upgrade.GetEffect(level);
                    multiplier *= effect;
                }
            }

            return multiplier;
        }

        /// <summary>
        /// 오프라인 수익 배수 계산
        /// </summary>
        public float GetOfflineEarningsMultiplier()
        {
            float multiplier = 1f;

            foreach (var kvp in upgradeLevels)
            {
                UpgradeData upgrade = kvp.Key;
                int level = kvp.Value;

                if (level == 0) continue;

                if (upgrade.type == UpgradeData.UpgradeType.OfflineEarnings)
                {
                    float effect = upgrade.GetEffect(level);
                    multiplier *= effect;
                }
            }

            return multiplier;
        }

        /// <summary>
        /// 특정 업그레이드의 레벨 가져오기
        /// </summary>
        public int GetUpgradeLevel(UpgradeData upgrade)
        {
            if (upgrade == null) return 0;

            if (upgradeLevels.ContainsKey(upgrade))
                return upgradeLevels[upgrade];

            return 0;
        }

        /// <summary>
        /// 모든 업그레이드 레벨 배열로 반환 (저장용)
        /// </summary>
        public int[] GetAllUpgradeLevels()
        {
            int[] levels = new int[allUpgrades.Count];

            for (int i = 0; i < allUpgrades.Count; i++)
            {
                if (allUpgrades[i] != null && upgradeLevels.ContainsKey(allUpgrades[i]))
                {
                    levels[i] = upgradeLevels[allUpgrades[i]];
                }
                else
                {
                    levels[i] = 0;
                }
            }

            return levels;
        }

        /// <summary>
        /// 저장된 레벨 데이터 로드
        /// </summary>
        public void LoadUpgradeLevels(int[] levels)
        {
            if (levels == null || levels.Length != allUpgrades.Count)
            {
                Debug.LogWarning("업그레이드 레벨 데이터가 일치하지 않습니다");
                return;
            }

            for (int i = 0; i < allUpgrades.Count; i++)
            {
                if (allUpgrades[i] != null)
                {
                    upgradeLevels[allUpgrades[i]] = levels[i];

                    // 효과 재적용
                    if (levels[i] > 0)
                    {
                        ApplyUpgradeEffect(allUpgrades[i], levels[i]);
                    }
                }
            }

            Debug.Log("업그레이드 레벨 로드 완료");
        }

        /// <summary>
        /// 업그레이드 리셋 (프레스티지용)
        /// </summary>
        public void ResetUpgrades()
        {
            foreach (var upgrade in allUpgrades)
            {
                if (upgrade != null)
                {
                    upgradeLevels[upgrade] = 0;
                }
            }

            // 게임 스탯 초기화
            if (GameManager.Instance != null)
            {
                GameManager.Instance.punchPower = new BigNumber(1);
                GameManager.Instance.critChance = 0.05f;
                GameManager.Instance.critMultiplier = 2.0f;
                GameManager.Instance.goldPerSecond = new BigNumber(0);
            }

            Debug.Log("모든 업그레이드 리셋됨");
        }

        /// <summary>
        /// 특정 타입의 업그레이드 목록 가져오기
        /// </summary>
        public List<UpgradeData> GetUpgradesByType(UpgradeData.UpgradeType type)
        {
            List<UpgradeData> result = new List<UpgradeData>();

            foreach (var upgrade in allUpgrades)
            {
                if (upgrade != null && upgrade.type == type)
                {
                    result.Add(upgrade);
                }
            }

            return result;
        }

        /// <summary>
        /// 잠금 해제된 업그레이드 목록
        /// </summary>
        public List<UpgradeData> GetUnlockedUpgrades()
        {
            List<UpgradeData> result = new List<UpgradeData>();
            int currentStage = GameManager.Instance?.currentStage ?? 1;
            int prestigeCount = PrestigeManager.Instance?.prestigeCount ?? 0;

            foreach (var upgrade in allUpgrades)
            {
                if (upgrade != null && upgrade.IsUnlocked(currentStage, prestigeCount))
                {
                    result.Add(upgrade);
                }
            }

            return result;
        }

#if UNITY_EDITOR
        [ContextMenu("Reset All Upgrades")]
        void EditorResetUpgrades()
        {
            ResetUpgrades();
            Debug.Log("모든 업그레이드가 리셋되었습니다");
        }

        [ContextMenu("Max All Upgrades")]
        void EditorMaxUpgrades()
        {
            foreach (var upgrade in allUpgrades)
            {
                if (upgrade != null)
                {
                    int maxLevel = upgrade.maxLevel > 0 ? upgrade.maxLevel : 100;
                    upgradeLevels[upgrade] = maxLevel;
                    ApplyUpgradeEffect(upgrade, maxLevel);
                }
            }
            Debug.Log("모든 업그레이드가 최대 레벨로 설정되었습니다");
        }
#endif
    }
}
