using Microsoft.Extensions.DependencyInjection;
using PageStudio.Core.Interfaces;
using PageStudio.Core.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddPageStudioCoreServices(this IServiceCollection services)
    {
        services.AddSingleton<IDocumentsRepository, DocumentsRepository>();
        return services;
    }
}