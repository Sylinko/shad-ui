using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

// ReSharper disable once CheckNamespace
namespace ShadUI;

public class CheckUpdate : TemplatedControl
{
    public static readonly StyledProperty<ICommand?> CommandProperty =
        Button.CommandProperty.AddOwner<CheckUpdate>();

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly StyledProperty<object?> CommandParameterProperty =
        Button.CommandParameterProperty.AddOwner<CheckUpdate>();

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public static readonly StyledProperty<bool> IsUpdateAvailableProperty =
        AvaloniaProperty.Register<CheckUpdate, bool>(nameof(IsUpdateAvailable));

    public bool IsUpdateAvailable
    {
        get => GetValue(IsUpdateAvailableProperty);
        set => SetValue(IsUpdateAvailableProperty, value);
    }

    public static readonly StyledProperty<bool> IsCheckingUpdateProperty =
        AvaloniaProperty.Register<CheckUpdate, bool>(nameof(IsCheckingUpdate));

    public bool IsCheckingUpdate
    {
        get => GetValue(IsCheckingUpdateProperty);
        set => SetValue(IsCheckingUpdateProperty, value);
    }

    public static readonly StyledProperty<string> UpdateAvailableTitleProperty =
        AvaloniaProperty.Register<CheckUpdate, string>(nameof(UpdateAvailableTitle));

    public string UpdateAvailableTitle
    {
        get => GetValue(UpdateAvailableTitleProperty);
        set => SetValue(UpdateAvailableTitleProperty, value);
    }

    public static readonly StyledProperty<string> UpdateAvailableVersionTitleProperty =
        AvaloniaProperty.Register<CheckUpdate, string>(nameof(UpdateAvailableVersionTitle));

    public string UpdateAvailableVersionTitle
    {
        get => GetValue(UpdateAvailableVersionTitleProperty);
        set => SetValue(UpdateAvailableVersionTitleProperty, value);
    }

    public static readonly StyledProperty<string> NewVersionProperty =
        AvaloniaProperty.Register<CheckUpdate, string>(nameof(NewVersion));

    public string NewVersion
    {
        get => GetValue(NewVersionProperty);
        set => SetValue(NewVersionProperty, value);
    }

    public static readonly StyledProperty<string> UpdateNotAvailableTitleProperty =
        AvaloniaProperty.Register<CheckUpdate, string>(nameof(UpdateNotAvailableTitle));

    public string UpdateNotAvailableTitle
    {
        get => GetValue(UpdateNotAvailableTitleProperty);
        set => SetValue(UpdateNotAvailableTitleProperty, value);
    }

    public static readonly StyledProperty<string> LastUpdateCheckTitleProperty =
        AvaloniaProperty.Register<CheckUpdate, string>(nameof(LastUpdateCheckTitle));

    public string LastUpdateCheckTitle
    {
        get => GetValue(LastUpdateCheckTitleProperty);
        set => SetValue(LastUpdateCheckTitleProperty, value);
    }

    public static readonly StyledProperty<DateTimeOffset> LastUpdateCheckDateProperty =
        AvaloniaProperty.Register<CheckUpdate, DateTimeOffset>(nameof(LastUpdateCheckDate));

    public DateTimeOffset LastUpdateCheckDate
    {
        get => GetValue(LastUpdateCheckDateProperty);
        set => SetValue(LastUpdateCheckDateProperty, value);
    }

    public static readonly StyledProperty<string> CheckingUpdateTitleProperty =
        AvaloniaProperty.Register<CheckUpdate, string>(nameof(CheckingUpdateTitle));

    public string CheckingUpdateTitle
    {
        get => GetValue(CheckingUpdateTitleProperty);
        set => SetValue(CheckingUpdateTitleProperty, value);
    }

    public static readonly StyledProperty<string> CheckingUpdateDescriptionProperty = AvaloniaProperty.Register<CheckUpdate, string>(
        nameof(CheckingUpdateDescription));

    public string CheckingUpdateDescription
    {
        get => GetValue(CheckingUpdateDescriptionProperty);
        set => SetValue(CheckingUpdateDescriptionProperty, value);
    }

    public static readonly StyledProperty<string> NeverCheckedTitleProperty =
        AvaloniaProperty.Register<CheckUpdate, string>(nameof(NeverCheckedTitle));

    public string NeverCheckedTitle
    {
        get => GetValue(NeverCheckedTitleProperty);
        set => SetValue(NeverCheckedTitleProperty, value);
    }

    public static readonly DirectProperty<CheckUpdate, bool> IsNeverCheckedProperty =
        AvaloniaProperty.RegisterDirect<CheckUpdate, bool>(nameof(IsNeverChecked), o => o.IsNeverChecked);

    public bool IsNeverChecked
    {
        get;
        private set => SetAndRaise(IsNeverCheckedProperty, ref field, value);
    } = true;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == LastUpdateCheckDateProperty)
        {
            IsNeverChecked = LastUpdateCheckDate == DateTimeOffset.MinValue;
        }
    }
}