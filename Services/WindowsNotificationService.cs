namespace DashboardTui.Services;

using Windows.UI.Notifications;
using Windows.UI.Notifications.Management;
using NotificationData = DashboardTui.Models.NotificationData;

/// <summary>
/// Windows notification service implementation using UserNotificationListener API
/// </summary>
public class WindowsNotificationService : IWindowsNotificationService
{
    private UserNotificationListener? _listener;
    private bool _accessGranted;

    /// <summary>
    /// Requests user permission to access notifications
    /// </summary>
    public async Task<bool> RequestAccessAsync()
    {
        try
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17134))
            {
                return false; // Requires Windows 10 1803 or later
            }

            _listener = UserNotificationListener.Current;
            var accessStatus = await _listener.RequestAccessAsync();
            _accessGranted = accessStatus == UserNotificationListenerAccessStatus.Allowed;
            return _accessGranted;
        }
        catch
        {
            _accessGranted = false;
            return false;
        }
    }

    /// <summary>
    /// Gets recent notifications ordered by newest first
    /// </summary>
    public async Task<List<NotificationData>> GetNotificationsAsync(int count = 5, CancellationToken cancellationToken = default)
    {
        if (!_accessGranted || _listener == null)
        {
            return [];
        }

        try
        {
            var notifications = await _listener.GetNotificationsAsync(NotificationKinds.Toast);

            return notifications
                .OrderByDescending(n => n.CreationTime)
                .Take(count)
                .Select(n => MapToNotificationData(n))
                .Where(n => n != null)
                .Cast<NotificationData>()
                .ToList();
        }
        catch
        {
            return [];
        }
    }

    /// <summary>
    /// Dismisses a notification by ID
    /// </summary>
    public async Task DismissNotificationAsync(uint notificationId, CancellationToken cancellationToken = default)
    {
        if (!_accessGranted || _listener == null)
        {
            return;
        }

        try
        {
            _listener.RemoveNotification(notificationId);
            await Task.CompletedTask;
        }
        catch
        {
            // Silently ignore dismissal errors
        }
    }

    /// <summary>
    /// Maps UserNotification to NotificationData model
    /// </summary>
    private static NotificationData? MapToNotificationData(UserNotification notification)
    {
        try
        {
            var binding = notification.Notification.Visual.GetBinding(KnownNotificationBindings.ToastGeneric);
            if (binding == null)
            {
                return null;
            }

            var textElements = binding.GetTextElements().ToList();
            var title = textElements.Count > 0 ? textElements[0].Text : "No Title";
            var body = textElements.Count > 1 ? textElements[1].Text : "";

            return new NotificationData
            {
                Id = notification.Id,
                AppName = notification.AppInfo.DisplayInfo.DisplayName,
                Title = title,
                Body = body,
                Timestamp = notification.CreationTime.DateTime
            };
        }
        catch
        {
            return null;
        }
    }
}
