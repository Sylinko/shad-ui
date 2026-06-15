using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Styling;

namespace ShadUI;

/// <summary>
/// A versatile drawer control that can be placed on any edge of its parent container.
/// Supports inline (push) and overlay (float) display modes, with customizable splitter
/// and overlay backdrop.
/// </summary>
/// <remarks>
/// <para>
/// The drawer provides two display modes:
/// <list type="bullet">
/// <item><see cref="DrawerDisplayMode.Inline"/>: The drawer pushes the main content aside, sharing the available space.</item>
/// <item><see cref="DrawerDisplayMode.Overlay"/>: The drawer overlays on top of the main content.</item>
/// </list>
/// </para>
/// <para>
/// In overlay mode, when <see cref="OverlayBrush"/> is set, a semi-transparent backdrop is rendered
/// behind the drawer and clicking the backdrop will close the drawer.
/// When <see cref="OverlayBrush"/> is <c>null</c>, no backdrop is rendered and pointer events
/// pass through to the underlying content.
/// </para>
/// <para>
/// <see cref="DrawerWidth"/> and <see cref="DrawerHeight"/> are bindable properties that represent
/// the logical drawer size for the current orientation. They persist across open/close cycles,
/// making them suitable for saving user preferences. When the splitter is dragged, these properties
/// are automatically updated to reflect the new size.
/// </para>
/// <para>
/// The splitter appearance can be fully customized via <see cref="SplitterTheme"/>.
/// Transitions for overlay mode (slide-in/out, backdrop fade) are defined in the control template
/// and can be overridden via Avalonia styles.
/// </para>
/// </remarks>
[PseudoClasses(":opened", ":closed", ":left", ":right", ":top", ":bottom", ":inline", ":overlay")]
[TemplatePart(LayoutGridPartName, typeof(Grid), IsRequired = true)]
[TemplatePart(ContentPresenterPartName, typeof(ContentPresenter), IsRequired = true)]
[TemplatePart(SplitterPartName, typeof(GridSplitter), IsRequired = true)]
[TemplatePart(OverlayBackdropPartName, typeof(Border), IsRequired = true)]
[TemplatePart(DrawerBorderPartName, typeof(Border), IsRequired = true)]
[TemplatePart(DrawerPresenterPartName, typeof(ContentPresenter), IsRequired = true)]
public class Drawer : ContentControl
{
    private const string LayoutGridPartName = "PART_LayoutGrid";
    private const string ContentPresenterPartName = "PART_ContentPresenter";
    private const string SplitterPartName = "PART_Splitter";
    private const string OverlayBackdropPartName = "PART_OverlayBackdrop";
    private const string DrawerBorderPartName = "PART_DrawerBorder";
    private const string DrawerPresenterPartName = "PART_DrawerPresenter";

    #region Template Parts

    private Grid? _layoutGrid;
    private ContentPresenter? _contentPresenter;
    private GridSplitter? _splitter;
    private Border? _overlayBackdrop;
    private Border? _drawerBorder;

    #endregion

    #region Grid Definition State

    private ColumnDefinition? _drawerColumn;
    private ColumnDefinition? _splitterColumn;
    private ColumnDefinition? _contentColumn;
    private RowDefinition? _drawerRow;
    private RowDefinition? _splitterRow;
    private RowDefinition? _contentRow;
    private IDisposable? _drawerColumnSubscription;
    private IDisposable? _drawerRowSubscription;
    private bool _isSyncingFromSplitter;

    #endregion

    #region Properties

    /// <summary>
    /// Defines the <see cref="DrawerContent"/> property.
    /// </summary>
    public static readonly StyledProperty<object?> DrawerContentProperty =
        AvaloniaProperty.Register<Drawer, object?>(nameof(DrawerContent));

    /// <summary>
    /// Gets or sets the content of the drawer.
    /// </summary>
    public object? DrawerContent
    {
        get => GetValue(DrawerContentProperty);
        set => SetValue(DrawerContentProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="DrawerContentTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> DrawerContentTemplateProperty =
        AvaloniaProperty.Register<Drawer, IDataTemplate?>(nameof(DrawerContentTemplate));

    /// <summary>
    /// Gets or sets the data template used to display the content of the drawer.
    /// </summary>
    public IDataTemplate? DrawerContentTemplate
    {
        get => GetValue(DrawerContentTemplateProperty);
        set => SetValue(DrawerContentTemplateProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="IsOpened"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenedProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(IsOpened));

    /// <summary>
    /// Gets or sets a value indicating whether the drawer is open.
    /// </summary>
    public bool IsOpened
    {
        get => GetValue(IsOpenedProperty);
        set => SetValue(IsOpenedProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="Placement"/> property.
    /// </summary>
    public static readonly StyledProperty<DrawerPlacement> PlacementProperty =
        AvaloniaProperty.Register<Drawer, DrawerPlacement>(nameof(Placement), DrawerPlacement.Right);

    /// <summary>
    /// Gets or sets the edge of the parent container where the drawer should appear.
    /// </summary>
    public DrawerPlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="DisplayMode"/> property.
    /// </summary>
    public static readonly StyledProperty<DrawerDisplayMode> DisplayModeProperty =
        AvaloniaProperty.Register<Drawer, DrawerDisplayMode>(nameof(DisplayMode));

    /// <summary>
    /// Gets or sets how the drawer content is displayed relative to the main content.
    /// </summary>
    public DrawerDisplayMode DisplayMode
    {
        get => GetValue(DisplayModeProperty);
        set => SetValue(DisplayModeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="OverlayBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> OverlayBrushProperty =
        AvaloniaProperty.Register<Drawer, IBrush?>(nameof(OverlayBrush));

    /// <summary>
    /// Gets or sets the brush used for the overlay backdrop behind the drawer.
    /// When set to a non-null value in overlay mode, a backdrop is rendered behind the drawer
    /// and clicking the backdrop will close the drawer.
    /// When set to <c>null</c>, no backdrop is rendered and pointer events pass through
    /// to the underlying content.
    /// </summary>
    public IBrush? OverlayBrush
    {
        get => GetValue(OverlayBrushProperty);
        set => SetValue(OverlayBrushProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="DrawerWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> DrawerWidthProperty =
        AvaloniaProperty.Register<Drawer, double>(nameof(DrawerWidth), 300d);

    /// <summary>
    /// Gets or sets the width of the drawer when <see cref="Placement"/> is
    /// <see cref="DrawerPlacement.Left"/> or <see cref="DrawerPlacement.Right"/>.
    /// This value is bindable and persists across open/close cycles, making it suitable
    /// for saving user preferences. The value represents the logical drawer width
    /// for the current orientation, not the real-time rendered width.
    /// When the splitter is dragged, this property is automatically updated.
    /// </summary>
    public double DrawerWidth
    {
        get => GetValue(DrawerWidthProperty);
        set => SetValue(DrawerWidthProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="DrawerHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<double> DrawerHeightProperty =
        AvaloniaProperty.Register<Drawer, double>(nameof(DrawerHeight), 300d);

    /// <summary>
    /// Gets or sets the height of the drawer when <see cref="Placement"/> is
    /// <see cref="DrawerPlacement.Top"/> or <see cref="DrawerPlacement.Bottom"/>.
    /// This value is bindable and persists across open/close cycles, making it suitable
    /// for saving user preferences. The value represents the logical drawer height
    /// for the current orientation, not the real-time rendered height.
    /// When the splitter is dragged, this property is automatically updated.
    /// </summary>
    public double DrawerHeight
    {
        get => GetValue(DrawerHeightProperty);
        set => SetValue(DrawerHeightProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="DrawerMinWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> DrawerMinWidthProperty =
        AvaloniaProperty.Register<Drawer, double>(nameof(DrawerMinWidth));

    /// <summary>
    /// Gets or sets the minimum width of the drawer when <see cref="Placement"/> is
    /// <see cref="DrawerPlacement.Left"/> or <see cref="DrawerPlacement.Right"/>.
    /// </summary>
    public double DrawerMinWidth
    {
        get => GetValue(DrawerMinWidthProperty);
        set => SetValue(DrawerMinWidthProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="DrawerMaxWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> DrawerMaxWidthProperty =
        AvaloniaProperty.Register<Drawer, double>(nameof(DrawerMaxWidth), double.PositiveInfinity);

    /// <summary>
    /// Gets or sets the maximum width of the drawer when <see cref="Placement"/> is
    /// <see cref="DrawerPlacement.Left"/> or <see cref="DrawerPlacement.Right"/>.
    /// </summary>
    public double DrawerMaxWidth
    {
        get => GetValue(DrawerMaxWidthProperty);
        set => SetValue(DrawerMaxWidthProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="DrawerMinHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<double> DrawerMinHeightProperty =
        AvaloniaProperty.Register<Drawer, double>(nameof(DrawerMinHeight));

    /// <summary>
    /// Gets or sets the minimum height of the drawer when <see cref="Placement"/> is
    /// <see cref="DrawerPlacement.Top"/> or <see cref="DrawerPlacement.Bottom"/>.
    /// </summary>
    public double DrawerMinHeight
    {
        get => GetValue(DrawerMinHeightProperty);
        set => SetValue(DrawerMinHeightProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="DrawerMaxHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<double> DrawerMaxHeightProperty =
        AvaloniaProperty.Register<Drawer, double>(nameof(DrawerMaxHeight), double.PositiveInfinity);

    /// <summary>
    /// Gets or sets the maximum height of the drawer when <see cref="Placement"/> is
    /// <see cref="DrawerPlacement.Top"/> or <see cref="DrawerPlacement.Bottom"/>.
    /// </summary>
    public double DrawerMaxHeight
    {
        get => GetValue(DrawerMaxHeightProperty);
        set => SetValue(DrawerMaxHeightProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="SplitterTheme"/> property.
    /// </summary>
    public static readonly StyledProperty<ControlTheme?> SplitterThemeProperty =
        AvaloniaProperty.Register<Drawer, ControlTheme?>(nameof(SplitterTheme));

    /// <summary>
    /// Gets or sets the template used to render the splitter between the main content and the drawer.
    /// This allows full customization of the splitter's visual appearance.
    /// The splitter's resize behavior is built into the <see cref="GridSplitter"/> class
    /// and is preserved regardless of the template.
    /// </summary>
    public ControlTheme? SplitterTheme
    {
        get => GetValue(SplitterThemeProperty);
        set => SetValue(SplitterThemeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="SplitterSize"/> property.
    /// </summary>
    public static readonly StyledProperty<double> SplitterSizeProperty =
        AvaloniaProperty.Register<Drawer, double>(nameof(SplitterSize), 4d);

    /// <summary>
    /// Gets or sets the thickness of the splitter in device-independent pixels.
    /// For left/right placement, this is the splitter width.
    /// For top/bottom placement, this is the splitter height.
    /// </summary>
    public double SplitterSize
    {
        get => GetValue(SplitterSizeProperty);
        set => SetValue(SplitterSizeProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="BoxShadow"/> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadow> BoxShadowProperty =
        AvaloniaProperty.Register<Drawer, BoxShadow>(nameof(BoxShadow));

    /// <summary>
    /// Gets or sets the box shadow applied to the overlay drawer border.
    /// </summary>
    public BoxShadow BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="CanAutoCollapse"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanAutoCollapseProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(CanAutoCollapse), true);

    /// <summary>
    /// Gets or sets a value indicating whether the drawer can be automatically collapsed
    /// when the user drags the splitter below <see cref="CollapseThreshold"/>.
    /// When <c>true</c>, dragging the splitter so that the drawer size falls below
    /// <see cref="CollapseThreshold"/> will set <see cref="IsOpened"/> to <c>false</c>.
    /// When <c>false</c>, the drawer size is clamped to the minimum and cannot be
    /// collapsed via the splitter.
    /// </summary>
    public bool CanAutoCollapse
    {
        get => GetValue(CanAutoCollapseProperty);
        set => SetValue(CanAutoCollapseProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="CollapseThreshold"/> property.
    /// </summary>
    public static readonly StyledProperty<double> CollapseThresholdProperty =
        AvaloniaProperty.Register<Drawer, double>(nameof(CollapseThreshold), 20d);

    /// <summary>
    /// Gets or sets the size threshold (in device-independent pixels) below which
    /// the drawer will auto-collapse when <see cref="CanAutoCollapse"/> is <c>true</c>.
    /// The drawer only collapses when the user drags the splitter so that the drawer
    /// size falls below this value, providing a "snap-to-close" feel near zero.
    /// Defaults to 20.
    /// </summary>
    public double CollapseThreshold
    {
        get => GetValue(CollapseThresholdProperty);
        set => SetValue(CollapseThresholdProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="ShowSplitterWhenCollapsed"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShowSplitterWhenCollapsedProperty =
        AvaloniaProperty.Register<Drawer, bool>(nameof(ShowSplitterWhenCollapsed));

    /// <summary>
    /// Gets or sets a value indicating whether the splitter remains visible when the drawer
    /// is collapsed (<see cref="IsOpened"/> is <c>false</c>).
    /// When <c>true</c>, the splitter is always visible, allowing the user to drag it
    /// to re-open the drawer.
    /// When <c>false</c>, the splitter is hidden when the drawer is closed.
    /// </summary>
    public bool ShowSplitterWhenCollapsed
    {
        get => GetValue(ShowSplitterWhenCollapsedProperty);
        set => SetValue(ShowSplitterWhenCollapsedProperty, value);
    }

    #endregion

    #region Template Application

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        CleanupParts();

        _layoutGrid = e.NameScope.Find<Grid>(LayoutGridPartName);
        _contentPresenter = e.NameScope.Find<ContentPresenter>(ContentPresenterPartName);
        _splitter = e.NameScope.Find<GridSplitter>(SplitterPartName);
        _overlayBackdrop = e.NameScope.Find<Border>(OverlayBackdropPartName);
        _drawerBorder = e.NameScope.Find<Border>(DrawerBorderPartName);

        if (_overlayBackdrop != null)
        {
            _overlayBackdrop.PointerPressed += OnOverlayBackdropPointerPressed;
        }

        UpdateAll();
    }

    private void CleanupParts()
    {
        if (_overlayBackdrop != null)
        {
            _overlayBackdrop.PointerPressed -= OnOverlayBackdropPointerPressed;
        }

        _drawerColumnSubscription?.Dispose();
        _drawerColumnSubscription = null;
        _drawerRowSubscription?.Dispose();
        _drawerRowSubscription = null;
    }

    #endregion

    #region Property Changes

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        switch (change.Property.Name)
        {
            case nameof(IsOpened):
            {
                UpdatePseudoClasses();
                UpdateLayoutState(false);
                break;
            }
            case nameof(Placement):
            {
                UpdatePseudoClasses();
                RebuildGridLayout();
                UpdateLayoutState(!IsOpened);
                break;
            }
            case nameof(DisplayMode):
            {
                UpdatePseudoClasses();
                RebuildGridLayout();
                UpdateLayoutState(!IsOpened);
                break;
            }
            case nameof(OverlayBrush):
            {
                UpdateOverlayBackdrop();
                break;
            }
            case nameof(DrawerWidth):
            case nameof(DrawerHeight):
            {
                if (!_isSyncingFromSplitter)
                {
                    UpdateGridLayout();
                    UpdateDrawerLayout(!IsOpened);
                }
                break;
            }
            case nameof(DrawerMinWidth):
            case nameof(DrawerMaxWidth):
            case nameof(DrawerMinHeight):
            case nameof(DrawerMaxHeight):
            {
                UpdateGridLayout();
                UpdateDrawerLayout(!IsOpened);
                break;
            }
            case nameof(SplitterTheme):
            case nameof(SplitterSize):
            {
                UpdateSplitter();
                break;
            }
            case nameof(ShowSplitterWhenCollapsed):
            {
                UpdateSplitter();
                break;
            }
        }
    }

    #endregion

    #region Layout Updates

    private void UpdateAll()
    {
        UpdatePseudoClasses();
        RebuildGridLayout();
        UpdateLayoutState(true);
    }

    private void UpdatePseudoClasses()
    {
        var isOpened = IsOpened;
        PseudoClasses.Set(":opened", isOpened);
        PseudoClasses.Set(":closed", !isOpened);

        var placement = Placement;
        PseudoClasses.Set(":left", placement == DrawerPlacement.Left);
        PseudoClasses.Set(":right", placement == DrawerPlacement.Right);
        PseudoClasses.Set(":top", placement == DrawerPlacement.Top);
        PseudoClasses.Set(":bottom", placement == DrawerPlacement.Bottom);

        var displayMode = DisplayMode;
        PseudoClasses.Set(":inline", displayMode == DrawerDisplayMode.Inline);
        PseudoClasses.Set(":overlay", displayMode == DrawerDisplayMode.Overlay);
    }

    private void UpdateLayoutState(bool suppressClosedTransition)
    {
        UpdateModeVisibility();
        UpdateGridLayout();
        UpdateDrawerLayout(suppressClosedTransition);
        UpdateSplitter();
        UpdateOverlayBackdrop();
    }

    private void UpdateModeVisibility()
    {
        var isOverlay = DisplayMode == DrawerDisplayMode.Overlay;
        if (_overlayBackdrop is not null) _overlayBackdrop.IsVisible = isOverlay;
        if (_drawerBorder is not null) _drawerBorder.IsVisible = true;
    }

    /// <summary>
    /// Rebuilds the Grid layout definitions. Content and drawer presenters stay in the same visual tree;
    /// only Grid placement changes.
    /// Disposes old subscriptions and creates new column/row definitions.
    /// </summary>
    private void RebuildGridLayout()
    {
        if (_layoutGrid == null) return;

        _drawerColumnSubscription?.Dispose();
        _drawerColumnSubscription = null;
        _drawerRowSubscription?.Dispose();
        _drawerRowSubscription = null;

        // Suppress subscription callbacks from overwriting DrawerWidth/DrawerHeight
        // with default GridLength values during rebuild.
        _isSyncingFromSplitter = true;
        try
        {
            if (DisplayMode == DrawerDisplayMode.Overlay)
            {
                _layoutGrid.ColumnDefinitions = [new ColumnDefinition(GridLength.Star)];
                _layoutGrid.RowDefinitions = [new RowDefinition(GridLength.Star)];

                SetGridPosition(_contentPresenter, 0, 0);
                SetGridPosition(_overlayBackdrop, 0, 0);
                SetGridPosition(_drawerBorder, 0, 0);
                SetGridPosition(_splitter, 0, 0);

                _drawerColumn = null;
                _splitterColumn = null;
                _contentColumn = null;
                _drawerRow = null;
                _splitterRow = null;
                _contentRow = null;

                return;
            }

            var isHorizontal = Placement is DrawerPlacement.Left or DrawerPlacement.Right;
            if (isHorizontal)
            {
                _layoutGrid.RowDefinitions = [new RowDefinition(GridLength.Star)];

                _drawerColumn = new ColumnDefinition();
                _splitterColumn = new ColumnDefinition(GridLength.Auto);
                _contentColumn = new ColumnDefinition(GridLength.Star);

                var isLeft = Placement == DrawerPlacement.Left;
                _layoutGrid.ColumnDefinitions = isLeft ?
                    [_drawerColumn, _splitterColumn, _contentColumn] :
                    [_contentColumn, _splitterColumn, _drawerColumn];

                SetGridPosition(_contentPresenter, isLeft ? 2 : 0, 0);
                SetGridPosition(_overlayBackdrop, 0, 0, 3);
                SetGridPosition(_splitter, 1, 0);
                SetGridPosition(_drawerBorder, isLeft ? 0 : 2, 0);

                _drawerRow = null;
                _splitterRow = null;
                _contentRow = null;

                _drawerColumnSubscription = _drawerColumn
                    .GetObservable(ColumnDefinition.WidthProperty)
                    .Subscribe(new AnonymousObserver<GridLength>(OnDrawerColumnWidthChanged));
            }
            else
            {
                _layoutGrid.ColumnDefinitions = [new ColumnDefinition(GridLength.Star)];

                _drawerRow = new RowDefinition();
                _splitterRow = new RowDefinition(GridLength.Auto);
                _contentRow = new RowDefinition(GridLength.Star);

                var isTop = Placement == DrawerPlacement.Top;
                _layoutGrid.RowDefinitions = isTop ?
                    [_drawerRow, _splitterRow, _contentRow] :
                    [_contentRow, _splitterRow, _drawerRow];

                SetGridPosition(_contentPresenter, 0, isTop ? 2 : 0);
                SetGridPosition(_overlayBackdrop, 0, 0, 1, 3);
                SetGridPosition(_splitter, 0, 1);
                SetGridPosition(_drawerBorder, 0, isTop ? 0 : 2);

                _drawerColumn = null;
                _splitterColumn = null;
                _contentColumn = null;

                _drawerRowSubscription = _drawerRow
                    .GetObservable(RowDefinition.HeightProperty)
                    .Subscribe(new AnonymousObserver<GridLength>(OnDrawerRowHeightChanged));
            }
        }
        finally
        {
            _isSyncingFromSplitter = false;
        }

        UpdateGridLayout();
    }

    private static void SetGridPosition(Control? control, int column, int row, int columnSpan = 1, int rowSpan = 1)
    {
        if (control is null) return;

        Grid.SetColumn(control, column);
        Grid.SetRow(control, row);
        Grid.SetColumnSpan(control, columnSpan);
        Grid.SetRowSpan(control, rowSpan);
    }

    /// <summary>
    /// Updates the inline Grid's column/row definitions based on current IsOpened and size properties.
    /// When closed, the drawer and splitter definitions collapse to 0.
    /// </summary>
    private void UpdateGridLayout()
    {
        if (_isSyncingFromSplitter) return;
        if (DisplayMode != DrawerDisplayMode.Inline) return;

        var isHorizontal = Placement is DrawerPlacement.Left or DrawerPlacement.Right;
        if (isHorizontal)
        {
            var width = IsOpened ? Math.Max(DrawerMinWidth, Math.Min(DrawerMaxWidth, DrawerWidth)) : 0;
            _drawerColumn?.Width = new GridLength(width);
            _splitterColumn?.Width = (IsOpened || ShowSplitterWhenCollapsed) ? GridLength.Auto : new GridLength(0);
        }
        else
        {
            var height = IsOpened ? Math.Max(DrawerMinHeight, Math.Min(DrawerMaxHeight, DrawerHeight)) : 0;
            _drawerRow?.Height = new GridLength(height);
            _splitterRow?.Height = (IsOpened || ShowSplitterWhenCollapsed) ? GridLength.Auto : new GridLength(0);
        }
    }

    /// <summary>
    /// Updates the drawer's position and size based on the current display mode and placement.
    /// </summary>
    private void UpdateDrawerLayout(bool suppressClosedTransition)
    {
        if (_drawerBorder == null) return;

        if (DisplayMode == DrawerDisplayMode.Inline)
        {
            _drawerBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            _drawerBorder.VerticalAlignment = VerticalAlignment.Stretch;
            _drawerBorder.Width = double.NaN;
            _drawerBorder.Height = double.NaN;
            _drawerBorder.ZIndex = 0;
            _drawerBorder.IsHitTestVisible = IsOpened;
            SetDrawerTranslation(0d, 0d, suppressClosedTransition);
            return;
        }

        var isHorizontal = Placement is DrawerPlacement.Left or DrawerPlacement.Right;
        if (isHorizontal)
        {
            var width = Math.Max(DrawerMinWidth, Math.Min(DrawerMaxWidth, DrawerWidth));
            _drawerBorder.HorizontalAlignment = Placement == DrawerPlacement.Left ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            _drawerBorder.VerticalAlignment = VerticalAlignment.Stretch;
            _drawerBorder.Width = width;
            _drawerBorder.Height = double.NaN;
        }
        else
        {
            var height = Math.Max(DrawerMinHeight, Math.Min(DrawerMaxHeight, DrawerHeight));
            _drawerBorder.HorizontalAlignment = HorizontalAlignment.Stretch;
            _drawerBorder.VerticalAlignment = Placement == DrawerPlacement.Top ? VerticalAlignment.Top : VerticalAlignment.Bottom;
            _drawerBorder.Width = double.NaN;
            _drawerBorder.Height = height;
        }

        _drawerBorder.ZIndex = 2;
        _drawerBorder.IsHitTestVisible = IsOpened;
        UpdateOverlayTransform(suppressClosedTransition);
    }

    /// <summary>
    /// Updates the overlay drawer's RenderTransform for slide-in/out animation.
    /// When opened, the drawer is at its natural position (no transform).
    /// When closed, the drawer is translated off-screen.
    /// Reuses the <see cref="TranslateTransform"/> instance from the template
    /// so that the transitions defined in AXAML can animate the X/Y properties.
    /// </summary>
    private void UpdateOverlayTransform(bool suppressClosedTransition)
    {
        if (DisplayMode != DrawerDisplayMode.Overlay || IsOpened)
        {
            SetDrawerTranslation(0d, 0d, suppressClosedTransition);
            return;
        }

        var isHorizontal = Placement is DrawerPlacement.Left or DrawerPlacement.Right;
        if (isHorizontal)
        {
            var width = Math.Max(DrawerMinWidth, Math.Min(DrawerMaxWidth, DrawerWidth));
            SetDrawerTranslation(Placement == DrawerPlacement.Left ? -width : width, 0d, suppressClosedTransition);
        }
        else
        {
            var height = Math.Max(DrawerMinHeight, Math.Min(DrawerMaxHeight, DrawerHeight));
            SetDrawerTranslation(0d, Placement == DrawerPlacement.Top ? -height : height, suppressClosedTransition);
        }
    }

    private void SetDrawerTranslation(double x, double y, bool suppressTransitions)
    {
        if (_drawerBorder?.RenderTransform is not TranslateTransform translate) return;

        if (!suppressTransitions)
        {
            translate.X = x;
            translate.Y = y;
            return;
        }

        var transitions = translate.Transitions;
        translate.Transitions = null;
        translate.X = x;
        translate.Y = y;
        translate.Transitions = transitions;
    }

    /// <summary>
    /// Updates the overlay backdrop visibility and hit-test state.
    /// When <see cref="OverlayBrush"/> is null, the backdrop is invisible and does not capture pointer events.
    /// When <see cref="OverlayBrush"/> is set, the backdrop fades in/out with <see cref="IsOpened"/>.
    /// </summary>
    private void UpdateOverlayBackdrop()
    {
        if (_overlayBackdrop == null) return;

        var isOverlay = DisplayMode == DrawerDisplayMode.Overlay;
        var isOpened = isOverlay && IsOpened;
        _overlayBackdrop.IsVisible = isOverlay;
        _overlayBackdrop.IsHitTestVisible = isOpened && OverlayBrush is not null;
        _overlayBackdrop.Opacity = isOpened ? 1 : 0;
        _overlayBackdrop.ZIndex = 1;
    }

    /// <summary>
    /// Updates the GridSplitter's resize direction, size, and visibility.
    /// </summary>
    private void UpdateSplitter()
    {
        if (_splitter == null) return;

        var isHorizontal = Placement is DrawerPlacement.Left or DrawerPlacement.Right;
        _splitter.ResizeDirection = isHorizontal ? GridResizeDirection.Columns : GridResizeDirection.Rows;

        if (isHorizontal)
        {
            _splitter.Width = SplitterSize;
            _splitter.Height = double.NaN;
        }
        else
        {
            _splitter.Width = double.NaN;
            _splitter.Height = SplitterSize;
        }

        _splitter.IsVisible = DisplayMode == DrawerDisplayMode.Inline && (IsOpened || ShowSplitterWhenCollapsed);
        _splitter.ZIndex = 1;
    }

    #endregion

    #region Splitter Size Sync

    private void OnDrawerColumnWidthChanged(GridLength width)
    {
        if (_isSyncingFromSplitter) return;
        if (width.IsAuto || width.Value <= 0) return;

        _isSyncingFromSplitter = true;
        try
        {
            // If the drawer is closed but the splitter is visible (ShowSplitterWhenCollapsed),
            // dragging the splitter should re-open the drawer.
            if (!IsOpened && width.Value > 0)
            {
                SetCurrentValue(DrawerWidthProperty, Math.Max(DrawerMinWidth, width.Value));
                SetCurrentValue(IsOpenedProperty, true);
                // OnPropertyChanged is suppressed by _isSyncingFromSplitter,
                // so we must manually update pseudo-classes and splitter visibility.
                UpdatePseudoClasses();
                UpdateSplitter();
                return;
            }

            // Auto-collapse: if the dragged size falls below the collapse threshold,
            // close the drawer instead of clamping. This provides a "snap-to-close" feel
            // near zero rather than triggering at the minimum size.
            if (CanAutoCollapse && width.Value < CollapseThreshold)
            {
                SetCurrentValue(IsOpenedProperty, false);
                // Manually collapse the grid since OnPropertyChanged is suppressed.
                if (_drawerColumn is not null) _drawerColumn.Width = new GridLength(0);
                if (_splitterColumn is not null)
                {
                    _splitterColumn.Width = ShowSplitterWhenCollapsed ? GridLength.Auto : new GridLength(0);
                }
                UpdatePseudoClasses();
                UpdateSplitter();
                return;
            }

            var clamped = Math.Max(DrawerMinWidth, Math.Min(DrawerMaxWidth, width.Value));
            SetCurrentValue(DrawerWidthProperty, clamped);

            // Clamp the column definition if the splitter exceeded min/max bounds
            if (_drawerColumn != null && Math.Abs(width.Value - clamped) > 0.01)
            {
                _drawerColumn.Width = new GridLength(clamped);
            }
        }
        finally
        {
            _isSyncingFromSplitter = false;
        }
    }

    private void OnDrawerRowHeightChanged(GridLength height)
    {
        if (_isSyncingFromSplitter) return;
        if (height.IsAuto || height.Value <= 0) return;

        _isSyncingFromSplitter = true;
        try
        {
            // If the drawer is closed but the splitter is visible (ShowSplitterWhenCollapsed),
            // dragging the splitter should re-open the drawer.
            if (!IsOpened && height.Value > 0)
            {
                SetCurrentValue(DrawerHeightProperty, Math.Max(DrawerMinHeight, height.Value));
                SetCurrentValue(IsOpenedProperty, true);
                // OnPropertyChanged is suppressed by _isSyncingFromSplitter,
                // so we must manually update pseudo-classes and splitter visibility.
                UpdatePseudoClasses();
                UpdateSplitter();
                return;
            }

            // Auto-collapse: if the dragged size falls below the collapse threshold,
            // close the drawer instead of clamping. This provides a "snap-to-close" feel
            // near zero rather than triggering at the minimum size.
            if (CanAutoCollapse && height.Value < CollapseThreshold)
            {
                SetCurrentValue(IsOpenedProperty, false);
                // Manually collapse the grid since OnPropertyChanged is suppressed.
                _drawerRow?.Height = new GridLength(0);
                _splitterRow?.Height = ShowSplitterWhenCollapsed ? GridLength.Auto : new GridLength(0);
                UpdatePseudoClasses();
                UpdateSplitter();
                return;
            }

            var clamped = Math.Max(DrawerMinHeight, Math.Min(DrawerMaxHeight, height.Value));
            SetCurrentValue(DrawerHeightProperty, clamped);

            // Clamp the row definition if the splitter exceeded min/max bounds
            if (_drawerRow != null && Math.Abs(height.Value - clamped) > 0.01)
            {
                _drawerRow.Height = new GridLength(clamped);
            }
        }
        finally
        {
            _isSyncingFromSplitter = false;
        }
    }

    #endregion

    #region Event Handlers

    private void OnOverlayBackdropPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (OverlayBrush != null)
        {
            SetCurrentValue(IsOpenedProperty, false);
        }
    }

    #endregion

}