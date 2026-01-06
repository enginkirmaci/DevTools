using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Tools.Library.Entities;
using Tools.ViewModels.Pages;

namespace Tools.Views.Pages;

public sealed partial class WorkspacesPage : Page
{
    public WorkspacesViewModel ViewModel { get; }

    public WorkspacesPage(WorkspacesViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
    }

    private void OnOpenSolutionClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe)
        {
            if (fe.DataContext is WorkspaceItem item)
            {
                ViewModel.OpenSolutionCommand.Execute(item.SolutionPath);
                return;
            }
        }
    }

    private void OnOpenWithVSCodeClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe)
        {
            if (fe.DataContext is WorkspaceItem item)
            {
                ViewModel.OpenWithVSCodeCommand.Execute(item.FolderPath);
                return;
            }
        }
    }

    private void OnOpenWithTerminalClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe)
        {
            if (fe.DataContext is WorkspaceItem item)
            {
                ViewModel.OpenWithTerminalCommand.Execute(item.FolderPath);
                return;
            }
        }
    }

    private void OnOpenFolderClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe)
        {
            if (fe.DataContext is WorkspaceItem item)
            {
                ViewModel.OpenFolderCommand.Execute(item.FolderPath);
                return;
            }
        }
    }
}