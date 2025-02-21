using FileAndFolderDialog.Abstractions;
using Prism.Commands;
using Prism.Mvvm;
using Wpf.Ui;

namespace Tools.ViewModels.Pages;

public class NugetLocalViewModel : BindableBase
{
    private readonly IFolderDialogService folderDialogService;
    private readonly ISnackbarService snackbarService;
    private FileSystemWatcher watcher;
    private ObservableCollection<string> fileList;
    private string watchFolder = "C:\\Repos\\STAKILPR-V2\\Clearing.Common";
    private string copyFolder = "C:\\Repos\\STAKILPR-V2\\Clearing.Common\\nuget";
    private bool watchStarted = false;
    private int count = 0;
    private DateTime lastChanges = DateTime.Now;

    public ObservableCollection<string> FileList { get => fileList; set => SetProperty(ref fileList, value); }
    public string WatchFolder { get => watchFolder; set => SetProperty(ref watchFolder, value); }
    public string CopyFolder { get => copyFolder; set => SetProperty(ref copyFolder, value); }
    public bool WatchStarted { get => watchStarted; set => SetProperty(ref watchStarted, value); }
    public int Count { get => count; set => SetProperty(ref count, value); }

    public DelegateCommand<object> WatchChangesCommand { get; private set; }
    public DelegateCommand<string> TextboxClickCommand { get; private set; }

    public NugetLocalViewModel(
           IFolderDialogService folderDialogService,
           ISnackbarService snackbarService)
    {
        this.folderDialogService = folderDialogService;
        this.snackbarService = snackbarService;

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

                if (DateTime.Now < lastChanges.AddSeconds(60))
                {
                    Count++;
                }
                else
                {
                    Count = 1;
                    lastChanges = DateTime.Now;
                }
            }
        }));
    }

    public async Task InitializeAsync()
    {
        FileList = new ObservableCollection<string>();
    }
}