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
    private const string PrimaryHoverKey = "PrimaryHoverColor";
    private const string Primary75Key = "PrimaryColor75";
    private const string Primary50Key = "PrimaryColor50";
    private const string Primary25Key = "PrimaryColor25";
    private const string Primary10Key = "PrimaryColor10";
    private const string PrimaryForegroundKey = "PrimaryForegroundColor";

    private static SystemAccentColors? _instance;
    
    public SystemAccentColors()
    {
        if (_instance is not null) throw new InvalidOperationException("Only one instance of SystemAccentColors is allowed.");
        _instance = this;

        if (Application.Current?.PlatformSettings is not { } platformSettings)
        {
            return;
        }

        platformSettings.ColorValuesChanged += PlatformSettingsOnColorValuesChanged;
        UpdateColors(platformSettings);
    }

    private void PlatformSettingsOnColorValuesChanged(object? sender, PlatformColorValues e)
    {
        UpdateColors(sender as IPlatformSettings);
    }

    private void UpdateColors(IPlatformSettings? platformSettings)
    {
        var systemAccentColor = ColorOverride ?? platformSettings?.GetColorValues().AccentColor1 ?? Color.FromRgb(0, 120, 215);
        
        var (d1, d2, d3, d4) = CalculateAccentShades(systemAccentColor);
        var luminance = GetRelativeLuminance(systemAccentColor);
        var darkForeground = Colors.Black;
        var lightForeground = Color.FromRgb(253, 252, 249);
        // Compare actual contrast in linear light. Weighted sRGB brightness selected unreadable light
        // text for medium blue and green accents. The system/user accent itself remains unchanged.
        var darkContrast = GetContrast(luminance, GetRelativeLuminance(darkForeground));
        var lightContrast = GetContrast(luminance, GetRelativeLuminance(lightForeground));
        var shouldUseDarkForeground = darkContrast > lightContrast;
        var systemAccentForegroundColor = shouldUseDarkForeground ? darkForeground : lightForeground;
        var hoverTarget = shouldUseDarkForeground ? Colors.White : Colors.Black;
        var hoverColor = new Color(
            systemAccentColor.A,
            MixChannel(systemAccentColor.R, hoverTarget.R),
            MixChannel(systemAccentColor.G, hoverTarget.G),
            MixChannel(systemAccentColor.B, hoverTarget.B));

        SetItems(
        [
            new KeyValuePair<object, object?>(PrimaryKey, systemAccentColor),
            // A solid hover surface preserves contrast; a translucent accent can wash out light text.
            new KeyValuePair<object, object?>(PrimaryHoverKey, hoverColor),
            new KeyValuePair<object, object?>(Primary75Key, d1),
            new KeyValuePair<object, object?>(Primary50Key, d2),
            new KeyValuePair<object, object?>(Primary25Key, d3),
            new KeyValuePair<object, object?>(Primary10Key, d4),
            new KeyValuePair<object, object?>(PrimaryForegroundKey, systemAccentForegroundColor),
        ]);
    }

    private static double GetRelativeLuminance(Color color)
    {
        return 0.2126 * Linearize(color.R) + 0.7152 * Linearize(color.G) + 0.0722 * Linearize(color.B);
    }

    private static byte MixChannel(byte channel, byte target) => (byte)Math.Round(channel * 0.9 + target * 0.1);

    private static double Linearize(byte channel)
    {
        var value = channel / 255d;
        return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
    }

    private static double GetContrast(double first, double second)
    {
        return (Math.Max(first, second) + 0.05) / (Math.Min(first, second) + 0.05);
    }

    private static (Color d1, Color d2, Color d3, Color d4) CalculateAccentShades(Color accentColor)
    {
        return (
            new Color(191, accentColor.R, accentColor.G, accentColor.B),
            new Color(128, accentColor.R, accentColor.G, accentColor.B),
            new Color(64, accentColor.R, accentColor.G, accentColor.B),
            new Color(26, accentColor.R, accentColor.G, accentColor.B)
        );
    }
}