using Terminal.Gui;

namespace DashboardTui.Components;

/// <summary>
/// Header component displaying application name, version, window name, and last update time with underline
/// </summary>
public class Header : View
{
    private const string AppName = "\U0001F4CA Dashboard TUI"; // 📊 Dashboard TUI
    private const string Version = "v0.0.1-alpha";
    private const int UpdateLabelWidth = 18; // "Updated: HH:mm:ss" = 18 chars

    private readonly Label _appLabel;
    private readonly Label _windowLabel;
    private readonly Label _updateLabel;
    private readonly Line _underline;
    private readonly bool _showLastUpdate;

    public Header(string windowName = "Dashboard", bool showLastUpdate = true)
    {
        _showLastUpdate = showLastUpdate;

        _appLabel = new()
        {
            Text = $"{AppName} {Version}",
            X = 0,
            Y = 0,
            Height = 1,
            Width = Dim.Auto()
        };

        _windowLabel = new()
        {
            Text = windowName,
            X = Pos.Center(),
            Y = 0,
            Height = 1,
            Width = Dim.Auto()
        };

        _updateLabel = new()
        {
            Text = "",
            X = Pos.AnchorEnd() - (UpdateLabelWidth + 1), // 1 space from right edge
            Y = 0,
            Height = 1,
            Width = UpdateLabelWidth,
            TextAlignment = Alignment.End
        };

        _underline = new()
        {
            Orientation = Orientation.Horizontal,
            X = 0,
            Y = 1,
            Width = Dim.Fill()
        };

        Add(_appLabel, _windowLabel, _underline);

        if (_showLastUpdate)
        {
            Add(_updateLabel);
            UpdateLastRefreshTime();
        }

        Height = 2; // Title + underline
        Width = Dim.Fill();
    }

    /// <summary>
    /// Updates the displayed window name
    /// </summary>
    public void UpdateWindowName(string windowName) =>
        _windowLabel.Text = windowName;

    /// <summary>
    /// Updates the displayed version
    /// </summary>
    public void UpdateVersion(string version) =>
        _appLabel.Text = $"{AppName} {version}";

    /// <summary>
    /// Updates the last refresh timestamp
    /// </summary>
    public void UpdateLastRefreshTime()
    {
        if (_showLastUpdate)
        {
            _updateLabel.Text = $"Updated: {DateTime.Now:HH:mm:ss}";
        }
    }
}
