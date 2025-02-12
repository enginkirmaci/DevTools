using Prism.Commands;
using Prism.Mvvm;
using Tools.Library.Entities;

namespace Tools.ViewModels.Pages;

public class WorkspacesViewModel : BindableBase
{
    public DelegateCommand<string> OpenSolutionCommand { get; set; }
    public DelegateCommand<string> OpenFolderCommand { get; set; }
    public DelegateCommand<string> OpenWithVSCodeCommand { get; set; }

    public ObservableCollection<WorkspaceItem> Workspaces { get; set; }
    public ObservableCollection<WorkspaceItem> Platforms { get; set; }

    public string[] FolderPaths { get; set; } = new string[] {
        @"C:\Repos\CLEARING",
        @"C:\Repos\STAKILPR"
    };

    public WorkspacesViewModel()
    {
        OpenSolutionCommand = new DelegateCommand<string>(OpenSolution);
        OpenFolderCommand = new DelegateCommand<string>(OpenFolder);
        OpenWithVSCodeCommand = new DelegateCommand<string>(OpenWithVSCode);

        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        Workspaces = new ObservableCollection<WorkspaceItem>();
        Platforms = new ObservableCollection<WorkspaceItem>();

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