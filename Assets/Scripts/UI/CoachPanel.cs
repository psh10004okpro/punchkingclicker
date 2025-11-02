using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace PunchKing
{
    /// <summary>
    /// 코치 선택 UI 패널
    /// </summary>
    public class CoachPanel : MonoBehaviour
    {
        [Header("UI")]
        public Transform coachButtonContainer;
        public GameObject coachButtonPrefab;

        [Header("선택된 코치 정보")]
        public Image selectedCoachPortrait;
        public TextMeshProUGUI selectedCoachName;
        public TextMeshProUGUI selectedCoachDescription;
        public TextMeshProUGUI selectedCoachBonus;

        private List<CoachButton> coachButtons = new List<CoachButton>();

        void Start()
        {
            PopulateCoaches();
        }

        /// <summary>
        /// 코치 버튼 생성
        /// </summary>
        void PopulateCoaches()
        {
            if (CoachSystem.Instance == null) return;

            ClearCoachButtons();

            foreach (var coach in CoachSystem.Instance.allCoaches)
            {
                CreateCoachButton(coach);
            }

            UpdateSelectedCoachInfo();
        }

        /// <summary>
        /// 코치 버튼 생성
        /// </summary>
        void CreateCoachButton(CoachSystem.Coach coach)
        {
            if (coachButtonPrefab == null || coachButtonContainer == null) return;

            GameObject buttonObj = Instantiate(coachButtonPrefab, coachButtonContainer);
            CoachButton coachButton = buttonObj.AddComponent<CoachButton>();
            coachButton.Setup(coach, this);

            coachButtons.Add(coachButton);
        }

        /// <summary>
        /// 기존 버튼 제거
        /// </summary>
        void ClearCoachButtons()
        {
            foreach (var button in coachButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            coachButtons.Clear();
        }

        /// <summary>
        /// 선택된 코치 정보 업데이트
        /// </summary>
        public void UpdateSelectedCoachInfo()
        {
            if (CoachSystem.Instance == null) return;

            int activeId = CoachSystem.Instance.GetActiveCoachId();
            if (activeId < 0) return;

            var coach = CoachSystem.Instance.allCoaches.Find(c => c.id == activeId);
            if (coach == null) return;

            if (selectedCoachPortrait != null && coach.portrait != null)
            {
                selectedCoachPortrait.sprite = coach.portrait;
            }

            if (selectedCoachName != null)
            {
                selectedCoachName.text = coach.coachName;
            }

            if (selectedCoachDescription != null)
            {
                selectedCoachDescription.text = coach.description;
            }

            if (selectedCoachBonus != null)
            {
                string bonus = "";
                if (coach.damageBonus > 0)
                    bonus += $"데미지 +{coach.damageBonus * 100}%\n";
                if (coach.goldBonus > 0)
                    bonus += $"골드 +{coach.goldBonus * 100}%\n";
                if (coach.critBonus > 0)
                    bonus += $"크리티컬 +{coach.critBonus * 100}%";

                selectedCoachBonus.text = bonus;
            }
        }

        /// <summary>
        /// 코치 버튼 클래스
        /// </summary>
        public class CoachButton : MonoBehaviour
        {
            private CoachSystem.Coach coach;
            private CoachPanel panel;

            private Image portrait;
            private TextMeshProUGUI nameText;
            private TextMeshProUGUI costText;
            private Button button;
            private GameObject lockedIndicator;

            public void Setup(CoachSystem.Coach coachData, CoachPanel coachPanel)
            {
                coach = coachData;
                panel = coachPanel;

                // UI 컴포넌트 가져오기
                portrait = transform.Find("Portrait")?.GetComponent<Image>();
                nameText = transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                costText = transform.Find("Cost")?.GetComponent<TextMeshProUGUI>();
                button = GetComponent<Button>();
                lockedIndicator = transform.Find("Locked")?.gameObject;

                UpdateUI();

                if (button != null)
                {
                    button.onClick.AddListener(OnClicked);
                }
            }

            void UpdateUI()
            {
                if (coach == null) return;

                // 초상화
                if (portrait != null && coach.portrait != null)
                {
                    portrait.sprite = coach.portrait;
                }

                // 이름
                if (nameText != null)
                {
                    nameText.text = coach.coachName;
                }

                // 잠금 상태
                if (coach.isUnlocked)
                {
                    if (costText != null)
                    {
                        costText.text = "선택";
                    }

                    if (lockedIndicator != null)
                    {
                        lockedIndicator.SetActive(false);
                    }

                    if (button != null)
                    {
                        button.interactable = true;
                    }
                }
                else
                {
                    if (costText != null)
                    {
                        costText.text = $"💰 {coach.unlockCost.ToKoreanString()}";
                    }

                    if (lockedIndicator != null)
                    {
                        lockedIndicator.SetActive(true);
                    }

                    // 구매 가능 여부
                    bool canAfford = GameManager.Instance != null &&
                                    GameManager.Instance.totalGold.IsGreaterThanOrEqual(coach.unlockCost);

                    if (button != null)
                    {
                        button.interactable = canAfford;
                    }
                }
            }

            void OnClicked()
            {
                if (coach == null) return;

                if (coach.isUnlocked)
                {
                    // 활성화
                    CoachSystem.Instance?.SetActiveCoach(coach.id);
                    panel?.UpdateSelectedCoachInfo();
                }
                else
                {
                    // 잠금 해제 시도
                    bool success = CoachSystem.Instance?.UnlockCoach(coach.id) ?? false;
                    if (success)
                    {
                        UpdateUI();
                    }
                }
            }
        }

        /// <summary>
        /// 패널 열기
        /// </summary>
        public void Open()
        {
            gameObject.SetActive(true);
            PopulateCoaches();
        }

        /// <summary>
        /// 패널 닫기
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
