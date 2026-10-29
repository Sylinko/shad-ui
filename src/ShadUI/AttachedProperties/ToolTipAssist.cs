using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
/// Controls pointer hit testing for tooltip popup hosts.
/// </summary>
public static class ToolTipAssist
{
    /// <summary>
    /// Defines whether the tooltip's popup allows pointer input to pass through it.
    /// </summary>
    public static readonly AttachedProperty<bool> IsHitTestVisibleProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, bool>("IsHitTestVisible", typeof(ToolTipAssist), defaultValue: true);

    private static readonly AttachedProperty<PopupSubscription?> PopupSubscriptionProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, PopupSubscription?>("PopupSubscription", typeof(ToolTipAssist));

    static ToolTipAssist()
    {
        IsHitTestVisibleProperty.Changed.AddClassHandler<ToolTip>(HandleIsHitTestVisibleChanged);
    }

    /// <summary>
    /// Gets whether the tooltip's popup allows pointer input to pass through it.
    /// </summary>
    public static bool GetIsHitTestVisible(ToolTip toolTip) => toolTip.GetValue(IsHitTestVisibleProperty);

    /// <summary>
    /// Sets whether the tooltip's popup allows pointer input to pass through it.
    /// </summary>
    public static void SetIsHitTestVisible(ToolTip toolTip, bool isHitTestVisible)
    {
        toolTip.SetValue(IsHitTestVisibleProperty, isHitTestVisible);
    }

    private static void HandleIsHitTestVisibleChanged(ToolTip toolTip, AvaloniaPropertyChangedEventArgs args)
    {
        toolTip.AttachedToVisualTree -= HandleAttachedToVisualTree;
        toolTip.DetachedFromVisualTree -= HandleDetachedFromVisualTree;
        DetachPopup(toolTip);

        if (!GetIsHitTestVisible(toolTip)) return;

        toolTip.AttachedToVisualTree += HandleAttachedToVisualTree;
        toolTip.DetachedFromVisualTree += HandleDetachedFromVisualTree;

        if (toolTip.IsAttachedToVisualTree()) AttachPopup(toolTip);
    }

    private static void HandleAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs args)
    {
        if (sender is ToolTip toolTip) AttachPopup(toolTip);
    }

    private static void HandleDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs args)
    {
        if (sender is ToolTip toolTip) DetachPopup(toolTip);
    }

    private static void AttachPopup(ToolTip toolTip)
    {
        if (toolTip.GetValue(PopupSubscriptionProperty) is not null || toolTip.Parent is not Popup popup) return;

        toolTip.SetValue(PopupSubscriptionProperty, new PopupSubscription(toolTip, popup));
    }

    private static void DetachPopup(ToolTip toolTip)
    {
        toolTip.GetValue(PopupSubscriptionProperty)?.Dispose();
        toolTip.ClearValue(PopupSubscriptionProperty);
    }

    private sealed class PopupSubscription : IDisposable
    {
        private readonly ToolTip _toolTip;
        private readonly Popup _popup;
        private IDisposable? _hitTestBinding;

        public PopupSubscription(ToolTip toolTip, Popup popup)
        {
            _toolTip = toolTip;
            _popup = popup;
            _popup.Opened += HandlePopupOpened;
            _toolTip.Loaded += HandleLoaded;

            if (_toolTip.IsLoaded) BindHitTestVisibility();
        }

        public void Dispose()
        {
            _popup.Opened -= HandlePopupOpened;
            _toolTip.Loaded -= HandleLoaded;
            _hitTestBinding?.Dispose();
        }

        private void HandlePopupOpened(object? sender, EventArgs args) => BindHitTestVisibility();

        private void HandleLoaded(object? sender, RoutedEventArgs args) => BindHitTestVisibility();

        private void BindHitTestVisibility()
        {
            // Visual attachment happens before Popup finishes creating its host. Apply the binding
            // after Opened so the initial value reaches the native window as well as overlay hosts.
            // Loaded also covers enabling this behavior on a popup that has already opened.
            _hitTestBinding ??= _popup.Bind(InputElement.IsHitTestVisibleProperty, _toolTip.GetObservable(IsHitTestVisibleProperty));
        }
    }
}