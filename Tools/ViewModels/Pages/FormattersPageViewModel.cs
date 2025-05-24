using Prism.Commands;
using Prism.Mvvm;
using Tools.Library.Extensions;

namespace Tools.ViewModels.Pages
{
    public class FormattersPageViewModel : BindableBase
    {
        private string _inputText;
        private string _outputText;
        private bool _isBase64EncodeSelected = true;
        private bool _isBase64DecodeSelected;
        private bool _isSnakeCaseSelected;
        private bool _isPascalCaseSelected;
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

        public bool IsSnakeCaseSelected
        {
            get => _isSnakeCaseSelected;
            set => SetProperty(ref _isSnakeCaseSelected, value);
        }

        public bool IsPascalCaseSelected
        {
            get => _isPascalCaseSelected;
            set => SetProperty(ref _isPascalCaseSelected, value);
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
                var lines = InputText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

                var output = new StringBuilder();
                foreach (var line in lines)
                {
                    if (IsBase64EncodeSelected)
                    {
                        output.AppendLine(Convert.ToBase64String(Encoding.UTF8.GetBytes(line)));
                    }
                    else if (IsBase64DecodeSelected)
                    {
                        output.AppendLine(Encoding.UTF8.GetString(Convert.FromBase64String(line)));
                    }
                    else if (IsSnakeCaseSelected)
                    {
                        output.AppendLine(line.ToSnakeCase().ToUpperInvariant());
                    }
                    else if (IsPascalCaseSelected)
                    {
                        output.AppendLine(line.ToPascalCase());
                    }
                }
                OutputText = output.ToString();

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