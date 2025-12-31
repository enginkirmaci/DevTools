using Microsoft.UI.Xaml.Controls;
using Tools.Library.Entities;
using Tools.Views.Pages;

namespace Tools.Provider;

public static class NavigationCollectionProvider
{
    public static ObservableCollection<NavigationItem> GetNavigationItems(System.Windows.Input.ICommand? cardClickCommand)
    {
        return new ObservableCollection<NavigationItem>
        {
            new NavigationItem
            {
                Title = "Workspaces",
                Subtitle = "Manage your workspaces",
                Symbol = "\uE8B7", // AppFolder24 equivalent
                Command = cardClickCommand!,
                CommandParameter = "Workspaces",
                TargetPageType = typeof(WorkspacesPage)
            },
            new NavigationItem
            {
                Title = "Nuget Local Copy",
                Subtitle = "copy new nuget packages to destination",
                Symbol = "\uE8DE", // BoxMultipleArrowRight20 equivalent
                Command = cardClickCommand!,
                CommandParameter = "NugetLocal",
                TargetPageType = typeof(NugetLocalPage)
            },
            new NavigationItem
            {
                Title = "Formatters",
                Subtitle = string.Empty,
                Symbol = "\uE8D2", // TextNumberFormat24 equivalent
                Command = cardClickCommand!,
                CommandParameter = "Formatters",
                TargetPageType = typeof(FormattersPage)
            },
            new NavigationItem
            {
                Title = "Clipboard Password",
                Subtitle = "Generate and copy passwords",
                Symbol = "\uE77F", // ClipboardPaste24 equivalent
                Command = cardClickCommand!,
                CommandParameter = "ClipboardPassword",
                TargetPageType = typeof(ClipboardPasswordPage)
            },
            new NavigationItem
            {
                Title = "EF Tools",
                Subtitle = "Entity Framework tools and utilities",
                Symbol = "\uEE94", // Database24 equivalent
                Command = cardClickCommand!,
                CommandParameter = "EFTools",
                TargetPageType = typeof(EFToolsPage)
            },
            new NavigationItem
            {
                Title = "Code Execute",
                Subtitle = string.Empty,
                Symbol = "\uE943", // Code24 equivalent
                Command = cardClickCommand!,
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
                Icon = new FontIcon { Glyph = "\uE80F" }, // Home24
                Tag = "Dashboard"
            }
        };

        foreach (NavigationItem item in GetNavigationItems(null))
        {
            result.Add(new NavigationViewItem
            {
                Content = item.Title,
                Icon = new FontIcon { Glyph = item.Symbol },
                Tag = item.CommandParameter // Use Tag for navigation in WinUI 3
            });
        }

        return result;
    }
}