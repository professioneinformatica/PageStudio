using Microsoft.AspNetCore.Components;
using PageStudio.Core.Models;
using PageStudio.Core.Models.Documents;

namespace PageStudio.Web.Client.Components;

public partial class DocumentPropertiesPanel : ComponentBase
{
    [Parameter] public bool IsVisible { get; set; }
    [Parameter] public Document? Document { get; set; }
    [Parameter] public EventCallback<Document> OnDocumentChanged { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private string DocumentName = string.Empty;
    private int DocumentDpi = 72;
    private UnitOfMeasure DocumentUnitOfMeasure = UnitOfMeasure.Centimeters;

    protected override void OnParametersSet()
    {
        if (Document != null)
        {
            DocumentName = Document.Name;
            DocumentDpi = Document.Dpi;
            DocumentUnitOfMeasure = Document.UnitOfMeasure;
        }
    }

    private async Task SaveChanges()
    {
        if (Document != null)
        {
            Document.Name = DocumentName;
            Document.Dpi = DocumentDpi;
            Document.UnitOfMeasure = DocumentUnitOfMeasure;
            Document.UpdateModifiedTime();
            Document.SetMetadata("DPI", DocumentDpi);
            Document.SetMetadata("UnitOfMeasure", DocumentUnitOfMeasure.ToString());
            await OnDocumentChanged.InvokeAsync(Document);
        }
        await Close();
    }

    private async Task Close()
    {
        await OnClose.InvokeAsync();
    }
}

