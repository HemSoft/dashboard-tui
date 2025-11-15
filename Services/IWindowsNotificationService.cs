namespace DashboardTui.Services;

using DashboardTui.Models;

/// <summary>
/// Interface for Windows notification services
/// </summary>
public interface IWindowsNotificationService
{
    /// <summary>
    /// Requests user permission to access notifications (first-time only)
    /// </summary>
    /// <returns>True if access granted, false otherwise</returns>
    Task<bool> RequestAccessAsync();
    
    /// <summary>
    /// Gets the most recent notifications
    /// </summary>
    /// <param name="count">Maximum number of notifications to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of notifications ordered by newest first</returns>
    Task<List<NotificationData>> GetNotificationsAsync(int count = 5, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Dismisses a notification by ID
    /// </summary>
    /// <param name="notificationId">Notification ID to dismiss</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DismissNotificationAsync(uint notificationId, CancellationToken cancellationToken = default);
}
