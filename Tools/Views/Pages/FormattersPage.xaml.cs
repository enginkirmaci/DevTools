using System.Windows.Controls;
using Tools.ViewModels.Pages;

namespace Tools.Views.Pages
{
    public partial class FormattersPage : Page
    {
        public FormattersPageViewModel ViewModel { get; }

        public FormattersPage(FormattersPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}