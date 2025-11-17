using DryIoc;
using Prism.DryIoc;
using Prism.Ioc;
using Serilog;
using Tools.Library.Extensions;
using Tools.Library.Services; // Added using statement
using Tools.Services;
using Tools.Views.Pages;
using Tools.Views.Windows;
using Wpf.Ui;

namespace Tools;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
    public static Assembly Assembly => Assembly.GetExecutingAssembly();

    public static Type DefaultPage => typeof(DashboardPage);

    protected override Window CreateShell()
    {
        var applicationWindow = Container.Resolve<MainWindow>();
        return applicationWindow;
    }

    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.AddTransientFromNamespace("Tools.Views", Assembly);
        containerRegistry.AddTransientFromNamespace("Tools.ViewModels", Assembly);

        containerRegistry.RegisterSingleton<INavigationService, NavigationService>();
        containerRegistry.RegisterSingleton<ISnackbarService, SnackbarService>();
        containerRegistry.RegisterSingleton<IContentDialogService, ContentDialogService>();
        containerRegistry.RegisterSingleton<IClipboardPasswordService, ClipboardPasswordService>();

        // Register the new settings service
        containerRegistry.RegisterSingleton<ISettingsService, SettingsService>();
    }

    private void OnStartup(object sender, StartupEventArgs e)
    {
        Log.Logger = new LoggerConfiguration()
                   .MinimumLevel.Error()
                   .WriteTo.File("logs\\log.txt", rollingInterval: RollingInterval.Day)
                   .CreateLogger();
        RegisterGlobalExceptionHandling(Log.Logger);

        Log.Logger.Information("Dev Tools Started");
    }

    /// <summary>
    /// Occurs when the application is closing.
    /// </summary>
    private void OnExit(object sender, ExitEventArgs e)
    {
        // Use Prism's Container property directly
        if (Container != null)
        {
            //var snapServiceContainer = Container.Resolve<ISnapService>(); // Example usage
            //if (snapServiceContainer != null)
            //{
            //    snapServiceContainer.Release();
            //    NotifyIcon.Dispose();
            //}
        }

        Log.Logger.Information("Dev Tools Exited");
    }

    private void RegisterGlobalExceptionHandling(ILogger log)
    {
        // this is the line you really want
        AppDomain.CurrentDomain.UnhandledException +=
            (sender, args) => CurrentDomainOnUnhandledException(args, log);

        // optional: hooking up some more handlers
        // remember that you need to hook up additional handlers when
        // logging from other dispatchers, shedulers, or applications
        DispatcherUnhandledException += (sender, args) => CurrentOnDispatcherUnhandledException(args, log);

        Dispatcher.UnhandledException += (sender, args) => DispatcherOnUnhandledException(args, log);

        TaskScheduler.UnobservedTaskException +=
            (sender, args) => TaskSchedulerOnUnobservedTaskException(args, log);
    }

    private static void CurrentDomainOnUnhandledException(UnhandledExceptionEventArgs args, ILogger log)
    {
        var exception = args.ExceptionObject as Exception;
        var terminatingMessage = args.IsTerminating ? " The application is terminating." : string.Empty;
        var exceptionMessage = exception?.Message ?? "An unmanaged exception occured.";
        var message = string.Concat(exceptionMessage, terminatingMessage);
        log.Error(exception, message);
    }

    private static void CurrentOnDispatcherUnhandledException(DispatcherUnhandledExceptionEventArgs args, ILogger log)
    {
        log.Error(args.Exception, args.Exception.Message);
        args.Handled = true;
    }

    private static void DispatcherOnUnhandledException(DispatcherUnhandledExceptionEventArgs args, ILogger log)
    {
        log.Error(args.Exception, args.Exception.Message);
        args.Handled = true;
    }

    private static void TaskSchedulerOnUnobservedTaskException(UnobservedTaskExceptionEventArgs args, ILogger log)
    {
        log.Error(args.Exception, args.Exception.Message);
        args.SetObserved();
    }
}