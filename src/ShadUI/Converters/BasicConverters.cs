using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Converters;
using Avalonia.Controls.Primitives.Converters;
using Avalonia.Data.Converters;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Provides wrapper access to common Avalonia converters for use in XAML.
/// </summary>
/// <remarks>
///     This class provides static properties that expose commonly used Avalonia converters
///     for easy access in XAML bindings.
/// </remarks>
public static class BasicConverters
{
    /// <summary>
    ///     Gets the EnumToBoolConverter instance for converting enum values to boolean.
    /// </summary>
    public static EnumToBoolConverter EnumToBoolConverter { get; } = new();

    /// <summary>
    ///     Gets the ToBrushConverter instance for converting values to brushes.
    /// </summary>
    public static ToBrushConverter ToBrushConverter { get; } = new();

    /// <summary>
    ///     Gets the DoNothingForNullConverter instance for handling null values.
    /// </summary>
    public static DoNothingForNullConverter DoNothingForNullConverter { get; } = new();

    /// <summary>
    ///     Gets the ColorToDisplayNameConverter instance for converting colors to display names.
    /// </summary>
    public static ColorToDisplayNameConverter ColorToDisplayNameConverter { get; } = new();

    /// <summary>
    ///     Gets the ContrastBrushConverter instance for generating contrasting brushes.
    /// </summary>
    public static ContrastBrushConverter ContrastBrushConverter { get; } = new();

    /// <summary>
    ///     Gets the AccentColorConverter instance for converting accent colors.
    /// </summary>
    public static AccentColorConverter AccentColorConverter { get; } = new();

    /// <summary>
    ///     Converts a <see cref="ColorPicker" />'s selected color to a string representation.
    /// </summary>
    public static IValueConverter ToColorStringConverter { get; } =
        new FuncValueConverter<ColorPicker, string, string>((picker, param) =>
        {
            if (picker is null) return "";

            var toUpper = param is "ToUpper";

            var color = new SolidColorBrush(picker.HsvColor.ToRgb()).ToString();

            if (picker is { IsAlphaEnabled: true, IsAlphaVisible: true }) return toUpper ? color.ToUpper() : color;

            var rgb = picker.HsvColor.ToRgb();
            color = $"#{rgb.R:X2}{rgb.G:X2}{rgb.B:X2}";

            return toUpper ? color.ToUpper() : color;
        });

    public static IValueConverter InvertThemeVariant { get; } =
        new FuncValueConverter<ThemeVariant?, ThemeVariant>(x => x?.Key switch
        {
            "Dark" => ThemeVariant.Light,
            "Light" => ThemeVariant.Dark,
            _ => x ?? ThemeVariant.Default
        });

    public static IValueConverter InvertOrientation { get; } =
        new FuncValueConverter<Orientation, Orientation>(x => x == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal);

        public static IValueConverter TypeEquals { get; } = new FuncValueConverter<object?, object?, bool>(
        convert: (x, parameter) => x?.GetType() == parameter as Type
    );

    public static IValueConverter FullPathToFileName { get; } = new FuncValueConverter<string, string?>(
        convert: x => Path.GetFileName(x) is { Length: > 0 } fileName ? fileName : x // return original if no file name found (e.g. Path root)
    );

    /// <summary>
    /// Converts an Enum Type to its values array.
    /// </summary>
    public static IValueConverter EnumTypeToValues { get; } = new FuncValueConverter<Type?, Type?, Array?>(
        convert: (x, parameter) =>
        {
            var type = x ?? parameter;
            return type?.IsEnum is true ? Enum.GetValues(type) : null;
        });

    public static IValueConverter IndexFromContainer { get; } = new FuncValueConverter<object?, int>(
        convert: x =>
        {
            if (x is not Control itemContainer) return -1;
            var itemsControl = ItemsControl.ItemsControlFromItemContainer(itemContainer);
            return itemsControl?.IndexFromContainer(itemContainer) ?? -1;
        });

    public static IMultiValueConverter AllEquals { get; } = new AllEqualsConverter();

    /// <summary>
    /// Returns the first non-null and non-UnsetValue value from the input values.
    /// </summary>
    public static IMultiValueConverter FirstNotNull { get; } = new FirstNotNullConverter();

    /// <summary>
    /// Returns the first non-null, non-empty (for strings), and non-UnsetValue value from the input values.
    /// </summary>
    public static IMultiValueConverter FirstNotNullOrEmpty { get; } = new FirstNotNullOrEmptyConverter();

    private sealed class AllEqualsConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            var first = values.FirstOrDefault(v => v != AvaloniaProperty.UnsetValue);
            return first != null && values.Skip(1).All(v => v == first);
        }
    }

    private sealed class FirstNotNullConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            return values.OfType<object>().FirstOrDefault(value => value != AvaloniaProperty.UnsetValue);
        }
    }

    private sealed class FirstNotNullOrEmptyConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            return values.FirstOrDefault(value =>
            {
                if (value == AvaloniaProperty.UnsetValue) return false;
                if (value is string str) return !string.IsNullOrEmpty(str);
                return true;
            });
        }
    }
}