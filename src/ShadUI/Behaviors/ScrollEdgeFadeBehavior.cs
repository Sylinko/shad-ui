using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Xaml.Interactivity;

namespace ShadUI;

/// <summary>
/// Fades scrollable edges using the associated panel for the horizontal mask and its presenter for the vertical mask.
/// </summary>
/// <remarks>
/// Owns both template elements' opacity masks while enabled and attached to the visual tree.
/// All state and subscriptions are confined to the UI thread. Nested masks multiply their alpha at the corners.
/// </remarks>
public sealed class ScrollEdgeFadeBehavior : Behavior<Panel>
{
    /// <summary>
    /// Defines the <see cref="ScrollViewer"/> property.
    /// </summary>
    public static readonly StyledProperty<ScrollViewer?> ScrollViewerProperty =
        AvaloniaProperty.Register<ScrollEdgeFadeBehavior, ScrollViewer?>(nameof(ScrollViewer));

    /// <summary>
    /// Defines the <see cref="Presenter"/> property.
    /// </summary>
    public static readonly StyledProperty<ScrollContentPresenter?> PresenterProperty =
        AvaloniaProperty.Register<ScrollEdgeFadeBehavior, ScrollContentPresenter?>(nameof(Presenter));

    /// <summary>
    /// Gets or sets the viewer supplying scroll offsets, ranges, and enabled axes.
    /// </summary>
    public ScrollViewer? ScrollViewer
    {
        get => GetValue(ScrollViewerProperty);
        set => SetValue(ScrollViewerProperty, value);
    }

    /// <summary>
    /// Gets or sets the content presenter receiving the vertical mask.
    /// </summary>
    public ScrollContentPresenter? Presenter
    {
        get => GetValue(PresenterProperty);
        set => SetValue(PresenterProperty, value);
    }

    private static readonly IBrush[] HorizontalMasks = CreateMasks(true);
    private static readonly IBrush[] VerticalMasks = CreateMasks(false);
    private const int InactiveState = 4;
    private const double BoundaryTolerance = 0.000001;

    private ScrollViewer? _scrollViewer;
    private ScrollContentPresenter? _presenter;
    private bool _isAttachedToVisualTree;
    private int _horizontalState = -1;
    private int _verticalState = -1;

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree()
    {
        base.OnAttachedToVisualTree();
        _isAttachedToVisualTree = true;
        Reconnect();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree()
    {
        _isAttachedToVisualTree = false;
        Disconnect();
        base.OnDetachedFromVisualTree();
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        _isAttachedToVisualTree = false;
        Disconnect();
        base.OnDetaching();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ScrollViewerProperty || change.Property == PresenterProperty || change.Property == IsEnabledProperty)
        {
            Reconnect();
        }
    }

    private void Reconnect()
    {
        Disconnect();
        if (!_isAttachedToVisualTree || !IsEnabled || ScrollViewer is not { } scrollViewer || Presenter is not { } presenter)
        {
            return;
        }

        _scrollViewer = scrollViewer;
        _presenter = presenter;
        scrollViewer.PropertyChanged += OnScrollChanged;
        UpdateMasks();
    }

    private void Disconnect()
    {
        if (_scrollViewer is null) return;

        _scrollViewer.PropertyChanged -= OnScrollChanged;
        if (AssociatedObject is { } panel) SetMask(panel, null, false);
        if (_presenter is { } presenter) SetMask(presenter, null, false);
        _scrollViewer = null;
        _presenter = null;
        _horizontalState = -1;
        _verticalState = -1;
    }

    private void OnScrollChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == ScrollViewer.OffsetProperty ||
            change.Property == ScrollViewer.ExtentProperty ||
            change.Property == ScrollViewer.ViewportProperty ||
            change.Property == ScrollViewer.HorizontalScrollBarVisibilityProperty ||
            change.Property == ScrollViewer.VerticalScrollBarVisibilityProperty)
        {
            UpdateMasks();
        }
    }

    private void UpdateMasks()
    {
        if (_scrollViewer is not { } viewer || AssociatedObject is not { } panel || _presenter is not { } presenter) return;

        UpdateMask(
            panel,
            viewer.HorizontalScrollBarVisibility,
            viewer.Offset.X,
            viewer.Extent.Width,
            viewer.Viewport.Width,
            HorizontalMasks,
            ref _horizontalState);
        UpdateMask(
            presenter,
            viewer.VerticalScrollBarVisibility,
            viewer.Offset.Y,
            viewer.Extent.Height,
            viewer.Viewport.Height,
            VerticalMasks,
            ref _verticalState);
    }

    private static void UpdateMask(
        Control target,
        ScrollBarVisibility visibility,
        double offset,
        double extent,
        double viewport,
        IBrush[] masks,
        ref int previousState)
    {
        // Offset, extent, and viewport can use logical scrolling units; only compare them with each other.
        var maximum = Math.Max(0, extent - viewport);
        var isDisabled = visibility == ScrollBarVisibility.Disabled;
        var hasRange = maximum > BoundaryTolerance;
        var state = !isDisabled && hasRange ? (offset > BoundaryTolerance ? 1 : 0) | (offset < maximum - BoundaryTolerance ? 2 : 0) : InactiveState;

        // Inactive is distinct from an opaque destination so disabling cancels an in-flight transition once.
        if (state == previousState) return;

        SetMask(target, masks[state == InactiveState ? 0 : state], previousState >= 0 && state != InactiveState);
        previousState = state;
    }

    private static void SetMask(Control target, IBrush? mask, bool shouldAnimate)
    {
        if (shouldAnimate)
        {
            target.OpacityMask = mask;
            return;
        }

        // These template-owned transitions must be suspended for initialization, disabling, and cleanup.
        var transitions = target.Transitions;
        target.Transitions = null;
        if (mask is null) target.ClearValue(Visual.OpacityMaskProperty);
        else target.OpacityMask = mask;
        target.Transitions = transitions;
    }

    private static IBrush[] CreateMasks(bool isHorizontal)
    {
        var masks = new IBrush[4];
        for (var state = 0; state < masks.Length; state++)
        {
            masks[state] = new LinearGradientBrush
            {
                StartPoint = new RelativePoint(isHorizontal ? 0 : 0.5, isHorizontal ? 0.5 : 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(isHorizontal ? 1 : 0.5, isHorizontal ? 0.5 : 1, RelativeUnit.Relative),
                GradientStops =
                [
                    new GradientStop((state & 1) != 0 ? Colors.Transparent : Colors.Black, 0),
                    new GradientStop(Colors.Black, 0.05),
                    new GradientStop(Colors.Black, 0.95),
                    new GradientStop((state & 2) != 0 ? Colors.Transparent : Colors.Black, 1)
                ]
            }.ToImmutable();
        }

        return masks;
    }
}