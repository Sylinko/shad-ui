using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ShadUI.Demo.ViewModels;

[Page("checkupdate")]
public sealed partial class CheckUpdateViewModel : ViewModelBase, INavigable
{
    private readonly PageManager _pageManager;

    public CheckUpdateViewModel(PageManager pageManager)
    {
        _pageManager = pageManager;
    }

    [RelayCommand]
    private void BackPage()
    {
        _pageManager.Navigate<CheckUpdateViewModel>();
    }

    [RelayCommand]
    private void NextPage()
    {
        _pageManager.Navigate<CheckUpdateViewModel>();
    }

    [ObservableProperty]
    public partial string UpdateAvailableCode { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string UpdateNotAvailableCode { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsUpdateAvailable { get; set; } = true;

    [ObservableProperty]
    public partial bool IsCheckingUpdate { get; set; } = false;
    
    [ObservableProperty]
    public partial DateTimeOffset UpdateNotAvailableLastCheckDate { get; set; } = DateTimeOffset.Now;
}