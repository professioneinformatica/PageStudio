using Microsoft.AspNetCore.Components;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models;
using PageStudio.Core.Models.Documents;

namespace PageStudio.Web.Client.Components;

public partial class DocumentPropertiesPanel : ComponentBase
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public IDocument? Document { get; set; }
    [Parameter] public EventCallback<IDocument> OnDocumentChanged { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private string _documentName = string.Empty;
    private int _documentDpi = 72;
    private UnitOfMeasure _documentUnitOfMeasure = UnitOfMeasure.Centimeters;

    protected override void OnParametersSet()
    {
        if (Document != null)
        {
            
            _documentName = Document.Name;
            _documentDpi = Document.Dpi;
            _documentUnitOfMeasure = Document.UnitOfMeasure;
        }
    }

    private async Task SaveChanges()
    {
        if (Document != null)
        {
            Document.Name = _documentName;
            Document.Dpi = _documentDpi;
            Document.UnitOfMeasure = _documentUnitOfMeasure;
            Document.UpdateModifiedTime();
            Document.SetMetadata("DPI", _documentDpi);
            Document.SetMetadata("UnitOfMeasure", _documentUnitOfMeasure.ToString());
            await OnDocumentChanged.InvokeAsync(Document);
        }
        await Close();
    }

    private async Task Close()
    {
        await OnClose.InvokeAsync();
    }
}

