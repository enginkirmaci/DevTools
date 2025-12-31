using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Tools.Services;
using Tools.ViewModels.Windows;
using Tools.Views.Pages;
using WinRT.Interop;
using System.Runtime.InteropServices;

namespace Tools.Views.Windows;

public sealed partial class MainWindow : Window
{
    private bool _isUserClosedPane;
    private bool _isPaneOpenedOrClosedFromCode;
    private bool _isNavigatingFromCode; // Guard to prevent re-entrancy when selecting items in code
    private readonly INavigationService _navigationService;
    private readonly IClipboardPasswordService _clipboardPasswordService;
    private AppWindow? _appWindow;

    // HWND and WndProc related
    private nint _hwnd;
    private IntPtr _oldWndProc = IntPtr.Zero;
    private WndProcDelegate? _wndProcDelegate;

    public MainWindowViewModel ViewModel { get; }

    public MainWindow(
        MainWindowViewModel viewModel,
        INavigationService navigationService,
        IClipboardPasswordService clipboardPasswordService)
    {
        ViewModel = viewModel;
        _navigationService = navigationService;
        _clipboardPasswordService = clipboardPasswordService;

        this.InitializeComponent();

        // Setup navigation service
        _navigationService.SetFrame(ContentFrame);

        // Ensure initial back button state
        NavigationView.IsBackEnabled = _navigationService.CanGoBack;

        // Subscribe to navigation events so we can update the NavigationView selection
        _navigationService.Navigated += OnNavigated;
        _navigationService.BackStackChanged += OnBackStackChanged;

        // Also update back button whenever the Frame navigates (framework journal changes)
        ContentFrame.Navigated += ContentFrame_Navigated;

        // Setup window
        SetupWindow();

        // Navigate to default page
        _navigationService.Navigate(App.DefaultPage);
        
        // Select first item
        NavigationView.SelectedItem = NavigationView.MenuItems[0];
    }

    private void ContentFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        // Update back button whenever the frame's navigation stack changes
        NavigationView.IsBackEnabled = _navigationService.CanGoBack;
    }

    private void OnBackStackChanged()
    {
        // Called when custom back stack or frame back stack changes
        NavigationView.IsBackEnabled = _navigationService.CanGoBack;
    }

    private void OnNavigated(Type? pageType)
    {
        // Always update back button enabled state when navigation occurs
        NavigationView.IsBackEnabled = _navigationService.CanGoBack;

        if (pageType == null)
            return;

        // Map page type to tag
        string? tag = pageType == typeof(DashboardPage) ? "Dashboard"
            : pageType == typeof(WorkspacesPage) ? "Workspaces"
            : pageType == typeof(NugetLocalPage) ? "NugetLocal"
            : pageType == typeof(FormattersPage) ? "Formatters"
            : pageType == typeof(ClipboardPasswordPage) ? "ClipboardPassword"
            : pageType == typeof(EFToolsPage) ? "EFTools"
            : pageType == typeof(CodeExecutePage) ? "CodeExecute"
            : null;

        if (tag == null)
            return;

        // Find matching NavigationViewItem and select it
        foreach (var menuItem in NavigationView.MenuItems)
        {
            if (menuItem is NavigationViewItem nvi)
            {
                if (nvi.Tag?.ToString() == tag)
                {
                    _isNavigatingFromCode = true;
                    NavigationView.SelectedItem = nvi;
                    _isNavigatingFromCode = false;
                    break;
                }
            }
        }
    }

    private void SetupWindow()
    {
        // Get the window handle
        _hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(_hwnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);

        // Set window size
        if (_appWindow != null)
        {
            _appWindow.Resize(new global::Windows.Graphics.SizeInt32(1450, 802));
            
            // Center window on screen
            var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
            if (displayArea != null)
            {
                var centerX = (displayArea.WorkArea.Width - 1450) / 2;
                var centerY = (displayArea.WorkArea.Height - 802) / 2;
                _appWindow.Move(new global::Windows.Graphics.PointInt32(centerX, centerY));
            }

            // Setup title bar
            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = _appWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.ButtonBackgroundColor = Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

                // Set the drag region
                SetTitleBar(AppTitleBar);
            }
        }

        // Register global hotkeys
        _clipboardPasswordService.RegisterHotKeys(_hwnd);

        // Subclass the window to receive WM_HOTKEY
        _wndProcDelegate = new WndProcDelegate(WndProc);
        var newProcPtr = Marshal.GetFunctionPointerForDelegate(_wndProcDelegate);
        _oldWndProc = SetWindowLongPtr(_hwnd, GWL_WNDPROC, newProcPtr);

        // Handle window closing
        this.Closed += MainWindow_Closed;
        this.SizeChanged += MainWindow_SizeChanged;
    }

    private void MainWindow_Closed(object sender, WindowEventArgs args)
    {
        _clipboardPasswordService.UnregisterHotKeys();

        // Unsubscribe from navigation events
        _navigationService.Navigated -= OnNavigated;
        _navigationService.BackStackChanged -= OnBackStackChanged;
        ContentFrame.Navigated -= ContentFrame_Navigated;

        // Restore original window proc
        if (_oldWndProc != IntPtr.Zero && _hwnd != nint.Zero)
        {
            SetWindowLongPtr(_hwnd, GWL_WNDPROC, _oldWndProc);
            _oldWndProc = IntPtr.Zero;
        }
    }

    private void MainWindow_SizeChanged(object sender, WindowSizeChangedEventArgs args)
    {
        if (_isUserClosedPane)
        {
            return;
        }

        _isPaneOpenedOrClosedFromCode = true;
        NavigationView.IsPaneOpen = args.Size.Width > 1200;
        _isPaneOpenedOrClosedFromCode = false;
    }

    private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (_isNavigatingFromCode)
            return;

        if (args.SelectedItemContainer is NavigationViewItem item)
        {
            var tag = item.Tag?.ToString();
            Type? pageType = tag switch
            {
                "Dashboard" => typeof(DashboardPage),
                "Workspaces" => typeof(WorkspacesPage),
                "NugetLocal" => typeof(NugetLocalPage),
                "Formatters" => typeof(FormattersPage),
                "ClipboardPassword" => typeof(ClipboardPasswordPage),
                "EFTools" => typeof(EFToolsPage),
                "CodeExecute" => typeof(CodeExecutePage),
                _ => null
            };

            if (pageType != null)
            {
                _navigationService.Navigate(pageType);
            }
        }
    }

    private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (_navigationService.CanGoBack)
        {
            _navigationService.GoBack();
            NavigationView.IsBackEnabled = _navigationService.CanGoBack;
        }
    }

    public void ShowInfoBar(string title, string message, InfoBarSeverity severity = InfoBarSeverity.Informational)
    {
        AppInfoBar.Title = title;
        AppInfoBar.Message = message;
        AppInfoBar.Severity = severity;
        AppInfoBar.IsOpen = true;

        // Auto-close after 5 seconds
        var timer = DispatcherQueue.CreateTimer();
        timer.Interval = TimeSpan.FromSeconds(5);
        timer.Tick += (s, e) =>
        {
            AppInfoBar.IsOpen = false;
            timer.Stop();
        };
        timer.Start();
    }

    // WndProc override for hotkey handling
    private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        const int WM_HOTKEY = 0x0312;
        const int HOTKEY_ID = 9000;

        if (msg == WM_HOTKEY)
        {
            if (wParam.ToInt32() == HOTKEY_ID)
            {
                // Fire-and-forget handling
                _ = _clipboardPasswordService.HandleHotkeyAsync();
                return IntPtr.Zero;
            }
        }

        return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
    }

    // PInvoke and helpers for subclassing
    private const int GWL_WNDPROC = -4;

    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtrW(IntPtr hWnd, int nIndex, IntPtr newProc);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongW", SetLastError = true)]
    private static extern int SetWindowLongW(IntPtr hWnd, int nIndex, int newProc);

    private static IntPtr SetWindowLongPtr(nint hWnd, int nIndex, IntPtr newProc)
    {
        if (IntPtr.Size == 8)
        {
            return SetWindowLongPtrW(hWnd, nIndex, newProc);
        }
        else
        {
            return new IntPtr(SetWindowLongW(hWnd, nIndex, newProc.ToInt32()));
        }
    }

    [DllImport("user32.dll", EntryPoint = "CallWindowProcW", SetLastError = true)]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
}