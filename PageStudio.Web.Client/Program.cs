using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

namespace PageStudio.Web.Client;

public static class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
        
        builder.Services.AddPageStudioCoreServices();
        
        builder.Services.AddFluentUIComponents();

        var app = builder.Build();
        var hostedServices = app.Services.GetServices<IHostedService>();
        foreach (var hostedService in hostedServices)
        {
            await hostedService.StartAsync(CancellationToken.None);
        }
        await app.RunAsync();
    }
}