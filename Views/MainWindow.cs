using Terminal.Gui;
using DashboardTui.Components;
using DashboardTui.Plugins;
using DashboardTui.Services;
using DashboardTui.Models;
using System.Text.Json;

namespace DashboardTui.Views;

/// <summary>
/// Main dashboard window containing the primary UI layout
/// </summary>
public class MainWindow : Window
{
    private readonly Header _header;
    private readonly WeatherPlugin _weatherPlugin;
    private readonly NotificationsPlugin _notificationsPlugin;
    private readonly System.Threading.Timer? _refreshTimer;
    private readonly int _refreshIntervalMs;
    private readonly MenuBar _menuBar;
    private readonly StatusBar _statusBar;
    private readonly AppConfig _config;

    public MainWindow(IWeatherService weatherService, IWindowsNotificationService notificationService, AppConfig config)
    {
        _config = config;
        _refreshIntervalMs = config.MainWindow.UiRefreshIntervalSeconds * 1000;

        UpdateTitleWithTime();

        // Apply custom color scheme if specified
        ApplyColorScheme(config.MainWindow.Theme);

        // Create menu bar
        _menuBar = CreateMenuBar();

        // Create status bar
        _statusBar = CreateStatusBar();

        // Create and add the header
        _header = new("Dashboard", false)  // Don't show update time in header since it's in title bar
        {
            X = 0,
            Y = Pos.Bottom(_menuBar),
            Width = Dim.Fill()
            // Height is set by Header component (2 rows: title + underline)
        };

        // Create weather plugin with injected service
        _weatherPlugin = new(weatherService, config.Weather, SaveCurrentLocationIndex, config.MainWindow.TimeFormat)
        {
            X = 1,
            Y = Pos.Bottom(_header) + 1,
            Width = 50,
            Height = 8,
            CanFocus = true,
            TabStop = TabBehavior.TabStop
        };

        // Create notifications plugin with injected service
        _notificationsPlugin = new(notificationService, config.Notifications)
        {
            X = Pos.Right(_weatherPlugin) + 2,
            Y = Pos.Bottom(_header) + 1,
            Width = Dim.Fill()! - 1,
            Height = 10,
            CanFocus = true,
            TabStop = TabBehavior.TabStop
        };

        Add(_menuBar, _statusBar, _header, _weatherPlugin, _notificationsPlugin);

        // Ensure menu bar can receive focus and handle Alt key combinations
        _menuBar.WantMousePositionReports = true;

        // Set initial focus to weather plugin
        _weatherPlugin.SetFocus();

        // Add keyboard handler for weather navigation
        KeyDown += OnKeyDown;

        // Start UI refresh timer if interval is positive
        if (_refreshIntervalMs > 0)
        {
            _refreshTimer = new System.Threading.Timer(
                _ => RefreshUi(),
                null,
                _refreshIntervalMs,
                _refreshIntervalMs
            );
        }
    }

    /// <summary>
    /// Refreshes all UI components
    /// </summary>
    private void RefreshUi()
    {
        UpdateTitleWithTime();
    }

    /// <summary>
    /// Handles keyboard shortcuts for weather navigation
    /// </summary>
    private void OnKeyDown(object? sender, Key e)
    {
        // Left Arrow - Previous location
        if (e == Key.CursorLeft)
        {
            NavigateWeatherPrevious();
        }
        // Right Arrow - Next location
        else if (e == Key.CursorRight)
        {
            NavigateWeatherNext();
        }
    }

    /// <summary>
    /// Navigates to the previous weather location
    /// </summary>
    private void NavigateWeatherPrevious()
    {
        if (_config.Weather.Locations.Count > 1)
        {
            _config.Weather.CurrentLocationIndex = (_config.Weather.CurrentLocationIndex - 1 + _config.Weather.Locations.Count) % _config.Weather.Locations.Count;
            SaveCurrentLocationIndex();
            _ = _weatherPlugin.RefreshAsync();
        }
    }

    /// <summary>
    /// Navigates to the next weather location
    /// </summary>
    private void NavigateWeatherNext()
    {
        if (_config.Weather.Locations.Count > 1)
        {
            _config.Weather.CurrentLocationIndex = (_config.Weather.CurrentLocationIndex + 1) % _config.Weather.Locations.Count;
            SaveCurrentLocationIndex();
            _ = _weatherPlugin.RefreshAsync();
        }
    }

    /// <summary>
    /// Updates the window title with right-aligned timestamp
    /// </summary>
    private void UpdateTitleWithTime()
    {
        var baseTitle = $"Dashboard TUI ({Application.QuitKey} to quit)";
        if (_config.MainWindow.ShowLastUpdateInHeader)
        {
            var timeStr = _config.MainWindow.TimeFormat.ToLowerInvariant() == "12h"
                ? $"Updated: {DateTime.Now:h:mm tt}"
                : $"Updated: {DateTime.Now:HH:mm}";
            var totalWidth = Application.Driver?.Cols ?? 80;
            var spacing = totalWidth - baseTitle.Length - timeStr.Length - 4; // 4 for window border
            if (spacing > 1)
            {
                Title = $"{baseTitle}{new string(' ', spacing)}{timeStr}";
            }
            else
            {
                Title = baseTitle;
            }
        }
        else
        {
            Title = baseTitle;
        }
    }

    /// <summary>
    /// Creates the menu bar with Configuration menu and Themes submenu
    /// </summary>
    private MenuBar CreateMenuBar()
    {
        var menuBar = new MenuBar
        {
            Menus =
            [
                new MenuBarItem("_Configuration", new MenuItem[]
                {
                    new MenuBarItem("_Themes", new MenuItem[]
                    {
                        new("_Default", "Blue theme", () => ChangeTheme("Default"), null, null, Key.D.WithCtrl),
                        new("D_ark", "Dark theme", () => ChangeTheme("Dark"), null, null, Key.K.WithCtrl),
                        new("_Light", "Light theme", () => ChangeTheme("Light"), null, null, Key.L.WithCtrl),
                        new("_Green", "Terminal green theme", () => ChangeTheme("Green"), null, null, Key.G.WithCtrl)
                    }),
                    new MenuBarItem("_Weather", new MenuItem[]
                    {
                        new("_Refresh", "Refresh weather data", async () => await _weatherPlugin.RefreshAsync(), null, null, Key.R.WithCtrl),
                        new("_Add Location", "Add a new weather location", AddWeatherLocation, null, null, Key.A.WithCtrl.WithShift),
                        new("_Remove Location", "Remove current weather location", RemoveWeatherLocation, null, null, Key.D.WithCtrl.WithShift)
                    })
                }),
                new MenuBarItem("_Help", new MenuItem[]
                {
                    new("_About", "About Dashboard TUI", ShowAbout, null, null, Key.F1)
                })
            ]
        };

        return menuBar;
    }

    /// <summary>
    /// Creates the status bar with helpful key bindings
    /// </summary>
    private StatusBar CreateStatusBar()
    {
        return new StatusBar(
        [
            new Shortcut(Key.F9, "Menu", null),
            new Shortcut(Key.F1, "About", ShowAbout),
            new Shortcut(Key.R.WithCtrl, "Refresh", async () => await _weatherPlugin.RefreshAsync()),
            new Shortcut(Key.CursorLeft, "◀Weather", NavigateWeatherPrevious),
            new Shortcut(Key.CursorRight, "Weather▶", NavigateWeatherNext),
            new Shortcut(Key.Q.WithCtrl, "Quit", () => Application.RequestStop())
        ]);
    }

    /// <summary>
    /// Changes the application theme
    /// </summary>
    private void ChangeTheme(string theme)
    {
        ApplyColorScheme(theme);
        SaveThemeToConfig(theme);
        MessageBox.Query("Theme Changed", $"Theme changed to: {theme}\nSetting saved to appsettings.json", "OK");
    }

    /// <summary>
    /// Saves the selected theme to appsettings.json
    /// </summary>
    private void SaveThemeToConfig(string theme)
    {
        try
        {
            var configPath = "appsettings.json";
            var json = File.ReadAllText(configPath);
            var doc = JsonDocument.Parse(json);

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
            {
                writer.WriteStartObject();

                foreach (var property in doc.RootElement.EnumerateObject())
                {
                    if (property.Name == "MainWindow")
                    {
                        writer.WritePropertyName("MainWindow");
                        writer.WriteStartObject();

                        foreach (var mainWindowProp in property.Value.EnumerateObject())
                        {
                            if (mainWindowProp.Name == "Theme")
                            {
                                writer.WriteString("Theme", theme);
                            }
                            else
                            {
                                mainWindowProp.WriteTo(writer);
                            }
                        }

                        writer.WriteEndObject();
                    }
                    else
                    {
                        property.WriteTo(writer);
                    }
                }

                writer.WriteEndObject();
            }

            File.WriteAllText(configPath, System.Text.Encoding.UTF8.GetString(stream.ToArray()));
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Save Error", $"Failed to save theme: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Saves the current location index to appsettings.json
    /// </summary>
    private void SaveCurrentLocationIndex()
    {
        try
        {
            var configPath = "appsettings.json";
            var json = File.ReadAllText(configPath);
            var doc = JsonDocument.Parse(json);

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
            {
                writer.WriteStartObject();

                foreach (var property in doc.RootElement.EnumerateObject())
                {
                    if (property.Name == "Weather")
                    {
                        writer.WritePropertyName("Weather");
                        writer.WriteStartObject();

                        foreach (var weatherProp in property.Value.EnumerateObject())
                        {
                            if (weatherProp.Name == "CurrentLocationIndex")
                            {
                                writer.WriteNumber("CurrentLocationIndex", _config.Weather.CurrentLocationIndex);
                            }
                            else
                            {
                                weatherProp.WriteTo(writer);
                            }
                        }

                        writer.WriteEndObject();
                    }
                    else
                    {
                        property.WriteTo(writer);
                    }
                }

                writer.WriteEndObject();
            }

            File.WriteAllText(configPath, System.Text.Encoding.UTF8.GetString(stream.ToArray()));
        }
        catch
        {
            // Silently ignore save errors during navigation
        }
    }

    /// <summary>
    /// Saves the weather locations to appsettings.json
    /// </summary>
    private void SaveLocationsToConfig()
    {
        try
        {
            var configPath = "appsettings.json";
            var json = File.ReadAllText(configPath);
            var doc = JsonDocument.Parse(json);

            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
            {
                writer.WriteStartObject();

                foreach (var property in doc.RootElement.EnumerateObject())
                {
                    if (property.Name == "Weather")
                    {
                        writer.WritePropertyName("Weather");
                        writer.WriteStartObject();

                        foreach (var weatherProp in property.Value.EnumerateObject())
                        {
                            if (weatherProp.Name == "Locations")
                            {
                                writer.WritePropertyName("Locations");
                                writer.WriteStartArray();
                                foreach (var location in _config.Weather.Locations)
                                {
                                    writer.WriteStringValue(location);
                                }
                                writer.WriteEndArray();
                            }
                            else if (weatherProp.Name == "CurrentLocationIndex")
                            {
                                writer.WriteNumber("CurrentLocationIndex", _config.Weather.CurrentLocationIndex);
                            }
                            else
                            {
                                weatherProp.WriteTo(writer);
                            }
                        }

                        writer.WriteEndObject();
                    }
                    else
                    {
                        property.WriteTo(writer);
                    }
                }

                writer.WriteEndObject();
            }

            File.WriteAllText(configPath, System.Text.Encoding.UTF8.GetString(stream.ToArray()));
        }
        catch (Exception ex)
        {
            MessageBox.ErrorQuery("Save Error", $"Failed to save locations: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Saves the weather location to appsettings.json (legacy - now uses SaveLocationsToConfig)
    /// </summary>
    private void SaveLocationToConfig(string location)
    {
        _config.Weather.Locations = [location];
        _config.Weather.CurrentLocationIndex = 0;
        SaveLocationsToConfig();
    }

    /// <summary>
    /// Shows the About dialog
    /// </summary>
    private void ShowAbout()
    {
        MessageBox.Query("About Dashboard TUI",
            "📊 Dashboard TUI v0.0.1-alpha\n\n" +
            "A beautiful terminal dashboard built with Terminal.Gui\n\n" +
            "Hotkeys:\n" +
            "  Ctrl+D - Default Theme\n" +
            "  Ctrl+K - Dark Theme\n" +
            "  Ctrl+L - Light Theme\n" +
            "  Ctrl+G - Green Theme\n" +
            "  Ctrl+R - Refresh Weather\n" +
            "  ←/→ or ,/. - Navigate Locations\n" +
            "  Ctrl+Shift+A - Add Location\n" +
            "  Ctrl+Shift+D - Remove Location\n" +
            "  Ctrl+Q - Quit",
            "OK");
    }

    /// <summary>
    /// Adds a new weather location
    /// </summary>
    private void AddWeatherLocation()
    {
        var dialog = new Dialog
        {
            Title = "Add Weather Location",
            Width = 60,
            Height = 10
        };

        var label = new Label
        {
            X = 1,
            Y = 1,
            Text = "Enter location (city name, zip code, or 'auto:ip'):"
        };

        var locationField = new TextField
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill()! - 2,
            Text = ""
        };

        var okButton = new Button
        {
            Text = "OK",
            X = Pos.Center() - 10,
            Y = Pos.Bottom(locationField) + 2,
            IsDefault = true
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            X = Pos.Center() + 2,
            Y = Pos.Bottom(locationField) + 2
        };

        okButton.Accepting += async (s, e) =>
        {
            var newLocation = locationField.Text?.ToString()?.Trim();
            if (!string.IsNullOrEmpty(newLocation))
            {
                _config.Weather.Locations.Add(newLocation);
                _config.Weather.CurrentLocationIndex = _config.Weather.Locations.Count - 1;
                SaveLocationsToConfig();
                await _weatherPlugin.RefreshAsync();
                Application.RequestStop(dialog);
            }
        };

        cancelButton.Accepting += (s, e) =>
        {
            Application.RequestStop(dialog);
        };

        dialog.Add(label, locationField, okButton, cancelButton);
        Application.Run(dialog);
        dialog.Dispose();
    }

    /// <summary>
    /// Removes the current weather location
    /// </summary>
    private void RemoveWeatherLocation()
    {
        if (_config.Weather.Locations.Count <= 1)
        {
            MessageBox.ErrorQuery("Cannot Remove", "You must have at least one location configured.", "OK");
            return;
        }

        var currentLocation = _config.Weather.Locations[_config.Weather.CurrentLocationIndex];
        var result = MessageBox.Query("Remove Location",
            $"Remove location '{currentLocation}'?",
            "Yes", "No");

        if (result == 0) // Yes
        {
            _config.Weather.Locations.RemoveAt(_config.Weather.CurrentLocationIndex);
            if (_config.Weather.CurrentLocationIndex >= _config.Weather.Locations.Count)
            {
                _config.Weather.CurrentLocationIndex = _config.Weather.Locations.Count - 1;
            }
            SaveLocationsToConfig();
            _ = _weatherPlugin.RefreshAsync();
        }
    }

    /// <summary>
    /// Changes the weather location (legacy - now redirects to Add)
    /// </summary>
    private void ChangeWeatherLocation()
    {
        AddWeatherLocation();
    }

    /// <summary>
    /// Applies color scheme based on theme name
    /// </summary>
    private void ApplyColorScheme(string theme)
    {
        var colorScheme = theme.ToLowerInvariant() switch
        {
            "default" => new ColorScheme
            {
                Normal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Blue),
                Focus = new Terminal.Gui.Attribute(Color.Black, Color.Cyan),
                HotNormal = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Blue),
                HotFocus = new Terminal.Gui.Attribute(Color.BrightYellow, Color.Cyan),
                Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Blue)
            },
            "dark" => new ColorScheme
            {
                Normal = new Terminal.Gui.Attribute(Color.White, Color.Black),
                Focus = new Terminal.Gui.Attribute(Color.BrightYellow, Color.DarkGray),
                HotNormal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black),
                HotFocus = new Terminal.Gui.Attribute(Color.BrightBlue, Color.DarkGray),
                Disabled = new Terminal.Gui.Attribute(Color.Gray, Color.Black)
            },
            "light" => new ColorScheme
            {
                Normal = new Terminal.Gui.Attribute(Color.Black, Color.White),
                Focus = new Terminal.Gui.Attribute(Color.White, Color.Blue),
                HotNormal = new Terminal.Gui.Attribute(Color.Blue, Color.White),
                HotFocus = new Terminal.Gui.Attribute(Color.BrightBlue, Color.Cyan),
                Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.White)
            },
            "green" => new ColorScheme
            {
                Normal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black),
                Focus = new Terminal.Gui.Attribute(Color.Black, Color.Green),
                HotNormal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black),
                HotFocus = new Terminal.Gui.Attribute(Color.White, Color.Green),
                Disabled = new Terminal.Gui.Attribute(Color.DarkGray, Color.Black)
            },
            _ => null // Keep current
        };

        if (colorScheme != null)
        {
            ColorScheme = colorScheme;
        }
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
