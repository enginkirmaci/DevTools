using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Media;
using Tools.Library.Entities;
using Tools.Library.Mvvm;
using Tools.Library.Services.Abstractions;

namespace Tools.ViewModels.Windows;

/// <summary>
/// ViewModel for the Timer Notification Window.
/// </summary>
public partial class TimerNotificationWindowViewModel : ViewModelBase
{
    #region Fields

    private readonly IFocusTimerService _focusTimerService;
    private readonly DispatcherQueue _dispatcherQueue;

    #endregion Fields

    #region Observable Properties

    [ObservableProperty]
    private string _countdownDisplay = "--:--";

    [ObservableProperty]
    private string _statusMessage = "Timer stopped";

    [ObservableProperty]
    private double _breakBankPercentage;

    [ObservableProperty]
    private double _breakPoolMinutes;

    [ObservableProperty]
    private int _remainingBreakCount;

    [ObservableProperty]
    private FocusTimerStatus _currentStatus = FocusTimerStatus.Stopped;

    [ObservableProperty]
    private bool _isNotificationState;

    [ObservableProperty]
    private bool _isBreakActiveState;

    [ObservableProperty]
    private bool _isWorkingState;

    [ObservableProperty]
    private bool _isTimerRunning;

    [ObservableProperty]
    private string _nextBreakDuration = "0";

    [ObservableProperty]
    private string _breakBankStatus = "Break bank: 0 min";

    [ObservableProperty]
    private string _workDayCountdown = "--:--";

    [ObservableProperty]
    private double _sessionProgress = 0;

    [ObservableProperty]
    private double _sessionProgressMax = 100;

    [ObservableProperty]
    private string _breakIntervalDisplay = "Every -- min";

    [ObservableProperty]
    private bool _isPinned = true;

    [ObservableProperty]
    private SolidColorBrush _progressRingBrush = new SolidColorBrush(Colors.DodgerBlue);

    #endregion Observable Properties

    #region Commands

    public IRelayCommand TakeBreakCommand { get; }
    public IRelayCommand TakeBreakNowCommand { get; }
    public IRelayCommand SkipBreakCommand { get; }
    public IRelayCommand SnoozeBreakCommand { get; }
    public IRelayCommand EndBreakEarlyCommand { get; }
    public IRelayCommand StartTimerCommand { get; }
    public IRelayCommand StopTimerCommand { get; }
    public IRelayCommand TogglePinCommand { get; }

    #endregion Commands

    #region Constructor

    public TimerNotificationWindowViewModel(IFocusTimerService focusTimerService)
    {
        _focusTimerService = focusTimerService;
        _dispatcherQueue = DispatcherQueue.GetForCurrentThread();

        TakeBreakCommand = new RelayCommand(OnTakeBreak, CanTakeBreak);
        TakeBreakNowCommand = new RelayCommand(OnTakeBreakNow, CanTakeBreakNow);
        SkipBreakCommand = new RelayCommand(OnSkipBreak, CanSkipBreak);
        SnoozeBreakCommand = new RelayCommand(OnSnoozeBreak, CanSnoozeBreak);
        EndBreakEarlyCommand = new RelayCommand(OnEndBreakEarly, CanEndBreakEarly);
        StartTimerCommand = new AsyncRelayCommand(OnStartTimerAsync, CanStartTimer);
        StopTimerCommand = new AsyncRelayCommand(OnStopTimerAsync, CanStopTimer);
        TogglePinCommand = new RelayCommand(() => IsPinned = !IsPinned);

        // Subscribe to service events
        _focusTimerService.StateChanged += OnStateChanged;

        // Initialize with current state
        UpdateFromState(_focusTimerService.CurrentState);
    }

    #endregion Constructor

    #region Event Handlers

    private void OnStateChanged(object? sender, FocusTimerStateChangedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            UpdateFromState(e.State);
        });
    }

    #endregion Event Handlers

    #region Private Methods

    private void UpdateFromState(FocusTimerState state)
    {
        CountdownDisplay = state.CountdownDisplay;
        StatusMessage = state.StatusMessage;
        BreakBankPercentage = state.BreakBankPercentage;
        BreakPoolMinutes = state.CurrentBreakPoolMinutes;
        RemainingBreakCount = state.RemainingBreakCount;
        CurrentStatus = state.Status;
        IsTimerRunning = _focusTimerService.IsRunning;

        // Update state flags for UI binding
        IsNotificationState = state.Status == FocusTimerStatus.NotificationTriggered;
        IsBreakActiveState = state.Status == FocusTimerStatus.BreakActive;
        IsWorkingState = state.Status == FocusTimerStatus.Working ||
                        state.Status == FocusTimerStatus.LunchMode;

        // Update display strings
        NextBreakDuration = $"{state.CurrentBreakDurationMinutes:F0}";
        BreakBankStatus = $"Break bank: {state.CurrentBreakPoolMinutes:F0} min ({state.RemainingBreakCount} breaks left)";

        // Calculate work day countdown
        CalculateWorkDayCountdown();

        // Calculate session progress (work session or break time)
        CalculateSessionProgress(state);

        // Calculate break interval display
        CalculateBreakInterval();

        // Update progress ring color based on status
        UpdateProgressRingColor(state.Status);

        // Update command states
        ((RelayCommand)TakeBreakNowCommand).NotifyCanExecuteChanged();
        ((RelayCommand)TakeBreakCommand).NotifyCanExecuteChanged();
        ((RelayCommand)SkipBreakCommand).NotifyCanExecuteChanged();
        ((RelayCommand)SnoozeBreakCommand).NotifyCanExecuteChanged();
        ((RelayCommand)EndBreakEarlyCommand).NotifyCanExecuteChanged();
        ((AsyncRelayCommand)StartTimerCommand).NotifyCanExecuteChanged();
        ((AsyncRelayCommand)StopTimerCommand).NotifyCanExecuteChanged();
    }

    private void OnTakeBreak()
    {
        _focusTimerService.TakeBreak();
    }

    private bool CanTakeBreak()
    {
        return CurrentStatus == FocusTimerStatus.NotificationTriggered;
    }

    private void OnTakeBreakNow()
    {
        _focusTimerService.TakeBreakNow();
    }

    private bool CanTakeBreakNow()
    {
        return CurrentStatus == FocusTimerStatus.Working || CurrentStatus == FocusTimerStatus.LunchMode;
    }

    private void OnSkipBreak()
    {
        System.Diagnostics.Debug.WriteLine("[TimerWindowViewModel] OnSkipBreak called");
        _focusTimerService.SkipBreak();
    }

    private bool CanSkipBreak()
    {
        return CurrentStatus == FocusTimerStatus.NotificationTriggered;
    }

    private void OnSnoozeBreak()
    {
        _focusTimerService.SnoozeBreak(5);
    }

    private bool CanSnoozeBreak()
    {
        return CurrentStatus == FocusTimerStatus.NotificationTriggered;
    }

    private void OnEndBreakEarly()
    {
        _focusTimerService.EndBreakEarly();
    }

    private bool CanEndBreakEarly()
    {
        return CurrentStatus == FocusTimerStatus.BreakActive;
    }

    private async Task OnStartTimerAsync()
    {
        await _focusTimerService.StartAsync();
    }

    private bool CanStartTimer()
    {
        return !_focusTimerService.IsRunning;
    }

    private async Task OnStopTimerAsync()
    {
        await _focusTimerService.StopAsync();
    }

    private bool CanStopTimer()
    {
        return _focusTimerService.IsRunning;
    }

    private void CalculateWorkDayCountdown()
    {
        var settings = _focusTimerService.Settings;
        if (settings == null) return;

        try
        {
            var now = DateTime.Now;
            if (TimeSpan.TryParse(settings.WorkEndTime, out var workEndTime))
            {
                var workEndToday = DateTime.Today.Add(workEndTime);

                if (now < workEndToday)
                {
                    var remaining = workEndToday - now;
                    WorkDayCountdown = remaining.TotalHours >= 1
                        ? $"{(int)remaining.TotalHours}h {remaining.Minutes}m until work ends"
                        : $"{remaining.Minutes}m until work ends";
                }
                else
                {
                    WorkDayCountdown = "Work day ended";
                }
            }
            else
            {
                WorkDayCountdown = "--:--";
            }
        }
        catch
        {
            WorkDayCountdown = "--:--";
        }
    }

    private void CalculateSessionProgress(FocusTimerState state)
    {
        if (state.Status == FocusTimerStatus.BreakActive)
        {
            // During break: show break time consumed (0% = just started, 100% = break ending)
            if (state.BreakTimeRemaining.HasValue && state.CurrentBreakDurationMinutes > 0)
            {
                var elapsed = state.CurrentBreakDurationMinutes - state.BreakTimeRemaining.Value.TotalMinutes;
                SessionProgress = Math.Max(0, Math.Min(100, (elapsed / state.CurrentBreakDurationMinutes) * 100));
            }
            else
            {
                SessionProgress = 0;
            }
        }
        else if (state.Status == FocusTimerStatus.Working)
        {
            // During work: show progress toward next break (0% = just started, 100% = break time)
            if (state.TimeUntilNextBreak.HasValue && state.NextBreakTime.HasValue)
            {
                var settings = _focusTimerService.Settings;
                if (settings != null && state.RemainingBreakCount > 0)
                {
                    // Calculate total work minutes in current session
                    if (TimeSpan.TryParse(settings.WorkStartTime, out var workStartTime) &&
                        TimeSpan.TryParse(settings.WorkEndTime, out var workEndTime))
                    {
                        var lunchDuration = settings.LunchDurationMinutes;

                        // Calculate work period for this session
                        var totalWorkMinutes = (workEndTime - workStartTime).TotalMinutes - lunchDuration;
                        var breakInterval = totalWorkMinutes / (state.RemainingBreakCount + 1);

                        var elapsed = breakInterval - state.TimeUntilNextBreak.Value.TotalMinutes;
                        SessionProgress = Math.Max(0, Math.Min(100, (elapsed / breakInterval) * 100));
                    }
                    else
                    {
                        SessionProgress = 0;
                    }
                }
                else
                {
                    SessionProgress = 100;
                }
            }
            else
            {
                SessionProgress = 0;
            }
        }
        else
        {
            SessionProgress = 0;
        }
    }

    private void CalculateBreakInterval()
    {
        var settings = _focusTimerService.Settings;
        if (settings == null || settings.DesiredBreakCount == 0)
        {
            BreakIntervalDisplay = "Every -- min";
            return;
        }

        try
        {
            if (TimeSpan.TryParse(settings.WorkStartTime, out var workStartTime) &&
                TimeSpan.TryParse(settings.WorkEndTime, out var workEndTime))
            {
                var totalWorkMinutes = (workEndTime - workStartTime).TotalMinutes - settings.LunchDurationMinutes;
                var breakInterval = totalWorkMinutes / (settings.DesiredBreakCount + 1);

                var intervalSpan = TimeSpan.FromMinutes(breakInterval);
                if (intervalSpan.TotalHours >= 1)
                {
                    BreakIntervalDisplay = $"Every {(int)intervalSpan.TotalHours}h {intervalSpan.Minutes}m";
                }
                else
                {
                    BreakIntervalDisplay = $"Every {intervalSpan.Minutes}m";
                }
            }
            else
            {
                BreakIntervalDisplay = "Every -- min";
            }
        }
        catch
        {
            BreakIntervalDisplay = "Every -- min";
        }
    }

    private void UpdateProgressRingColor(FocusTimerStatus status)
    {
        var color = status switch
        {
            FocusTimerStatus.Working => Colors.DodgerBlue, // Blue - working
            FocusTimerStatus.BreakActive => Colors.Green, // Green - on break
            FocusTimerStatus.NotificationTriggered => Colors.Orange, // Orange - break notification
            FocusTimerStatus.LunchMode => Colors.Purple, // Purple - lunch
            FocusTimerStatus.DayEnded => Colors.Gray, // Gray - day ended
            _ => Colors.DodgerBlue // Default blue
        };
        ProgressRingBrush = new SolidColorBrush(color);
    }

    #endregion Private Methods

}