using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Tests.Helpers;

namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>Startet einen echten Kestrel-Server für Playwright-Tests. Ersetzt WebApplicationFactory.</summary>
public class PlaywrightServer : IAsyncLifetime
{
    private WebApplication? _app;

    /// <summary>Feste Basis-URL für Playwright-Tests.</summary>
    public const string PlaywrightBaseUrl = "http://127.0.0.1:5099";
    private const string DbName = "schnittstellenzentrale-playwright.db";

    /// <summary>Basis-URL des gestarteten Kestrel-Servers.</summary>
    public string BaseAddress => PlaywrightBaseUrl;

    /// <summary>Service-Provider des laufenden Servers.</summary>
    public IServiceProvider Services => _app!.Services;

    /// <summary>Kann in Unterklassen überschrieben werden, um weitere Services zu ersetzen.</summary>
    protected virtual void ConfigureTestServices(IServiceCollection services) { }

    /// <inheritdoc/>
    public async Task InitializeAsync()
    {
        foreach (var f in new[] { DbName, DbName + "-wal", DbName + "-shm" })
            if (File.Exists(f)) File.Delete(f);

        var signalRMock = new Mock<ISignalRNotificationService>();
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(s => s.GetCurrentUserName()).Returns("TEST\\testuser");

        var contentRoot = Path.GetDirectoryName(typeof(Program).Assembly.Location)!;

        _app = await Program.BuildWebApplicationAsync(
            [],
            new WebApplicationOptions
            {
                EnvironmentName = "Playwright",
                ContentRootPath = contentRoot
            },
            services =>
            {
                services.RemoveAll<IAuthenticationSchemeProvider>();
                services.RemoveAll<IAuthenticationHandlerProvider>();
                services.RemoveAll<IConfigureOptions<AuthenticationOptions>>();
                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", _ => { });

                services.RemoveAll<IHostedService>();

                services.RemoveAll<ISignalRNotificationService>();
                services.AddScoped(_ => signalRMock.Object);

                services.RemoveAll<ICurrentUserService>();
                services.AddSingleton(_ => currentUserMock.Object);

                ConfigureTestServices(services);
            });

        await _app.StartAsync();
    }

    /// <inheritdoc/>
    public async Task DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.StopAsync();
            await _app.DisposeAsync();
        }
    }
}
