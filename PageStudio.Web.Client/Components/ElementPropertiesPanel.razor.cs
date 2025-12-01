using Microsoft.AspNetCore.Components;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Abstractions;

namespace PageStudio.Web.Client.Components;

public partial class ElementPropertiesPanel
{
    [Parameter] public IPageElement? SelectedElement { get; set; }
    [Parameter] public EventCallback OnPropertyChanged { get; set; }

    private void ToggleAspectRatioLock()
    {
        if (SelectedElement != null)
        {
            SelectedElement.LockAspectRatio = !SelectedElement.LockAspectRatio;
            OnPropertyChanged.InvokeAsync();
        }
    }
}