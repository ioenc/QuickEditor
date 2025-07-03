# QuickEditor

A minimal pop-up text editor that appears at your mouse cursor with a global hotkey.

## 🔧 Overview

**QuickEditor** is a lightweight, always-on-top text editor designed to appear at the center of your mouse cursor when you press a hotkey.  
It's useful when you want to quickly write multi-line text (e.g., for pasting into single-line inputs like chat boxes).

---

## 🖥 Requirements

- Windows 10 or 11
- [.NET 8 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (for pre-built releases)
  - Download **Desktop Runtime** (not ASP.NET Core)
  - The application will notify you if runtime is missing
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) (for building from source)

---

## ⚙ Features

### Core Functionality
- **Global hotkey**: `Ctrl + Shift + F` to trigger the editor instantly
- **Professional text editing** with Monaco Editor (VS Code's editor component)
- **Multi-line text editing** with syntax highlighting capabilities
- **Clipboard integration**: `Ctrl + Enter` to copy text and close, `Esc` to show confirmation dialog before closing

### Smart Positioning & Multi-Display Support
- **Multi-display aware**: Automatically detects which display contains the mouse cursor
- **Smart positioning**: 
  - X-axis: Always centers on the current display
  - Y-axis: Positions 80% of the way from display center towards cursor
- **No manual positioning required**: Works seamlessly across different screen configurations

### Window Management
- **Fully resizable window** with custom resize handles on all edges and corners
- **Configurable transparency**: Set window opacity in config.json (0.0 = transparent, 1.0 = opaque)
- **Movable via title bar** with custom close button
- **Always on top**: Stays above other windows for quick access
- **Minimum size**: 300x150px to ensure usability

### User Experience
- **Immediate keyboard focus**: No click required after hotkey activation
- **Instant input ready**: Advanced Windows API focus handling ensures immediate typing
- **International input support**: Full IME support for Japanese, Chinese, Korean, and other languages
- **Accidental close prevention**: ESC key shows confirmation dialog
  - Confirm with Yes/No buttons or Enter/Space keys for quick interaction
  - Prevents accidental loss of work when typing quickly
- **System tray integration**: Runs quietly in background with custom icon and minimal system impact
- **Custom branding**: Professional application icon for both executable and system tray

---

## 🚀 Getting Started

### 1. Build and Run

```bash
dotnet restore
dotnet run
```

Or simply:
```bash
dotnet run
```

---

### 2. Create Single Executable

To build a release version (requires .NET 8 Runtime on target machine):

```bash
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
```

Output (~40MB):

```
bin\Release\net8.0-windows\win-x64\publish\QuickEditor.exe
```

**Alternative: Fully Self-Contained** (larger size, no runtime required):
```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

---

## ⚙️ Configuration

QuickEditor creates a `config.json` file in the application directory with the following settings:

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

- **enableLogging**: Enable/disable debug logging for troubleshooting
- **logFilePath**: Path to log file (relative to executable directory)
- **logLevel**: Logging level (Debug, Info, Warning, Error)
- **editorWidth**: Initial window width in pixels (default: 600)
- **editorHeight**: Initial window height in pixels (default: 300)  
- **windowOpacity**: Window transparency level (default: 0.9)
  - `1.0`: Fully opaque (no transparency)
  - `0.9`: Recommended setting (slight transparency)
  - `0.7`: Semi-transparent for overlay work
  - `0.5`: High transparency for background reference
  - `0.0`: Fully transparent (not recommended for daily use)
- **enableConsoleLogging**: Enable/disable JavaScript console logging in Monaco Editor (default: false)

### Customization Examples

**High Performance Setup** (fully opaque, larger size):
```json
{
  "editorWidth": 800,
  "editorHeight": 400,
  "windowOpacity": 1.0
}
```

**Overlay Mode** (semi-transparent for working over other windows):
```json
{
  "editorWidth": 500,
  "editorHeight": 250,
  "windowOpacity": 0.7
}
```

**Debug Mode** (enable logging for troubleshooting):
```json
{
  "enableLogging": true,
  "logLevel": "Debug",
  "enableConsoleLogging": true
}
```

Changes take effect after restarting the application.

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

### Third-Party Components

- **Monaco Editor**: Licensed under MIT License - Copyright (c) Microsoft Corporation
- **WebView2**: Microsoft WebView2 control for embedding web content
