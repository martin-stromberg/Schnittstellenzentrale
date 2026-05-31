using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Controllers;

/// <summary>Abstrakte Basisklasse für alle API-Controller der Anwendung.</summary>
[AllowAnonymous]
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    private const string TeamStorageMode = "Team";

    private readonly ITokenStore _tokenStore;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ApiControllerBase"/>.</summary>
    protected ApiControllerBase(ITokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    /// <summary>Prüft das Bearer-Token, rotiert es und schreibt das neue Token in den Response-Header.</summary>
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

    /// <summary>Liest den <c>X-Storage-Mode</c>-Header und gibt den entsprechenden <see cref="StorageMode"/> zurück.</summary>
    protected StorageMode ParseStorageMode()
    {
        var storageModeHeader = Request.Headers["X-Storage-Mode"].ToString();
        return storageModeHeader == TeamStorageMode ? StorageMode.Team : StorageMode.User;
    }

    /// <summary>Validiert das Token, liest StorageMode und Owner und gibt ein Kontextobjekt zurück. Gibt <c>null</c> zurück, wenn das Token ungültig ist.</summary>
    protected async Task<RequestContext?> ParseRequestContextAsync()
    {
        var newToken = await ValidateTokenAndSetResponseHeaderAsync();
        if (newToken == null)
            return null;

        var storageMode = ParseStorageMode();
        var owner = Request.Headers["X-Owner"].ToString();
        return new RequestContext(newToken, storageMode, owner);
    }

    /// <summary>Mappt eine <see cref="Application"/> auf ein <see cref="ApplicationResponse"/>-DTO.</summary>
    protected static ApplicationResponse MapToResponse(Application application) => new()
    {
        Id = application.Id,
        Name = application.Name,
        IsSystem = application.IsSystem,
        BaseUrl = application.BaseUrl,
        ApplicationGroupId = application.ApplicationGroupId,
        Description = application.Description,
        InterfaceUrl = application.InterfaceUrl,
        InterfaceType = (int)application.InterfaceType,
        Owner = application.Owner,
        Subtitle = application.Subtitle,
        IconData = application.IconData,
        RowVersion = application.RowVersion
    };

    /// <summary>Mappt eine <see cref="ApplicationGroup"/> auf ein <see cref="ApplicationGroupResponse"/>-DTO.</summary>
    protected static ApplicationGroupResponse MapToResponse(ApplicationGroup group) => new()
    {
        Id = group.Id,
        Name = group.Name,
        IsSystem = group.IsSystem,
        Description = group.Description,
        Subtitle = group.Subtitle,
        IconData = group.IconData,
        RowVersion = group.RowVersion,
        Applications = group.Applications.Select(MapToResponse).ToList()
    };
}

/// <summary>Enthält die aus dem Request extrahierten Kontextinformationen.</summary>
public sealed record RequestContext(AuthToken NewToken, StorageMode StorageMode, string Owner);
