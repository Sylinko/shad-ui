using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ShadUI.Demo.ViewModels.Examples.ComboBox;

public sealed partial class FormComboBoxViewModel : ViewModelBase
{

    public FormComboBoxViewModel()
    {

        var xamlPath = Path.Combine(AppContext.BaseDirectory, "views", "Examples", "ComboBox",
            "FormComboBoxContent.axaml");
        XamlCode = xamlPath.ExtractByLineRange(1, 47).CleanIndentation();

        var csharpPath = Path.Combine(AppContext.BaseDirectory, "viewModels", "Examples", "ComboBox",
            "FormComboBoxViewModel.cs");
        CSharpCode = csharpPath.ExtractWithSkipRanges((16, 22), (24, 29)).CleanIndentation();
    }

    [ObservableProperty]
    private string _xamlCode = string.Empty;

    [ObservableProperty]
    private string _cSharpCode = string.Empty;

    [ObservableProperty]
    private string[] _items =
    [
        "Angular",
        "Astro",
        "Lit",
        "Next.js",
        "Nuxt.js",
        "Preact",
        "Qwik",
        "React",
        "Remix",
        "SolidJS",
        "Svelte",
        "SvelteKit",
        "Vue.js"
    ];

    private string? _selectedItem = "Next.js";

    [Required(ErrorMessage = "Please select an item.")]
    public string? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value, true);
    }

    [RelayCommand]
    private void Submit()
    {
        ClearAllErrors();
        ValidateAllProperties();

        if (HasErrors) return;

        ToastManager.Create("Submission Successful")
            .WithContent($"Your selection ({SelectedItem}) has been submitted successfully!")
            .DismissOnClick()
            .ShowSuccess();
    }

    [RelayCommand]
    private void Clear()
    {
        SelectedItem = null;
    }
}