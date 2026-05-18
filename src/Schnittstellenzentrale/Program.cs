using Microsoft.OpenApi;
using Serilog;
using Schnittstellenzentrale.Components;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Hubs;
using Schnittstellenzentrale.Infrastructure.Data;
using Schnittstellenzentrale.Infrastructure.Repositories;
using Schnittstellenzentrale.Infrastructure.Services;
using Schnittstellenzentrale.Services;
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
    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", doc), new List<string>() }
    });
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
// Scoped ist in Blazor Server korrekt: ein Scope entspricht genau einem Circuit (Verbindung),
// sodass jede Benutzersitzung eine eigene Instanz erhält.
builder.Services.AddScoped<IStorageModeService, StorageModeService>();
builder.Services.AddScoped<IThemeService, ThemeService>();
builder.Services.AddSingleton<IHealthCheckService, HealthCheckService>();
builder.Services.AddScoped<IEndpointExecutionService, EndpointExecutionService>();
builder.Services.AddScoped<ISwaggerImportService, SwaggerImportService>();
builder.Services.AddScoped<IODataImportService, ODataImportService>();
builder.Services.AddSingleton<ICredentialService, WindowsCredentialService>();
builder.Services.AddSingleton<ICurrentUserService, WindowsCurrentUserService>();
builder.Services.AddScoped<ISignalRNotificationService, SignalRNotificationService<EndpointHub>>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ITokenStore, TokenStore>();
builder.Services.AddHttpClient<IApplicationApiClient, ApplicationApiClient>();

var app = builder.Build();

await EnsureDatabaseInitializedAsync(app.Services);

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

#pragma warning disable CS1591
public partial class Program { }
#pragma warning restore CS1591
