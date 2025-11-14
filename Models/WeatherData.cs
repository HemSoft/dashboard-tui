namespace DashboardTui.Models;

/// <summary>
/// Weather data model for display
/// </summary>
public record WeatherData
{
    public required string Location { get; init; }
    public required double TemperatureFahrenheit { get; init; }
    public required string Condition { get; init; }
    public required DateTime LastUpdate { get; init; }
    public double? Humidity { get; init; }
    public double? WindSpeedKph { get; init; }
    public List<ForecastDay>? Forecast { get; init; }
}

/// <summary>
/// Daily forecast data
/// </summary>
public record ForecastDay
{
    public required DateTime Date { get; init; }
    public required double MaxTempF { get; init; }
    public required double MinTempF { get; init; }
    public required string Condition { get; init; }
}
