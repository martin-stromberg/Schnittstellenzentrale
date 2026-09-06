using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Schnittstellenzentrale.Tests.Helpers;

/// <summary>Authentifizierungs-Handler für Tests; liefert immer <c>TEST\testuser</c> als Benutzer zurück.</summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>Initialisiert den Handler mit den erforderlichen Abhängigkeiten.</summary>
    /// <param name="options">Monitor für die Scheme-Optionen.</param>
    /// <param name="logger">Logger-Factory für den Handler.</param>
    /// <param name="encoder">URL-Encoder für den Handler.</param>
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    /// <inheritdoc/>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "TEST\\testuser") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
