using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.VisualTree;

namespace ShadUI;

/// <summary>
/// An attached property helper that can be used to set properties on the top-level window.
/// </summary>
public class TopLevelAssist
{
    /// <summary>
    /// An attached property that can be used to set the transparency level hint on a top-level window.
    /// </summary>
    public static readonly AttachedProperty<IReadOnlyList<WindowTransparencyLevel>?> TransparencyLevelHintProperty =
        AvaloniaProperty.RegisterAttached<TopLevelAssist, Visual, IReadOnlyList<WindowTransparencyLevel>?>("TransparencyLevelHint");

    /// <summary>
    /// Sets the transparency level hint on a top-level window.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public static void SetTransparencyLevelHint(Visual obj, IReadOnlyList<WindowTransparencyLevel>? value) =>
        obj.SetValue(TransparencyLevelHintProperty, value);

    /// <summary>
    /// Gets the transparency level hint on a top-level window.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static IReadOnlyList<WindowTransparencyLevel>? GetTransparencyLevelHint(Visual obj) =>
        obj.GetValue(TransparencyLevelHintProperty);

    /// <summary>
    /// An attached property that can be used to set the system decorations on a top-level window.
    /// </summary>
    public static readonly AttachedProperty<SystemDecorations?> SystemDecorationsProperty =
        AvaloniaProperty.RegisterAttached<TopLevelAssist, Visual, SystemDecorations?>("SystemDecorations");

    /// <summary>
    /// Sets the system decorations on a top-level window.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public static void SetSystemDecorations(Visual obj, SystemDecorations? value) =>
        obj.SetValue(SystemDecorationsProperty, value);

    /// <summary>
    /// Gets the system decorations on a top-level window.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static SystemDecorations? GetSystemDecorations(Visual obj) =>
        obj.GetValue(SystemDecorationsProperty);

    /// <summary>
    /// An attached property that can be used to set the ExtendClientAreaChromeHints on a top-level window.
    /// </summary>
    public static readonly AttachedProperty<ExtendClientAreaChromeHints?> ExtendClientAreaChromeHintsProperty =
        AvaloniaProperty.RegisterAttached<TopLevelAssist, Visual, ExtendClientAreaChromeHints?>("ExtendClientAreaChromeHints");

    /// <summary>
    /// Sets the ExtendClientAreaChromeHints on a top-level window.
    /// </summary>
    public static void SetExtendClientAreaChromeHints(Visual obj, ExtendClientAreaChromeHints? value) =>
        obj.SetValue(ExtendClientAreaChromeHintsProperty, value);

    /// <summary>
    /// Gets the ExtendClientAreaChromeHints on a top-level window.
    /// </summary>
    public static ExtendClientAreaChromeHints? GetExtendClientAreaChromeHints(Visual obj) =>
        obj.GetValue(ExtendClientAreaChromeHintsProperty);

    /// <summary>
    /// An attached property that can be used to set the ExtendClientAreaToDecorationsHint on a top-level window.
    /// </summary>
    public static readonly AttachedProperty<bool?> ExtendClientAreaToDecorationsHintProperty =
        AvaloniaProperty.RegisterAttached<TopLevelAssist, Visual, bool?>("ExtendClientAreaToDecorationsHint");

    /// <summary>
    /// Sets the ExtendClientAreaToDecorationsHint on a top-level window.
    /// </summary>
    public static void SetExtendClientAreaToDecorationsHint(Visual obj, bool? value) =>
        obj.SetValue(ExtendClientAreaToDecorationsHintProperty, value);

    /// <summary>
    /// Gets the ExtendClientAreaToDecorationsHint on a top-level window.
    /// </summary>
    public static bool? GetExtendClientAreaToDecorationsHint(Visual obj) =>
        obj.GetValue(ExtendClientAreaToDecorationsHintProperty);

    /// <summary>
    /// An attached property that can be used to set the CanResize on a top-level window.
    /// </summary>
    public static readonly AttachedProperty<bool?> CanResizeProperty =
        AvaloniaProperty.RegisterAttached<TopLevelAssist, Visual, bool?>("CanResize");

    /// <summary>
    /// Sets the CanResize on a top-level window.
    /// </summary>
    public static void SetCanResize(Visual obj, bool? value) =>
        obj.SetValue(CanResizeProperty, value);

    /// <summary>
    /// Gets the CanResize on a top-level window.
    /// </summary>
    public static bool? GetCanResize(Visual obj) =>
        obj.GetValue(CanResizeProperty);

    /// <summary>
    /// An attached property that can be used to set the Topmost on a top-level window.
    /// </summary>
    public static readonly AttachedProperty<bool?> TopmostProperty =
        AvaloniaProperty.RegisterAttached<TopLevelAssist, Visual, bool?>("Topmost");

    /// <summary>
    /// Sets the Topmost on a top-level window.
    /// </summary>
    public static void SetTopmost(Visual obj, bool? value) =>
        obj.SetValue(TopmostProperty, value);

    /// <summary>
    /// Gets the Topmost on a top-level window.
    /// </summary>
    public static bool? GetTopmost(Visual obj) =>
        obj.GetValue(TopmostProperty);

    static TopLevelAssist()
    {
        TransparencyLevelHintProperty.Changed.AddClassHandler<Visual>(HandleTransparencyLevelHintChanged);
        SystemDecorationsProperty.Changed.AddClassHandler<Visual>(HandleSystemDecorationsChanged);
        ExtendClientAreaChromeHintsProperty.Changed.AddClassHandler<Visual>(HandleExtendClientAreaChromeHintsChanged);
        ExtendClientAreaToDecorationsHintProperty.Changed.AddClassHandler<Visual>(HandleExtendClientAreaToDecorationsHintChanged);
        CanResizeProperty.Changed.AddClassHandler<Visual>(HandleCanResizeChanged);
        TopmostProperty.Changed.AddClassHandler<Visual>(HandleTopmostChanged);
    }

    private static void HandleTransparencyLevelHintChanged(Visual sender, AvaloniaPropertyChangedEventArgs args)
    {
        ExecuteWhenAttached(sender, () =>
        {
            var topLevel = TopLevel.GetTopLevel(sender);
            if (topLevel is null) return;

            if (args.NewValue is IReadOnlyList<WindowTransparencyLevel> levels)
            {
                topLevel.TransparencyLevelHint = levels;
            }
        });
    }

    private static void HandleSystemDecorationsChanged(Visual sender, AvaloniaPropertyChangedEventArgs args)
    {
        ExecuteWhenAttached(sender, () =>
        {
            var topLevel = TopLevel.GetTopLevel(sender);
            if (topLevel is not Avalonia.Controls.Window window) return;

            if (args.NewValue is SystemDecorations decorations)
            {
                window.SystemDecorations = decorations;
            }
        });
    }

    private static void HandleExtendClientAreaChromeHintsChanged(Visual sender, AvaloniaPropertyChangedEventArgs args)
    {
        ExecuteWhenAttached(sender, () =>
        {
            var topLevel = TopLevel.GetTopLevel(sender);
            if (topLevel is not Avalonia.Controls.Window window) return;

            if (args.NewValue is ExtendClientAreaChromeHints hints)
            {
                window.ExtendClientAreaChromeHints = hints;
            }
        });
    }

    private static void HandleExtendClientAreaToDecorationsHintChanged(Visual sender, AvaloniaPropertyChangedEventArgs args)
    {
        ExecuteWhenAttached(sender, () =>
        {
            var topLevel = TopLevel.GetTopLevel(sender);
            if (topLevel is not Avalonia.Controls.Window window) return;

            if (args.NewValue is bool value)
            {
                window.ExtendClientAreaToDecorationsHint = value;
            }
        });
    }

    private static void HandleCanResizeChanged(Visual sender, AvaloniaPropertyChangedEventArgs args)
    {
        ExecuteWhenAttached(sender, () =>
        {
            var topLevel = TopLevel.GetTopLevel(sender);
            if (topLevel is not Avalonia.Controls.Window window) return;

            if (args.NewValue is bool value)
            {
                window.CanResize = value;
            }
        });
    }

    private static void HandleTopmostChanged(Visual sender, AvaloniaPropertyChangedEventArgs args)
    {
        ExecuteWhenAttached(sender, () =>
        {
            var topLevel = TopLevel.GetTopLevel(sender);
            if (topLevel is not WindowBase window) return;

            if (args.NewValue is bool value)
            {
                window.Topmost = value;
            }
        });
    }

    private static void ExecuteWhenAttached(Visual sender, Action action)
    {
        if (sender.IsAttachedToVisualTree())
        {
            action();
        }
        else
        {
            sender.AttachedToVisualTree += HandleAttachedToVisualTree;

            void HandleAttachedToVisualTree(object? o, VisualTreeAttachmentEventArgs e)
            {
                sender.AttachedToVisualTree -= HandleAttachedToVisualTree;
                action();
            }
        }
    }
}