using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
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

        await builder.Build().RunAsync();
    }
}