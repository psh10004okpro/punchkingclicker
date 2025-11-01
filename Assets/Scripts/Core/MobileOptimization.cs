using UnityEngine;

#if UNITY_6_0_OR_NEWER
using UnityEngine.Rendering.Universal;
#endif

namespace PunchKing
{
    /// <summary>
    /// 모바일 최적화 설정
    /// Unity 6의 최신 기능을 활용한 성능 최적화
    /// </summary>
    public class MobileOptimization : MonoBehaviour
    {
        [Header("프레임레이트 설정")]
        [SerializeField] private int targetFrameRate = 60;

        [Header("배터리 절약 모드")]
        [SerializeField] private bool batterySaveMode = false;
        [SerializeField] private int batterySaveFrameRate = 30;

        void Start()
        {
            ConfigureFrameRate();
            ConfigureQualitySettings();
            ConfigureURPSettings();
            ConfigureMobileSpecificSettings();

            Debug.Log("모바일 최적화 설정 완료");
        }

        /// <summary>
        /// 프레임레이트 설정
        /// </summary>
        void ConfigureFrameRate()
        {
            int frameRate = batterySaveMode ? batterySaveFrameRate : targetFrameRate;
            Application.targetFrameRate = frameRate;

            // VSync 끄기 (수동으로 프레임레이트 제어)
            QualitySettings.vSyncCount = 0;

            Debug.Log($"Target FPS: {frameRate}");
        }

        /// <summary>
        /// 품질 설정
        /// </summary>
        void ConfigureQualitySettings()
        {
            // 텍스처 품질 (Full resolution)
            QualitySettings.masterTextureLimit = 0;

            // 그림자 비활성화 (2D 클릭커 게임은 그림자 불필요)
            QualitySettings.shadows = ShadowQuality.Disable;

            // 파티클 레이캐스트 예산
            QualitySettings.particleRaycastBudget = 64;

            // Unity 6 GPU Resident Drawer 최적화
#if UNITY_6_0_OR_NEWER
            QualitySettings.enableGPUResidentDrawer = true;
            QualitySettings.smallMeshScreenPercentage = 0.15f;

            Debug.Log("Unity 6 GPU Resident Drawer 활성화");
#endif

            // 안티앨리어싱 설정
            QualitySettings.antiAliasing = 2;  // 2x MSAA

            // LOD Bias
            QualitySettings.lodBias = 1.0f;

            // 픽셀 라이트 카운트 (2D 게임이므로 최소화)
            QualitySettings.pixelLightCount = 1;
        }

        /// <summary>
        /// URP (Universal Render Pipeline) 설정
        /// </summary>
        void ConfigureURPSettings()
        {
#if UNITY_6_0_OR_NEWER
            var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;

            if (urpAsset != null)
            {
                // 렌더 스케일 조정
                float renderScale = CalculateOptimalRenderScale();
                urpAsset.renderScale = renderScale;

                // HDR 비활성화 (배터리 절약)
                urpAsset.supportsHDR = false;

                // MSAA 설정
                urpAsset.msaaSampleCount = batterySaveMode ? 0 : 2;

                Debug.Log($"URP Render Scale: {renderScale}, MSAA: {urpAsset.msaaSampleCount}");
            }
            else
            {
                Debug.LogWarning("URP Asset을 찾을 수 없습니다");
            }
#endif
        }

        /// <summary>
        /// 기기별 최적 렌더 스케일 계산
        /// </summary>
        float CalculateOptimalRenderScale()
        {
            float renderScale = 1.0f;

#if UNITY_IOS
            // iOS 기기별 최적화
            string deviceModel = UnityEngine.iOS.Device.generation.ToString();

            if (deviceModel.Contains("iPhone12") || deviceModel.Contains("iPhone13") || deviceModel.Contains("iPhone14"))
            {
                renderScale = 0.85f;  // 고해상도 아이폰은 약간 낮춤
            }
            else if (deviceModel.Contains("iPhone11") || deviceModel.Contains("iPhoneX"))
            {
                renderScale = 0.9f;
            }
            else if (deviceModel.Contains("iPad"))
            {
                renderScale = 0.8f;  // 아이패드는 화면이 크므로 더 낮춤
            }

            Debug.Log($"iOS Device: {deviceModel}, Render Scale: {renderScale}");
#elif UNITY_ANDROID
            // Android RAM 기반 최적화
            int systemMemoryMB = SystemInfo.systemMemorySize;

            if (systemMemoryMB < 2048)
            {
                renderScale = 0.65f;  // 2GB 미만
            }
            else if (systemMemoryMB < 4096)
            {
                renderScale = 0.75f;  // 2~4GB
            }
            else if (systemMemoryMB < 6144)
            {
                renderScale = 0.85f;  // 4~6GB
            }
            else
            {
                renderScale = 0.95f;  // 6GB 이상
            }

            Debug.Log($"Android RAM: {systemMemoryMB}MB, Render Scale: {renderScale}");
#endif

            // 배터리 절약 모드일 때 추가 감소
            if (batterySaveMode)
            {
                renderScale *= 0.8f;
            }

            return renderScale;
        }

        /// <summary>
        /// 모바일 전용 설정
        /// </summary>
        void ConfigureMobileSpecificSettings()
        {
            // 화면 꺼짐 방지
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            // 멀티 터치 활성화
            Input.multiTouchEnabled = true;

            // 자동 회전 비활성화 (세로 모드 고정)
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;

#if UNITY_ANDROID
            // Android 전용 최적화
            Screen.brightness = 1.0f;
#endif

#if UNITY_IOS
            // iOS 전용 최적화
            // 로우 파워 모드 감지 등
#endif
        }

        /// <summary>
        /// 배터리 절약 모드 토글
        /// </summary>
        public void ToggleBatterySaveMode(bool enabled)
        {
            batterySaveMode = enabled;
            ConfigureFrameRate();
            ConfigureURPSettings();

            Debug.Log($"배터리 절약 모드: {(enabled ? "ON" : "OFF")}");
        }

        /// <summary>
        /// 성능 정보 로그
        /// </summary>
        [ContextMenu("Log Performance Info")]
        void LogPerformanceInfo()
        {
            Debug.Log("===== 성능 정보 =====");
            Debug.Log($"Device Model: {SystemInfo.deviceModel}");
            Debug.Log($"Device Type: {SystemInfo.deviceType}");
            Debug.Log($"Graphics API: {SystemInfo.graphicsDeviceType}");
            Debug.Log($"Graphics Memory: {SystemInfo.graphicsMemorySize}MB");
            Debug.Log($"System Memory: {SystemInfo.systemMemorySize}MB");
            Debug.Log($"Processor: {SystemInfo.processorType}");
            Debug.Log($"Processor Count: {SystemInfo.processorCount}");
            Debug.Log($"Screen Resolution: {Screen.width}x{Screen.height} @{Screen.currentResolution.refreshRate}Hz");
            Debug.Log($"Target FPS: {Application.targetFrameRate}");
            Debug.Log("====================");
        }

#if UNITY_EDITOR
        void OnGUI()
        {
            // 에디터에서 FPS 표시
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.white;

            float fps = 1.0f / Time.deltaTime;
            GUI.Label(new Rect(10, 10, 200, 30), $"FPS: {fps:F1}", style);
        }
#endif
    }
}
