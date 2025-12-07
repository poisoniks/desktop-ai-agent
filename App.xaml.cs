using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Hardcodet.Wpf.TaskbarNotification;
using DesktopAIAgent.Service;

namespace DesktopAIAgent
{
    public partial class App : System.Windows.Application
    {
        private TaskbarIcon? _trayIcon;
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

            _trayIcon = new TaskbarIcon
            {
                ToolTipText = "AI Agent",
                IconSource = new BitmapImage(new Uri("pack://application:,,,/Assets/app.ico"))
            };

            var menu = new ContextMenu();
            var showItem = new MenuItem { Header = "Show" };
            showItem.Click += (_, __) => ToggleWindow();
            menu.Items.Add(showItem);

            var exitItem = new MenuItem { Header = "Exit" };
            exitItem.Click += (_, __) => ExitApp();
            menu.Items.Add(exitItem);

            _trayIcon.ContextMenu = menu;

            _trayIcon.TrayLeftMouseUp += (_, __) => ToggleWindow();

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
