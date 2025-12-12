using System.Diagnostics;
using Avalonia;

namespace ShadUI;

public static class DebugAssist
{
    public static readonly AttachedProperty<object?> WriteLineProperty =
        AvaloniaProperty.RegisterAttached<AvaloniaObject, AvaloniaObject, object?>("WriteLine");

    public static void SetWriteLine(AvaloniaObject obj, object? value) => obj.SetValue(WriteLineProperty, value);
    
    public static object? GetWriteLine(AvaloniaObject obj) => obj.GetValue(WriteLineProperty);

    static DebugAssist()
    {
        WriteLineProperty.Changed.AddClassHandler<AvaloniaObject>(HandleWriteLinePropertyChanged);
    }

    private static void HandleWriteLinePropertyChanged(AvaloniaObject sender, AvaloniaPropertyChangedEventArgs args)
    {
        Debug.WriteLine($"[{sender}] {args.OldValue} -> {args.NewValue}");
    }
}