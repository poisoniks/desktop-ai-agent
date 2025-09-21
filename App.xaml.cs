using System;
using System.Drawing;
using System.Windows;
using Forms = System.Windows.Forms;

namespace TrayPopupApp
{
    public partial class App : System.Windows.Application
    {
        private Forms.NotifyIcon? _trayIcon;
        private MainWindow? _window;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _window = new MainWindow();

            _trayIcon = new Forms.NotifyIcon
            {
                Text = "Tray Popup App",
                Visible = true,
                Icon = LoadIconOrFallback()
            };

            var menu = new Forms.ContextMenuStrip();
            menu.Items.Add("Show", null, (_, __) => ShowWindow());
            menu.Items.Add("Hide", null, (_, __) => _window?.Hide());
            menu.Items.Add(new Forms.ToolStripSeparator());
            menu.Items.Add("Exit", null, (_, __) => ExitApp());
            _trayIcon.ContextMenuStrip = menu;

            _trayIcon.MouseClick += (s, args) =>
            {
                if (args.Button == Forms.MouseButtons.Left)
                {
                    ShowWindow();
                }
            };

            ShowWindow();
        }

        private Icon LoadIconOrFallback()
        {
            try
            {
                var uri = new Uri("pack://application:,,,/Assets/app.ico", UriKind.Absolute);
                using var stream = System.Windows.Application.GetResourceStream(uri)?.Stream;
                if (stream != null)
                {
                    return new Icon(stream);
                }
            }
            catch { }

            return (Icon)SystemIcons.Application.Clone();
        }

        private void ShowWindow()
        {
            if (_window == null) return;
            if (!_window.IsVisible)
            {
                _window.Show();
            }
            if (_window.WindowState == WindowState.Minimized)
            {
                _window.WindowState = WindowState.Normal;
            }
            _window.Activate();
            _window.Focus();
        }

        private void ExitApp()
        {
            try
            {
                _trayIcon!.Visible = false;
                _trayIcon!.Dispose();
            }
            catch { }

            _window?.ForceShutdownCleanup();
            System.Windows.Application.Current.Shutdown();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                _trayIcon?.Dispose();
            }
            catch { }
            base.OnExit(e);
        }
    }
}