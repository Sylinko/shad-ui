using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace ShadUI;

/// <summary>
/// A responsive drawer that automatically adjusts its <see cref="Drawer.Placement"/>
/// based on the available width. When the width exceeds <see cref="ResponsiveWidth"/>,
/// the drawer uses <see cref="HorizontalPlacement"/>; otherwise, it uses <see cref="VerticalPlacement"/>.
/// </summary>
/// <remarks>
/// <para>
/// This control inherits all functionality from <see cref="Drawer"/>, including
/// <see cref="Drawer.DisplayMode"/>, <see cref="Drawer.OverlayBrush"/>,
/// <see cref="Drawer.SplitterTemplate"/>, and all size constraints.
/// </para>
/// <para>
/// The responsive behavior only changes the <see cref="Drawer.Placement"/> property.
/// All other drawer properties remain available and can be customized as usual.
/// </para>
/// <para>
/// The <c>:wide</c> pseudo-class is active when the available width exceeds
/// <see cref="ResponsiveWidth"/>; otherwise, <c>:narrow</c> is active.
/// These states follow the width threshold, independently of the configured placements.
/// </para>
/// </remarks>
[PseudoClasses(":narrow", ":wide")]
public sealed class ResponsiveDrawer : Drawer
{
    /// <summary>
    /// Defines the <see cref="ResponsiveWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ResponsiveWidthProperty =
        AvaloniaProperty.Register<ResponsiveDrawer, double>(nameof(ResponsiveWidth), 800d);

    /// <summary>
    /// Gets or sets the width threshold at which the drawer switches between
    /// horizontal and vertical placement.
    /// When the available width is greater than this value, the drawer uses
    /// <see cref="HorizontalPlacement"/>; otherwise, it uses <see cref="VerticalPlacement"/>.
    /// </summary>
    public double ResponsiveWidth
    {
        get => GetValue(ResponsiveWidthProperty);
        set => SetValue(ResponsiveWidthProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="HorizontalPlacement"/> property.
    /// </summary>
    public static readonly StyledProperty<DrawerPlacement> HorizontalPlacementProperty =
        AvaloniaProperty.Register<ResponsiveDrawer, DrawerPlacement>(
            nameof(HorizontalPlacement), DrawerPlacement.Right);

    /// <summary>
    /// Gets or sets the drawer placement used when the available width exceeds
    /// <see cref="ResponsiveWidth"/>. Defaults to <see cref="DrawerPlacement.Right"/>.
    /// </summary>
    public DrawerPlacement HorizontalPlacement
    {
        get => GetValue(HorizontalPlacementProperty);
        set => SetValue(HorizontalPlacementProperty, value);
    }

    /// <summary>
    /// Defines the <see cref="VerticalPlacement"/> property.
    /// </summary>
    public static readonly StyledProperty<DrawerPlacement> VerticalPlacementProperty =
        AvaloniaProperty.Register<ResponsiveDrawer, DrawerPlacement>(
            nameof(VerticalPlacement), DrawerPlacement.Bottom);

    /// <summary>
    /// Gets or sets the drawer placement used when the available width is less than
    /// or equal to <see cref="ResponsiveWidth"/>. Defaults to <see cref="DrawerPlacement.Bottom"/>.
    /// </summary>
    public DrawerPlacement VerticalPlacement
    {
        get => GetValue(VerticalPlacementProperty);
        set => SetValue(VerticalPlacementProperty, value);
    }

    /// <summary>
    /// Initializes the responsive state for the drawer's initial bounds.
    /// </summary>
    public ResponsiveDrawer()
    {
        UpdatePlacement(Bounds.Width);
    }

    /// <inheritdoc />
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdatePlacement(e.NewSize.Width);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        switch (change.Property.Name)
        {
            case nameof(ResponsiveWidth):
            case nameof(HorizontalPlacement):
            case nameof(VerticalPlacement):
                UpdatePlacement(Bounds.Width);
                break;
        }
    }

    private void UpdatePlacement(double width)
    {
        var isWide = width > ResponsiveWidth;
        PseudoClasses.Set(":narrow", !isWide);
        PseudoClasses.Set(":wide", isWide);

        var newPlacement = isWide
            ? HorizontalPlacement
            : VerticalPlacement;

        if (Placement != newPlacement)
        {
            Placement = newPlacement;
        }
    }
}