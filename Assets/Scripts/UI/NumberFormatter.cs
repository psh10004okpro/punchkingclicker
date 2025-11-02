using UnityEngine;

namespace PunchKing
{
    /// <summary>
    /// 숫자 포맷 유틸리티
    /// UI에서 숫자를 보기 좋게 표시
    /// </summary>
    public static class NumberFormatter
    {
        /// <summary>
        /// 숫자를 K, M, B, T 형식으로 변환
        /// </summary>
        public static string FormatShort(double value)
        {
            if (value < 1000)
                return value.ToString("F0");

            string[] suffixes = { "", "K", "M", "B", "T", "aa", "ab", "ac", "ad", "ae" };
            int suffixIndex = 0;

            while (value >= 1000 && suffixIndex < suffixes.Length - 1)
            {
                value /= 1000;
                suffixIndex++;
            }

            return $"{value:F2}{suffixes[suffixIndex]}";
        }

        /// <summary>
        /// 한국어 단위로 변환 (만, 억, 조...)
        /// </summary>
        public static string FormatKorean(double value)
        {
            if (value < 10000)
                return value.ToString("N0");

            string[] units = { "", "만", "억", "조", "경", "해", "자", "양", "구", "간", "정", "재", "극" };
            int unitIndex = 0;

            while (value >= 10000 && unitIndex < units.Length - 1)
            {
                value /= 10000;
                unitIndex++;
            }

            if (value >= 100)
                return $"{value:F0}{units[unitIndex]}";
            else if (value >= 10)
                return $"{value:F1}{units[unitIndex]}";
            else
                return $"{value:F2}{units[unitIndex]}";
        }

        /// <summary>
        /// 시간 포맷 (초 → HH:MM:SS)
        /// </summary>
        public static string FormatTime(int seconds)
        {
            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            int secs = seconds % 60;

            if (hours > 0)
                return $"{hours:D2}:{minutes:D2}:{secs:D2}";
            else
                return $"{minutes:D2}:{secs:D2}";
        }

        /// <summary>
        /// 시간 포맷 (한국어)
        /// </summary>
        public static string FormatTimeKorean(int seconds)
        {
            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            int secs = seconds % 60;

            if (hours > 0)
                return $"{hours}시간 {minutes}분";
            else if (minutes > 0)
                return $"{minutes}분 {secs}초";
            else
                return $"{secs}초";
        }

        /// <summary>
        /// 퍼센트 포맷
        /// </summary>
        public static string FormatPercent(float value, int decimals = 1)
        {
            return $"{(value * 100).ToString($"F{decimals}")}%";
        }

        /// <summary>
        /// 콤마 구분 숫자
        /// </summary>
        public static string FormatComma(double value)
        {
            return value.ToString("N0");
        }

        /// <summary>
        /// 소수점 포맷
        /// </summary>
        public static string FormatDecimal(double value, int decimals = 2)
        {
            return value.ToString($"F{decimals}");
        }

        /// <summary>
        /// 배수 표시 (×2.5 형식)
        /// </summary>
        public static string FormatMultiplier(float value)
        {
            return $"×{value:F1}";
        }

        /// <summary>
        /// 증가량 표시 (+50% 형식)
        /// </summary>
        public static string FormatIncrease(float increase)
        {
            string sign = increase >= 0 ? "+" : "";
            return $"{sign}{(increase * 100):F0}%";
        }

        /// <summary>
        /// 초당 변화량 (/sec)
        /// </summary>
        public static string FormatPerSecond(BigNumber value)
        {
            return $"{value.ToKoreanString()}/초";
        }

        /// <summary>
        /// 레벨 표시
        /// </summary>
        public static string FormatLevel(int level, int maxLevel = 0)
        {
            if (maxLevel > 0)
                return $"Lv.{level}/{maxLevel}";
            else
                return $"Lv.{level}";
        }

        /// <summary>
        /// 날짜 시간 포맷
        /// </summary>
        public static string FormatDateTime(System.DateTime dateTime)
        {
            System.TimeSpan span = System.DateTime.Now - dateTime;

            if (span.TotalDays >= 1)
                return $"{(int)span.TotalDays}일 전";
            else if (span.TotalHours >= 1)
                return $"{(int)span.TotalHours}시간 전";
            else if (span.TotalMinutes >= 1)
                return $"{(int)span.TotalMinutes}분 전";
            else
                return "방금 전";
        }

        /// <summary>
        /// 파일 크기 포맷
        /// </summary>
        public static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double size = bytes;
            int order = 0;

            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size /= 1024;
            }

            return $"{size:F2} {sizes[order]}";
        }
    }
}
