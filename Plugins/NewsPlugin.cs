using Terminal.Gui;

namespace DashboardTui.Plugins;

/// <summary>
/// News plugin displaying recent news articles
/// </summary>
public class NewsPlugin : FrameView
{
    private readonly Label _placeholderLabel;

    public NewsPlugin()
    {
        Title = "\U0001F4F0 News"; // 📰 News
        BorderStyle = LineStyle.Rounded;

        _placeholderLabel = new()
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Text = "News plugin - implementation pending",
            Width = Dim.Auto(),
            Height = 1,
            TextAlignment = Alignment.Center
        };

        Add(_placeholderLabel);
    }

    /// <summary>
    /// Refreshes news data
    /// </summary>
    public async Task RefreshAsync()
    {
        // TODO: Implement news data fetching
        await Task.CompletedTask;
    }
}
