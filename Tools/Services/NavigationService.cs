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

    // Event raised when back stack availability changes
    event Action? BackStackChanged;
}

public class NavigationService : INavigationService
{
    private Frame? _frame;

    // Deferred DI-resolved page instance to replace the framework-created instance
    private Page? _deferredPage;
    private Type? _deferredPageType;

    // Custom back stack for DI-resolved pages (pages without parameterless ctors)
    private readonly List<Page> _customBackStack = new();

    public Frame? Frame
    {
        get => _frame;
        set => _frame = value;
    }

    public bool CanGoBack => (_customBackStack.Count > 0) || (_frame?.CanGoBack ?? false);

    public event Action<Type?>? Navigated;
    public event Action? BackStackChanged;

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
                var hasParameterlessCtor = pageType.GetConstructor(Type.EmptyTypes) != null;

                if (hasParameterlessCtor)
                {
                    // Let framework create the instance so a JournalEntry is created, but
                    // replace it with our DI-resolved instance after navigation completes.
                    _deferredPage = resolved;
                    _deferredPageType = pageType;
                    var result = _frame.Navigate(pageType, parameter);
                    BackStackChanged?.Invoke();
                    return result;
                }

                // No parameterless ctor - set content directly and manage our own back stack.
                if (_frame.Content is Page current && current.GetType() != pageType)
                {
                    _customBackStack.Add(current);
                    BackStackChanged?.Invoke();
                }

                _frame.Content = resolved;

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

        // Fallback to framework navigation which will maintain its own journal
        // Clear custom back stack because framework will manage navigation entries now.
        if (_customBackStack.Count > 0)
        {
            _customBackStack.Clear();
            BackStackChanged?.Invoke();
        }
        var navResult = _frame.Navigate(pageType, parameter);
        return navResult;
    }

    public bool GoBack()
    {
        if (_frame == null)
            return false;

        // If we have our own custom back entries, use them first
        if (_customBackStack.Count > 0)
        {
            var lastIndex = _customBackStack.Count - 1;
            var previous = _customBackStack[lastIndex];
            _customBackStack.RemoveAt(lastIndex);

            _frame.Content = previous;

            try
            {
                Navigated?.Invoke(previous.GetType());
            }
            catch
            {
                // Ignore subscriber exceptions
            }

            BackStackChanged?.Invoke();
            return true;
        }

        if (!_frame.CanGoBack)
            return false;

        _frame.GoBack();
        // Frame.Navigated will fire and BackStackChanged will be invoked there
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
        else
        {
            // If navigation occurred via the framework (not our direct content set),
            // clear any custom back stack because framework now owns navigation.
            if (e.NavigationMode == NavigationMode.New || e.NavigationMode == NavigationMode.Back)
            {
                if (_customBackStack.Count > 0)
                {
                    _customBackStack.Clear();
                    BackStackChanged?.Invoke();
                }
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

        // Notify listeners about back stack availability
        BackStackChanged?.Invoke();
    }
}
