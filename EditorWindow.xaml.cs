using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Web.WebView2.Core;

namespace QuickEditor
{
    public partial class EditorWindow : Window
    {
        private Config _config;
        private bool _isWebViewReady = false;

        // Windows API for proper focus handling
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();
        
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        
        private const int SW_SHOW = 5;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;
        private const int HTTOP = 0xC;
        private const int HTBOTTOM = 0xF;
        private const int HTLEFT = 0xA;
        private const int HTRIGHT = 0xB;
        private const int HTTOPLEFT = 0xD;
        private const int HTTOPRIGHT = 0xE;
        private const int HTBOTTOMLEFT = 0x10;
        private const int HTBOTTOMRIGHT = 0x11;

        public EditorWindow(Config config)
        {
            _config = config;
            InitializeComponent();
            
            ApplyConfiguration();
            SetupWebViewInitialization();
        }

        /// <summary>
        /// Apply configuration settings to the window
        /// </summary>
        private void ApplyConfiguration()
        {
            Width = _config.EditorWidth;
            Height = _config.EditorHeight;
            Opacity = _config.WindowOpacity;
            
            Logger.Debug($"EditorWindow created with size: {Width}x{Height}, opacity: {Opacity}");
        }

        /// <summary>
        /// Set up WebView2 initialization when window loads
        /// </summary>
        private void SetupWebViewInitialization()
        {
            Loaded += async (s, e) =>
            {
                try
                {
                    await InitializeWebView();
                    SetupWebMessageHandling();
                    LoadMonacoEditor();
                }
                catch (Exception ex)
                {
                    Logger.Error($"Failed to initialize Monaco Editor: {ex.Message}");
                }
            };
        }

        /// <summary>
        /// Initialize WebView2 control with basic settings
        /// </summary>
        private async Task InitializeWebView()
        {
            await WebViewEditor.EnsureCoreWebView2Async(null);
            WebViewEditor.DefaultBackgroundColor = System.Drawing.Color.Transparent;
            Logger.Debug("WebView2 initialized successfully");
        }

        /// <summary>
        /// Set up handling of messages from Monaco Editor
        /// </summary>
        private void SetupWebMessageHandling()
        {
            WebViewEditor.CoreWebView2.WebMessageReceived += HandleWebMessage;
        }

        /// <summary>
        /// Handle messages received from Monaco Editor JavaScript
        /// </summary>
        private void HandleWebMessage(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            try
            {
                var rawMessage = args.TryGetWebMessageAsString();
                Logger.Debug($"Received web message: {rawMessage}");
                
                if (string.IsNullOrEmpty(rawMessage))
                {
                    Logger.Warning("Received empty web message");
                    return;
                }
                
                ProcessWebMessage(rawMessage);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error handling web message: {ex.Message}");
                Logger.Error($"Exception details: {ex}");
            }
        }

        /// <summary>
        /// Process parsed web message and perform corresponding actions
        /// </summary>
        private void ProcessWebMessage(string rawMessage)
        {
            using var document = System.Text.Json.JsonDocument.Parse(rawMessage);
            var root = document.RootElement;
            
            if (!root.TryGetProperty("type", out var typeProperty))
            {
                Logger.Warning("Web message missing 'type' property");
                return;
            }
            
            var messageType = typeProperty.GetString();
            Logger.Debug($"Web message type: {messageType}");
            
            switch (messageType)
            {
                case "copyAndClose":
                    HandleCopyAndClose(root);
                    break;
                case "close":
                    HandleCloseOnly();
                    break;
                default:
                    Logger.Warning($"Unknown message type: {messageType}");
                    break;
            }
        }

        /// <summary>
        /// Handle copy text to clipboard and close window
        /// </summary>
        private void HandleCopyAndClose(System.Text.Json.JsonElement root)
        {
            if (root.TryGetProperty("text", out var textProperty))
            {
                var text = textProperty.GetString() ?? "";
                Logger.Info($"Editor closed with Ctrl+Enter, text length: {text.Length}");
                if (!string.IsNullOrEmpty(text))
                {
                    Clipboard.SetText(text);
                }
            }
            else
            {
                Logger.Warning("copyAndClose message missing 'text' property");
            }
            Close();
        }

        /// <summary>
        /// Handle close window without copying
        /// </summary>
        private void HandleCloseOnly()
        {
            Logger.Info("Editor closed with Escape key");
            Close();
        }

        /// <summary>
        /// Load Monaco Editor HTML file and set up navigation handling
        /// </summary>
        private void LoadMonacoEditor()
        {
            var htmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "editor.html");
            var fileUri = new Uri(htmlFilePath).AbsoluteUri;
            
            Logger.Debug($"Loading Monaco Editor from: {fileUri}");
            
            SetupNavigationHandling();
            WebViewEditor.CoreWebView2.Navigate(fileUri);
            Logger.Debug("Monaco Editor navigation started");
        }

        /// <summary>
        /// Set up handling of navigation completion for Monaco Editor
        /// </summary>
        private void SetupNavigationHandling()
        {
            WebViewEditor.CoreWebView2.NavigationCompleted += HandleNavigationCompleted;
        }

        /// <summary>
        /// Handle Monaco Editor navigation completion and set initial focus
        /// </summary>
        private async void HandleNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            if (args.IsSuccess)
            {
                _isWebViewReady = true;
                Logger.Debug("Monaco Editor navigation completed successfully");
                
                await SetInitialFocus();
            }
            else
            {
                Logger.Error($"Monaco Editor navigation failed: {args.WebErrorStatus}");
            }
        }

        /// <summary>
        /// Set initial focus to Monaco Editor after navigation completes
        /// </summary>
        private async Task SetInitialFocus()
        {
            // Wait for Monaco Editor to fully initialize
            await Task.Delay(100);
            try
            {
                // Configure console logging based on settings
                var enableConsoleLogging = _config.EnableConsoleLogging.ToString().ToLower();
                await WebViewEditor.CoreWebView2.ExecuteScriptAsync($"window.enableConsoleLogging = {enableConsoleLogging};");
                Logger.Debug($"Console logging configured: {enableConsoleLogging}");
                
                await WebViewEditor.CoreWebView2.ExecuteScriptAsync("focusEditor();");
                Logger.Debug("Focus set to Monaco Editor after navigation");
            }
            catch (Exception focusEx)
            {
                Logger.Warning($"Failed to set focus after navigation: {focusEx.Message}");
            }
        }
        
        private void TitleBar_MouseLeftButtonDown(object? sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Logger.Info("Editor closed with close button");
            Close();
        }
        
        private void ResizeHandle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (sender is not FrameworkElement element)
                {
                    Logger.Warning("ResizeHandle_MouseDown: sender is not FrameworkElement");
                    return;
                }
                
                var hwnd = new WindowInteropHelper(this).Handle;
                if (hwnd == IntPtr.Zero) 
                {
                    Logger.Warning("ResizeHandle_MouseDown: Cannot get window handle");
                    return;
                }
                
                Logger.Debug($"Resize initiated: {element.Name}");
                
                ReleaseCapture();
                
                int direction = 0;
                switch (element.Name)
                {
                    case "TopResize":
                        direction = HTTOP;
                        break;
                    case "BottomResize":
                        direction = HTBOTTOM;
                        break;
                    case "LeftResize":
                        direction = HTLEFT;
                        break;
                    case "RightResize":
                        direction = HTRIGHT;
                        break;
                    case "TopLeftResize":
                        direction = HTTOPLEFT;
                        break;
                    case "TopRightResize":
                        direction = HTTOPRIGHT;
                        break;
                    case "BottomLeftResize":
                        direction = HTBOTTOMLEFT;
                        break;
                    case "BottomRightResize":
                        direction = HTBOTTOMRIGHT;
                        break;
                    default:
                        Logger.Warning($"Unknown resize handle: {element.Name}");
                        return;
                }
                
                SendMessage(hwnd, WM_NCLBUTTONDOWN, new IntPtr(direction), IntPtr.Zero);
            }
        }

        public async void ShowEditorAtCursorCenter()
        {
            try
            {
                var cursorPos = System.Windows.Forms.Cursor.Position;
                var mouseX = cursorPos.X;
                var mouseY = cursorPos.Y;
                double w = Width;
                double h = Height;
                
                // Get the screen that contains the mouse cursor
                var currentScreen = System.Windows.Forms.Screen.FromPoint(cursorPos);
                var screenBounds = currentScreen.Bounds;
                
                // Calculate X position: center of the display containing the cursor
                var displayCenterX = screenBounds.Left + screenBounds.Width / 2;
                Left = displayCenterX - w / 2;
                
                // Calculate Y position: 80% towards cursor Y position from display center
                var displayCenterY = screenBounds.Top + screenBounds.Height / 2;
                var targetY = displayCenterY + 0.8 * (mouseY - displayCenterY);
                Top = targetY - h / 2;
                
                Logger.Debug($"Mouse at: ({mouseX}, {mouseY}), Screen: ({screenBounds.Left}, {screenBounds.Top}) {screenBounds.Width}x{screenBounds.Height}");
                Logger.Debug($"Display center: ({displayCenterX}, {displayCenterY}), Target Y: {targetY}");
                Logger.Debug($"Window size: {w}x{h}, Final position: ({Left}, {Top})");
                
                Show();
                Activate();
                
                // Use Windows API for robust focus handling
                var hwnd = new WindowInteropHelper(this).Handle;
                Logger.Debug($"Window handle: {hwnd}");
                
                if (hwnd != IntPtr.Zero)
                {
                    ShowWindow(hwnd, SW_SHOW);
                    BringWindowToTop(hwnd);
                    SetForegroundWindow(hwnd);
                    Logger.Debug("Applied Windows API focus calls");
                }
                
                // Focus the WebView2 control first
                WebViewEditor.Focus();
                Logger.Debug("Set focus to WebView2 control");
                
                // Then attempt to focus Monaco Editor with multiple strategies
                await AttemptMonacoFocus();
                
                Logger.Debug("Editor window shown and activated with comprehensive focus");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to show editor at cursor: {ex.Message}");
            }
        }
        
        private async Task AttemptMonacoFocus()
        {
            try
            {
                // Simple focus approach - avoid multiple competing strategies
                if (_isWebViewReady)
                {
                    await Task.Delay(100);  // Allow WebView2 to stabilize
                    await WebViewEditor.CoreWebView2.ExecuteScriptAsync("focusEditor();");
                    Logger.Debug("Monaco Editor focus attempted");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"Exception in AttemptMonacoFocus: {ex.Message}");
            }
        }
    }
}