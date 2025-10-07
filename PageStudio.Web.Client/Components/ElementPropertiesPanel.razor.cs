using Microsoft.AspNetCore.Components;
using PageStudio.Core.Interfaces;

namespace PageStudio.Web.Client.Components;

public partial class ElementPropertiesPanel
{
    [Parameter] public IPageElement? SelectedElement { get; set; }

    [Parameter] public EventCallback OnPropertyChanged { get; set; }

    private double? aspectRatio = null;
    private double? lastWidth = null;
    private double? lastHeight = null;

    protected override void OnParametersSet()
    {
        if (SelectedElement != null)
        {
            // Aggiorna il rapporto solo se cambia elemento selezionato
            if (lastWidth != SelectedElement.Width || lastHeight != SelectedElement.Height)
            {
                if (SelectedElement.Width > 0 && SelectedElement.Height > 0)
                    aspectRatio = SelectedElement.Width / SelectedElement.Height;
                else
                    aspectRatio = null;
                lastWidth = SelectedElement.Width;
                lastHeight = SelectedElement.Height;
            }
        }
    }

    private void ToggleAspectRatioLock()
    {
        if (SelectedElement != null)
        {
            SelectedElement.LockAspectRatio = !SelectedElement.LockAspectRatio;
            if (SelectedElement.LockAspectRatio && SelectedElement.Width > 0 && SelectedElement.Height > 0)
                aspectRatio = SelectedElement.Width / SelectedElement.Height;
            OnPropertyChanged.InvokeAsync();
        }
    }

    private async Task OnWidthChanged(ChangeEventArgs e)
    {
        if (SelectedElement == null) return;
        if (double.TryParse(e.Value?.ToString(), out var newWidth))
        {
            if (SelectedElement.LockAspectRatio && aspectRatio.HasValue && aspectRatio.Value > 0)
            {
                SelectedElement.Width = newWidth;
                SelectedElement.Height = Math.Round(newWidth / aspectRatio.Value);
            }
            else
            {
                SelectedElement.Width = newWidth;
            }

            await OnPropertyChanged.InvokeAsync();
        }
    }

    private async Task OnHeightChanged(ChangeEventArgs e)
    {
        if (SelectedElement == null) return;
        if (double.TryParse(e.Value?.ToString(), out var newHeight))
        {
            if (SelectedElement.LockAspectRatio && aspectRatio.HasValue && aspectRatio.Value > 0)
            {
                SelectedElement.Height = newHeight;
                SelectedElement.Width = Math.Round(newHeight * aspectRatio.Value);
            }
            else
            {
                SelectedElement.Height = newHeight;
            }

            await OnPropertyChanged.InvokeAsync();
        }
    }
}