using Microsoft.UI.Xaml.Controls;
using Tools.Library.Mvvm;
using Tools.Library.Providers;

namespace Tools.ViewModels.Windows;

/// <summary>
/// ViewModel for the main window of the application.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Gets the title of the application.
    /// </summary>
    public string ApplicationTitle { get; } = "Dev Tools";

    /// <summary>
    /// Gets the collection of menu items for navigation.
    /// </summary>
    public IReadOnlyCollection<NavigationViewItem> MenuItems { get; }

    public MainWindowViewModel()
    {
        MenuItems = NavigationProvider.GetNavigationMenuItems();
    }
}