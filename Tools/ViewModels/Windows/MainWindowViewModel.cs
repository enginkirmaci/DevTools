using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Tools.Provider;

namespace Tools.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject
{
    public string ApplicationTitle { get; } = "Dev Tools";
    public ObservableCollection<NavigationViewItem> MenuItems { get; set; }

    public MainWindowViewModel()
    {
        MenuItems = NavigationCollectionProvider.GetNavigationViewItems();
    }
}