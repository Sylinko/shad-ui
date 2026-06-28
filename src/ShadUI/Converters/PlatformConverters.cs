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
        private static double Opacity => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000) ? 0.0 : // full mica
            OperatingSystem.IsWindows() ? 0.8 :
            OperatingSystem.IsMacOS() ? 0.95 :
            1.0;

        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            return values is not [Color color] ? null : new SolidColorBrush(color, opacity: Opacity);
        }
    }
}