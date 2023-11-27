using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FileAndFolderDialog.Abstractions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Tools.Services;

namespace Tools.ViewModels;

public class NugetLocalViewModel : BindableBase, INavigationAware
{
    private readonly INavigationService navigationService;
    private readonly IFolderDialogService folderDialogService;

    private FileSystemWatcher watcher;
    private ObservableCollection<string> fileList;
    private string watchFolder = "C:\\Repos\\STAKILPR\\Clearing.Common";
    private string copyFolder = "C:\\Repos\\STAKILPR\\Clearing.Common\\nuget";
    private bool watchStarted = false;

    public ObservableCollection<string> FileList { get => fileList; set => SetProperty(ref fileList, value); }
    public string WatchFolder { get => watchFolder; set => SetProperty(ref watchFolder, value); }
    public string CopyFolder { get => copyFolder; set => SetProperty(ref copyFolder, value); }
    public bool WatchStarted { get => watchStarted; set => SetProperty(ref watchStarted, value); }

    public DelegateCommand<object> WatchChangesCommand { get; private set; }
    public DelegateCommand<string> TextboxClickCommand { get; private set; }

    public NugetLocalViewModel(
           INavigationService navigationService,
           IFolderDialogService folderDialogService)
    {
        this.navigationService = navigationService;
        this.folderDialogService = folderDialogService;

        _ = InitializeAsync();

        WatchChangesCommand = new DelegateCommand<object>(WatchChangesCommandMethod);
        TextboxClickCommand = new DelegateCommand<string>(TextboxClickCommandMethod);
    }

    private void TextboxClickCommandMethod(string operation)
    {
        var result = folderDialogService.ShowSelectFolderDialog(new SelectFolderOptions() { SelectedPath = watchFolder });
        if (!string.IsNullOrEmpty(result))
        {
            if (operation == "Watch")
            {
                WatchFolder = result;
            }
            else
            {
                CopyFolder = result;
            }
        }
    }

    private void WatchChangesCommandMethod(object started)
    {
        if ((bool)started)
        {
            watcher = new FileSystemWatcher();
            watcher.Path = WatchFolder;
            watcher.Created += FileCreated;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            watcher.Filter = "*.nupkg";

            WatchStarted = true;
        }
        else
        {
            watcher.EndInit();
            watcher = null;

            WatchStarted = false;
        }
    }

    private void FileCreated(object sender, FileSystemEventArgs e)
    {
        Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(async () =>
        {
            await Task.Delay(2000);

            if (!e.FullPath.Contains(copyFolder))
            {
                File.Copy(e.FullPath, Path.Combine(copyFolder, Path.GetFileName(e.FullPath)), true);

                FileList.Insert(0, $"{e.FullPath} moved.");

                Debug.WriteLine("File created: " + e.FullPath);
            }
        }));
    }

    public async Task InitializeAsync()
    {
        FileList = new ObservableCollection<string>();
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        _ = InitializeAsync();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
}