using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Hubs;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>Variante von <see cref="PlaywrightServer"/> mit echtem SignalR für Echtzeitsynchronisations-Tests.</summary>
public class PlaywrightSignalRServer : PlaywrightServer
{
    /// <inheritdoc/>
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.RemoveAll<ISignalRNotificationService>();
        services.AddScoped<ISignalRNotificationService, SignalRNotificationService<EndpointHub>>();
    }
}
