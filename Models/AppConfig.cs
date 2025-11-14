namespace DashboardTui.Models;

/// <summary>
/// Application configuration settings
/// </summary>
public record AppConfig
{
    public required MainWindowConfig MainWindow { get; init; }
    public required WeatherConfig Weather { get; init; }
}

/// <summary>
/// Main window UI configuration
/// </summary>
public record MainWindowConfig
{
    public int UiRefreshIntervalSeconds { get; init; } = 15;
    public bool ShowLastUpdateInHeader { get; init; } = true;
    public string Theme { get; init; } = "Default";
}

/// <summary>
/// Weather plugin configuration
/// </summary>
public record WeatherConfig
{
    public required string ApiKey { get; init; }
    public List<string> Locations { get; set; } = ["auto:ip"];
    public int CurrentLocationIndex { get; set; } = 0;
    public int DataRefreshIntervalSeconds { get; init; } = 600; // 10 minutes default
}
