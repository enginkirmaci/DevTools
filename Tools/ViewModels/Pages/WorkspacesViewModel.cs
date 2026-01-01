using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Tools.Library.Configuration;
using Tools.Library.Models;
using Tools.Library.Mvvm;
using Tools.Library.Services.Abstractions;

namespace Tools.ViewModels.Pages;

/// <summary>
/// ViewModel for the Workspaces page.
/// </summary>
public partial class WorkspacesViewModel : PageViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly DispatcherQueue _dispatcherQueue;
    private WorkspacesSettings _workspaceSettings = new();

    private static ObservableCollection<WorkspaceItem> _workspaces = new();
    private static ObservableCollection<WorkspaceItem> _platforms = new();

    [ObservableProperty]
    private string _filterText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<WorkspaceItem> _filteredWorkspaces = new();

    [ObservableProperty]
    private ObservableCollection<WorkspaceItem> _filteredPlatforms = new();

    /// <summary>
    /// Gets the command to open a solution.
    /// </summary>
    public IRelayCommand<string> OpenSolutionCommand { get; }

    /// <summary>
    /// Gets the command to open a folder.
    /// </summary>
    public IRelayCommand<string> OpenFolderCommand { get; }

    /// <summary>
    /// Gets the command to open with VS Code.
    /// </summary>
    public IRelayCommand<string> OpenWithVSCodeCommand { get; }

    /// <summary>
    /// Gets the command to refresh workspaces.
    /// </summary>
    public IRelayCommand RefreshCommand { get; }

    public WorkspacesViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        OpenSolutionCommand = new RelayCommand<string>(OnOpenSolution);
        OpenFolderCommand = new RelayCommand<string>(OnOpenFolder);
        OpenWithVSCodeCommand = new RelayCommand<string>(OnOpenWithVSCode);
        RefreshCommand = new RelayCommand(async () =>
        {
            _workspaces = new ObservableCollection<WorkspaceItem>();
            _platforms = new ObservableCollection<WorkspaceItem>();
            await OnInitializeAsync();
        });

        _ = OnInitializeAsync();
    }

    /// <inheritdoc/>
    public override async Task OnInitializeAsync()
    {
        var settings = await _settingsService.GetSettingsAsync();
        _workspaceSettings = settings.Workspaces ?? new WorkspacesSettings();

        if (_workspaceSettings.WorkspaceScanFolders != null &&
            _workspaceSettings.WorkspaceScanFolders.Any() &&
            !_workspaces.Any() && !_platforms.Any())
        {
            await ScanWorkspacesAsync();
        }
        else
        {
            ApplyFilter();
        }
    }

    partial void OnFilterTextChanged(string value)
    {
        ApplyFilter();
    }

    private async Task ScanWorkspacesAsync()
    {
        var workspaces = new List<WorkspaceItem>();
        var platforms = new List<WorkspaceItem>();

        foreach (var folderPath in _workspaceSettings.WorkspaceScanFolders!)
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.WriteLine($"Warning: Workspace scan folder not found: {folderPath}");
                continue;
            }

            try
            {
                var directories = await Task.Run(() =>
                    GetAccessibleDirectoriesRecursively(folderPath, _workspaceSettings.GitFolderPattern ?? ".git").ToList()
                );

                foreach (var dir in directories)
                {
                    var parentDir = Path.GetDirectoryName(dir);
                    if (parentDir == null) continue;

                    string[] solutionFiles = Array.Empty<string>();
                    await Task.Run(() =>
                    {
                        solutionFiles = Directory.GetFiles(parentDir, _workspaceSettings.SolutionFilePattern ?? "*.sln");
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

                    if (dir.Contains(_workspaceSettings.PlatformFolderName ?? "platform", StringComparison.OrdinalIgnoreCase))
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
        _dispatcherQueue.TryEnqueue(() =>
        {
            _workspaces = new ObservableCollection<WorkspaceItem>(workspaces.OrderBy(w => w.SolutionName));
            _platforms = new ObservableCollection<WorkspaceItem>(platforms.OrderBy(p => p.PlatformName));
            ApplyFilter();
        });
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(FilterText))
        {
            FilteredWorkspaces = new ObservableCollection<WorkspaceItem>(_workspaces);
            FilteredPlatforms = new ObservableCollection<WorkspaceItem>(_platforms);
        }
        else
        {
            FilteredWorkspaces = new ObservableCollection<WorkspaceItem>(
                _workspaces.Where(w => w.SolutionName?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) == true));
            FilteredPlatforms = new ObservableCollection<WorkspaceItem>(
                _platforms.Where(p => p.PlatformName?.Contains(FilterText, StringComparison.OrdinalIgnoreCase) == true));
        }
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

    private void OnOpenSolution(string? solutionPath)
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

    private void OnOpenFolder(string? folderPath)
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

    private void OnOpenWithVSCode(string? folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = _workspaceSettings?.VSCodeExecutable ?? "code",
            Arguments = folderPath,
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });
    }
}
