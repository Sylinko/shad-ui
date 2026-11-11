using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ShadUI.Demo.ViewModels;

[Page("menu")]
public sealed partial class MenuViewModel : ViewModelBase, INavigable
{
    private readonly PageManager _pageManager;

    public MenuViewModel(PageManager pageManager)
    {
        _pageManager = pageManager;
        var path = Path.Combine(AppContext.BaseDirectory, "views", "MenuPage.axaml");
        MenuFlyoutCode = path.ExtractByLineRange(58, 199).CleanIndentation();
        MenuBarCode = path.ExtractByLineRange(202, 277).CleanIndentation();
        DropDownCode = path.ExtractByLineRange(281, 444).CleanIndentation();
    }

    [RelayCommand]
    private void BackPage()
    {
        _pageManager.Navigate<InputViewModel>();
    }

    [RelayCommand]
    private void NextPage()
    {
        _pageManager.Navigate<NumericViewModel>();
    }

    [ObservableProperty]
    private string _menuFlyoutCode = string.Empty;

    [ObservableProperty]
    private string _menuBarCode = string.Empty;

    [ObservableProperty]
    private string _dropDownCode = string.Empty;
}