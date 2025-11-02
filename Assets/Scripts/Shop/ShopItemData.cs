using UnityEngine;
using System;

namespace PunchKing
{
    /// <summary>
    /// 상점 아이템 데이터 (ScriptableObject)
    /// IAP, 골드 패키지, 특별 상품 정의
    /// </summary>
    [CreateAssetMenu(fileName = "ShopItem", menuName = "PunchKing/Shop Item", order = 5)]
    public class ShopItemData : ScriptableObject
    {
        [Header("기본 정보")]
        public string itemID;
        public string itemName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("카테고리")]
        public ShopCategory category;

        [Header("가격 (실제 돈)")]
        public bool isRealMoney = false;
        public float priceUSD = 0.99f;
        public string productID; // IAP Product ID (com.punchking.gold100)

        [Header("가격 (게임 화폐)")]
        public bool usePrestigeCurrency = false;
        public long prestigeCurrencyCost = 0;

        [Header("보상")]
        public RewardData[] rewards;

        [Header("특별 속성")]
        public bool isLimitedTime = false;
        public int limitedTimeDays = 7;
        public bool isPermanent = false;
        public bool isBestValue = false;
        public int discountPercent = 0;

        [Header("구매 제한")]
        public bool hasLimitedPurchases = false;
        public int maxPurchases = 1;

        public enum ShopCategory
        {
            Gold,              // 골드 패키지
            PrestigeCurrency,  // 프레스티지 화폐
            SpecialPackage,    // 특별 패키지 (골드+부스트 등)
            PermanentUpgrade,  // 영구 업그레이드
            TimedBoost,        // 시한부 부스트
            StarterPack        // 신규 유저 스타터 팩
        }

        [System.Serializable]
        public class RewardData
        {
            public RewardType type;
            public long amount;
            public float multiplier = 1f;
            public int durationHours = 0;

            public enum RewardType
            {
                Gold,                    // 골드
                PrestigeCurrency,        // 프레스티지 화폐
                DamageBoost,             // 데미지 부스트 (시간제)
                GoldBoost,               // 골드 부스트 (시간제)
                AutoClickSpeedPermanent, // 영구 자동클릭 속도 증가
                OfflineEarningsPermanent,// 영구 오프라인 수익 증가
                CritChancePermanent,     // 영구 크리티컬 확률 증가
                SkillCooldownReset,      // 스킬 쿨다운 즉시 리셋
                RemoveAds                // 광고 제거
            }
        }

        /// <summary>
        /// 가격 문자열 반환
        /// </summary>
        public string GetPriceString()
        {
            if (isRealMoney)
            {
                return $"${priceUSD:F2}";
            }
            else if (usePrestigeCurrency)
            {
                return $"{prestigeCurrencyCost} 프레스티지";
            }
            return "무료";
        }

        /// <summary>
        /// 할인된 가격 반환
        /// </summary>
        public float GetDiscountedPrice()
        {
            if (discountPercent > 0)
            {
                return priceUSD * (1f - discountPercent / 100f);
            }
            return priceUSD;
        }

        /// <summary>
        /// 보상 설명 반환
        /// </summary>
        public string GetRewardDescription()
        {
            string result = "";
            foreach (var reward in rewards)
            {
                switch (reward.type)
                {
                    case RewardData.RewardType.Gold:
                        result += $"💰 {new BigNumber(reward.amount).ToKoreanString()} 골드\n";
                        break;
                    case RewardData.RewardType.PrestigeCurrency:
                        result += $"⭐ {reward.amount} 프레스티지 화폐\n";
                        break;
                    case RewardData.RewardType.DamageBoost:
                        result += $"⚔️ {reward.multiplier}배 데미지 부스트 ({reward.durationHours}시간)\n";
                        break;
                    case RewardData.RewardType.GoldBoost:
                        result += $"💎 {reward.multiplier}배 골드 부스트 ({reward.durationHours}시간)\n";
                        break;
                    case RewardData.RewardType.AutoClickSpeedPermanent:
                        result += $"🔄 영구 자동클릭 속도 +{reward.multiplier * 100f}%\n";
                        break;
                    case RewardData.RewardType.OfflineEarningsPermanent:
                        result += $"😴 영구 오프라인 수익 +{reward.multiplier * 100f}%\n";
                        break;
                    case RewardData.RewardType.CritChancePermanent:
                        result += $"💥 영구 크리티컬 확률 +{reward.multiplier * 100f}%\n";
                        break;
                    case RewardData.RewardType.SkillCooldownReset:
                        result += $"⚡ 모든 스킬 쿨다운 즉시 리셋\n";
                        break;
                    case RewardData.RewardType.RemoveAds:
                        result += $"🚫 광고 영구 제거\n";
                        break;
                }
            }
            return result;
        }
    }
}
