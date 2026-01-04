using Microsoft.UI.Xaml.Controls;
using Tools.ViewModels.Pages;
using Tools.Views.Windows;

namespace Tools.Views.Pages;

/// <summary>
/// Settings page for the Focus Timer feature.
/// </summary>
public sealed partial class FocusTimerSettingsPage : Page
{
    /// <summary>
    /// Gets the ViewModel for this page.
    /// </summary>
    public FocusTimerSettingsViewModel ViewModel { get; }

    public FocusTimerSettingsPage(FocusTimerSettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        this.InitializeComponent();

        // Subscribe to show timer window request
        ViewModel.ShowTimerWindowRequested += OnShowTimerWindowRequested;
        Unloaded += OnPageUnloaded;
    }

    private void OnShowTimerWindowRequested(object? sender, EventArgs e)
    {
        // Get the timer window from DI and show it
        var timerWindow = App.GetService<TimerNotificationWindow>();
        timerWindow.ShowWindow();
    }

    private void OnPageUnloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ViewModel.ShowTimerWindowRequested -= OnShowTimerWindowRequested;
    }
}
