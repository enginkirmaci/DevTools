using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Tools.ViewModels.Pages
{
    public class FormattersPageViewModel : BindableBase
    {
        private string _inputText;
        private string _outputText;
        private bool _isBase64EncodeSelected = true;
        private bool _isBase64DecodeSelected;
        private ObservableCollection<string> _history;

        public string InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }

        public string OutputText
        {
            get => _outputText;
            set => SetProperty(ref _outputText, value);
        }

        public bool IsBase64EncodeSelected
        {
            get => _isBase64EncodeSelected;
            set => SetProperty(ref _isBase64EncodeSelected, value);
        }

        public bool IsBase64DecodeSelected
        {
            get => _isBase64DecodeSelected;
            set => SetProperty(ref _isBase64DecodeSelected, value);
        }

        public ObservableCollection<string> History
        {
            get => _history;
            set => SetProperty(ref _history, value);
        }

        public ICommand ConvertCommand { get; }
        public ICommand CopyToClipboardCommand { get; }

        public FormattersPageViewModel()
        {
            _ = InitializeAsync();

            ConvertCommand = new DelegateCommand(OnConvert);
            CopyToClipboardCommand = new DelegateCommand<string>(OnCopyToClipboard);
        }

        public async Task InitializeAsync()
        {
            _history = new ObservableCollection<string>();
        }

        private void OnConvert()
        {
            if (!string.IsNullOrEmpty(InputText))
            {
                if (IsBase64EncodeSelected)
                {
                    var plainTextBytes = Encoding.UTF8.GetBytes(InputText);
                    OutputText = Convert.ToBase64String(plainTextBytes);
                }
                else if (IsBase64DecodeSelected)
                {
                    var base64EncodedBytes = Convert.FromBase64String(InputText);
                    OutputText = Encoding.UTF8.GetString(base64EncodedBytes);
                }
                History.Add(OutputText);
                InputText = string.Empty; // Clear input text after conversion
            }
        }

        private void OnCopyToClipboard(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.SetText(text);
            }
        }
    }
}