using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.OData;

/// <summary>Authentifizierungsendpunkt für die OData-API — liefert per Negotiate einen Bearer-Token.</summary>
[Authorize]
[Route("odatav4")]
public class ODataAuthController : ControllerBase
{
    private readonly ITokenStore _tokenStore;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ODataAuthController"/>.</summary>
    public ODataAuthController(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    /// <summary>Authentifiziert den aktuellen Windows-Benutzer und gibt einen Bearer-Token für die OData-API zurück (OData Unbound Action, POST).</summary>
    [HttpPost("Authenticate()")]
    [ODataAttributeRouting]
    public Task<IActionResult> AuthenticatePost() => AuthenticateAsync();

    /// <summary>Authentifiziert den aktuellen Windows-Benutzer und gibt einen Bearer-Token für die OData-API zurück (OData Unbound Function, GET).</summary>
    [HttpGet("Authenticate()")]
    public Task<IActionResult> AuthenticateGet() => AuthenticateAsync();

    private async Task<IActionResult> AuthenticateAsync()
    {
        var username = HttpContext.User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var token = await _tokenStore.CreateTokenAsync(username);
        return Ok(new AuthenticateResponse { Token = token.TokenValue });
    }
}
