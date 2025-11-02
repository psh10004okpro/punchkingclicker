using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Linq;

namespace PunchKing.Editor
{
    /// <summary>
    /// 생성된 ScriptableObject 데이터 검증 도구
    /// </summary>
    public class DataVerifier : MonoBehaviour
    {
        private const string UPGRADES_PATH = "Assets/Data/Upgrades";
        private const string ACHIEVEMENTS_PATH = "Assets/Data/Achievements";
        private const string SHOP_PATH = "Assets/Data/Shop";

        [MenuItem("PunchKing/Verify Generated Data")]
        public static void VerifyAllData()
        {
            Debug.Log("[DataVerifier] 데이터 검증 시작...\n");

            bool upgradesValid = VerifyUpgrades();
            bool achievementsValid = VerifyAchievements();
            bool shopValid = VerifyShopItems();

            Debug.Log("\n========== 검증 결과 ==========");
            Debug.Log($"업그레이드: {(upgradesValid ? "✅ 통과" : "❌ 실패")}");
            Debug.Log($"업적: {(achievementsValid ? "✅ 통과" : "❌ 실패")}");
            Debug.Log($"상점 아이템: {(shopValid ? "✅ 통과" : "❌ 실패")}");

            if (upgradesValid && achievementsValid && shopValid)
            {
                Debug.Log("\n🎉 모든 데이터 검증 성공!");
                EditorUtility.DisplayDialog("검증 완료",
                    "모든 ScriptableObject 데이터가 정상적으로 생성되었습니다!\n\n" +
                    "✅ 업그레이드: 8개\n" +
                    "✅ 업적: 60개\n" +
                    "✅ 상점 아이템: 20개",
                    "확인");
            }
            else
            {
                EditorUtility.DisplayDialog("검증 실패",
                    "일부 데이터에 문제가 있습니다.\n\n" +
                    "Console 창을 확인하여 상세 에러를 확인하세요.",
                    "확인");
            }
        }

        static bool VerifyUpgrades()
        {
            Debug.Log("\n[업그레이드 검증]");

            string[] guids = AssetDatabase.FindAssets("t:UpgradeData", new[] { UPGRADES_PATH });
            var upgrades = guids.Select(guid => AssetDatabase.LoadAssetAtPath<UpgradeData>(
                AssetDatabase.GUIDToAssetPath(guid))).ToList();

            if (upgrades.Count != 8)
            {
                Debug.LogError($"❌ 업그레이드 개수 오류: {upgrades.Count}개 (예상: 8개)");
                return false;
            }

            Debug.Log($"✅ 개수: {upgrades.Count}/8");

            // 각 업그레이드 타입 확인
            var expectedTypes = new[] {
                UpgradeData.UpgradeType.PunchPower,
                UpgradeData.UpgradeType.AutoClicker,
                UpgradeData.UpgradeType.CriticalChance,
                UpgradeData.UpgradeType.CriticalDamage,
                UpgradeData.UpgradeType.GoldMultiplier,
                UpgradeData.UpgradeType.OfflineEarnings,
                UpgradeData.UpgradeType.ClickDamage,
                UpgradeData.UpgradeType.GoldPerClick
            };

            bool allTypesFound = true;
            foreach (var type in expectedTypes)
            {
                if (!upgrades.Any(u => u.upgradeType == type))
                {
                    Debug.LogError($"❌ 업그레이드 타입 누락: {type}");
                    allTypesFound = false;
                }
            }

            if (allTypesFound)
            {
                Debug.Log("✅ 모든 업그레이드 타입 존재");
            }

            // 데이터 유효성 검사
            foreach (var upgrade in upgrades)
            {
                if (string.IsNullOrEmpty(upgrade.upgradeID))
                {
                    Debug.LogWarning($"⚠️ {upgrade.name}: ID가 비어있음");
                }
                if (upgrade.baseCost <= 0)
                {
                    Debug.LogWarning($"⚠️ {upgrade.name}: 베이스 비용이 0 이하");
                }
                if (upgrade.costMultiplier <= 1f)
                {
                    Debug.LogWarning($"⚠️ {upgrade.name}: 비용 배율이 1 이하");
                }
            }

            return allTypesFound;
        }

        static bool VerifyAchievements()
        {
            Debug.Log("\n[업적 검증]");

            string[] guids = AssetDatabase.FindAssets("t:AchievementData", new[] { ACHIEVEMENTS_PATH });
            var achievements = guids.Select(guid => AssetDatabase.LoadAssetAtPath<AchievementData>(
                AssetDatabase.GUIDToAssetPath(guid))).ToList();

            if (achievements.Count != 60)
            {
                Debug.LogError($"❌ 업적 개수 오류: {achievements.Count}개 (예상: 60개)");
                return false;
            }

            Debug.Log($"✅ 개수: {achievements.Count}/60");

            // 각 업적 타입별 5개씩 확인
            var achievementTypes = System.Enum.GetValues(typeof(AchievementData.AchievementType))
                .Cast<AchievementData.AchievementType>();

            bool allTypesValid = true;
            foreach (var type in achievementTypes)
            {
                var typeAchievements = achievements.Where(a => a.type == type).ToList();

                if (typeAchievements.Count != 5)
                {
                    Debug.LogError($"❌ {type}: {typeAchievements.Count}개 (예상: 5개)");
                    allTypesValid = false;
                }
                else
                {
                    // 티어 1~5 확인
                    for (int tier = 1; tier <= 5; tier++)
                    {
                        if (!typeAchievements.Any(a => a.tier == tier))
                        {
                            Debug.LogError($"❌ {type}: 티어 {tier} 누락");
                            allTypesValid = false;
                        }
                    }
                }
            }

            if (allTypesValid)
            {
                Debug.Log("✅ 모든 업적 타입 및 티어 존재");
            }

            // 데이터 유효성 검사
            foreach (var achievement in achievements)
            {
                if (string.IsNullOrEmpty(achievement.achievementId))
                {
                    Debug.LogWarning($"⚠️ {achievement.name}: ID가 비어있음");
                }
                if (achievement.targetValue <= 0)
                {
                    Debug.LogWarning($"⚠️ {achievement.name}: 목표값이 0 이하");
                }
                if (achievement.rewardAmount <= 0)
                {
                    Debug.LogWarning($"⚠️ {achievement.name}: 보상이 0");
                }
            }

            return allTypesValid;
        }

        static bool VerifyShopItems()
        {
            Debug.Log("\n[상점 아이템 검증]");

            string[] guids = AssetDatabase.FindAssets("t:ShopItemData", new[] { SHOP_PATH });
            var items = guids.Select(guid => AssetDatabase.LoadAssetAtPath<ShopItemData>(
                AssetDatabase.GUIDToAssetPath(guid))).ToList();

            if (items.Count != 20)
            {
                Debug.LogError($"❌ 상점 아이템 개수 오류: {items.Count}개 (예상: 20개)");
                return false;
            }

            Debug.Log($"✅ 개수: {items.Count}/20");

            // 카테고리별 개수 확인
            var categoryCounts = new System.Collections.Generic.Dictionary<ShopItemData.ShopCategory, int>
            {
                { ShopItemData.ShopCategory.Gold, 5 },
                { ShopItemData.ShopCategory.PrestigeCurrency, 3 },
                { ShopItemData.ShopCategory.SpecialPackage, 4 },
                { ShopItemData.ShopCategory.PermanentUpgrade, 4 },
                { ShopItemData.ShopCategory.TimedBoost, 3 },
                { ShopItemData.ShopCategory.StarterPack, 1 }
            };

            bool allCategoriesValid = true;
            foreach (var kvp in categoryCounts)
            {
                int count = items.Count(i => i.category == kvp.Key);
                if (count != kvp.Value)
                {
                    Debug.LogWarning($"⚠️ {kvp.Key}: {count}개 (예상: {kvp.Value}개)");
                    allCategoriesValid = false;
                }
            }

            if (allCategoriesValid)
            {
                Debug.Log("✅ 모든 카테고리 아이템 존재");
            }

            // Best Value 개수 확인
            int bestValueCount = items.Count(i => i.isBestValue);
            Debug.Log($"✅ Best Value 아이템: {bestValueCount}개");

            // 데이터 유효성 검사
            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.itemID))
                {
                    Debug.LogWarning($"⚠️ {item.name}: ID가 비어있음");
                }
                if (item.isRealMoney && item.priceUSD <= 0)
                {
                    Debug.LogWarning($"⚠️ {item.name}: 가격이 0 이하");
                }
                if (item.rewards == null || item.rewards.Length == 0)
                {
                    Debug.LogWarning($"⚠️ {item.name}: 보상이 없음");
                }
            }

            return true;
        }

        [MenuItem("PunchKing/Data Summary")]
        public static void ShowDataSummary()
        {
            string[] upgradeGuids = AssetDatabase.FindAssets("t:UpgradeData", new[] { UPGRADES_PATH });
            string[] achievementGuids = AssetDatabase.FindAssets("t:AchievementData", new[] { ACHIEVEMENTS_PATH });
            string[] shopGuids = AssetDatabase.FindAssets("t:ShopItemData", new[] { SHOP_PATH });

            string summary = "=== 데이터 요약 ===\n\n";
            summary += $"📦 업그레이드: {upgradeGuids.Length}개\n";
            summary += $"🏆 업적: {achievementGuids.Length}개\n";
            summary += $"🛒 상점 아이템: {shopGuids.Length}개\n\n";

            if (upgradeGuids.Length == 8 && achievementGuids.Length == 60 && shopGuids.Length == 20)
            {
                summary += "✅ 모든 데이터가 정상적으로 생성되었습니다!";
            }
            else
            {
                summary += "⚠️ 일부 데이터가 누락되었습니다.\n";
                summary += "PunchKing > Generate All Data를 실행하세요.";
            }

            Debug.Log(summary);
            EditorUtility.DisplayDialog("데이터 요약", summary, "확인");
        }

        [MenuItem("PunchKing/Clear All Data")]
        public static void ClearAllData()
        {
            if (!EditorUtility.DisplayDialog("경고",
                "모든 생성된 데이터를 삭제하시겠습니까?\n\n" +
                "이 작업은 되돌릴 수 없습니다!",
                "삭제", "취소"))
            {
                return;
            }

            int deletedCount = 0;

            // 업그레이드 삭제
            string[] upgradeGuids = AssetDatabase.FindAssets("t:UpgradeData", new[] { UPGRADES_PATH });
            foreach (string guid in upgradeGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.DeleteAsset(path);
                deletedCount++;
            }

            // 업적 삭제
            string[] achievementGuids = AssetDatabase.FindAssets("t:AchievementData", new[] { ACHIEVEMENTS_PATH });
            foreach (string guid in achievementGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.DeleteAsset(path);
                deletedCount++;
            }

            // 상점 아이템 삭제
            string[] shopGuids = AssetDatabase.FindAssets("t:ShopItemData", new[] { SHOP_PATH });
            foreach (string guid in shopGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.DeleteAsset(path);
                deletedCount++;
            }

            AssetDatabase.Refresh();

            Debug.Log($"[DataVerifier] {deletedCount}개 파일 삭제 완료");
            EditorUtility.DisplayDialog("완료", $"{deletedCount}개 데이터 파일이 삭제되었습니다.", "확인");
        }
    }
}
#endif
