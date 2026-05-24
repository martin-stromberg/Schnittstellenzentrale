using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Hubs;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>Variante von <see cref="PlaywrightTestFactory"/> mit echtem SignalR für Echtzeitsynchronisations-Tests.</summary>
public class PlaywrightSignalRFactory : PlaywrightTestFactory
{
    /// <inheritdoc/>
    protected override void ConfigureTestServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    {
        base.ConfigureTestServices(services);

        services.RemoveAll<ISignalRNotificationService>();
        services.AddScoped<ISignalRNotificationService, SignalRNotificationService<EndpointHub>>();
    }
}
