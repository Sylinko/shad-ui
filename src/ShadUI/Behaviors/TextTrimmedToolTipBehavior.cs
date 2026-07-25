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
            ToolTip.SetTip(AssociatedObject, Tip);
        }
    }

    private void HandleLayoutUpdated(object? sender, EventArgs e)
    {
        if (AssociatedObject?.TextLayout.TextLines.Any(x => x.HasCollapsed) is true)
        {
            ToolTip.SetTip(AssociatedObject, Tip);
        }
        else if (AssociatedObject is not null)
        {
            ToolTip.SetTip(AssociatedObject, null);
        }
    }
}