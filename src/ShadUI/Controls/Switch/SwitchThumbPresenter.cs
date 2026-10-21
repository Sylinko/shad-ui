using Avalonia;
using Avalonia.Controls;

namespace ShadUI;

/// <summary>
/// Places a switch thumb along its available travel, keeping the selected edge fixed as the thumb changes width.
/// </summary>
public class SwitchThumbPresenter : Decorator
{
    /// <summary>
    /// Defines the thumb's position: zero anchors the left edge and one anchors the right edge.
    /// </summary>
    public static readonly StyledProperty<double> PositionProperty =
        AvaloniaProperty.Register<SwitchThumbPresenter, double>(nameof(Position));

    /// <summary>
    /// Gets or sets the normalized position. Animate this value to move between the two endpoints.
    /// </summary>
    public double Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    static SwitchThumbPresenter()
    {
        AffectsArrange<SwitchThumbPresenter>(PositionProperty);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Child is { } child)
        {
            var bounds = new Rect(finalSize).Deflate(Padding);
            var size = child.DesiredSize;

            // Width and travel may have independent transitions. Deriving the origin from the current width
            // keeps either endpoint exact on every frame, without a second animation compensating for growth.
            var origin = new Point(bounds.X + (bounds.Width - size.Width) * Position, bounds.Y + (bounds.Height - size.Height) / 2);
            child.Arrange(new Rect(origin, size));
        }

        return finalSize;
    }
}