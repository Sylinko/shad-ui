
using System.Globalization;
using Avalonia.Data.Converters;
using Lucide.Avalonia;

namespace ShadUI.Demo.Converters;

public class ThemeToIconConverter : IValueConverter
{
    private static readonly Dictionary<ThemeMode, LucideIcon> ThemeIconDictionary = new()
        {
            [ThemeMode.System] = new LucideIcon { Kind = LucideIconKind.SunMoon, StrokeWidth = 1.5, Size = 18 },
            [ThemeMode.Light]  = new LucideIcon { Kind = LucideIconKind.Sun,     StrokeWidth = 1.5, Size = 18 },
            [ThemeMode.Dark]   = new LucideIcon { Kind = LucideIconKind.Moon,    StrokeWidth = 1.5, Size = 18 },
        };

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ThemeMode mode && ThemeIconDictionary.TryGetValue(mode, out var icon))
            return icon;

        if (value is not int idx)
            return ThemeIconDictionary[ThemeMode.System];
        
        var modes = new[] { ThemeMode.System, ThemeMode.Light, ThemeMode.Dark };
        if (idx >= 0 && idx < modes.Length && ThemeIconDictionary.TryGetValue(modes[idx], out icon))
            return icon;

        return ThemeIconDictionary[ThemeMode.System];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}