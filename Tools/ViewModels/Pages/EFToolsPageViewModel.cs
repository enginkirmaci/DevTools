using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tools.Library.Formatters;
using Tools.Library.Services;
using Windows.ApplicationModel.DataTransfer;

namespace Tools.ViewModels.Pages;

public partial class EFToolsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string _sqlInput = string.Empty;

    [ObservableProperty]
    private string _cSharpOutput = string.Empty;

    [ObservableProperty]
    private string _repositoryOutput = string.Empty;

    [ObservableProperty]
    private string _repositoryTemplate = string.Empty;

    public IRelayCommand ConvertSqlCommand { get; }
    public IRelayCommand CopyToClipboardCommand { get; }
    public IRelayCommand CopyRepositoryToClipboardCommand { get; }

    private readonly ISettingsService _settingsService;

    public EFToolsPageViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;

        ConvertSqlCommand = new RelayCommand(OnConvertSqlCommand);
        CopyToClipboardCommand = new RelayCommand(OnCopyToClipboardCommand);
        CopyRepositoryToClipboardCommand = new RelayCommand(OnCopyRepositoryToClipboardCommand);

        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        var settings = await _settingsService.GetSettingsAsync();
        RepositoryTemplate = settings.EFToolsPage?.RepositoryTemplate ?? string.Empty;
    }

    private void OnConvertSqlCommand()
    {
        try
        {
            CSharpOutput = SqlToCSharpFormatter.FormatCreateTableToClass(SqlInput);
            var tableName = SqlToCSharpFormatter.GetTableName(SqlInput);
            RepositoryOutput = GenerateRepositoryCode(tableName);
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
            var dataPackage = new DataPackage();
            dataPackage.SetText(CSharpOutput);
            Clipboard.SetContent(dataPackage);
        }
    }

    private void OnCopyRepositoryToClipboardCommand()
    {
        if (!string.IsNullOrEmpty(RepositoryOutput))
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(RepositoryOutput);
            Clipboard.SetContent(dataPackage);
        }
    }

    private string GenerateRepositoryCode(string className)
    {
        return RepositoryTemplate.Replace("{TABLENAME}", className);
    }
}