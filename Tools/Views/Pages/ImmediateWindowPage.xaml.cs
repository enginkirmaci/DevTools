
using System.Windows.Controls;
using Tools.ViewModels.Pages;

namespace Tools.Views.Pages
{
    public partial class ImmediateWindowPage : Page
    {
        public ImmediateWindowPageViewModel ViewModel { get; }

        public ImmediateWindowPage(ImmediateWindowPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}