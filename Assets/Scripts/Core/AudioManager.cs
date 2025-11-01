using UnityEngine;
using System.Collections.Generic;

namespace PunchKing
{
    /// <summary>
    /// 오디오 관리 시스템
    /// 사운드 이펙트와 배경 음악 재생
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("오디오 소스")]
        public AudioSource musicSource;
        public AudioSource sfxSource;

        [Header("사운드 클립")]
        public AudioClip[] soundEffects;

        [Header("음악 클립")]
        public AudioClip backgroundMusic;

        [Header("설정")]
        public bool soundEnabled = true;
        public bool musicEnabled = true;
        public float soundVolume = 1.0f;
        public float musicVolume = 0.7f;

        private Dictionary<string, AudioClip> soundDictionary = new Dictionary<string, AudioClip>();

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
                return;
            }

            InitializeAudioSources();
            LoadSoundClips();
        }

        void Start()
        {
            // 배경 음악 재생
            PlayBackgroundMusic();
        }

        /// <summary>
        /// 오디오 소스 초기화
        /// </summary>
        void InitializeAudioSources()
        {
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
            }

            ApplySettings();
        }

        /// <summary>
        /// 사운드 클립 로드
        /// </summary>
        void LoadSoundClips()
        {
            soundDictionary.Clear();

            // 기본 사운드 이름 매핑
            string[] soundNames = new string[]
            {
                "SandbagHit",
                "SandbagDestroy",
                "BossHit",
                "BossDefeat",
                "BossAppear",
                "UpgradePurchase",
                "SkillActivate",
                "CoachUnlock",
                "Prestige",
                "Click"
            };

            // 사운드 클립을 딕셔너리에 추가
            for (int i = 0; i < soundEffects.Length && i < soundNames.Length; i++)
            {
                if (soundEffects[i] != null)
                {
                    soundDictionary[soundNames[i]] = soundEffects[i];
                }
            }
        }

        /// <summary>
        /// 사운드 재생
        /// </summary>
        public void PlaySound(string soundName)
        {
            if (!soundEnabled || sfxSource == null) return;

            if (soundDictionary.ContainsKey(soundName))
            {
                sfxSource.PlayOneShot(soundDictionary[soundName], soundVolume);
            }
            else
            {
                Debug.LogWarning($"사운드 '{soundName}'을(를) 찾을 수 없습니다");
            }
        }

        /// <summary>
        /// 배경 음악 재생
        /// </summary>
        public void PlayBackgroundMusic()
        {
            if (!musicEnabled || musicSource == null || backgroundMusic == null) return;

            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }

        /// <summary>
        /// 배경 음악 정지
        /// </summary>
        public void StopBackgroundMusic()
        {
            if (musicSource != null)
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// 설정 적용
        /// </summary>
        public void ApplySettings()
        {
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
                musicSource.mute = !musicEnabled;
            }

            if (sfxSource != null)
            {
                sfxSource.volume = soundVolume;
                sfxSource.mute = !soundEnabled;
            }
        }

        /// <summary>
        /// 사운드 켜기/끄기
        /// </summary>
        public void ToggleSound(bool enabled)
        {
            soundEnabled = enabled;
            ApplySettings();
        }

        /// <summary>
        /// 음악 켜기/끄기
        /// </summary>
        public void ToggleMusic(bool enabled)
        {
            musicEnabled = enabled;

            if (enabled && musicSource != null && !musicSource.isPlaying)
            {
                PlayBackgroundMusic();
            }
            else if (!enabled && musicSource != null)
            {
                StopBackgroundMusic();
            }

            ApplySettings();
        }

        /// <summary>
        /// 사운드 볼륨 설정
        /// </summary>
        public void SetSoundVolume(float volume)
        {
            soundVolume = Mathf.Clamp01(volume);
            ApplySettings();
        }

        /// <summary>
        /// 음악 볼륨 설정
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            ApplySettings();
        }
    }
}
