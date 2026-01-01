using CommunityToolkit.Mvvm.Input;
using Tools.Library.Converters;
using Tools.Library.Entities;
using Tools.Library.Mvvm;
using Tools.Library.Providers;
using Tools.Library.Services.Abstractions;

namespace Tools.ViewModels.Pages;

/// <summary>
/// ViewModel for the Dashboard page.
/// </summary>
public partial class DashboardViewModel : PageViewModelBase
{
    private readonly INavigationService _navigationService;

    /// <summary>
    /// Gets the command for card click actions.
    /// </summary>
    public IRelayCommand<string> CardClickCommand { get; }

    /// <summary>
    /// Gets the collection of dashboard navigation cards.
    /// </summary>
    public IReadOnlyCollection<NavigationItem> DashboardCards { get; }

    public DashboardViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        CardClickCommand = new RelayCommand<string>(OnCardClick);
        DashboardCards = NavigationProvider.GetDashboardItems(CardClickCommand);
    }

    private void OnCardClick(string? parameter)
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