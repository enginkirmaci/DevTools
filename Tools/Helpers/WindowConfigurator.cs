using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;
using Tools.Library.Entities;

namespace Tools.Helpers;

/// <summary>
/// Configures window appearance, size, position, and system integration.
/// Implements Single Responsibility Principle - only responsible for window configuration.
/// </summary>
public sealed class WindowConfigurator
{
    #region Constants

    private const int DefaultWindowWidth = 1400;
    private const int DefaultWindowHeight = 800;

    #endregion Constants

    #region Fields

    private readonly Window _window;
    private AppWindow? _appWindow;
    private nint _hwnd;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the window handle.
    /// </summary>
    public nint WindowHandle => _hwnd;

    /// <summary>
    /// Gets the AppWindow instance.
    /// </summary>
    public AppWindow? AppWindow => _appWindow;

    #endregion Properties

    #region Constructor

    /// <summary>
    /// Initializes a new instance of the WindowConfigurator.
    /// </summary>
    /// <param name="window">The window to configure.</param>
    public WindowConfigurator(Window window)
    {
        _window = window;
    }

    #endregion Constructor

    #region Public Methods

    /// <summary>
    /// Configures the window with backdrop, size, position, and title bar.
    /// </summary>
    /// <param name="titleBarElement">The UIElement to use as the title bar drag region.</param>
    public void Configure(UIElement? titleBarElement = null)
    {
        ConfigureBackdrop();
        ConfigureSizeAndPosition();
        ConfigureTitleBar();
    }

    /// <summary>
    /// Configures the window backdrop (Mica effect).
    /// </summary>
    public void ConfigureBackdrop()
    {
        _window.SystemBackdrop = new MicaBackdrop { Kind = MicaKind.BaseAlt };
    }

    /// <summary>
    /// Configures window size and centers it on screen.
    /// </summary>
    public void ConfigureSizeAndPosition()
    {
        _hwnd = WindowNative.GetWindowHandle(_window);
        var windowId = Win32Interop.GetWindowIdFromWindow(_hwnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);

        if (_appWindow == null) return;

        _appWindow.Resize(new global::Windows.Graphics.SizeInt32(DefaultWindowWidth, DefaultWindowHeight));

        var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);
        if (displayArea != null)
        {
            var centerX = (displayArea.WorkArea.Width - DefaultWindowWidth) / 2;
            var centerY = (displayArea.WorkArea.Height - DefaultWindowHeight) / 2;
            _appWindow.Move(new global::Windows.Graphics.PointInt32(centerX, centerY));
        }
    }

    /// <summary>
    /// Configures custom title bar appearance.
    /// </summary>
    /// <param name="titleBarElement">The UIElement to use as the title bar drag region.</param>
    public void ConfigureTitleBar(UIElement? titleBarElement = null)
    {
        if (_appWindow == null || !AppWindowTitleBar.IsCustomizationSupported()) return;

        var titleBar = _appWindow.TitleBar;
        titleBar.ExtendsContentIntoTitleBar = true;
        titleBar.ButtonBackgroundColor = Colors.Transparent;
        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

        if (titleBarElement != null)
        {
            _window.SetTitleBar(titleBarElement);
        }
    }

    /// <summary>
    /// Sets the window size.
    /// </summary>
    /// <param name="width">The window width.</param>
    /// <param name="height">The window height.</param>
    public void SetSize(int width, int height)
    {
        _appWindow?.Resize(new global::Windows.Graphics.SizeInt32(width, height));
    }

    /// <summary>
    /// Centers the window on the screen.
    /// </summary>
    public void CenterWindow()
    {
        if (_appWindow == null) return;

        var windowId = Win32Interop.GetWindowIdFromWindow(_hwnd);
        var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);

        if (displayArea != null)
        {
            var size = _appWindow.Size;
            var centerX = (displayArea.WorkArea.Width - size.Width) / 2;
            var centerY = (displayArea.WorkArea.Height - size.Height) / 2;
            _appWindow.Move(new global::Windows.Graphics.PointInt32(centerX, centerY));
        }
    }

    /// <summary>
    /// Sets the window to be always on top (topmost).
    /// </summary>
    /// <param name="isAlwaysOnTop">True to make the window always on top, false otherwise.</param>
    public void SetAlwaysOnTop(bool isAlwaysOnTop)
    {
        if (_appWindow?.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsAlwaysOnTop = isAlwaysOnTop;
        }
    }

    /// <summary>
    /// Positions the window in a specific corner of the screen.
    /// </summary>
    /// <param name="position">The corner position.</param>
    /// <param name="margin">Margin from the screen edge in pixels.</param>
    public void PositionInCorner(WindowCornerPosition position, int margin = 20)
    {
        if (_appWindow == null) return;

        var windowId = Win32Interop.GetWindowIdFromWindow(_hwnd);
        var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Primary);

        if (displayArea == null) return;

        var size = _appWindow.Size;
        var workArea = displayArea.WorkArea;
        int x, y;

        switch (position)
        {
            case WindowCornerPosition.TopLeft:
                x = workArea.X + margin;
                y = workArea.Y + margin;
                break;

            case WindowCornerPosition.TopRight:
                x = workArea.X + workArea.Width - size.Width - margin;
                y = workArea.Y + margin;
                break;

            case WindowCornerPosition.BottomLeft:
                x = workArea.X + margin;
                y = workArea.Y + workArea.Height - size.Height - margin;
                break;

            case WindowCornerPosition.BottomRight:
            default:
                x = workArea.X + workArea.Width - size.Width - margin;
                y = workArea.Y + workArea.Height - size.Height - margin;
                break;
        }

        _appWindow.Move(new global::Windows.Graphics.PointInt32(x, y));
    }

    /// <summary>
    /// Brings the window to the front and activates it.
    /// </summary>
    public void BringToFront()
    {
        if (_hwnd == 0) return;

        // Use Win32 API to bring window to front
        SetForegroundWindow(_hwnd);
        _window.Activate();
    }

    /// <summary>
    /// Configures the window as a compact overlay (small, minimal chrome).
    /// </summary>
    /// <param name="width">Window width.</param>
    /// <param name="height">Window height.</param>
    public void SetCompactOverlayStyle(int width, int height)
    {
        if (_appWindow == null) return;

        // Resize to compact dimensions
        _appWindow.Resize(new global::Windows.Graphics.SizeInt32(width, height));

        // Configure presenter for overlay style
        if (_appWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMinimizable = true;
            presenter.IsMaximizable = false;
            presenter.IsAlwaysOnTop = true;

            // Hide default titlebar for custom titlebar
            presenter.SetBorderAndTitleBar(true, false);
        }
    }

    /// <summary>
    /// Configures a custom title bar with drag region.
    /// </summary>
    /// <param name="titleBarElement">The UIElement to use as the drag region.</param>
    public void ConfigureCustomTitleBar(UIElement titleBarElement)
    {
        if (_appWindow == null) return;

        // Set the drag region for the custom title bar
        // The entire titleBarElement will be draggable
        _window.ExtendsContentIntoTitleBar = true;
        _window.SetTitleBar(titleBarElement);
    }

    /// <summary>
    /// Hides the window from the taskbar and Alt+Tab switcher.
    /// </summary>
    public void HideFromTaskbar()
    {
        if (_appWindow != null)
        {
            _appWindow.IsShownInSwitchers = false;
        }
    }

    /// <summary>
    /// Hides the window using Win32 ShowWindow(SW_HIDE).
    /// </summary>
    public void Hide()
    {
        if (_hwnd == 0) return;
        ShowWindow(_hwnd, SW_HIDE);
    }

    /// <summary>
    /// Shows the window using Win32 ShowWindow(SW_SHOW) and activates it.
    /// </summary>
    public void Show()
    {
        if (_hwnd == 0) return;
        ShowWindow(_hwnd, SW_SHOW);
        SetForegroundWindow(_hwnd);
        _window.Activate();
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(nint hWnd);

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern bool ShowWindow(nint hWnd, int nCmdShow);

    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;

    #endregion Public Methods
}