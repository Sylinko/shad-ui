using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ShadUI.Demo;

public partial class InstanceDialog : ShadWindow
{
    public InstanceDialog()
    {
        InitializeComponent();
    }

    private void OnClose(object sender, RoutedEventArgs e)
    {
        Close();
    }
}