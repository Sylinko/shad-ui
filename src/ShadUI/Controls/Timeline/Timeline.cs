using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace ShadUI;

/// <summary>
/// A container that draws a timeline connecting attached items.
/// Items can be anywhere in the visual tree under this container.
/// Use <see cref="Timeline.AnchorProperty"/> to mark items.
/// </summary>
public class Timeline : Decorator
{
    private readonly List<Control> _items = [];

    #region Properties

    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Timeline, Orientation>(nameof(Orientation), Orientation.Vertical);

    /// <summary>
    /// Gets or sets the orientation of the timeline.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="IgnoreSingleItem"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IgnoreSingleItemProperty =
        AvaloniaProperty.Register<Timeline, bool>(nameof(IgnoreSingleItem));

    /// <summary>
    /// Gets or sets whether to ignore rendering when there is only a single item.
    /// </summary>
    public bool IgnoreSingleItem
    {
        get => GetValue(IgnoreSingleItemProperty);
        set => SetValue(IgnoreSingleItemProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="DotSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> DotSizeProperty =
        AvaloniaProperty.Register<Timeline, double>(nameof(DotSize), 10.0);

    /// <summary>
    /// Gets or sets the size of the dots on the timeline.
    /// </summary>
    public double DotSize
    {
        get => GetValue(DotSizeProperty);
        set => SetValue(DotSizeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Fill"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> FillProperty =
        AvaloniaProperty.Register<Timeline, IBrush>(nameof(Fill), Brushes.Gray);

    /// <summary>
    /// Gets or sets the fill brush for the dots on the timeline.
    /// </summary>
    public IBrush Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Spacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty =
        AvaloniaProperty.Register<Timeline, double>(nameof(Spacing));

    /// <summary>
    /// Gets or sets the spacing between the dot and the line.
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="StrokeThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<Timeline, double>(nameof(StrokeThickness), 2.0);

    /// <summary>
    /// Gets or sets the stroke thickness for the timeline lines.
    /// </summary>
    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Stroke"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush> StrokeProperty =
        AvaloniaProperty.Register<Timeline, IBrush>(nameof(Stroke), Brushes.Gray);

    /// <summary>
    /// Gets or sets the stroke brush for the timeline lines.
    /// </summary>
    public IBrush Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    #endregion

    #region Attached Properties

    /// <summary>
    /// Defines the anchor point for the timeline connection, relative to the attached control.
    /// Only the relevant coordinate (Y for Vertical, X for Horizontal) is used for sorting.
    /// </summary>
    public static readonly AttachedProperty<Point?> AnchorProperty =
        AvaloniaProperty.RegisterAttached<Timeline, Control, Point?>("Anchor");

    public static Point? GetAnchor(Control element) => element.GetValue(AnchorProperty);

    public static void SetAnchor(Control element, Point? value) => element.SetValue(AnchorProperty, value);

    /// <summary>
    /// Defines the IsBreak attached property.
    /// </summary>
    public static readonly AttachedProperty<bool> IsBreakProperty =
        AvaloniaProperty.RegisterAttached<Timeline, Control, bool>("IsBreak");

    public static bool GetIsBreak(Control element) => element.GetValue(IsBreakProperty);

    public static void SetIsBreak(Control element, bool value) => element.SetValue(IsBreakProperty, value);

    #endregion

    static Timeline()
    {
        AffectsRender<Timeline>(
            OrientationProperty,
            DotSizeProperty,
            FillProperty,
            StrokeThicknessProperty,
            StrokeProperty,
            SpacingProperty);

        AnchorProperty.Changed.AddClassHandler<Control>(OnAnchorChanged);
        IsBreakProperty.Changed.AddClassHandler<Control>(OnIsBreakChanged);
    }

    public Timeline()
    {
        LayoutUpdated += (_, _) => InvalidateVisual();
    }

    private static void OnIsBreakChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        var timeline = control.FindAncestorOfType<Timeline>();
        timeline?.InvalidateVisual();
    }

    private static void OnAnchorChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is Point)
        {
            control.AttachedToVisualTree += OnItemAttached;
            control.DetachedFromVisualTree += OnItemDetached;

            // If already attached, try to register immediately
            if (control.IsLoaded)
            {
                RegisterItem(control);
            }
        }
        else
        {
            control.AttachedToVisualTree -= OnItemAttached;
            control.DetachedFromVisualTree -= OnItemDetached;
            UnregisterItem(control);
        }
    }

    private static void OnItemAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control control)
        {
            RegisterItem(control);
        }
    }

    private static void OnItemDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control control)
        {
            UnregisterItem(control);
        }
    }

    private static void RegisterItem(Control control)
    {
        var timeline = control.FindAncestorOfType<Timeline>();
        timeline?.AddItem(control);
    }

    private static void UnregisterItem(Control control)
    {
        // We can't easily find the *old* timeline if detached, 
        // but the timeline itself can clean up invalid items during Render.
        // However, for correctness, we can try to find it if still possible,
        // or rely on the Timeline list management.

        // In this implementation, we rely on the Timeline instance methods.
        // Since we don't store the reference to Timeline in the child, 
        // we have to search. If detached, FindAncestor won't work.
        // So we might need to iterate all Timelines? No, that's too heavy.

        // Optimization: The Timeline will filter out detached items during Render.
        // But to prevent memory leaks, we should try to remove it if possible.
        // For now, we'll rely on the Render loop to clean up or just keep it simple.
        // Actually, let's just try to find it.
        var timeline = control.FindAncestorOfType<Timeline>();
        timeline?.RemoveItem(control);
    }

    private void AddItem(Control item)
    {
        if (_items.Contains(item)) return;

        _items.Add(item);
        InvalidateVisual();
    }

    private void RemoveItem(Control item)
    {
        if (_items.Remove(item))
        {
            InvalidateVisual();
        }
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (_items.Count == 0) return;

        // 1. Collect valid points
        var allPoints = new List<(Point Point, Control Item)>(_items.Count);

        for (var i = 0; i < _items.Count; i++)
        {
            var item = _items[i];

            // Ensure item is still a descendant
            if (!this.IsVisualAncestorOf(item))
            {
                _items.RemoveAt(i--);
                continue;
            }

            if (!item.IsEffectivelyVisible)
            {
                // Skip invisible or detached items
                continue;
            }

            var anchor = GetAnchor(item);
            if (anchor == null) continue;

            // Transform anchor to Timeline coordinates
            var matrix = item.TransformToVisual(this);
            if (matrix.HasValue)
            {
                var p = anchor.Value.Transform(matrix.Value);
                allPoints.Add((p, item));
            }
        }

        if (allPoints.Count == 0) return;

        // 2. Sort points based on Orientation
        if (Orientation == Orientation.Vertical)
        {
            allPoints.Sort((a, b) => a.Point.Y.CompareTo(b.Point.Y));
        }
        else
        {
            allPoints.Sort((a, b) => a.Point.X.CompareTo(b.Point.X));
        }

        // 3. Split into segments
        var segments = new List<List<(Point Point, Control Item)>>();
        var currentSegment = new List<(Point Point, Control Item)>();

        foreach (var point in allPoints)
        {
            if (GetIsBreak(point.Item))
            {
                if (currentSegment.Count > 0)
                {
                    segments.Add(currentSegment);
                    currentSegment = [];
                }
            }
            else
            {
                currentSegment.Add(point);
            }
        }
        if (currentSegment.Count > 0)
        {
            segments.Add(currentSegment);
        }

        // 4. Draw
        var linePen = new Pen(Stroke, StrokeThickness, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
        var dotBrush = Fill;
        var dotRadius = DotSize / 2;
        var spacing = Spacing;
        var isVertical = Orientation == Orientation.Vertical;

        foreach (var segment in segments)
        {
            if (segment.Count <= 1 && IgnoreSingleItem) continue;

            for (var i = 0; i < segment.Count; i++)
            {
                var originalPoint = segment[i].Point;
                var current = isVertical ? new Point(0, originalPoint.Y) : new Point(originalPoint.X, 0);

                // Draw Dot
                context.DrawEllipse(dotBrush, null, current, dotRadius, dotRadius);

                // Draw Line to next
                if (i >= segment.Count - 1) continue;

                var nextOriginal = segment[i + 1].Point;
                var next = isVertical ? new Point(0, nextOriginal.Y) : new Point(nextOriginal.X, 0);

                // Apply spacing
                var diff = next - current;
                var length = Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
                var offset = dotRadius + spacing;

                if (length <= offset * 2) continue;

                Point start, end;
                if (isVertical)
                {
                    start = new Point(current.X, current.Y + offset);
                    end = new Point(next.X, next.Y - offset);
                }
                else
                {
                    start = new Point(current.X + offset, current.Y);
                    end = new Point(next.X - offset, next.Y);
                }

                context.DrawLine(linePen, start, end);
            }
        }
    }
}