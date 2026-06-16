using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;

namespace ShadUI;

/// <summary>
/// A behavior that swallows the <see cref="Control.RequestBringIntoViewEvent"/> event, preventing the control from being scrolled into view when it receives focus.
/// </summary>
public sealed class SwallowBringIntoViewEventBehavior : Behavior<Control>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        AssociatedObject?.AddHandler(
            Control.RequestBringIntoViewEvent,
            HandleRequestBringIntoView,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        AssociatedObject?.RemoveHandler(
            Control.RequestBringIntoViewEvent,
            HandleRequestBringIntoView);
    }

    private static void HandleRequestBringIntoView(object? sender, RequestBringIntoViewEventArgs e)
    {
        e.Handled = true;
    }
}