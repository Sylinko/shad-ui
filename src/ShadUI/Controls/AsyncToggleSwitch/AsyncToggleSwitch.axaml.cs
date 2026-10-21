using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Threading;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
/// Requests an acknowledged state change through Command, passing the desired Boolean state.
/// IsChecked remains owned by its binding source; IsBusy blocks interaction without changing IsEnabled.
/// </summary>
/// <remarks>
/// CommandParameter is maintained by this control and should not be bound by callers.
/// The command owns completion and error presentation; the switch never awaits or commits a state change itself.
/// </remarks>
[PseudoClasses(":busy", ":pending", ":loading")]
public class AsyncToggleSwitch : ToggleButton
{
    /// <summary>
    /// Defines whether a state change is in progress.
    /// </summary>
    public static readonly StyledProperty<bool> IsBusyProperty =
        AvaloniaProperty.Register<AsyncToggleSwitch, bool>(nameof(IsBusy));

    /// <summary>
    /// Gets or sets whether interaction is blocked. Only the switch that requested the change shows waiting feedback.
    /// </summary>
    public bool IsBusy
    {
        get => GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    /// <inheritdoc />
    protected override bool IsEnabledCore => base.IsEnabledCore && !IsBusy;

    private IDisposable? _loadingDelay;

    static AsyncToggleSwitch()
    {
        IsCheckedProperty.OverrideMetadata<AsyncToggleSwitch>(new StyledPropertyMetadata<bool?>(false, BindingMode.OneWay));
    }

    /// <summary>
    /// Creates a switch whose command parameter is always the opposite of its confirmed state.
    /// </summary>
    public AsyncToggleSwitch()
    {
        SetCurrentValue(CommandParameterProperty, true);
    }

    /// <inheritdoc />
    protected override void Toggle()
    {
        // Button executes Command for pointer, keyboard and automation activation alike.
        // Do not optimistically toggle IsChecked: only the acknowledged binding may change it.
    }

    /// <inheritdoc />
    protected override void OnClick()
    {
        if (!IsEffectivelyEnabled || Command is null)
        {
            return;
        }

        // Several switches may share IsBusy. Only the initiating switch stretches and shows Loading;
        // its peers remain visually steady while the shared operation blocks interaction.
        PseudoClasses.Set(":pending", true);
        try
        {
            base.OnClick();
        }
        finally
        {
            if (!IsBusy)
            {
                PseudoClasses.Set(":pending", false);
            }
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
            PseudoClasses.Set(":busy", IsBusy);
            if (!IsBusy)
            {
                PseudoClasses.Set(":pending", false);
            }

            UpdateIsEffectivelyEnabled();
            UpdateLoading();
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateLoading();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _loadingDelay?.Dispose();
        _loadingDelay = null;
        PseudoClasses.Set(":pending", false);
        PseudoClasses.Set(":loading", false);
        base.OnDetachedFromVisualTree(e);
    }

    private void UpdateLoading()
    {
        _loadingDelay?.Dispose();
        _loadingDelay = null;
        PseudoClasses.Set(":loading", false);

        if (!IsBusy || !PseudoClasses.Contains(":pending") || VisualRoot is null)
        {
            return;
        }

        // This delay affects presentation only. Neither command execution nor confirmation waits for it.
        _loadingDelay = DispatcherTimer.RunOnce(
            () =>
            {
                _loadingDelay = null;
                PseudoClasses.Set(":loading", true);
            },
            TimeSpan.FromMilliseconds(200));
    }
}