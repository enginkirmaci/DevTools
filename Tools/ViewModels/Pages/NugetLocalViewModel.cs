using System.Runtime.InteropServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Tools.Library.Configuration;
using Tools.Library.Mvvm;
using Tools.Library.Services.Abstractions;
using Tools.Views.Windows;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Tools.ViewModels.Pages;

/// <summary>
/// ViewModel for the Nuget Local page.
/// </summary>
public partial class NugetLocalViewModel : PageViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly DispatcherQueue _dispatcherQueue;
    private FileSystemWatcher? _watcher;
    private NugetLocalSettings _nugetSettings = new();
    private DateTime _lastChanges = DateTime.Now;

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

    [ObservableProperty]
    private bool _clearCacheOnCopy;

    /// <summary>
    /// Gets the command to start/stop watching for changes.
    /// </summary>
    public IRelayCommand<object> WatchChangesCommand { get; }

    /// <summary>
    /// Gets the command to select folders.
    /// </summary>
    public IAsyncRelayCommand<string> SelectFolderCommand { get; }

    public NugetLocalViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        WatchChangesCommand = new RelayCommand<object>(OnWatchChanges);
        SelectFolderCommand = new AsyncRelayCommand<string>(OnSelectFolderAsync);

        _ = OnInitializeAsync();
    }

    /// <inheritdoc/>
    public override async Task OnInitializeAsync()
    {
        var settings = await _settingsService.GetSettingsAsync();
        _nugetSettings = settings.NugetLocal ?? new NugetLocalSettings();

        WatchFolder = _nugetSettings.WatchFolder ?? string.Empty;
        CopyFolder = _nugetSettings.CopyFolder ?? string.Empty;
        ClearCacheOnCopy = _nugetSettings.ClearCacheOnCopy;
    }

    partial void OnWatchFolderChanged(string value)
    {
        _ = SaveSettingAsync(() => _nugetSettings.WatchFolder = value);
    }

    partial void OnCopyFolderChanged(string value)
    {
        _ = SaveSettingAsync(() => _nugetSettings.CopyFolder = value);
    }

    partial void OnClearCacheOnCopyChanged(bool value)
    {
        _ = SaveSettingAsync(() => _nugetSettings.ClearCacheOnCopy = value);
    }

    private async Task SaveSettingAsync(Action updateAction)
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();
            updateAction();
            await _settingsService.SaveSettingsAsync(settings);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error saving settings: {ex.Message}");
        }
    }

    private async Task OnSelectFolderAsync(string? operation)
    {
        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);

        // Try WinRT FolderPicker first (standard for WinUI 3)
        try
        {
            var picker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.Desktop
            };
            picker.FileTypeFilter.Add("*");

            InitializeWithWindow.Initialize(picker, hwnd);

            var folder = await picker.PickSingleFolderAsync();
            if (folder != null)
            {
                UpdateFolderPath(operation, folder.Path);
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"WinRT FolderPicker failed: {ex.Message}");
        }

        // Fallback to Win32 Common Item Dialog (more reliable in unpackaged apps)
        try
        {
            var title = operation == "Watch" ? "Select Watch Folder" : "Select Copy Folder";
            var path = Tools.Helpers.FolderPickerHelper.PickFolder(hwnd, title);

            if (!string.IsNullOrEmpty(path))
            {
                UpdateFolderPath(operation, path);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Native FolderPicker fallback failed: {ex.Message}");
            ShowError($"Failed to open folder picker: {ex.Message}");
        }
    }

    private void UpdateFolderPath(string? operation, string path)
    {
        if (operation == "Watch")
        {
            WatchFolder = path;
        }
        else
        {
            CopyFolder = path;
        }
    }

    private void OnWatchChanges(object? started)
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

                // Clear NuGet cache for this package if enabled
                if (ClearCacheOnCopy)
                {
                    await ClearPackageCacheAsync(e.FullPath);
                }

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

    private async Task ClearPackageCacheAsync(string packagePath)
    {
        try
        {
            var (packageId, version) = ExtractPackageInfo(Path.GetFileName(packagePath));
            if (string.IsNullOrEmpty(packageId) || string.IsNullOrEmpty(version))
            {
                FileList.Insert(0, $"  ⚠ Could not parse package info from {Path.GetFileName(packagePath)}");
                return;
            }

            // Get global packages folder
            var globalPackagesPath = await GetGlobalPackagesFolderAsync();
            if (string.IsNullOrEmpty(globalPackagesPath))
            {
                FileList.Insert(0, "  ⚠ Could not locate NuGet global packages folder");
                return;
            }

            // Delete specific package version folder
            var packageFolderPath = Path.Combine(globalPackagesPath, packageId.ToLowerInvariant(), version.ToLowerInvariant());
            if (Directory.Exists(packageFolderPath))
            {
                Directory.Delete(packageFolderPath, true);
                FileList.Insert(0, $"  ✓ Cleared cache for {packageId} {version}");
                Debug.WriteLine($"Cleared cache: {packageFolderPath}");
            }
            else
            {
                FileList.Insert(0, $"  ℹ No cache found for {packageId} {version}");
            }
        }
        catch (Exception ex)
        {
            FileList.Insert(0, $"  ✗ Cache clear error: {ex.Message}");
            Debug.WriteLine($"Error clearing cache: {ex.Message}");
        }
    }

    private (string packageId, string version) ExtractPackageInfo(string fileName)
    {
        // Pattern: PackageId.Version.nupkg
        // Example: MyPackage.1.0.0.nupkg or MyPackage.1.0.0-beta.nupkg
        if (!fileName.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase))
        {
            return (string.Empty, string.Empty);
        }

        var nameWithoutExt = fileName.Substring(0, fileName.Length - 6); // Remove .nupkg
        var parts = nameWithoutExt.Split('.');

        // Find the first part that looks like a version number
        for (int i = parts.Length - 1; i >= 0; i--)
        {
            if (int.TryParse(parts[i], out _))
            {
                // Found a numeric part, this might be the start of version
                var packageId = string.Join(".", parts.Take(i));
                var version = string.Join(".", parts.Skip(i));
                return (packageId, version);
            }
        }

        return (string.Empty, string.Empty);
    }

    private async Task<string> GetGlobalPackagesFolderAsync()
    {
        try
        {
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "nuget locals global-packages --list",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process == null)
            {
                return string.Empty;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            // Output format: "global-packages: C:\Users\...\packages"
            var match = System.Text.RegularExpressions.Regex.Match(output, @"global-packages:\s*(.+)");
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting global packages folder: {ex.Message}");
        }

        return string.Empty;
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