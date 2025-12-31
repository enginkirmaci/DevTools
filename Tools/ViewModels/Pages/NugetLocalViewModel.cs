using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Tools.Library.Entities;
using Tools.Library.Services;
using Tools.Views.Windows;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Tools.ViewModels.Pages;

public partial class NugetLocalViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private FileSystemWatcher? _watcher;
    private NugetLocalSettings _nugetSettings = new();
    private DateTime _lastChanges = DateTime.Now;
    private readonly DispatcherQueue _dispatcherQueue;

    [ObservableProperty]
    private ObservableCollection<string> _fileList = new();

    [ObservableProperty]
    private string _watchFolder = string.Empty;

    [ObservableProperty]
    private string _copyFolder = string.Empty;

    [ObservableProperty]
    private bool _watchStarted;

    [ObservableProperty]
    private int _count;

    public IRelayCommand<object> WatchChangesCommand { get; }
    public IAsyncRelayCommand<string> TextboxClickCommand { get; }

    public NugetLocalViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        WatchChangesCommand = new RelayCommand<object>(WatchChangesCommandMethod);
        TextboxClickCommand = new AsyncRelayCommand<string>(TextboxClickCommandMethod);

        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        var settings = await _settingsService.GetSettingsAsync();
        _nugetSettings = settings.NugetLocal ?? new NugetLocalSettings();

        WatchFolder = _nugetSettings.WatchFolder ?? string.Empty;
        CopyFolder = _nugetSettings.CopyFolder ?? string.Empty;
    }

    private async Task TextboxClickCommandMethod(string? operation)
    {
        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);

        // Try WinRT FolderPicker first (standard for WinUI 3)
        try
        {
            var picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.FileTypeFilter.Add("*");

            InitializeWithWindow.Initialize(picker, hwnd);

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                if (operation == "Watch")
                    WatchFolder = folder.Path;
                else
                    CopyFolder = folder.Path;

                return;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"WinRT FolderPicker failed: {ex.Message}");
            // Fall through to fallback
        }

        // Fallback to Win32 Common Item Dialog (more reliable in unpackaged apps)
        try
        {
            var title = operation == "Watch" ? "Select Watch Folder" : "Select Copy Folder";
            var path = Tools.Helpers.FolderPickerHelper.PickFolder(hwnd, title);
            
            if (!string.IsNullOrEmpty(path))
            {
                if (operation == "Watch")
                    WatchFolder = path;
                else
                    CopyFolder = path;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Native FolderPicker fallback failed: {ex.Message}");
            ShowError($"Failed to open folder picker: {ex.Message}");
        }
    }

    private void WatchChangesCommandMethod(object? started)
    {
        if (started is bool isStarted && isStarted)
        {
            if (string.IsNullOrEmpty(WatchFolder) || !Directory.Exists(WatchFolder))
            {
                ShowError("Watch folder path is invalid or not set in settings.");
                WatchStarted = false;
                return;
            }
            if (string.IsNullOrEmpty(CopyFolder) || !Directory.Exists(CopyFolder))
            {
                ShowError("Copy folder path is invalid or not set in settings.");
                WatchStarted = false;
                return;
            }

            _watcher = new FileSystemWatcher(WatchFolder);
            _watcher.Created += FileCreated;
            _watcher.EnableRaisingEvents = true;
            _watcher.IncludeSubdirectories = true;
            _watcher.Filter = _nugetSettings.NugetPackageFilter ?? "*.nupkg";

            WatchStarted = true;
        }
        else if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
            _watcher = null;

            WatchStarted = false;
        }
    }

    private void FileCreated(object sender, FileSystemEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(async () =>
        {
            await Task.Delay(_nugetSettings.FileCopyDelayMs);

            if (!e.FullPath.Contains(CopyFolder, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    File.Copy(e.FullPath, Path.Combine(CopyFolder, Path.GetFileName(e.FullPath)), true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error copying file {e.FullPath}: {ex.Message}");
                    FileList.Insert(0, $"ERROR copying {e.FullPath}: {ex.Message}");
                    return;
                }

                FileList.Insert(0, $"{e.FullPath} moved.");

                Debug.WriteLine("File copied: " + e.FullPath);

                if (DateTime.Now < _lastChanges.AddSeconds(_nugetSettings.CountResetIntervalSeconds))
                {
                    Count++;
                }
                else
                {
                    Count = 1;
                    _lastChanges = DateTime.Now;
                }
            }
        });
    }

    private void ShowError(string message)
    {
        // Show error via MainWindow InfoBar
        if (App.MainWindow is MainWindow mainWindow)
        {
            mainWindow.ShowInfoBar("Error", message, Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error);
        }
    }
}