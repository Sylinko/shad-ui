using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace ShadUI;

public class NavigationBarItemButton : RadioButton
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<NavigationBarItemButton, Orientation>(nameof(Orientation));

    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<NavigationBarItemButton, bool>(nameof(IsExpanded));

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly StyledProperty<bool> IsChildrenCheckedProperty =
        AvaloniaProperty.Register<NavigationBarItemButton, bool>(nameof(IsChildrenChecked));

    public bool IsChildrenChecked
    {
        get => GetValue(IsChildrenCheckedProperty);
        set => SetValue(IsChildrenCheckedProperty, value);
    }
}