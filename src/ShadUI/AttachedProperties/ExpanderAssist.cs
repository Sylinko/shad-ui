using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace ShadUI;

public static class ExpanderAssist
{
    public static readonly AttachedProperty<IBrush?> HeaderBackgroundProperty =
        AvaloniaProperty.RegisterAttached<Control, Control, IBrush?>("HeaderBackground");

    public static void SetHeaderBackground(Control obj, IBrush? value) => obj.SetValue(HeaderBackgroundProperty, value);

    public static IBrush? GetHeaderBackground(Control obj) => obj.GetValue(HeaderBackgroundProperty);
}