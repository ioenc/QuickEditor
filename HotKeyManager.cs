using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace QuickEditor
{
    public class HotKeyManager : IDisposable
    {
        [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private HwndSource _source;
        private const int HOTKEY_ID = 9000;
        private Action _callback;
        private bool _isRegistered = false;
        private bool _disposed = false;

        public HotKeyManager(Window window)
        {
            var handle = new WindowInteropHelper(window).Handle;
            Logger.Debug($"HotKeyManager initializing with window handle: {handle}");
            
            _source = HwndSource.FromHwnd(handle);
            _source.AddHook(HwndHook);
            
            Logger.Debug("HotKeyManager initialized and hook added");
        }

        public bool RegisterHotKey(ModifierKeys modifiers, Key key, Action callback)
        {
            _callback = callback;
            uint mod = (uint)modifiers;
            uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);
            
            Logger.Debug($"Registering hotkey: Modifiers={modifiers} ({mod}), Key={key} ({vk}), Handle={_source.Handle}");
            
            bool success = RegisterHotKey(_source.Handle, HOTKEY_ID, mod, vk);
            
            if (success)
            {
                _isRegistered = true;
                Logger.Info($"Hotkey registered successfully: {modifiers}+{key}");
            }
            else
            {
                var error = Marshal.GetLastWin32Error();
                Logger.Error($"Failed to register hotkey {modifiers}+{key}. Win32 Error: {error}");
                if (error == 1409) // ERROR_HOTKEY_ALREADY_REGISTERED
                {
                    Logger.Warning("Hotkey is already registered by another application or instance");
                }
            }
            
            return success;
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == 0x0312) // WM_HOTKEY
            {
                Logger.Debug($"WM_HOTKEY message received: ID={wParam.ToInt32()}");
                if (wParam.ToInt32() == HOTKEY_ID)
                {
                    Logger.Info("Hotkey pressed - invoking callback");
                    try 
                    {
                        _callback?.Invoke();
                        Logger.Debug("Callback invoked successfully");
                    }
                    catch (Exception ex)
                    {
                        Logger.Error($"Error in hotkey callback: {ex.Message}");
                    }
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        public void Unregister()
        {
            if (_isRegistered && _source?.Handle != IntPtr.Zero)
            {
                Logger.Debug($"Unregistering hotkey, Handle={_source.Handle}");
                bool success = UnregisterHotKey(_source.Handle, HOTKEY_ID);
                if (success)
                {
                    Logger.Info("Hotkey unregistered successfully");
                    _isRegistered = false;
                }
                else
                {
                    var error = Marshal.GetLastWin32Error();
                    Logger.Warning($"Failed to unregister hotkey. Win32 Error: {error}");
                }
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Unregister();
                _source?.RemoveHook(HwndHook);
                _disposed = true;
            }
        }
    }
}