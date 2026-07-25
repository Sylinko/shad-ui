using Avalonia;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;

namespace ShadUI;

/// <summary>
/// Behavior that shows a ToolTip when the associated TextBlock's text is trimmed (collapsed).
/// If the text layout contains collapsed lines, the value of <see cref="Tip"/> is applied as
/// the ToolTip. When the text is not trimmed the ToolTip is removed.
/// </summary>
public sealed class TextTrimmedToolTipBehavior : Behavior<TextBlock>
{
    /// <summary>
    /// Defines the <see cref="Tip"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> TipProperty =
        AvaloniaProperty.Register<TextTrimmedToolTipBehavior, object?>(nameof(Tip));

    /// <summary>
    /// Gets or sets the tooltip content to display when the associated TextBlock's text is trimmed.
    /// Can be any object usable by <see cref="ToolTip.SetTip"/> (string, control, etc.).
    /// </summary>
    public object? Tip
    {
        get => GetValue(TipProperty);
        set => SetValue(TipProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Target"/> property.
    /// </summary>
    public static readonly StyledProperty<Control?> TargetProperty =
        AvaloniaProperty.Register<TextTrimmedToolTipBehavior, Control?>(nameof(Target));

    /// <summary>
    /// Gets or sets the target Control for the <see cref="Tip"/>
    /// </summary>
    public Control? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    protected override void OnAttached()
    {
        AssociatedObject?.LayoutUpdated += HandleLayoutUpdated;
    }

    protected override void OnDetaching()
    {
        AssociatedObject?.LayoutUpdated -= HandleLayoutUpdated;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TipProperty && AssociatedObject?.TextLayout.TextLines.Any(x => x.HasCollapsed) is true)
        {
            ToolTip.SetTip(Target ?? AssociatedObject, Tip);
        }
    }

    private void HandleLayoutUpdated(object? sender, EventArgs e)
    {
        var target = Target ?? AssociatedObject;
        if (target is null) return;

        if (AssociatedObject?.TextLayout.TextLines.Any(x => x.HasCollapsed) is true)
        {
            ToolTip.SetTip(target, Tip);
        }
        else if (AssociatedObject is not null)
        {
            ToolTip.SetTip(target, null);
        }
    }
}