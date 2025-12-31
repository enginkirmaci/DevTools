using Microsoft.UI.Xaml.Controls;
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
}
