using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace PunchKing
{
    /// <summary>
    /// 상점 UI 패널
    /// 카테고리별 아이템 표시 및 구매 처리
    /// </summary>
    public class ShopPanel : MonoBehaviour
    {
        [Header("카테고리 탭")]
        public Toggle tabGold;
        public Toggle tabPrestigeCurrency;
        public Toggle tabSpecial;
        public Toggle tabPermanent;

        [Header("아이템 리스트")]
        public Transform itemContainer;
        public GameObject shopItemPrefab;

        [Header("상세 정보")]
        public GameObject detailPanel;
        public Image detailIcon;
        public TextMeshProUGUI detailNameText;
        public TextMeshProUGUI detailDescriptionText;
        public TextMeshProUGUI detailPriceText;
        public TextMeshProUGUI detailRewardsText;
        public Button purchaseButton;
        public TextMeshProUGUI purchaseButtonText;

        [Header("특별 표시")]
        public GameObject bestValueBadge;
        public GameObject limitedTimeBadge;
        public TextMeshProUGUI discountText;

        private List<ShopItemUI> shopItemUIs = new List<ShopItemUI>();
        private ShopItemData selectedItem;
        private ShopItemData.ShopCategory currentCategory = ShopItemData.ShopCategory.Gold;

        /// <summary>
        /// 상점 아이템 UI 클래스
        /// </summary>
        [System.Serializable]
        public class ShopItemUI
        {
            public GameObject gameObject;
            public Image icon;
            public TextMeshProUGUI nameText;
            public TextMeshProUGUI priceText;
            public GameObject bestValueBadge;
            public GameObject limitedBadge;
            public Button button;
            public ShopItemData data;
        }

        void Start()
        {
            SetupTabs();
            if (purchaseButton != null)
            {
                purchaseButton.onClick.AddListener(OnPurchaseClicked);
            }

            GenerateShopItems();

            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }
        }

        void OnEnable()
        {
            RefreshShopItems();
        }

        /// <summary>
        /// 탭 설정
        /// </summary>
        void SetupTabs()
        {
            if (tabGold != null)
                tabGold.onValueChanged.AddListener((isOn) => { if (isOn) OnCategoryChanged(ShopItemData.ShopCategory.Gold); });

            if (tabPrestigeCurrency != null)
                tabPrestigeCurrency.onValueChanged.AddListener((isOn) => { if (isOn) OnCategoryChanged(ShopItemData.ShopCategory.PrestigeCurrency); });

            if (tabSpecial != null)
                tabSpecial.onValueChanged.AddListener((isOn) => { if (isOn) OnCategoryChanged(ShopItemData.ShopCategory.SpecialPackage); });

            if (tabPermanent != null)
                tabPermanent.onValueChanged.AddListener((isOn) => { if (isOn) OnCategoryChanged(ShopItemData.ShopCategory.PermanentUpgrade); });
        }

        /// <summary>
        /// 카테고리 변경
        /// </summary>
        void OnCategoryChanged(ShopItemData.ShopCategory category)
        {
            currentCategory = category;
            FilterItemsByCategory();
        }

        /// <summary>
        /// 상점 아이템 생성
        /// </summary>
        void GenerateShopItems()
        {
            if (ShopManager.Instance == null || itemContainer == null || shopItemPrefab == null)
                return;

            ClearShopItems();

            var allItems = ShopManager.Instance.shopItems;

            foreach (var itemData in allItems)
            {
                CreateShopItemUI(itemData);
            }

            FilterItemsByCategory();
        }

        /// <summary>
        /// 상점 아이템 UI 생성
        /// </summary>
        void CreateShopItemUI(ShopItemData data)
        {
            GameObject itemObj = Instantiate(shopItemPrefab, itemContainer);
            ShopItemUI item = new ShopItemUI
            {
                gameObject = itemObj,
                data = data
            };

            // UI 컴포넌트 참조
            item.icon = itemObj.transform.Find("Icon")?.GetComponent<Image>();
            item.nameText = itemObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            item.priceText = itemObj.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            item.bestValueBadge = itemObj.transform.Find("BestValueBadge")?.gameObject;
            item.limitedBadge = itemObj.transform.Find("LimitedBadge")?.gameObject;
            item.button = itemObj.GetComponent<Button>();

            // 데이터 설정
            if (item.icon != null && data.icon != null)
                item.icon.sprite = data.icon;

            if (item.nameText != null)
                item.nameText.text = data.itemName;

            if (item.priceText != null)
                item.priceText.text = data.GetPriceString();

            // 특별 배지
            if (item.bestValueBadge != null)
                item.bestValueBadge.SetActive(data.isBestValue);

            if (item.limitedBadge != null)
                item.limitedBadge.SetActive(data.isLimitedTime);

            // 버튼 클릭
            if (item.button != null)
            {
                item.button.onClick.AddListener(() => OnItemClicked(item));
            }

            shopItemUIs.Add(item);
        }

        /// <summary>
        /// 아이템 클릭
        /// </summary>
        void OnItemClicked(ShopItemUI itemUI)
        {
            selectedItem = itemUI.data;
            ShowItemDetail(itemUI.data);
        }

        /// <summary>
        /// 아이템 상세 정보 표시
        /// </summary>
        void ShowItemDetail(ShopItemData item)
        {
            if (detailPanel == null) return;

            detailPanel.SetActive(true);

            // 아이콘
            if (detailIcon != null && item.icon != null)
                detailIcon.sprite = item.icon;

            // 이름
            if (detailNameText != null)
                detailNameText.text = item.itemName;

            // 설명
            if (detailDescriptionText != null)
                detailDescriptionText.text = item.description;

            // 가격
            if (detailPriceText != null)
            {
                if (item.discountPercent > 0)
                {
                    detailPriceText.text = $"<s>{item.GetPriceString()}</s> → ${item.GetDiscountedPrice():F2}";
                }
                else
                {
                    detailPriceText.text = item.GetPriceString();
                }
            }

            // 보상
            if (detailRewardsText != null)
                detailRewardsText.text = item.GetRewardDescription();

            // 할인 배지
            if (discountText != null)
            {
                if (item.discountPercent > 0)
                {
                    discountText.gameObject.SetActive(true);
                    discountText.text = $"-{item.discountPercent}%";
                }
                else
                {
                    discountText.gameObject.SetActive(false);
                }
            }

            // Best Value 배지
            if (bestValueBadge != null)
                bestValueBadge.SetActive(item.isBestValue);

            // Limited Time 배지
            if (limitedTimeBadge != null)
                limitedTimeBadge.SetActive(item.isLimitedTime);

            // 구매 버튼
            UpdatePurchaseButton(item);
        }

        /// <summary>
        /// 구매 버튼 업데이트
        /// </summary>
        void UpdatePurchaseButton(ShopItemData item)
        {
            if (purchaseButton == null) return;

            bool canPurchase = ShopManager.Instance.CanPurchaseItem(item);

            purchaseButton.interactable = canPurchase;

            if (purchaseButtonText != null)
            {
                if (canPurchase)
                {
                    purchaseButtonText.text = "구매하기";
                }
                else
                {
                    if (item.hasLimitedPurchases)
                    {
                        int count = ShopManager.Instance.GetPurchaseCount(item.itemID);
                        purchaseButtonText.text = $"구매 완료 ({count}/{item.maxPurchases})";
                    }
                    else
                    {
                        purchaseButtonText.text = "구매 불가";
                    }
                }
            }
        }

        /// <summary>
        /// 구매 버튼 클릭
        /// </summary>
        void OnPurchaseClicked()
        {
            if (selectedItem == null || ShopManager.Instance == null) return;

            // 확인 다이얼로그 (옵션)
            ShowPurchaseConfirmation(selectedItem);
        }

        /// <summary>
        /// 구매 확인 다이얼로그
        /// </summary>
        void ShowPurchaseConfirmation(ShopItemData item)
        {
            // 간단한 확인 메시지 (실제로는 다이얼로그 UI 사용 권장)
            string message = $"{item.itemName}을(를) 구매하시겠습니까?\n가격: {item.GetPriceString()}";
            Debug.Log($"[ShopPanel] 구매 확인: {message}");

            // 실제 구매 진행
            ShopManager.Instance.PurchaseItem(item);

            // 구매 후 UI 업데이트
            RefreshShopItems();
            UpdatePurchaseButton(item);
        }

        /// <summary>
        /// 카테고리별 필터링
        /// </summary>
        void FilterItemsByCategory()
        {
            foreach (var itemUI in shopItemUIs)
            {
                bool shouldShow = itemUI.data.category == currentCategory;
                itemUI.gameObject.SetActive(shouldShow);
            }
        }

        /// <summary>
        /// 상점 아이템 갱신
        /// </summary>
        void RefreshShopItems()
        {
            foreach (var itemUI in shopItemUIs)
            {
                // 구매 가능 여부에 따라 UI 업데이트
                bool canPurchase = ShopManager.Instance.CanPurchaseItem(itemUI.data);

                if (itemUI.button != null)
                {
                    itemUI.button.interactable = canPurchase;
                }

                // 구매 횟수 표시 (제한이 있는 경우)
                if (itemUI.data.hasLimitedPurchases)
                {
                    int count = ShopManager.Instance.GetPurchaseCount(itemUI.data.itemID);
                    if (itemUI.priceText != null && count > 0)
                    {
                        itemUI.priceText.text = $"{itemUI.data.GetPriceString()} ({count}/{itemUI.data.maxPurchases})";
                    }
                }
            }

            // 선택된 아이템이 있으면 상세 정보도 업데이트
            if (selectedItem != null)
            {
                UpdatePurchaseButton(selectedItem);
            }
        }

        /// <summary>
        /// 상점 아이템 초기화
        /// </summary>
        void ClearShopItems()
        {
            foreach (var item in shopItemUIs)
            {
                if (item.gameObject != null)
                    Destroy(item.gameObject);
            }
            shopItemUIs.Clear();
        }

        /// <summary>
        /// 패널 열기
        /// </summary>
        public void Open()
        {
            gameObject.SetActive(true);
            RefreshShopItems();
        }

        /// <summary>
        /// 패널 닫기
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 상세 패널 닫기
        /// </summary>
        public void CloseDetail()
        {
            if (detailPanel != null)
            {
                detailPanel.SetActive(false);
            }
            selectedItem = null;
        }
    }
}
