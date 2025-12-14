using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;

namespace ShadUI.Themes;

/// <summary>
/// A ResourceDictionary that updates its colors based on the system accent color.
/// </summary>
public sealed class SystemAccentColors : ResourceDictionary
{
    private const string PrimaryKey = "PrimaryColor";
    private const string Primary75Key = "PrimaryColor75";
    private const string Primary50Key = "PrimaryColor50";
    private const string Primary10Key = "PrimaryColor10";
    private const string PrimaryForegroundKey = "PrimaryForegroundColor";
    
    private static readonly Color SDefaultSystemAccentColor = Color.FromRgb(0, 120, 215);
    
    public SystemAccentColors()
    {
        if (Application.Current?.PlatformSettings is { } platformSettings)
        {
            platformSettings.ColorValuesChanged += PlatformSettingsOnColorValuesChanged;
        }
        
        UpdateColors();
    }

    private void PlatformSettingsOnColorValuesChanged(object? sender, PlatformColorValues e) => 
        UpdateColors();

    private void UpdateColors()
    {
        var platformSettings = Application.Current?.PlatformSettings;
        var systemAccentColor = platformSettings?.GetColorValues().AccentColor1 ?? SDefaultSystemAccentColor;
        
        var (d1, d2, d3) = CalculateAccentShades(systemAccentColor);
        
        var luminance = (0.299 * systemAccentColor.R + 0.587 * systemAccentColor.G + 0.114 * systemAccentColor.B) / 255;
        var systemAccentForegroundColor = luminance > 0.6 ? 
            new Color(255, 29, 29, 31) : 
            new Color(255, 245, 245, 247);

        SetItems(
        [
            new KeyValuePair<object, object?>(PrimaryKey, systemAccentColor),
            new KeyValuePair<object, object?>(Primary75Key, d1),
            new KeyValuePair<object, object?>(Primary50Key, d2),
            new KeyValuePair<object, object?>(Primary10Key, d3),
            new KeyValuePair<object, object?>(PrimaryForegroundKey, systemAccentForegroundColor),
        ]);
    }

    private static (Color d1, Color d2, Color d3) CalculateAccentShades(Color accentColor)
    {
        const double dark1Step = 28.5 / 255d;
        const double dark2Step = 49 / 255d;
        const double dark3Step = 74.5 / 255d;
        
        var hslAccent = accentColor.ToHsl();
        return (
            new HslColor(hslAccent.A, hslAccent.H, hslAccent.S, hslAccent.L - dark1Step).ToRgb(),
            new HslColor(hslAccent.A, hslAccent.H, hslAccent.S, hslAccent.L - dark2Step).ToRgb(),
            new HslColor(hslAccent.A, hslAccent.H, hslAccent.S, hslAccent.L - dark3Step).ToRgb()
        );
    }
}