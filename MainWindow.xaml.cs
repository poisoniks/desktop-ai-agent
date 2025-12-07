using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using DesktopAIAgent.ViewModel;

namespace DesktopAIAgent
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<TabItemVm> Tabs { get; } = new();

        private TabItemVm? _selectedTab;
        public TabItemVm? SelectedTab
        {
            get => _selectedTab;
            set { _selectedTab = value; OnPropertyChanged(nameof(SelectedTab)); OnPropertyChanged(nameof(ActiveContent)); }
        }

        private bool _isSettingsActive;
        public bool IsSettingsActive
        {
            get => _isSettingsActive;
            set { _isSettingsActive = value; OnPropertyChanged(nameof(IsSettingsActive)); OnPropertyChanged(nameof(ActiveContent)); }
        }

        private SettingsViewModel _settingsVm = new SettingsViewModel();

        public object? ActiveContent => IsSettingsActive ? _settingsVm : SelectedTab?.Content;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
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
                if (vm.IsAddButton) AddNewTab();
                else Select(vm);
            }
        }

        private void AddNewTab()
        {
            var idx = Math.Max(0, Tabs.Count - 1);
            var newTabVm = NewContentTab($"Tab {idx}", TabItemVm.CheckIcon());
            Tabs.Insert(idx, newTabVm);
            Select(newTabVm);
        }

        private void Select(TabItemVm vm)
        {
            IsSettingsActive = false;
            foreach (var t in Tabs) t.IsSelected = false;
            vm.IsSelected = true;
            SelectedTab = vm;
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var t in Tabs) t.IsSelected = false;
            SelectedTab = null;
            IsSettingsActive = true;
        }

        private TabItemVm NewContentTab(string title, Geometry icon)
        {
            var chatVm = new ChatViewModel();
            chatVm.ReceiveMessage($"Welcome to {title}", false);

            return new TabItemVm
            {
                Title = title,
                Icon = icon,
                Content = chatVm
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) => PositionBottomRight();

        public void PositionBottomRight()
        {
            const int margin = 10;
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - margin;
            Top = workArea.Bottom - Height - margin;
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);
            if (WindowState == WindowState.Minimized) Hide();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
