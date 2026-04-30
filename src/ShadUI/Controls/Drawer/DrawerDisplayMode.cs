namespace ShadUI;

/// <summary>
/// Specifies how the drawer content is displayed relative to the main content.
/// </summary>
public enum DrawerDisplayMode
{
    /// <summary>
    /// The drawer pushes the main content aside, sharing the available space.
    /// The drawer occupies its own region in the layout alongside the main content.
    /// </summary>
    Inline,

    /// <summary>
    /// The drawer overlays on top of the main content.
    /// When <see cref="Drawer.OverlayBrush"/> is set, a semi-transparent backdrop is rendered
    /// behind the drawer. Clicking the backdrop will close the drawer.
    /// When <see cref="Drawer.OverlayBrush"/> is <c>null</c>, the backdrop is not rendered
    /// and pointer events pass through to the underlying content.
    /// </summary>
    Overlay
}
