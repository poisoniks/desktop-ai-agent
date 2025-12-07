using System.ComponentModel;
using System.Windows.Media;

namespace DesktopAIAgent.ViewModel
{

    public class TabItemVm : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

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
    }
}
