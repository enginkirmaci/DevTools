using Tools.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Tools.Views.Pages;

public partial class NugetLocalPage : INavigableView<NugetLocalViewModel>
{
    public NugetLocalViewModel ViewModel { get; }

    public NugetLocalPage(NugetLocalViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }
}