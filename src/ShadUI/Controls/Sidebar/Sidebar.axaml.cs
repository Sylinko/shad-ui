using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Represents a sidebar control that can be expanded or collapsed.
/// </summary>
public class Sidebar : ContentControl
{
    /// <summary>
    ///     Defines the <see cref="Expanded" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ExpandedProperty = AvaloniaProperty.Register<Sidebar, bool>(
        nameof(Expanded), true);

    /// <summary>
    ///     Gets or sets a value indicating whether the sidebar is expanded.
    /// </summary>
    public bool Expanded
    {
        get => GetValue(ExpandedProperty);
        set => SetValue(ExpandedProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="Header" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderProperty = AvaloniaProperty.Register<Sidebar, object?>(
        nameof(Header));

    /// <summary>
    ///     Gets or sets the header content of the sidebar.
    /// </summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="Footer" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> FooterProperty = AvaloniaProperty.Register<Sidebar, object?>(
        nameof(Footer));

    /// <summary>
    ///     Gets or sets the footer content of the sidebar.
    /// </summary>
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="ItemIconContentSpacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemIconContentSpacingProperty =
        AvaloniaProperty.Register<Sidebar, double>(
            nameof(ItemIconContentSpacing));

    /// <summary>
    ///     Gets or sets the spacing between icon and content in sidebar items.
    /// </summary>
    public double ItemIconContentSpacing
    {
        get => GetValue(ItemIconContentSpacingProperty);
        set => SetValue(ItemIconContentSpacingProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="DefaultItemsSharedSizeGroup" /> property.
    /// </summary>
    public static readonly StyledProperty<string> DefaultItemsSharedSizeGroupProperty =
        AvaloniaProperty.Register<Sidebar, string>(
            nameof(DefaultItemsSharedSizeGroup));

    /// <summary>
    ///     Gets the default item SharedSizeGroup name for the sidebar.
    /// </summary>
    public string DefaultItemsSharedSizeGroup
    {
        get => GetValue(DefaultItemsSharedSizeGroupProperty);
        private set => SetValue(DefaultItemsSharedSizeGroupProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="DefaultItemsGroup" /> property.
    /// </summary>
    public static readonly StyledProperty<string> DefaultItemsGroupProperty =
        AvaloniaProperty.Register<Sidebar, string>(
            nameof(DefaultItemsGroup));

    /// <summary>
    ///     Gets the default item group name for the sidebar.
    /// </summary>
    public string DefaultItemsGroup
    {
        get => GetValue(DefaultItemsGroupProperty);
        private set => SetValue(DefaultItemsGroupProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="ExpandPaneWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ExpandPaneWidthProperty = AvaloniaProperty.Register<Sidebar, double>(
        nameof(ExpandPaneWidth), 180);

    /// <summary>
    ///     Gets or sets the width of the sidebar when expanded.
    /// </summary>
    public double ExpandPaneWidth
    {
        get => GetValue(ExpandPaneWidthProperty);
        set => SetValue(ExpandPaneWidthProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="CollapseEasing" /> property.
    /// </summary>
    public static readonly StyledProperty<Easing> CollapseEasingProperty = AvaloniaProperty.Register<Sidebar, Easing>(
        nameof(CollapseEasing), new EaseOut());

    /// <summary>
    ///     Gets or sets the easing function used for the collapse animation of the sidebar.
    /// </summary>
    public Easing CollapseEasing
    {
        get => GetValue(CollapseEasingProperty);
        set => SetValue(CollapseEasingProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="CollapseAnimationDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<double> CollapseAnimationDurationProperty =
        AvaloniaProperty.Register<Sidebar, double>(
            nameof(CollapseAnimationDuration), 200);

    /// <summary>
    ///     Gets or sets the duration of the collapse animation in milliseconds.
    /// </summary>
    public double CollapseAnimationDuration
    {
        get => GetValue(CollapseAnimationDurationProperty);
        set => SetValue(CollapseAnimationDurationProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="SelectedSidebarItem" /> property.
    /// </summary>
    public static readonly StyledProperty<SidebarItem?> SelectedSidebarItemProperty = AvaloniaProperty.Register<Sidebar, SidebarItem?>(
        nameof(SelectedSidebarItem));

    /// <summary>
    ///     Gets or sets the currently selected sidebar item.
    /// </summary>
    public SidebarItem? SelectedSidebarItem
    {
        get => GetValue(SelectedSidebarItemProperty);
        set => SetValue(SelectedSidebarItemProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="CurrentRoute" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CurrentRouteProperty = AvaloniaProperty.Register<Sidebar, object?>(
        nameof(CurrentRoute));

    /// <summary>
    ///     Gets or sets the current route or navigation path for the sidebar.
    ///     This property enables navigation-aware sidebar behavior by allowing the control
    ///     to track the current application route. It's particularly useful for:
    ///     <list type="bullet">
    ///         <item><description>Highlighting the currently active navigation item</description></item>
    ///         <item><description>Implementing route-based sidebar state management</description></item>
    ///         <item><description>Enabling automatic sidebar item selection based on the current page</description></item>
    ///         <item><description>Supporting deep linking scenarios where the sidebar should reflect the current route</description></item>
    ///     </list>
    /// </summary>
    public object? CurrentRoute
    {
        get => GetValue(CurrentRouteProperty);
        set => SetValue(CurrentRouteProperty, value);
    }
    
    /// <summary>
    ///     Called when the template is applied to the control.
    /// </summary>
    /// <param name="e">The template applied event arguments.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        DefaultItemsSharedSizeGroup = $"Shared{Guid.NewGuid():N}";
        DefaultItemsGroup = $"Group{Guid.NewGuid():N}";
    }

    /// <summary>
    ///     Called when a property value changes.
    /// </summary>
    /// <param name="change">The property change event arguments containing information about the changed property.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ExpandedProperty)
        {
            var expanded = change.GetNewValue<bool>();
            if (expanded)
            {
                Width = ExpandPaneWidth;
                if (MinWidth == 0) Opacity = 1.0;
            }
            else
            {
                Width = MinWidth;
                if (MinWidth == 0) Opacity = 0d;
            }
        }

        if (change.Property == SelectedSidebarItemProperty)
        {
            if (change.OldValue is SidebarItem oldItem) oldItem.IsChecked = false;
            if (change.NewValue is SidebarItem newItem)
            {
                newItem.IsChecked = true;
                CurrentRoute = newItem.Route;
            }
            else
            {
                CurrentRoute = null;
            }
        }
    }
}