using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PunchKing
{
    /// <summary>
    /// 설정 패널
    /// 사운드, 음악, 기타 옵션 관리
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [Header("사운드 설정")]
        public Toggle soundToggle;
        public Slider soundVolumeSlider;
        public TextMeshProUGUI soundVolumeText;

        [Header("음악 설정")]
        public Toggle musicToggle;
        public Slider musicVolumeSlider;
        public TextMeshProUGUI musicVolumeText;

        [Header("게임 설정")]
        public Toggle batterySaveModeToggle;
        public TMP_Dropdown qualityDropdown;
        public Toggle autoSaveToggle;

        [Header("계정")]
        public Button resetButton;
        public Button deleteDataButton;

        void Start()
        {
            LoadSettings();
            SetupUI();
        }

        /// <summary>
        /// UI 설정
        /// </summary>
        void SetupUI()
        {
            // 사운드
            if (soundToggle != null)
            {
                soundToggle.onValueChanged.AddListener(OnSoundToggle);
            }

            if (soundVolumeSlider != null)
            {
                soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
            }

            // 음악
            if (musicToggle != null)
            {
                musicToggle.onValueChanged.AddListener(OnMusicToggle);
            }

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            // 배터리 절약 모드
            if (batterySaveModeToggle != null)
            {
                batterySaveModeToggle.onValueChanged.AddListener(OnBatterySaveToggle);
            }

            // 품질 설정
            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "낮음",
                    "중간",
                    "높음"
                });
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }

            // 리셋 버튼
            if (resetButton != null)
            {
                resetButton.onClick.AddListener(OnResetClicked);
            }

            // 데이터 삭제 버튼
            if (deleteDataButton != null)
            {
                deleteDataButton.onClick.AddListener(OnDeleteDataClicked);
            }
        }

        /// <summary>
        /// 설정 로드
        /// </summary>
        void LoadSettings()
        {
            if (AudioManager.Instance != null)
            {
                if (soundToggle != null)
                    soundToggle.isOn = AudioManager.Instance.soundEnabled;

                if (soundVolumeSlider != null)
                    soundVolumeSlider.value = AudioManager.Instance.soundVolume;

                if (musicToggle != null)
                    musicToggle.isOn = AudioManager.Instance.musicEnabled;

                if (musicVolumeSlider != null)
                    musicVolumeSlider.value = AudioManager.Instance.musicVolume;
            }

            UpdateVolumeTexts();
        }

        /// <summary>
        /// 사운드 토글
        /// </summary>
        void OnSoundToggle(bool isOn)
        {
            AudioManager.Instance?.ToggleSound(isOn);
        }

        /// <summary>
        /// 사운드 볼륨 변경
        /// </summary>
        void OnSoundVolumeChanged(float value)
        {
            AudioManager.Instance?.SetSoundVolume(value);
            UpdateVolumeTexts();
        }

        /// <summary>
        /// 음악 토글
        /// </summary>
        void OnMusicToggle(bool isOn)
        {
            AudioManager.Instance?.ToggleMusic(isOn);
        }

        /// <summary>
        /// 음악 볼륨 변경
        /// </summary>
        void OnMusicVolumeChanged(float value)
        {
            AudioManager.Instance?.SetMusicVolume(value);
            UpdateVolumeTexts();
        }

        /// <summary>
        /// 배터리 절약 모드 토글
        /// </summary>
        void OnBatterySaveToggle(bool isOn)
        {
            var optimizer = FindObjectOfType<MobileOptimization>();
            if (optimizer != null)
            {
                optimizer.ToggleBatterySaveMode(isOn);
            }
        }

        /// <summary>
        /// 품질 설정 변경
        /// </summary>
        void OnQualityChanged(int index)
        {
            QualitySettings.SetQualityLevel(index);
            Debug.Log($"품질 설정: {index}");
        }

        /// <summary>
        /// 볼륨 텍스트 업데이트
        /// </summary>
        void UpdateVolumeTexts()
        {
            if (soundVolumeText != null && soundVolumeSlider != null)
            {
                soundVolumeText.text = $"{(soundVolumeSlider.value * 100):F0}%";
            }

            if (musicVolumeText != null && musicVolumeSlider != null)
            {
                musicVolumeText.text = $"{(musicVolumeSlider.value * 100):F0}%";
            }
        }

        /// <summary>
        /// 리셋 버튼
        /// </summary>
        void OnResetClicked()
        {
            // 확인 팝업 표시
            ShowConfirmation("게임을 리셋하시겠습니까?\n(프레스티지는 유지됩니다)", () =>
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ResetGameForPrestige();
                }
            });
        }

        /// <summary>
        /// 데이터 삭제 버튼
        /// </summary>
        void OnDeleteDataClicked()
        {
            // 확인 팝업 표시
            ShowConfirmation("모든 데이터를 삭제하시겠습니까?\n이 작업은 되돌릴 수 없습니다!", () =>
            {
                SaveManager.Instance?.DeleteSaveFile();
                Application.Quit();
            });
        }

        /// <summary>
        /// 확인 팝업 표시
        /// </summary>
        void ShowConfirmation(string message, System.Action onConfirm)
        {
            // TODO: 확인 팝업 UI 구현
            bool confirm = true; // 임시
            if (confirm)
            {
                onConfirm?.Invoke();
            }
        }

        /// <summary>
        /// 패널 열기
        /// </summary>
        public void Open()
        {
            gameObject.SetActive(true);
            LoadSettings();
        }

        /// <summary>
        /// 패널 닫기
        /// </summary>
        public void Close()
        {
            gameObject.SetActive(false);
            SaveManager.Instance?.SaveGame();
        }
    }
}
