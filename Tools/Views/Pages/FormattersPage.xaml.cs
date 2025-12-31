using Microsoft.UI.Xaml.Controls;
using Tools.ViewModels.Pages;

namespace Tools.Views.Pages;

public sealed partial class FormattersPage : Page
{
    public FormattersPageViewModel ViewModel { get; }

    public FormattersPage(FormattersPageViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
    }
}