#pragma warning disable CS1591
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Controllers;

/// <summary>
/// Authentication endpoints.
/// </summary>
[Authorize]
[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly ITokenStore _tokenStore;

    public AuthController(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    /// <summary>
    /// Authenticates the current Windows user and returns a bearer token.
    /// </summary>
    /// <returns>A bearer token for use with all other API endpoints.</returns>
    /// <response code="200">Authentication successful, token returned.</response>
    /// <response code="401">Windows authentication failed or user identity unavailable.</response>
    [HttpPost("/authenticate")]
    [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AuthenticateAsync()
    {
        var username = HttpContext.User.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return Unauthorized();

        var token = await _tokenStore.CreateTokenAsync(username);
        return Ok(new AuthenticateResponse { Token = token.TokenValue });
    }
}
