using Microsoft.UI.Xaml.Controls;
using Tools.ViewModels.Pages;

namespace Tools.Views.Pages;

public sealed partial class CodeExecutePage : Page
{
    public CodeExecutePageViewModel ViewModel { get; }

    public CodeExecutePage(CodeExecutePageViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();
    }
}