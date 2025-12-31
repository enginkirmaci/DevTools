using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tools.Library.Converters;
using Tools.Library.Entities;
using Tools.Provider;
using Tools.Services;

namespace Tools.ViewModels.Pages;

public partial class DashboardViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    public IRelayCommand<string> CardClickCommand { get; }
    public ObservableCollection<NavigationItem> DashboardCards { get; set; }

    public DashboardViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;

        CardClickCommand = new RelayCommand<string>(CardClick);

        DashboardCards = NavigationCollectionProvider.GetNavigationItems(CardClickCommand);
    }

    private void CardClick(string? parameter)
    {
        if (string.IsNullOrWhiteSpace(parameter))
        {
            return;
        }

        Type? pageType = NameToPageTypeConverter.Convert(parameter);

        if (pageType == null)
        {
            return;
        }

        _navigationService.Navigate(pageType);
    }
}