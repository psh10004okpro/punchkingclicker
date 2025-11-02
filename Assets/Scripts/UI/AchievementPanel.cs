using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

namespace PunchKing
{
    /// <summary>
    /// 업적 UI 패널
    /// 업적 목록 표시 및 진행도 추적
    /// </summary>
    public class AchievementPanel : MonoBehaviour
    {
        [Header("업적 목록")]
        public Transform achievementContainer;
        public GameObject achievementItemPrefab;

        [Header("필터")]
        public Toggle filterAll;
        public Toggle filterCompleted;
        public Toggle filterInProgress;

        [Header("통계")]
        public TextMeshProUGUI totalProgressText;
        public ProgressBar totalProgressBar;
        public TextMeshProUGUI completedCountText;

        [Header("정렬")]
        public TMP_Dropdown sortDropdown;

        private List<AchievementItem> achievementItems = new List<AchievementItem>();
        private FilterType currentFilter = FilterType.All;
        private SortType currentSort = SortType.Progress;

        public enum FilterType
        {
            All,
            Completed,
            InProgress
        }

        public enum SortType
        {
            Progress,      // 진행도 순
            Type,          // 타입 순
            Completion     // 완료 우선
        }

        /// <summary>
        /// 업적 아이템 클래스
        /// </summary>
        [System.Serializable]
        public class AchievementItem
        {
            public GameObject gameObject;
            public TextMeshProUGUI nameText;
            public TextMeshProUGUI descriptionText;
            public TextMeshProUGUI progressText;
            public ProgressBar progressBar;
            public Image completedIcon;
            public Button claimButton;
            public AchievementData data;
            public int tier;

            public bool IsCompleted
            {
                get
                {
                    if (AchievementManager.Instance == null || data == null) return false;
                    return AchievementManager.Instance.IsAchievementCompleted(data.achievementId);
                }
            }

            public float Progress
            {
                get
                {
                    if (AchievementManager.Instance == null || data == null) return 0f;
                    long current = AchievementManager.Instance.GetProgress(data.type);
                    long target = data.targetValue;
                    return target > 0 ? Mathf.Clamp01((float)current / target) : 0f;
                }
            }
        }

        void Start()
        {
            SetupFilters();
            SetupSort();
            GenerateAchievementList();
        }

        void OnEnable()
        {
            UpdateAllAchievements();
            UpdateStatistics();
        }

        /// <summary>
        /// 필터 설정
        /// </summary>
        void SetupFilters()
        {
            if (filterAll != null)
                filterAll.onValueChanged.AddListener((isOn) => { if (isOn) OnFilterChanged(FilterType.All); });

            if (filterCompleted != null)
                filterCompleted.onValueChanged.AddListener((isOn) => { if (isOn) OnFilterChanged(FilterType.Completed); });

            if (filterInProgress != null)
                filterInProgress.onValueChanged.AddListener((isOn) => { if (isOn) OnFilterChanged(FilterType.InProgress); });
        }

        /// <summary>
        /// 정렬 설정
        /// </summary>
        void SetupSort()
        {
            if (sortDropdown != null)
            {
                sortDropdown.ClearOptions();
                sortDropdown.AddOptions(new List<string>
                {
                    "진행도 순",
                    "타입 순",
                    "완료 우선"
                });
                sortDropdown.onValueChanged.AddListener(OnSortChanged);
            }
        }

        /// <summary>
        /// 업적 목록 생성
        /// </summary>
        void GenerateAchievementList()
        {
            if (AchievementManager.Instance == null || achievementContainer == null || achievementItemPrefab == null)
                return;

            // 기존 아이템 제거
            ClearAchievementList();

            // 모든 업적 데이터에 대해 아이템 생성
            var achievementDatas = AchievementManager.Instance.achievements;

            foreach (var data in achievementDatas)
            {
                CreateAchievementItem(data);
            }

            // 초기 정렬 및 필터 적용
            ApplySortAndFilter();
        }

        /// <summary>
        /// 업적 아이템 생성
        /// </summary>
        void CreateAchievementItem(AchievementData data)
        {
            GameObject itemObj = Instantiate(achievementItemPrefab, achievementContainer);
            AchievementItem item = new AchievementItem
            {
                gameObject = itemObj,
                data = data,
                tier = data.tier
            };

            // UI 컴포넌트 참조
            item.nameText = itemObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            item.descriptionText = itemObj.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
            item.progressText = itemObj.transform.Find("ProgressText")?.GetComponent<TextMeshProUGUI>();
            item.progressBar = itemObj.transform.Find("ProgressBar")?.GetComponent<ProgressBar>();
            item.completedIcon = itemObj.transform.Find("CompletedIcon")?.GetComponent<Image>();
            item.claimButton = itemObj.transform.Find("ClaimButton")?.GetComponent<Button>();

            // 텍스트 설정
            if (item.nameText != null)
                item.nameText.text = data.achievementName;

            if (item.descriptionText != null)
                item.descriptionText.text = data.description;

            // 보상 클레임 버튼 (업적은 완료 시 즉시 보상이 지급되므로 클레임 버튼 숨김)
            if (item.claimButton != null)
            {
                item.claimButton.gameObject.SetActive(false);
            }

            achievementItems.Add(item);
            UpdateAchievementItem(item);
        }

        /// <summary>
        /// 업적 아이템 업데이트
        /// </summary>
        void UpdateAchievementItem(AchievementItem item)
        {
            if (item == null || item.data == null || AchievementManager.Instance == null)
                return;

            long currentProgress = AchievementManager.Instance.GetProgress(item.data.achievementType);
            long targetValue = item.data.GetTargetValue(item.tier);
            bool isCompleted = item.IsCompleted;

            // 진행도 바
            if (item.progressBar != null)
            {
                item.progressBar.SetProgress(currentProgress, targetValue);
            }

            // 진행도 텍스트
            if (item.progressText != null)
            {
                if (isCompleted)
                {
                    item.progressText.text = "<color=green>완료!</color>";
                }
                else
                {
                    item.progressText.text = $"{currentProgress:N0} / {targetValue:N0}";
                }
            }

            // 완료 아이콘
            if (item.completedIcon != null)
            {
                item.completedIcon.gameObject.SetActive(isCompleted);
            }

            // 완료된 업적은 회색으로
            if (isCompleted)
            {
                if (item.nameText != null)
                    item.nameText.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }

        /// <summary>
        /// 모든 업적 업데이트
        /// </summary>
        void UpdateAllAchievements()
        {
            foreach (var item in achievementItems)
            {
                UpdateAchievementItem(item);
            }
        }

        /// <summary>
        /// 필터 변경
        /// </summary>
        void OnFilterChanged(FilterType filter)
        {
            currentFilter = filter;
            ApplySortAndFilter();
        }

        /// <summary>
        /// 정렬 변경
        /// </summary>
        void OnSortChanged(int index)
        {
            currentSort = (SortType)index;
            ApplySortAndFilter();
        }

        /// <summary>
        /// 정렬 및 필터 적용
        /// </summary>
        void ApplySortAndFilter()
        {
            // 필터링
            var filteredItems = achievementItems.Where(item =>
            {
                switch (currentFilter)
                {
                    case FilterType.Completed:
                        return item.IsCompleted;
                    case FilterType.InProgress:
                        return !item.IsCompleted;
                    case FilterType.All:
                    default:
                        return true;
                }
            }).ToList();

            // 정렬
            switch (currentSort)
            {
                case SortType.Progress:
                    filteredItems = filteredItems.OrderByDescending(i => i.Progress).ToList();
                    break;
                case SortType.Type:
                    filteredItems = filteredItems.OrderBy(i => i.data.type).ThenBy(i => i.tier).ToList();
                    break;
                case SortType.Completion:
                    filteredItems = filteredItems.OrderByDescending(i => i.IsCompleted).ThenByDescending(i => i.Progress).ToList();
                    break;
            }

            // UI 순서 재배치
            for (int i = 0; i < achievementItems.Count; i++)
            {
                var item = achievementItems[i];
                bool shouldShow = filteredItems.Contains(item);
                item.gameObject.SetActive(shouldShow);

                if (shouldShow)
                {
                    int sortIndex = filteredItems.IndexOf(item);
                    item.gameObject.transform.SetSiblingIndex(sortIndex);
                }
            }
        }

        /// <summary>
        /// 통계 업데이트
        /// </summary>
        void UpdateStatistics()
        {
            if (AchievementManager.Instance == null) return;

            int totalCount = achievementItems.Count;
            int completedCount = achievementItems.Count(i => i.IsCompleted);
            float progressPercent = totalCount > 0 ? (float)completedCount / totalCount : 0f;

            // 완료 수
            if (completedCountText != null)
            {
                completedCountText.text = $"완료: {completedCount} / {totalCount}";
            }

            // 전체 진행도
            if (totalProgressText != null)
            {
                totalProgressText.text = $"전체 진행도: {progressPercent * 100f:F1}%";
            }

            if (totalProgressBar != null)
            {
                totalProgressBar.SetProgress(completedCount, totalCount);
            }
        }

        /// <summary>
        /// 업적 목록 초기화
        /// </summary>
        void ClearAchievementList()
        {
            foreach (var item in achievementItems)
            {
                if (item.gameObject != null)
                    Destroy(item.gameObject);
            }
            achievementItems.Clear();
        }

        /// <summary>
        /// 패널 열기
        /// </summary>
        public void Open()
        {
            gameObject.SetActive(true);
            UpdateAllAchievements();
            UpdateStatistics();
        }

        /// <summary>
        /// 패널 닫기
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }

        void Update()
        {
            // 0.5초마다 업적 갱신
            if (Time.frameCount % 30 == 0 && gameObject.activeSelf)
            {
                UpdateAllAchievements();
                UpdateStatistics();
            }
        }
    }
}
