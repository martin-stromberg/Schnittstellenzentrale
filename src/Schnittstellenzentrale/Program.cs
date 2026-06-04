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
using Schnittstellenzentrale.Resources;
using Schnittstellenzentrale.Services;
using Microsoft.EntityFrameworkCore;
using ShadcnBlazor;


/// <summary>Einstiegspunkt der Anwendung (für Integrationstests als partielle Klasse zugänglich).</summary>
public partial class Program {
    /// <summary>
    /// Einstiegsmethode der Anwendung
    /// </summary>
    /// <param name="args">Kommandozeilenparameter</param>
    public static async Task Main(string[] args)
    {
        var app = await BuildWebApplicationAsync(args);
        app.Run();
    }

    /// <summary>Erstellt und konfiguriert die WebApplication. Kann aus Tests aufgerufen werden.</summary>
    public static async Task<WebApplication> BuildWebApplicationAsync(
        string[] args,
        WebApplicationOptions? options = null,
        Action<IServiceCollection>? configureServices = null)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build())
            .CreateLogger();

        var builder = options is not null
            ? WebApplication.CreateBuilder(options)
            : WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog();

        builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Negotiate.NegotiateDefaults.AuthenticationScheme)
            .AddNegotiate();
        builder.Services.AddAuthorization();

        builder.Services.AddLocalization();

        builder.Services.AddControllers()
            .AddDataAnnotationsLocalization(options =>
                options.DataAnnotationLocalizerProvider = (type, factory) =>
                    factory.Create(typeof(SharedResources)));

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
        builder.Services.Configure<ImpressumSettings>(builder.Configuration.GetSection("Impressum"));
        builder.Services.AddSingleton<IImpressumService, ImpressumService>();

        builder.Services.AddShadcnBlazor();

        configureServices?.Invoke(builder.Services);

        var app = builder.Build();

        await EnsureDatabaseInitializedAsync(app.Services);
        await SystemEntryInitializer.InitializeAsync(app.Services, builder.Configuration);

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        // HTTPS-Redirect nur außerhalb von Playwright-Tests, da dort HTTP verwendet wird
        if (!app.Environment.EnvironmentName.Equals("Playwright", StringComparison.OrdinalIgnoreCase))
            app.UseHttpsRedirection();

        var localizationOptions = new RequestLocalizationOptions()
            .SetDefaultCulture("en")
            .AddSupportedCultures("en", "de")
            .AddSupportedUICultures("en", "de");
        app.UseRequestLocalization(localizationOptions);

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseAntiforgery();

        if (app.Environment.EnvironmentName.Equals("Playwright", StringComparison.OrdinalIgnoreCase))
            app.UseStaticFiles();
        else
            app.MapStaticAssets();

        app.MapControllers();
        app.MapHub<EndpointHub>("/hubs/endpoint");

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        return app;
    }

    private static async Task EnsureDatabaseInitializedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}
