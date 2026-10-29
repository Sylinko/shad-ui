using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
/// Requests a state change without optimistically toggling its checked state.
/// The command owns asynchronous completion and errors; the binding source confirms IsChecked.
/// </summary>
/// <remarks>
/// CommandParameter is maintained by this control as the opposite of the confirmed state.
/// Bind IsBusy to the command's running state to block repeated activation.
/// </remarks>
public class AsyncToggleButton : ToggleButton
{
    /// <summary>
    /// Defines whether a state change is in progress.
    /// </summary>
    public static readonly StyledProperty<bool> IsBusyProperty =
        AvaloniaProperty.Register<AsyncToggleButton, bool>(nameof(IsBusy));

    /// <summary>
    /// Gets or sets whether interaction is blocked without changing the caller's IsEnabled value.
    /// </summary>
    public bool IsBusy
    {
        get => GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ToggleButton);

    /// <inheritdoc />
    protected override bool IsEnabledCore => base.IsEnabledCore && !IsBusy;

    static AsyncToggleButton()
    {
        IsCheckedProperty.OverrideMetadata<AsyncToggleButton>(new StyledPropertyMetadata<bool?>(false, BindingMode.OneWay));
    }

    /// <summary>
    /// Creates a button that requests the opposite of its confirmed state.
    /// </summary>
    public AsyncToggleButton()
    {
        SetCurrentValue(CommandParameterProperty, true);
    }

    /// <inheritdoc />
    protected override void Toggle()
    {
        // The command requests a change; only the binding source acknowledges it.
    }

    /// <inheritdoc />
    protected override void OnClick()
    {
        if (IsEffectivelyEnabled && Command is not null)
        {
            base.OnClick();
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsCheckedProperty)
        {
            SetCurrentValue(CommandParameterProperty, IsChecked != true);
        }
        else if (change.Property == IsBusyProperty)
        {
            UpdateIsEffectivelyEnabled();
        }
    }
}