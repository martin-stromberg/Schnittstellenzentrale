#pragma warning disable CS1591
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Controllers;

[AllowAnonymous]
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private const string TeamStorageMode = "Team";

    private readonly ITokenStore _tokenStore;

    protected ApiControllerBase(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    protected async Task<AuthToken?> ValidateTokenAndSetResponseHeaderAsync()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;

        var tokenString = authHeader["Bearer ".Length..].Trim();
        var newToken = await _tokenStore.ValidateAndRotateAsync(tokenString);
        if (newToken == null)
            return null;

        Response.Headers["X-New-Token"] = newToken.TokenValue;
        return newToken;
    }

    protected StorageMode ParseStorageMode()
    {
        var storageModeHeader = Request.Headers["X-Storage-Mode"].ToString();
        return storageModeHeader == TeamStorageMode ? StorageMode.Team : StorageMode.User;
    }

    protected static ApplicationResponse MapToResponse(Application application) => new()
    {
        Id = application.Id,
        Name = application.Name,
        BaseUrl = application.BaseUrl,
        ApplicationGroupId = application.ApplicationGroupId,
        Description = application.Description,
        InterfaceUrl = application.InterfaceUrl,
        InterfaceType = (int)application.InterfaceType,
        Owner = application.Owner
    };
}
