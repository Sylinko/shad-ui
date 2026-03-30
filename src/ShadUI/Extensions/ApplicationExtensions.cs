using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;

// ReSharper disable once CheckNamespace
namespace ShadUI;

/// <summary>
///     Provides extension methods for <see cref="Avalonia.Application" /> class.
/// </summary>
internal static class ApplicationExtensions
{
    /// <summary>
    ///     Gets the top-level window or visual element from an <see cref="Avalonia.Application" />.
    /// </summary>
    /// <param name="app">The <see cref="Avalonia.Application" /> instance.</param>
    /// <returns>
    ///     The top-level window or visual element if found; otherwise, null.
    ///     For desktop applications, returns the main window.
    ///     For single-view applications, returns the visual root of the main view.
    /// </returns>
    public static TopLevel? GetTopLevel(this Application? app)
    {
        return app?.ApplicationLifetime switch
        {
            IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
            ISingleViewApplicationLifetime viewApp => viewApp.MainView?.GetPresentationSource()?.RootVisual as TopLevel,
            _ => null
        };
    }
}