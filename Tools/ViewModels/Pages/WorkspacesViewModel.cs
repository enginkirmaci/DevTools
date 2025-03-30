using Prism.Commands;
using Prism.Mvvm;
using Tools.Library.Entities;
using Tools.Library.Services; // Add using for SettingsService

namespace Tools.ViewModels.Pages;

public class WorkspacesViewModel : BindableBase
{
    public DelegateCommand<string> OpenSolutionCommand { get; set; }
    public DelegateCommand<string> OpenFolderCommand { get; set; }
    public DelegateCommand<string> OpenWithVSCodeCommand { get; set; }

    public static ObservableCollection<WorkspaceItem> Workspaces { get; set; } = new ObservableCollection<WorkspaceItem>();
    public static ObservableCollection<WorkspaceItem> Platforms { get; set; } = new ObservableCollection<WorkspaceItem>();
    public ObservableCollection<WorkspaceItem> FilteredWorkspaces { get; set; }
    public ObservableCollection<WorkspaceItem> FilteredPlatforms { get; set; }

    private static string _filterText = string.Empty;
    private readonly ISettingsService _settingsService;
    private WorkspacesSettings _workspaceSettings;

    public string FilterText
    {
        get => _filterText;
        set
        {
            SetProperty(ref _filterText, value);
            ApplyFilter();
        }
    }

    public WorkspacesViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;

        OpenSolutionCommand = new DelegateCommand<string>(OpenSolution);
        OpenFolderCommand = new DelegateCommand<string>(OpenFolder);
        OpenWithVSCodeCommand = new DelegateCommand<string>(OpenWithVSCode);

        FilteredWorkspaces = new ObservableCollection<WorkspaceItem>();
        FilteredPlatforms = new ObservableCollection<WorkspaceItem>();

        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        var settings = _settingsService.GetSettings();
        _workspaceSettings = settings.Workspaces ?? new WorkspacesSettings();

        // Check if settings provide folders and if workspaces/platforms are empty
        if (_workspaceSettings.WorkspaceScanFolders != null &&
            _workspaceSettings.WorkspaceScanFolders.Any() &&
            !Workspaces.Any() && !Platforms.Any())
        {
            foreach (var folderPath in _workspaceSettings.WorkspaceScanFolders)
            {
                if (!Directory.Exists(folderPath))
                {
                    Debug.WriteLine($"Warning: Workspace scan folder not found: {folderPath}");
                    continue; // Skip non-existent folders
                }
                try
                {
                    var directories = Directory.GetDirectories(folderPath, _workspaceSettings.GitFolderPattern, SearchOption.AllDirectories);
                    foreach (var dir in directories)
                    {
                        var parentDir = Path.GetDirectoryName(dir);
                        if (parentDir == null) continue;

                        var solutionFiles = Directory.GetFiles(parentDir, _workspaceSettings.SolutionFilePattern);
                        foreach (var solutionFile in solutionFiles)
                        {
                            Workspaces.Add(new WorkspaceItem
                            {
                                SolutionName = Path.GetFileNameWithoutExtension(solutionFile),
                                FolderPath = Path.GetDirectoryName(solutionFile),
                                SolutionPath = solutionFile
                            });
                        }

                        // Use platform folder name from settings for comparison
                        if (dir.Contains(_workspaceSettings.PlatformFolderName, StringComparison.OrdinalIgnoreCase))
                        {
                            // More robust removal of the git pattern part (e.g., ".git") from the directory path
                            var platformDir = parentDir; // Start with the parent directory of the .git folder
                            if (Directory.Exists(platformDir)) // Ensure the directory exists
                            {
                                Platforms.Add(new WorkspaceItem
                                {
                                    PlatformName = Path.GetFileName(platformDir.TrimEnd(Path.DirectorySeparatorChar)),
                                    FolderPath = platformDir
                                });
                            }
                        }
                    } // End of inner foreach (var dir...)
                } // End of try block <-- CORRECT PLACEMENT
                catch (Exception ex) // Catch potential exceptions during directory scanning
                {
                    Debug.WriteLine($"Error scanning folder {folderPath}: {ex.Message}");
                    // Optionally notify the user via Snackbar or log more formally
                }
            } // End of outer foreach (var folderPath...)

            Workspaces = new ObservableCollection<WorkspaceItem>(Workspaces.OrderBy(w => w.SolutionName));
            Platforms = new ObservableCollection<WorkspaceItem>(Platforms.OrderBy(p => p.PlatformName));
        }

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(FilterText))
        {
            FilteredWorkspaces = new ObservableCollection<WorkspaceItem>(Workspaces);
            FilteredPlatforms = new ObservableCollection<WorkspaceItem>(Platforms);
        }
        else
        {
            FilteredWorkspaces = new ObservableCollection<WorkspaceItem>(
                Workspaces.Where(w => w.SolutionName.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));
            FilteredPlatforms = new ObservableCollection<WorkspaceItem>(
                Platforms.Where(p => p.PlatformName.Contains(FilterText, StringComparison.OrdinalIgnoreCase)));
        }
        RaisePropertyChanged(nameof(FilteredWorkspaces));
        RaisePropertyChanged(nameof(FilteredPlatforms));
    }

    private void OpenSolution(string solutionPath)
    {
        if (string.IsNullOrWhiteSpace(solutionPath))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = solutionPath,
            UseShellExecute = true
        });
    }

    private void OpenFolder(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = folderPath,
            UseShellExecute = true
        });
    }

    private void OpenWithVSCode(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = _workspaceSettings.VSCodeExecutable,
            Arguments = folderPath,
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });
    }
}