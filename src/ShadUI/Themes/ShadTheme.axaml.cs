using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using ShadUI.Extensions;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     The main theme for the application.
/// </summary>
public class ShadTheme : Styles
{
    static ShadTheme()
    {
        ToolTip.ShowDelayProperty.ForceOverrideDefaultValue(typeof(Control), 50);
        ToolTip.PlacementProperty.ForceOverrideDefaultValue(typeof(Control), PlacementMode.Top);
        ToolTip.VerticalOffsetProperty.ForceOverrideDefaultValue(typeof(Control), 0d);
        ToolTip.PlacementProperty.Changed.AddClassHandler<Control>(HandleToolTipPlacementChanged);
    }

    private static void HandleToolTipPlacementChanged(Control sender, AvaloniaPropertyChangedEventArgs args)
    {
        switch (args.NewValue)
        {
            case PlacementMode.Top:
            {
                sender.SetValue(ToolTip.VerticalOffsetProperty, -3);
                sender.SetValue(ToolTip.HorizontalOffsetProperty, 0);
                break;
            }
            case PlacementMode.Bottom:
            {
                sender.SetValue(ToolTip.VerticalOffsetProperty, 3);
                sender.SetValue(ToolTip.HorizontalOffsetProperty, 0);
                break;
            }
            case PlacementMode.Left:
            {
                sender.SetValue(ToolTip.VerticalOffsetProperty, 0);
                sender.SetValue(ToolTip.HorizontalOffsetProperty, -3);
                break;
            }
            case PlacementMode.Right:
            {
                sender.SetValue(ToolTip.VerticalOffsetProperty, 0);
                sender.SetValue(ToolTip.HorizontalOffsetProperty, 3);
                break;
            }
        }
    }

    /// <summary>
    ///     Returns a new instance of the <see cref="ShadTheme" /> class.
    /// </summary>
    public ShadTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}