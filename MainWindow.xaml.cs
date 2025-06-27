using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using System.Windows.Interop;

namespace QuickEditor
{
    public partial class MainWindow : Window
    {
        private HotKeyManager _hotKeyManager;
        private NotifyIcon _notifyIcon;
        private Config _config;
        private static bool _isInitialized = false;

        public MainWindow()
        {
            if (_isInitialized)
            {
                Logger.Warning("MainWindow constructor called multiple times - skipping duplicate initialization");
                return;
            }
            _isInitialized = true;
            
            InitializeComponent();
            
            // Initialize configuration and logging
            _config = Config.Load();
            Logger.Initialize(_config);
            
            Logger.Info("QuickEditor starting up");
            Logger.Debug($"Config loaded - Logging: {_config.EnableLogging}, LogLevel: {_config.LogLevel}");
            
            // Log the absolute path of the log file for easier debugging
            var logPath = Path.IsPathRooted(_config.LogFilePath) 
                ? _config.LogFilePath 
                : Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "", _config.LogFilePath);
            Logger.Info($"Log file location: {logPath}");
            
            InitializeSystemTray();
            
            // Show window briefly to create handle, then hide
            Logger.Debug("Creating window handle");
            Show();
            Logger.Debug($"Window shown. IsVisible: {IsVisible}, IsLoaded: {IsLoaded}, WindowState: {WindowState}");
            Hide();
            Logger.Debug($"Window hidden. IsVisible: {IsVisible}, Handle: {new WindowInteropHelper(this).Handle}");
            
            // Initialize hotkey manager
            Logger.Debug("Initializing hotkey manager");
            _hotKeyManager = new HotKeyManager(this);
            
            // Test with Ctrl+Shift+F (more reliable)
            bool hotkeyRegistered = _hotKeyManager.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Shift, Key.F, OnHotKeyPressed);
            
            if (hotkeyRegistered)
            {
                Logger.Info("Hotkey registered successfully: Ctrl+Shift+F");
            }
            else
            {
                Logger.Error("Failed to register hotkey Ctrl+Shift+F");
            }
        }

        private void InitializeSystemTray()
        {
            if (_notifyIcon != null)
            {
                Logger.Warning("System tray already initialized - skipping");
                return;
            }
            
            Logger.Debug("Initializing system tray");
            _notifyIcon = new NotifyIcon();
            
            // Load custom icon
            try
            {
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "QuickEditor.ico");
                if (File.Exists(iconPath))
                {
                    _notifyIcon.Icon = new Icon(iconPath);
                    Logger.Debug($"Custom icon loaded from: {iconPath}");
                }
                else
                {
                    _notifyIcon.Icon = SystemIcons.Application;
                    Logger.Warning($"Custom icon not found at: {iconPath}, using default");
                }
            }
            catch (Exception ex)
            {
                _notifyIcon.Icon = SystemIcons.Application;
                Logger.Warning($"Failed to load custom icon: {ex.Message}, using default");
            }
            
            _notifyIcon.Text = "QuickEditor - Ctrl+Shift+F to open";
            _notifyIcon.Visible = true;

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Exit", null, (s, e) => 
            {
                Logger.Info("Exit requested from system tray");
                _notifyIcon.Visible = false;
                System.Windows.Application.Current.Shutdown();
            });
            _notifyIcon.ContextMenuStrip = contextMenu;
            Logger.Debug("System tray initialized");
        }

        private void OnHotKeyPressed()
        {
            Logger.Info("Hotkey pressed - showing editor");
            try
            {
                var editor = new EditorWindow(_config);
                editor.ShowEditorAtCursorCenter();
                Logger.Debug("Editor window created and shown");
            }
            catch (Exception ex)
            {
                Logger.Error($"Failed to show editor: {ex.Message}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            Logger.Info("Application closing - cleaning up resources");
            _hotKeyManager?.Dispose();
            _notifyIcon?.Dispose();
            Logger.Info("Application closed");
            base.OnClosed(e);
        }
    }
}