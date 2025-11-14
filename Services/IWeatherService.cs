namespace DashboardTui.Services;

using DashboardTui.Models;

/// <summary>
/// Interface for weather data services
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Gets current weather data for a location
    /// </summary>
    /// <param name="location">City name, coordinates, or IP address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current weather data</returns>
    Task<WeatherData> GetCurrentWeatherAsync(string location, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current weather with forecast for a location
    /// </summary>
    /// <param name="location">City name, coordinates, or IP address</param>
    /// <param name="days">Number of days to forecast (1-10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Current weather data with forecast</returns>
    Task<WeatherData> GetWeatherWithForecastAsync(string location, int days = 3, CancellationToken cancellationToken = default);
}
