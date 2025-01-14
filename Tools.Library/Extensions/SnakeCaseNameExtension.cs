﻿namespace Tools.Library.Extensions;

public static class SnakeCaseNameExtension
{
    public static string ToSnakeCase(this string name, CultureInfo culture = null)
    {
        culture = culture == null ? CultureInfo.InvariantCulture : culture;

        var builder = new StringBuilder(name.Length + Math.Min(2, name.Length / 5));
        var previousCategory = default(UnicodeCategory?);

        for (var currentIndex = 0; currentIndex < name.Length; currentIndex++)
        {
            var currentChar = name[currentIndex];
            if (currentChar == '_')
            {
                builder.Append('_');
                previousCategory = null;
                continue;
            }

            var currentCategory = char.GetUnicodeCategory(currentChar);
            switch (currentCategory)
            {
                case UnicodeCategory.UppercaseLetter:
                case UnicodeCategory.TitlecaseLetter:
                    if (previousCategory == UnicodeCategory.SpaceSeparator ||
                        previousCategory == UnicodeCategory.UppercaseLetter ||
                        previousCategory == UnicodeCategory.LowercaseLetter ||
                        previousCategory == UnicodeCategory.DecimalDigitNumber &&
                        previousCategory != null &&
                    currentIndex > 0 &&
                        currentIndex + 1 < name.Length &&
                        (char.IsLower(name[currentIndex + 1]) || char.GetUnicodeCategory(name[currentIndex + 1]) == UnicodeCategory.DecimalDigitNumber))
                    {
                        builder.Append('_');
                    }

                    currentChar = char.ToLower(currentChar, culture);
                    break;

                case UnicodeCategory.LowercaseLetter:
                case UnicodeCategory.DecimalDigitNumber:
                    if (previousCategory == UnicodeCategory.SpaceSeparator)
                    {
                        builder.Append('_');
                    }
                    break;

                default:
                    if (previousCategory != null)
                    {
                        previousCategory = UnicodeCategory.SpaceSeparator;
                    }
                    continue;
            }

            builder.Append(currentChar);
            previousCategory = currentCategory;
        }

        return builder.ToString();
    }
}