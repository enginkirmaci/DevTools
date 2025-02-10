using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Tools.Library.Formatters;
using System.Windows;

namespace Tools.ViewModels.Pages
{
    public class EFToolsPageViewModel : BindableBase
    {
        private string _sqlInput;
        private string _cSharpOutput;

        public string SqlInput
        {
            get => _sqlInput;
            set => SetProperty(ref _sqlInput, value);
        }

        public string CSharpOutput
        {
            get => _cSharpOutput;
            set => SetProperty(ref _cSharpOutput, value);
        }

        public ICommand ConvertSqlCommand { get; }
        public ICommand CopyToClipboardCommand { get; }

        public EFToolsPageViewModel()
        {
            ConvertSqlCommand = new DelegateCommand(OnConvertSqlCommand);
            CopyToClipboardCommand = new DelegateCommand(OnCopyToClipboardCommand);
        }

        private void OnConvertSqlCommand()
        {
            try
            {
                CSharpOutput = SqlToCSharpFormatter.FormatCreateTableToClass(SqlInput);
            }
            catch (Exception ex)
            {
                CSharpOutput = $"Error: {ex.Message}";
            }
        }

        private void OnCopyToClipboardCommand()
        {
            if (!string.IsNullOrEmpty(CSharpOutput))
            {
                Clipboard.SetText(CSharpOutput);
            }
        }
    }
}