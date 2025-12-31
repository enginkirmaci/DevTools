using Microsoft.UI.Xaml.Controls;

namespace Tools.Services;

public interface INavigationService
{
    Frame? Frame { get; set; }
    bool CanGoBack { get; }
    bool Navigate(Type pageType, object? parameter = null);
    bool GoBack();
    void SetFrame(Frame frame);
}

public class NavigationService : INavigationService
{
    private Frame? _frame;

    public Frame? Frame
    {
        get => _frame;
        set => _frame = value;
    }

    public bool CanGoBack => _frame?.CanGoBack ?? false;

    public void SetFrame(Frame frame)
    {
        _frame = frame;
    }

    public bool Navigate(Type pageType, object? parameter = null)
    {
        if (_frame == null)
            return false;

        // Try to resolve the page from the DI container so pages with constructor
        // parameters (ViewModels) can be created. Falls back to XAML activation
        // which requires a parameterless constructor.
        try
        {
            var resolved = App.Services.GetService(pageType) as Page;
            if (resolved is not null)
            {
                // Set the frame content to the DI-created page instance.
                _frame.Content = resolved;
                return true;
            }
        }
        catch
        {
            // Resolution failed — fall back to framework activation below.
        }

        return _frame.Navigate(pageType, parameter);
    }

    public bool GoBack()
    {
        if (_frame == null || !_frame.CanGoBack)
            return false;

        _frame.GoBack();
        return true;
    }
}
