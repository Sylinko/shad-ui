using System.Globalization;
using Avalonia.Data.Converters;

namespace ShadUI;

/// <summary>
///     Provides value converters for boolean operations and transformations.
/// </summary>
/// <remarks>
///     This class contains static converters that can be used to transform boolean values
///     into other types such as opacity values, loading controls, or numeric values.
/// </remarks>
public static class BooleanConverters
{
    /// <summary>
    ///     Converts a boolean value to an inverse opacity value (0 for true, 1 for false).
    /// </summary>
    /// <remarks>
    ///     This converter is useful for hiding elements when a condition is true.
    ///     Returns 0 (fully transparent) when the boolean is true, and 1 (fully opaque) when false.
    /// </remarks>
    public static IValueConverter ToInverseOpacity { get; } =
        new FuncValueConverter<bool, int>(value => value ? 0 : 1);

    /// <summary>
    ///     Converts a boolean value to either a Loading control or null.
    /// </summary>
    /// <remarks>
    ///     This converter returns a Loading control when the boolean is true, and a Panel when false.
    ///     Useful for showing loading states in UI elements.
    /// </remarks>
    public static IValueConverter ToLoading { get; } =
        new FuncValueConverter<bool, object?>(value => value ? new Loading() : null);

    /// <summary>
    ///     Needs at least 2 values: [bool?, valueIfTrue, (optional) valueIfFalse, (optional) valueIfNull]
    /// </summary>
    public static IMultiValueConverter Gate { get; } = new GateImpl();

    private sealed class GateImpl : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Count < 2)
                throw new ArgumentException("Gate converter needs at least 2 values.");

            var condition = values[0];
            var valueIfTrue = values[1];
            var valueIfFalse = values.Count >= 3 ? values[2] : null;
            var valueIfNull = values.Count >= 4 ? values[3] : null;

            return condition switch
            {
                true => valueIfTrue,
                false => valueIfFalse,
                _ => valueIfNull
            };
        }
    }
}