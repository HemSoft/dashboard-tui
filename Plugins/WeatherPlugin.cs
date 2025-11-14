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
    private readonly Button _prevButton;
    private readonly Button _nextButton;
    private readonly IWeatherService _weatherService;
    private readonly WeatherConfig _config;
    private readonly System.Threading.Timer? _refreshTimer;
    private readonly Action? _onLocationIndexChanged;

    public WeatherPlugin(IWeatherService weatherService, WeatherConfig config, Action? onLocationIndexChanged = null)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _onLocationIndexChanged = onLocationIndexChanged;

        Title = "Weather";
        BorderStyle = LineStyle.Single;

        // Location
        _locationLabel = new()
        {
            X = 1,
            Y = 0,
            Height = 1,
            Width = Dim.Fill()! - 10,
            Text = "Location: Loading..."
        };

        // Previous location button
        _prevButton = new()
        {
            X = Pos.AnchorEnd() - 7,
            Y = 0,
            Text = "◀",
            Width = 3
        };
        _prevButton.Accepting += async (s, e) =>
        {
            if (_config.Locations.Count > 1)
            {
                _config.CurrentLocationIndex = (_config.CurrentLocationIndex - 1 + _config.Locations.Count) % _config.Locations.Count;
                _onLocationIndexChanged?.Invoke();
                await LoadWeatherDataAsync();
            }
        };

        // Next location button
        _nextButton = new()
        {
            X = Pos.AnchorEnd() - 3,
            Y = 0,
            Text = "▶",
            Width = 3
        };
        _nextButton.Accepting += async (s, e) =>
        {
            if (_config.Locations.Count > 1)
            {
                _config.CurrentLocationIndex = (_config.CurrentLocationIndex + 1) % _config.Locations.Count;
                _onLocationIndexChanged?.Invoke();
                await LoadWeatherDataAsync();
            }
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

        Add(_prevButton, _nextButton, _locationLabel, _temperatureLabel, _conditionLabel, _forecastLabel, _lastUpdateLabel);

        // Update button visibility
        UpdateNavigationButtons();

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
        var locationIndex = _config.Locations.Count > 1 ? $" ({_config.CurrentLocationIndex + 1}/{_config.Locations.Count})" : "";
        _locationLabel.Text = $"{data.Location}{locationIndex}";
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
        UpdateNavigationButtons();
    }

    /// <summary>
    /// Updates navigation button visibility based on location count
    /// </summary>
    private void UpdateNavigationButtons()
    {
        var hasMultipleLocations = _config.Locations.Count > 1;
        _prevButton.Visible = hasMultipleLocations;
        _nextButton.Visible = hasMultipleLocations;
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
            if (_config.Locations.Count == 0)
            {
                _locationLabel.Text = "No locations configured";
                _temperatureLabel.Text = "Temperature: --°F";
                _conditionLabel.Text = "Add a location to see weather";
                _forecastLabel.Text = "Forecast: Unavailable";
                _lastUpdateLabel.Text = $"Last update: {DateTime.Now:HH:mm:ss}";
                return;
            }

            // Ensure current index is valid
            if (_config.CurrentLocationIndex < 0 || _config.CurrentLocationIndex >= _config.Locations.Count)
            {
                _config.CurrentLocationIndex = 0;
            }

            var currentLocation = _config.Locations[_config.CurrentLocationIndex];
            var data = await _weatherService.GetWeatherWithForecastAsync(currentLocation, days: 3);
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
