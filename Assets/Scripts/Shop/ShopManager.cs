using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace PunchKing
{
    /// <summary>
    /// 상점 매니저
    /// IAP 구매, 게임 화폐 구매, 보상 지급
    /// </summary>
    public class ShopManager : MonoBehaviour
    {
        public static ShopManager Instance { get; private set; }

        [Header("상점 아이템")]
        public List<ShopItemData> shopItems = new List<ShopItemData>();

        [Header("영구 업그레이드 보너스")]
        public float permanentAutoClickSpeedBonus = 0f;
        public float permanentOfflineEarningsBonus = 0f;
        public float permanentCritChanceBonus = 0f;
        public bool adsRemoved = false;

        [Header("이벤트")]
        public UnityEngine.Events.UnityEvent OnPurchaseSuccess;
        public UnityEngine.Events.UnityEvent OnPurchaseFailed;

        // 구매 횟수 추적
        private Dictionary<string, int> purchaseCount = new Dictionary<string, int>();

        // 제한 시간 아이템 추적
        private Dictionary<string, DateTime> limitedTimeItems = new Dictionary<string, DateTime>();

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
            }
        }

        void Start()
        {
            LoadPurchaseData();
            InitializeIAP();
        }

        /// <summary>
        /// IAP 초기화 (Unity IAP 통합)
        /// </summary>
        void InitializeIAP()
        {
#if UNITY_PURCHASING
            // Unity IAP 초기화
            // 실제 구현 시 Unity IAP 패키지 필요
            Debug.Log("[ShopManager] IAP 초기화 시작");

            // ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            // foreach (var item in shopItems.Where(i => i.isRealMoney))
            // {
            //     builder.AddProduct(item.productID, ProductType.Consumable);
            // }
            // UnityPurchasing.Initialize(this, builder);
#else
            Debug.Log("[ShopManager] Unity IAP 패키지가 설치되지 않았습니다. 테스트 모드로 실행됩니다.");
#endif
        }

        /// <summary>
        /// 상점 아이템 구매
        /// </summary>
        public void PurchaseItem(ShopItemData item)
        {
            if (item == null)
            {
                Debug.LogError("[ShopManager] 구매 실패: 아이템이 null입니다.");
                OnPurchaseFailed?.Invoke();
                return;
            }

            // 구매 제한 체크
            if (!CanPurchaseItem(item))
            {
                Debug.Log($"[ShopManager] 구매 불가: {item.itemName} (구매 제한 초과 또는 기간 만료)");
                UIManager.Instance?.ShowBuffNotification("구매 불가", "구매 제한에 도달했거나 기간이 만료되었습니다.", 2f);
                OnPurchaseFailed?.Invoke();
                return;
            }

            // 실제 돈으로 구매
            if (item.isRealMoney)
            {
                PurchaseWithRealMoney(item);
            }
            // 프레스티지 화폐로 구매
            else if (item.usePrestigeCurrency)
            {
                PurchaseWithPrestigeCurrency(item);
            }
            // 무료 아이템
            else
            {
                ProcessPurchase(item);
            }
        }

        /// <summary>
        /// 실제 돈으로 구매 (IAP)
        /// </summary>
        void PurchaseWithRealMoney(ShopItemData item)
        {
#if UNITY_PURCHASING
            // Unity IAP 구매 프로세스
            // m_StoreController.InitiatePurchase(item.productID);
            Debug.Log($"[ShopManager] IAP 구매 시작: {item.productID}");
#else
            // 테스트 모드: 즉시 구매 성공 처리
            Debug.Log($"[ShopManager] 테스트 모드 - IAP 구매 시뮬레이션: {item.itemName}");
            ProcessPurchase(item);
#endif
        }

        /// <summary>
        /// 프레스티지 화폐로 구매
        /// </summary>
        void PurchaseWithPrestigeCurrency(ShopItemData item)
        {
            if (PrestigeManager.Instance == null)
            {
                Debug.LogError("[ShopManager] PrestigeManager가 없습니다.");
                OnPurchaseFailed?.Invoke();
                return;
            }

            BigNumber currentCurrency = PrestigeManager.Instance.prestigeCurrency;
            BigNumber cost = new BigNumber(item.prestigeCurrencyCost);

            if (currentCurrency.CompareTo(cost) >= 0)
            {
                // 화폐 차감
                PrestigeManager.Instance.prestigeCurrency = currentCurrency.Subtract(cost);
                ProcessPurchase(item);
            }
            else
            {
                Debug.Log("[ShopManager] 프레스티지 화폐 부족");
                UIManager.Instance?.ShowBuffNotification("구매 실패", "프레스티지 화폐가 부족합니다.", 2f);
                OnPurchaseFailed?.Invoke();
            }
        }

        /// <summary>
        /// 구매 처리 및 보상 지급
        /// </summary>
        public void ProcessPurchase(ShopItemData item)
        {
            Debug.Log($"[ShopManager] 구매 성공: {item.itemName}");

            // 보상 지급
            GiveRewards(item);

            // 구매 횟수 증가
            IncrementPurchaseCount(item.itemID);

            // 제한 시간 아이템 추적
            if (item.isLimitedTime)
            {
                DateTime expiryDate = DateTime.Now.AddDays(item.limitedTimeDays);
                limitedTimeItems[item.itemID] = expiryDate;
            }

            // 저장
            SavePurchaseData();

            // 성공 알림
            UIManager.Instance?.ShowBuffNotification(
                "구매 성공!",
                $"{item.itemName}\n{item.GetRewardDescription()}",
                3f
            );

            OnPurchaseSuccess?.Invoke();

            // 오디오
            AudioManager.Instance?.PlaySFX("Purchase");
        }

        /// <summary>
        /// 보상 지급
        /// </summary>
        void GiveRewards(ShopItemData item)
        {
            foreach (var reward in item.rewards)
            {
                switch (reward.type)
                {
                    case ShopItemData.RewardData.RewardType.Gold:
                        GameManager.Instance?.AddGold(new BigNumber(reward.amount));
                        break;

                    case ShopItemData.RewardData.RewardType.PrestigeCurrency:
                        if (PrestigeManager.Instance != null)
                        {
                            PrestigeManager.Instance.prestigeCurrency =
                                PrestigeManager.Instance.prestigeCurrency.Add(new BigNumber(reward.amount));
                        }
                        break;

                    case ShopItemData.RewardData.RewardType.DamageBoost:
                        ApplyTimedBoost("Damage", reward.multiplier, reward.durationHours);
                        break;

                    case ShopItemData.RewardData.RewardType.GoldBoost:
                        ApplyTimedBoost("Gold", reward.multiplier, reward.durationHours);
                        break;

                    case ShopItemData.RewardData.RewardType.AutoClickSpeedPermanent:
                        permanentAutoClickSpeedBonus += reward.multiplier;
                        Debug.Log($"[ShopManager] 영구 자동클릭 속도 증가: +{reward.multiplier * 100f}%");
                        break;

                    case ShopItemData.RewardData.RewardType.OfflineEarningsPermanent:
                        permanentOfflineEarningsBonus += reward.multiplier;
                        Debug.Log($"[ShopManager] 영구 오프라인 수익 증가: +{reward.multiplier * 100f}%");
                        break;

                    case ShopItemData.RewardData.RewardType.CritChancePermanent:
                        permanentCritChanceBonus += reward.multiplier;
                        if (GameManager.Instance != null)
                        {
                            GameManager.Instance.critChance += reward.multiplier;
                        }
                        Debug.Log($"[ShopManager] 영구 크리티컬 확률 증가: +{reward.multiplier * 100f}%");
                        break;

                    case ShopItemData.RewardData.RewardType.SkillCooldownReset:
                        SkillManager.Instance?.ResetAllCooldowns();
                        break;

                    case ShopItemData.RewardData.RewardType.RemoveAds:
                        adsRemoved = true;
                        Debug.Log("[ShopManager] 광고 영구 제거");
                        break;
                }
            }
        }

        /// <summary>
        /// 시한부 부스트 적용
        /// </summary>
        void ApplyTimedBoost(string boostType, float multiplier, int hours)
        {
            if (GameManager.Instance == null) return;

            float duration = hours * 3600f;

            if (boostType == "Damage")
            {
                GameManager.Instance.StartCoroutine(ApplyDamageBoost(multiplier, duration));
            }
            else if (boostType == "Gold")
            {
                GameManager.Instance.StartCoroutine(ApplyGoldBoost(multiplier, duration));
            }
        }

        System.Collections.IEnumerator ApplyDamageBoost(float multiplier, float duration)
        {
            Debug.Log($"[ShopManager] 데미지 부스트 시작: {multiplier}배, {duration / 3600f}시간");

            // GameManager에 배율 적용 (실제로는 GameManager에 boostMultiplier 변수 추가 필요)
            float originalDamage = 1f;
            // GameManager.Instance.damageMultiplier *= multiplier;

            yield return new WaitForSeconds(duration);

            // GameManager.Instance.damageMultiplier /= multiplier;
            Debug.Log("[ShopManager] 데미지 부스트 종료");
        }

        System.Collections.IEnumerator ApplyGoldBoost(float multiplier, float duration)
        {
            Debug.Log($"[ShopManager] 골드 부스트 시작: {multiplier}배, {duration / 3600f}시간");

            // GameManager.Instance.goldMultiplier *= multiplier;

            yield return new WaitForSeconds(duration);

            // GameManager.Instance.goldMultiplier /= multiplier;
            Debug.Log("[ShopManager] 골드 부스트 종료");
        }

        /// <summary>
        /// 구매 가능 여부 확인
        /// </summary>
        public bool CanPurchaseItem(ShopItemData item)
        {
            // 구매 횟수 제한 체크
            if (item.hasLimitedPurchases)
            {
                int count = GetPurchaseCount(item.itemID);
                if (count >= item.maxPurchases)
                {
                    return false;
                }
            }

            // 제한 시간 체크
            if (item.isLimitedTime)
            {
                if (limitedTimeItems.ContainsKey(item.itemID))
                {
                    if (DateTime.Now > limitedTimeItems[item.itemID])
                    {
                        // 기간 만료
                        limitedTimeItems.Remove(item.itemID);
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 구매 횟수 증가
        /// </summary>
        void IncrementPurchaseCount(string itemID)
        {
            if (purchaseCount.ContainsKey(itemID))
            {
                purchaseCount[itemID]++;
            }
            else
            {
                purchaseCount[itemID] = 1;
            }
        }

        /// <summary>
        /// 구매 횟수 가져오기
        /// </summary>
        public int GetPurchaseCount(string itemID)
        {
            return purchaseCount.ContainsKey(itemID) ? purchaseCount[itemID] : 0;
        }

        /// <summary>
        /// 카테고리별 상점 아이템 가져오기
        /// </summary>
        public List<ShopItemData> GetItemsByCategory(ShopItemData.ShopCategory category)
        {
            return shopItems.Where(i => i.category == category).ToList();
        }

        /// <summary>
        /// 저장
        /// </summary>
        void SavePurchaseData()
        {
            // 구매 횟수 저장
            foreach (var kvp in purchaseCount)
            {
                PlayerPrefs.SetInt($"ShopPurchase_{kvp.Key}", kvp.Value);
            }

            // 영구 업그레이드 저장
            PlayerPrefs.SetFloat("Shop_AutoClickSpeedBonus", permanentAutoClickSpeedBonus);
            PlayerPrefs.SetFloat("Shop_OfflineEarningsBonus", permanentOfflineEarningsBonus);
            PlayerPrefs.SetFloat("Shop_CritChanceBonus", permanentCritChanceBonus);
            PlayerPrefs.SetInt("Shop_AdsRemoved", adsRemoved ? 1 : 0);

            PlayerPrefs.Save();
        }

        /// <summary>
        /// 로드
        /// </summary>
        void LoadPurchaseData()
        {
            // 영구 업그레이드 로드
            permanentAutoClickSpeedBonus = PlayerPrefs.GetFloat("Shop_AutoClickSpeedBonus", 0f);
            permanentOfflineEarningsBonus = PlayerPrefs.GetFloat("Shop_OfflineEarningsBonus", 0f);
            permanentCritChanceBonus = PlayerPrefs.GetFloat("Shop_CritChanceBonus", 0f);
            adsRemoved = PlayerPrefs.GetInt("Shop_AdsRemoved", 0) == 1;

            // 구매 횟수 로드
            foreach (var item in shopItems)
            {
                int count = PlayerPrefs.GetInt($"ShopPurchase_{item.itemID}", 0);
                if (count > 0)
                {
                    purchaseCount[item.itemID] = count;
                }
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Reset All Purchases")]
        void ResetAllPurchases()
        {
            purchaseCount.Clear();
            limitedTimeItems.Clear();
            permanentAutoClickSpeedBonus = 0f;
            permanentOfflineEarningsBonus = 0f;
            permanentCritChanceBonus = 0f;
            adsRemoved = false;

            foreach (var item in shopItems)
            {
                PlayerPrefs.DeleteKey($"ShopPurchase_{item.itemID}");
            }
            PlayerPrefs.DeleteKey("Shop_AutoClickSpeedBonus");
            PlayerPrefs.DeleteKey("Shop_OfflineEarningsBonus");
            PlayerPrefs.DeleteKey("Shop_CritChanceBonus");
            PlayerPrefs.DeleteKey("Shop_AdsRemoved");

            Debug.Log("[ShopManager] 모든 구매 기록 초기화 완료");
        }

        [ContextMenu("Test Purchase First Item")]
        void TestPurchaseFirstItem()
        {
            if (shopItems.Count > 0)
            {
                ProcessPurchase(shopItems[0]);
            }
        }
#endif
    }
}
