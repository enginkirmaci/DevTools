using Prism.Commands;
using Prism.Mvvm;
using Tools.Library.Converters;
using Tools.Library.Entities;
using Wpf.Ui;

namespace Tools.ViewModels.Pages;

public class DashboardViewModel : BindableBase
{
    private readonly INavigationService navigationService;

    public DelegateCommand<string> CardClickCommand { get; set; }
    public ObservableCollection<DashboardCard> DashboardCards { get; set; }

    public DashboardViewModel(INavigationService navigationService)
    {
        this.navigationService = navigationService;

        CardClickCommand = new DelegateCommand<string>(CardClick);

        DashboardCards = new ObservableCollection<DashboardCard>
        {
            new DashboardCard
            {
                Title = "Workspaces",
                Subtitle = "Manage your workspaces",
                Symbol = "AppFolder24",
                Command = CardClickCommand,
                CommandParameter = "Workspaces"
            },
            new DashboardCard
            {
                Title = "Nuget Local Copy",
                Subtitle = "copy new nuget packages to destination",
                Symbol = "BoxMultipleArrowRight20",
                Command = CardClickCommand,
                CommandParameter = "NugetLocal"
            },
            new DashboardCard
            {
                Title = "Formatters",
                Subtitle = string.Empty,
                Symbol = "TextNumberFormat24",
                Command = CardClickCommand,
                CommandParameter = "Formatters"
            },
            new DashboardCard
            {
                Title = "EF Tools",
                Subtitle = "Entity Framework tools and utilities",
                Symbol = "Database24",
                Command = CardClickCommand,
                CommandParameter = "EFTools"
            },
            new DashboardCard
            {
                Title = "Code Execute",
                Subtitle = string.Empty,
                Symbol = "Code24",
                Command = CardClickCommand,
                CommandParameter = "CodeExecute"
            }
        };
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