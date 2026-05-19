using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Static router for displaying <see cref="Toast" /> notifications from anywhere,
///     without requiring a reference to a specific <see cref="ToastHost" />.
/// </summary>
/// <remarks>
///     <para>
///         <see cref="ToastHost" /> instances automatically register themselves when attached
///         to the visual tree and unregister when detached. When a toast is shown,
///         the router selects the best available host (active window &gt; visible window &gt; any host).
///     </para>
///     <para>
///         Thread-safe: calling <see cref="Create" /> from a background thread is valid —
///         the thread switch to the UI dispatcher happens inside <see cref="ToastBuilder.Show" />.
///     </para>
/// </remarks>
public static class ToastManager
{
    /// <summary>
    ///     Maps owning <see cref="TopLevel" /> → <see cref="ToastHost" />, populated on attach.
    ///     When a window closes and the host detaches, the entry is removed.
    /// </summary>
    private static readonly Dictionary<TopLevel, ToastHost> TopLevelToHost = [];

    #region Internal — Registration (called by ToastHost)

    /// <summary>
    ///     Registers a <see cref="ToastHost" /> so it can receive toast requests.
    ///     Called automatically by <see cref="ToastHost.OnAttachedToVisualTree" />.
    /// </summary>
    internal static void Register(ToastHost host)
    {
        Dispatcher.UIThread.CheckAccess();

        var topLevel = TopLevel.GetTopLevel(host);
        if (topLevel is null) return;

        TopLevelToHost[topLevel] = host;
    }

    /// <summary>
    ///     Unregisters a <see cref="ToastHost" />.
    ///     Called automatically by <see cref="ToastHost.OnDetachedFromVisualTree" />.
    /// </summary>
    internal static void Unregister(ToastHost host)
    {
        Dispatcher.UIThread.CheckAccess();

        foreach (var (topLevel, existing) in TopLevelToHost)
        {
            if (ReferenceEquals(existing, host))
            {
                TopLevelToHost.Remove(topLevel);
                return;
            }
        }
    }

    /// <summary>
    ///     Resolves the best available <see cref="ToastHost" />.
    ///     Must be called on the UI thread.
    /// </summary>
    /// <returns>The best host, or <c>null</c> if none is registered.</returns>
    internal static ToastHost? ResolveHost()
    {
        Dispatcher.UIThread.CheckAccess();

        switch (Application.Current?.ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
            {
                // Active window
                foreach (var window in desktop.Windows)
                {
                    if (window.IsActive && TopLevelToHost.TryGetValue(window, out var host))
                        return host;
                }

                // Any visible window
                foreach (var window in desktop.Windows)
                {
                    if (window.IsVisible && TopLevelToHost.TryGetValue(window, out var host))
                        return host;
                }

                break;
            }
            case ISingleViewApplicationLifetime singleView:
            {
                if (singleView.MainView is TopLevel topLevel)
                {
                    if (TopLevelToHost.TryGetValue(topLevel, out var host))
                        return host;
                }

                break;
            }
        }

        // Fallback: any registered host (e.g. embedded TopLevel not tracked by Application)
        return TopLevelToHost.Values.FirstOrDefault();
    }

    #endregion

    #region Public API

    /// <summary>
    ///     Creates a <see cref="ToastBuilder" /> for building and showing a toast.
    ///     The host is resolved lazily when <see cref="ToastBuilder.Show" /> is called.
    /// </summary>
    /// <param name="title">The title text of the toast.</param>
    /// <returns>A fluent <see cref="ToastBuilder" /> instance.</returns>
    public static ToastBuilder Create(string title) => new(null, title);

    /// <summary>
    ///     Shows a success-styled toast notification with default 5s duration.
    /// </summary>
    /// <param name="title">The title text.</param>
    /// <param name="content">Optional body content.</param>
    /// <param name="dismissOnClick"></param>
    /// <param name="position"></param>
    /// <param name="durationSeconds"></param>
    public static void Success(
        string title,
        object? content = null,
        bool dismissOnClick = true,
        ToastPosition position = ToastPosition.BottomRight,
        double durationSeconds = 5d)
    {
        var builder = Create(title);
        if (content is not null) builder = builder.WithContent(content);
        if (dismissOnClick) builder.DismissOnClick();
        builder.WithPosition(position).WithDurationSeconds(durationSeconds).ShowSuccess();
    }

    /// <summary>
    ///     Shows a warning-styled toast notification.
    /// </summary>
    /// <param name="title">The title text.</param>
    /// <param name="content">Optional body content.</param>
    /// <param name="dismissOnClick"></param>
    /// <param name="position"></param>
    /// <param name="durationSeconds"></param>
    public static void Warning(
        string title,
        object? content = null,
        bool dismissOnClick = true,
        ToastPosition position = ToastPosition.BottomRight,
        double durationSeconds = 0d)
    {
        var builder = Create(title);
        if (content is not null) builder = builder.WithContent(content);
        if (dismissOnClick) builder.DismissOnClick();
        builder.WithPosition(position).WithDurationSeconds(durationSeconds).ShowWarning();
    }

    /// <summary>
    ///     Shows an error-styled toast notification.
    /// </summary>
    /// <param name="title">The title text.</param>
    /// <param name="message">Optional error message displayed as content.</param>
    /// <param name="dismissOnClick"></param>
    /// <param name="position"></param>
    /// <param name="durationSeconds"></param>
    public static void Error(
        string title,
        object? message = null,
        bool dismissOnClick = true,
        ToastPosition position = ToastPosition.BottomRight,
        double durationSeconds = 0d)
    {
        var builder = Create(title);
        if (message is not null) builder = builder.WithContent(message);
        if (dismissOnClick) builder.DismissOnClick();
        builder.WithPosition(position).WithDurationSeconds(durationSeconds).ShowError();
    }

    /// <summary>
    ///     Dismisses all toasts on all registered hosts.
    /// </summary>
    public static void DismissAll()
    {
        Dispatcher.UIThread.Post(() =>
        {
            foreach (var host in TopLevelToHost.Values) host.DismissAll();
        });
    }

    #endregion

}