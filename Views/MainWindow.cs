using Terminal.Gui;
using DashboardTui.Components;
using DashboardTui.Plugins;

namespace DashboardTui.Views;

/// <summary>
/// Main dashboard window containing the primary UI layout
/// </summary>
public class MainWindow : Window
{
    public MainWindow()
    {
        Title = $"Dashboard TUI ({Application.QuitKey} to quit)";
        
        // Create and add the centered header with target-typed new
        Header header = new()
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = 1
        };

        // Create weather plugin
        WeatherPlugin weatherPlugin = new()
        {
            X = 1,
            Y = Pos.Bottom(header) + 1,
            Width = 40,
            Height = 8
        };

        Add(header, weatherPlugin);
    }
}
