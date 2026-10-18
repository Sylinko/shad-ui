using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace ShadUI;

/// <summary>
///     Renders a compact circular progress indicator.
/// </summary>
/// <remarks>
///     <see cref="RangeBase.Value" /> is normalized against
///     <see cref="RangeBase.Minimum" /> and <see cref="RangeBase.Maximum" />; the default range is
///     0 to 1 so callers can bind a progress ratio directly. Set <see cref="IsIndeterminate" /> for
///     work whose length is unknown; the ring then shows a rotating segment instead of a value arc.
/// </remarks>
[PseudoClasses(":indeterminate")]
public class ProgressRing : RangeBase
{
    /// <summary>
    ///     Defines the <see cref="Size" /> property.
    /// </summary>
    public static readonly StyledProperty<double> SizeProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(Size), 24d);

    /// <summary>
    ///     Defines the <see cref="IsIndeterminate" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(nameof(IsIndeterminate));

    /// <summary>
    ///     Defines the <see cref="TrackBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> TrackBrushProperty =
        AvaloniaProperty.Register<ProgressRing, IBrush?>(nameof(TrackBrush));

    /// <summary>
    ///     Defines the <see cref="IndicatorBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> IndicatorBrushProperty =
        AvaloniaProperty.Register<ProgressRing, IBrush?>(nameof(IndicatorBrush));

    /// <summary>
    ///     Defines the <see cref="StrokeThickness" /> property.
    /// </summary>
    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(StrokeThickness), 2d);

    private TopLevel? _topLevel;
    private bool _animationStarted;
    private TimeSpan? _lastFrameTime;
    private double _animationAngle;

    static ProgressRing()
    {
        MaximumProperty.OverrideDefaultValue<ProgressRing>(1d);

        AffectsRender<ProgressRing>(
            ValueProperty,
            MinimumProperty,
            MaximumProperty,
            IsIndeterminateProperty,
            TrackBrushProperty,
            IndicatorBrushProperty,
            StrokeThicknessProperty);
    }

    public ProgressRing()
    {
        this[!WidthProperty] = this[!SizeProperty];
        this[!HeightProperty] = this[!SizeProperty];
        UpdatePseudoClasses(IsIndeterminate);
    }

    /// <summary>
    ///     Gets or sets the outer diameter of the ring.
    /// </summary>
    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    ///     Gets or sets whether the ring shows a rotating segment instead of a determinate value arc.
    /// </summary>
    public bool IsIndeterminate
    {
        get => GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>
    ///     Gets or sets the brush used for the full-circle track.
    /// </summary>
    public IBrush? TrackBrush
    {
        get => GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    /// <summary>
    ///     Gets or sets the brush used for the progress arc or rotating segment.
    /// </summary>
    public IBrush? IndicatorBrush
    {
        get => GetValue(IndicatorBrushProperty);
        set => SetValue(IndicatorBrushProperty, value);
    }

    /// <summary>
    ///     Gets or sets the ring stroke thickness.
    /// </summary>
    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsIndeterminateProperty)
        {
            UpdatePseudoClasses(IsIndeterminate);
            if (IsIndeterminate)
            {
                StartAnimation();
            }
            else
            {
                StopAnimation();
            }
        }
        else if (change.Property == IsVisibleProperty)
        {
            if (IsVisible)
            {
                StartAnimation();
            }
            else
            {
                StopAnimation();
            }
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _topLevel = TopLevel.GetTopLevel(this);
        StartAnimation();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _topLevel = null;
        StopAnimation();
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var bounds = Bounds;
        var thickness = Math.Max(0d, StrokeThickness);
        var radius = Math.Max(0d, Math.Min(bounds.Width, bounds.Height) / 2d - thickness / 2d);
        if (radius <= 0d || thickness <= 0d) return;

        var center = new Point(bounds.Width / 2d, bounds.Height / 2d);
        context.DrawEllipse(null, new Pen(TrackBrush, thickness), center, radius, radius);

        var range = Maximum - Minimum;
        var value = range <= double.Epsilon ? 1d : Math.Clamp((Value - Minimum) / range, 0d, 1d);
        var isIndeterminate = IsIndeterminate;
        if (!isIndeterminate && value <= 0d) return;
        if (!isIndeterminate && value >= 0.999d)
        {
            context.DrawEllipse(null, new Pen(IndicatorBrush, thickness), center, radius, radius);
            return;
        }

        var startAngle = isIndeterminate ? _animationAngle : -90d;
        var sweepAngle = isIndeterminate ? 100d : 360d * value;
        const int segmentCount = 48;
        var geometry = new StreamGeometry();
        using (var geometryContext = geometry.Open())
        {
            geometryContext.BeginFigure(GetPoint(startAngle), false);
            for (var i = 1; i <= segmentCount; i++)
            {
                geometryContext.LineTo(GetPoint(startAngle + sweepAngle * i / segmentCount));
            }
        }

        context.DrawGeometry(null, new Pen(IndicatorBrush, thickness), geometry);
        return;

        Point GetPoint(double angle)
        {
            var radians = angle * Math.PI / 180d;
            return new Point(center.X + radius * Math.Cos(radians), center.Y + radius * Math.Sin(radians));
        }
    }

    private void UpdatePseudoClasses(bool isIndeterminate)
    {
        PseudoClasses.Set(":indeterminate", isIndeterminate);
    }

    private void StartAnimation()
    {
        if (_animationStarted || _topLevel is null || !IsVisible || !IsIndeterminate) return;

        _animationStarted = true;
        _lastFrameTime = null;
        _topLevel.RequestAnimationFrame(OnAnimationFrame);
    }

    private void StopAnimation()
    {
        _animationStarted = false;
        _lastFrameTime = null;
    }

    private void OnAnimationFrame(TimeSpan time)
    {
        if (_topLevel is null || !IsVisible || !IsIndeterminate)
        {
            StopAnimation();
            return;
        }

        if (_lastFrameTime is { } lastFrameTime)
        {
            _animationAngle = (_animationAngle + (time - lastFrameTime).TotalSeconds * 270d) % 360d;
        }

        _lastFrameTime = time;
        InvalidateVisual();
        _topLevel.RequestAnimationFrame(OnAnimationFrame);
    }
}
