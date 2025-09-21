using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace TrayPopupApp
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_NOREPEAT = 0x4000;
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 0xBEEF;

        [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private HwndSource? _hwndSource;

        public ObservableCollection<TabItemVm> Tabs { get; } = new();
        private TabItemVm? _selectedTab;
        public TabItemVm? SelectedTab
        {
            get => _selectedTab;
            set { _selectedTab = value; OnPropertyChanged(nameof(SelectedTab)); }
        }

        private bool _isSettingsActive;
        public bool IsSettingsActive
        {
            get => _isSettingsActive;
            set { _isSettingsActive = value; OnPropertyChanged(nameof(IsSettingsActive)); OnPropertyChanged(nameof(ActiveContent)); }
        }

        private FrameworkElement? _settingsContent;
        public FrameworkElement? SettingsContent
        {
            get => _settingsContent;
            set { _settingsContent = value; OnPropertyChanged(nameof(SettingsContent)); OnPropertyChanged(nameof(ActiveContent)); }
        }

        public object? ActiveContent => IsSettingsActive ? (object?)SettingsContent : SelectedTab?.Content;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            SettingsContent = BuildSettingsView();
            InitTabs();
        }

        private void InitTabs()
        {
            AddNewTab();
            Tabs.Add(TabItemVm.AddButton());
        }

        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.Tag is TabItemVm vm)
            {
                if (vm.IsAddButton)
                {
                    AddNewTab();
                }
                else
                {
                    Select(vm);
                }
            }
        }

        private void AddNewTab() 
        {
            var idx = Math.Max(0, Tabs.Count - 1);
            var created = NewContentTab($"Tab {idx}", TabItemVm.CheckIcon());
            Tabs.Insert(idx, created);
            Select(created);
        }

        private void Select(TabItemVm vm)
        {
            IsSettingsActive = false;
            foreach (var t in Tabs)
                t.IsSelected = false;

            vm.IsSelected = true;
            SelectedTab = vm;
            OnPropertyChanged(nameof(ActiveContent));
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var t in Tabs) t.IsSelected = false;
            SelectedTab = null;
            IsSettingsActive = true;
            OnPropertyChanged(nameof(ActiveContent));
        }

        private TabItemVm NewContentTab(string title, Geometry icon)
        {
            var grid = new Grid
            {
                Margin = new Thickness(24),
            };
            grid.Children.Add(new TextBlock
            {
                Text = $"{title}: content here",
                Foreground = new SolidColorBrush(Color.FromArgb(0xFF, 0xE6, 0xE6, 0xE6)),
                FontSize = 20
            });

            return new TabItemVm
            {
                Title = title,
                Icon = icon,
                Content = grid
            };
        }

        private FrameworkElement BuildSettingsView()
        {
            var p = new StackPanel { Margin = new Thickness(24) };
            p.Children.Add(new TextBlock
            {
                Text = "Settings",
                FontSize = 22,
                Foreground = new SolidColorBrush(Color.FromRgb(230, 230, 230)),
                Margin = new Thickness(0, 0, 0, 8)
            });
            p.Children.Add(new TextBlock
            {
                Text = "Application settings will be there...",
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200))
            });
            return p;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => PositionBottomRight();

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var helper = new WindowInteropHelper(this);
            _hwndSource = HwndSource.FromHwnd(helper.Handle);
            _hwndSource.AddHook(WndProc);

            _ = RegisterHotKey(helper.Handle, HOTKEY_ID,
                MOD_CONTROL | MOD_ALT | MOD_NOREPEAT,
                (uint)System.Windows.Input.KeyInterop.VirtualKeyFromKey(System.Windows.Input.Key.T));
        }

        private void PositionBottomRight()
        {
            const int margin = 10;
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - margin;
            Top = workArea.Bottom - Height - margin;
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                if (IsVisible && WindowState != WindowState.Minimized)
                    Hide();
                else
                {
                    if (!IsVisible) Show();
                    if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
                    PositionBottomRight();
                    Activate();
                    Focus();
                }
                handled = true;
            }
            return IntPtr.Zero;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (WindowState == WindowState.Minimized)
                Hide();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        public void ForceShutdownCleanup()
        {
            try
            {
                var handle = new WindowInteropHelper(this).Handle;
                if (handle != IntPtr.Zero)
                    UnregisterHotKey(handle, HOTKEY_ID);
            }
            catch { }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class TabItemVm : INotifyPropertyChanged
    {
        public string Title { get; set; } = "";
        public Geometry Icon { get; set; } = CheckIcon();
        public object? Content { get; set; }

        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }

        public bool IsAddButton { get; set; }
        private bool _isLoading;
        public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(nameof(IsLoading)); } }

        public string Tooltip => Title;

        public static Geometry PlusIcon() =>
            Geometry.Parse("M12,22 L12,12 2,12 2,10 12,10 12,0 14,0 14,10 24,10 24,12 14,12 14,22 Z");
        public static Geometry CheckIcon() =>
            Geometry.Parse("M2,12 L5,9 10,14 19,5 22,8 10,20 Z");
        public static Geometry LoaderStubIcon() =>
            Geometry.Parse("M12,2 A10,10 0 1 1 11.999,2 Z");

        public static TabItemVm AddButton() => new TabItemVm
        {
            Title = "New",
            Icon = PlusIcon(),
            IsAddButton = true
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
