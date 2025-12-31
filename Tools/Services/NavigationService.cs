using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Tools.Services;

public interface INavigationService
{
    Frame? Frame { get; set; }
    bool CanGoBack { get; }
    bool Navigate(Type pageType, object? parameter = null);
    bool GoBack();
    void SetFrame(Frame frame);

    // Event raised after navigation to notify subscribers of the new page type
    event Action<Type?>? Navigated;
}

public class NavigationService : INavigationService
{
    private Frame? _frame;

    // Deferred DI-resolved page instance to replace the framework-created instance
    private Page? _deferredPage;
    private Type? _deferredPageType;

    public Frame? Frame
    {
        get => _frame;
        set => _frame = value;
    }

    public bool CanGoBack => _frame?.CanGoBack ?? false;

    public event Action<Type?>? Navigated;

    public void SetFrame(Frame frame)
    {
        if (_frame != null)
        {
            _frame.Navigated -= Frame_Navigated;
        }

        _frame = frame;

        if (_frame != null)
        {
            _frame.Navigated += Frame_Navigated;
        }
    }

    public bool Navigate(Type pageType, object? parameter = null)
    {
        if (_frame == null)
            return false;

        // Try to resolve the page from the DI container so pages with constructor
        // parameters (ViewModels) can be created. Falls back to framework activation
        // which requires a parameterless constructor.
        try
        {
            var resolved = App.Services.GetService(pageType) as Page;
            if (resolved is not null)
            {
                // If the page type has a public parameterless constructor then allow
                // the framework to navigate by type so it builds a JournalEntry.
                // Otherwise set the frame content directly to the DI-resolved instance
                // to avoid XAML activator instantiation (which will be null for pages
                // without a parameterless ctor).
                var hasParameterlessCtor = pageType.GetConstructor(Type.EmptyTypes) != null;

                if (hasParameterlessCtor)
                {
                    _deferredPage = resolved;
                    _deferredPageType = pageType;
                    return _frame.Navigate(pageType, parameter);
                }

                _frame.Content = resolved;

                // Notify subscribers that navigation completed (content set directly)
                try
                {
                    Navigated?.Invoke(pageType);
                }
                catch
                {
                    // Swallow exceptions from subscribers to avoid breaking navigation
                }

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

    private void Frame_Navigated(object? sender, NavigationEventArgs e)
    {
        // If we have a deferred DI-resolved page and the navigation target matches,
        // replace the framework-created page instance with our resolved instance.
        if (_deferredPage is not null && _deferredPageType != null && e.SourcePageType == _deferredPageType)
        {
            try
            {
                _frame!.Content = _deferredPage;
            }
            finally
            {
                _deferredPage = null;
                _deferredPageType = null;
            }
        }

        // Notify subscribers about the completed navigation
        try
        {
            Navigated?.Invoke(e.SourcePageType);
        }
        catch
        {
            // Ignore subscriber exceptions
        }
    }
}
