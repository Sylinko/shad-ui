using Avalonia.Controls;
using Avalonia.Threading;
using ShadUI.Extensions;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Static router for displaying dialogs without requiring a reference to a particular
///     <see cref="DialogHost"/>.
/// </summary>
/// <remarks>
///     Hosts register themselves while attached to a TopLevel. Global builders resolve their host
///     only when shown, whereas builders created by <see cref="DialogHost"/> remain pinned to that
///     host. All dialog state is owned by the selected host.
/// </remarks>
public static class DialogManager
{
    private static readonly TopLevelHostRegistry<DialogHost> Hosts = new();

    /// <summary>Registers an attached host with the global router.</summary>
    internal static void Register(DialogHost host) => Hosts.Register(host);

    /// <summary>Unregisters a detached host from the global router.</summary>
    internal static void Unregister(DialogHost host) => Hosts.Unregister(host);

    /// <summary>
    ///     Resolves the host belonging to <paramref name="preferredTopLevel"/> when available,
    ///     otherwise the active, visible, or first registered host in that order.
    /// </summary>
    /// <remarks>This method must be called on the UI thread.</remarks>
    public static DialogHost? ResolveHost(TopLevel? preferredTopLevel = null) =>
        Hosts.Resolve(preferredTopLevel);

    /// <summary>Creates a simple dialog whose host is resolved when it is shown.</summary>
    public static SimpleDialogBuilder CreateDialog(object content, object? title = null) =>
        new(null, content, title);

    /// <summary>Creates a custom dialog whose host is resolved when it is shown.</summary>
    public static CustomDialogBuilder CreateCustomDialog(Control control) => new(null, control);

    /// <summary>Closes all dialogs on every registered host.</summary>
    public static void CloseAll(DialogResult result = DialogResult.Cancel)
    {
        void CloseCore()
        {
            foreach (var host in Hosts.Hosts.ToArray()) host.CloseAll(result);
        }

        if (Dispatcher.UIThread.CheckAccess()) CloseCore();
        else Dispatcher.UIThread.Post(CloseCore);
    }

    /// <summary>
    ///     Shows a dialog through an explicit host or the globally resolved host. Cancellation and
    ///     host lookup are both handled here so every builder has identical completion semantics.
    /// </summary>
    internal static void Show(
        DialogHost? explicitHost,
        Control control,
        Action<DialogResult>? callback,
        DialogOptions options,
        CancellationToken cancellationToken)
    {
        void ShowCore()
        {
            if (cancellationToken.IsCancellationRequested)
            {
                callback?.Invoke(DialogResult.Cancel);
                return;
            }

            var host = explicitHost ?? ResolveHost();
            if (host is null)
            {
                callback?.Invoke(DialogResult.HostNotFound);
                return;
            }

            CancellationTokenRegistration cancellationRegistration = default;

            void Complete(DialogResult result)
            {
                // ReSharper disable once AccessToModifiedClosure
                cancellationRegistration.Dispose();
                callback?.Invoke(result);
            }

            if (!host.Show(control, Complete, options))
            {
                Complete(DialogResult.Cancel);
                return;
            }

            if (cancellationToken.CanBeCanceled)
            {
                cancellationRegistration = cancellationToken.Register(() => Dispatcher.UIThread.PostOnDemand(() => host.Close(control)));
            }
        }

        Dispatcher.UIThread.PostOnDemand(ShowCore);
    }

    /// <summary>Shows a dialog and completes with a deterministic result even if no host exists.</summary>
    internal static Task<DialogResult> ShowAsync(
        DialogHost? explicitHost,
        Control control,
        DialogOptions options,
        CancellationToken cancellationToken)
    {
        var completion = new TaskCompletionSource<DialogResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        Show(
            explicitHost,
            control,
            result => completion.TrySetResult(result),
            options,
            cancellationToken);
        return completion.Task;
    }
}