using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PunchKing
{
    /// <summary>
    /// 범용 프로그레스 바
    /// 체력바, 경험치바, 로딩바 등에 사용
    /// </summary>
    public class ProgressBar : MonoBehaviour
    {
        [Header("UI 레퍼런스")]
        public Image fillImage;
        public TextMeshProUGUI valueText;
        public TextMeshProUGUI percentText;

        [Header("설정")]
        public bool showValue = true;
        public bool showPercent = false;
        public bool smoothTransition = true;
        public float transitionSpeed = 5f;

        [Header("색상")]
        public Gradient colorGradient;
        public bool useGradient = false;

        private float currentFill = 0f;
        private float targetFill = 0f;

        /// <summary>
        /// 진행도 설정 (0~1)
        /// </summary>
        public void SetProgress(float progress)
        {
            targetFill = Mathf.Clamp01(progress);

            if (!smoothTransition)
            {
                currentFill = targetFill;
                UpdateUI();
            }
        }

        /// <summary>
        /// 값으로 설정
        /// </summary>
        public void SetValue(float current, float max)
        {
            if (max <= 0)
            {
                SetProgress(0);
                return;
            }

            float progress = current / max;
            SetProgress(progress);

            // 텍스트 업데이트
            if (showValue && valueText != null)
            {
                valueText.text = $"{current:F0} / {max:F0}";
            }
        }

        /// <summary>
        /// BigNumber 값으로 설정
        /// </summary>
        public void SetValue(BigNumber current, BigNumber max)
        {
            if (max.IsZero())
            {
                SetProgress(0);
                return;
            }

            double currentVal = current.ToDouble();
            double maxVal = max.ToDouble();
            float progress = (float)(currentVal / maxVal);
            SetProgress(progress);

            // 텍스트 업데이트
            if (showValue && valueText != null)
            {
                valueText.text = $"{current.ToKoreanString()} / {max.ToKoreanString()}";
            }
        }

        void Update()
        {
            if (smoothTransition && !Mathf.Approximately(currentFill, targetFill))
            {
                currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * transitionSpeed);
                UpdateUI();
            }
        }

        /// <summary>
        /// UI 업데이트
        /// </summary>
        void UpdateUI()
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = currentFill;

                // 그라데이션 색상
                if (useGradient && colorGradient != null)
                {
                    fillImage.color = colorGradient.Evaluate(currentFill);
                }
            }

            // 퍼센트 텍스트
            if (showPercent && percentText != null)
            {
                percentText.text = $"{(currentFill * 100f):F0}%";
            }
        }

        /// <summary>
        /// 즉시 변경
        /// </summary>
        public void SetProgressImmediate(float progress)
        {
            targetFill = Mathf.Clamp01(progress);
            currentFill = targetFill;
            UpdateUI();
        }
    }
}
