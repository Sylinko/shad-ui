using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ShadUI;

/// <summary>
///     Provides platform-specific converters.
/// </summary>
public static class PlatformConverters
{
    /// <summary>
    ///     Converts a <see cref="Color" /> to a <see cref="SolidColorBrush" /> with platform-specific opacity for window backgrounds.
    /// </summary>
    public static IMultiValueConverter WindowBackgroundConverter { get; } = new WindowBackgroundConverterImpl();

    private sealed class WindowBackgroundConverterImpl : IMultiValueConverter
    {
        // TODO: Cross-platform opacity handling
        private static double Opacity
        {
            get
            {
                var osVersion = Environment.OSVersion;
                return osVersion.Platform switch
                {
                    // 0 for Windows 11, 0.9 for Windows 10
                    PlatformID.Win32NT when osVersion.Version >= new Version(10, 0, 22000) => 0.0,
                    PlatformID.Win32NT => 0.7,
                    PlatformID.MacOSX => 0.6,
                    _ => 1.0
                };
            }
        }
        
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            return values is not [Color color] ? null : new SolidColorBrush(color, opacity: Opacity);
        }
    }
}