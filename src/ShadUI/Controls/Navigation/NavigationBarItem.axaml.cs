using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Layout;
using Avalonia.VisualTree;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Represents a selectable item within a navigation bar control.
/// </summary>
[TemplatePart("PART_BorderContainer", typeof(Border))]
[TemplatePart("PART_ContentPresenter", typeof(ContentPresenter))]
[PseudoClasses(":expanded", ":vertical", ":horizontal")]
public class NavigationBarItem : RadioButton
{
    /// <summary>
    ///     Icon property.
    /// </summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<NavigationBarItem, object?>(nameof(Icon));

    /// <summary>
    ///     Gets or sets the icon of the menu item.
    /// </summary>
    public object? Icon
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
    ///     Defines the <see cref="IsExpanded" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsExpandedProperty =
        NavigationBar.IsExpandedProperty.AddOwner<NavigationBarItem>();

    /// <summary>
    ///     Gets or sets a value indicating whether the navigation bar item is expanded.
    /// </summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly StyledProperty<Orientation> OrientationProperty =
        NavigationBar.OrientationProperty.AddOwner<NavigationBarItem>();

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="Spacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty =
        StackPanel.SpacingProperty.AddOwner<NavigationBarItem>();

    /// <summary>
    ///     Gets or sets the spacing between elements in the navigation bar item.
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
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

    public NavigationBarItem()
    {
        UpdatePseudoClasses();
    }

    public NavigationBarItem(object? route) : this()
    {
        Route = route;
    }

    /// <summary>
    ///     Called when a property value changes.
    /// </summary>
    /// <param name="change">The property change event arguments.</param>
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

        if (change.Property == IsCheckedProperty &&
            change.NewValue is true &&
            this.GetVisualAncestors().OfType<NavigationBar>().FirstOrDefault() is { } navigationBar)
        {
            navigationBar.SelectedItem = this;
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