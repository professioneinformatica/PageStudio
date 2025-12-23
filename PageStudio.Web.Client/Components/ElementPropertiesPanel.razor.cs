using Microsoft.AspNetCore.Components;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Abstractions;

namespace PageStudio.Web.Client.Components;

public partial class ElementPropertiesPanel
{
    [Parameter] public IPageElement? SelectedElement { get; set; }
    [Parameter] public EventCallback OnPropertyChanged { get; set; }

    private string _activeTab = "general";
    private string? _elementName;
    private string? _nameError;

    protected override void OnParametersSet()
    {
        if (SelectedElement != null && _elementName != SelectedElement.Name)
        {
            _elementName = SelectedElement.Name;
            _nameError = null;
        }
    }

    private void HandleNameChange(ChangeEventArgs e)
    {
        var newName = e.Value?.ToString();
        if (string.IsNullOrWhiteSpace(newName))
        {
            _nameError = "Name cannot be empty";
            return;
        }

        if (SelectedElement == null) return;

        try
        {
            SelectedElement.Name = newName;
            _elementName = newName;
            _nameError = null;
            OnPropertyChanged.InvokeAsync();
        }
        catch (InvalidOperationException ex)
        {
            _nameError = ex.Message;
        }
    }

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