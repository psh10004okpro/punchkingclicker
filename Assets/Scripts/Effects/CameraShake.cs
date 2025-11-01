using UnityEngine;
using System.Collections;

namespace PunchKing
{
    /// <summary>
    /// 카메라 흔들림 효과
    /// 타격감 연출을 위한 카메라 셰이크
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        [Header("설정")]
        [SerializeField] private bool enableShake = true;

        private Camera mainCamera;
        private Vector3 originalPosition;
        private Coroutine shakeCoroutine;

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

            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                originalPosition = mainCamera.transform.localPosition;
            }
        }

        /// <summary>
        /// 카메라 흔들기
        /// </summary>
        /// <param name="duration">지속 시간</param>
        /// <param name="magnitude">강도</param>
        public void Shake(float duration, float magnitude)
        {
            if (!enableShake || mainCamera == null) return;

            // 이미 흔들림 중이면 중지
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }

            shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
        }

        IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                mainCamera.transform.localPosition = originalPosition + new Vector3(x, y, 0);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 원래 위치로 복귀
            mainCamera.transform.localPosition = originalPosition;
            shakeCoroutine = null;
        }

        /// <summary>
        /// 카메라 셰이크 활성화/비활성화
        /// </summary>
        public void SetShakeEnabled(bool enabled)
        {
            enableShake = enabled;
        }
    }
}
