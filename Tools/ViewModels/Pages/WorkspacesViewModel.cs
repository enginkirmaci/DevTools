using Prism.Commands;
using Prism.Mvvm;
using Tools.Library.Entities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

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

    private static string _filterText;
    public string FilterText
    {
        get => _filterText;
        set
        {
            SetProperty(ref _filterText, value);
            ApplyFilter();
        }
    }

    public string[] FolderPaths { get; set; } = new string[] {
        @"C:\Repos\CLEARING",
        @"C:\Repos\STAKILPR"
    };

    public WorkspacesViewModel()
    {
        OpenSolutionCommand = new DelegateCommand<string>(OpenSolution);
        OpenFolderCommand = new DelegateCommand<string>(OpenFolder);
        OpenWithVSCodeCommand = new DelegateCommand<string>(OpenWithVSCode);

        FilteredWorkspaces = new ObservableCollection<WorkspaceItem>();
        FilteredPlatforms = new ObservableCollection<WorkspaceItem>();

        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        if (!Workspaces.Any() && !Platforms.Any())
        {
            foreach (var folderPath in FolderPaths)
            {
                var directories = Directory.GetDirectories(folderPath, "*.git", SearchOption.AllDirectories);
                foreach (var dir in directories)
                {
                    var solutionFiles = Directory.GetFiles(Path.GetDirectoryName(dir), "*.sln");
                    foreach (var solutionFile in solutionFiles)
                    {
                        Workspaces.Add(new WorkspaceItem
                        {
                            SolutionName = Path.GetFileNameWithoutExtension(solutionFile),
                            FolderPath = Path.GetDirectoryName(solutionFile),
                            SolutionPath = solutionFile
                        });
                    }

                    if (dir.Contains("platform"))
                    {
                        var platformDir = dir.Replace(".git", string.Empty);

                        Platforms.Add(new WorkspaceItem
                        {
                            PlatformName = Path.GetFileName(platformDir.TrimEnd(Path.DirectorySeparatorChar)),
                            FolderPath = platformDir
                        });
                    }
                }
            }

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
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
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
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
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
            FileName = "code",
            Arguments = folderPath,
            UseShellExecute = true,
            CreateNoWindow = true,
            WindowStyle = ProcessWindowStyle.Hidden
        });
    }
}