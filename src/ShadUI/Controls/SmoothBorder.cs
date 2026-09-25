using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace ShadUI;

/// <summary>
/// Border-like decorator with Figma-style corner smoothing.
///
/// Notes:
/// - CornerSmoothing is clamped to [0, 1]. 0 is a normal rounded corner; 0.6 is the iOS/Figma preset.
/// - Clip is internally managed when ClipToBounds is enabled. Do not set Visual.Clip manually on this control.
/// - Uniform BorderThickness is the fast/most accurate path. Non-uniform thickness uses an approximated inner offset.
/// - BoxShadow intentionally uses a normal RoundedRect because blur hides the small geometric difference.
/// </summary>
public class SmoothBorder : Decorator
{
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

    private const double Epsilon = 1e-6;

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
            BoxShadowProperty);

        AffectsMeasure<SmoothBorder>(BorderThicknessProperty);

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

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

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

        // Radius/smoothing are visual-only. Update the cached path and clip immediately instead of
        // invalidating arrangement, which keeps animated corner changes out of the layout pipeline.
        if (change.Property == CornerRadiusProperty ||
            change.Property == CornerSmoothingProperty ||
            change.Property == ClipToBoundsProperty)
        {
            if (Bounds is { Width: > Epsilon, Height: > Epsilon })
                EnsureGeometry(Bounds.Size);

            UpdateManagedClip();
        }
    }

    public override void Render(DrawingContext context)
    {
        EnsureGeometry(Bounds.Size);

        // Shadow deliberately uses a normal rounded rectangle. It is much cheaper and the blur
        // makes the difference from a continuous corner practically invisible.
        if (BoxShadow.Count > 0)
        {
            context.DrawRectangle(
                null,
                null,
                new RoundedRect(new Rect(Bounds.Size), SanitizeCornerRadius(CornerRadius)),
                BoxShadow);
        }

        var thickness = GetLayoutThickness();

        if (thickness.IsUniform)
        {
            var t = Math.Max(0, thickness.Top);
            var pen = GetPen(t);
            var backgroundGeometry = GetBackgroundGeometry();

            // CenterBorder can be submitted as a single fill+stroke command. The other sizing
            // modes use a different fill path, so they need a second draw for the stroke.
            if (BackgroundSizing == BackgroundSizing.CenterBorder && _centerGeometry is not null)
            {
                context.DrawGeometry(Background, pen, _centerGeometry);
            }
            else
            {
                if (backgroundGeometry is not null)
                    context.DrawGeometry(Background, null, backgroundGeometry);

                if (_centerGeometry is not null && pen is not null)
                    context.DrawGeometry(null, pen, _centerGeometry);
            }

            // If an absurdly thick border collapses the centerline geometry, fill the outer
            // shape instead of silently dropping the border.
            if (_centerGeometry is null && pen is not null && _outerGeometry is not null)
                context.DrawGeometry(BorderBrush, null, _outerGeometry);
        }
        else
        {
            var backgroundGeometry = GetBackgroundGeometry();

            if (backgroundGeometry is not null)
                context.DrawGeometry(Background, null, backgroundGeometry);

            if (_borderGeometry is not null && BorderBrush is not null)
                context.DrawGeometry(BorderBrush, null, _borderGeometry);
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return LayoutHelper.MeasureChild(Child, availableSize, Padding, BorderThickness);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var result = LayoutHelper.ArrangeChild(Child, finalSize, Padding, BorderThickness);

        EnsureGeometry(finalSize);

        // Visual.Clip becomes a geometry clip (SKPath clip on the Skia backend).
        // ClipToBounds itself still contributes the normal rectangular clip; the intersection is cheap.
        UpdateManagedClip();

        return result;
    }


    private StreamGeometry? GetBackgroundGeometry()
    {
        return BackgroundSizing switch
        {
            BackgroundSizing.OuterBorderEdge => _outerGeometry,
            BackgroundSizing.InnerBorderEdge => _innerGeometry,
            _ => _centerGeometry,
        };
    }

    private void UpdateManagedClip()
    {
        var desired = ClipToBounds ? _outerGeometry : null;
        if (!ReferenceEquals(Clip, desired))
            Clip = desired;
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

        var halfThickness = ScaleThickness(thickness, 0.5);
        var centerRect = InsetRect(outerRect, halfThickness);
        var centerRadius = InsetCornerRadius(radius, halfThickness);
        _centerGeometry = CreateSmoothRectGeometry(centerRect, centerRadius, smoothing);

        var innerRect = InsetRect(outerRect, thickness);
        var innerRadius = InsetCornerRadius(radius, thickness);
        _innerGeometry = CreateSmoothRectGeometry(innerRect, innerRadius, smoothing);

        if (!thickness.IsUniform && HasVisibleThickness(thickness))
        {
            _borderGeometry = CreateBorderRingGeometry(
                outerRect,
                radius,
                innerRect,
                innerRadius,
                smoothing);
        }
        else
        {
            _borderGeometry = null;
        }
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
    /// Builds a clockwise rectangle using the same geometric construction described by Figma:
    /// smoothing cubic -> circular arc -> smoothing cubic.
    ///
    /// For a right-angle corner:
    ///   q = R
    ///   p = (1 + xi) * q
    ///   beta = pi/4 * xi
    ///   t = R * tan(beta/2)
    /// The two smoothing cubics use a 2:1 split for their collinear control distances.
    /// </summary>
    private static void AppendSmoothRectFigure(
        StreamGeometryContext ctx,
        Rect rect,
        CornerRadius radius,
        double smoothing)
    {
        smoothing = Clamp01(smoothing);

        Span<double> radii = stackalloc double[4]
        {
            SanitizeRadius(radius.TopLeft),
            SanitizeRadius(radius.TopRight),
            SanitizeRadius(radius.BottomRight),
            SanitizeRadius(radius.BottomLeft),
        };

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

        // Start immediately after the top-left corner and walk clockwise.
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

    /// <summary>
    /// Figma reduces smoothing/radius when adjacent corners do not fit on an edge.
    /// Each edge independently splits the available length proportionally to the two corners'
    /// requested p values; a corner's final budget is the tighter of its two adjacent edges.
    /// </summary>
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

        LimitEdge(0, 1, Math.Max(0, width), demand, budgets); // top
        LimitEdge(1, 2, Math.Max(0, height), demand, budgets); // right
        LimitEdge(2, 3, Math.Max(0, width), demand, budgets); // bottom
        LimitEdge(3, 0, Math.Max(0, height), demand, budgets); // left
    }

    private static void LineToIfNeeded(
        StreamGeometryContext ctx,
        Point from,
        Point to)
    {
        var dx = to.X - from.X;
        var dy = to.Y - from.Y;

        if (dx * dx + dy * dy > Epsilon * Epsilon)
            ctx.LineTo(to);
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

        // First preserve ordinary rounding. Only after q fits do we spend remaining edge
        // length on smoothing. This is the behavior Figma uses when a rectangle gets cramped.
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

            // Tangent directions at the trimmed circular-arc endpoints.
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

    // Exact for uniform thickness. For a non-uniform border the true constant-offset curve
    // is no longer represented by a single scalar corner radius, so use the adjacent-side mean.
    private static CornerRadius InsetCornerRadius(CornerRadius radius, Thickness inset) => new(
        Math.Max(0, radius.TopLeft - (inset.Left + inset.Top) * 0.5),
        Math.Max(0, radius.TopRight - (inset.Right + inset.Top) * 0.5),
        Math.Max(0, radius.BottomRight - (inset.Right + inset.Bottom) * 0.5),
        Math.Max(0, radius.BottomLeft - (inset.Left + inset.Bottom) * 0.5));

    private static Thickness ScaleThickness(Thickness value, double scale) => new(
        value.Left * scale,
        value.Top * scale,
        value.Right * scale,
        value.Bottom * scale);

    private static Thickness SanitizeThickness(Thickness value) => new(
        SanitizeNonNegative(value.Left),
        SanitizeNonNegative(value.Top),
        SanitizeNonNegative(value.Right),
        SanitizeNonNegative(value.Bottom));

    private static CornerRadius SanitizeCornerRadius(CornerRadius value) => new(
        SanitizeRadius(value.TopLeft),
        SanitizeRadius(value.TopRight),
        SanitizeRadius(value.BottomRight),
        SanitizeRadius(value.BottomLeft));

    private static double SanitizeRadius(double value) => SanitizeNonNegative(value);

    private static double SanitizeNonNegative(double value) => double.IsFinite(value) ? Math.Max(0, value) : 0;

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

    private static bool HasVisibleThickness(Thickness value) =>
        value.Left > Epsilon ||
        value.Top > Epsilon ||
        value.Right > Epsilon ||
        value.Bottom > Epsilon;

    private static Point Offset(Point point, double x, double y) =>
        new(point.X + x, point.Y + y);

    private readonly struct CornerGeometry(
        Point start,
        Point end,
        bool hasInRamp,
        Point inControl1,
        Point inControl2,
        Point arcStart,
        bool hasArc,
        Point arcControl1,
        Point arcControl2,
        Point arcEnd,
        bool hasOutRamp,
        Point outControl1,
        Point outControl2
    )
    {
        public Point Start { get; } = start;
        public Point End { get; } = end;

        public bool HasInRamp { get; } = hasInRamp;
        public Point InControl1 { get; } = inControl1;
        public Point InControl2 { get; } = inControl2;
        public Point ArcStart { get; } = arcStart;

        public bool HasArc { get; } = hasArc;
        public Point ArcControl1 { get; } = arcControl1;
        public Point ArcControl2 { get; } = arcControl2;
        public Point ArcEnd { get; } = arcEnd;

        public bool HasOutRamp { get; } = hasOutRamp;
        public Point OutControl1 { get; } = outControl1;
        public Point OutControl2 { get; } = outControl2;

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