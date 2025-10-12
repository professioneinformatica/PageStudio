using Mediator;
using Microsoft.Extensions.Logging;
using PageStudio.Core.Interfaces;

namespace PageStudio.Core.Models.ContainerPageElements;

public sealed record AddPageElementRequest(IPageElement PageElement) : INotification;

public sealed class AddPageElementHandler(ILogger<AddPageElementHandler> logger) : INotificationHandler<AddPageElementRequest>
{
    public ValueTask Handle(AddPageElementRequest request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Added element with ID: {PageElementId} and Name: {PageElementName} to page {PageName}",
            request.PageElement.Id, request.PageElement.Name, request.PageElement.Page.Name);
        return default;
    }
}

public sealed record PageElementZOrderChangedMessage(IPageElement PageElement) : INotification;

public sealed class PageElementZOrderChangedtHandler : INotificationHandler<PageElementZOrderChangedMessage>
{
    public ValueTask Handle(PageElementZOrderChangedMessage message, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[EVENT] Changed element with ID: {message.PageElement.Id} and Name: {message.PageElement.Name} set ZOrder to: {message.PageElement.ZOrder}");
        // recalculate Z-order of all elements in the page


        return default;
    }
}