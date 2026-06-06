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

        context.HttpContext.Response.Headers["X-New-Token"] = newToken.TokenValue;
        await next();
    }
}
