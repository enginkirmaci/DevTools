using Prism.Commands;
using Prism.Mvvm;
using Tools.Library.Formatters;

namespace Tools.ViewModels.Pages
{
    public class EFToolsPageViewModel : BindableBase
    {
        private const string Template = @"
public class {TABLENAME}Repository : QueryableRepository<{TABLENAME}, DbContext>, I{TABLENAME}Repository
{
    public {TABLENAME}Repository(DbContext context) : base(context)
    {
    }
}

public interface I{TABLENAME}Repository : IQueryableRepository<{TABLENAME}>
{
}";

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

        public EFToolsPageViewModel()
        {
            ConvertSqlCommand = new DelegateCommand(OnConvertSqlCommand);
            CopyToClipboardCommand = new DelegateCommand(OnCopyToClipboardCommand);
            CopyRepositoryToClipboardCommand = new DelegateCommand(OnCopyRepositoryToClipboardCommand);

            RepositoryTemplate = Template;
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