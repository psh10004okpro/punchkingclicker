using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace PunchKing
{
    /// <summary>
    /// 업그레이드 패널 관리
    /// 모든 업그레이드를 표시하고 카테고리별 필터링
    /// </summary>
    public class UpgradePanel : MonoBehaviour
    {
        [Header("UI 레퍼런스")]
        public Transform upgradeButtonContainer;
        public GameObject upgradeButtonPrefab;
        public ScrollRect scrollRect;

        [Header("카테고리 필터")]
        public Toggle allToggle;
        public Toggle damageToggle;
        public Toggle goldToggle;
        public Toggle autoToggle;

        [Header("정렬")]
        public TMP_Dropdown sortDropdown;

        private List<UpgradeButton> upgradeButtons = new List<UpgradeButton>();
        private UpgradeData.UpgradeType currentFilter = UpgradeData.UpgradeType.PunchPower;
        private bool showAll = true;

        public enum SortMode
        {
            Default,        // 기본 순서
            CostAscending,  // 비용 오름차순
            CostDescending, // 비용 내림차순
            Level           // 레벨 순
        }

        private SortMode currentSortMode = SortMode.Default;

        void Start()
        {
            InitializeUI();
            PopulateUpgrades();
            SetupFilters();
            SetupSorting();

            // 업그레이드 구매 이벤트 구독
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnUpgradePurchased.AddListener(OnUpgradePurchased);
            }
        }

        /// <summary>
        /// UI 초기화
        /// </summary>
        void InitializeUI()
        {
            if (upgradeButtonContainer == null)
            {
                Debug.LogError("UpgradePanel: upgradeButtonContainer가 설정되지 않았습니다!");
                return;
            }

            if (upgradeButtonPrefab == null)
            {
                Debug.LogError("UpgradePanel: upgradeButtonPrefab이 설정되지 않았습니다!");
                return;
            }
        }

        /// <summary>
        /// 업그레이드 버튼 생성
        /// </summary>
        void PopulateUpgrades()
        {
            if (UpgradeManager.Instance == null) return;

            // 기존 버튼 제거
            ClearUpgradeButtons();

            List<UpgradeData> upgrades = GetFilteredUpgrades();
            upgrades = SortUpgrades(upgrades);

            // 버튼 생성
            foreach (var upgradeData in upgrades)
            {
                CreateUpgradeButton(upgradeData);
            }
        }

        /// <summary>
        /// 업그레이드 버튼 생성
        /// </summary>
        void CreateUpgradeButton(UpgradeData upgradeData)
        {
            GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeButtonContainer);
            UpgradeButton button = buttonObj.GetComponent<UpgradeButton>();

            if (button != null)
            {
                int level = UpgradeManager.Instance.GetUpgradeLevel(upgradeData);
                button.Setup(upgradeData, level);
                upgradeButtons.Add(button);
            }
        }

        /// <summary>
        /// 기존 버튼 제거
        /// </summary>
        void ClearUpgradeButtons()
        {
            foreach (var button in upgradeButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            upgradeButtons.Clear();
        }

        /// <summary>
        /// 필터링된 업그레이드 목록
        /// </summary>
        List<UpgradeData> GetFilteredUpgrades()
        {
            if (UpgradeManager.Instance == null)
                return new List<UpgradeData>();

            if (showAll)
            {
                return UpgradeManager.Instance.allUpgrades;
            }
            else
            {
                return UpgradeManager.Instance.GetUpgradesByType(currentFilter);
            }
        }

        /// <summary>
        /// 업그레이드 정렬
        /// </summary>
        List<UpgradeData> SortUpgrades(List<UpgradeData> upgrades)
        {
            List<UpgradeData> sorted = new List<UpgradeData>(upgrades);

            switch (currentSortMode)
            {
                case SortMode.CostAscending:
                    sorted.Sort((a, b) =>
                    {
                        int levelA = UpgradeManager.Instance.GetUpgradeLevel(a);
                        int levelB = UpgradeManager.Instance.GetUpgradeLevel(b);
                        BigNumber costA = a.GetCost(levelA);
                        BigNumber costB = b.GetCost(levelB);
                        return costA.IsGreaterThan(costB) ? 1 : -1;
                    });
                    break;

                case SortMode.CostDescending:
                    sorted.Sort((a, b) =>
                    {
                        int levelA = UpgradeManager.Instance.GetUpgradeLevel(a);
                        int levelB = UpgradeManager.Instance.GetUpgradeLevel(b);
                        BigNumber costA = a.GetCost(levelA);
                        BigNumber costB = b.GetCost(levelB);
                        return costA.IsGreaterThan(costB) ? -1 : 1;
                    });
                    break;

                case SortMode.Level:
                    sorted.Sort((a, b) =>
                    {
                        int levelA = UpgradeManager.Instance.GetUpgradeLevel(a);
                        int levelB = UpgradeManager.Instance.GetUpgradeLevel(b);
                        return levelB.CompareTo(levelA); // 내림차순
                    });
                    break;

                case SortMode.Default:
                default:
                    // 원래 순서 유지
                    break;
            }

            return sorted;
        }

        /// <summary>
        /// 필터 설정
        /// </summary>
        void SetupFilters()
        {
            if (allToggle != null)
            {
                allToggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        showAll = true;
                        PopulateUpgrades();
                    }
                });
                allToggle.isOn = true;
            }

            if (damageToggle != null)
            {
                damageToggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        showAll = false;
                        currentFilter = UpgradeData.UpgradeType.PunchPower;
                        PopulateUpgrades();
                    }
                });
            }

            if (goldToggle != null)
            {
                goldToggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        showAll = false;
                        currentFilter = UpgradeData.UpgradeType.GoldMultiplier;
                        PopulateUpgrades();
                    }
                });
            }

            if (autoToggle != null)
            {
                autoToggle.onValueChanged.AddListener((isOn) =>
                {
                    if (isOn)
                    {
                        showAll = false;
                        currentFilter = UpgradeData.UpgradeType.AutoClicker;
                        PopulateUpgrades();
                    }
                });
            }
        }

        /// <summary>
        /// 정렬 설정
        /// </summary>
        void SetupSorting()
        {
            if (sortDropdown != null)
            {
                sortDropdown.ClearOptions();
                sortDropdown.AddOptions(new List<string>
                {
                    "기본 순서",
                    "비용 낮은 순",
                    "비용 높은 순",
                    "레벨 높은 순"
                });

                sortDropdown.onValueChanged.AddListener((index) =>
                {
                    currentSortMode = (SortMode)index;
                    PopulateUpgrades();
                });
            }
        }

        /// <summary>
        /// 업그레이드 구매 시 호출
        /// </summary>
        void OnUpgradePurchased(UpgradeData upgrade, int newLevel)
        {
            // 해당 버튼만 업데이트 (성능 최적화)
            UpgradeButton button = upgradeButtons.Find(b => b != null && b.name.Contains(upgrade.upgradeName));
            if (button != null)
            {
                button.Setup(upgrade, newLevel);
            }
        }

        /// <summary>
        /// 패널 열기
        /// </summary>
        public void Open()
        {
            gameObject.SetActive(true);
            PopulateUpgrades();

            // 스크롤 위치 초기화
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 1f;
            }
        }

        /// <summary>
        /// 패널 닫기
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 모두 업그레이드 버튼 (디버그/프리미엄 기능)
        /// </summary>
        public void UpgradeAllAffordable()
        {
            if (UpgradeManager.Instance == null) return;

            int upgraded = 0;
            foreach (var upgrade in UpgradeManager.Instance.allUpgrades)
            {
                if (UpgradeManager.Instance.PurchaseUpgrade(upgrade))
                {
                    upgraded++;
                }
            }

            if (upgraded > 0)
            {
                PopulateUpgrades();
                Debug.Log($"{upgraded}개 업그레이드 구매 완료!");
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Refresh Upgrades")]
        void EditorRefresh()
        {
            PopulateUpgrades();
        }
#endif
    }
}
