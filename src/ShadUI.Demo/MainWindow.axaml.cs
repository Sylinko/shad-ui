using Avalonia.Controls;
using Avalonia.Interactivity;
// using HotAvalonia;

namespace ShadUI.Demo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Initialize();
    }

    // [AvaloniaHotReload]
    private void Initialize()
    {
        ToolTip.SetTip(FullscreenButton, "Fullscreen");
        FullscreenButton.Click += OnFullScreen;
    }

    private void OnFullScreen(object? sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.FullScreen)
        {
            ToolTip.SetTip(FullscreenButton, "Fullscreen");
        }
        else
        {
            WindowState = WindowState.FullScreen;
            ToolTip.SetTip(FullscreenButton, "Exit Fullscreen");
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        e.Cancel = e is { CloseReason: not WindowCloseReason.OSShutdown and not WindowCloseReason.ApplicationShutdown };

        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.TryCloseCommand.Execute(null);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // Dispose ViewModel
        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }

        // Clear DataContext
        DataContext = null;
    }
}