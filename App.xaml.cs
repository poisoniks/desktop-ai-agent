using System.Windows;
using System.Windows.Input;
using DesktopAIAgent.Service;
using Forms = System.Windows.Forms;

namespace DesktopAIAgent
{
    public partial class App : System.Windows.Application
    {
        private Forms.NotifyIcon? _trayIcon;
        private MainWindow? _window;
        private IGlobalHotkeyService? _hotkeyService;

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_NOREPEAT = 0x4000;
        private const int HOTKEY_ID = 0xBEEF;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _window = new MainWindow();

            _hotkeyService = new GlobalHotkeyService();
            _hotkeyService.Subscribe(OnHotkeyPressed);

            var helper = new System.Windows.Interop.WindowInteropHelper(_window);
            helper.EnsureHandle();

            uint key = (uint)KeyInterop.VirtualKeyFromKey(Key.T);
            _hotkeyService.Register(_window, HOTKEY_ID, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, key);

            _trayIcon = new Forms.NotifyIcon
            {
                Text = "AI Agent",
                Visible = true,
                Icon = LoadIconOrFallback()
            };

            var menu = new Forms.ContextMenuStrip();
            menu.Items.Add("Show", null, (_, __) => ToggleWindow());
            menu.Items.Add("Exit", null, (_, __) => ExitApp());
            _trayIcon.ContextMenuStrip = menu;

            _trayIcon.MouseClick += (s, args) =>
            {
                if (args.Button == Forms.MouseButtons.Left) ToggleWindow();
            };

            ToggleWindow();
        }

        private void OnHotkeyPressed(object? sender, EventArgs e)
        {
            ToggleWindow();
        }

        private void ToggleWindow()
        {
            if (_window == null) return;

            if (_window.IsVisible)
            {
                if (_window.WindowState == WindowState.Minimized)
                {
                    _window.WindowState = WindowState.Normal;
                    _window.Activate();
                    _window.PositionBottomRight();
                }
                else
                {
                    _window.Hide();
                }
            }
            else
            {
                _window.Show();
                _window.WindowState = WindowState.Normal;
                _window.Activate();
                _window.PositionBottomRight();
            }
        }

        private Icon LoadIconOrFallback()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/Assets/app.ico", UriKind.Absolute);
                using var stream = GetResourceStream(uri)?.Stream;
                if (stream != null) return new Icon(stream);
            }
            catch { }
            return SystemIcons.Application;
        }

        private void ExitApp()
        {
            _trayIcon?.Dispose();
            _hotkeyService?.Dispose();
            Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _trayIcon?.Dispose();
            _hotkeyService?.Dispose();
            base.OnExit(e);
        }
    }
}
