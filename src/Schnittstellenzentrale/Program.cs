using Serilog;
using Schnittstellenzentrale.Components;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Hubs;
using Schnittstellenzentrale.Infrastructure.Data;
using Schnittstellenzentrale.Infrastructure.Repositories;
using Schnittstellenzentrale.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Negotiate.NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();
builder.Services.AddAuthorization();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("negotiate")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseDefaultCredentials = true });

DatabaseProviderFactory.RegisterDbContext(builder.Services, builder.Configuration);

builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<IEndpointRepository, EndpointRepository>();
// Scoped ist in Blazor Server korrekt: ein Scope entspricht genau einem Circuit (Verbindung),
// sodass jede Benutzersitzung eine eigene Instanz erhält.
builder.Services.AddScoped<IStorageModeService, StorageModeService>();
builder.Services.AddSingleton<IHealthCheckService, HealthCheckService>();
builder.Services.AddScoped<IEndpointExecutionService, EndpointExecutionService>();
builder.Services.AddScoped<ISwaggerImportService, SwaggerImportService>();
builder.Services.AddScoped<IODataImportService, ODataImportService>();
builder.Services.AddSingleton<ICredentialService, WindowsCredentialService>();
builder.Services.AddSingleton<ICurrentUserService, WindowsCurrentUserService>();
builder.Services.AddScoped<ISignalRNotificationService, SignalRNotificationService<EndpointHub>>();

var app = builder.Build();

await EnsureDatabaseInitializedAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<EndpointHub>("/hubs/endpoint");

app.Run();

static async Task EnsureDatabaseInitializedAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await MigrateDatabaseAsync(dbContext);
}

static async Task MigrateDatabaseAsync(AppDbContext dbContext)
{
    await dbContext.Database.MigrateAsync();
}
