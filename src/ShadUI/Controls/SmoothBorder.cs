using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;

namespace ShadUI;

/// <summary>
/// Selects how <see cref="SmoothBorder"/> clips its child when ClipToBounds is enabled.
/// </summary>
public enum ClipToBoundsMode
{
    /// <summary>
    /// Clips content to the inner edge of the border using the same smooth geometry as the border.
    /// This is the default and gives the best visual separation between content and border.
    /// </summary>
    Inner,

    /// <summary>
    /// Clips content to the outer smooth edge of the control.
    /// Content may therefore render underneath the border.
    /// </summary>
    Outer,

    /// <summary>
    /// Clips content to the inner edge using Avalonia's native rounded-rectangle clip.
    /// This is faster than a smooth geometry clip, but does not preserve continuous corners.
    /// </summary>
    FastInner,
}

/// <summary>
/// Border-like control with Figma-style continuous corners.
///
/// The control itself owns background/border/shadow rendering. User content is hosted in an
/// internal Avalonia Border so that child clipping is independent from the outer border drawing.
/// This avoids clipping the border itself with the same antialiased path used for its content.
///
/// Notes:
/// - CornerSmoothing is clamped to [0, 1]. 0 is a normal rounded corner; 0.6 is the iOS/Figma preset.
/// - ClipToBoundsMode.Inner is the default and clips to the border's inner edge.
/// - ClipToBoundsMode.FastInner uses Avalonia's native rounded-rectangle clipping as a cheaper fallback.
/// - Uniform BorderThickness remains the fastest drawing path except when exact Inner clipping is active,
///   where the border is filled as a ring so its inner edge exactly matches the content clip.
/// - BoxShadow intentionally uses a normal RoundedRect because blur hides the small geometric difference.
/// </summary>
public class SmoothBorder : Control
{
    public static readonly StyledProperty<Control?> ChildProperty =
        Decorator.ChildProperty.AddOwner<SmoothBorder>();

    public static readonly StyledProperty<Thickness> PaddingProperty =
        Decorator.PaddingProperty.AddOwner<SmoothBorder>();

    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<SmoothBorder, IBrush?>(nameof(Background));

    public static readonly StyledProperty<BackgroundSizing> BackgroundSizingProperty =
        AvaloniaProperty.Register<SmoothBorder, BackgroundSizing>(
            nameof(BackgroundSizing),
            BackgroundSizing.CenterBorder);

    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<SmoothBorder, IBrush?>(nameof(BorderBrush));

    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<SmoothBorder, Thickness>(
            nameof(BorderThickness),
            validate: MarginProperty.ValidateValue);

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<SmoothBorder, CornerRadius>(nameof(CornerRadius));

    public static readonly StyledProperty<double> CornerSmoothingProperty =
        AvaloniaProperty.Register<SmoothBorder, double>(nameof(CornerSmoothing), 0.6);

    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        AvaloniaProperty.Register<SmoothBorder, BoxShadows>(nameof(BoxShadow));

    public static readonly StyledProperty<ClipToBoundsMode> ClipToBoundsModeProperty =
        AvaloniaProperty.Register<SmoothBorder, ClipToBoundsMode>(nameof(ClipToBoundsMode));

    private const double Epsilon = 1e-6;
    private static readonly Geometry EmptyClipGeometry = new RectangleGeometry(new Rect());

    // The internal Border is deliberately kept visually empty. It exists only to host the user's
    // child and to use Avalonia's native rounded clip in FastInner mode.
    private readonly Border _contentHost;

    private StreamGeometry? _outerGeometry;
    private StreamGeometry? _centerGeometry;
    private StreamGeometry? _innerGeometry;
    private StreamGeometry? _borderGeometry;

    private Size _geometrySize;
    private Thickness _geometryThickness;
    private CornerRadius _geometryRadius;
    private double _geometrySmoothing;
    private bool _geometryValid;

    private Pen? _cachedPen;
    private IBrush? _cachedPenBrush;
    private double _cachedPenThickness = double.NaN;

    private Thickness? _layoutThickness;
    private double _layoutScale;

    static SmoothBorder()
    {
        AffectsRender<SmoothBorder>(
            BackgroundProperty,
            BackgroundSizingProperty,
            BorderBrushProperty,
            BorderThicknessProperty,
            CornerRadiusProperty,
            CornerSmoothingProperty,
            BoxShadowProperty,
            ClipToBoundsModeProperty,
            ClipToBoundsProperty);

        // Mode/ClipToBounds/CornerRadius can switch the internal host between full-bounds and
        // inner-bounds layout for native rectangular clipping, so treating them as measure-affecting
        // keeps the host layout coherent without ad-hoc arrange invalidation.
        AffectsMeasure<SmoothBorder>(
            ChildProperty,
            PaddingProperty,
            BorderThicknessProperty,
            ClipToBoundsModeProperty,
            ClipToBoundsProperty,
            CornerRadiusProperty);
    }

    public SmoothBorder()
    {
        _contentHost = new Border
        {
            // Local values prevent ordinary Border styles from accidentally making this implementation
            // detail visible. These values are also refreshed when layout/clipping mode changes.
            Background = null,
            BorderBrush = null,
            BorderThickness = default,
            BoxShadow = default,
            Padding = default,
            CornerRadius = default,
            Clip = null,
            ClipToBounds = false,
        };

        LogicalChildren.Add(_contentHost);
        VisualChildren.Add(_contentHost);
    }

    [Content]
    public Control? Child
    {
        get => GetValue(ChildProperty);
        set => SetValue(ChildProperty, value);
    }

    public Thickness Padding
    {
        get => GetValue(PaddingProperty);
        set => SetValue(PaddingProperty, value);
    }

    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    public BackgroundSizing BackgroundSizing
    {
        get => GetValue(BackgroundSizingProperty);
        set => SetValue(BackgroundSizingProperty, value);
    }

    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <summary>
    /// Figma-style corner smoothing in [0, 1].
    /// 0 = ordinary circular rounding, 0.6 = Figma's iOS preset, 1 = maximum smoothing.
    /// </summary>
    public double CornerSmoothing
    {
        get => GetValue(CornerSmoothingProperty);
        set => SetValue(CornerSmoothingProperty, value);
    }

    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }

    /// <summary>
    /// Chooses which edge is used for child clipping when ClipToBounds is true.
    /// </summary>
    public ClipToBoundsMode ClipToBoundsMode
    {
        get => GetValue(ClipToBoundsModeProperty);
        set => SetValue(ClipToBoundsModeProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ChildProperty)
            _contentHost.Child = Child;

        if (change.Property == BorderThicknessProperty ||
            change.Property == CornerRadiusProperty ||
            change.Property == CornerSmoothingProperty ||
            change.Property == UseLayoutRoundingProperty)
        {
            _geometryValid = false;
        }

        if (change.Property == BorderThicknessProperty ||
            change.Property == UseLayoutRoundingProperty)
        {
            _layoutThickness = null;
        }

        if (change.Property == PaddingProperty ||
            change.Property == BorderThicknessProperty ||
            change.Property == ClipToBoundsModeProperty ||
            change.Property == ClipToBoundsProperty ||
            change.Property == CornerRadiusProperty)
        {
            ConfigureContentHostLayout();
        }

        if (change.Property == CornerRadiusProperty ||
            change.Property == CornerSmoothingProperty ||
            change.Property == ClipToBoundsProperty ||
            change.Property == ClipToBoundsModeProperty ||
            change.Property == BorderThicknessProperty)
        {
            if (Bounds is { Width: > Epsilon, Height: > Epsilon })
                EnsureGeometry(Bounds.Size);

            UpdateContentClip();
        }
    }

    public override void Render(DrawingContext context)
    {
        EnsureGeometry(Bounds.Size);

        // Shadow deliberately uses a normal rounded rectangle. It is cheaper, and after blur the
        // difference from the continuous outline is effectively invisible.
        if (BoxShadow.Count > 0)
        {
            context.DrawRectangle(
                null,
                null,
                new RoundedRect(new Rect(Bounds.Size), SanitizeCornerRadius(CornerRadius)),
                BoxShadow);
        }

        var thickness = GetLayoutThickness();
        var hasBorder = HasVisibleThickness(thickness) && BorderBrush is not null;

        // Exact inner clipping intentionally uses the ring path even for a uniform border. This makes
        // the border's visual inner edge and the content clip derive from exactly the same geometry.
        // Non-uniform borders also require the ring path because a single Pen cannot represent them.
        var useBorderRing =
            !thickness.IsUniform ||
            (ClipToBounds && ClipToBoundsMode == ClipToBoundsMode.Inner);

        if (useBorderRing)
        {
            var backgroundGeometry = GetBackgroundGeometry();

            if (backgroundGeometry is not null && Background is not null)
                context.DrawGeometry(Background, null, backgroundGeometry);

            if (hasBorder && _borderGeometry is not null)
                context.DrawGeometry(BorderBrush, null, _borderGeometry);

            return;
        }

        // Fast path: uniform thickness can use one cached center path and a Pen.
        var t = Math.Max(0, thickness.Top);
        var pen = GetPen(t);
        var background = GetBackgroundGeometry();

        if (BackgroundSizing == BackgroundSizing.CenterBorder && _centerGeometry is not null)
        {
            context.DrawGeometry(Background, pen, _centerGeometry);
        }
        else
        {
            if (background is not null && Background is not null)
                context.DrawGeometry(Background, null, background);

            if (_centerGeometry is not null && pen is not null)
                context.DrawGeometry(null, pen, _centerGeometry);
        }

        // If an absurdly thick border collapses the centerline path, fill the outer shape instead
        // of silently dropping the border.
        if (_centerGeometry is null && pen is not null && _outerGeometry is not null)
            context.DrawGeometry(BorderBrush, null, _outerGeometry);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var thickness = GetLayoutThickness();
        ConfigureContentHostLayout();

        if (UsesInsetContentHost())
        {
            _contentHost.Measure(availableSize.Deflate(thickness));
            return _contentHost.DesiredSize.Inflate(thickness);
        }

        _contentHost.Measure(availableSize);
        return _contentHost.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var thickness = GetLayoutThickness();
        ConfigureContentHostLayout();

        if (UsesInsetContentHost())
        {
            var innerRect = InsetRect(new Rect(finalSize), thickness);
            _contentHost.Arrange(innerRect);
        }
        else
        {
            _contentHost.Arrange(new Rect(finalSize));
        }

        EnsureGeometry(finalSize);
        UpdateContentClip();

        return finalSize;
    }

    private Geometry? GetBackgroundGeometry()
    {
        return BackgroundSizing switch
        {
            BackgroundSizing.OuterBorderEdge => _outerGeometry,
            BackgroundSizing.InnerBorderEdge => _innerGeometry,
            _ => _centerGeometry,
        };
    }

    /// <summary>
    /// Returns true when the host itself is arranged at the inner border edge. This is required by
    /// FastInner, because Avalonia's native Border clip always clips to its own bounds. It is also a
    /// useful zero-radius fast path for exact Inner clipping: a native rectangular clip is exact there.
    /// </summary>
    private bool UsesInsetContentHost()
    {
        if (!ClipToBounds)
            return false;

        if (ClipToBoundsMode == ClipToBoundsMode.FastInner)
            return true;

        return ClipToBoundsMode == ClipToBoundsMode.Inner &&
            IsZeroCornerRadius(SanitizeCornerRadius(CornerRadius));
    }

    private void ConfigureContentHostLayout()
    {
        // Keep all visually meaningful Border properties pinned to local values so an application-wide
        // Border style cannot leak into this implementation detail.
        _contentHost.Background = null;
        _contentHost.BorderBrush = null;
        _contentHost.BoxShadow = default;
        _contentHost.Padding = Padding;

        if (UsesInsetContentHost())
        {
            _contentHost.BorderThickness = default;
        }
        else
        {
            // A full-size host uses an invisible border solely for layout, placing the user child at
            // exactly the same position as a normal Avalonia Border would.
            _contentHost.BorderThickness = GetLayoutThickness();
        }
    }

    private void UpdateContentClip()
    {
        if (!ClipToBounds)
        {
            _contentHost.Clip = null;
            _contentHost.ClipToBounds = false;
            _contentHost.CornerRadius = default;
            return;
        }

        var outerRadius = SanitizeCornerRadius(CornerRadius);
        var smoothing = Clamp01(CornerSmoothing);

        switch (ClipToBoundsMode)
        {
            case ClipToBoundsMode.Outer:
                // SmoothBorder.ClipToBounds already contributes the normal axis-aligned bounds clip.
                // Therefore an all-zero outer radius needs no second clip on the content host.
                if (IsZeroCornerRadius(outerRadius))
                {
                    _contentHost.Clip = null;
                    _contentHost.ClipToBounds = false;
                    _contentHost.CornerRadius = default;
                }
                else if (smoothing <= Epsilon)
                {
                    // No smoothing means the exact geometry is a normal rounded rectangle, so use
                    // Avalonia's native rounded-rect clip rather than a generic geometry clip.
                    _contentHost.Clip = null;
                    _contentHost.CornerRadius = outerRadius;
                    _contentHost.ClipToBounds = true;
                }
                else
                {
                    _contentHost.ClipToBounds = false;
                    _contentHost.CornerRadius = default;
                    _contentHost.Clip = _outerGeometry;
                }
                break;

            case ClipToBoundsMode.FastInner:
                _contentHost.Clip = null;
                _contentHost.CornerRadius = InsetCornerRadiusConservative(
                    outerRadius,
                    GetLayoutThickness());
                _contentHost.ClipToBounds = true;
                break;

            default:
                if (UsesInsetContentHost())
                {
                    // Exact zero-radius Inner mode. The host already occupies the inner rectangle,
                    // so Avalonia's native rectangular ClipToBounds is both exact and maximally cheap.
                    _contentHost.Clip = null;
                    _contentHost.CornerRadius = default;
                    _contentHost.ClipToBounds = true;
                }
                else
                {
                    // The full-size host lets us reuse the exact same positioned inner geometry that
                    // also defines the border ring's inner edge.
                    _contentHost.ClipToBounds = false;
                    _contentHost.CornerRadius = default;
                    _contentHost.Clip = _innerGeometry ?? EmptyClipGeometry;
                }
                break;
        }
    }

    private Thickness GetLayoutThickness()
    {
        var scale = LayoutHelper.GetLayoutScale(this);

        if (Math.Abs(scale - _layoutScale) > Epsilon)
        {
            _layoutScale = scale;
            _layoutThickness = null;
            _geometryValid = false;
        }

        if (_layoutThickness is null)
        {
            var thickness = SanitizeThickness(BorderThickness);
            if (UseLayoutRounding)
                thickness = LayoutHelper.RoundLayoutThickness(thickness, scale);

            _layoutThickness = thickness;
        }

        return _layoutThickness.Value;
    }

    private Pen? GetPen(double thickness)
    {
        if (BorderBrush is null || thickness <= Epsilon)
            return null;

        if (_cachedPen is null ||
            !ReferenceEquals(_cachedPenBrush, BorderBrush) ||
            Math.Abs(_cachedPenThickness - thickness) > Epsilon)
        {
            _cachedPenBrush = BorderBrush;
            _cachedPenThickness = thickness;
            _cachedPen = new Pen(BorderBrush, thickness);
        }

        return _cachedPen;
    }

    private void EnsureGeometry(Size size)
    {
        var thickness = GetLayoutThickness();
        var radius = SanitizeCornerRadius(CornerRadius);
        var smoothing = Clamp01(CornerSmoothing);

        if (_geometryValid &&
            _geometrySize == size &&
            _geometryThickness == thickness &&
            _geometryRadius == radius &&
            Math.Abs(_geometrySmoothing - smoothing) <= Epsilon)
        {
            return;
        }

        _geometryValid = true;
        _geometrySize = size;
        _geometryThickness = thickness;
        _geometryRadius = radius;
        _geometrySmoothing = smoothing;

        if (size.Width <= Epsilon || size.Height <= Epsilon)
        {
            _outerGeometry = null;
            _centerGeometry = null;
            _innerGeometry = null;
            _borderGeometry = null;
            return;
        }

        var outerRect = new Rect(size);
        _outerGeometry = CreateSmoothRectGeometry(outerRect, radius, smoothing);

        if (!HasVisibleThickness(thickness))
        {
            // Reuse the same immutable path references when all three edges coincide.
            _centerGeometry = _outerGeometry;
            _innerGeometry = _outerGeometry;
            _borderGeometry = null;
            return;
        }

        var halfThickness = ScaleThickness(thickness, 0.5);
        var centerRect = InsetRect(outerRect, halfThickness);
        var centerRadius = InsetCornerRadius(radius, halfThickness);
        _centerGeometry = CreateSmoothRectGeometry(centerRect, centerRadius, smoothing);

        var innerRect = InsetRect(outerRect, thickness);
        var innerRadius = InsetCornerRadius(radius, thickness);
        _innerGeometry = CreateSmoothRectGeometry(innerRect, innerRadius, smoothing);

        _borderGeometry = CreateBorderRingGeometry(
            outerRect,
            radius,
            innerRect,
            innerRadius,
            smoothing);
    }

    private static StreamGeometry? CreateSmoothRectGeometry(
        Rect rect,
        CornerRadius radius,
        double smoothing)
    {
        if (rect.Width <= Epsilon || rect.Height <= Epsilon)
            return null;

        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        AppendSmoothRectFigure(ctx, rect, radius, smoothing);
        return geometry;
    }

    private static StreamGeometry? CreateBorderRingGeometry(
        Rect outerRect,
        CornerRadius outerRadius,
        Rect innerRect,
        CornerRadius innerRadius,
        double smoothing)
    {
        if (outerRect.Width <= Epsilon || outerRect.Height <= Epsilon)
            return null;

        var geometry = new StreamGeometry();
        using var ctx = geometry.Open();
        ctx.SetFillRule(FillRule.EvenOdd);

        AppendSmoothRectFigure(ctx, outerRect, outerRadius, smoothing);

        if (innerRect is { Width: > Epsilon, Height: > Epsilon })
            AppendSmoothRectFigure(ctx, innerRect, innerRadius, smoothing);

        return geometry;
    }

    /// <summary>
    /// Builds a clockwise rectangle using Figma's smoothing-cubic -> circular-arc -> smoothing-cubic
    /// construction. This intentionally keeps the v1/v1.1 Figma-compatible curve unchanged so v3's
    /// clipping/layout changes can be evaluated independently from curve-profile experiments.
    /// </summary>
    private static void AppendSmoothRectFigure(
        StreamGeometryContext ctx,
        Rect rect,
        CornerRadius radius,
        double smoothing)
    {
        smoothing = Clamp01(smoothing);

        Span<double> radii =
        [
            SanitizeRadius(radius.TopLeft),
            SanitizeRadius(radius.TopRight),
            SanitizeRadius(radius.BottomRight),
            SanitizeRadius(radius.BottomLeft),
        ];

        Span<double> budgets = stackalloc double[4];
        ResolveCornerBudgets(rect.Width, rect.Height, radii, smoothing, budgets);

        var tl = BuildCorner(
            new Point(rect.Left, rect.Top),
            0,
            -1,
            1,
            0,
            radii[0],
            smoothing,
            budgets[0]);

        var tr = BuildCorner(
            new Point(rect.Right, rect.Top),
            1,
            0,
            0,
            1,
            radii[1],
            smoothing,
            budgets[1]);

        var br = BuildCorner(
            new Point(rect.Right, rect.Bottom),
            0,
            1,
            -1,
            0,
            radii[2],
            smoothing,
            budgets[2]);

        var bl = BuildCorner(
            new Point(rect.Left, rect.Bottom),
            -1,
            0,
            0,
            -1,
            radii[3],
            smoothing,
            budgets[3]);

        ctx.BeginFigure(tl.End);

        LineToIfNeeded(ctx, tl.End, tr.Start);
        AppendCorner(ctx, tr);

        LineToIfNeeded(ctx, tr.End, br.Start);
        AppendCorner(ctx, br);

        LineToIfNeeded(ctx, br.End, bl.Start);
        AppendCorner(ctx, bl);

        LineToIfNeeded(ctx, bl.End, tl.Start);
        AppendCorner(ctx, tl);

        ctx.EndFigure(true);
    }

    private static void ResolveCornerBudgets(
        double width,
        double height,
        ReadOnlySpan<double> radii,
        double smoothing,
        Span<double> budgets)
    {
        Span<double> demand = stackalloc double[4];
        for (var i = 0; i < 4; i++)
        {
            demand[i] = (1 + smoothing) * radii[i];
            budgets[i] = demand[i];
        }

        LimitEdge(0, 1, Math.Max(0, width), demand, budgets);
        LimitEdge(1, 2, Math.Max(0, height), demand, budgets);
        LimitEdge(2, 3, Math.Max(0, width), demand, budgets);
        LimitEdge(3, 0, Math.Max(0, height), demand, budgets);
    }

    private static void LimitEdge(
        int first,
        int second,
        double edgeLength,
        ReadOnlySpan<double> demand,
        Span<double> budgets)
    {
        var d0 = demand[first];
        var d1 = demand[second];
        var total = d0 + d1;

        if (total <= edgeLength || total <= Epsilon)
            return;

        var scale = edgeLength / total;
        budgets[first] = Math.Min(budgets[first], d0 * scale);
        budgets[second] = Math.Min(budgets[second], d1 * scale);
    }

    private static CornerGeometry BuildCorner(
        Point vertex,
        double inX,
        double inY,
        double outX,
        double outY,
        double requestedRadius,
        double requestedSmoothing,
        double budget)
    {
        var q = Math.Max(0, requestedRadius);
        var xi = Clamp01(requestedSmoothing);
        budget = Math.Max(0, budget);

        if (q <= Epsilon || budget <= Epsilon)
            return CornerGeometry.Sharp(vertex);

        // Preserve ordinary rounding first. Only remaining edge budget is spent on smoothing.
        if (q > budget)
        {
            q = budget;
            xi = 0;
        }
        else
        {
            var desiredP = (1 + xi) * q;
            if (desiredP > budget)
                xi = Clamp01(budget / q - 1);
        }

        var radius = q;
        var p = (1 + xi) * q;

        var start = Offset(vertex, -inX * p, -inY * p);
        var end = Offset(vertex, outX * p, outY * p);

        if (radius <= Epsilon)
            return CornerGeometry.Sharp(vertex);

        var center = Offset(
            vertex,
            -inX * radius + outX * radius,
            -inY * radius + outY * radius);

        if (xi <= Epsilon)
        {
            var arcStart = Offset(center, -outX * radius, -outY * radius);
            var arcEnd = Offset(center, inX * radius, inY * radius);
            var k = (4.0 / 3.0) * Math.Tan(Math.PI / 8.0) * radius;

            return new CornerGeometry(
                start,
                end,
                false,
                default,
                default,
                arcStart,
                true,
                Offset(arcStart, inX * k, inY * k),
                Offset(arcEnd, -outX * k, -outY * k),
                arcEnd,
                false,
                default,
                default);
        }

        var beta = (Math.PI / 4.0) * xi;
        var sinBeta = Math.Sin(beta);
        var cosBeta = Math.Cos(beta);
        var t = radius * Math.Tan(beta / 2.0);

        var aPlusB = p - q + t;
        var a = (2.0 / 3.0) * aPlusB;

        var inControl1 = Offset(vertex, -inX * (p - a), -inY * (p - a));
        var inControl2 = Offset(vertex, -inX * (q - t), -inY * (q - t));

        var arcStartPoint = Offset(
            center,
            inX * radius * sinBeta - outX * radius * cosBeta,
            inY * radius * sinBeta - outY * radius * cosBeta);

        var arcEndPoint = Offset(
            center,
            inX * radius * cosBeta - outX * radius * sinBeta,
            inY * radius * cosBeta - outY * radius * sinBeta);

        var sweep = (Math.PI / 2.0) * (1 - xi);
        var hasArc = sweep > Epsilon;

        Point arcControl1 = default;
        Point arcControl2 = default;

        if (hasArc)
        {
            var arcK = (4.0 / 3.0) * Math.Tan(sweep / 4.0) * radius;

            var tanStartX = inX * cosBeta + outX * sinBeta;
            var tanStartY = inY * cosBeta + outY * sinBeta;
            var tanEndX = inX * sinBeta + outX * cosBeta;
            var tanEndY = inY * sinBeta + outY * cosBeta;

            arcControl1 = Offset(arcStartPoint, tanStartX * arcK, tanStartY * arcK);
            arcControl2 = Offset(arcEndPoint, -tanEndX * arcK, -tanEndY * arcK);
        }

        var outControl1 = Offset(vertex, outX * (q - t), outY * (q - t));
        var outControl2 = Offset(vertex, outX * (p - a), outY * (p - a));

        return new CornerGeometry(
            start,
            end,
            true,
            inControl1,
            inControl2,
            arcStartPoint,
            hasArc,
            arcControl1,
            arcControl2,
            arcEndPoint,
            true,
            outControl1,
            outControl2);
    }

    private static void LineToIfNeeded(StreamGeometryContext ctx, Point from, Point to)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;

        if (dx * dx + dy * dy > Epsilon * Epsilon)
            ctx.LineTo(to);
    }

    private static void AppendCorner(StreamGeometryContext ctx, in CornerGeometry corner)
    {
        if (corner.HasInRamp)
        {
            ctx.CubicBezierTo(
                corner.InControl1,
                corner.InControl2,
                corner.ArcStart);
        }

        if (corner.HasArc)
        {
            ctx.CubicBezierTo(
                corner.ArcControl1,
                corner.ArcControl2,
                corner.ArcEnd);
        }

        if (corner.HasOutRamp)
        {
            ctx.CubicBezierTo(
                corner.OutControl1,
                corner.OutControl2,
                corner.End);
        }
    }

    private static Rect InsetRect(Rect rect, Thickness inset)
    {
        var left = Math.Max(0, inset.Left);
        var top = Math.Max(0, inset.Top);
        var right = Math.Max(0, inset.Right);
        var bottom = Math.Max(0, inset.Bottom);

        return new Rect(
            rect.X + left,
            rect.Y + top,
            Math.Max(0, rect.Width - left - right),
            Math.Max(0, rect.Height - top - bottom));
    }

    private static CornerRadius InsetCornerRadius(CornerRadius radius, Thickness inset)
    {
        // Exact for uniform thickness. For a non-uniform border the true constant-offset curve is
        // not representable by one scalar radius per corner, so use the adjacent-side mean.
        return new CornerRadius(
            Math.Max(0, radius.TopLeft - (inset.Left + inset.Top) * 0.5),
            Math.Max(0, radius.TopRight - (inset.Right + inset.Top) * 0.5),
            Math.Max(0, radius.BottomRight - (inset.Right + inset.Bottom) * 0.5),
            Math.Max(0, radius.BottomLeft - (inset.Left + inset.Bottom) * 0.5));
    }

    private static CornerRadius InsetCornerRadiusConservative(CornerRadius radius, Thickness inset)
    {
        // FastInner only has one scalar radius per corner. With a non-uniform border, subtracting the
        // larger adjacent thickness keeps content conservatively inside the visible border.
        return new CornerRadius(
            Math.Max(0, radius.TopLeft - Math.Max(inset.Left, inset.Top)),
            Math.Max(0, radius.TopRight - Math.Max(inset.Right, inset.Top)),
            Math.Max(0, radius.BottomRight - Math.Max(inset.Right, inset.Bottom)),
            Math.Max(0, radius.BottomLeft - Math.Max(inset.Left, inset.Bottom)));
    }

    private static Thickness ScaleThickness(Thickness value, double scale)
    {
        return new Thickness(
            value.Left * scale,
            value.Top * scale,
            value.Right * scale,
            value.Bottom * scale);
    }

    private static Thickness SanitizeThickness(Thickness value)
    {
        return new Thickness(
            SanitizeNonNegative(value.Left),
            SanitizeNonNegative(value.Top),
            SanitizeNonNegative(value.Right),
            SanitizeNonNegative(value.Bottom));
    }

    private static CornerRadius SanitizeCornerRadius(CornerRadius value)
    {
        return new CornerRadius(
            SanitizeRadius(value.TopLeft),
            SanitizeRadius(value.TopRight),
            SanitizeRadius(value.BottomRight),
            SanitizeRadius(value.BottomLeft));
    }

    private static bool IsZeroCornerRadius(CornerRadius value)
    {
        return value is { TopLeft: <= Epsilon, TopRight: <= Epsilon, BottomRight: <= Epsilon, BottomLeft: <= Epsilon };
    }

    private static double SanitizeRadius(double value)
    {
        return SanitizeNonNegative(value);
    }

    private static double SanitizeNonNegative(double value)
    {
        return double.IsFinite(value) ? Math.Max(0, value) : 0;
    }

    private static double Clamp01(double value)
    {
        if (!double.IsFinite(value))
            return 0;
        if (value <= 0)
            return 0;
        if (value >= 1)
            return 1;
        return value;
    }

    private static bool HasVisibleThickness(Thickness value)
    {
        return value.Left > Epsilon ||
            value.Top > Epsilon ||
            value.Right > Epsilon ||
            value.Bottom > Epsilon;
    }

    private static Point Offset(Point point, double x, double y)
        => new(point.X + x, point.Y + y);

    private readonly record struct CornerGeometry(
        Point Start,
        Point End,
        bool HasInRamp,
        Point InControl1,
        Point InControl2,
        Point ArcStart,
        bool HasArc,
        Point ArcControl1,
        Point ArcControl2,
        Point ArcEnd,
        bool HasOutRamp,
        Point OutControl1,
        Point OutControl2
    )
    {
        public static CornerGeometry Sharp(Point point) => new(
            point,
            point,
            false,
            default,
            default,
            point,
            false,
            default,
            default,
            point,
            false,
            default,
            default);
    }
}