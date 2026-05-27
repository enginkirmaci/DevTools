using Microsoft.UI.Xaml.Controls;
using Tools.ViewModels.Pages;

namespace Tools.Views.Pages;

public sealed partial class SnapItSettingsPage : Page
{
    public SnapItSettingsViewModel ViewModel { get; }

    public SnapItSettingsPage(SnapItSettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
    }
}
