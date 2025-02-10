
using System.Windows.Controls;
using Tools.ViewModels.Pages;

namespace Tools.Views.Pages
{
    public partial class CodeExecutePage : Page
    {
        public CodeExecutePageViewModel ViewModel { get; }

        public CodeExecutePage(CodeExecutePageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}