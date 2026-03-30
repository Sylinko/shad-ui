using Avalonia;
using Avalonia.Controls;
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
    public static readonly AttachedProperty<WindowDecorations?> WindowDecorationsProperty =
        AvaloniaProperty.RegisterAttached<TopLevelAssist, Visual, WindowDecorations?>("WindowDecorations");

    /// <summary>
    /// Sets the system decorations on a top-level window.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="value"></param>
    public static void SetWindowDecorations(Visual obj, WindowDecorations? value) =>
        obj.SetValue(WindowDecorationsProperty, value);

    /// <summary>
    /// Gets the system decorations on a top-level window.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static WindowDecorations? GetWindowDecorations(Visual obj) =>
        obj.GetValue(WindowDecorationsProperty);

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
        WindowDecorationsProperty.Changed.AddClassHandler<Visual>(HandleWindowDecorationsChanged);
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

    private static void HandleWindowDecorationsChanged(Visual sender, AvaloniaPropertyChangedEventArgs args)
    {
        ExecuteWhenAttached(sender, () =>
        {
            var topLevel = TopLevel.GetTopLevel(sender);
            if (topLevel is not Avalonia.Controls.Window window) return;

            if (args.NewValue is WindowDecorations hints)
            {
                window.WindowDecorations = hints;
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