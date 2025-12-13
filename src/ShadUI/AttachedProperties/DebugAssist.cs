using System.Diagnostics;
using Avalonia;

namespace ShadUI;

/// <summary>
///     Provides attached properties and methods for assisting with debugging.
/// </summary>
public static class DebugAssist
{
    /// <summary>
    ///     Defines an attached property that writes the value to the debug output when it changes.
    /// </summary>
    public static readonly AttachedProperty<object?> WriteLineProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, AvaloniaObject, object?>("WriteLine");

    /// <summary>
    ///     Sets the value to be written to the debug output.
    /// </summary>
    /// <param name="obj">The object to set the value on.</param>
    /// <param name="value">The value to write.</param>
    public static void SetWriteLine(AvaloniaObject obj, object? value) => obj.SetValue(WriteLineProperty, value);

    /// <summary>
    ///     Gets the value to be written to the debug output.
    /// </summary>
    /// <param name="obj">The object to get the value from.</param>
    /// <returns>The value.</returns>
    public static object? GetWriteLine(AvaloniaObject obj) => obj.GetValue(WriteLineProperty);

    static DebugAssist()
    {
        WriteLineProperty.Changed.AddClassHandler<AvaloniaObject>(HandleWriteLinePropertyChanged);
    }

    private static void HandleWriteLinePropertyChanged(AvaloniaObject sender, AvaloniaPropertyChangedEventArgs args) =>
        Debug.WriteLine($"[{sender}] {args.OldValue} -> {args.NewValue}");
}