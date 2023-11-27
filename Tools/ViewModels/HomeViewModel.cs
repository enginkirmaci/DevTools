using System.Threading.Tasks;
using Prism.Mvvm;
using Prism.Regions;
using Tools.Services;

namespace Tools.ViewModels;

public class HomeViewModel : BindableBase, INavigationAware
{
    private readonly INavigationService navigationService;

    public HomeViewModel(
        INavigationService navigationService)
    {
        this.navigationService = navigationService;

        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        _ = InitializeAsync();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
}