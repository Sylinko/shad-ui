using Avalonia.Data.Converters;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Provides value converters for converting DialogButtonStyle enum values to CSS class names.
/// </summary>
/// <remarks>
///     This class contains static converters that can be used to convert DialogButtonStyle enum values
///     to their corresponding CSS class names for styling purposes.
/// </remarks>
public static class ButtonStyleConverters
{
    /// <summary>
    ///     Converts a DialogButtonStyle enum value to its corresponding CSS class name.
    /// </summary>
    /// <remarks>
    ///     This converter maps DialogButtonStyle enum values to their corresponding CSS class names:
    ///     - Primary → "Primary"
    ///     - Secondary → "Secondary"
    ///     - Outline → "Outline"
    ///     - Ghost → "Ghost"
    ///     - Destructive → "Destructive"
    /// </remarks>
    public static IValueConverter ToClass { get; } =
        new FuncValueConverter<ButtonStyle, string>(value => value switch
        {
            ButtonStyle.Primary => "Primary",
            ButtonStyle.Secondary => "Secondary",
            ButtonStyle.Outline => "Outline",
            ButtonStyle.Ghost => "Ghost",
            ButtonStyle.Destructive => "Destructive",
            _ => ""
        });
}