# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

QuickEditor is a C# WPF application that provides a professional pop-up text editor triggered by a global hotkey (Ctrl+Shift+F). The editor uses Monaco Editor (VS Code's editor component) via WebView2 and features smart positioning across multiple displays, full resizing capabilities, configurable transparency, and immediate keyboard focus.

## Build Commands

### Development
```bash
# restore packages and run
dotnet restore
dotnet run

# build only
dotnet build
```

### Publishing Single Executable
```bash
# Recommended: Framework-dependent (requires .NET 8 Runtime, ~40MB)
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true

# Alternative: Self-contained (no runtime required, ~200MB)
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true

# Output location: bin\Release\net8.0-windows\win-x64\publish\QuickEditor.exe
```

## Architecture

### Core Components

**MainWindow** (`MainWindow.xaml/.cs`)
- Hidden primary window that manages application lifecycle
- Registers global hotkey (Ctrl+Shift+F) via HotKeyManager
- Creates and shows EditorWindow instances when hotkey is pressed
- System tray integration with custom icon (QuickEditor.ico)
- Professional application icon for executable and system tray

**EditorWindow** (`EditorWindow.xaml/.cs`)
- Professional popup text editor window with Monaco Editor via WebView2
- Smart positioning: X-axis centered on current display, Y-axis 80% towards cursor from display center
- **Multi-display aware** - automatically detects which display contains the mouse cursor
- **Fully resizable** with custom resize handles on all edges and corners
- **Configurable transparency** via windowOpacity setting (0.0-1.0)
- **Immediate keyboard focus** - no click required after hotkey activation
- Features movable title bar with close button
- Handles keyboard shortcuts via JavaScript:
  - Ctrl+Enter: Copy text to clipboard and close
  - Esc: Show confirmation dialog before closing
- Dark themed with rounded corners and configurable transparency

**ConfirmDialog** (`ConfirmDialog.xaml/.cs`)
- Custom confirmation dialog matching editor's dark theme
- Compact size (220x100px) with rounded corners and transparency
- Prevents accidental editor closure when ESC is pressed
- Keyboard shortcuts: Enter/Space for Yes, ESC for No
- Owner window centering for proper positioning

**HotKeyManager** (`HotKeyManager.cs`)
- Win32 API wrapper for global hotkey registration
- Uses `user32.dll` RegisterHotKey/UnregisterHotKey
- Handles WM_HOTKEY messages via window message hooks

### Key Dependencies
- **Microsoft.Web.WebView2**: For hosting Monaco Editor in WPF
- **System.Text.Json**: For configuration file handling and WebView2 message parsing
- **System.Windows.Forms**: For cursor position and screen detection
- **.NET 8 Windows**: Target framework with WPF support
- **Monaco Editor**: VS Code's editor component (local files in wwwroot/)

## Development Notes

### UI Behavior
- Editor window is topmost with rounded corners and custom title bar
- Movable via title bar drag, closable via close button or Esc key (with confirmation)
- **Fully resizable window** with custom resize handles on all edges and corners (not just bottom-right)
- **Configurable transparency** - set windowOpacity in config.json (0.0 = transparent, 1.0 = opaque)
- **Smart multi-display positioning**:
  - X-axis: Always centered on the display containing mouse cursor
  - Y-axis: 80% of the way from display center towards cursor position
- Monaco Editor with Consolas font, 13pt size for optimal readability
- 600x300px default size, configurable via editorWidth/editorHeight
- Minimum size: 300x150px to ensure usability
- **Immediate keyboard focus** with comprehensive Windows API focus handling
- **Instant input ready** - no manual click required after hotkey activation
- **Accidental close prevention**: ESC key shows themed confirmation dialog
  - Dialog matches editor's dark theme and positioning
  - Quick keyboard interaction: Enter/Space for Yes, ESC for No
- WebView2 hosts Monaco Editor for professional VS Code-like editing experience

### Global Hotkey Integration
- Hotkey registration happens in MainWindow.Loaded event
- Uses Win32 message pump integration via HwndSource
- Single hotkey ID (9000) for the application
- Requires proper cleanup on application shutdown

### Monaco Editor Integration
- WebView2 hosts Monaco Editor (VS Code's editor component)
- JavaScript communication via `WebMessageReceived` event
- Keyboard shortcuts handled in JavaScript and posted to C#
- Supports all input methods (Japanese, Chinese, Korean, etc.) natively
- Professional code editing features with immediate input response

### Monaco Editor Configuration
- Plain text mode optimized for quick text editing
- Dark theme matching application design
- Disabled advanced features (minimap, line numbers, suggestions)
- Word wrap enabled for better text editing experience
- Auto-focus and keyboard shortcut handling via JavaScript

### Clipboard Operations
- Text automatically copied to clipboard on Ctrl+Enter
- No auto-save or persistence - purely transient editing

## Configuration System

The application uses a `config.json` file for configuration:

```json
{
  "enableLogging": false,
  "logFilePath": "quickeditor.log", 
  "logLevel": "Debug",
  "editorWidth": 600,
  "editorHeight": 300,
  "windowOpacity": 0.9,
  "enableConsoleLogging": false
}
```

### Configuration Options
- **enableLogging**: Enable/disable debug logging
- **logFilePath**: Path to log file (relative to executable)
- **logLevel**: Debug, Info, Warning, Error
- **editorWidth/Height**: Default editor window dimensions (pixels)
- **windowOpacity**: Window transparency (0.0 = fully transparent, 1.0 = fully opaque)
- **enableConsoleLogging**: Enable/disable JavaScript console logging in Monaco Editor (default: false)

### Debugging
- **Logging is disabled by default** for production use
- Enable debug logging by setting `enableLogging: true` and `logLevel: "Debug"` in config.json
- Check `quickeditor.log` for detailed operation logs (located in same directory as executable)
- When running with `dotnet run`: log file is in `bin/Debug/net8.0-windows/quickeditor.log`
- When running published exe: log file is in same directory as the exe
- Logs include hotkey registration, window creation, and user actions
- Log file path is shown in the log file itself on startup

## Common Development Tasks

### Debugging Hotkey Issues
1. Enable debug logging in config.json
2. Check log file for hotkey registration success/failure
3. Verify window handle creation in logs
4. Look for Win32 error codes if registration fails
5. Common Win32 error 1409 means hotkey is already registered by another instance

### Current Hotkey Configuration
- Uses `Ctrl+Shift+F` for reliable cross-keyboard compatibility
- Standard letter keys work consistently across all keyboard layouts
- Single instance checking prevents multiple registrations

### Modifying Hotkey
Change the hotkey in `MainWindow.xaml.cs`:
```csharp
// Current implementation (reliable)
_hotKeyManager.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Shift, Key.F, OnHotKeyPressed);

// Alternative examples
_hotKeyManager.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Shift, Key.J, OnHotKeyPressed);
_hotKeyManager.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt, Key.Q, OnHotKeyPressed);
```

### Adjusting Editor Appearance
- Modify `config.json` for:
  - Window size: `editorWidth` and `editorHeight` (pixels)
  - Transparency: `windowOpacity` (0.0-1.0)
- Edit `EditorWindow.xaml` for visual properties:
  - Window background: `Background="#FF1E1E1E"` in Border element
  - Title bar: Modify Border with `Background="#FF333333"`
- Monaco Editor theme: Edit `editor.html` to modify `transparent-dark` theme definition
- Font settings: Change `fontSize` and `fontFamily` in Monaco Editor configuration

### Adding Editor Features
- Extend JavaScript keyboard shortcuts in `editor.html`
- Add new WebView2 message types in `EditorWindow.xaml.cs` WebMessageReceived handler
- Modify Monaco Editor configuration in `editor.html` for editor behavior
- Window positioning: Edit `ShowEditorAtCursorCenter()` method for custom positioning logic
- Resize behavior: Modify resize handle configuration in `EditorWindow.xaml`

### Focus and Performance Optimization
- Focus handling uses multiple strategies: Windows API calls, WebView2 focus, and Monaco Editor focus
- Resize handles use Windows API `SendMessage` for native OS resize behavior
- Multi-display support via `System.Windows.Forms.Screen.FromPoint()` for accurate positioning

### Confirmation Dialog Customization
- **ConfirmDialog** provides themed confirmation for ESC key presses
- Modify `ConfirmDialog.xaml` for visual changes:
  - Size: Adjust `Width` and `Height` properties (default: 250x100)
  - Colors: Match `Background="#FF1E1E1E"` and `Foreground="#CCC"` with editor theme
  - Button styling: Custom hover effects in XAML button styles
- Keyboard handling in `ConfirmDialog.xaml.cs`:
  - `KeyDown` event handles Enter/Space for Yes action
  - `IsDefault="True"` on Yes button for Enter key
  - `IsCancel="True"` on No button for ESC key
- Message customization via `ConfirmDialog.ShowConfirmation()` static method