using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace ShadUI;

public class ButtonGroup : StackPanel
{
    /// <summary>
    /// Defines the <see cref="CornerRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        Border.CornerRadiusProperty.AddOwner<ButtonGroup>();

    /// <summary>
    /// Defines the <see cref="BorderBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        Border.BorderBrushProperty.AddOwner<ButtonGroup>();

    /// <summary>
    /// Defines the <see cref="BorderThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        Border.BorderThicknessProperty.AddOwner<ButtonGroup>();

    /// <summary>
    /// Gets or sets the corner radius of the button group.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the border brush of the button group.
    /// </summary>
    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the border thickness of the button group.
    /// </summary>
    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    static ButtonGroup()
    {
        AffectsRender<ButtonGroup>(CornerRadiusProperty, BorderBrushProperty, BorderThicknessProperty);
    }

    protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.ChildrenChanged(sender, e);

        UpdateChildrenStyles();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == OrientationProperty ||
            change.Property == CornerRadiusProperty ||
            change.Property == BorderBrushProperty ||
            change.Property == BorderThicknessProperty)
        {
            UpdateChildrenStyles();
        }
    }

    private void UpdateChildrenStyles()
    {
        var children = Children.OfType<Button>().ToList();
        var count = children.Count;
        if (count == 0) return;

        var orientation = Orientation;
        var radius = CornerRadius;
        var brush = BorderBrush;
        var thickness = BorderThickness;

        for (var i = 0; i < count; i++)
        {
            var button = children[i];
            var isFirst = i == 0;
            var isLast = i == count - 1;

            // Apply BorderBrush
            if (brush != null)
            {
                button.BorderBrush = brush;
            }

            // Apply BorderThickness
            // To avoid double borders, we can adjust thickness for adjacent items
            // However, simple implementation first as requested: "set child Button's these properties"
            // Usually ButtonGroup implies merging borders.
            // Let's assume standard behavior:
            // Horizontal: Middle items lose Left border (or Right, depending on direction).
            // Vertical: Middle items lose Top border (or Bottom).
            
            var newThickness = thickness;
            if (count > 1)
            {
                if (orientation == Orientation.Horizontal)
                {
                    // If not first, remove left border to avoid double thickness with previous button's right border
                    if (!isFirst)
                    {
                        newThickness = new Thickness(0, thickness.Top, thickness.Right, thickness.Bottom);
                    }
                }
                else
                {
                    // If not first, remove top border
                    if (!isFirst)
                    {
                        newThickness = new Thickness(thickness.Left, 0, thickness.Right, thickness.Bottom);
                    }
                }
            }
            button.BorderThickness = newThickness;

            // Apply CornerRadius
            var newRadius = new CornerRadius(0);
            if (count == 1)
            {
                newRadius = radius;
            }
            else
            {
                if (orientation == Orientation.Horizontal)
                {
                    if (isFirst)
                        newRadius = new CornerRadius(radius.TopLeft, 0, 0, radius.BottomLeft);
                    else if (isLast)
                        newRadius = new CornerRadius(0, radius.TopRight, radius.BottomRight, 0);
                }
                else // Vertical
                {
                    if (isFirst)
                        newRadius = new CornerRadius(radius.TopLeft, radius.TopRight, 0, 0);
                    else if (isLast)
                        newRadius = new CornerRadius(0, 0, radius.BottomRight, radius.BottomLeft);
                }
            }
            button.CornerRadius = newRadius;
        }
    }
}
