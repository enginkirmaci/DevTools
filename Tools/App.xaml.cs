using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Tools.Library.Services;
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
        // Register Windows
        services.AddSingleton<MainWindow>();
        services.AddSingleton<MainWindowViewModel>();

        // Register Pages
        services.AddTransient<DashboardPage>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<WorkspacesPage>();
        services.AddTransient<WorkspacesViewModel>();
        services.AddTransient<FormattersPage>();
        services.AddTransient<FormattersPageViewModel>();
        services.AddTransient<NugetLocalPage>();
        services.AddTransient<NugetLocalViewModel>();
        services.AddTransient<EFToolsPage>();
        services.AddTransient<EFToolsPageViewModel>();
        services.AddTransient<CodeExecutePage>();
        services.AddTransient<CodeExecutePageViewModel>();
        services.AddTransient<ClipboardPasswordPage>();
        services.AddTransient<ClipboardPasswordPageViewModel>();
        services.AddTransient<HostFileProxyPage>();
        services.AddTransient<HostFileProxyViewModel>();

        // Register Services
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IClipboardPasswordService, ClipboardPasswordService>();
    }

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = Services.GetRequiredService<MainWindow>();
        _mainWindow.Activate();
    }

    public static TService GetService<TService>() where TService : class
    {
        return Services.GetRequiredService<TService>();
    }

    public static Window MainWindow => ((App)Current)._mainWindow!;
}

/// <summary>
/// Simple BooleanToVisibilityConverter for WinUI 3
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility v && v == Visibility.Visible;
    }
}