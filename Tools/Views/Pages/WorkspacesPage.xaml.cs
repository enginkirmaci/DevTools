using Tools.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Tools.Views.Pages;

/// <summary>
/// Interaction logic for WorkspacesPage.xaml
/// </summary>
public partial class WorkspacesPage : INavigableView<WorkspacesViewModel>
{
    public WorkspacesViewModel ViewModel { get; }

    public WorkspacesPage(WorkspacesViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}
