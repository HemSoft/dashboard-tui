namespace DashboardTui.Models;

/// <summary>
/// Notification data model for display
/// </summary>
public record NotificationData
{
    public required uint Id { get; init; }
    public required string AppName { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required DateTime Timestamp { get; init; }

    /// <summary>
    /// Gets a truncated single-line display of the notification body
    /// </summary>
    public string GetTruncatedBody(int maxLength = 50)
    {
        var cleanBody = Body.Replace("\n", " ").Replace("\r", " ").Trim();
        return cleanBody.Length <= maxLength ? cleanBody : cleanBody[..maxLength] + "...";
    }
}
