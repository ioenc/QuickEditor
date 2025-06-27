using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace QuickEditor
{
    public partial class App : Application
    {
        private const string MUTEX_NAME = "QuickEditor_SingleInstance_Mutex";
        private System.Threading.Mutex _mutex;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            // Check for single instance
            bool createdNew;
            _mutex = new System.Threading.Mutex(true, MUTEX_NAME, out createdNew);
            
            if (!createdNew)
            {
                // Another instance is already running
                MessageBox.Show("QuickEditor is already running. Check the system tray.", 
                               "QuickEditor", MessageBoxButton.OK, MessageBoxImage.Information);
                Shutdown();
                return;
            }
            
            // Ensure we handle shutdown properly
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            
            base.OnStartup(e);
            
            // Create and show MainWindow to prevent multiple instances
            var main = new MainWindow();
            this.MainWindow = main;
        }
        
        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            base.OnExit(e);
        }
    }
}