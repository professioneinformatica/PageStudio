using Mediator;
using Microsoft.Extensions.Logging;
using PageStudio.Core.Interfaces;

namespace PageStudio.Core.Models.Documents;

public sealed record AddPageRequest(IPage Page, int? InsertIndex) : INotification;

public sealed class AddPageHandler (ILogger<AddPageHandler> logger): INotificationHandler<AddPageRequest>
{
    public ValueTask Handle(AddPageRequest request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Added page with ID: {PageId} at index {InsertIndex} of document {DocumentName}",
            request.Page.Id, request.InsertIndex, request.Page.Document.Name);

        return default;
    }
}