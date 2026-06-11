using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Reactive;
using Avalonia.Xaml.Interactivity;

namespace ShadUI;

/// <summary>
/// Defines the FixedSelectedItem attached property for SelectingItemsControl.
/// When multi SelectingItemsControl bind to one SelectedItem,
/// select the second SelectingItemsControl will clear the 1st SelectingItemsControl's selection,
/// this will cause 2nd SelectingItemsControl's selection to be cleared immediately after being set, which is not expected.
/// Use this to fix the issue.
/// </summary>
public sealed class FixSelectedItemBehavior : StyledElementBehavior<SelectingItemsControl>
{
    public static readonly StyledProperty<object?> SelectedItemProperty =
        AvaloniaProperty.Register<FixSelectedItemBehavior, object?>(nameof(SelectedItem), defaultBindingMode: BindingMode.TwoWay);

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    private IDisposable? _subscription1, _subscription2;

    protected override void OnAttached()
    {
        base.OnAttached();
        _subscription1 = AssociatedObject?.GetObservable(SelectingItemsControl.SelectedItemProperty).Subscribe(new AnonymousObserver<object?>(x =>
        {
            if (x is not null) SelectedItem = x;
        }));
        _subscription2 = this.GetObservable(SelectedItemProperty).Subscribe(new AnonymousObserver<object?>(x =>
        {
            AssociatedObject?.SelectedItem = x;
        }));
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        _subscription1?.Dispose();
        _subscription2?.Dispose();
    }
}