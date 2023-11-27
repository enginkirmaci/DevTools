using System.Collections.Generic;
using Prism.Regions;
using Tools.Library.Entities;

namespace Tools.Services;

public class NavigationService : INavigationService
{
    private readonly IRegionManager regionManager;

    private readonly Dictionary<string, List<string>> childViews = new()
    {
        { "EditorView" , new List<string> { "TelemetryView", "ExportView" } }
    };

    private Parameters parameters { get; set; }

    public NavigationService(
        IRegionManager regionManager)
    {
        this.regionManager = regionManager;

        parameters = new Parameters();
    }

    public void Navigate(string navigatePath, Regions region, Parameters? parameters = null)
    {
        if (parameters != null)
        {
            this.parameters = parameters;
        }
        else
        {
            this.parameters.Clear();
        }

        regionManager.RequestNavigate(region.ToString(), navigatePath);
    }

    public T GetParameter<T>(string key)
    {
        return parameters.GetValue<T>(key);
    }

    public void SetParameters(Parameters parameters)
    {
        this.parameters = parameters;
    }

    public bool CanGoBack(Regions region = Regions.MainRegion)
    {
        return regionManager.Regions[region.ToString()].NavigationService.Journal.CanGoBack;
    }

    public void GoBack(Regions region = Regions.MainRegion)
    {
        regionManager.Regions[region.ToString()].NavigationService.Journal.GoBack();
    }

    public bool IsChildView(string currentView, string childView)
    {
        return childViews[currentView].Contains(childView);
    }
}