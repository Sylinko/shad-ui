using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace ShadUI;

public class ExpandableDecorator : Decorator
{
    /// <summary>
    /// Controls the expansion state. 
    /// True = Expanded, False = Collapsed.
    /// </summary>
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<ExpandableDecorator, bool>(nameof(IsExpanded), true);

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>
    /// The direction in which the control expands.
    /// Horizontal = Animate Width. Vertical = Animate Height.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<ExpandableDecorator, Orientation>(nameof(Orientation));

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// The animation progress. 0.0 is fully collapsed, 1.0 is fully expanded.
    /// Animate this property using Transitions in XAML.
    /// </summary>
    public static readonly StyledProperty<double> ProgressProperty =
        AvaloniaProperty.Register<ExpandableDecorator, double>(nameof(Progress), 1.0);

    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    static ExpandableDecorator()
    {
        AffectsMeasure<ExpandableDecorator>(ProgressProperty, OrientationProperty);
        
        IsExpandedProperty.Changed.AddClassHandler<ExpandableDecorator>((x, _) => x.UpdateProgress());
        
        // Important: Clip content during animation to prevent overflow
        ClipToBoundsProperty.OverrideDefaultValue<ExpandableDecorator>(true);
    }

    private void UpdateProgress()
    {
        // Use SetValue to ensure the value is set with higher precedence, 
        // preventing potential binding fallbacks or style interferences.
        // The Transition system will still pick this up and animate it.
        SetValue(ProgressProperty, IsExpanded ? 1.0 : 0.0);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var child = Child;
        if (child == null) return new Size();

        Size childConstraint;
        if (Orientation == Orientation.Horizontal)
        {
            // Animate Width: Give infinite width to find natural size, respect available height
            childConstraint = new Size(double.PositiveInfinity, availableSize.Height);
        }
        else
        {
            // Animate Height: Give infinite height to find natural size, respect available width
            childConstraint = new Size(availableSize.Width, double.PositiveInfinity);
        }

        child.Measure(childConstraint);
        var desiredSize = child.DesiredSize;
        var width = desiredSize.Width;
        var height = desiredSize.Height;

        if (Orientation == Orientation.Horizontal)
        {
            var minWidth = double.IsNaN(MinWidth) ? 0 : MinWidth;
            var maxWidth = double.IsNaN(MaxWidth) ? double.PositiveInfinity : MaxWidth;
            var expandedWidth = Math.Min(desiredSize.Width, maxWidth);
            var collapsedWidth = minWidth;
            
            // Interpolate
            width = collapsedWidth + (expandedWidth - collapsedWidth) * Progress;
            
            // Clamp to ensure valid size even if Easing overshoots
            width = Math.Max(0, width);
        }
        else
        {
            var minHeight = double.IsNaN(MinHeight) ? 0 : MinHeight;
            var maxHeight = double.IsNaN(MaxHeight) ? double.PositiveInfinity : MaxHeight;
            var expandedHeight = Math.Min(desiredSize.Height, maxHeight);

            // Interpolate
            height = minHeight + (expandedHeight - minHeight) * Progress;
            
            // Clamp
            height = Math.Max(0, height);
        }

        return new Size(width, height);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var child = Child;
        if (child == null) return finalSize;

        // We arrange the child with the final interpolated size.
        // This ensures that background elements (like Borders) resize correctly with the animation.
        // Note: Content inside the child must handle the reduced size gracefully 
        // (e.g., by using TextWrapping="NoWrap", ClipToBounds=True, or fixed sizes for icons).
        child.Arrange(new Rect(finalSize));
        
        return finalSize;
    }
}