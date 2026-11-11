using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ShadUI.Demo.ViewModels;

public sealed partial class AboutViewModel : ViewModelBase
{
    [RelayCommand]
    private void Close()
    {
        DialogManager.CloseAll();
    }

    [ObservableProperty]
    private string _appVersion = $"version {GetAssemblyVersion()}";

    private static string GetAssemblyVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString()
               ?? string.Empty;
    }
}