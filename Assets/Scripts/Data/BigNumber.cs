using System;
using UnityEngine;

namespace PunchKing
{
    /// <summary>
    /// 큰 숫자를 처리하기 위한 유틸리티 클래스
    /// 가수(mantissa)와 지수(exponent)를 사용한 과학적 표기법 구현
    /// 예: 1.5e10 = 15,000,000,000
    ///
    /// 최대 표현 가능 숫자: 10^(9,223,372,036,854,775,807) - 사실상 무제한
    /// </summary>
    [System.Serializable]
    public class BigNumber
    {
        // 가수 (1.0 ~ 9.999...)
        private double mantissa;

        // 지수 (10의 몇 승인지) - long으로 사실상 무제한
        private long exponent;

        /// <summary>
        /// double 값으로부터 BigNumber 생성
        /// </summary>
        public BigNumber(double value)
        {
            if (value == 0)
            {
                mantissa = 0;
                exponent = 0;
            }
            else
            {
                exponent = (int)Math.Floor(Math.Log10(Math.Abs(value)));
                mantissa = value / Math.Pow(10, exponent);
            }
            Normalize();
        }

        /// <summary>
        /// 가수와 지수를 직접 지정하여 생성 (내부용)
        /// </summary>
        private BigNumber(double mantissa, long exponent)
        {
            this.mantissa = mantissa;
            this.exponent = exponent;
            Normalize();
        }

        /// <summary>
        /// 곱셈 연산
        /// </summary>
        public BigNumber Multiply(double multiplier)
        {
            if (multiplier == 0 || this.IsZero())
                return new BigNumber(0);

            double newMantissa = this.mantissa * multiplier;
            return new BigNumber(newMantissa, this.exponent);
        }

        /// <summary>
        /// BigNumber 곱셈
        /// </summary>
        public BigNumber Multiply(BigNumber other)
        {
            if (this.IsZero() || other.IsZero())
                return new BigNumber(0);

            double newMantissa = this.mantissa * other.mantissa;
            long newExponent = this.exponent + other.exponent;
            return new BigNumber(newMantissa, newExponent);
        }

        /// <summary>
        /// 덧셈 연산
        /// </summary>
        public BigNumber Add(BigNumber other)
        {
            if (this.IsZero()) return other;
            if (other.IsZero()) return this;

            long expDiff = this.exponent - other.exponent;

            // 지수 차이가 너무 크면 큰 쪽만 반환
            if (expDiff > 15) return this;
            if (expDiff < -15) return other;

            // 같은 지수로 맞춰서 덧셈
            double result = this.mantissa * Math.Pow(10, expDiff) + other.mantissa;
            return new BigNumber(result, other.exponent);
        }

        /// <summary>
        /// 뺄셈 연산
        /// </summary>
        public BigNumber Subtract(BigNumber other)
        {
            if (other.IsZero()) return this;
            if (this.IsZero()) return new BigNumber(-other.mantissa, other.exponent);

            long expDiff = this.exponent - other.exponent;

            if (expDiff > 15) return this;
            if (expDiff < -15) return new BigNumber(0);

            double result = this.mantissa * Math.Pow(10, expDiff) - other.mantissa;

            if (result <= 0) return new BigNumber(0);

            return new BigNumber(result, other.exponent);
        }

        /// <summary>
        /// 나눗셈 연산
        /// </summary>
        public BigNumber Divide(double divisor)
        {
            if (divisor == 0)
            {
                Debug.LogError("BigNumber: Division by zero!");
                return new BigNumber(0);
            }

            if (this.IsZero())
                return new BigNumber(0);

            double newMantissa = this.mantissa / divisor;
            return new BigNumber(newMantissa, this.exponent);
        }

        /// <summary>
        /// 크기 비교: this > other
        /// </summary>
        public bool IsGreaterThan(BigNumber other)
        {
            if (this.exponent > other.exponent) return true;
            if (this.exponent < other.exponent) return false;
            return this.mantissa > other.mantissa;
        }

        /// <summary>
        /// 크기 비교: this >= other
        /// </summary>
        public bool IsGreaterThanOrEqual(BigNumber other)
        {
            if (this.exponent > other.exponent) return true;
            if (this.exponent < other.exponent) return false;
            return this.mantissa >= other.mantissa;
        }

        /// <summary>
        /// 크기 비교: this < other
        /// </summary>
        public bool IsLessThan(BigNumber other)
        {
            return !IsGreaterThanOrEqual(other);
        }

        /// <summary>
        /// 0인지 확인
        /// </summary>
        public bool IsZero()
        {
            return mantissa == 0;
        }

        /// <summary>
        /// double로 변환 (오버플로우 주의)
        /// </summary>
        public double ToDouble()
        {
            if (IsZero()) return 0;

            // 지수가 너무 크면 double 범위 초과
            if (exponent > 308)
                return double.MaxValue;
            if (exponent < -308)
                return 0;

            return mantissa * Math.Pow(10, exponent);
        }

        /// <summary>
        /// 한국어 단위로 변환 (만, 억, 조, 경...)
        /// </summary>
        public string ToKoreanString()
        {
            if (IsZero()) return "0";

            // 만 단위 미만은 그냥 숫자로 표시
            if (exponent < 4)
            {
                return ToDouble().ToString("N0");
            }

            string[] units = { "", "만", "억", "조", "경", "해", "자", "양", "구", "간", "정", "재", "극" };

            // 각 단위는 10^4 (만)씩 증가
            long unitIndex = exponent / 4;

            // 단위 배열을 벗어나면 과학적 표기법 사용
            if (unitIndex >= units.Length)
            {
                return mantissa.ToString("F2") + "e" + exponent;
            }

            // 해당 단위로 변환
            double displayValue = ToDouble() / Math.Pow(10000, unitIndex);

            // 소수점 표시 여부
            if (displayValue >= 100)
                return displayValue.ToString("F0") + units[unitIndex];
            else if (displayValue >= 10)
                return displayValue.ToString("F1") + units[unitIndex];
            else
                return displayValue.ToString("F2") + units[unitIndex];
        }

        /// <summary>
        /// 짧은 표기법 (K, M, B, T...)
        /// </summary>
        public string ToShortString()
        {
            if (IsZero()) return "0";

            if (exponent < 3)
                return ToDouble().ToString("N0");

            string[] suffixes = { "", "K", "M", "B", "T", "aa", "ab", "ac", "ad", "ae", "af" };
            long suffixIndex = exponent / 3;

            if (suffixIndex >= suffixes.Length)
                return mantissa.ToString("F2") + "e" + exponent;

            double displayValue = ToDouble() / Math.Pow(1000, suffixIndex);
            return displayValue.ToString("F2") + suffixes[suffixIndex];
        }

        /// <summary>
        /// 저장용 문자열로 변환
        /// </summary>
        public string ToSaveString()
        {
            return $"{mantissa}e{exponent}";
        }

        /// <summary>
        /// 저장 문자열에서 복원
        /// </summary>
        public static BigNumber FromSaveString(string saveString)
        {
            if (string.IsNullOrEmpty(saveString))
                return new BigNumber(0);

            try
            {
                string[] parts = saveString.Split('e');
                if (parts.Length != 2)
                    return new BigNumber(0);

                double m = double.Parse(parts[0]);
                long e = long.Parse(parts[1]);
                return new BigNumber(m, e);
            }
            catch
            {
                Debug.LogError($"Failed to parse BigNumber from: {saveString}");
                return new BigNumber(0);
            }
        }

        /// <summary>
        /// 정규화: 가수를 1.0 ~ 9.999 범위로 조정
        /// </summary>
        private void Normalize()
        {
            if (mantissa == 0)
            {
                exponent = 0;
                return;
            }

            // 가수가 10 이상이면 지수 증가
            while (Math.Abs(mantissa) >= 10)
            {
                mantissa /= 10;
                exponent++;
            }

            // 가수가 1 미만이면 지수 감소
            while (Math.Abs(mantissa) < 1 && mantissa != 0)
            {
                mantissa *= 10;
                exponent--;
            }
        }

        /// <summary>
        /// ToString 오버라이드
        /// </summary>
        public override string ToString()
        {
            return ToKoreanString();
        }

        /// <summary>
        /// 복사본 생성
        /// </summary>
        public BigNumber Clone()
        {
            return new BigNumber(this.mantissa, this.exponent);
        }

        // 편의 연산자 오버로딩
        public static BigNumber operator +(BigNumber a, BigNumber b) => a.Add(b);
        public static BigNumber operator -(BigNumber a, BigNumber b) => a.Subtract(b);
        public static BigNumber operator *(BigNumber a, double b) => a.Multiply(b);
        public static BigNumber operator *(BigNumber a, BigNumber b) => a.Multiply(b);
        public static BigNumber operator /(BigNumber a, double b) => a.Divide(b);
        public static bool operator >(BigNumber a, BigNumber b) => a.IsGreaterThan(b);
        public static bool operator <(BigNumber a, BigNumber b) => a.IsLessThan(b);
        public static bool operator >=(BigNumber a, BigNumber b) => a.IsGreaterThanOrEqual(b);
        public static bool operator <=(BigNumber a, BigNumber b) => !a.IsGreaterThan(b);
    }
}
