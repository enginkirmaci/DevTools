using Microsoft.UI.Xaml.Controls;
using Tools.ViewModels.Pages;

namespace Tools.Views.Pages;

public sealed partial class EFToolsPage : Page
{
    public EFToolsPageViewModel ViewModel { get; }

    public EFToolsPage(EFToolsPageViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
    }
}
