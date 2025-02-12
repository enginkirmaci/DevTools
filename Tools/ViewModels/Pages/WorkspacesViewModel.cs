using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using Tools.Library.Converters;
using Wpf.Ui;

namespace Tools.ViewModels.Pages;

public class WorkspacesViewModel : BindableBase
{
    public DelegateCommand<string> OpenSolutionCommand { get; set; }
    public DelegateCommand<string> OpenFolderCommand { get; set; }

    public ObservableCollection<WorkspaceItem> Workspaces { get; set; }
    public string WorkspaceFolderPath { get; set; } = @"H:\ARCHIVE\Projects\_old";

    public WorkspacesViewModel()
    {
        OpenSolutionCommand = new DelegateCommand<string>(OpenSolution);
        OpenFolderCommand = new DelegateCommand<string>(OpenFolder); 

        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        Workspaces = new ObservableCollection<WorkspaceItem>();
        var directories = Directory.GetDirectories(WorkspaceFolderPath, "*.git", SearchOption.AllDirectories);
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
        }
        Workspaces = new ObservableCollection<WorkspaceItem>(Workspaces.OrderBy(w => w.SolutionName));
    }

    private void OpenSolution(string solutionPath)
    {
        if (string.IsNullOrWhiteSpace(solutionPath))
        {
            return;
        }

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
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

        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = folderPath,
            UseShellExecute = true
        });
    }
}

public class WorkspaceItem
{
    public string SolutionName { get; set; }
    public string FolderPath { get; set; }
    public string SolutionPath { get; set; }
}