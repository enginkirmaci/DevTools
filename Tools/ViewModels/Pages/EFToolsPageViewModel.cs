using Prism.Commands;
using Prism.Mvvm;
using Tools.Library.Formatters;
using Tools.Library.Services; // Add using for SettingsService
using Tools.Library.Entities; // Add using for Settings Entities

namespace Tools.ViewModels.Pages
{
    public class EFToolsPageViewModel : BindableBase
    {
        private string _sqlInput;
        private string _cSharpOutput;
        private string _repositoryOutput;
        private string _repositoryTemplate;

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

        public string RepositoryOutput
        {
            get => _repositoryOutput;
            set => SetProperty(ref _repositoryOutput, value);
        }

        public string RepositoryTemplate
        {
            get => _repositoryTemplate;
            set => SetProperty(ref _repositoryTemplate, value);
        }

        public ICommand ConvertSqlCommand { get; }
        public ICommand CopyToClipboardCommand { get; }
        public ICommand CopyRepositoryToClipboardCommand { get; }

        private readonly ISettingsService _settingsService;

        public EFToolsPageViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService;

            ConvertSqlCommand = new DelegateCommand(OnConvertSqlCommand);
            CopyToClipboardCommand = new DelegateCommand(OnCopyToClipboardCommand);
            CopyRepositoryToClipboardCommand = new DelegateCommand(OnCopyRepositoryToClipboardCommand);

            // Load template from settings, fallback to default
            var settings = _settingsService.GetSettings();
            RepositoryTemplate = settings.EFToolsPage.RepositoryTemplate;
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
                Clipboard.SetText(CSharpOutput);
            }
        }

        private void OnCopyRepositoryToClipboardCommand()
        {
            if (!string.IsNullOrEmpty(RepositoryOutput))
            {
                Clipboard.SetText(RepositoryOutput);
            }
        }

        private string GenerateRepositoryCode(string className)
        {
            return RepositoryTemplate.Replace("{TABLENAME}", className);
        }
    }
}