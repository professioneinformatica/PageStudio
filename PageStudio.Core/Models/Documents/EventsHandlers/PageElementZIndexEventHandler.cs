using PageStudio.Core.Features.EventsManagement;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Services;

namespace PageStudio.Core.Models.Documents.EventsHandlers;

public class DocumentZoomChangedEventHandler(IDocumentsRepository documentsRepository) : IEventHandler<CanvasDocumentInteractor.DocumentZoomChangedMessage>
{
    public async Task HandleAsync(CanvasDocumentInteractor.DocumentZoomChangedMessage e, CancellationToken cancellationToken = default)
    {
        e.CanvasDocumentInteractor.RenderDocument();
        await Task.CompletedTask;
    }
}