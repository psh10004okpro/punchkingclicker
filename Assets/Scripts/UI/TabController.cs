using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace PunchKing
{
    /// <summary>
    /// 탭 전환 시스템
    /// 업그레이드, 스킬, 코치 등 패널 전환
    /// </summary>
    public class TabController : MonoBehaviour
    {
        [System.Serializable]
        public class Tab
        {
            public string tabName;
            public Button tabButton;
            public GameObject tabPanel;
            public Color activeColor = Color.white;
            public Color inactiveColor = Color.gray;
        }

        [Header("탭 목록")]
        public List<Tab> tabs = new List<Tab>();

        [Header("설정")]
        public int defaultTabIndex = 0;

        private int currentTabIndex = 0;

        void Start()
        {
            SetupTabs();
            SelectTab(defaultTabIndex);
        }

        /// <summary>
        /// 탭 설정
        /// </summary>
        void SetupTabs()
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                int index = i; // 클로저 문제 해결
                Tab tab = tabs[i];

                if (tab.tabButton != null)
                {
                    tab.tabButton.onClick.AddListener(() => SelectTab(index));
                }
            }
        }

        /// <summary>
        /// 탭 선택
        /// </summary>
        public void SelectTab(int index)
        {
            if (index < 0 || index >= tabs.Count) return;

            currentTabIndex = index;

            for (int i = 0; i < tabs.Count; i++)
            {
                Tab tab = tabs[i];
                bool isActive = (i == index);

                // 패널 활성화/비활성화
                if (tab.tabPanel != null)
                {
                    tab.tabPanel.SetActive(isActive);
                }

                // 버튼 색상 변경
                if (tab.tabButton != null)
                {
                    var colors = tab.tabButton.colors;
                    colors.normalColor = isActive ? tab.activeColor : tab.inactiveColor;
                    tab.tabButton.colors = colors;

                    // 버튼 활성화 상태
                    tab.tabButton.interactable = !isActive;
                }
            }
        }

        /// <summary>
        /// 탭 이름으로 선택
        /// </summary>
        public void SelectTabByName(string tabName)
        {
            int index = tabs.FindIndex(t => t.tabName == tabName);
            if (index >= 0)
            {
                SelectTab(index);
            }
        }

        /// <summary>
        /// 다음 탭
        /// </summary>
        public void NextTab()
        {
            int next = (currentTabIndex + 1) % tabs.Count;
            SelectTab(next);
        }

        /// <summary>
        /// 이전 탭
        /// </summary>
        public void PreviousTab()
        {
            int prev = (currentTabIndex - 1 + tabs.Count) % tabs.Count;
            SelectTab(prev);
        }

#if UNITY_EDITOR
        [ContextMenu("Setup Tabs Automatically")]
        void AutoSetupTabs()
        {
            tabs.Clear();

            // 자식 오브젝트에서 버튼 찾기
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (var button in buttons)
            {
                Tab tab = new Tab();
                tab.tabName = button.name;
                tab.tabButton = button;

                // 같은 이름의 패널 찾기
                GameObject panel = GameObject.Find(button.name + "Panel");
                if (panel != null)
                {
                    tab.tabPanel = panel;
                }

                tabs.Add(tab);
            }

            Debug.Log($"자동으로 {tabs.Count}개 탭 설정됨");
        }
#endif
    }
}
