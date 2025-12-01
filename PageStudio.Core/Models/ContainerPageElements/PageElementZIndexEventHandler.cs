using Microsoft.Extensions.Hosting;
using PageStudio.Core.Features.EventsManagement;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Models.Documents;

namespace PageStudio.Core.Models.ContainerPageElements;

public class PageElementZIndexEventHandler(IDocumentsRepository documentsRepository) : IEventHandler<ZIndexChangedMessage>
{
    public Task HandleAsync(ZIndexChangedMessage e, CancellationToken cancellationToken = default)
    {
        
        return Task.CompletedTask;
    }
}