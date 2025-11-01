using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PunchKing
{
    /// <summary>
    /// 게임 저장/로드 관리
    /// 바이너리 직렬화를 사용하여 데이터 저장
    /// 오프라인 수익 계산 포함
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        /// <summary>
        /// 저장할 모든 게임 데이터
        /// </summary>
        [System.Serializable]
        public class SaveData
        {
            public string version = "1.0.0";
            public long lastSaveTime;  // Unix timestamp

            // 게임 상태
            public int currentStage;
            public string totalGold;  // BigNumber를 문자열로 저장
            public string punchPower;
            public float critChance;
            public float critMultiplier;
            public int clickCount;
            public int totalKillCount;

            // 업그레이드 레벨
            public int[] upgradeLevels;

            // 프레스티지
            public int prestigeCount;
            public string prestigeCurrency;

            // 스킬 쿨다운 (저장 시점의 남은 시간)
            public float[] skillCooldowns;

            // 코치
            public int activeCoachId;
            public bool[] unlockedCoaches;

            // 설정
            public bool soundEnabled = true;
            public bool musicEnabled = true;
            public float soundVolume = 1.0f;
            public float musicVolume = 0.7f;

            // 통계
            public int totalPlayTime;  // 초 단위
        }

        private string savePath;
        private const string SAVE_FILE = "punchking_save.dat";

        // 오프라인 수익 설정
        [Header("오프라인 수익 설정")]
        [SerializeField] private float offlineEarningsEfficiency = 0.5f;  // 50% 효율
        [SerializeField] private int maxOfflineHours = 8;  // 최대 8시간

        void Awake()
        {
            // 싱글톤
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            // 저장 경로 설정
            savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE);

            // iOS 클라우드 백업 제외 설정
#if UNITY_IOS
            UnityEngine.iOS.Device.SetNoBackupFlag(savePath);
#endif

            Debug.Log($"Save path: {savePath}");
        }

        /// <summary>
        /// 게임 저장
        /// </summary>
        public void SaveGame()
        {
            try
            {
                SaveData data = CreateSaveData();

                // 바이너리 직렬화
                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = new FileStream(savePath, FileMode.Create);
                bf.Serialize(stream, data);
                stream.Close();

                Debug.Log("게임 저장 완료!");

                // 클라우드 저장 (옵션)
#if UNITY_SERVICES_CLOUD_SAVE
                SaveToCloud();
#endif
            }
            catch (Exception e)
            {
                Debug.LogError($"저장 실패: {e.Message}");
            }
        }

        /// <summary>
        /// 현재 게임 상태로부터 SaveData 생성
        /// </summary>
        SaveData CreateSaveData()
        {
            SaveData data = new SaveData();

            // 현재 시간
            data.lastSaveTime = DateTimeOffset.Now.ToUnixTimeSeconds();

            // 게임 매니저 데이터
            if (GameManager.Instance != null)
            {
                data.currentStage = GameManager.Instance.currentStage;
                data.totalGold = GameManager.Instance.totalGold.ToSaveString();
                data.punchPower = GameManager.Instance.punchPower.ToSaveString();
                data.critChance = GameManager.Instance.critChance;
                data.critMultiplier = GameManager.Instance.critMultiplier;
                data.clickCount = GameManager.Instance.clickCount;
                data.totalKillCount = GameManager.Instance.totalKillCount;
            }

            // 업그레이드 데이터
            if (UpgradeManager.Instance != null)
            {
                data.upgradeLevels = UpgradeManager.Instance.GetAllUpgradeLevels();
            }

            // 프레스티지 데이터
            if (PrestigeManager.Instance != null)
            {
                data.prestigeCount = PrestigeManager.Instance.prestigeCount;
                data.prestigeCurrency = PrestigeManager.Instance.prestigeCurrency.ToSaveString();
            }

            // 스킬 쿨다운
            if (SkillManager.Instance != null)
            {
                data.skillCooldowns = SkillManager.Instance.GetSkillCooldowns();
            }

            // 코치 데이터
            if (CoachSystem.Instance != null)
            {
                data.activeCoachId = CoachSystem.Instance.GetActiveCoachId();
                data.unlockedCoaches = CoachSystem.Instance.GetUnlockedCoaches();
            }

            // 설정 데이터
            if (AudioManager.Instance != null)
            {
                data.soundEnabled = AudioManager.Instance.soundEnabled;
                data.musicEnabled = AudioManager.Instance.musicEnabled;
                data.soundVolume = AudioManager.Instance.soundVolume;
                data.musicVolume = AudioManager.Instance.musicVolume;
            }

            return data;
        }

        /// <summary>
        /// 게임 로드
        /// </summary>
        public bool LoadGame()
        {
            if (!File.Exists(savePath))
            {
                Debug.Log("저장 파일이 없습니다. 새 게임을 시작합니다.");
                return false;
            }

            try
            {
                // 바이너리 역직렬화
                BinaryFormatter bf = new BinaryFormatter();
                FileStream stream = new FileStream(savePath, FileMode.Open);
                SaveData data = bf.Deserialize(stream) as SaveData;
                stream.Close();

                if (data == null)
                {
                    Debug.LogError("저장 데이터 로드 실패");
                    return false;
                }

                // 데이터 복원
                ApplySaveData(data);

                // 오프라인 수익 계산
                long currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                long offlineTime = currentTime - data.lastSaveTime;
                CalculateOfflineEarnings(offlineTime);

                Debug.Log($"게임 로드 완료! (오프라인: {offlineTime}초)");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"로드 실패: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// SaveData를 게임에 적용
        /// </summary>
        void ApplySaveData(SaveData data)
        {
            // 게임 매니저 복원
            if (GameManager.Instance != null)
            {
                GameManager.Instance.currentStage = data.currentStage;
                GameManager.Instance.totalGold = BigNumber.FromSaveString(data.totalGold);
                GameManager.Instance.punchPower = BigNumber.FromSaveString(data.punchPower);
                GameManager.Instance.critChance = data.critChance;
                GameManager.Instance.critMultiplier = data.critMultiplier;
                GameManager.Instance.clickCount = data.clickCount;
                GameManager.Instance.totalKillCount = data.totalKillCount;
            }

            // 업그레이드 복원
            if (UpgradeManager.Instance != null && data.upgradeLevels != null)
            {
                UpgradeManager.Instance.LoadUpgradeLevels(data.upgradeLevels);
            }

            // 프레스티지 복원
            if (PrestigeManager.Instance != null)
            {
                PrestigeManager.Instance.prestigeCount = data.prestigeCount;
                PrestigeManager.Instance.prestigeCurrency = BigNumber.FromSaveString(data.prestigeCurrency);
            }

            // 스킬 쿨다운 복원 (이미 지난 시간은 0으로)
            if (SkillManager.Instance != null && data.skillCooldowns != null)
            {
                SkillManager.Instance.LoadSkillCooldowns(data.skillCooldowns);
            }

            // 코치 복원
            if (CoachSystem.Instance != null)
            {
                if (data.unlockedCoaches != null)
                {
                    CoachSystem.Instance.LoadCoachData(data.activeCoachId, data.unlockedCoaches);
                }
            }

            // 설정 복원
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.soundEnabled = data.soundEnabled;
                AudioManager.Instance.musicEnabled = data.musicEnabled;
                AudioManager.Instance.soundVolume = data.soundVolume;
                AudioManager.Instance.musicVolume = data.musicVolume;
                AudioManager.Instance.ApplySettings();
            }
        }

        /// <summary>
        /// 오프라인 수익 계산
        /// </summary>
        void CalculateOfflineEarnings(long offlineSeconds)
        {
            if (offlineSeconds <= 0) return;

            // 최대 시간 제한
            long maxSeconds = maxOfflineHours * 3600;
            offlineSeconds = Math.Min(offlineSeconds, maxSeconds);

            // 초당 골드가 0이면 수익 없음
            if (GameManager.Instance.goldPerSecond.IsZero())
            {
                Debug.Log("초당 골드가 0이므로 오프라인 수익이 없습니다.");
                return;
            }

            // 오프라인 수익 = 초당골드 × 오프라인시간 × 효율
            BigNumber offlineGold = GameManager.Instance.goldPerSecond
                .Multiply(offlineSeconds)
                .Multiply(offlineEarningsEfficiency);

            // 골드 지급
            GameManager.Instance.AddGold(offlineGold);

            // UI 팝업 표시
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowOfflineEarningsPopup(offlineGold, offlineSeconds);
            }

            Debug.Log($"오프라인 수익: {offlineGold.ToKoreanString()} ({offlineSeconds}초)");
        }

        /// <summary>
        /// 저장 파일 삭제 (게임 리셋)
        /// </summary>
        public void DeleteSaveFile()
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("저장 파일 삭제됨");
            }
        }

        /// <summary>
        /// 저장 파일 존재 여부
        /// </summary>
        public bool SaveFileExists()
        {
            return File.Exists(savePath);
        }

#if UNITY_SERVICES_CLOUD_SAVE
        /// <summary>
        /// Unity Cloud Save에 저장 (선택적)
        /// </summary>
        async void SaveToCloud()
        {
            try
            {
                var data = new Dictionary<string, object>
                {
                    { "saveData", CreateSaveData() }
                };

                await CloudSaveService.Instance.Data.ForceSaveAsync(data);
                Debug.Log("클라우드 저장 완료");
            }
            catch (Exception e)
            {
                Debug.LogError($"클라우드 저장 실패: {e.Message}");
            }
        }

        /// <summary>
        /// Unity Cloud Save에서 로드
        /// </summary>
        async void LoadFromCloud()
        {
            try
            {
                var data = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "saveData" });

                if (data.TryGetValue("saveData", out var saveDataObj))
                {
                    SaveData saveData = saveDataObj as SaveData;
                    if (saveData != null)
                    {
                        ApplySaveData(saveData);
                        Debug.Log("클라우드 로드 완료");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"클라우드 로드 실패: {e.Message}");
            }
        }
#endif

#if UNITY_EDITOR
        // 에디터 디버그
        [ContextMenu("Delete Save File")]
        void EditorDeleteSave()
        {
            DeleteSaveFile();
            Debug.Log("저장 파일 삭제됨");
        }

        [ContextMenu("Show Save Path")]
        void ShowSavePath()
        {
            Debug.Log($"저장 경로: {savePath}");
        }
#endif
    }
}
