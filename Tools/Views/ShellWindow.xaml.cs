using System.Windows;

namespace Tools.Views;

/// <summary>
/// Interaction logic for ShellWindow.xaml
/// </summary>
public partial class ShellWindow : Wpf.Ui.Controls.UiWindow
{
    public ShellWindow()
    {
        InitializeComponent();
    }

    private void RootTitleBar_MinimizeClicked(object sender, System.Windows.RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }
}