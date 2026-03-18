using Avalonia;
using Avalonia.Controls;

namespace ShadUI;

public class NavigationBarItemChildrenExpander : Decorator
{
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<NavigationBarItemChildrenExpander, bool>(
            nameof(IsExpanded), true);

    public static readonly StyledProperty<double> ExpansionProgressProperty =
        AvaloniaProperty.Register<NavigationBarItemChildrenExpander, double>(
            nameof(ExpansionProgress), 1.0);

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public double ExpansionProgress
    {
        get => GetValue(ExpansionProgressProperty);
        set => SetValue(ExpansionProgressProperty, value);
    }

    public NavigationBarItemChildrenExpander()
    {
        // Clip out-of-bounds content to prevent overflow during the collapse animation
        ClipToBounds = true;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsExpandedProperty)
        {
            // Update the progress target.
            // If the caller has defined a Transition for ExpansionProgress in Styles,
            // Avalonia will automatically animate this value change.
            ExpansionProgress = IsExpanded ? 1.0 : 0.0;
        }
        else if (change.Property == ExpansionProgressProperty)
        {
            // Invalidate the layout whenever the progress value changes (e.g., every animation frame)
            InvalidateMeasure();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var child = Child;
        if (child == null)
            return default;

        // Give the child infinite height to measure its true fully-expanded size
        child.Measure(new Size(availableSize.Width, double.PositiveInfinity));

        var desiredSize = child.DesiredSize;

        // Report the scaled height based on the current animation progress
        return new Size(desiredSize.Width, desiredSize.Height * ExpansionProgress);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var child = Child;
        if (child == null) return finalSize;

        // Always arrange the child with its full desired height, anchored at the top (0,0).
        // Combined with ClipToBounds = true, this creates the visual effect of the content
        // being pushed up/clipped rather than being squished.
        child.Arrange(new Rect(0, 0, finalSize.Width, child.DesiredSize.Height));

        return finalSize;
    }
}