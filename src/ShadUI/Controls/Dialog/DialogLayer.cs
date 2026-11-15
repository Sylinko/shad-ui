using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace ShadUI;

/// <summary>
/// The stable presentation of one dialog. Its mask fills the host, but blur and
/// scale belong only to its card, never to the layer containing other dialogs.
/// </summary>
public sealed class DialogLayer : ContentControl
{
    internal Control View { get; }
    internal DialogOptions Options { get; }
    internal IInputElement? Opener { get; set; }
    internal bool IsClosing { get; private set; }

    private readonly DialogHost _host;
    private readonly Action<DialogResult>? _callback;
    private IDisposable? _exitTimer;
    private Border? _mask;
    private Button? _closeButton;
    private SmoothBorder? _card;

    internal DialogLayer(DialogHost host, Control view, DialogOptions options, Action<DialogResult>? callback, IInputElement? opener)
    {
        _host = host;
        View = view;
        Options = new DialogOptions { MaxWidth = options.MaxWidth, MinWidth = options.MinWidth, Dismissible = options.Dismissible };
        Opener = opener;
        _callback = callback;
        Content = view;
    }

    /// <inheritdoc/>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _mask?.PointerPressed -= HandleMaskPressed;
        _closeButton?.Click -= HandleCloseClicked;

        base.OnApplyTemplate(e);
        _mask = e.NameScope.Get<Border>("PART_Mask");
        _closeButton = e.NameScope.Get<Button>("PART_CloseButton");
        _card = e.NameScope.Get<SmoothBorder>("DialogContent");

        ApplyOptions();
        _mask.PointerPressed += HandleMaskPressed;
        _closeButton.Click += HandleCloseClicked;
    }

    /// <inheritdoc/>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (IsClosing) return;
        Classes.Add("open");
        if (IsEffectivelyEnabled && !IsKeyboardFocusWithin) Focus();
    }

    internal void ApplyOptions()
    {
        if (_card is null || _closeButton is null) return;
        _card.MinWidth = Options.MinWidth;
        _card.MaxWidth = Options.MaxWidth;
        _closeButton.IsVisible = Options.Dismissible;
    }

    internal void SetCovered(bool isCovered)
    {
        Classes.Set("covered", isCovered);
        IsEnabled = !isCovered;
    }

    internal void StartExit(bool isLast, Action release)
    {
        IsClosing = true;
        IsEnabled = false;
        Classes.Set("last", isLast);
        Classes.Add("closing");
        Classes.Remove("open");
        _exitTimer = DispatcherTimer.RunOnce(release, TimeSpan.FromMilliseconds(160));
    }

    internal void Release()
    {
        _exitTimer?.Dispose();
        Content = null;
    }

    internal void Complete(DialogResult result) => _callback?.Invoke(result);

    private void HandleMaskPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_host.CanDismissWithBackgroundClick) _host.RequestDismiss(this);
        e.Handled = true;
    }

    private void HandleCloseClicked(object? sender, RoutedEventArgs e) => _host.RequestDismiss(this);
}