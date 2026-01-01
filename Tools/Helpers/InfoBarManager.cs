using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;

namespace Tools.Helpers;

/// <summary>
/// Manages InfoBar display and auto-close functionality.
/// Implements Single Responsibility Principle - only responsible for info bar management.
/// </summary>
public sealed class InfoBarManager
{
    #region Constants
    private const int DefaultAutoCloseSeconds = 5;
    #endregion

    #region Fields
    private readonly InfoBar _infoBar;
    private readonly DispatcherQueue _dispatcherQueue;
    private DispatcherQueueTimer? _timer;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the InfoBarManager.
    /// </summary>
    /// <param name="infoBar">The InfoBar control to manage.</param>
    /// <param name="dispatcherQueue">The dispatcher queue for timer operations.</param>
    public InfoBarManager(InfoBar infoBar, DispatcherQueue dispatcherQueue)
    {
        _infoBar = infoBar;
        _dispatcherQueue = dispatcherQueue;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Shows an informational message bar.
    /// </summary>
    /// <param name="title">The title of the message.</param>
    /// <param name="message">The message content.</param>
    /// <param name="severity">The severity level of the message.</param>
    /// <param name="autoCloseSeconds">The number of seconds before auto-closing. Use 0 to disable auto-close.</param>
    public void Show(
        string title,
        string message,
        InfoBarSeverity severity = InfoBarSeverity.Informational,
        int autoCloseSeconds = DefaultAutoCloseSeconds)
    {
        _infoBar.Title = title;
        _infoBar.Message = message;
        _infoBar.Severity = severity;
        _infoBar.IsOpen = true;

        if (autoCloseSeconds > 0)
        {
            AutoClose(autoCloseSeconds);
        }
    }

    /// <summary>
    /// Closes the info bar.
    /// </summary>
    public void Close()
    {
        _timer?.Stop();
        _infoBar.IsOpen = false;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Auto-closes the info bar after a delay.
    /// </summary>
    /// <param name="seconds">The number of seconds to wait before closing.</param>
    private void AutoClose(int seconds)
    {
        _timer?.Stop();

        _timer = _dispatcherQueue.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(seconds);
        _timer.Tick += OnTimerTick;
        _timer.Start();
    }

    /// <summary>
    /// Handles the timer tick event.
    /// </summary>
    private void OnTimerTick(DispatcherQueueTimer sender, object args)
    {
        Close();
    }
    #endregion
}
