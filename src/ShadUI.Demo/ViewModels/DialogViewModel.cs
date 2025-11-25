using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ShadUI.Demo.Views;

namespace ShadUI.Demo.ViewModels;

[Page("dialog")]
public sealed partial class DialogViewModel : ViewModelBase, INavigable
{
    private readonly PageManager _pageManager;
    private readonly DialogManager _dialogManager;
    private readonly ToastManager _toastManager;
    private readonly LoginViewModel _loginViewModel;

    public DialogViewModel(
        PageManager pageManager,
        DialogManager dialogManager,
        ToastManager toastManager,
        LoginViewModel loginViewModel)
    {
        _pageManager = pageManager;
        _dialogManager = dialogManager;
        _toastManager = toastManager;
        _loginViewModel = loginViewModel;

        var path = Path.Combine(AppContext.BaseDirectory, "viewModels", "DialogViewModel.cs");
        AlertDialogCode = WrapCode(path.ExtractByLineRange(62, 78).CleanIndentation());
        DestructiveAlertDialogCode = WrapCode(path.ExtractByLineRange(83, 100).CleanIndentation());
        CustomDialogCode = WrapCode(path.ExtractByLineRange(105, 122).CleanIndentation());
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
        var result = await _dialogManager
            .CreateDialog(
                "Are you absolutely sure?",
                "This action cannot be undone. This will permanently delete your account and remove your data from our servers.")
            .WithPrimaryButton("Continue")
            .WithCancelButton("Cancel")
            .WithMaxWidth(512)
            .Dismissible()
            .ShowAsync();

        if (result == DialogResult.Primary)
            _toastManager.CreateToast("Delete account")
                .WithContent("Account deleted successfully!")
                .DismissOnClick()
                .ShowSuccess();
    }

    [ObservableProperty]
    private string _destructiveAlertDialogCode = string.Empty;

    [RelayCommand]
    private async Task ShowDestructiveStyleDialog()
    {
        var result = await _dialogManager
            .CreateDialog(
                "Are you absolutely sure?",
                "This action cannot be undone. This will permanently delete your account and remove your data from our servers.")
            .WithPrimaryButton("Continue", buttonStyle: DialogButtonStyle.Destructive)
            .WithCancelButton("Cancel")
            .WithMaxWidth(512)
            .Dismissible()
            .ShowAsync();

        if (result == DialogResult.Primary)
            _toastManager.CreateToast("Delete account")
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
        var result = await _dialogManager.CreateDialog(new LoginContent
            {
                DataContext = _loginViewModel
            })
            .Dismissible()
            .ShowAsync();

        if (result == DialogResult.Cancel)
        {
            _toastManager.CreateToast("Sign in cancelled")
                .WithContent("Please sign in to continue.")
                .DismissOnClick()
                .ShowWarning();
        }
        else
        {
            _toastManager.CreateToast("Sign in successful")
                .WithContent($"Hi {_loginViewModel.Email}, welcome back!")
                .DismissOnClick()
                .ShowSuccess();
        }
    }
}