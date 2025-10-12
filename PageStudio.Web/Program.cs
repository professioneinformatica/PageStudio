using Mediator;
using PageStudio.Web.Components;

namespace PageStudio.Web;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddMediator(
            (MediatorOptions options) =>
            {
                options.Assemblies = [typeof(PageStudio.Web.Program), 
                    typeof(PageStudio.Web.Client.Program),
                    typeof(PageStudio.Core.Interfaces.IDocument)
                ];
                options.GenerateTypesAsInternal = true;
            }
        );

        builder.Services.AddPageStudioCoreServices();
        
// Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents()
            .AddCircuitOptions(x => x.DetailedErrors = true)
            .AddInteractiveWebAssemblyComponents();


        var app = builder.Build();

// Configure the HTTP request pipeline.
// Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddInteractiveWebAssemblyRenderMode()
            .AddAdditionalAssemblies(typeof(PageStudio.Web.Client._Imports).Assembly);

        await app.RunAsync();
    }
}