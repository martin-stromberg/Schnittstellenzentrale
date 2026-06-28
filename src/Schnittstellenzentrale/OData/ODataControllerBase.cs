using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Filters;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.OData;

/// <summary>Abstrakte Basisklasse für alle OData-Controller; reimplementiert Token-Validierung aus <see cref="Controllers.ApiControllerBase"/>.</summary>
[AllowAnonymous]
[ApiExplorerSettings(IgnoreApi = true)]
public abstract class ODataControllerBase : ControllerBase, IAsyncActionFilter
{
    private readonly ITokenStore _tokenStore;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ODataControllerBase"/>.</summary>
    protected ODataControllerBase(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    /// <summary>Gibt den Benutzernamen zurück, der durch den validierten Bearer-Token authentifiziert wurde, oder <c>null</c> wenn kein authentifizierter Benutzer im Kontext gesetzt ist.</summary>
    protected string? AuthenticatedUser =>
        HttpContext.Items.TryGetValue("ODataAuthenticatedUser", out var user) ? (string)user! : null;

    /// <summary>Liest den <c>X-Storage-Mode</c>-Header und gibt den entsprechenden <see cref="Core.Enums.StorageMode"/> zurück.</summary>
    protected Core.Enums.StorageMode ParseStorageMode()
    {
        var storageModeHeader = HttpContext.Request.Headers["X-Storage-Mode"].ToString();
        return storageModeHeader == "Team" ? Core.Enums.StorageMode.Team : Core.Enums.StorageMode.User;
    }

    /// <inheritdoc/>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var authHeader = context.HttpContext.Request.Headers.Authorization.ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var tokenString = authHeader["Bearer ".Length..].Trim();
        var newToken = await _tokenStore.ValidateAndRotateAsync(tokenString);
        if (newToken == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        context.HttpContext.Items["ODataAuthenticatedUser"] = newToken.WindowsUsername;
        context.HttpContext.Response.Headers["X-New-Token"] = newToken.TokenValue;
        await next();
    }
}
