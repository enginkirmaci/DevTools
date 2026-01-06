using Microsoft.UI.Xaml;
using Tools.Helpers;
using Tools.Library.Entities;
using Tools.Library.Services.Abstractions;
using Tools.ViewModels.Windows;
using Tools.Views.Pages;

namespace Tools.Views.Windows;

/// <summary>
/// Timer notification window - compact overlay for break notifications.
/// </summary>
public sealed partial class TimerNotificationWindow : Window
{
    #region Constants

    private const int WindowWidth = 280;
    private const int WindowHeight = 440;

    #endregion Constants

    #region Fields

    private readonly WindowConfigurator _windowConfigurator;
    private readonly IFocusTimerService _focusTimerService;
    private readonly INavigationService _navigationService;
    private WindowCornerPosition _currentPosition = WindowCornerPosition.BottomRight;

    #endregion Fields

    #region Properties

    /// <summary>
    /// Gets the ViewModel for this window.
    /// </summary>
    public TimerNotificationWindowViewModel ViewModel { get; }

    #endregion Properties

    #region Constructor

    public TimerNotificationWindow(
        TimerNotificationWindowViewModel viewModel,
        IFocusTimerService focusTimerService,
        INavigationService navigationService)
    {
        ViewModel = viewModel;
        _focusTimerService = focusTimerService;
        _navigationService = navigationService;
        _windowConfigurator = new WindowConfigurator(this);

        InitializeComponent();
        InitializeWindow();
        SubscribeToEvents();
    }

    #endregion Constructor

    #region Initialization

    private void InitializeWindow()
    {
        _windowConfigurator.ConfigureBackdrop();
        _windowConfigurator.ConfigureSizeAndPosition();
        _windowConfigurator.SetCompactOverlayStyle(WindowWidth, WindowHeight);
        _windowConfigurator.ConfigureCustomTitleBar(CustomTitleBar);
        _windowConfigurator.HideFromTaskbar();

        // Position in configured corner
        _currentPosition = (WindowCornerPosition)_focusTimerService.Settings.WindowCornerPosition;
        _windowConfigurator.PositionInCorner(_currentPosition);
    }

    private void SubscribeToEvents()
    {
        _focusTimerService.WindowVisibilityRequested += OnWindowVisibilityRequested;
        _focusTimerService.BreakNotificationTriggered += OnBreakNotificationTriggered;
        _focusTimerService.StateChanged += OnStateChanged;
        Closed += OnWindowClosed;

        // Subscribe to IsPinned property changes to update pin icon
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;

        // Initialize button visibility
        UpdateButtonVisibility(_focusTimerService.CurrentState.Status);

        // Initialize pin icon
        UpdatePinIcon();
    }

    #endregion Initialization

    #region Event Handlers

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.IsPinned))
        {
            DispatcherQueue.TryEnqueue(UpdatePinIcon);
        }
    }

    private void OnWindowVisibilityRequested(object? sender, bool shouldShow)
    {
        System.Diagnostics.Debug.WriteLine($"[TimerWindow] OnWindowVisibilityRequested: shouldShow={shouldShow}");
        DispatcherQueue.TryEnqueue(() =>
        {
            if (shouldShow)
            {
                System.Diagnostics.Debug.WriteLine("[TimerWindow] Calling ShowWindow()");
                ShowWindow();
            }
            else
            {
                HideWindow();
            }
        });
    }

    private void OnBreakNotificationTriggered(object? sender, EventArgs e)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            // Bring window to front on notification
            _windowConfigurator.BringToFront();
        });
    }

    private void OnStateChanged(object? sender, FocusTimerStateChangedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[TimerWindow] OnStateChanged: Status={e.State.Status}, TimeUntilNextBreak={e.State.TimeUntilNextBreak?.TotalMinutes:F1}min, BreakTimeRemaining={e.State.BreakTimeRemaining?.TotalMinutes:F1}min");
        DispatcherQueue.TryEnqueue(() =>
        {
            UpdateButtonVisibility(e.State.Status);
        });
    }

    private void OnWindowClosed(object sender, WindowEventArgs args)
    {
        _focusTimerService.WindowVisibilityRequested -= OnWindowVisibilityRequested;
        _focusTimerService.BreakNotificationTriggered -= OnBreakNotificationTriggered;
        _focusTimerService.StateChanged -= OnStateChanged;
    }

    private void OnPinToggle(object sender, RoutedEventArgs e)
    {
        ViewModel.TogglePinCommand.Execute(null);
        _windowConfigurator.SetAlwaysOnTop(ViewModel.IsPinned);
    }

    private void OnCloseClick(object sender, RoutedEventArgs e)
    {
        HideWindow();
    }

    #endregion Event Handlers

    #region Private Methods

    private void UpdatePinIcon()
    {
        // Pinned: Show Unpin icon (E840)
        // Unpinned: Show Pin icon (E718)
        PinIcon.Glyph = ViewModel.IsPinned ? "\uE77A" : "\uE718";
    }

    private void UpdateButtonVisibility(FocusTimerStatus status)
    {
        // Hide all button panels
        StoppedButtons.Visibility = Visibility.Collapsed;
        WorkingButtons.Visibility = Visibility.Collapsed;
        NotificationButtons.Visibility = Visibility.Collapsed;
        BreakActiveButtons.Visibility = Visibility.Collapsed;

        // Show appropriate panel based on status
        switch (status)
        {
            case FocusTimerStatus.Stopped:
            case FocusTimerStatus.DayEnded:
                StoppedButtons.Visibility = Visibility.Visible;
                break;

            case FocusTimerStatus.Working:
            case FocusTimerStatus.LunchMode:
                WorkingButtons.Visibility = Visibility.Visible;
                break;

            case FocusTimerStatus.NotificationTriggered:
                NotificationButtons.Visibility = Visibility.Visible;
                break;

            case FocusTimerStatus.BreakActive:
                BreakActiveButtons.Visibility = Visibility.Visible;
                break;
        }
    }

    #endregion Private Methods

    #region Public Methods

    /// <summary>
    /// Shows the timer window.
    /// </summary>
    public void ShowWindow()
    {
        // Use configurator to show and activate the window, then apply topmost based on pin state
        _windowConfigurator.Show();
        _windowConfigurator.SetAlwaysOnTop(ViewModel.IsPinned);
    }

    /// <summary>
    /// Hides the timer window.
    /// </summary>
    public void HideWindow()
    {
        // Hide the window using Win32 ShowWindow to truly hide it
        _windowConfigurator.Hide();
    }

    /// <summary>
    /// Updates the window position based on settings.
    /// </summary>
    /// <param name="position">The corner position.</param>
    public void UpdatePosition(WindowCornerPosition position)
    {
        _currentPosition = position;
        _windowConfigurator.PositionInCorner(position);
    }

    /// <summary>
    /// Brings the window to the front.
    /// </summary>
    public void BringToFront()
    {
        _windowConfigurator.BringToFront();
    }

    #endregion Public Methods
}