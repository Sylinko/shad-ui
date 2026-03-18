using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Represents a navigation bar control that can be expanded or collapsed.
/// </summary>
[PseudoClasses(":expanded", ":vertical", ":horizontal")]
public class NavigationBar : ItemsControl
{
    /// <summary>
    ///     Defines the <see cref="IsExpanded" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<NavigationBar, bool>(nameof(IsExpanded), true);

    /// <summary>
    ///     Gets or sets a value indicating whether the navigation bar is expanded.
    /// </summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="Orientation" /> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<NavigationBar>();

    /// <summary>
    ///     Gets or sets the orientation of the navigation bar.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="HorizontalSpacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> HorizontalSpacingProperty =
        AvaloniaProperty.Register<NavigationBar, double>(nameof(HorizontalSpacing));

    /// <summary>
    ///     Gets or sets the horizontal spacing between navigation bar items.
    /// </summary>
    public double HorizontalSpacing
    {
        get => GetValue(HorizontalSpacingProperty);
        set => SetValue(HorizontalSpacingProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="VerticalSpacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> VerticalSpacingProperty =
        AvaloniaProperty.Register<NavigationBar, double>(nameof(VerticalSpacing));

    /// <summary>
    ///     Gets or sets the vertical spacing between navigation bar items.
    /// </summary>
    public double VerticalSpacing
    {
        get => GetValue(VerticalSpacingProperty);
        set => SetValue(VerticalSpacingProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="Footer" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<NavigationBar, object?>(nameof(Footer));

    /// <summary>
    ///     Gets or sets the footer content of the navigation bar.
    /// </summary>
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="DefaultItemsSharedSizeGroup" /> property.
    /// </summary>
    public static readonly StyledProperty<string> DefaultItemsSharedSizeGroupProperty =
        AvaloniaProperty.Register<NavigationBar, string>(nameof(DefaultItemsSharedSizeGroup));

    /// <summary>
    ///     Gets the default item SharedSizeGroup name for the navigation bar.
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
        AvaloniaProperty.Register<NavigationBar, string>(
            nameof(DefaultItemsGroup));

    /// <summary>
    ///     Gets the default item group name for the navigation bar.
    /// </summary>
    public string DefaultItemsGroup
    {
        get => GetValue(DefaultItemsGroupProperty);
        private set => SetValue(DefaultItemsGroupProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="SelectedItem" /> property.
    /// </summary>
    public static readonly StyledProperty<NavigationBarItem?> SelectedItemProperty =
        AvaloniaProperty.Register<NavigationBar, NavigationBarItem?>(nameof(SelectedItem));

    /// <summary>
    ///     Gets or sets the currently selected navigation bar item.
    /// </summary>
    public NavigationBarItem? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="CurrentRoute" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CurrentRouteProperty =
        AvaloniaProperty.Register<NavigationBar, object?>(nameof(CurrentRoute));

    /// <summary>
    ///     Gets or sets the current route or navigation path for the navigation bar.
    ///     This property enables navigation-aware navigation bar behavior by allowing the control
    ///     to track the current application route. It's particularly useful for:
    ///     <list type="bullet">
    ///         <item><description>Highlighting the currently active navigation item</description></item>
    ///         <item><description>Implementing route-based navigation bar state management</description></item>
    ///         <item><description>Enabling automatic navigation bar item selection based on the current page</description></item>
    ///         <item><description>Supporting deep linking scenarios where the navigation bar should reflect the current route</description></item>
    ///     </list>
    /// </summary>
    public object? CurrentRoute
    {
        get => GetValue(CurrentRouteProperty);
        set => SetValue(CurrentRouteProperty, value);
    }

    public NavigationBar()
    {
        UpdatePseudoClasses();
    }

    private static int _groupIndex;

    /// <summary>
    ///     Called when the template is applied to the control.
    /// </summary>
    /// <param name="e">The template applied event arguments.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        DefaultItemsSharedSizeGroup = $"Shared{_groupIndex}";
        DefaultItemsGroup = $"Group{_groupIndex++}";
    }

    /// <summary>
    ///     Called when a property value changes.
    /// </summary>
    /// <param name="change">The property change event arguments containing information about the changed property.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsExpandedProperty)
        {
            var isExpanded = change.GetNewValue<bool>();
            PseudoClasses.Set(":expanded", isExpanded);
        }

        if (change.Property == OrientationProperty)
        {
            var orientation = change.GetNewValue<Orientation>();
            PseudoClasses.Set(":vertical", orientation == Orientation.Vertical);
            PseudoClasses.Set(":horizontal", orientation == Orientation.Horizontal);
        }

        if (change.Property == SelectedItemProperty)
        {
            if (change.OldValue is NavigationBarItem oldItem) oldItem.IsChecked = false;
            if (change.NewValue is NavigationBarItem newItem)
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

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<NavigationBarItem>(item, out recycleKey);
    }

    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
    {
        return new NavigationBarItem(item);
    }

    internal void RearrangeChildrenIndex()
    {
        var i = 0;
        foreach (var item in Items.OfType<NavigationBarItem>())
        {
            item.Index = i++;
            foreach (var child in item.Children)
            {
                child.Index = i++;
            }
        }
    }

    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":expanded", IsExpanded);
        var orientation = Orientation;
        PseudoClasses.Set(":vertical", orientation == Orientation.Vertical);
        PseudoClasses.Set(":horizontal", orientation == Orientation.Horizontal);
    }
}