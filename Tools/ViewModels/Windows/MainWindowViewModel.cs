using System.Collections;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Prism.Mvvm;

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