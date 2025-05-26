using Tools.Library.Entities;

namespace Tools.Provider;

public static class NavigationCollectionProvider
{
    public static ObservableCollection<NavigationItem> GetNavigationItems(ICommand cardClickCommand)
    {
        return new ObservableCollection<NavigationItem>
        {
            new NavigationItem
            {
                Title = "Workspaces",
                Subtitle = "Manage your workspaces",
                Symbol = "AppFolder24",
                Command = cardClickCommand,
                CommandParameter = "Workspaces"
            },
            new NavigationItem
            {
                Title = "Nuget Local Copy",
                Subtitle = "copy new nuget packages to destination",
                Symbol = "BoxMultipleArrowRight20",
                Command = cardClickCommand,
                CommandParameter = "NugetLocal"
            },
            new NavigationItem
            {
                Title = "Formatters",
                Subtitle = string.Empty,
                Symbol = "TextNumberFormat24",
                Command = cardClickCommand,
                CommandParameter = "Formatters"
            },
            new NavigationItem
            {
                Title = "EF Tools",
                Subtitle = "Entity Framework tools and utilities",
                Symbol = "Database24",
                Command = cardClickCommand,
                CommandParameter = "EFTools"
            },
            new NavigationItem
            {
                Title = "Code Execute",
                Subtitle = string.Empty,
                Symbol = "Code24",
                Command = cardClickCommand,
                CommandParameter = "CodeExecute"
            }
        };
    }
}