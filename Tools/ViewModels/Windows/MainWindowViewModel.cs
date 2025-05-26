using Prism.Mvvm;
using Tools.Provider;
using Wpf.Ui.Controls;

namespace Tools.ViewModels.Windows
{
    public class MainWindowViewModel : BindableBase
    {
        public string ApplicationTitle { get; } = "Dev Tools";
        public ObservableCollection<NavigationViewItem> MenuItems { get; set; }

        public MainWindowViewModel()
        {
            MenuItems = NavigationCollectionProvider.GetNavigationViewItems();
        }
    }
}