using Terminal.Gui;

namespace DashboardTui.Components;

/// <summary>
/// Header component displaying application name, version, and last update time with underline
/// </summary>
public class Header : View
{
    private const string AppName = "\U0001F4CA Dashboard TUI"; // 📊 Dashboard TUI
    private const string Version = "v0.0.1-alpha";
    private const int UpdateLabelWidth = 18; // "Updated: HH:mm:ss" = 18 chars

    private readonly Label _titleLabel;
    private readonly Label _updateLabel;
    private readonly Line _underline;
    private readonly bool _showLastUpdate;

    public Header(bool showLastUpdate = true)
    {
        _showLastUpdate = showLastUpdate;

        _titleLabel = new()
        {
            Text = $"{AppName} {Version}",
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

        Add(_titleLabel, _underline);

        if (_showLastUpdate)
        {
            Add(_updateLabel);
            UpdateLastRefreshTime();
        }

        Height = 2; // Title + underline
        Width = Dim.Fill();
    }

    /// <summary>
    /// Updates the displayed version
    /// </summary>
    public void UpdateVersion(string version) =>
        _titleLabel.Text = $"{AppName} {version}";

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
