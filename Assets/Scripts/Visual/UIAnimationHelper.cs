using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace PunchKing
{
    /// <summary>
    /// UI 애니메이션 헬퍼
    /// 다양한 UI 애니메이션 효과 제공
    /// </summary>
    public class UIAnimationHelper : MonoBehaviour
    {
        /// <summary>
        /// 펄스 애니메이션 (크기 커졌다 작아지기)
        /// </summary>
        public static IEnumerator Pulse(Transform target, float scale = 1.2f, float duration = 0.3f)
        {
            if (target == null) yield break;

            Vector3 originalScale = target.localScale;
            Vector3 targetScale = originalScale * scale;
            float elapsed = 0f;

            // 확대
            while (elapsed < duration / 2f)
            {
                target.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / (duration / 2f));
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;

            // 축소
            while (elapsed < duration / 2f)
            {
                target.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / (duration / 2f));
                elapsed += Time.deltaTime;
                yield return null;
            }

            target.localScale = originalScale;
        }

        /// <summary>
        /// 스케일 애니메이션
        /// </summary>
        public static IEnumerator ScaleTo(Transform target, Vector3 targetScale, float duration = 0.3f)
        {
            if (target == null) yield break;

            Vector3 startScale = target.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                target.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            target.localScale = targetScale;
        }

        /// <summary>
        /// 페이드 인 애니메이션
        /// </summary>
        public static IEnumerator FadeIn(CanvasGroup target, float duration = 0.3f)
        {
            if (target == null) yield break;

            float elapsed = 0f;
            target.alpha = 0f;

            while (elapsed < duration)
            {
                target.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            target.alpha = 1f;
        }

        /// <summary>
        /// 페이드 아웃 애니메이션
        /// </summary>
        public static IEnumerator FadeOut(CanvasGroup target, float duration = 0.3f)
        {
            if (target == null) yield break;

            float elapsed = 0f;
            target.alpha = 1f;

            while (elapsed < duration)
            {
                target.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            target.alpha = 0f;
        }

        /// <summary>
        /// 이미지 페이드
        /// </summary>
        public static IEnumerator FadeImage(Image target, float startAlpha, float endAlpha, float duration)
        {
            if (target == null) yield break;

            float elapsed = 0f;
            Color color = target.color;

            while (elapsed < duration)
            {
                color.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
                target.color = color;
                elapsed += Time.deltaTime;
                yield return null;
            }

            color.a = endAlpha;
            target.color = color;
        }

        /// <summary>
        /// 텍스트 색상 변경 애니메이션
        /// </summary>
        public static IEnumerator ColorTransition(TextMeshProUGUI target, Color endColor, float duration = 0.3f)
        {
            if (target == null) yield break;

            Color startColor = target.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                target.color = Color.Lerp(startColor, endColor, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            target.color = endColor;
        }

        /// <summary>
        /// 흔들림 애니메이션 (UI 셰이크)
        /// </summary>
        public static IEnumerator Shake(Transform target, float intensity = 10f, float duration = 0.3f)
        {
            if (target == null) yield break;

            Vector3 originalPosition = target.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;

                target.localPosition = originalPosition + new Vector3(x, y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            target.localPosition = originalPosition;
        }

        /// <summary>
        /// 바운스 애니메이션
        /// </summary>
        public static IEnumerator Bounce(Transform target, float height = 20f, float duration = 0.5f)
        {
            if (target == null) yield break;

            Vector3 originalPosition = target.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                // Sin 곡선으로 바운스 효과
                float offset = Mathf.Sin(t * Mathf.PI) * height;
                target.localPosition = originalPosition + new Vector3(0f, offset, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            target.localPosition = originalPosition;
        }

        /// <summary>
        /// 회전 애니메이션
        /// </summary>
        public static IEnumerator Rotate(Transform target, float angle, float duration = 0.3f)
        {
            if (target == null) yield break;

            Quaternion startRotation = target.localRotation;
            Quaternion endRotation = Quaternion.Euler(0f, 0f, angle);
            float elapsed = 0f;

            while (elapsed < duration)
            {
                target.localRotation = Quaternion.Lerp(startRotation, endRotation, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            target.localRotation = endRotation;
        }

        /// <summary>
        /// 연속 회전 애니메이션 (무한 루프)
        /// </summary>
        public static IEnumerator RotateContinuous(Transform target, float speed = 90f)
        {
            if (target == null) yield break;

            while (true)
            {
                target.Rotate(0f, 0f, speed * Time.deltaTime);
                yield return null;
            }
        }

        /// <summary>
        /// 슬라이드 인 애니메이션 (위치 이동)
        /// </summary>
        public static IEnumerator SlideIn(RectTransform target, Vector2 startOffset, float duration = 0.3f)
        {
            if (target == null) yield break;

            Vector2 originalPosition = target.anchoredPosition;
            Vector2 startPosition = originalPosition + startOffset;
            target.anchoredPosition = startPosition;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                target.anchoredPosition = Vector2.Lerp(startPosition, originalPosition, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            target.anchoredPosition = originalPosition;
        }

        /// <summary>
        /// 슬라이드 아웃 애니메이션
        /// </summary>
        public static IEnumerator SlideOut(RectTransform target, Vector2 endOffset, float duration = 0.3f)
        {
            if (target == null) yield break;

            Vector2 startPosition = target.anchoredPosition;
            Vector2 endPosition = startPosition + endOffset;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                target.anchoredPosition = Vector2.Lerp(startPosition, endPosition, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            target.anchoredPosition = endPosition;
        }

        /// <summary>
        /// 숫자 카운트업 애니메이션
        /// </summary>
        public static IEnumerator CountUp(TextMeshProUGUI target, long startValue, long endValue, float duration = 1f)
        {
            if (target == null) yield break;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                long currentValue = (long)Mathf.Lerp(startValue, endValue, elapsed / duration);
                target.text = currentValue.ToString("N0");
                elapsed += Time.deltaTime;
                yield return null;
            }

            target.text = endValue.ToString("N0");
        }

        /// <summary>
        /// 글로우 효과 (Image 밝기 변화)
        /// </summary>
        public static IEnumerator Glow(Image target, float intensity = 1.5f, float duration = 0.5f)
        {
            if (target == null) yield break;

            Color originalColor = target.color;
            Color glowColor = originalColor * intensity;
            float elapsed = 0f;

            // 밝아지기
            while (elapsed < duration / 2f)
            {
                target.color = Color.Lerp(originalColor, glowColor, elapsed / (duration / 2f));
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;

            // 원래대로
            while (elapsed < duration / 2f)
            {
                target.color = Color.Lerp(glowColor, originalColor, elapsed / (duration / 2f));
                elapsed += Time.deltaTime;
                yield return null;
            }

            target.color = originalColor;
        }

        /// <summary>
        /// 플래시 효과 (빠른 깜빡임)
        /// </summary>
        public static IEnumerator Flash(Image target, Color flashColor, int flashCount = 3, float flashSpeed = 0.1f)
        {
            if (target == null) yield break;

            Color originalColor = target.color;

            for (int i = 0; i < flashCount; i++)
            {
                target.color = flashColor;
                yield return new WaitForSeconds(flashSpeed);
                target.color = originalColor;
                yield return new WaitForSeconds(flashSpeed);
            }
        }

        /// <summary>
        /// 타이핑 효과 (텍스트가 한 글자씩 표시)
        /// </summary>
        public static IEnumerator TypeText(TextMeshProUGUI target, string fullText, float charDelay = 0.05f)
        {
            if (target == null) yield break;

            target.text = "";

            foreach (char c in fullText)
            {
                target.text += c;
                yield return new WaitForSeconds(charDelay);
            }
        }
    }
}
