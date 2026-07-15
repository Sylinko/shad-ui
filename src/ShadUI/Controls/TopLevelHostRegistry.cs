using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Tracks one overlay host per <see cref="TopLevel"/> and applies the shared window-selection
///     policy used by dialog and toast routing.
/// </summary>
/// <remarks>
///     This type deliberately abstracts only registration and host selection. Dialog stacks and
///     toast queues remain owned by their concrete hosts because their lifecycle semantics are
///     otherwise unrelated.
/// </remarks>
internal sealed class TopLevelHostRegistry<THost> where THost : Control
{
    private readonly Dictionary<TopLevel, THost> _topLevelToHost = [];

    /// <summary>Gets the currently registered hosts. The collection is UI-thread-affine.</summary>
    public IEnumerable<THost> Hosts => _topLevelToHost.Values;

    /// <summary>Registers a host against the TopLevel that currently contains it.</summary>
    public void Register(THost host)
    {
        Dispatcher.UIThread.CheckAccess();

        if (TopLevel.GetTopLevel(host) is { } topLevel)
            _topLevelToHost[topLevel] = host;
    }

    /// <summary>Removes a host without relying on its TopLevel still being discoverable.</summary>
    public void Unregister(THost host)
    {
        Dispatcher.UIThread.CheckAccess();

        TopLevel? owner = null;
        foreach (var pair in _topLevelToHost)
        {
            if (!ReferenceEquals(pair.Value, host)) continue;
            owner = pair.Key;
            break;
        }

        if (owner is not null) _topLevelToHost.Remove(owner);
    }

    /// <summary>
    ///     Resolves the preferred TopLevel first, followed by the active window, a visible window,
    ///     and finally any registered host.
    /// </summary>
    public THost? Resolve(TopLevel? preferredTopLevel = null)
    {
        Dispatcher.UIThread.CheckAccess();

        if (preferredTopLevel is not null && _topLevelToHost.TryGetValue(preferredTopLevel, out var preferredHost))
        {
            return preferredHost;
        }

        switch (Application.Current?.ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
            {
                foreach (var window in desktop.Windows)
                {
                    if (window.IsActive && _topLevelToHost.TryGetValue(window, out var host))
                        return host;
                }

                foreach (var window in desktop.Windows)
                {
                    if (window.IsVisible && _topLevelToHost.TryGetValue(window, out var host))
                        return host;
                }

                break;
            }
            case ISingleViewApplicationLifetime { MainView: { } mainView }:
            {
                var topLevel = mainView as TopLevel ?? TopLevel.GetTopLevel(mainView);
                if (topLevel is not null && _topLevelToHost.TryGetValue(topLevel, out var host))
                    return host;

                break;
            }
        }

        // Embedded TopLevels are not necessarily represented by the application lifetime.
        return _topLevelToHost.Values.FirstOrDefault();
    }
}