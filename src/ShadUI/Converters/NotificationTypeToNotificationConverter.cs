using System.Globalization;
using Avalonia.Controls.Notifications;
using Avalonia.Data.Converters;

namespace ShadUI;

public sealed class NotificationTypeToNotificationConverter : IValueConverter
{
    public static NotificationTypeToNotificationConverter Shared { get; } = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        NotificationType.Information => Notification.Info,
        NotificationType.Success => Notification.Success,
        NotificationType.Warning => Notification.Warning,
        NotificationType.Error => Notification.Error,
        _ => Notification.Basic
    };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => value switch
    {
        Notification.Info => NotificationType.Information,
        Notification.Success => NotificationType.Success,
        Notification.Warning => NotificationType.Warning,
        Notification.Error => NotificationType.Error,
        _ => NotificationType.Information
    };
}