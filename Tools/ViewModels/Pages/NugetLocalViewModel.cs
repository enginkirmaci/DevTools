﻿using Microsoft.Win32;
using Prism.Commands;
using Prism.Mvvm;
using Tools.Library.Entities;
using Tools.Library.Services;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Tools.ViewModels.Pages;

public class NugetLocalViewModel : BindableBase
{
    private readonly ISnackbarService snackbarService;
    private readonly ISettingsService _settingsService;
    private FileSystemWatcher? watcher;
    private ObservableCollection<string> fileList;

    private string watchFolder;

    private string copyFolder;
    private bool watchStarted = false;
    private int count = 0;
    private DateTime lastChanges = DateTime.Now;
    private NugetLocalSettings _nugetSettings;
    public ObservableCollection<string> FileList { get => fileList; set => SetProperty(ref fileList, value); }
    public string WatchFolder { get => watchFolder; set => SetProperty(ref watchFolder, value); }
    public string CopyFolder { get => copyFolder; set => SetProperty(ref copyFolder, value); }
    public bool WatchStarted { get => watchStarted; set => SetProperty(ref watchStarted, value); }
    public int Count { get => count; set => SetProperty(ref count, value); }

    public DelegateCommand<object> WatchChangesCommand { get; private set; }
    public DelegateCommand<string> TextboxClickCommand { get; private set; }

    public NugetLocalViewModel(
           ISnackbarService snackbarService,
           ISettingsService settingsService)
    {
        this.snackbarService = snackbarService;
        _settingsService = settingsService;

        _ = InitializeAsync();

        WatchChangesCommand = new DelegateCommand<object>(WatchChangesCommandMethod);
        TextboxClickCommand = new DelegateCommand<string>(TextboxClickCommandMethod);
    }

    public async Task InitializeAsync()
    {
        FileList = new ObservableCollection<string>();

        var settings = await _settingsService.GetSettingsAsync();
        _nugetSettings = settings.NugetLocal ?? new NugetLocalSettings();

        WatchFolder = _nugetSettings.WatchFolder ?? string.Empty;
        CopyFolder = _nugetSettings.CopyFolder ?? string.Empty;
    }

    private void TextboxClickCommandMethod(string operation)
    {
        var folderDialog = new OpenFolderDialog
        {
            InitialDirectory = watchFolder
        };

        if (folderDialog.ShowDialog() == true)
        {
            if (operation == "Watch")
            {
                WatchFolder = folderDialog.FolderName;
            }
            else
            {
                CopyFolder = folderDialog.FolderName;
            }
        }
    }

    private void WatchChangesCommandMethod(object started)
    {
        if ((bool)started)
        {
            if (string.IsNullOrEmpty(WatchFolder) || !Directory.Exists(WatchFolder))
            {
                snackbarService.Show("Error", "Watch folder path is invalid or not set in settings.", ControlAppearance.Danger, null, TimeSpan.FromSeconds(5));
                WatchStarted = false;

                return;
            }
            if (string.IsNullOrEmpty(CopyFolder) || !Directory.Exists(CopyFolder))
            {
                snackbarService.Show("Error", "Copy folder path is invalid or not set in settings.", ControlAppearance.Danger, null, TimeSpan.FromSeconds(5));
                WatchStarted = false;

                return;
            }

            watcher = new FileSystemWatcher(WatchFolder);
            watcher.Created += FileCreated;
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = true;
            watcher.Filter = _nugetSettings.NugetPackageFilter;

            WatchStarted = true;
        }
        else if (watcher != null)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
            watcher = null;

            WatchStarted = false;
        }
    }

    private void FileCreated(object sender, FileSystemEventArgs e)
    {
        // Use Dispatcher.InvokeAsync for modern async/await pattern with dispatcher
        _ = Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            // Use delay from settings
            await Task.Delay(_nugetSettings.FileCopyDelayMs);

            // Check against CopyFolder property which is loaded from settings
            if (!e.FullPath.Contains(CopyFolder, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    File.Copy(e.FullPath, Path.Combine(CopyFolder, Path.GetFileName(e.FullPath)), true);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error copying file {e.FullPath}: {ex.Message}");
                    snackbarService.Show("Copy Error", $"Failed to copy {Path.GetFileName(e.FullPath)}: {ex.Message}", ControlAppearance.Danger, null, TimeSpan.FromSeconds(10));
                    // Optionally add failed copy attempt to FileList
                    FileList.Insert(0, $"ERROR copying {e.FullPath}: {ex.Message}");
                    return; // Stop processing this file on error
                }

                FileList.Insert(0, $"{e.FullPath} moved.");

                Debug.WriteLine("File copied: " + e.FullPath);

                // Use interval from settings
                if (DateTime.Now < lastChanges.AddSeconds(_nugetSettings.CountResetIntervalSeconds))
                {
                    Count++;
                }
                else
                {
                    Count = 1;
                    lastChanges = DateTime.Now;
                }
            }
        });
    }
}