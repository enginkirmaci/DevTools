using Tools.Library.Entities;

namespace Tools.Services;

public interface INavigationService
{
    void Navigate(string navigatePath, Regions region, Parameters? parameters = null);

    T GetParameter<T>(string key);

    void SetParameters(Parameters parameters);

    bool CanGoBack(Regions region = Regions.MainRegion);

    void GoBack(Regions region = Regions.MainRegion);

    bool IsChildView(string currentView, string childView);
}