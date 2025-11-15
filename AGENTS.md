# Dashboard TUI - Agent Documentation

## ⚠️ CRITICAL: Testing Protocol
**NEVER run the application with `dotnet run` - User handles all testing.**
After making changes:
1. Build with `dotnet build` only
2. Check for unused using directives
3. Ask user what to test/verify
4. Wait for user feedback before proceeding

## ⚠️ CRITICAL: PowerShell Commands
**DO NOT USE THIS COMMAND - IT CRASHES VSCODE:**
```powershell
Get-Process | Where-Object {$_.ProcessName -eq 'dashboard-tui'} | Stop-Process -Force; dotnet build
```
Instead, run commands separately or manually stop the application before building.

## ⚠️ CRITICAL: Documentation Policy
**DO NOT create markdown files without explicit permission from the user.**
This includes summary files, documentation files, or any .md files. Only modify existing markdown files when necessary.

---

## Terminal.Gui Technical Reference

### Core Concepts

#### Application Lifecycle
- `Application.Init()` - Initialize the terminal application
- `Application.Run(Toplevel)` - Run the main event loop with a top-level view
- `Application.Shutdown()` - Clean shutdown, restore terminal state
- Must call `Dispose()` on Toplevel views after `Run()` returns

#### View Hierarchy
- `Toplevel` - Base class for top-level views (Window, Dialog, etc.)
- `Window` - Container with border and title support
- `View` - Base class for all UI elements
- Views contain child views via `Add()` method

#### Layout System
##### Absolute Positioning
```csharp
X = 5          // Column 5
Y = 10         // Row 10
Width = 20     // 20 columns wide
Height = 5     // 5 rows tall
```

##### Relative Positioning (Pos)
```csharp
X = Pos.Center()                 // Centered horizontally
X = Pos.Right(otherView) + 1     // 1 column right of otherView
Y = Pos.Bottom(otherView) + 2    // 2 rows below otherView
X = Pos.Left(otherView)          // Align left with otherView
Y = Pos.Top(otherView)           // Align top with otherView
X = Pos.AnchorEnd()              // Anchor to right/bottom edge
```

##### Relative Sizing (Dim)
```csharp
Width = Dim.Fill()               // Fill remaining space
Height = Dim.Fill() - 1          // Fill minus 1 row
Width = Dim.Auto()               // Size to content
Height = Dim.Percent(50)         // 50% of parent
```

#### Common Views
- `Label` - Static text display with `Text` property
- `Button` - Clickable button with `Accepting` event
- `TextField` - Single-line text input, `Secret = true` for passwords
- `TextView` - Multi-line text input/display
- `ListView` - Scrollable list with `SetSource()` method
- `MenuBar` - Top menu bar with `MenuBarItem[]`
- `StatusBar` - Bottom status bar with `StatusItem[]`
- `FrameView` - Bordered container with `Title` property
- `Dialog` - Modal dialog window
- `MessageBox` - Quick message/question dialogs

#### Events
- `Accepting` - User confirms/accepts (Enter key, button click)
- `KeyDown` / `KeyUp` - Keyboard events
- `MouseClick` - Mouse events

#### Colors & Themes
- `ThemeManager.Theme` - Set theme by name
- `ThemeManager.GetThemeNames()` - Get available themes
- Default themes: "Default", "Light", "Dark", "Anders", etc.

#### Properties
- `Title` - Window/View title
- `Text` - Label/Button text
- `Enabled` - Enable/disable interaction
- `Visible` - Show/hide view
- `CanFocus` - Can receive keyboard focus
- `TabStop` - Include in tab navigation

### API Patterns

#### Basic Application
```csharp
Application.Init();
var window = new Window { Title = "App Name" };
// Add views to window
Application.Run(window);
window.Dispose();
Application.Shutdown();
```

#### Generic Toplevel
```csharp
Application.Run<MyWindow>().Dispose();
Application.Shutdown();
```

#### Quit Key
- Default: `Application.QuitKey` (typically Ctrl+Q)
- `Application.RequestStop()` - Programmatic quit

---

## Project Architecture

### Version
- v1.0.0

### Structure
```
dashboard-tui/
├── Program.cs           # Entry point, Application.Init/Run/Shutdown
├── Views/
│   └── MainWindow.cs    # Main dashboard window
├── Components/
│   └── Header.cs        # Centered header component
├── Plugins/
│   └── WeatherPlugin.cs # Weather data display plugin
├── AGENTS.md            # This file
└── README.md            # User documentation
```

### Design Decisions

#### MainWindow
- Toplevel window container
- Full-screen layout
- Contains Header component at top
- Hosts plugin components (Weather, etc.)

#### Header Component
- View-based component for reusability
- Displays application name and version
- Centered horizontally using `Pos.Center()`
- Positioned at top (Y = 0)
- Read-only display element

#### Weather Plugin
- FrameView-based plugin with border and title
- Displays location, temperature, condition, and last update time
- Self-contained component with data refresh capability
- Demo data implementation (can be extended to real API)

---

## Implementation Status

### Phase 1 - Foundation ✓
- [x] Terminal.Gui v2.0.0 installed
- [x] Project structure defined
- [x] Documentation created

### Phase 2 - Main Display ✓
- [x] Create MainWindow class
- [x] Create Header component with emoji
- [x] Implement centered layout
- [x] Display app name and version
- [x] Wire up in Program.cs
- [x] Build verification successful
- [x] Create Weather plugin component
- [x] Integrate Weather plugin into MainWindow
- [x] Add emoji support (NetDriver)
- [x] Weather condition emojis (20+ conditions mapped)
- [x] 3-day forecast display (compact single-line)
- [x] Theme system (Default, Dark, Light, Green)
- [x] Menu bar with Configuration, Weather, Help menus
- [x] Status bar with F9, F1, Ctrl+R, Ctrl+Q shortcuts
- [x] Theme persistence to appsettings.json
- [x] Weather location change dialog
- [x] Weather location persistence to appsettings.json

### Phase 3 - Future
- [ ] Add dashboard panels
- [ ] Implement data visualization
