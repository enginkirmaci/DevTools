using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tools.Library.Extensions;
using Windows.ApplicationModel.DataTransfer;

namespace Tools.ViewModels.Pages;

public partial class FormattersPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _inputText = string.Empty;

    [ObservableProperty]
    private string _outputText = string.Empty;

    [ObservableProperty]
    private bool _isBase64EncodeSelected = true;

    [ObservableProperty]
    private bool _isBase64DecodeSelected;

    [ObservableProperty]
    private bool _isSnakeCaseSelected;

    [ObservableProperty]
    private bool _isPascalCaseSelected;

    [ObservableProperty]
    private ObservableCollection<string> _history = new();

    public IRelayCommand ConvertCommand { get; }
    public IRelayCommand<string> CopyToClipboardCommand { get; }

    public FormattersPageViewModel()
    {
        ConvertCommand = new RelayCommand(OnConvert);
        CopyToClipboardCommand = new RelayCommand<string>(OnCopyToClipboard);
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
                    try
                    {
                        output.AppendLine(Encoding.UTF8.GetString(Convert.FromBase64String(line)));
                    }
                    catch
                    {
                        output.AppendLine($"[Invalid Base64: {line}]");
                    }
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
            InputText = string.Empty;
        }
    }

    private void OnCopyToClipboard(string? text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(text);
            Clipboard.SetContent(dataPackage);
        }
    }
}