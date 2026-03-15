using System;
using System.Globalization;

public static class CurrencyConverter
{
    // Ordered from thousand to decillion; extend as needed.
    private static readonly string[] Suffixes =
    {
        "K", "M", "B", "T", "Qa", "Qi", "Sx", "Sp", "Oc", "No", "Dc"
    };

    public static string ToAbbreviated(long value, int decimals = 1)
    {
        if (value < 1000)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        var safeDecimals = Math.Max(0, decimals);
        var magnitude = 0;
        decimal shortValue = value;

        while (shortValue >= 1000m && magnitude < Suffixes.Length)
        {
            shortValue /= 1000m;
            magnitude++;
        }

        if (magnitude == 0)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        var suffix = Suffixes[magnitude - 1];
        var rounded = Math.Round(shortValue, safeDecimals, MidpointRounding.AwayFromZero);

        // Handle rollover like 999.95K -> 1.0M
        if (rounded >= 1000m && magnitude < Suffixes.Length)
        {
            rounded /= 1000m;
            magnitude++;
            suffix = Suffixes[magnitude - 1];
        }

        return rounded.ToString($"F{safeDecimals}", CultureInfo.InvariantCulture) + suffix;
    }

    public static string ToDelimited(long value)
    {
        return value.ToString("N0", CultureInfo.InvariantCulture);
    }

    public static string ToCurrencyText(long value, int decimals = 1, bool abbreviated = true)
    {
        return abbreviated ? ToAbbreviated(value, decimals) : ToDelimited(value);
    }
}
