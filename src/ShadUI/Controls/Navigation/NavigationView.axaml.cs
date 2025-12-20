using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace ShadUI;

/// <summary>
/// A NavigationView contains a ListBox for selecting pages and a ContentControl for displaying the selected page.
/// It displays as a multi-column when there is enough space, and as a single-column with a hamburger menu when space is limited.
/// </summary>
[PseudoClasses(":compact", ":split", ":root", ":detail")]
[TemplatePart(RootPanelPartName, typeof(Panel), IsRequired = true)]
[TemplatePart(SplitContainerPartName, typeof(Grid), IsRequired = true)]
[TemplatePart(CompactContainerPartName, typeof(TransitioningContentControl), IsRequired = true)]
[TemplatePart(PaneRootPartName, typeof(DockPanel), IsRequired = true)]
[TemplatePart(ContentRootPartName, typeof(DockPanel), IsRequired = true)]
[TemplatePart(ContentContainerPartName, typeof(TransitioningContentControl), IsRequired = true)]
public class NavigationView : TemplatedControl
{
    private const string RootPanelPartName = "PART_RootPanel";
    private const string SplitContainerPartName = "PART_SplitContainer";
    private const string CompactContainerPartName = "PART_CompactContainer";
    private const string PaneRootPartName = "PART_PaneRoot";
    private const string ContentRootPartName = "PART_ContentRoot";
    private const string ContentContainerPartName = "PART_ContentContainer";

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

    public static readonly StyledProperty<GridLength> PaneWidthProperty =
        AvaloniaProperty.Register<NavigationView, GridLength>(nameof(PaneWidth), new GridLength(320d, GridUnitType.Pixel));

    public GridLength PaneWidth
    {
        get => GetValue(PaneWidthProperty);
        set => SetValue(PaneWidthProperty, value);
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

    private Panel? _rootPanel;
    private Grid? _splitContainer;
    private TransitioningContentControl? _compactContainer;
    private DockPanel? _paneRoot;
    private DockPanel? _contentRoot;
    private TransitioningContentControl? _contentContainer;

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

        _rootPanel = e.NameScope.Find<Panel>(RootPanelPartName);
        _splitContainer = e.NameScope.Find<Grid>(SplitContainerPartName);
        _compactContainer = e.NameScope.Find<TransitioningContentControl>(CompactContainerPartName);
        _paneRoot = e.NameScope.Find<DockPanel>(PaneRootPartName);
        _contentRoot = e.NameScope.Find<DockPanel>(ContentRootPartName);
        _contentContainer = e.NameScope.Find<TransitioningContentControl>(ContentContainerPartName);

        if (_rootPanel is not null)
        {
            // Ensure pane and content roots are not direct children of root panel
            if (_paneRoot is not null) _rootPanel.Children.Remove(_paneRoot);
            if (_contentRoot is not null) _rootPanel.Children.Remove(_contentRoot);
        }

        if (_splitContainer is not null)
        {
            _splitContainer.ColumnDefinitions.Clear();
            _splitContainer.ColumnDefinitions.Add(new ColumnDefinition
            {
                [!ColumnDefinition.WidthProperty] = this[!PaneWidthProperty]
            });
            _splitContainer.ColumnDefinitions.Add(new ColumnDefinition
            {
                [!ColumnDefinition.WidthProperty] = this[!SpacingProperty]
            });
            _splitContainer.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            });
        }

        UpdateLayoutState();
    }

    private void UpdateLayoutState()
    {
        if (_splitContainer == null || _compactContainer == null) return;

        if (IsCompact)
        {
            // Switch to Compact Mode
            if (_splitContainer.IsVisible)
            {
                _splitContainer.IsVisible = false;
                if (_paneRoot is not null) _splitContainer.Children.Remove(_paneRoot);
                if (_contentRoot is not null) _splitContainer.Children.Remove(_contentRoot);
            }

            _compactContainer.IsVisible = true;

            Control? targetContent = SelectedItem is not null ? _contentRoot : _paneRoot;

            if (!Equals(_compactContainer.Content, targetContent))
            {
                _compactContainer.PageTransition = CompactPageTransition;
                _compactContainer.Content = targetContent;
            }

            _contentContainer?.PageTransition = null;
        }
        else
        {
            // Switch to Split Mode
            if (_compactContainer.IsVisible)
            {
                _compactContainer.IsVisible = false;
                _compactContainer.PageTransition = null;
                _compactContainer.Content = null;
            }

            _splitContainer.IsVisible = true;
            _contentContainer?.PageTransition = PageTransition;

            if (_paneRoot is not null && !_splitContainer.Children.Contains(_paneRoot))
            {
                _splitContainer.Children.Add(_paneRoot);
                Grid.SetColumn(_paneRoot, 0);
            }

            if (_contentRoot is not null && !_splitContainer.Children.Contains(_contentRoot))
            {
                _splitContainer.Children.Add(_contentRoot);
                Grid.SetColumn(_contentRoot, 2);
            }
        }
    }

    public void GoBack()
    {
        SelectedItem = null;
    }
}