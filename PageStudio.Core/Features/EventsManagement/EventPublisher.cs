using System.Threading.Channels;

namespace PageStudio.Core.Features.EventsManagement;

public interface IEventPublisher
{
    void Publish<TEvent>(TEvent eventMessage) where TEvent : IEvent;
    ChannelReader<IEvent> GetReader();
}

public class EventPublisher : IEventPublisher
{
    private readonly Channel<IEvent> _channel = Channel.CreateUnbounded<IEvent>();
    public void Publish<TEvent>(TEvent eventMessage) where TEvent : IEvent
    {
        _channel.Writer.TryWrite(eventMessage);
    }
    public ChannelReader<IEvent> GetReader()
    {
        return _channel.Reader;
    }
}

