namespace DashboardTui.Plugins;

using Terminal.Gui;
using DashboardTui.Models;
using DashboardTui.Services;

/// <summary>
/// Weather plugin component displaying current weather information
/// </summary>
public class WeatherPlugin : FrameView
{
    private readonly Label _temperatureLabel;
    private readonly Label _conditionLabel;
    private readonly Label _locationLabel;
    private readonly Label _forecastLabel;
    private readonly Label _lastUpdateLabel;
    private readonly IWeatherService _weatherService;
    private readonly WeatherConfig _config;
    private readonly System.Threading.Timer? _refreshTimer;

    public WeatherPlugin(IWeatherService weatherService, WeatherConfig config)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        Title = "Weather";
        BorderStyle = LineStyle.Single;

        // Location
        _locationLabel = new()
        {
            X = 1,
            Y = 0,
            Height = 1,
            Width = Dim.Fill(),
            Text = "Location: Loading..."
        };

        // Temperature
        _temperatureLabel = new()
        {
            X = 1,
            Y = Pos.Bottom(_locationLabel),
            Height = 1,
            Width = Dim.Fill(),
            Text = "Temperature: --°F"
        };

        // Condition
        _conditionLabel = new()
        {
            X = 1,
            Y = Pos.Bottom(_temperatureLabel),
            Height = 1,
            Width = Dim.Fill(),
            Text = "Condition: --"
        };

        // 3-day forecast
        _forecastLabel = new()
        {
            X = 1,
            Y = Pos.Bottom(_conditionLabel),
            Height = 1,
            Width = Dim.Fill(),
            Text = "Forecast: Loading..."
        };

        // Last update timestamp
        _lastUpdateLabel = new()
        {
            X = 1,
            Y = Pos.Bottom(_forecastLabel) + 1,
            Height = 1,
            Width = Dim.Fill(),
            Text = "Last update: Never"
        };

        Add(_locationLabel, _temperatureLabel, _conditionLabel, _forecastLabel, _lastUpdateLabel);

        // Load initial weather data asynchronously
        _ = LoadWeatherDataAsync();

        // Start weather refresh timer if interval is positive
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
    /// Updates weather data display
    /// </summary>
    private void UpdateWeather(WeatherData data)
    {
        var weatherEmoji = GetWeatherEmoji(data.Condition);
        _locationLabel.Text = $"Location: {data.Location}";
        _temperatureLabel.Text = $"Temperature: {data.TemperatureFahrenheit:F1}°F";
        _conditionLabel.Text = $"{weatherEmoji} {data.Condition}";

        // Format 3-day forecast in one line
        if (data.Forecast != null && data.Forecast.Count > 0)
        {
            var forecastParts = data.Forecast.Take(3).Select(f =>
            {
                var emoji = GetWeatherEmoji(f.Condition);
                var dayName = f.Date.ToString("ddd");
                return $"{dayName} {emoji}{f.MaxTempF:F0}°/{f.MinTempF:F0}°";
            });
            _forecastLabel.Text = string.Join(" | ", forecastParts);
        }
        else
        {
            _forecastLabel.Text = "Forecast: Unavailable";
        }

        _lastUpdateLabel.Text = $"Last update: {data.LastUpdate:HH:mm:ss}";
    }

    /// <summary>
    /// Maps weather conditions to appropriate emojis
    /// </summary>
    private static string GetWeatherEmoji(string condition)
    {
        var lowerCondition = condition.ToLowerInvariant();

        return lowerCondition switch
        {
            // Clear/Sunny
            var c when c.Contains("clear") || c.Contains("sunny") => "\u2600\uFE0F",  // ☀️

            // Clouds
            var c when c.Contains("partly cloudy") || c.Contains("partly sunny") => "\u26C5",  // ⛅
            var c when c.Contains("cloudy") || c.Contains("overcast") => "\u2601\uFE0F",  // ☁️

            // Rain
            var c when c.Contains("drizzle") || c.Contains("light rain") => "\U0001F327\uFE0F",  // 🌧️
            var c when c.Contains("rain") || c.Contains("shower") => "\U0001F327\uFE0F",  // 🌧️
            var c when c.Contains("heavy rain") || c.Contains("downpour") => "\u26C8\uFE0F",  // ⛈️

            // Thunderstorm
            var c when c.Contains("thunder") || c.Contains("lightning") => "\u26C8\uFE0F",  // ⛈️

            // Snow
            var c when c.Contains("snow") || c.Contains("flurries") => "\u2744\uFE0F",  // ❄️
            var c when c.Contains("blizzard") => "\U0001F328\uFE0F",  // 🌨️

            // Fog/Mist
            var c when c.Contains("fog") || c.Contains("mist") || c.Contains("haze") => "\U0001F32B\uFE0F",  // 🌫️

            // Wind
            var c when c.Contains("wind") || c.Contains("breezy") => "\U0001F32C\uFE0F",  // 🌬️

            // Storm/Tornado
            var c when c.Contains("tornado") || c.Contains("hurricane") => "\U0001F32A\uFE0F",  // 🌪️

            // Night
            var c when c.Contains("night") && c.Contains("clear") => "\U0001F319",  // 🌙

            // Default
            _ => "\U0001F324\uFE0F"  // 🌤️ (sun behind cloud)
        };
    }

    /// <summary>
    /// Loads weather data from API
    /// </summary>
    private async Task LoadWeatherDataAsync()
    {
        try
        {
            var data = await _weatherService.GetWeatherWithForecastAsync(_config.Location, days: 3);
            UpdateWeather(data);
        }
        catch (Exception ex)
        {
            _locationLabel.Text = "Location: Error";
            _temperatureLabel.Text = "Temperature: --°F";
            _conditionLabel.Text = $"Error: {ex.Message}";
            _forecastLabel.Text = "Forecast: Unavailable";
            _lastUpdateLabel.Text = $"Last update: {DateTime.Now:HH:mm:ss}";
        }
    }

    /// <summary>
    /// Refreshes weather data from source
    /// </summary>
    public async Task RefreshAsync()
    {
        await LoadWeatherDataAsync();
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
