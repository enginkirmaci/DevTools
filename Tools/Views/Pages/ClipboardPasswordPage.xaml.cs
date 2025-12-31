using Microsoft.UI.Xaml.Controls;
using Tools.ViewModels.Pages;

namespace Tools.Views.Pages;

public sealed partial class ClipboardPasswordPage : Page
{
    public ClipboardPasswordPageViewModel ViewModel { get; }

    public ClipboardPasswordPage(ClipboardPasswordPageViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
    }
}
