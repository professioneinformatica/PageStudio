namespace PageStudio.Core.Features.EventsManagement;

public interface IEvent
{
}

public interface IEventHandler<in TEvent> where TEvent : IEvent
{
    Task HandleAsync(TEvent e, CancellationToken cancellationToken = default);
}