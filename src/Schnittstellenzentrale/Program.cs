using Microsoft.OpenApi;
using Serilog;
using Schnittstellenzentrale;
using Schnittstellenzentrale.Components;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Filters;
using Schnittstellenzentrale.Hubs;
using Schnittstellenzentrale.Infrastructure.Data;
using Schnittstellenzentrale.Infrastructure.Repositories;
using Schnittstellenzentrale.Infrastructure.Services;
using Schnittstellenzentrale.Services;
using Microsoft.EntityFrameworkCore;
using ShadcnBlazor;

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

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Schnittstellenzentrale API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Bearer-Token aus POST /authenticate",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });
    c.AddSecurityDefinition("Negotiate", new OpenApiSecurityScheme
    {
        Description = "Windows-Authentifizierung (NTLM/Kerberos)",
        Type = SecuritySchemeType.Http,
        Scheme = "negotiate"
    });
    c.OperationFilter<SecurityOperationFilter>();
    c.OperationFilter<ContextHeadersOperationFilter>();
    c.OperationFilter<SzExtensionsOperationFilter>();
    var xmlPath = Path.Combine(AppContext.BaseDirectory, "Schnittstellenzentrale.xml");
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("negotiate")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { UseDefaultCredentials = true });

DatabaseProviderFactory.RegisterDbContext(builder.Services, builder.Configuration);

builder.Services.AddScoped<IApplicationRepository, ApplicationRepository>();
builder.Services.AddScoped<IEndpointRepository, EndpointRepository>();
builder.Services.AddScoped<ISystemEnvironmentRepository, SystemEnvironmentRepository>();
builder.Services.AddScoped<IActiveEnvironmentService, ActiveEnvironmentService>();
// Scoped ist in Blazor Server korrekt: ein Scope entspricht genau einem Circuit (Verbindung),
// sodass jede Benutzersitzung eine eigene Instanz erhält.
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();
builder.Services.AddScoped<IStorageModeService, StorageModeService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddSingleton<IHealthCheckService, HealthCheckService>();
builder.Services.AddScoped<IEndpointScriptRunner, EndpointScriptRunner>();
builder.Services.AddScoped<IEndpointExecutionService, EndpointExecutionService>();
builder.Services.AddScoped<ISwaggerImportService, SwaggerImportService>();
builder.Services.AddScoped<IODataImportService, ODataImportService>();
builder.Services.AddSingleton<ICredentialService, WindowsCredentialService>();
builder.Services.AddSingleton<ICurrentUserService, WindowsCurrentUserService>();
builder.Services.AddScoped<ISignalRNotificationService, SignalRNotificationService<EndpointHub>>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ITokenStore, TokenStore>();
builder.Services.AddHttpClient<IApplicationApiClient, ApplicationApiClient>();
builder.Services.AddHostedService<SystemEndpointSyncService>();

builder.Services.AddScoped<INavigationStateService, NavigationStateService>();
builder.Services.AddScoped<IApplicationGroupService, ApplicationGroupService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IApplicationLinkService, ApplicationLinkService>();
builder.Services.AddScoped<IApplicationLinkRepository, ApplicationLinkRepository>();
builder.Services.AddScoped<IHistoryService, HistoryService>();

builder.Services.Configure<UploadSettings>(builder.Configuration.GetSection("Upload"));
builder.Services.Configure<HistorySettings>(builder.Configuration.GetSection("History"));

builder.Services.AddShadcnBlazor();

var app = builder.Build();

await EnsureDatabaseInitializedAsync(app.Services);
await SystemEntryInitializer.InitializeAsync(app.Services, builder.Configuration);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();
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

/// <summary>Einstiegspunkt der Anwendung (für Integrationstests als partielle Klasse zugänglich).</summary>
public partial class Program { }
