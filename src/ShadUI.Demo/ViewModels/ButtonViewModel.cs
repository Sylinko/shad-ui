using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ShadUI.Demo.ViewModels;

[Page("button")]
public sealed partial class ButtonViewModel : ViewModelBase, INavigable
{
    private readonly PageManager _pageManager;

    public ButtonViewModel(PageManager pageManager)
    {
        _pageManager = pageManager;
        var path = Path.Combine(AppContext.BaseDirectory, "views", "ButtonPage.axaml");
        PrimaryCode = path.ExtractByLineRange(58, 82).CleanIndentation();
        SecondaryCode = path.ExtractByLineRange(85, 109).CleanIndentation();
        DestructiveCode = path.ExtractByLineRange(112, 136).CleanIndentation();
        OutlineCode = path.ExtractByLineRange(139, 163).CleanIndentation();
        GhostCode = path.ExtractByLineRange(166, 190).CleanIndentation();
        IconCode = path.ExtractByLineRange(193, 218).CleanIndentation();
        DestructiveIconCode = path.ExtractByLineRange(221, 242).CleanIndentation();
    }

    [RelayCommand]
    private void BackPage()
    {
        _pageManager.Navigate<AvatarViewModel>();
    }

    [RelayCommand]
    private void NextPage()
    {
        _pageManager.Navigate<CardViewModel>();
    }

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task SubmitAsync()
    {
        IsBusy = true;
        await Task.Delay(5000);
        IsBusy = false;
    }

    [ObservableProperty]
    private string _primaryCode = string.Empty;

    [ObservableProperty]
    private string _secondaryCode = string.Empty;

    [ObservableProperty]
    private string _destructiveCode = string.Empty;

    [ObservableProperty]
    private string _outlineCode = string.Empty;

    [ObservableProperty]
    private string _ghostCode = string.Empty;

    [ObservableProperty]
    private string _iconCode = string.Empty;

    [ObservableProperty]
    private string _destructiveIconCode = string.Empty;
}