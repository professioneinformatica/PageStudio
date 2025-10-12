using Mediator;
using PageStudio.Core.Interfaces;

namespace PageStudio.Core.Models.Documents;

public sealed record AddPageRequest(IPage Page, int? InsertIndex) : INotification;

public sealed class AddPageHandler : INotificationHandler<AddPageRequest>
{
    public ValueTask Handle(AddPageRequest request, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[EVENT] Added page with ID: {request.Page.Id} at index {request.InsertIndex} of document {request.Page.Document.Name}");
        return default;
    }
}