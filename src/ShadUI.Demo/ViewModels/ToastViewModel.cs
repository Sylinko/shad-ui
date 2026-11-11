using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ShadUI.Demo.ViewModels;

[Page("toast")]
public sealed partial class ToastViewModel : ViewModelBase, INavigable
{
    private readonly PageManager _pageManager;

    public ToastViewModel(PageManager pageManager)
    {
        _pageManager = pageManager;
        var path = Path.Combine(AppContext.BaseDirectory, "viewModels", "ToastViewModel.cs");
        SimpleToastCode = WrapCode(path.ExtractByLineRange(52, 56).CleanIndentation());
        WithTitleToastCode = WrapCode(path.ExtractByLineRange(61, 67).CleanIndentation());
        WithActionToastCode = WrapCode(path.ExtractByLineRange(72, 84).CleanIndentation());
        WithDelayToastCode = WrapCode(path.ExtractByLineRange(89, 96).CleanIndentation());
        InfoToastCode = WrapCode(path.ExtractByLineRange(101, 108).CleanIndentation());
        SuccessToastCode = WrapCode(path.ExtractByLineRange(113, 120).CleanIndentation());
        WarningToastCode = WrapCode(path.ExtractByLineRange(125, 132).CleanIndentation());
        ErrorToastCode = WrapCode(path.ExtractByLineRange(137, 149).CleanIndentation());
        DynamicToastCode = WrapCode(path.ExtractByLineRange(170, 183).CleanIndentation());
    }

    [RelayCommand]
    private void BackPage()
    {
        _pageManager.Navigate<TimeViewModel>();
    }

    [RelayCommand]
    private void NextPage()
    {
        _pageManager.Navigate<ToggleViewModel>();
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

    [RelayCommand]
    private void ShowSimpleToast()
    {
        ToastManager.Create("Your message has been sent.").Show();
    }

    [ObservableProperty]
    private string _simpleToastCode = string.Empty;

    [RelayCommand]
    private void ShowToastWithTitle()
    {
        ToastManager.Create("Uh oh! Something went wrong.")
            .WithContent("There was a problem with your request.")
            .Show();
    }

    [ObservableProperty]
    private string _withTitleToastCode = string.Empty;

    [RelayCommand]
    private async Task ShowToastWithActionAsync()
    {
        var result = await ToastManager.Create("Uh oh! Something went wrong.")
            .WithContent("There was a problem with your request.")
            .WithAction("Try again")
            .ShowAsync();

        if (result == ToastResult.ActionButtonClicked)
        {
            await ToastManager.Create("Retry clicked").ShowAsync();
        }
    }

    [ObservableProperty]
    private string _withActionToastCode = string.Empty;

    [RelayCommand]
    private void ShowToastWithDelay()
    {
        ToastManager.Create("This toast will disappear in 5 seconds.")
            .WithContent("You can hover over it to prevent it from disappearing.")
            .WithDurationSeconds(5)
            .Show();
    }

    [ObservableProperty]
    private string _withDelayToastCode = string.Empty;

    [RelayCommand]
    private void ShowInfoToast()
    {
        ToastManager.Create("Event has been created")
            .WithContent($"{DateTime.Now:dddd, MMMM d 'at' h:mm tt}")
            .DismissOnClick()
            .ShowInfo();
    }

    [ObservableProperty]
    private string _infoToastCode = string.Empty;

    [RelayCommand]
    private void ShowSuccessToast()
    {
        ToastManager.Create("Event has been created successfully")
            .WithContent($"{DateTime.Now:dddd, MMMM d 'at' h:mm tt}")
            .DismissOnClick()
            .ShowSuccess();
    }

    [ObservableProperty]
    private string _successToastCode = string.Empty;

    [RelayCommand]
    private void ShowWarningToast()
    {
        ToastManager.Create("Event has been created with warnings")
            .WithContent($"{DateTime.Now:dddd, MMMM d 'at' h:mm tt}")
            .DismissOnClick()
            .ShowWarning();
    }

    [ObservableProperty]
    private string _warningToastCode = string.Empty;

    [RelayCommand]
    private async Task ShowErrorToastAsync()
    {
        var result = await ToastManager.Create("Failed to create event")
            .WithContent("Unable to connect to the server.")
            .WithAction("Retry")
            .ShowErrorAsync();

        if (result == ToastResult.ActionButtonClicked)
        {
            await ToastManager.Create("Retry clicked").ShowAsync();
        }
    }

    [ObservableProperty]
    private string _errorToastCode = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Notification> _notificationsCollection =
    [
        Notification.Basic,
        Notification.Info,
        Notification.Success,
        Notification.Warning,
        Notification.Error
    ];

    [ObservableProperty]
    private Notification _selectedNotification;

    [ObservableProperty]
    private string _dynamicToastCode = string.Empty;

    [RelayCommand]
    private async Task ShowToastWithTypeAsync()
    {
        var result = await ToastManager.Create("Your message has been sent.")
            .WithContent($"{DateTime.Now:dddd, MMMM d 'at' h:mm tt}")
            .WithAction("Retry")
            .DismissOnClick()
            .ShowAsync(SelectedNotification);

        if (result == ToastResult.ActionButtonClicked)
        {
            await ToastManager.Create("Retry clicked").ShowAsync(Notification.Success);
        }
    }
}