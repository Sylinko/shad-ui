using Avalonia.Data.Converters;

// ReSharper disable once CheckNamespace
namespace ShadUI;


/// <summary>
///     Provides value converters for double operations and comparisons.
/// </summary>
/// <remarks>
///     This class contains static converters that can be used for string comparison and validation
///     operations in XAML bindings.
/// </remarks>
public static class DoubleConverters
{
    /// <summary>
    ///     Converts a double value to a boolean indicating whether it is zero.
    /// </summary>
    public static IValueConverter IsNaN { get; } = new FuncValueConverter<double, bool>(
        double.IsNaN
    );

    /// <summary>
    ///     Converts a double value to a boolean indicating whether it is NaN (Not a Number).
    /// </summary>
    public static IValueConverter IsNotNaN { get; } = new FuncValueConverter<double, bool>(
        value => !double.IsNaN(value)
    );
}