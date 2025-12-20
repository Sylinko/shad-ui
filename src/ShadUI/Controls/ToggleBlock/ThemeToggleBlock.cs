using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;
using Avalonia.Styling;

namespace ShadUI;

/// <summary>
/// A toggle block control that switches content based on the theme variant.
/// </summary>
public class ThemeToggleBlock : ContentControl
{
    public static readonly StyledProperty<IControlTemplate?> DarkContentProperty =
        AvaloniaProperty.Register<ThemeToggleBlock, IControlTemplate?>(nameof(DarkContent));

    public IControlTemplate? DarkContent
    {
        get => GetValue(DarkContentProperty);
        set => SetValue(DarkContentProperty, value);
    }

    public static readonly StyledProperty<IControlTemplate?> LightContentProperty =
        AvaloniaProperty.Register<ThemeToggleBlock, IControlTemplate?>(nameof(LightContent));

    public IControlTemplate? LightContent
    {
        get => GetValue(LightContentProperty);
        set => SetValue(LightContentProperty, value);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        ApplyContent(ActualThemeVariant);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property.Name == "ActualThemeVariant" && change.NewValue is ThemeVariant themeVariant)
        {
            ApplyContent(themeVariant);
        }
    }

    private void ApplyContent(ThemeVariant? themeVariant)
    {
        switch (themeVariant?.Key)
        {
            case "Light":
            {
                Content = LightContent?.Build(this)?.Result;
                break;
            }
            case "Dark":
            {
                Content = DarkContent?.Build(this)?.Result;
                break;
            }
            default:
            {
                Content = null;
                break;
            }
        }
    }
}