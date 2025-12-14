using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.VisualTree;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Represents a selectable item within a sidebar control.
/// </summary>
[TemplatePart("PART_BorderContainer", typeof(Border))]
[TemplatePart("PART_ContentPresenter", typeof(ContentPresenter))]
public class SidebarItem : RadioButton
{
    /// <summary>
    ///     Icon property.
    /// </summary>
    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<SidebarItem, object?>(nameof(Icon));

    /// <summary>
    ///     Gets or sets the icon of the menu item.
    /// </summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="Expanded" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ExpandedProperty = AvaloniaProperty.Register<SidebarItem, bool>(
        nameof(Expanded), true);

    /// <summary>
    ///     Gets or sets a value indicating whether the sidebar item is expanded.
    /// </summary>
    public bool Expanded
    {
        get => GetValue(ExpandedProperty);
        set => SetValue(ExpandedProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="Spacing" /> property.
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty = AvaloniaProperty.Register<SidebarItem, double>(
        nameof(Spacing));

    /// <summary>
    ///     Gets or sets the spacing between elements in the sidebar item.
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="HasIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> HasIconProperty = AvaloniaProperty.Register<SidebarItem, bool>(
        nameof(HasIcon));

    /// <summary>
    ///     Gets a value indicating whether the sidebar item has an icon.
    /// </summary>
    public bool HasIcon
    {
        get => GetValue(HasIconProperty);
        private set => SetValue(HasIconProperty, value);
    }

    /// <summary>
    ///     Defines the <see cref="Route" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> RouteProperty = AvaloniaProperty.Register<SidebarItem, object?>(
        nameof(Route));

    /// <summary>
    ///     Gets or sets the route or navigation path associated with this sidebar item.
    ///     This property is useful for:
    ///     <list type="bullet">
    ///         <item><description>Navigation-aware sidebar behavior where items can be highlighted based on the current route</description></item>
    ///         <item><description>Implementing automatic selection of sidebar items when navigating to specific pages</description></item>
    ///         <item><description>Enabling deep linking scenarios where the sidebar reflects the current application state</description></item>
    ///         <item><description>Facilitating route-based sidebar item activation and deactivation</description></item>
    ///     </list>
    /// </summary>
    public object? Route
    {
        get => GetValue(RouteProperty);
        set => SetValue(RouteProperty, value);
    }

    /// <summary>
    ///     Called when a property value changes.
    /// </summary>
    /// <param name="change">The property change event arguments.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ExpandedProperty)
        {
            UpdateToolTip();
        }

        if (change.Property == ContentProperty)
        {
            UpdateToolTip();
        }

        if (change.Property == IconProperty)
        {
            HasIcon = Icon != null;
        }

        if (change.Property == IsCheckedProperty &&
            change.NewValue is true &&
            this.GetVisualAncestors().OfType<Sidebar>().FirstOrDefault() is { } sidebar)
        {
            sidebar.SelectedSidebarItem = this;
        }
    }

    private void UpdateToolTip()
    {
        if (Expanded || Content is null)
        {
            ToolTip.SetTip(this, null);
            return;
        }

        if (Content is Visual)
        {
            if (Content is TextBlock tb)
            {
                ToolTip.SetTip(this, tb.Text);
            }
            else
            {
                ToolTip.SetTip(this, null);
            }
        }
        else
        {
            ToolTip.SetTip(this, Content);
        }
    }
}