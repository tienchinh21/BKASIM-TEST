using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace MiniAppGIBA.Base.Common
{
    public static class SlugGenerator
    {
        /// <summary>
        /// Chuyển đổi text thành slug (URL-friendly string)
        /// Example: "Nguyễn Văn A" -> "nguyen-van-a"
        /// </summary>
        public static string ToSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            text = RemoveDiacritics(text);

            text = text.ToLowerInvariant();

            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");

            text = Regex.Replace(text, @"[\s-]+", " ").Trim();

            text = Regex.Replace(text, @"\s", "-");

            if (text.Length > 100)
                text = text.Substring(0, 100).TrimEnd('-');

            return text;
        }

        /// <summary>
        /// Remove diacritics (Vietnamese accents) from text
        /// Example: "Nguyễn" -> "Nguyen"
        /// </summary>
        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Normalize to FormD (decompose characters)
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Replace Đ/đ specifically (not handled by FormD normalization)
            return stringBuilder.ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace("Đ", "D")
                .Replace("đ", "d");
        }

        /// <summary>
        /// Validate if slug is valid
        /// </summary>
        public static bool IsValidSlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return false;

            // Must contain only lowercase letters, numbers, and hyphens
            return Regex.IsMatch(slug, @"^[a-z0-9-]+$");
        }

        /// <summary>
        /// Check if slug is in reserved list
        /// </summary>
        public static bool IsReservedSlug(string slug)
        {
            var reservedSlugs = new HashSet<string>
            {
                "admin", "api", "profile", "public", "system",
                "settings", "dashboard", "login", "logout", "register"
            };

            return reservedSlugs.Contains(slug.ToLowerInvariant());
        }

        /// <summary>
        /// Generate unique slug by appending number
        /// Example: "nguyen-van-a" -> "nguyen-van-a-1", "nguyen-van-a-2"...
        /// </summary>
        public static string AppendNumber(string baseSlug, int number)
        {
            return $"{baseSlug}-{number}";
        }
    }
}
