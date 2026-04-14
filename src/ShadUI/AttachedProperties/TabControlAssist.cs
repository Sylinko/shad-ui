using Avalonia;
using Avalonia.Controls;

namespace ShadUI;

public static class TabControlAssist
{
    /// <summary>
    /// Defines the HeaderContent attached property.
    /// </summary>
    public static readonly AttachedProperty<object?> HeaderContentProperty =
        AvaloniaProperty.RegisterAttached<TabControl, TabControl, object?>("HeaderContent");

    /// <summary>
    /// Sets the HeaderContent for the specified control.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public static void SetHeaderContent(TabControl obj, object? value) => obj.SetValue(HeaderContentProperty, value);

    /// <summary>
    /// Gets the HeaderContent for the specified control.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static object? GetHeaderContent(TabControl obj) => obj.GetValue(HeaderContentProperty);
}