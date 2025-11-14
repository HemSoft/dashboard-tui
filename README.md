# Dashboard TUI

A beautiful terminal-based dashboard application built with Terminal.Gui for .NET 10.

## Overview

Dashboard TUI is a modern, cross-platform terminal user interface application that provides an elegant dashboard experience directly in your terminal. Built with the powerful Terminal.Gui library, it runs seamlessly on Windows, macOS, and Linux.

## Features

### Current
- **Elegant Header Display** - Centered application name and version at the top of the screen
- **Weather Plugin** - Real-time weather information display with location, temperature, and conditions
- **Full Terminal UI** - Responsive interface that adapts to your terminal size
- **Cross-Platform** - Works on Windows, macOS, and Linux

### Planned
- Real-time data visualization panels
- Configurable themes and color schemes
- Interactive menu system
- Status monitoring and alerts

## Requirements

- .NET 10 SDK
- Terminal with Unicode support (Windows Terminal, iTerm2, etc.)

## Installation

```bash
git clone <repository-url>
cd dashboard-tui
dotnet restore
dotnet build
```

## Usage

Run the application:

```bash
dotnet run
```

### Keyboard Shortcuts

- **Ctrl+Q** - Quit the application
- **Tab** - Navigate between UI elements
- **Enter** - Select/Activate focused element

## Technology Stack

- **.NET 10** - Latest .NET framework with C# 14 features
- **Terminal.Gui v2.0.0** - Cross-platform terminal UI toolkit
- **PowerShell** - Command-line automation (Windows)

## Project Structure

```
dashboard-tui/
├── Program.cs              # Application entry point
├── Views/
│   └── MainWindow.cs       # Main dashboard window
├── Components/
│   └── Header.cs           # Header display component
├── Plugins/
│   └── WeatherPlugin.cs    # Weather data plugin
├── dashboard-tui.csproj    # Project configuration
├── AGENTS.md               # Technical/agent documentation
└── README.md               # This file
```

## Development

This project follows modern C# best practices:
- C# 14 language features
- .NET 10 framework
- Minimal, clean architecture
- Component-based UI structure

## Version

Current version: **1.0.0**

## License

[Add your license here]

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

## Support

For issues and questions, please open an issue on the project repository.
