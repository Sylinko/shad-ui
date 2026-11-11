using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShadUI.Demo.Views;

namespace ShadUI.Demo.ViewModels;

[Page("dialog")]
public sealed partial class DialogViewModel : ViewModelBase, INavigable
{
    private readonly PageManager _pageManager;
    private readonly LoginViewModel _loginViewModel;

    public DialogViewModel(
        PageManager pageManager,
        LoginViewModel loginViewModel)
    {
        _pageManager = pageManager;
        _loginViewModel = loginViewModel;

        var path = Path.Combine(AppContext.BaseDirectory, "viewModels", "DialogViewModel.cs");
        AlertDialogCode = WrapCode(path.ExtractByLineRange(55, 73).CleanIndentation());
        DestructiveAlertDialogCode = WrapCode(path.ExtractByLineRange(78, 96).CleanIndentation());
        CustomDialogCode = WrapCode(path.ExtractByLineRange(101, 126).CleanIndentation());
    }

    [RelayCommand]
    private void BackPage()
    {
        _pageManager.Navigate<DateViewModel>();
    }

    [RelayCommand]
    private void NextPage()
    {
        _pageManager.Navigate<InputViewModel>();
    }

    private string WrapCode(string code)
    {
        return $"""
                using CommunityToolkit.Mvvm.Input;

                //..other code

                {code}

                //..rest of the code

                """;
    }

    [ObservableProperty]
    private string _alertDialogCode = string.Empty;

    [RelayCommand]
    private async Task ShowDialog()
    {
        var result = await DialogManager
            .CreateDialog(
                "This action cannot be undone. This will permanently delete your account and remove your data from our servers.",
                "Are you absolutely sure?")
            .WithPrimaryButton("Continue")
            .WithCancelButton("Cancel")
            .WithMaxWidth(512)
            .Dismissible()
            .ShowAsync(CancellationToken.None);

        if (result == DialogResult.Primary)
            ToastManager.Create("Delete account")
                .WithContent("Account deleted successfully!")
                .DismissOnClick()
                .ShowSuccess();
    }

    [ObservableProperty]
    private string _destructiveAlertDialogCode = string.Empty;

    [RelayCommand]
    private async Task ShowDestructiveStyleDialog()
    {
        var result = await DialogManager
            .CreateDialog(
                "This action cannot be undone. This will permanently delete your account and remove your data from our servers.",
                "Are you absolutely sure?")
            .WithPrimaryButton("Continue", buttonStyle: ButtonStyle.Destructive)
            .WithCancelButton("Cancel")
            .WithMaxWidth(512)
            .Dismissible()
            .ShowAsync(CancellationToken.None);

        if (result == DialogResult.Primary)
            ToastManager.Create("Delete account")
                .WithContent("Account deleted successfully!")
                .DismissOnClick()
                .ShowSuccess();
    }

    [ObservableProperty]
    private string _customDialogCode = string.Empty;

    [RelayCommand]
    private async Task ShowCustomDialog()
    {
        _loginViewModel.Initialize();
        var result = await DialogManager.CreateCustomDialog(new LoginContent
            {
                DataContext = _loginViewModel
            })
            .Dismissible()
            .ShowAsync(CancellationToken.None);

        if (result == DialogResult.Cancel)
        {
            ToastManager.Create("Sign in cancelled")
                .WithContent("Please sign in to continue.")
                .DismissOnClick()
                .ShowWarning();
        }
        else
        {
            ToastManager.Create("Sign in successful")
                .WithContent($"Hi {_loginViewModel.Email}, welcome back!")
                .DismissOnClick()
                .ShowSuccess();
        }
    }
}