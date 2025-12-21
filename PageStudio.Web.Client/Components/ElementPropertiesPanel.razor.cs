using Microsoft.AspNetCore.Components;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Abstractions;

namespace PageStudio.Web.Client.Components;

public partial class ElementPropertiesPanel
{
    [Parameter] public IPageElement? SelectedElement { get; set; }
    [Parameter] public EventCallback OnPropertyChanged { get; set; }

    private string _activeTab = "general";

    private void SetActiveTab(string tabName)
    {
        _activeTab = tabName;
    }

    private string GetTabClass(string tabName)
    {
        return _activeTab == tabName ? "nav-link active" : "nav-link";
    }

    private string GetPaneClass(string tabName)
    {
        return _activeTab == tabName ? "tab-pane fade show active" : "tab-pane fade";
    }

    private void ToggleAspectRatioLock()
    {
        if (SelectedElement != null)
        {
            SelectedElement.LockAspectRatio = !SelectedElement.LockAspectRatio;
            OnPropertyChanged.InvokeAsync();
        }
    }
}