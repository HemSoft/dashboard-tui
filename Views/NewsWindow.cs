namespace DashboardTui.Views;

using Terminal.Gui;
using DashboardTui.Components;
using DashboardTui.Plugins;
using DashboardTui.Models;

/// <summary>
/// News window displaying news articles
/// </summary>
public class NewsWindow : Window
{
    private readonly Header _header;
    private readonly NewsPlugin _newsPlugin;
    private readonly StatusBar _statusBar;
    private readonly AppConfig _config;

    public NewsWindow(AppConfig config)
    {
        _config = config;

        UpdateTitleWithTime();

        // Apply custom color scheme if specified
        ApplyColorScheme(config.MainWindow.Theme);

        // Create and add the header with window name
        _header = new("News", false)  // Don't show update time in header since it's in title bar
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill()
        };

        // Create news plugin
        _newsPlugin = new()
        {
            X = 1,
            Y = Pos.Bottom(_header) + 1,
            Width = Dim.Fill()! - 1,
            Height = Dim.Fill()! - 1,
            CanFocus = true,
            TabStop = TabBehavior.TabStop
        };

        // Create status bar
        _statusBar = CreateStatusBar();

        Add(_header, _newsPlugin, _statusBar);

        // Set initial focus to news plugin
        _newsPlugin.SetFocus();
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
            var spacing = totalWidth - baseTitle.Length - timeStr.Length - 4;
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
    /// Creates the status bar with helpful key bindings
    /// </summary>
    private StatusBar CreateStatusBar()
    {
        return new StatusBar(
        [
            new Shortcut(Key.R.WithCtrl, "Refresh", async () => await _newsPlugin.RefreshAsync()),
            new Shortcut(Key.Q.WithCtrl, "Quit", () => Application.RequestStop())
        ]);
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
            _ => null
        };

        if (colorScheme != null)
        {
            ColorScheme = colorScheme;
        }
    }
}
