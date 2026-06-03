using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Endpoint = Schnittstellenzentrale.Core.Models.Endpoint;

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

    /// <summary>Mappt eine <see cref="EndpointGroup"/> auf ein <see cref="EndpointGroupResponse"/>-DTO.</summary>
    protected static EndpointGroupResponse MapToResponse(EndpointGroup group) => new()
    {
        Id = group.Id,
        Name = group.Name,
        ApplicationId = group.ApplicationId,
        ParentGroupId = group.ParentGroupId,
        RowVersion = group.RowVersion
    };

    /// <summary>Mappt einen <see cref="EndpointHeader"/> auf ein <see cref="EndpointKeyValueResponse"/>-DTO.</summary>
    protected static EndpointKeyValueResponse MapToResponse(EndpointHeader header) => new()
    {
        Id = header.Id,
        Key = header.Key,
        Value = header.Value,
        EndpointId = header.EndpointId
    };

    /// <summary>Mappt einen <see cref="EndpointQueryParameter"/> auf ein <see cref="EndpointKeyValueResponse"/>-DTO.</summary>
    protected static EndpointKeyValueResponse MapToResponse(EndpointQueryParameter parameter) => new()
    {
        Id = parameter.Id,
        Key = parameter.Key,
        Value = parameter.Value,
        EndpointId = parameter.EndpointId
    };

    /// <summary>Mappt einen <see cref="Endpoint"/> auf ein <see cref="EndpointResponse"/>-DTO inkl. Headers und QueryParameters.</summary>
    protected static EndpointResponse MapToResponse(Endpoint endpoint) => new()
    {
        Id = endpoint.Id,
        Name = endpoint.Name,
        Method = endpoint.Method,
        RelativePath = endpoint.RelativePath,
        Body = endpoint.Body,
        BodyMode = endpoint.BodyMode,
        AuthenticationType = endpoint.AuthenticationType,
        ApplicationId = endpoint.ApplicationId,
        EndpointGroupId = endpoint.EndpointGroupId,
        RowVersion = endpoint.RowVersion,
        PreRequestScript = endpoint.PreRequestScript,
        PostRequestScript = endpoint.PostRequestScript,
        Headers = endpoint.Headers.Select(MapToResponse).ToList(),
        QueryParameters = endpoint.QueryParameters.Select(MapToResponse).ToList()
    };
}

/// <summary>Enthält die aus dem Request extrahierten Kontextinformationen.</summary>
public sealed record RequestContext(AuthToken NewToken, StorageMode StorageMode, string Owner);
