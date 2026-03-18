using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Lucide.Avalonia;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Represents a selectable item within a navigation bar control.
/// </summary>
[TemplatePart("PART_BorderContainer", typeof(Border))]
[TemplatePart("PART_ContentPresenter", typeof(ContentPresenter))]
[PseudoClasses(":expanded", ":vertical", ":horizontal")]
public class NavigationBarItem : ContentControl
{
    /// <summary>
    ///     Icon property.
    /// </summary>
    public static readonly StyledProperty<LucideIconKind?> IconProperty =
        AvaloniaProperty.Register<NavigationBarItem, LucideIconKind?>(nameof(Icon));

    /// <summary>
    ///     Gets or sets the icon of the menu item.
    /// </summary>
    public LucideIconKind? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<object?> ToolTipProperty =
        AvaloniaProperty.Register<NavigationBarItem, object?>(nameof(ToolTip));

    public object? ToolTip
    {
        get => GetValue(ToolTipProperty);
        set => SetValue(ToolTipProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="GroupName" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> GroupNameProperty =
        AvaloniaProperty.Register<NavigationBarItem, string?>(nameof(GroupName));

    /// <summary>
    ///     Gets or sets the name of the group that this navigation bar item belongs to. Used for grouping items together for selection behavior.
    /// </summary>
    public string? GroupName
    {
        get => GetValue(GroupNameProperty);
        set => SetValue(GroupNameProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="IsChecked" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsCheckedProperty =
        AvaloniaProperty.Register<NavigationBarItem, bool>(nameof(IsChecked));

    /// <summary>
    ///     Gets or sets a value indicating whether this navigation bar item is currently selected or active.
    /// </summary>
    public bool IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="Route" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> RouteProperty =
        AvaloniaProperty.Register<NavigationBarItem, object?>(nameof(Route));

    /// <summary>
    ///     Gets or sets the route or navigation path associated with this navigation bar item.
    ///     This property is useful for:
    ///     <list type="bullet">
    ///         <item><description>Navigation-aware navigation bar behavior where items can be highlighted based on the current route</description></item>
    ///         <item><description>Implementing automatic selection of navigation bar items when navigating to specific pages</description></item>
    ///         <item><description>Enabling deep linking scenarios where the navigation bar reflects the current application state</description></item>
    ///         <item><description>Facilitating route-based navigation bar item activation and deactivation</description></item>
    ///     </list>
    /// </summary>
    public object? Route
    {
        get => GetValue(RouteProperty);
        set => SetValue(RouteProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="Children" /> property.
    /// </summary>
    public static readonly DirectProperty<NavigationBarItem, AvaloniaList<NavigationBarItem>> ChildrenProperty =
        AvaloniaProperty.RegisterDirect<NavigationBarItem, AvaloniaList<NavigationBarItem>>(
        nameof(Children),
        o => o.Children);

    /// <summary>
    ///     Gets the collection of child navigation bar items, allowing for hierarchical navigation structures.
    /// </summary>
    public AvaloniaList<NavigationBarItem> Children { get; } = [];

    /// <summary>
    ///    Defines the <see cref="Index" /> property.
    /// </summary>
    public static readonly DirectProperty<NavigationBarItem, int> IndexProperty =
        AvaloniaProperty.RegisterDirect<NavigationBarItem, int>(
        nameof(Index),
        o => o.Index);

    /// <summary>
    ///     Gets the index of this navigation bar item within its parent navigation bar. Used for animation.
    /// </summary>
    public int Index
    {
        get;
        internal set => SetAndRaise(IndexProperty, ref field, value);
    }

    public static readonly DirectProperty<NavigationBarItem, bool> IsChildrenCheckedProperty =
        AvaloniaProperty.RegisterDirect<NavigationBarItem, bool>(
        nameof(IsChildrenChecked), o => o.IsChildrenChecked);

    public bool IsChildrenChecked => Children.Any(c => c.IsChecked);

    private NavigationBar? _navigationBar;
    private NavigationBarItem? _parent;

    public NavigationBarItem() { }

    public NavigationBarItem(object? route)
    {
        Route = route;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _navigationBar = this.GetVisualAncestors().OfType<NavigationBar>().FirstOrDefault();
        if (_navigationBar is not null)
        {
            _navigationBar.RearrangeChildrenIndex();
            _navigationBar.PropertyChanged += HandleNavigationBarPropertyChanged;

            GroupName = _navigationBar.DefaultItemsGroup;
            PseudoClasses.Set(":expanded", _navigationBar.IsExpanded);
            var orientation = _navigationBar.Orientation;
            PseudoClasses.Set(":vertical", orientation == Orientation.Vertical);
            PseudoClasses.Set(":horizontal", orientation == Orientation.Horizontal);
        }

        _parent = this.GetVisualAncestors().OfType<NavigationBarItem>().FirstOrDefault();
        if (_parent is not null)
        {
            if (_parent._parent is not null)
            {
                throw new InvalidOperationException("NavigationBarItem cannot be nested more than two levels deep.");
            }

            GroupName = _parent.GroupName;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _parent = null;

        if (_navigationBar is not null)
        {
            _navigationBar.RearrangeChildrenIndex();
            _navigationBar.PropertyChanged -= HandleNavigationBarPropertyChanged;
            _navigationBar = null;
        }
    }

    private void HandleNavigationBarPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == NavigationBar.IsExpandedProperty)
        {
            var isExpanded = e.GetNewValue<bool>();
            PseudoClasses.Set(":expanded", isExpanded);
        }
        else if (e.Property == NavigationBar.OrientationProperty)
        {
            var orientation = e.GetNewValue<Orientation>();
            PseudoClasses.Set(":vertical", orientation == Orientation.Vertical);
            PseudoClasses.Set(":horizontal", orientation == Orientation.Horizontal);
        }
    }

    /// <summary>
    ///     Called when a property value changes.
    /// </summary>
    /// <param name="change">The property change event arguments.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != IsCheckedProperty) return;

        {
            var value = IsChildrenChecked;
            RaisePropertyChanged(IsChildrenCheckedProperty, !value, value);
        }
        if (_parent is not null)
        {
            var value = _parent.IsChildrenChecked;
            _parent.RaisePropertyChanged(IsChildrenCheckedProperty, !value, value);
        }

        if (change.NewValue is not true) return;

        NavigationBarItem? selectedItem;
        if (Route is not null)
        {
            selectedItem = this;
        }
        else if (Children.Count > 0)
        {
            selectedItem = Children[0];
            selectedItem.IsChecked = true;
        }
        else
        {
            selectedItem = null;
        }

        _navigationBar?.SelectedItem = selectedItem;
    }
}