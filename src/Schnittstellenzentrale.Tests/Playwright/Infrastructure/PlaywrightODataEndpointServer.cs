using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>
/// Playwright-Server für OData-Endpunkt-Ausführungstests.
/// Ersetzt <see cref="ICredentialService"/> durch einen Mock, der für Bearer-Token-Endpunkte
/// ein festes Test-Token zurückgibt. Da <see cref="PlaywrightApiFactory"/> einen
/// <c>PermissiveTokenStore</c> verwendet, der jeden Token-String akzeptiert, können
/// so OData-GET-Endpunkte im Playwright-Testlauf erfolgreich ausgeführt werden.
/// </summary>
public class PlaywrightODataEndpointServer : PlaywrightServer
{
    private const string TestBearerToken = "playwright-odata-bearer-token";

    /// <inheritdoc/>
    protected override string BindUrl => "http://127.0.0.1:5103";

    /// <inheritdoc/>
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        var credentialMock = new Mock<ICredentialService>();
        credentialMock.Setup(s => s.GetPassword(It.IsAny<string>())).Returns(TestBearerToken);
        credentialMock.Setup(s => s.SavePassword(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        credentialMock.Setup(s => s.DeletePassword(It.IsAny<string>()));

        services.RemoveAll<ICredentialService>();
        services.AddSingleton(_ => credentialMock.Object);
    }
}
