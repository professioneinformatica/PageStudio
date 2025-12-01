using Microsoft.Extensions.DependencyInjection;

namespace PageStudio.Core.Features.EventsManagement;

public interface IEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;
}

public class EventDispatcher(IServiceProvider serviceProvider) : IEventDispatcher
{
    public async Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent
    {
        var eventType = @event.GetType();
        var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
        using var scope = serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices(handlerType);
        foreach (var handler in handlers)
        {
            var method = typeof(IEventHandler<>).MakeGenericType(eventType).GetMethod(nameof(IEventHandler<TEvent>.HandleAsync));
            if (method == null) continue;
            
            try
            {
                await (Task)method.Invoke(handler, [@event, cancellationToken])!;
            }
            catch (Exception e)
            {
                // need a mechanism to prevent events from being lost // log error
            }
        }
    }
}