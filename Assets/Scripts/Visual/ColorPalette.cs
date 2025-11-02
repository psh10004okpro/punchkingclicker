using UnityEngine;

namespace PunchKing
{
    /// <summary>
    /// 게임 전체 색상 팔레트
    /// 일관된 색상 테마 관리
    /// </summary>
    [CreateAssetMenu(fileName = "ColorPalette", menuName = "PunchKing/Color Palette", order = 10)]
    public class ColorPalette : ScriptableObject
    {
        [Header("주요 색상")]
        public Color primaryColor = new Color(1f, 0.5f, 0f);      // 주황색
        public Color secondaryColor = new Color(0.2f, 0.6f, 1f);  // 파란색
        public Color accentColor = new Color(1f, 0.84f, 0f);      // 골드

        [Header("UI 색상")]
        public Color uiBackground = new Color(0.1f, 0.1f, 0.15f);
        public Color uiPanel = new Color(0.15f, 0.15f, 0.2f);
        public Color uiText = new Color(1f, 1f, 1f);
        public Color uiTextDark = new Color(0.7f, 0.7f, 0.7f);

        [Header("버튼 색상")]
        public Color buttonNormal = new Color(0.2f, 0.6f, 1f);
        public Color buttonHighlight = new Color(0.3f, 0.7f, 1f);
        public Color buttonPressed = new Color(0.1f, 0.5f, 0.9f);
        public Color buttonDisabled = new Color(0.3f, 0.3f, 0.3f);

        [Header("레어도 색상")]
        public Color rarity Common = new Color(0.7f, 0.7f, 0.7f);      // 회색
        public Color rarityUncommon = new Color(0.2f, 1f, 0.2f);       // 녹색
        public Color rarityRare = new Color(0.2f, 0.5f, 1f);           // 파란색
        public Color rarityEpic = new Color(0.8f, 0.2f, 1f);           // 보라색
        public Color rarityLegendary = new Color(1f, 0.5f, 0f);        // 주황색
        public Color rarityMythic = new Color(1f, 0.84f, 0f);          // 골드

        [Header("게임플레이 색상")]
        public Color damageNormal = new Color(1f, 1f, 1f);
        public Color damageCritical = new Color(1f, 0.2f, 0.2f);
        public Color gold = new Color(1f, 0.84f, 0f);
        public Color prestige = new Color(0.8f, 0.3f, 1f);
        public Color health = new Color(0.2f, 1f, 0.2f);
        public Color mana = new Color(0.2f, 0.5f, 1f);

        [Header("상태 색상")]
        public Color success = new Color(0.2f, 1f, 0.2f);
        public Color warning = new Color(1f, 0.84f, 0f);
        public Color error = new Color(1f, 0.2f, 0.2f);
        public Color info = new Color(0.2f, 0.6f, 1f);

        [Header("업적/등급 색상")]
        public Color bronze = new Color(0.8f, 0.5f, 0.2f);
        public Color silver = new Color(0.75f, 0.75f, 0.75f);
        public Color goldTier = new Color(1f, 0.84f, 0f);
        public Color platinum = new Color(0.9f, 1f, 1f);
        public Color diamond = new Color(0.7f, 0.9f, 1f);

        /// <summary>
        /// 레어도에 따른 색상 반환
        /// </summary>
        public Color GetRarityColor(int rarity)
        {
            switch (rarity)
            {
                case 0: return rarityCommon;
                case 1: return rarityUncommon;
                case 2: return rarityRare;
                case 3: return rarityEpic;
                case 4: return rarityLegendary;
                case 5: return rarityMythic;
                default: return rarityCommon;
            }
        }

        /// <summary>
        /// 티어에 따른 색상 반환
        /// </summary>
        public Color GetTierColor(int tier)
        {
            switch (tier)
            {
                case 1: return bronze;
                case 2: return silver;
                case 3: return goldTier;
                case 4: return platinum;
                case 5: return diamond;
                default: return bronze;
            }
        }

        /// <summary>
        /// 상태에 따른 색상 반환
        /// </summary>
        public Color GetStatusColor(string status)
        {
            switch (status.ToLower())
            {
                case "success": return success;
                case "warning": return warning;
                case "error": return error;
                case "info": return info;
                default: return uiText;
            }
        }

        /// <summary>
        /// 그라데이션 생성
        /// </summary>
        public Gradient CreateGradient(Color startColor, Color endColor)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(startColor, 0f),
                    new GradientColorKey(endColor, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            );
            return gradient;
        }

        /// <summary>
        /// 색상 밝게 만들기
        /// </summary>
        public static Color Lighten(Color color, float amount = 0.2f)
        {
            return Color.Lerp(color, Color.white, amount);
        }

        /// <summary>
        /// 색상 어둡게 만들기
        /// </summary>
        public static Color Darken(Color color, float amount = 0.2f)
        {
            return Color.Lerp(color, Color.black, amount);
        }

        /// <summary>
        /// 투명도 조정
        /// </summary>
        public static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        /// <summary>
        /// Hex 코드로부터 색상 생성
        /// </summary>
        public static Color FromHex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out Color color))
            {
                return color;
            }
            return Color.white;
        }

        /// <summary>
        /// 색상을 Hex 코드로 변환
        /// </summary>
        public static string ToHex(Color color)
        {
            return "#" + ColorUtility.ToHtmlStringRGBA(color);
        }
    }
}
