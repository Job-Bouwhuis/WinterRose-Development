using ContentPageAndSectionTests.Components;
using ContentPageAndSectionTests.PageContentSystem;
using WinterRose.AnonymousTypes;

internal class Program
{
    private static async Task Main(string[] args)
    {
        Anonymous.ThrowOnMissingField = false;

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        builder.Services.Configure<ContentOptions>(builder.Configuration.GetSection(ContentOptions.SECTION));

        builder.Services.AddSingleton<ContentService>();

        builder.Services.AddSignalR();

        var app = builder.Build();

        app.MapHub<ContentHub>("/hubs/content");

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        ContentService CONTENT_SERVICE = app.Services.GetRequiredService<ContentService>();

        await CONTENT_SERVICE.InitializeAsync();

        await app.RunAsync();
    }
}