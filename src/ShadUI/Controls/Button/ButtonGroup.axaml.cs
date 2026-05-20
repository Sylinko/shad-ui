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

    private readonly HashSet<Control> _listeningChildren = new();

    static ButtonGroup()
    {
        AffectsRender<ButtonGroup>(CornerRadiusProperty, BorderBrushProperty, BorderThicknessProperty);
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
        var children = Children.OfType<Button>().ToList();
        var count = children.Count;
        if (count == 0) return;

        var orientation = Orientation;
        var radius = CornerRadius;
        var brush = BorderBrush;
        var thickness = BorderThickness;

        Button? firstButton = null;
        Button? lastButton = null;

        for (var i = 0; i < count; i++)
        {
            var button = children[i];
            if (!button.IsVisible) continue;

            firstButton ??= button;
            lastButton = button;

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
            
            var newMargin = new Thickness();
            if (button != firstButton)
            {
                if (orientation == Orientation.Horizontal)
                {
                    newMargin = new Thickness(-thickness.Left, 0, 0, 0);
                }
                else
                {
                    newMargin = new Thickness(0, -thickness.Top, 0, 0);
                }
            }

            button.Margin = newMargin;
            button.CornerRadius = new CornerRadius(0);
        }

        // Apply CornerRadius to first and last buttons
        if (firstButton == null) return;

        if (firstButton == lastButton)
        {
            // only one button
            firstButton.CornerRadius = radius;
        }
        else
        {
            if (orientation == Orientation.Horizontal)
            {
                firstButton.CornerRadius = new CornerRadius(radius.TopLeft, 0, 0, radius.BottomLeft);
                lastButton?.CornerRadius = new CornerRadius(0, radius.TopRight, radius.BottomRight, 0);
            }
            else
            {
                firstButton.CornerRadius = new CornerRadius(radius.TopLeft, radius.TopRight, 0, 0);
                lastButton?.CornerRadius = new CornerRadius(0, 0, radius.BottomRight, radius.BottomLeft);
            }
        }
    }
}
