using System.Runtime.InteropServices;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Tools.Library.Services.Abstractions;
using Tools.ViewModels.Windows;
using Tools.Views.Pages;
using WinRT.Interop;

using Tools.Helpers;
using Tools.Library.Providers;
using Tools.Library.Converters;

namespace Tools.Views.Windows;

/// <summary>
/// Main application window with navigation, hotkey support, and window management.
/// Implements Interface Segregation Principle - depends only on required service interfaces.
/// </summary>
public sealed partial class MainWindow : Window
{
    #region Constants

    private const int NavigationPaneThreshold = 1200;

    #endregion Constants

    #region Fields

    private readonly INavigationService _navigationService;
    private readonly IClipboardPasswordService _clipboardPasswordService;
    private readonly WindowMessageHandler _messageHandler;
    private readonly WindowConfigurator _windowConfigurator;
    private readonly InfoBarManager _infoBarManager;

    private bool _isUserClosedPane;
    private bool _isPaneOpenedOrClosedFromCode;
    private bool _isNavigatingFromCode;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the ViewModel for this window.
    /// </summary>
    public MainWindowViewModel ViewModel { get; }

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the MainWindow.
    /// </summary>
    public MainWindow(
        MainWindowViewModel viewModel,
        INavigationService navigationService,
        IClipboardPasswordService clipboardPasswordService)
    {
        ViewModel = viewModel;
        _navigationService = navigationService;
        _clipboardPasswordService = clipboardPasswordService;

        // Initialize helper classes (Dependency Inversion Principle)
        _messageHandler = new WindowMessageHandler(clipboardPasswordService);
        _windowConfigurator = new WindowConfigurator(this);

        InitializeComponent();

        // Initialize InfoBarManager after InitializeComponent
        _infoBarManager = new InfoBarManager(AppInfoBar, DispatcherQueue);

        InitializeNavigation();
        InitializeWindow();
        NavigateToDefaultPage();
    }

    #endregion Constructor

    #region Initialization

    /// <summary>
    /// Initializes navigation service and event subscriptions.
    /// </summary>
    private void InitializeNavigation()
    {
        // Populate NavigationView menu items using the shared provider so tags/keys remain consistent
        var menuItems = NavigationProvider.GetNavigationMenuItems();
        foreach (var mi in menuItems)
        {
            NavigationView.MenuItems.Add(mi);
        }

        _navigationService.SetFrame(ContentFrame);
        _navigationService.Navigated += OnNavigated;
        _navigationService.BackStackChanged += OnBackStackChanged;
        ContentFrame.Navigated += OnContentFrameNavigated;

        AppTitleBar.IsBackButtonEnabled = _navigationService.CanGoBack;
    }

    /// <summary>
    /// Navigates to the default application page.
    /// </summary>
    private void NavigateToDefaultPage()
    {
        _navigationService.Navigate(App.DefaultPage);
        NavigationView.SelectedItem = NavigationView.MenuItems[0];
    }

    /// <summary>
    /// Handles content frame navigation events.
    /// </summary>
    private void OnContentFrameNavigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        UpdateBackButtonState();
    }

    /// <summary>
    /// Handles navigation back stack changes.
    /// </summary>
    private void OnBackStackChanged()
    {
        UpdateBackButtonState();
    }

    /// <summary>
    /// Handles navigation completion and updates NavigationView selection.
    /// </summary>
    private void OnNavigated(Type? pageType)
    {
        UpdateBackButtonState();

        if (pageType == null) return;

        var tag = PageNavigationMapper.GetTagFromPageType(pageType);
        if (tag == null) return;

        SelectNavigationItem(tag);
    }

    /// <summary>
    /// Updates the back button enabled state.
    /// </summary>
    private void UpdateBackButtonState()
    {
        AppTitleBar.IsBackButtonEnabled = _navigationService.CanGoBack;
    }

    /// <summary>
    /// Selects the navigation item matching the specified tag.
    /// </summary>
    private void SelectNavigationItem(string tag)
    {
        foreach (var menuItem in NavigationView.MenuItems)
        {
            if (menuItem is NavigationViewItem nvi && nvi.Tag?.ToString() == tag)
            {
                _isNavigatingFromCode = true;
                NavigationView.SelectedItem = nvi;

                if ((string?)nvi.Tag == "DashboardPage")
                {
                    NavigationView.Header = string.Empty;
                    NavigationView.AlwaysShowHeader = false;
                }
                else
                {
                    NavigationView.Header = nvi.Content;
                    NavigationView.AlwaysShowHeader = true;
                }

                _isNavigatingFromCode = false;
                break;
            }
        }
    }

    #endregion Initialization

    #region UI Event Handlers

    /// <summary>
    /// Initializes window appearance, size, position, and event handlers.
    /// </summary>
    private void InitializeWindow()
    {
        _windowConfigurator.Configure();
        ConfigureHotkeys();
        SubscribeToWindowEvents();
    }

    /// <summary>
    /// Configures global hotkey handling.
    /// </summary>
    private void ConfigureHotkeys()
    {
        _clipboardPasswordService.RegisterHotKeys(_windowConfigurator.WindowHandle);
        _messageHandler.Install(_windowConfigurator.WindowHandle);
    }

    /// <summary>
    /// Subscribes to window events.
    /// </summary>
    private void SubscribeToWindowEvents()
    {
        Closed += OnWindowClosed;
        SizeChanged += OnWindowSizeChanged;
    }

    #endregion UI Event Handlers

    #region Navigation Event Handlers

    /// <summary>
    /// Handles window closing event and performs cleanup.
    /// </summary>
    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        Cleanup();

        App.Current.Exit();
    }

    /// <summary>
    /// Handles window size changes and adjusts navigation pane.
    /// </summary>
    private void OnWindowSizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        if (_isUserClosedPane) return;

        _isPaneOpenedOrClosedFromCode = true;
        NavigationView.IsPaneOpen = args.Size.Width > NavigationPaneThreshold;
        _isPaneOpenedOrClosedFromCode = false;
    }

    /// <summary>
    /// Performs cleanup when window is closing.
    /// </summary>
    private void Cleanup()
    {
        _clipboardPasswordService.UnregisterHotKeys();

        _navigationService.Navigated -= OnNavigated;
        _navigationService.BackStackChanged -= OnBackStackChanged;
        ContentFrame.Navigated -= OnContentFrameNavigated;

        _messageHandler.Uninstall(_windowConfigurator.WindowHandle);
    }

    /// <summary>
    /// Handles NavigationView selection changes.
    /// Uses NameToPageTypeConverter to match page keys/tags the same way as DashboardViewModel.
    /// </summary>
    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (_isNavigatingFromCode) return;

        if (args.SelectedItemContainer is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            var pageType = NameToPageTypeConverter.Convert(tag ?? string.Empty);

            if (pageType != null)
            {
                _navigationService.Navigate(pageType);
            }
        }
    }

    private void TitleBar_BackRequested(TitleBar sender, object args)
    {
        NavigateBack();
    }

    /// <summary>
    /// Handles NavigationView back button requests.
    /// </summary>
    private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        NavigateBack();
    }

    /// <summary>
    /// Navigates back if possible.
    /// </summary>
    private void NavigateBack()
    {
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
            UpdateBackButtonState();
        }
    }

    #endregion Navigation Event Handlers

    #region Public Methods

    /// <summary>
    /// Shows an informational message bar.
    /// Delegates to InfoBarManager (Dependency Inversion Principle).
    /// </summary>
    /// <param name="title">The title of the message.</param>
    /// <param name="message">The message content.</param>
    /// <param name="severity">The severity level of the message.</param>
    public void ShowInfoBar(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
    {
        _infoBarManager.Show(title, message, severity);
    }

    #endregion Public Methods
}