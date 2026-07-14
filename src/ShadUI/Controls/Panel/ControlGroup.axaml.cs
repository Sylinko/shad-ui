using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace ShadUI;

public sealed class ControlGroup : StackPanel
{
    /// <summary>
    /// Defines the <see cref="CornerRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        Border.CornerRadiusProperty.AddOwner<ControlGroup>();

    /// <summary>
    /// Defines the <see cref="BorderBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        Border.BorderBrushProperty.AddOwner<ControlGroup>();

    /// <summary>
    /// Defines the <see cref="BorderThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        Border.BorderThicknessProperty.AddOwner<ControlGroup>();

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

    private readonly HashSet<Control> _listeningChildren = [];

    static ControlGroup()
    {
        AffectsRender<ControlGroup>(CornerRadiusProperty, BorderBrushProperty, BorderThicknessProperty);
    }

    protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.ChildrenChanged(sender, e);

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                    foreach (var item in e.NewItems.OfType<Control>())
                        StartListening(item);
                break;
            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                    foreach (var item in e.OldItems.OfType<Control>())
                        StopListening(item);
                break;
            case NotifyCollectionChangedAction.Replace:
                if (e.OldItems != null)
                    foreach (var item in e.OldItems.OfType<Control>())
                        StopListening(item);
                if (e.NewItems != null)
                    foreach (var item in e.NewItems.OfType<Control>())
                        StartListening(item);
                break;
            case NotifyCollectionChangedAction.Reset:
                foreach (var child in _listeningChildren.ToList())
                {
                    StopListening(child);
                }
                foreach (var child in Children.OfType<Control>())
                {
                    StartListening(child);
                }
                break;
        }

        UpdateChildrenStyles();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        foreach (var child in _listeningChildren) child.PropertyChanged -= HandleChildPropertyChanged;
        _listeningChildren.Clear();
    }

    private void StartListening(Control c)
    {
        if (_listeningChildren.Add(c))
        {
            c.PropertyChanged += HandleChildPropertyChanged;
        }
    }

    private void StopListening(Control c)
    {
        if (_listeningChildren.Remove(c))
        {
            c.PropertyChanged -= HandleChildPropertyChanged;
        }
    }

    private void HandleChildPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == IsVisibleProperty)
        {
            UpdateChildrenStyles();
        }
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
        var visibleChildren = Children.Where(c => c.IsVisible).ToList();
        var count = visibleChildren.Count;
        if (count == 0) return;

        var orientation = Orientation;
        var radius = CornerRadius;
        var brush = BorderBrush;
        var thickness = BorderThickness;

        for (var i = 0; i < count; i++)
        {
            var child = visibleChildren[i];

            // Apply BorderThickness
            // To avoid double borders, we can adjust thickness for adjacent items
            // However, simple implementation first as requested: "set child Control's these properties"
            // Usually ControlGroup implies merging borders.
            // Let's assume standard behavior:
            // Horizontal: Middle items lose Left border (or Right, depending on direction).
            // Vertical: Middle items lose Top border (or Bottom).

            CornerRadius cornerRadius;
            if (count > 1 && i > 0)
            {
                if (orientation == Orientation.Horizontal)
                {
                    child.Margin = new Thickness(-thickness.Left, 0, 0, 0);
                    cornerRadius = i == count - 1 ? new CornerRadius(0, radius.TopRight, radius.BottomRight, 0) : default;
                }
                else
                {
                    child.Margin = new Thickness(0, -thickness.Top, 0, 0);
                    cornerRadius = i == count - 1 ? new CornerRadius(0, 0, radius.BottomRight, radius.BottomLeft) : default;
                }
            }
            else
            {
                child.Margin = new Thickness(0); // First item has no negative margin
                cornerRadius = count == 1 ? radius : // Only 1 item, use full radius
                    orientation == Orientation.Horizontal ? // More than 1, depending on orientation
                        new CornerRadius(radius.TopLeft, 0, 0, radius.BottomLeft) :
                        new CornerRadius(radius.TopLeft, radius.TopRight, 0, 0);
            }

            switch (child)
            {
                case TemplatedControl templatedControl:
                {
                    if (brush != null) templatedControl.BorderBrush = brush;
                    templatedControl.BorderThickness = thickness;
                    templatedControl.CornerRadius = cornerRadius;
                    break;
                }
                case Border border:
                {
                    if (brush != null) border.BorderBrush = brush;
                    border.BorderThickness = thickness;
                    border.CornerRadius = cornerRadius;
                    break;
                }
            }
        }
    }
}