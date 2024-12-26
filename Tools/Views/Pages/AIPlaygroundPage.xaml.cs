using Tools.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace Tools.Views.Pages
{
    /// <summary>
    /// Interaction logic for HostFileProxy.xaml
    /// </summary>
    public partial class AIPlaygroundPage : INavigableView<AIPlaygroundViewModel>
    {
        public AIPlaygroundViewModel ViewModel { get; }

        public AIPlaygroundPage(AIPlaygroundViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}