using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace ShadUI;

/// <summary>
/// A NavigationView contains a ListBox for selecting pages and a ContentControl for displaying the selected page.
/// It displays as a multi-column when there is enough space, and as a single-column with a hamburger menu when space is limited.
/// </summary>
[PseudoClasses(":compact", ":split", ":root", ":detail")]
[TemplatePart(SplitContainerPartName, typeof(Grid), IsRequired = true)]
[TemplatePart(PaneRootPartName, typeof(DockPanel), IsRequired = true)]
[TemplatePart(ContentRootPartName, typeof(Control), IsRequired = true)]
[TemplatePart(ContentContainerPartName, typeof(TransitioningContentControl), IsRequired = true)]
public class NavigationView : TemplatedControl
{
    private const string SplitContainerPartName = "PART_SplitContainer";
    private const string PaneRootPartName = "PART_PaneRoot";
    private const string ContentRootPartName = "PART_ContentRoot";
    private const string ContentContainerPartName = "PART_ContentContainer";
    private static readonly TimeSpan CompactNavigationDuration = TimeSpan.FromMilliseconds(240);
    private static readonly Easing CompactNavigationEasing = new CubicEaseInOut();

    /// <summary>
    /// Defines the <see cref="SelectedItem"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<NavigationView, object?>(nameof(SelectedItem));

    /// <summary>
    /// Gets or sets the currently selected item in the navigation pane.
    /// </summary>
    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ContentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        AvaloniaProperty.Register<NavigationView, IDataTemplate?>(nameof(ContentTemplate));

    /// <summary>
    /// Gets or sets the content template for the content area.
    /// </summary>
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="EmptyContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> EmptyContentProperty =
        AvaloniaProperty.Register<NavigationView, object?>(nameof(EmptyContent));

    /// <summary>
    /// Gets or sets the content when no item is selected in the navigation pane.
    /// </summary>
    public object? EmptyContent
    {
        get => GetValue(EmptyContentProperty);
        set => SetValue(EmptyContentProperty, value);
    }

    public static readonly StyledProperty<GridLength> PaneWidthProperty =
        AvaloniaProperty.Register<NavigationView, GridLength>(nameof(PaneWidth), new GridLength(320d, GridUnitType.Pixel));

    public GridLength PaneWidth
    {
        get => GetValue(PaneWidthProperty);
        set => SetValue(PaneWidthProperty, value);
    }

    public static readonly StyledProperty<double> HeaderMaxWidthProperty =
        AvaloniaProperty.Register<NavigationView, double>(nameof(HeaderMaxWidth), double.PositiveInfinity);

    public double HeaderMaxWidth
    {
        get => GetValue(HeaderMaxWidthProperty);
        set => SetValue(HeaderMaxWidthProperty, value);
    }

    public static readonly StyledProperty<Thickness> HeaderPaddingProperty =
        AvaloniaProperty.Register<NavigationView, Thickness>(nameof(HeaderPadding));

    public Thickness HeaderPadding
    {
        get => GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Spacing"/> property.
    /// </summary>
    public static readonly StyledProperty<GridLength> SpacingProperty =
        AvaloniaProperty.Register<NavigationView, GridLength>(nameof(Spacing), new GridLength(16d, GridUnitType.Pixel));

    /// <summary>
    /// Gets or sets the spacing between pane and content area.
    /// </summary>
    public GridLength Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="CompactThreshold"/> property.
    /// </summary>
    public static readonly StyledProperty<double> CompactThresholdProperty =
        AvaloniaProperty.Register<NavigationView, double>(nameof(CompactThreshold), 800d);

    /// <summary>
    /// Gets or sets the width threshold below which the NavigationView enters compact mode.
    /// </summary>
    public double CompactThreshold
    {
        get => GetValue(CompactThresholdProperty);
        set => SetValue(CompactThresholdProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="IsCompact"/> property.
    /// </summary>
    public static readonly DirectProperty<NavigationView, bool> IsCompactProperty =
        AvaloniaProperty.RegisterDirect<NavigationView, bool>(nameof(IsCompact), o => o.IsCompact);

    /// <summary>
    /// Gets a value indicating whether the NavigationView is in compact mode.
    /// </summary>
    public bool IsCompact
    {
        get;
        private set => SetAndRaise(IsCompactProperty, ref field, value);
    }

    /// <summary>
    /// Defines the <see cref="PaneHeader"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> PaneHeaderProperty =
        AvaloniaProperty.Register<NavigationView, object?>(nameof(PaneHeader));

    /// <summary>
    /// Gets or sets the header content for the navigation pane.
    /// </summary>
    public object? PaneHeader
    {
        get => GetValue(PaneHeaderProperty);
        set => SetValue(PaneHeaderProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="PaneHeaderTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> PaneHeaderTemplateProperty =
        AvaloniaProperty.Register<NavigationView, IDataTemplate?>(nameof(PaneHeaderTemplate));

    /// <summary>
    /// Gets or sets the data template for the navigation pane header.
    /// </summary>
    public IDataTemplate? PaneHeaderTemplate
    {
        get => GetValue(PaneHeaderTemplateProperty);
        set => SetValue(PaneHeaderTemplateProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="PaneContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> PaneContentProperty =
        AvaloniaProperty.Register<NavigationView, object?>(nameof(PaneContent));

    /// <summary>
    /// Gets or sets the content for the navigation pane.
    /// </summary>
    public object? PaneContent
    {
        get => GetValue(PaneContentProperty);
        set => SetValue(PaneContentProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Header"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<NavigationView, object?>(nameof(Header));

    /// <summary>
    /// Gets or sets the header content for the NavigationView.
    /// </summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="HeaderTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.Register<NavigationView, IDataTemplate?>(nameof(HeaderTemplate));

    /// <summary>
    /// Gets or sets the data template for the NavigationView header.
    /// </summary>
    public IDataTemplate? HeaderTemplate
    {
        get => GetValue(HeaderTemplateProperty);
        set => SetValue(HeaderTemplateProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="PageTransition"/> property.
    /// </summary>
    public static readonly StyledProperty<IPageTransition?> PageTransitionProperty =
        AvaloniaProperty.Register<NavigationView, IPageTransition?>(nameof(PageTransition));

    /// <summary>
    /// Gets or sets the page transition animation used when navigating between pages.
    /// </summary>
    public IPageTransition? PageTransition
    {
        get => GetValue(PageTransitionProperty);
        set => SetValue(PageTransitionProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="CompactPageTransition"/> property.
    /// </summary>
    public static readonly StyledProperty<IPageTransition?> CompactPageTransitionProperty =
        AvaloniaProperty.Register<NavigationView, IPageTransition?>(nameof(CompactPageTransition));

    /// <summary>
    /// Gets or sets the page transition animation used when navigating between pages in compact mode.
    /// </summary>
    public IPageTransition? CompactPageTransition
    {
        get => GetValue(CompactPageTransitionProperty);
        set => SetValue(CompactPageTransitionProperty, value);
    }

    private Grid? _splitContainer;
    private DockPanel? _paneRoot;
    private Control? _contentRoot;
    private TransitioningContentControl? _contentContainer;
    private Control? _activeCompactRoot;
    private TopLevel? _topLevel;
    private CompactAnimationState? _compactAnimation;
    private bool _clearSelectionAfterCompactBack;
    private int _layoutStateVersion;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _topLevel = TopLevel.GetTopLevel(this);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CancelCompactTransition();
        _topLevel = null;

        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (e.WidthChanged)
        {
            IsCompact = e.NewSize.Width < CompactThreshold;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsCompactProperty || change.Property == SelectedItemProperty)
        {
            UpdateLayoutState();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _splitContainer = e.NameScope.Find<Grid>(SplitContainerPartName);
        _paneRoot = e.NameScope.Find<DockPanel>(PaneRootPartName);
        _contentRoot = e.NameScope.Find<Control>(ContentRootPartName);
        _contentContainer = e.NameScope.Find<TransitioningContentControl>(ContentContainerPartName);

        if (_splitContainer is not null)
        {
            _splitContainer.ColumnDefinitions.Clear();
            _splitContainer.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    [!ColumnDefinition.WidthProperty] = this[!PaneWidthProperty]
                });
            _splitContainer.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    [!ColumnDefinition.WidthProperty] = this[!SpacingProperty]
                });
            _splitContainer.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
        }

        UpdateLayoutState();
    }

    private void UpdateLayoutState()
    {
        if (_splitContainer is null || _paneRoot is null || _contentRoot is null) return;

        var version = ++_layoutStateVersion;
        var wasCompact = _activeCompactRoot is not null;
        var previousCompactRoot = _activeCompactRoot;

        CancelCompactTransition();
        ResetRootVisualState(_paneRoot);
        ResetRootVisualState(_contentRoot);

        PseudoClasses.Set(":compact", IsCompact);
        PseudoClasses.Set(":split", !IsCompact);
        PseudoClasses.Set(":root", IsCompact && SelectedItem is null);
        PseudoClasses.Set(":detail", IsCompact && SelectedItem is not null);

        _splitContainer.IsVisible = true;

        if (IsCompact)
        {
            _contentContainer?.PageTransition = null;

            ConfigureCompactRoot(_paneRoot);
            ConfigureCompactRoot(_contentRoot);

            var targetRoot = SelectedItem is not null ? _contentRoot : _paneRoot;
            var fromRoot = previousCompactRoot is not null && previousCompactRoot != targetRoot ? previousCompactRoot : null;

            _activeCompactRoot = targetRoot;

            if (wasCompact && fromRoot is not null)
            {
                StartCompactTransition(_paneRoot, _contentRoot, SelectedItem is not null, version);
            }
            else
            {
                ApplyCompactTarget(targetRoot);
            }
        }
        else
        {
            _activeCompactRoot = null;

            ConfigureSplitRoot(_paneRoot, 0);
            ConfigureSplitRoot(_contentRoot, 2);

            _contentContainer?.PageTransition = PageTransition;
            _paneRoot.IsVisible = true;
            _contentRoot.IsVisible = true;
        }
    }

    private void StartCompactTransition(DockPanel paneRoot, Control contentRoot, bool forward, int version)
    {
        _topLevel ??= TopLevel.GetTopLevel(this);

        paneRoot.IsVisible = true;
        paneRoot.IsHitTestVisible = false;
        contentRoot.IsVisible = true;
        contentRoot.IsHitTestVisible = false;
        paneRoot.ZIndex = 0;
        contentRoot.ZIndex = 1;

        var distance = GetCompactTransitionDistance();
        var paneOffset = -distance * 0.25d;
        var paneTransform = new TranslateTransform(forward ? 0d : paneOffset, 0d);
        var contentTransform = new TranslateTransform(forward ? distance : 0d, 0d);

        paneRoot.RenderTransform = paneTransform;
        contentRoot.RenderTransform = contentTransform;

        _compactAnimation = new CompactAnimationState(
            paneRoot,
            contentRoot,
            paneTransform,
            contentTransform,
            version,
            forward,
            forward ? 0d : paneOffset,
            forward ? paneOffset : 0d,
            forward ? distance : 0d,
            forward ? 0d : distance);

        if (_topLevel is { } topLevel)
        {
            topLevel.RequestAnimationFrame(OnCompactAnimationTick);
            return;
        }

        CompleteCompactTransition(_compactAnimation);
    }

    private double GetCompactTransitionDistance()
    {
        if (_splitContainer is null) return 1d;

        var width = _splitContainer.Bounds.Width;
        return width > 0d ? width : 1d;
    }

    private void OnCompactAnimationTick(TimeSpan time)
    {
        var animation = _compactAnimation;
        if (animation is null || animation.IsCancelled || animation.Version != _layoutStateVersion)
        {
            return;
        }

        animation.StartTime ??= time;

        var elapsed = time - animation.StartTime.Value;
        var rawProgress = elapsed.TotalMilliseconds / CompactNavigationDuration.TotalMilliseconds;
        var progress = Math.Clamp(rawProgress, 0d, 1d);
        var easedProgress = CompactNavigationEasing.Ease(progress);

        animation.PaneTransform.X = Lerp(animation.PaneFrom, animation.PaneTo, easedProgress);
        animation.ContentTransform.X = Lerp(animation.ContentFrom, animation.ContentTo, easedProgress);

        if (progress >= 1d)
        {
            CompleteCompactTransition(animation);
            return;
        }

        if (_topLevel is { } topLevel)
        {
            topLevel.RequestAnimationFrame(OnCompactAnimationTick);
            return;
        }

        CompleteCompactTransition(animation);
    }

    private void CompleteCompactTransition(CompactAnimationState animation)
    {
        if (!ReferenceEquals(_compactAnimation, animation) || animation.IsCancelled)
        {
            return;
        }

        _compactAnimation = null;

        var targetRoot = animation.Forward ? animation.ContentRoot : animation.PaneRoot;
        ResetRootVisualState(animation.PaneRoot);
        ResetRootVisualState(animation.ContentRoot);
        ApplyCompactTarget(targetRoot);

        if (_clearSelectionAfterCompactBack && !animation.Forward)
        {
            _clearSelectionAfterCompactBack = false;
            SelectedItem = null;
        }
    }

    private static double Lerp(double from, double to, double progress)
    {
        return from + ((to - from) * progress);
    }

    private void ApplyCompactTarget(Control targetRoot)
    {
        if (_paneRoot is null || _contentRoot is null) return;

        _paneRoot.IsVisible = ReferenceEquals(targetRoot, _paneRoot);
        _contentRoot.IsVisible = ReferenceEquals(targetRoot, _contentRoot);
        _paneRoot.IsHitTestVisible = _paneRoot.IsVisible;
        _contentRoot.IsHitTestVisible = _contentRoot.IsVisible;
        _paneRoot.ZIndex = ReferenceEquals(targetRoot, _paneRoot) ? 1 : 0;
        _contentRoot.ZIndex = ReferenceEquals(targetRoot, _contentRoot) ? 1 : 0;
    }

    private static void ConfigureSplitRoot(Control root, int column)
    {
        Grid.SetColumn(root, column);
        Grid.SetColumnSpan(root, 1);
        root.IsHitTestVisible = true;
        root.ZIndex = 0;
    }

    private static void ConfigureCompactRoot(Control root)
    {
        Grid.SetColumn(root, 0);
        Grid.SetColumnSpan(root, 3);
    }

    private static void ResetRootVisualState(Control root)
    {
        root.Opacity = 1d;
        root.RenderTransform = null;
        root.IsHitTestVisible = true;
        root.ZIndex = 0;
    }

    private void CancelCompactTransition()
    {
        _clearSelectionAfterCompactBack = false;

        if (_compactAnimation is null) return;

        _compactAnimation.IsCancelled = true;
        _compactAnimation = null;
    }

    public void GoBack()
    {
        if (IsCompact && SelectedItem is not null && _paneRoot is not null && _contentRoot is not null)
        {
            var version = ++_layoutStateVersion;

            CancelCompactTransition();
            ResetRootVisualState(_paneRoot);
            ResetRootVisualState(_contentRoot);
            ConfigureCompactRoot(_paneRoot);
            ConfigureCompactRoot(_contentRoot);

            _activeCompactRoot = _paneRoot;
            _clearSelectionAfterCompactBack = true;

            StartCompactTransition(_paneRoot, _contentRoot, false, version);
            return;
        }

        SelectedItem = null;
    }

    private sealed class CompactAnimationState(
        DockPanel paneRoot,
        Control contentRoot,
        TranslateTransform paneTransform,
        TranslateTransform contentTransform,
        int version,
        bool forward,
        double paneFrom,
        double paneTo,
        double contentFrom,
        double contentTo
    )
    {
        public DockPanel PaneRoot { get; } = paneRoot;

        public Control ContentRoot { get; } = contentRoot;

        public TranslateTransform PaneTransform { get; } = paneTransform;

        public TranslateTransform ContentTransform { get; } = contentTransform;

        public int Version { get; } = version;

        public bool Forward { get; } = forward;

        public double PaneFrom { get; } = paneFrom;

        public double PaneTo { get; } = paneTo;

        public double ContentFrom { get; } = contentFrom;

        public double ContentTo { get; } = contentTo;

        public TimeSpan? StartTime { get; set; }

        public bool IsCancelled { get; set; }
    }
}