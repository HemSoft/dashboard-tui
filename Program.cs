using System.Text.Json;
using Terminal.Gui;
using DashboardTui.Models;
using DashboardTui.Services;
using DashboardTui.Views;

// Load configuration
var configJson = File.ReadAllText("appsettings.json");
var config = JsonSerializer.Deserialize<AppConfig>(configJson) ?? throw new InvalidOperationException("Failed to load configuration");

// Initialize services
var httpClient = new HttpClient();
var weatherService = new WeatherApiService(httpClient, config.Weather.ApiKey);

// Initialize Terminal.Gui with NetDriver for better emoji support
Application.Init(driverName: "NetDriver");

try
{
    // Create and run the main dashboard window with dependencies
    var mainWindow = new MainWindow(weatherService, config);
    Application.Run(mainWindow);
    mainWindow.Dispose();
}
finally
{
    // Cleanup
    httpClient.Dispose();
    Application.Shutdown();
}
