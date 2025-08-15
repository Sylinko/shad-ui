using Avalonia.Controls;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Builds a dialog.
/// </summary>
public sealed class DialogBuilder
{
    private readonly DialogManager _manager;

    internal DialogBuilder(DialogManager manager)
    {
        _manager = manager;
    }

    internal Action? OnCancelCallback { get; set; }
    internal Func<Task>? OnCancelAsyncCallback { get; set; }
    internal Action? OnSuccessCallback { get; set; }
    internal Action<Control>? OnSuccessWithControlCallback { get; set; }
    internal Func<Task>? OnSuccessAsyncCallback { get; set; }
    internal Func<Control, Task>? OnSuccessWithControlAsyncCallback { get; set; }
    internal DialogOptions Options { get; } = new();

    private Control? _control;

    internal DialogBuilder CreateDialog(Control control)
    {
        _control = control;
        return this;
    }

    internal void Show()
    {
        if (_control == null) throw new InvalidOperationException("Dialog control is not set.");

        if (OnSuccessCallback != null)
        {
            _manager.OnSuccessCallbacks.TryAdd(_control, OnSuccessCallback);
        }

        if (OnSuccessWithControlCallback != null)
        {
            _manager.OnSuccessWithContextCallbacks.TryAdd(_control, OnSuccessWithControlCallback);
        }

        if (OnSuccessAsyncCallback != null)
        {
            _manager.OnSuccessAsyncCallbacks.TryAdd(_control, OnSuccessAsyncCallback);
        }

        if (OnSuccessWithControlAsyncCallback != null)
        {
            _manager.OnSuccessWithContextAsyncCallbacks.TryAdd(_control, OnSuccessWithControlAsyncCallback);
        }

        if (OnCancelCallback != null)
        {
            _manager.OnCancelCallbacks.TryAdd(_control, OnCancelCallback);
        }

        if (OnCancelAsyncCallback != null)
        {
            _manager.OnCancelAsyncCallbacks.TryAdd(_control, OnCancelAsyncCallback);
        }

        _manager.Show(_control, Options);
    }
}