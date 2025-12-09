using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public static class SlugHelper
{
    public static string ToSlug(this string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return string.Empty;

        title = title.ToLowerInvariant();

        // Xóa dấu tiếng Việt
        title = title.Replace("đ", "d");

        // Normalize ký tự Unicode
        title = title.Normalize(NormalizationForm.FormD);
        title = string.Concat(title.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark));

        // Xóa ký tự không phải chữ số/chữ cái, khoảng trắng, gạch ngang
        title = Regex.Replace(title, @"[^a-z0-9\s-]", "");

        // Chuyển khoảng trắng thành gạch ngang
        title = Regex.Replace(title.Trim(), @"\s+", "-");

        // Xóa gạch ngang trùng lặp
        title = Regex.Replace(title, @"-+", "-");

        return title;
    }
}
    