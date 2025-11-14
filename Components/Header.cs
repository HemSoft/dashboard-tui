using Terminal.Gui;

namespace DashboardTui.Components;

/// <summary>
/// Header component displaying application name and version centered at the top
/// </summary>
public class Header : View
{
    private const string AppName = "Dashboard TUI";
    private const string Version = "v0.0.1-alpha";

    private readonly Label _titleLabel = new()
    {
        Text = $"{AppName} {Version}",
        X = Pos.Center(),
        Y = 0,
        Height = 1,
        Width = Dim.Auto()
    };

    public Header()
    {
        Add(_titleLabel);
        Height = 1;
        Width = Dim.Fill();
    }

    /// <summary>
    /// Updates the displayed version
    /// </summary>
    public void UpdateVersion(string version) =>
        _titleLabel.Text = $"{AppName} {version}";
}
