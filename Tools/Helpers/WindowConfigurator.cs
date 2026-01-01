using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;

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

    #endregion Public Methods
}