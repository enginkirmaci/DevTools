using Microsoft.UI.Dispatching;
using Serilog;
using Tools.Library.Extensions;
using Tools.Library.Services;
using Tools.Library.Services.Abstractions;
using Tools.Services;
using Tools.ViewModels.Pages;
using Tools.ViewModels.Windows;
using Tools.Views.Pages;
using Tools.Views.Windows;

namespace Tools;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    public static IHost Host { get; private set; } = null!;
    public static IServiceProvider Services => Host.Services;
    public static Type DefaultPage => typeof(DashboardPage);

    private Window? _mainWindow;

    /// <summary>
    /// Initializes the singleton application object.
    /// </summary>
    public App()
    {
        this.InitializeComponent();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Error()
            .WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // Build the host with DI
        Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                ConfigureServices(services);
            })
            .Build();

        Log.Logger.Information("Dev Tools Started");
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Register core library services
        services.AddCoreServices();

        // Register application services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IClipboardPasswordService, ClipboardPasswordService>();

        // Register Focus Timer service (needs DispatcherQueue from UI thread)
        services.AddSingleton<IFocusTimerService>(sp =>
        {
            var settingsService = sp.GetRequiredService<ISettingsService>();
            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            return new FocusTimerService(settingsService, dispatcherQueue);
        });

        // Register windows and view models
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();

        // Register Timer Notification Window (singleton to reuse)
        services.AddSingleton<TimerNotificationWindow>();
        services.AddSingleton<TimerNotificationWindowViewModel>();

        // Register pages and view models
        RegisterPageWithViewModel<DashboardPage, DashboardViewModel>(services);
        RegisterPageWithViewModel<WorkspacesPage, WorkspacesViewModel>(services);
        RegisterPageWithViewModel<FormattersPage, FormattersPageViewModel>(services);
        RegisterPageWithViewModel<NugetLocalPage, NugetLocalViewModel>(services);
        RegisterPageWithViewModel<EFToolsPage, EFToolsPageViewModel>(services);
        RegisterPageWithViewModel<CodeExecutePage, CodeExecutePageViewModel>(services);
        RegisterPageWithViewModel<ClipboardPasswordPage, ClipboardPasswordPageViewModel>(services);
        RegisterPageWithViewModel<HostFileProxyPage, HostFileProxyViewModel>(services);
        RegisterPageWithViewModel<FocusTimerSettingsPage, FocusTimerSettingsViewModel>(services);
    }

    private static void RegisterPageWithViewModel<TPage, TViewModel>(IServiceCollection services)
        where TPage : class
        where TViewModel : class
    {
        services.AddTransient<TPage>();
        services.AddTransient<TViewModel>();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = Services.GetRequiredService<MainWindow>();
        _mainWindow.Activate();

        // Initialize the timer notification window (but don't show it yet)
        // This ensures event handlers are set up before the timer service fires events
        //_ = Services.GetRequiredService<TimerNotificationWindow>();
    }

    public static TService GetService<TService>() where TService : class
    {
        return Services.GetRequiredService<TService>();
    }

    public static Window MainWindow => ((App)Current)._mainWindow!;
}