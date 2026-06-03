using Microsoft.AspNetCore.SignalR;
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
    protected override string BindUrl => "http://127.0.0.1:5100";

    /// <inheritdoc/>
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        services.RemoveAll<ISignalRNotificationService>();
        services.AddScoped<ISignalRNotificationService, SignalRNotificationService<EndpointHub>>();
    }

    /// <summary>
    /// Stellt den <see cref="ApiNotificationProxy"/> auf den echten <see cref="IHubContext{EndpointHub}"/>
    /// des Kestrel-Servers um, damit API-Controller-Notifications die verbundenen Browser erreichen.
    /// </summary>
    protected override Task OnAfterStartAsync()
    {
        var hubContext = _app!.Services.GetRequiredService<IHubContext<EndpointHub>>();
        ApiNotificationProxy.SetInner(new SignalRNotificationService<EndpointHub>(hubContext));
        return Task.CompletedTask;
    }
}
