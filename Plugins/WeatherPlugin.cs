namespace DashboardTui.Plugins;

using Terminal.Gui;

/// <summary>
/// Weather plugin component displaying current weather information
/// </summary>
public class WeatherPlugin : FrameView
{
    private readonly Label _temperatureLabel;
    private readonly Label _conditionLabel;
    private readonly Label _locationLabel;
    private readonly Label _lastUpdateLabel;
    
    public WeatherPlugin()
    {
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
            Text = "Temperature: --°C"
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
        
        // Last update timestamp
        _lastUpdateLabel = new()
        {
            X = 1,
            Y = Pos.Bottom(_conditionLabel) + 1,
            Height = 1,
            Width = Dim.Fill(),
            Text = "Last update: Never"
        };
        
        Add(_locationLabel, _temperatureLabel, _conditionLabel, _lastUpdateLabel);
        
        // Load initial demo data
        LoadWeatherData();
    }
    
    /// <summary>
    /// Updates weather data display
    /// </summary>
    public void UpdateWeather(string location, double temperature, string condition)
    {
        _locationLabel.Text = $"Location: {location}";
        _temperatureLabel.Text = $"Temperature: {temperature:F1}°C";
        _conditionLabel.Text = $"Condition: {condition}";
        _lastUpdateLabel.Text = $"Last update: {DateTime.Now:HH:mm:ss}";
    }
    
    /// <summary>
    /// Loads demo weather data
    /// </summary>
    private void LoadWeatherData()
    {
        // Demo data - in a real implementation, this would call a weather API
        UpdateWeather("San Francisco, CA", 18.5, "Partly Cloudy");
    }
    
    /// <summary>
    /// Refreshes weather data from source
    /// </summary>
    public void Refresh()
    {
        LoadWeatherData();
    }
}
