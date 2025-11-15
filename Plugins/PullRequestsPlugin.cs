using Terminal.Gui;

namespace DashboardTui.Plugins;

/// <summary>
/// Pull Requests plugin displaying GitHub PRs
/// </summary>
public class PullRequestsPlugin : FrameView
{
    private readonly Label _placeholderLabel;

    public PullRequestsPlugin()
    {
        Title = "\U0001F504 Pull Requests"; // 🔄 Pull Requests
        BorderStyle = LineStyle.Rounded;

        _placeholderLabel = new()
        {
            X = Pos.Center(),
            Y = Pos.Center(),
            Text = "Pull Requests plugin - implementation pending",
            Width = Dim.Auto(),
            Height = 1,
            TextAlignment = Alignment.Center
        };

        Add(_placeholderLabel);
    }

    /// <summary>
    /// Refreshes pull request data
    /// </summary>
    public async Task RefreshAsync()
    {
        // TODO: Implement GitHub PR fetching
        await Task.CompletedTask;
    }
}
