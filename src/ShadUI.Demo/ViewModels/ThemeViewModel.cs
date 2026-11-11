using CommunityToolkit.Mvvm.Input;

namespace ShadUI.Demo.ViewModels;

[Page("theme")]
public sealed partial class ThemeViewModel(PageManager pageManager) : ViewModelBase, INavigable
{
    [RelayCommand]
    private async Task PreviewDialogAsync()
    {
        await DialogManager.CreateDialog("Review the input, button, border and surface colors together.", "Save changes?")
            .WithPrimaryButton("Save changes")
            .WithCancelButton("Cancel")
            .Dismissible()
            .ShowAsync(CancellationToken.None);
    }

    [RelayCommand]
    private void BackPage()
    {
        pageManager.Navigate<DashboardViewModel>();
    }

    [RelayCommand]
    private void NextPage()
    {
        pageManager.Navigate<TypographyViewModel>();
    }
}