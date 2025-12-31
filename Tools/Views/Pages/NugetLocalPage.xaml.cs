using Microsoft.UI.Xaml.Controls;
using Tools.ViewModels.Pages;

namespace Tools.Views.Pages;

public sealed partial class NugetLocalPage : Page
{
    public NugetLocalViewModel ViewModel { get; }

    public NugetLocalPage(NugetLocalViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
    }
}