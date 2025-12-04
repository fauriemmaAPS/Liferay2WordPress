using System.Text;
using System.Text.RegularExpressions;

namespace Liferay2WordPress.Services;

public static class SlugHelper
{
    public static string ToSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var s = input.ToLowerInvariant();
        s = RemoveDiacritics(s);
        s = Regex.Replace(s, "[^a-z0-9\\s-]", "");
        s = Regex.Replace(s, "[\\s-]+", "-").Trim('-');
        return s;
    }

    public static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
