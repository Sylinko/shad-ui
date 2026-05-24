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
    }

    /// <summary>
    ///     Returns a new instance of the <see cref="ShadTheme" /> class.
    /// </summary>
    public ShadTheme()
    {
        AvaloniaXamlLoader.Load(this);
    }
}