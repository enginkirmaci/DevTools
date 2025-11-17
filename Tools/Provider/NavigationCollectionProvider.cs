using Tools.Library.Entities;
using Tools.Views.Pages;
using Wpf.Ui.Controls;

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
                CommandParameter = "Workspaces",
                TargetPageType = typeof(WorkspacesPage)
            },
            new NavigationItem
            {
                Title = "Nuget Local Copy",
                Subtitle = "copy new nuget packages to destination",
                Symbol = "BoxMultipleArrowRight20",
                Command = cardClickCommand,
                CommandParameter = "NugetLocal",
                TargetPageType = typeof(NugetLocalPage)
            },
            new NavigationItem
            {
                Title = "Formatters",
                Subtitle = string.Empty,
                Symbol = "TextNumberFormat24",
                Command = cardClickCommand,
                CommandParameter = "Formatters",
                TargetPageType = typeof(FormattersPage)
            },
            new NavigationItem
            {
                Title = "Clipboard Password",
                Subtitle = "Generate and copy passwords",
                Symbol = "ClipboardPaste24",
                Command = cardClickCommand,
                CommandParameter = "ClipboardPassword",
                TargetPageType = typeof(ClipboardPasswordPage)
            },
            new NavigationItem
            {
                Title = "EF Tools",
                Subtitle = "Entity Framework tools and utilities",
                Symbol = "Database24",
                Command = cardClickCommand,
                CommandParameter = "EFTools",
                TargetPageType = typeof(EFToolsPage)
            },
            new NavigationItem
            {
                Title = "Code Execute",
                Subtitle = string.Empty,
                Symbol = "Code24",
                Command = cardClickCommand,
                CommandParameter = "CodeExecute",
                TargetPageType = typeof(CodeExecutePage)
            }
        };
    }

    public static ObservableCollection<NavigationViewItem> GetNavigationViewItems()
    {
        var result = new ObservableCollection<NavigationViewItem>
        {
            new NavigationViewItem
            {
                Content = "Dashboard",
                Icon = new SymbolIcon(SymbolRegular.Home24),
                TargetPageType = typeof(DashboardPage)
            }
        };

        foreach (NavigationItem item in GetNavigationItems(null))
        {
            result.Add(new NavigationViewItem
            {
                Content = item.Title,
                Icon = new SymbolIcon((SymbolRegular)Enum.Parse(typeof(SymbolRegular), item.Symbol)),
                TargetPageType = item.TargetPageType
            });
        }

        return result;
    }
}