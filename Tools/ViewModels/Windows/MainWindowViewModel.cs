using Prism.Mvvm;
using Prism.Regions;
using Tools.Views.Pages;
using Wpf.Ui;

namespace Tools.ViewModels.Windows
{
    public class MainWindowViewModel : BindableBase
    {
        public string ApplicationTitle { get; } = "Dev Tools";

        public MainWindowViewModel()
        {
        }
    }
}