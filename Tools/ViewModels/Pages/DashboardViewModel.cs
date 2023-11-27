using Prism.Commands;
using Prism.Mvvm;
using Tools.Library.Converters;
using Wpf.Ui;

namespace Tools.ViewModels.Pages;

public class DashboardViewModel : BindableBase
{
    private readonly INavigationService navigationService;

    public DelegateCommand<string> CardClickCommand { get; set; }

    public DashboardViewModel(INavigationService navigationService)
    {
        this.navigationService = navigationService;

        CardClickCommand = new DelegateCommand<string>(CardClick);
    }

    private void CardClick(string parameter)
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

        _ = navigationService.Navigate(pageType);
    }
}