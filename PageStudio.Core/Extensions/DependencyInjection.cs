using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using PageStudio.Core.Features.EventsManagement;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddPageStudioCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IDocumentsRepository, DocumentsRepository>();

        services.AddSingleton<IEventPublisher, EventPublisher>();
        services.AddSingleton<IEventDispatcher, EventDispatcher>();
        services.AddHostedService<EventConsumer>();

        // Registrazione automatica di tutti gli IEventHandler
        services.Scan(scan => scan
            .FromAssemblyOf<EventConsumer>()
            .AddClasses(classes => classes.AssignableTo(typeof(IEventHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
        );

        return services;
    }
}