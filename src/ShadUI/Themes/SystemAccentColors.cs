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
    /// <summary>
    /// Gets or sets a color to override the system accent color. If set to null, the actual system accent color will be used.
    /// </summary>
    public static Color? ColorOverride
    {
        get;
        set
        {
            if (field == value) return;
            field = value;

            _instance?.UpdateColors(Application.Current?.PlatformSettings);
        }
    }

    private const string PrimaryKey = "PrimaryColor";
    private const string Primary75Key = "PrimaryColor75";
    private const string Primary50Key = "PrimaryColor50";
    private const string Primary10Key = "PrimaryColor10";
    private const string PrimaryForegroundKey = "PrimaryForegroundColor";

    private static SystemAccentColors? _instance;
    
    public SystemAccentColors()
    {
        if (_instance is not null) throw new InvalidOperationException("Only one instance of SystemAccentColors is allowed.");
        _instance = this;

        if (Application.Current?.PlatformSettings is { } platformSettings)
        {
            platformSettings.ColorValuesChanged += PlatformSettingsOnColorValuesChanged;
            UpdateColors(platformSettings);
        }
    }

    private void PlatformSettingsOnColorValuesChanged(object? sender, PlatformColorValues e)
    {
        UpdateColors(sender as IPlatformSettings);
    }

    private void UpdateColors(IPlatformSettings? platformSettings)
    {
        var systemAccentColor = ColorOverride ?? platformSettings?.GetColorValues().AccentColor1 ?? Color.FromRgb(0, 120, 215);
        
        var (d1, d2, d3) = CalculateAccentShades(systemAccentColor);
        var luminance = (0.299 * systemAccentColor.R + 0.587 * systemAccentColor.G + 0.114 * systemAccentColor.B) / 255;
        var systemAccentForegroundColor = luminance > 0.6 ? new Color(255, 29, 29, 31) : new Color(255, 245, 245, 247);

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
        return (
            new Color(191, accentColor.R, accentColor.G, accentColor.B),
            new Color(128, accentColor.R, accentColor.G, accentColor.B),
            new Color(26, accentColor.R, accentColor.G, accentColor.B)
        );
    }
}