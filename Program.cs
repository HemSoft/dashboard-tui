using Terminal.Gui;
using DashboardTui.Views;

// Initialize Terminal.Gui
Application.Init();

try
{
    // Run the main dashboard window
    Application.Run<MainWindow>().Dispose();
}
finally
{
    // Ensure clean shutdown
    Application.Shutdown();
}
