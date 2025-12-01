using System.Threading.Channels;
using Microsoft.Extensions.Hosting;

namespace PageStudio.Core.Features.EventsManagement;

public class EventConsumer : BackgroundService
{
    private readonly ChannelReader<IEvent> _reader;
    private readonly IEventDispatcher _dispatcher;

    public EventConsumer(IEventPublisher eventPublisher, IEventDispatcher dispatcher)
    {
        _reader = eventPublisher.GetReader();
        _dispatcher = dispatcher;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _reader.WaitToReadAsync(stoppingToken))
        {
            while (_reader.TryRead(out var message))
            {
                await _dispatcher.DispatchAsync(message, stoppingToken);
            }
        }
    }
}