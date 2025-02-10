using System.Windows.Controls;
using Tools.ViewModels.Pages;

namespace Tools.Views.Pages
{
    public partial class EFToolsPage : Page
    {
        public EFToolsPageViewModel ViewModel { get; }

        public EFToolsPage(EFToolsPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
