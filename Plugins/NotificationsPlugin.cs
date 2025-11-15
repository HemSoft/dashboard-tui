namespace DashboardTui.Plugins;

using System.Collections.ObjectModel;
using Terminal.Gui;
using DashboardTui.Models;
using DashboardTui.Services;

/// <summary>
/// Notifications plugin component displaying Windows Action Center notifications
/// </summary>
public class NotificationsPlugin : FrameView
{
    private readonly ListView _notificationsList;
    private readonly Label _statusLabel;
    private readonly IWindowsNotificationService _notificationService;
    private readonly NotificationsConfig _config;
    private readonly System.Threading.Timer? _refreshTimer;
    private List<NotificationData> _notifications = [];
    private bool _accessGranted;

    public NotificationsPlugin(IWindowsNotificationService notificationService, NotificationsConfig config)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        Title = "Notifications";
        BorderStyle = LineStyle.Single;

        // Status label (shows permission status or error messages)
        _statusLabel = new()
        {
            X = 1,
            Y = 0,
            Height = 1,
            Width = Dim.Fill()! - 2,
            Text = "Requesting notification access..."
        };

        // Notifications list view
        _notificationsList = new()
        {
            X = 1,
            Y = Pos.Bottom(_statusLabel),
            Width = Dim.Fill()! - 2,
            Height = Dim.Fill()! - 1,
            AllowsMarking = true,
            AllowsMultipleSelection = true
        };

        // Handle notification selection (dismiss on Enter)
        _notificationsList.OpenSelectedItem += async (s, e) => await DismissSelectedNotificationAsync();

        // Handle key presses for dismiss all functionality
        KeyDown += async (s, e) => await OnKeyDownAsync(e);

        Add(_statusLabel, _notificationsList);

        // Request access and load initial notifications
        _ = InitializeAsync();

        // Start auto-refresh timer if interval is positive
        if (config.DataRefreshIntervalSeconds > 0)
        {
            var intervalMs = config.DataRefreshIntervalSeconds * 1000;
            _refreshTimer = new System.Threading.Timer(
                async _ => await RefreshAsync(),
                null,
                intervalMs,
                intervalMs
            );
        }
    }

    /// <summary>
    /// Initializes the notification service and requests access
    /// </summary>
    private async Task InitializeAsync()
    {
        try
        {
            _accessGranted = await _notificationService.RequestAccessAsync();

            if (_accessGranted)
            {
                _statusLabel.Text = "Space to select, Enter to dismiss - Enter/Space with no selection dismisses all";
                await LoadNotificationsAsync();
            }
            else
            {
                _statusLabel.Text = "⚠️ Permission denied - Enable in Windows Settings";
                _notificationsList.SetSource(new ObservableCollection<string>());
            }
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"⚠️ Error: {ex.Message}";
            _notificationsList.SetSource(new ObservableCollection<string>());
        }
    }

    /// <summary>
    /// Loads notifications from the service
    /// </summary>
    private async Task LoadNotificationsAsync()
    {
        if (!_accessGranted)
        {
            return;
        }

        try
        {
            _notifications = await _notificationService.GetNotificationsAsync(_config.DisplayCount);

            if (_notifications.Count == 0)
            {
                _statusLabel.Text = "No notifications";
                _notificationsList.SetSource(new ObservableCollection<string>());
            }
            else
            {
                _statusLabel.Text = $"Showing {_notifications.Count} notification(s) - Space to select, Enter to dismiss - Enter/Space dismisses all if none selected";

                var displayItems = _notifications.Select(n =>
                {
                    var timeStr = FormatTimestamp(n.Timestamp);
                    var truncatedBody = n.GetTruncatedBody(50);
                    var bodyDisplay = string.IsNullOrWhiteSpace(truncatedBody) ? "" : $" - {truncatedBody}";
                    return $"[{timeStr}] {n.AppName}: {n.Title}{bodyDisplay}";
                }).ToList();

                _notificationsList.SetSource(new ObservableCollection<string>(displayItems));
            }
        }
        catch (Exception ex)
        {
            _statusLabel.Text = $"⚠️ Error loading notifications: {ex.Message}";
            _notificationsList.SetSource(new ObservableCollection<string>());
        }
    }

    /// <summary>
    /// Dismisses the currently selected notification(s)
    /// </summary>
    private async Task DismissSelectedNotificationAsync()
    {
        if (!_accessGranted || _notifications.Count == 0)
        {
            return;
        }

        try
        {
            // Get marked items (multi-select) or current selected item
            var markedIndices = new List<int>();
            for (int i = 0; i < _notificationsList.Source.Count; i++)
            {
                if (_notificationsList.Source.IsMarked(i))
                {
                    markedIndices.Add(i);
                }
            }

            // If no items marked, dismiss the currently selected item
            if (markedIndices.Count == 0 && _notificationsList.SelectedItem >= 0 && _notificationsList.SelectedItem < _notifications.Count)
            {
                markedIndices.Add(_notificationsList.SelectedItem);
            }

            // Dismiss all selected notifications
            foreach (var index in markedIndices)
            {
                if (index >= 0 && index < _notifications.Count)
                {
                    var notification = _notifications[index];
                    await _notificationService.DismissNotificationAsync(notification.Id);
                }
            }

            if (markedIndices.Count > 0)
            {
                await RefreshAsync();
            }
        }
        catch
        {
            // Silently ignore dismissal errors
        }
    }

    /// <summary>
    /// Handles key presses for dismiss all functionality
    /// </summary>
    private async Task OnKeyDownAsync(Key key)
    {
        // If Enter or Space is pressed and no item is selected in the list, dismiss all
        if ((key == Key.Enter || key == Key.Space) &&
            _accessGranted &&
            _notifications.Count > 0 &&
            _notificationsList.SelectedItem < 0)
        {
            try
            {
                // Dismiss all notifications
                foreach (var notification in _notifications)
                {
                    await _notificationService.DismissNotificationAsync(notification.Id);
                }
                await RefreshAsync();
            }
            catch
            {
                // Silently ignore dismissal errors
            }
        }
    }

    /// <summary>
    /// Formats a timestamp for display
    /// </summary>
    private static string FormatTimestamp(DateTime timestamp)
    {
        var now = DateTime.Now;
        var diff = now - timestamp;

        if (diff.TotalMinutes < 1)
        {
            return "Just now";
        }
        else if (diff.TotalMinutes < 60)
        {
            var mins = (int)diff.TotalMinutes;
            return $"{mins}m ago";
        }
        else if (diff.TotalHours < 24)
        {
            var hours = (int)diff.TotalHours;
            return $"{hours}h ago";
        }
        else if (diff.TotalDays < 7)
        {
            var days = (int)diff.TotalDays;
            return $"{days}d ago";
        }
        else
        {
            return timestamp.ToString("MM/dd HH:mm");
        }
    }

    /// <summary>
    /// Refreshes notifications from source
    /// </summary>
    public async Task RefreshAsync()
    {
        await LoadNotificationsAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _refreshTimer?.Dispose();
        }
        base.Dispose(disposing);
    }
}
