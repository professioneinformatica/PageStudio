using Mediator;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

namespace PageStudio.Web.Client;

public static class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Services.AddMediator((MediatorOptions options) =>
            {
                options.Assemblies = [typeof(PageStudio.Web.Client.Program),typeof(PageStudio.Core.Interfaces.IDocument)];
                options.GenerateTypesAsInternal = true;
            }
        );

        builder.Services.AddPageStudioCoreServices();
        
        builder.Services.AddFluentUIComponents();

        await builder.Build().RunAsync();
    }
}