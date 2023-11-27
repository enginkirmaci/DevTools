using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Prism.Commands;
using Prism.Mvvm;
using Tools.Library.Extensions;
using Tools.Services;
using Wpf.Ui.Appearance;

namespace Tools.ViewModels;

public class ShellWindowModel : BindableBase
{
    private Window mainWindow;
    private readonly INavigationService navigationService;

    //private readonly ISettingsService settingsService;
    private bool canGoBack;

    public bool CanGoBack { get => canGoBack; set => SetProperty(ref canGoBack, value); }

    public DelegateCommand GoBackCommand { get; set; }
    public DelegateCommand<Window> LoadedCommand { get; private set; }
    public DelegateCommand<CancelEventArgs> ClosingWindowCommand { get; private set; }

    public ShellWindowModel(INavigationService navigationService)
    //ISettingsService settingsService)
    {
        this.navigationService = navigationService;
        //this.settingsService = settingsService;

        GoBackCommand = new DelegateCommand(GoBack);

        LoadedCommand = new DelegateCommand<Window>(async (window) =>
        {
            //await settingsService.InitializeAsync();

            mainWindow = window;
            var mainFrame = mainWindow.FindChild<Frame>("MainFrame");
            mainFrame.Navigating += MainFrame_Navigating;
            mainFrame.Navigated += MainFrame_Navigated;

            ChangeTheme();

            navigationService.Navigate("HomeView", Regions.MainRegion);
        });

        ClosingWindowCommand = new DelegateCommand<CancelEventArgs>((args) =>
        {
            args.Cancel = true;

            Application.Current.Shutdown();
        });
    }

    private void MainFrame_Navigated(object sender, NavigationEventArgs e)
    {
        CanGoBack = navigationService.CanGoBack();
    }

    private void MainFrame_Navigating(object sender, NavigatingCancelEventArgs e)
    {
        if (e.NavigationMode == NavigationMode.Back)
        {
            e.Cancel = true;

            if (CanGoBack)
            {
                GoBack();
            }
        }
    }

    private void GoBack()
    {
        navigationService.GoBack();
    }

    private void ChangeTheme()
    {
        //switch (settingsService.Settings.AppTheme)
        //{
        //    case UITheme.Dark:
        //        Theme.Apply(ThemeType.Dark, BackgroundType.Mica, false, true);
        //        break;

        //    case UITheme.Light:
        //        Theme.Apply(ThemeType.Light, BackgroundType.Mica, false, true);
        //        break;

        //    case UITheme.System:
        var system = Theme.GetSystemTheme();
        switch (system)
        {
            case SystemThemeType.Light:
            case SystemThemeType.Sunrise:
            case SystemThemeType.Flow:
                Theme.Apply(ThemeType.Light, BackgroundType.Mica, true, true);
                break;

            case SystemThemeType.Dark:
            case SystemThemeType.Glow:
            case SystemThemeType.CapturedMotion:
                Theme.Apply(ThemeType.Dark, BackgroundType.Mica, true, true);
                break;
        }

        //        break;
        //}
    }
}