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
var notificationService = new WindowsNotificationService();

// Initialize Terminal.Gui with NetDriver for better emoji support
Application.Init(driverName: "NetDriver");

try
{
    // Window navigation state
    var windows = new Func<Window>[]
    {
        () => new MainWindow(weatherService, notificationService, config),
        () => new NewsWindow(config),
        () => new PullRequestsWindow(config)
    };

    var currentWindowIndex = 0;
    var shouldContinue = true;
    Window? currentWindow = null;

    // Function to switch windows
    void SwitchToWindow(int index)
    {
        if (index < 0 || index >= windows.Length)
            return;

        currentWindowIndex = index;

        // Stop current window if running
        if (currentWindow != null)
        {
            Application.RequestStop();
        }
    }

    // Function to quit application
    void QuitApplication()
    {
        shouldContinue = false;
        Application.RequestStop();
    }

    // Main window loop
    while (shouldContinue)
    {
        // Create current window
        currentWindow = windows[currentWindowIndex]();

        // Add global key handler for window navigation
        currentWindow.KeyDown += (sender, e) =>
        {
            // Plus (+) - Next window
            if (e == (Key)'+')
            {
                var nextIndex = (currentWindowIndex + 1) % windows.Length;
                SwitchToWindow(nextIndex);
                e.Handled = true;
            }
            // Minus (-) - Previous window
            else if (e == (Key)'-')
            {
                var prevIndex = (currentWindowIndex - 1 + windows.Length) % windows.Length;
                SwitchToWindow(prevIndex);
                e.Handled = true;
            }
            // Ctrl+Q - Quit application
            else if (e == Key.Q.WithCtrl)
            {
                QuitApplication();
                e.Handled = true;
            }
            // Esc - Quit application
            else if (e == Key.Esc)
            {
                QuitApplication();
                e.Handled = true;
            }
        };

        Application.Run(currentWindow);
        currentWindow.Dispose();
    }
}
finally
{
    // Cleanup
    httpClient.Dispose();
    Application.Shutdown();
}
