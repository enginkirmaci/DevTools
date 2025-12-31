using Microsoft.UI.Xaml.Controls;
using Tools.ViewModels.Pages;

namespace Tools.Views.Pages;

public sealed partial class HostFileProxyPage : Page
{
    public HostFileProxyViewModel ViewModel { get; }

    public HostFileProxyPage(HostFileProxyViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
    }
}