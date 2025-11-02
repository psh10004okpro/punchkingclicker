using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;

namespace PunchKing.Editor
{
    /// <summary>
    /// ScriptableObject 데이터 자동 생성기
    /// 게임에 필요한 모든 데이터를 한 번에 생성
    /// </summary>
    public class DataGenerator : MonoBehaviour
    {
        private const string DATA_PATH = "Assets/Data";
        private const string UPGRADES_PATH = "Assets/Data/Upgrades";
        private const string ACHIEVEMENTS_PATH = "Assets/Data/Achievements";
        private const string SHOP_PATH = "Assets/Data/Shop";

        [MenuItem("PunchKing/Generate All Data")]
        public static void GenerateAllData()
        {
            Debug.Log("[DataGenerator] 모든 데이터 생성 시작...");

            CreateDirectories();
            GenerateUpgrades();
            GenerateAchievements();
            GenerateShopItems();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[DataGenerator] ✅ 모든 데이터 생성 완료!");
            EditorUtility.DisplayDialog("완료", "모든 ScriptableObject 데이터가 생성되었습니다!\n\n" +
                $"- 업그레이드: 8개\n" +
                $"- 업적: 12개 (각 5티어)\n" +
                $"- 상점 아이템: 20개", "확인");
        }

        /// <summary>
        /// 디렉토리 생성
        /// </summary>
        static void CreateDirectories()
        {
            if (!AssetDatabase.IsValidFolder(DATA_PATH))
                AssetDatabase.CreateFolder("Assets", "Data");

            if (!AssetDatabase.IsValidFolder(UPGRADES_PATH))
                AssetDatabase.CreateFolder("Assets/Data", "Upgrades");

            if (!AssetDatabase.IsValidFolder(ACHIEVEMENTS_PATH))
                AssetDatabase.CreateFolder("Assets/Data", "Achievements");

            if (!AssetDatabase.IsValidFolder(SHOP_PATH))
                AssetDatabase.CreateFolder("Assets/Data", "Shop");
        }

        #region 업그레이드 생성

        [MenuItem("PunchKing/Generate Upgrades Only")]
        public static void GenerateUpgrades()
        {
            Debug.Log("[DataGenerator] 업그레이드 데이터 생성 중...");

            CreateUpgrade("PunchPower", "펀치 파워", "클릭 데미지가 증가합니다.", UpgradeData.UpgradeType.PunchPower, 100, 1.15f, 2f);
            CreateUpgrade("AutoClicker", "자동 클릭", "초당 자동으로 클릭합니다.", UpgradeData.UpgradeType.AutoClicker, 500, 1.2f, 1f);
            CreateUpgrade("CriticalChance", "크리티컬 확률", "크리티컬 확률이 증가합니다.", UpgradeData.UpgradeType.CriticalChance, 1000, 1.25f, 0.01f);
            CreateUpgrade("CriticalDamage", "크리티컬 데미지", "크리티컬 데미지 배율이 증가합니다.", UpgradeData.UpgradeType.CriticalDamage, 2000, 1.3f, 0.5f);
            CreateUpgrade("GoldMultiplier", "골드 배율", "획득하는 골드가 증가합니다.", UpgradeData.UpgradeType.GoldMultiplier, 1500, 1.25f, 0.1f);
            CreateUpgrade("OfflineEarnings", "오프라인 수익", "오프라인 중에도 골드를 획득합니다.", UpgradeData.UpgradeType.OfflineEarnings, 5000, 1.35f, 0.05f);
            CreateUpgrade("ClickDamage", "클릭 데미지", "직접 클릭 시 추가 데미지를 줍니다.", UpgradeData.UpgradeType.ClickDamage, 800, 1.18f, 1.5f);
            CreateUpgrade("GoldPerClick", "클릭당 골드", "클릭 시 추가 골드를 획득합니다.", UpgradeData.UpgradeType.GoldPerClick, 3000, 1.28f, 5f);

            Debug.Log("[DataGenerator] ✅ 업그레이드 8개 생성 완료");
        }

        static void CreateUpgrade(string id, string name, string description, UpgradeData.UpgradeType type,
            long baseCost, float costMultiplier, float effectPerLevel)
        {
            string path = $"{UPGRADES_PATH}/{id}.asset";

            UpgradeData upgrade = AssetDatabase.LoadAssetAtPath<UpgradeData>(path);
            if (upgrade == null)
            {
                upgrade = ScriptableObject.CreateInstance<UpgradeData>();
                AssetDatabase.CreateAsset(upgrade, path);
            }

            upgrade.upgradeID = id;
            upgrade.upgradeName = name;
            upgrade.description = description;
            upgrade.upgradeType = type;
            upgrade.baseCost = baseCost;
            upgrade.costMultiplier = costMultiplier;
            upgrade.effectPerLevel = effectPerLevel;
            upgrade.maxLevel = 0; // 무제한

            EditorUtility.SetDirty(upgrade);
        }

        #endregion

        #region 업적 생성

        [MenuItem("PunchKing/Generate Achievements Only")]
        public static void GenerateAchievements()
        {
            Debug.Log("[DataGenerator] 업적 데이터 생성 중...");

            // 1. 총 클릭 수
            CreateAchievement("TotalClicks", "클릭 마스터", "총 N번 클릭 달성",
                AchievementData.AchievementType.TotalClicks,
                new long[] { 100, 1000, 10000, 100000, 1000000 },
                new AchievementReward[] {
                    new AchievementReward(AchievementData.RewardType.Gold, 1000),
                    new AchievementReward(AchievementData.RewardType.Gold, 10000),
                    new AchievementReward(AchievementData.RewardType.Gold, 100000),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 10),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 50)
                });

            // 2. 총 골드
            CreateAchievement("TotalGold", "골드 수집가", "총 N골드 획득",
                AchievementData.AchievementType.TotalGold,
                new long[] { 10000, 1000000, 100000000, 10000000000, 1000000000000 },
                new AchievementReward[] {
                    new AchievementReward(AchievementData.RewardType.Gold, 5000),
                    new AchievementReward(AchievementData.RewardType.Gold, 50000),
                    new AchievementReward(AchievementData.RewardType.GoldMultiplier, 0.1),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 20),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 100)
                });

            // 3. 스테이지 도달
            CreateAchievement("ReachStage", "스테이지 정복자", "스테이지 N 도달",
                AchievementData.AchievementType.ReachStage,
                new long[] { 10, 50, 100, 250, 500 },
                new AchievementReward[] {
                    new AchievementReward(AchievementData.RewardType.Gold, 5000),
                    new AchievementReward(AchievementData.RewardType.Gold, 50000),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 15),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 30),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 75)
                });

            // 4. 보스 처치
            CreateAchievement("DefeatBosses", "보스 헌터", "보스 N마리 처치",
                AchievementData.AchievementType.DefeatBosses,
                new long[] { 5, 25, 50, 100, 250 },
                new AchievementReward[] {
                    new AchievementReward(AchievementData.RewardType.Gold, 10000),
                    new AchievementReward(AchievementData.RewardType.DamageMultiplier, 0.1),
                    new AchievementReward(AchievementData.RewardType.DamageMultiplier, 0.2),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 25),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 80)
                });

            // 5. 프레스티지
            CreateAchievement("PrestigeCount", "환생의 달인", "N번 프레스티지",
                AchievementData.AchievementType.PrestigeCount,
                new long[] { 1, 5, 10, 25, 50 },
                new AchievementReward[] {
                    new AchievementReward(AchievementData.RewardType.Gold, 100000),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 30),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 60),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 120),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 250)
                });

            // 6. 업그레이드 레벨
            CreateAchievement("UpgradeLevel", "업그레이드 마니아", "업그레이드 총 N레벨 달성",
                AchievementData.AchievementType.UpgradeLevel,
                new long[] { 50, 200, 500, 1000, 2500 },
                new AchievementReward[] {
                    new AchievementReward(AchievementData.RewardType.Gold, 50000),
                    new AchievementReward(AchievementData.RewardType.Gold, 500000),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 20),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 50),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 150)
                });

            // 7. 스킬 사용
            CreateAchievement("UseSkills", "스킬 마스터", "스킬 N번 사용",
                AchievementData.AchievementType.UseSkills,
                new long[] { 10, 50, 200, 500, 1000 },
                new AchievementReward[] {
                    new AchievementReward(AchievementData.RewardType.Gold, 20000),
                    new AchievementReward(AchievementData.RewardType.Gold, 100000),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 15),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 40),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 100)
                });

            // 8. 크리티컬 히트
            CreateAchievement("CriticalHits", "크리티컬 스나이퍼", "크리티컬 N번 발동",
                AchievementData.AchievementType.CriticalHits,
                new long[] { 100, 500, 2000, 5000, 10000 },
                new AchievementReward[] {
                    new AchievementReward(AchievementData.RewardType.CritChance, 0.01),
                    new AchievementReward(AchievementData.RewardType.CritChance, 0.02),
                    new AchievementReward(AchievementData.RewardType.CritChance, 0.03),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 25),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 70)
                });

            // 9. 코치 해금
            CreateAchievement("UnlockCoaches", "코치 컬렉터", "코치 N명 해금",
                AchievementData.AchievementType.UnlockCoaches,
                new long[] { 1, 2, 3, 4, 5 },
                new AchievementReward[] {
                    new AchievementReward(AchievementData.RewardType.Gold, 50000),
                    new AchievementReward(AchievementData.RewardType.Gold, 100000),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 20),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 50),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 100)
                });

            // 10. 플레이 타임 (시간 단위 -> 초 단위로 변환)
            CreateAchievement("PlayTime", "시간의 수호자", "N시간 플레이",
                AchievementData.AchievementType.PlayTime,
                new long[] { 3600, 18000, 36000, 86400, 172800 }, // 1h, 5h, 10h, 24h, 48h
                new AchievementReward[] {
                    new AchievementReward(AchievementData.RewardType.Gold, 30000),
                    new AchievementReward(AchievementData.RewardType.Gold, 200000),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 25),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 60),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 150)
                });

            // 11. 일일 로그인
            CreateAchievement("DailyLogins", "출석왕", "N일 연속 로그인",
                AchievementData.AchievementType.DailyLogins,
                new long[] { 3, 7, 14, 30, 100 },
                new AchievementReward[] {
                    new AchievementReward(AchievementData.RewardType.Gold, 50000),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 20),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 50),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 100),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 300)
                });

            // 12. 퀘스트 완료
            CreateAchievement("CompleteQuests", "퀘스트 달인", "퀘스트 N개 완료",
                AchievementData.AchievementType.CompleteQuests,
                new long[] { 10, 50, 100, 250, 500 },
                new AchievementReward[] {
                    new AchievementReward(AchievementData.RewardType.Gold, 40000),
                    new AchievementReward(AchievementData.RewardType.Gold, 250000),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 30),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 75),
                    new AchievementReward(AchievementData.RewardType.PrestigeCurrency, 200)
                });

            Debug.Log("[DataGenerator] ✅ 업적 12개 (각 5티어) 생성 완료");
        }

        static void CreateAchievement(string id, string name, string description,
            AchievementData.AchievementType type, long[] targets, AchievementReward[] rewards)
        {
            // 각 티어마다 별도의 ScriptableObject 생성
            for (int i = 0; i < targets.Length; i++)
            {
                string path = $"{ACHIEVEMENTS_PATH}/{id}_Tier{i + 1}.asset";

                AchievementData achievement = AssetDatabase.LoadAssetAtPath<AchievementData>(path);
                if (achievement == null)
                {
                    achievement = ScriptableObject.CreateInstance<AchievementData>();
                    AssetDatabase.CreateAsset(achievement, path);
                }

                achievement.achievementId = $"{id}_Tier{i + 1}";
                achievement.achievementName = $"{name} {i + 1}";
                achievement.description = description.Replace("N", targets[i].ToString());
                achievement.type = type;
                achievement.targetValue = targets[i];
                achievement.tier = i + 1;
                achievement.rewardType = rewards[i].rewardType;
                achievement.rewardAmount = rewards[i].amount;
                achievement.isSecret = false;

                EditorUtility.SetDirty(achievement);
            }
        }

        class AchievementReward
        {
            public AchievementData.RewardType rewardType;
            public double amount;

            public AchievementReward(AchievementData.RewardType type, double amt)
            {
                rewardType = type;
                amount = amt;
            }
        }

        #endregion

        #region 상점 아이템 생성

        [MenuItem("PunchKing/Generate Shop Items Only")]
        public static void GenerateShopItems()
        {
            Debug.Log("[DataGenerator] 상점 아이템 생성 중...");

            // === 골드 패키지 ===
            CreateShopItem("gold_small", "소량의 골드", "100만 골드 획득", ShopItemData.ShopCategory.Gold,
                0.99f, "com.punchking.gold_small",
                new ShopReward(ShopItemData.RewardData.RewardType.Gold, 1000000));

            CreateShopItem("gold_medium", "골드 묶음", "1,000만 골드 획득", ShopItemData.ShopCategory.Gold,
                2.99f, "com.punchking.gold_medium",
                new ShopReward(ShopItemData.RewardData.RewardType.Gold, 10000000));

            CreateShopItem("gold_large", "골드 보따리", "1억 골드 획득", ShopItemData.ShopCategory.Gold,
                4.99f, "com.punchking.gold_large",
                new ShopReward(ShopItemData.RewardData.RewardType.Gold, 100000000));

            CreateShopItem("gold_mega", "메가 골드", "10억 골드 획득 + 2시간 골드 2배", ShopItemData.ShopCategory.Gold,
                9.99f, "com.punchking.gold_mega",
                new ShopReward(ShopItemData.RewardData.RewardType.Gold, 1000000000),
                new ShopReward(ShopItemData.RewardData.RewardType.GoldBoost, 0, 2f, 2));

            CreateShopItem("gold_ultra", "울트라 골드", "100억 골드 획득 + 6시간 골드 3배", ShopItemData.ShopCategory.Gold,
                19.99f, "com.punchking.gold_ultra",
                new ShopReward(ShopItemData.RewardData.RewardType.Gold, 10000000000),
                new ShopReward(ShopItemData.RewardData.RewardType.GoldBoost, 0, 3f, 6),
                isBestValue: true);

            // === 프레스티지 화폐 ===
            CreateShopItem("prestige_small", "프레스티지 스톤", "100 프레스티지 화폐", ShopItemData.ShopCategory.PrestigeCurrency,
                1.99f, "com.punchking.prestige_small",
                new ShopReward(ShopItemData.RewardData.RewardType.PrestigeCurrency, 100));

            CreateShopItem("prestige_medium", "프레스티지 크리스탈", "500 프레스티지 화폐", ShopItemData.ShopCategory.PrestigeCurrency,
                4.99f, "com.punchking.prestige_medium",
                new ShopReward(ShopItemData.RewardData.RewardType.PrestigeCurrency, 500));

            CreateShopItem("prestige_large", "프레스티지 다이아", "1,500 프레스티지 화폐", ShopItemData.ShopCategory.PrestigeCurrency,
                9.99f, "com.punchking.prestige_large",
                new ShopReward(ShopItemData.RewardData.RewardType.PrestigeCurrency, 1500),
                isBestValue: true);

            // === 특별 패키지 ===
            CreateShopItem("starter_pack", "스타터 팩", "50억 골드 + 200 프레스티지 + 24시간 부스트", ShopItemData.ShopCategory.StarterPack,
                4.99f, "com.punchking.starter_pack",
                new ShopReward(ShopItemData.RewardData.RewardType.Gold, 5000000000),
                new ShopReward(ShopItemData.RewardData.RewardType.PrestigeCurrency, 200),
                new ShopReward(ShopItemData.RewardData.RewardType.DamageBoost, 0, 2f, 24),
                maxPurchases: 1);

            CreateShopItem("mega_bundle", "메가 번들", "100억 골드 + 500 프레스티지 + 모든 스킬 쿨다운 리셋", ShopItemData.ShopCategory.SpecialPackage,
                14.99f, "com.punchking.mega_bundle",
                new ShopReward(ShopItemData.RewardData.RewardType.Gold, 10000000000),
                new ShopReward(ShopItemData.RewardData.RewardType.PrestigeCurrency, 500),
                new ShopReward(ShopItemData.RewardData.RewardType.SkillCooldownReset),
                isBestValue: true);

            // === 영구 업그레이드 ===
            CreateShopItem("permanent_auto_speed", "영구 자동클릭 속도 +50%", "자동클릭 속도가 영구적으로 50% 증가", ShopItemData.ShopCategory.PermanentUpgrade,
                9.99f, "com.punchking.perm_auto_speed",
                new ShopReward(ShopItemData.RewardData.RewardType.AutoClickSpeedPermanent, 0, 0.5f),
                maxPurchases: 1);

            CreateShopItem("permanent_offline", "영구 오프라인 수익 +100%", "오프라인 수익이 영구적으로 100% 증가", ShopItemData.ShopCategory.PermanentUpgrade,
                9.99f, "com.punchking.perm_offline",
                new ShopReward(ShopItemData.RewardData.RewardType.OfflineEarningsPermanent, 0, 1.0f),
                maxPurchases: 1);

            CreateShopItem("permanent_crit", "영구 크리티컬 확률 +5%", "크리티컬 확률이 영구적으로 5% 증가", ShopItemData.ShopCategory.PermanentUpgrade,
                12.99f, "com.punchking.perm_crit",
                new ShopReward(ShopItemData.RewardData.RewardType.CritChancePermanent, 0, 0.05f),
                maxPurchases: 1);

            CreateShopItem("remove_ads", "광고 제거", "모든 광고를 영구적으로 제거", ShopItemData.ShopCategory.PermanentUpgrade,
                2.99f, "com.punchking.remove_ads",
                new ShopReward(ShopItemData.RewardData.RewardType.RemoveAds),
                maxPurchases: 1);

            // === 시한부 부스트 ===
            CreateShopItem("damage_boost_24h", "24시간 데미지 3배", "24시간 동안 데미지 3배", ShopItemData.ShopCategory.TimedBoost,
                3.99f, "com.punchking.damage_boost_24h",
                new ShopReward(ShopItemData.RewardData.RewardType.DamageBoost, 0, 3f, 24));

            CreateShopItem("gold_boost_24h", "24시간 골드 3배", "24시간 동안 골드 3배", ShopItemData.ShopCategory.TimedBoost,
                3.99f, "com.punchking.gold_boost_24h",
                new ShopReward(ShopItemData.RewardData.RewardType.GoldBoost, 0, 3f, 24));

            CreateShopItem("combo_boost_48h", "48시간 풀 부스트", "48시간 동안 데미지/골드 모두 4배", ShopItemData.ShopCategory.TimedBoost,
                9.99f, "com.punchking.combo_boost_48h",
                new ShopReward(ShopItemData.RewardData.RewardType.DamageBoost, 0, 4f, 48),
                new ShopReward(ShopItemData.RewardData.RewardType.GoldBoost, 0, 4f, 48),
                isBestValue: true);

            // === 제한 시간 특별 상품 ===
            CreateShopItem("limited_weekend", "주말 특가", "1조 골드 + 1000 프레스티지 (70% 할인!)", ShopItemData.ShopCategory.SpecialPackage,
                29.99f, "com.punchking.limited_weekend",
                new ShopReward(ShopItemData.RewardData.RewardType.Gold, 1000000000000),
                new ShopReward(ShopItemData.RewardData.RewardType.PrestigeCurrency, 1000),
                discountPercent: 70,
                isLimitedTime: true,
                limitedDays: 3);

            CreateShopItem("black_friday", "블랙프라이데이 패키지", "10조 골드 + 5000 프레스티지 + 모든 영구 업그레이드 (80% 할인!)", ShopItemData.ShopCategory.SpecialPackage,
                49.99f, "com.punchking.black_friday",
                new ShopReward(ShopItemData.RewardData.RewardType.Gold, 10000000000000),
                new ShopReward(ShopItemData.RewardData.RewardType.PrestigeCurrency, 5000),
                new ShopReward(ShopItemData.RewardData.RewardType.AutoClickSpeedPermanent, 0, 1f),
                new ShopReward(ShopItemData.RewardData.RewardType.OfflineEarningsPermanent, 0, 2f),
                new ShopReward(ShopItemData.RewardData.RewardType.CritChancePermanent, 0, 0.1f),
                discountPercent: 80,
                isLimitedTime: true,
                limitedDays: 7,
                isBestValue: true,
                maxPurchases: 1);

            Debug.Log("[DataGenerator] ✅ 상점 아이템 20개 생성 완료");
        }

        static void CreateShopItem(string id, string name, string description, ShopItemData.ShopCategory category,
            float priceUSD, string productID, params ShopReward[] rewards)
        {
            CreateShopItem(id, name, description, category, priceUSD, productID, rewards,
                0, false, 7, false, false, 0);
        }

        static void CreateShopItem(string id, string name, string description, ShopItemData.ShopCategory category,
            float priceUSD, string productID, ShopReward[] rewards,
            int discountPercent = 0, bool isLimitedTime = false, int limitedDays = 7,
            bool isBestValue = false, bool isPermanent = false, int maxPurchases = 0)
        {
            string path = $"{SHOP_PATH}/{id}.asset";

            ShopItemData item = AssetDatabase.LoadAssetAtPath<ShopItemData>(path);
            if (item == null)
            {
                item = ScriptableObject.CreateInstance<ShopItemData>();
                AssetDatabase.CreateAsset(item, path);
            }

            item.itemID = id;
            item.itemName = name;
            item.description = description;
            item.category = category;
            item.isRealMoney = true;
            item.priceUSD = priceUSD;
            item.productID = productID;
            item.discountPercent = discountPercent;
            item.isLimitedTime = isLimitedTime;
            item.limitedTimeDays = limitedDays;
            item.isBestValue = isBestValue;
            item.isPermanent = isPermanent;
            item.hasLimitedPurchases = maxPurchases > 0;
            item.maxPurchases = maxPurchases;

            item.rewards = new ShopItemData.RewardData[rewards.Length];
            for (int i = 0; i < rewards.Length; i++)
            {
                item.rewards[i] = new ShopItemData.RewardData
                {
                    type = rewards[i].type,
                    amount = rewards[i].amount,
                    multiplier = rewards[i].multiplier,
                    durationHours = rewards[i].durationHours
                };
            }

            EditorUtility.SetDirty(item);
        }

        class ShopReward
        {
            public ShopItemData.RewardData.RewardType type;
            public long amount;
            public float multiplier;
            public int durationHours;

            public ShopReward(ShopItemData.RewardData.RewardType rewardType, long amt = 0, float mult = 1f, int hours = 0)
            {
                type = rewardType;
                amount = amt;
                multiplier = mult;
                durationHours = hours;
            }
        }

        #endregion
    }
}
#endif
