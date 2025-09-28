using Prism.Commands;
using Prism.Mvvm;
using Tools.Library.Entities;
using Tools.Library.Services;

namespace Tools.ViewModels.Pages;

public class WorkspacesViewModel : BindableBase
{
    public DelegateCommand<string> OpenSolutionCommand { get; set; }
    public DelegateCommand<string> OpenFolderCommand { get; set; }
    public DelegateCommand<string> OpenWithVSCodeCommand { get; set; }
    public DelegateCommand RefreshCommand { get; set; }

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
        RefreshCommand = new DelegateCommand(async () =>
        {
            Workspaces = new ObservableCollection<WorkspaceItem>();
            Platforms = new ObservableCollection<WorkspaceItem>();

            await InitializeAsync();
        });

        FilteredWorkspaces = new ObservableCollection<WorkspaceItem>();
        FilteredPlatforms = new ObservableCollection<WorkspaceItem>();

        // Do not block the UI thread; fire-and-forget async initialization
        _ = InitializeAsync();
    }

    // Helper to safely run async initialization without blocking UI thread

    public async Task InitializeAsync()
    {
        // Offload blocking I/O to background thread
        var settings = await _settingsService.GetSettingsAsync();
        _workspaceSettings = settings.Workspaces ?? new WorkspacesSettings();

        if (_workspaceSettings.WorkspaceScanFolders != null &&
            _workspaceSettings.WorkspaceScanFolders.Any() &&
            !Workspaces.Any() && !Platforms.Any())
        {
            var workspaces = new List<WorkspaceItem>();
            var platforms = new List<WorkspaceItem>();

            foreach (var folderPath in _workspaceSettings.WorkspaceScanFolders)
            {
                if (!Directory.Exists(folderPath))
                {
                    Debug.WriteLine($"Warning: Workspace scan folder not found: {folderPath}");
                    continue;
                }
                try
                {
                    // Offload directory scanning to background thread
                    var directories = await Task.Run(() =>
                        GetAccessibleDirectoriesRecursively(folderPath, _workspaceSettings.GitFolderPattern).ToList()
                    );

                    foreach (var dir in directories)
                    {
                        var parentDir = Path.GetDirectoryName(dir);
                        if (parentDir == null) continue;

                        string[] solutionFiles = Array.Empty<string>();
                        // Offload file search to background thread
                        await Task.Run(() =>
                        {
                            solutionFiles = Directory.GetFiles(parentDir, _workspaceSettings.SolutionFilePattern);
                        });

                        foreach (var solutionFile in solutionFiles)
                        {
                            workspaces.Add(new WorkspaceItem
                            {
                                SolutionName = Path.GetFileNameWithoutExtension(solutionFile),
                                FolderPath = Path.GetDirectoryName(solutionFile),
                                SolutionPath = solutionFile
                            });
                        }

                        if (dir.Contains(_workspaceSettings.PlatformFolderName, StringComparison.OrdinalIgnoreCase))
                        {
                            var platformDir = parentDir;
                            if (Directory.Exists(platformDir))
                            {
                                platforms.Add(new WorkspaceItem
                                {
                                    PlatformName = Path.GetFileName(platformDir.TrimEnd(Path.DirectorySeparatorChar)),
                                    FolderPath = platformDir
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error scanning folder {folderPath}: {ex.Message}");
                }
            }

            // Update collections on UI thread
            App.Current.Dispatcher.Invoke(() =>
            {
                Workspaces = new ObservableCollection<WorkspaceItem>(workspaces.OrderBy(w => w.SolutionName));
                Platforms = new ObservableCollection<WorkspaceItem>(platforms.OrderBy(p => p.PlatformName));
                ApplyFilter();
            });
        }
        else
        {
            ApplyFilter();
        }
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

    private IEnumerable<string> GetAccessibleDirectoriesRecursively(string rootPath, string searchPattern)
    {
        var foundDirectories = new List<string>();
        try
        {
            foreach (var dir in Directory.EnumerateDirectories(rootPath, searchPattern, SearchOption.TopDirectoryOnly))
            {
                foundDirectories.Add(dir);
            }

            var excludedFolders = _workspaceSettings?.ExcludedFolders ?? Array.Empty<string>();

            foreach (var subDir in Directory.EnumerateDirectories(rootPath))
            {
                var subDirName = Path.GetFileName(subDir);

                if (subDirName.Equals(searchPattern, StringComparison.OrdinalIgnoreCase) ||
                    excludedFolders.Any(excluded => excluded.Equals(subDir, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                foundDirectories.AddRange(GetAccessibleDirectoriesRecursively(subDir, searchPattern));
            }
        }
        catch (UnauthorizedAccessException)
        {
            Debug.WriteLine($"Access denied to folder: {rootPath}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error accessing folder {rootPath}: {ex.Message}");
        }

        return foundDirectories;
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