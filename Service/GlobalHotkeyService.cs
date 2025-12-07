using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DesktopAIAgent.Service
{
    public class GlobalHotkeyService : IGlobalHotkeyService
    {
        private const int WM_HOTKEY = 0x0312;
        private HwndSource? _source;
        private int _currentId;

        [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public event EventHandler? HotkeyPressed;

        public void Subscribe(EventHandler handler)
        {
            HotkeyPressed += handler;
        }

        public void Register(Window window, int id, uint modifiers, uint key)
        {
            _currentId = id;
            var helper = new WindowInteropHelper(window);

            if (helper.Handle == IntPtr.Zero)
            {
                throw new InvalidOperationException("Window handle not created yet.");
            }

            _source = HwndSource.FromHwnd(helper.Handle);
            _source.AddHook(WndProc);

            if (!RegisterHotKey(helper.Handle, id, modifiers, key))
            {
                // Handle error
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == _currentId)
            {
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (_source != null)
            {
                _source.RemoveHook(WndProc);
                UnregisterHotKey(_source.Handle, _currentId);
                _source = null;
            }
        }
    }
}
