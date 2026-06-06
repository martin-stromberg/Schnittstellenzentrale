using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Schnittstellenzentrale.Core.Interfaces;
using Endpoint = Schnittstellenzentrale.Core.Models.Endpoint;

namespace Schnittstellenzentrale.OData;

/// <summary>OData-CRUD-Controller für den Entity-Set <c>Endpoints</c>.</summary>
[Route("odatav4")]
public class ODataEndpointsController : ODataControllerBase
{
    private readonly IEndpointRepository _endpointRepository;
    private readonly IApplicationRepository _applicationRepository;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ODataEndpointsController"/>.</summary>
    public ODataEndpointsController(
        ITokenStore tokenStore,
        IEndpointRepository endpointRepository,
        IApplicationRepository applicationRepository)
        : base(tokenStore)
    {
        _endpointRepository = endpointRepository;
        _applicationRepository = applicationRepository;
    }

    /// <summary>Gibt alle Endpunkte zurück.</summary>
    [EnableQuery]
    [HttpGet("Endpoints")]
    public async Task<IActionResult> Get()
    {
        var applications = await _applicationRepository.GetApplicationsAsync(Core.Enums.StorageMode.Team, string.Empty);
        var ids = applications.Select(a => a.Id);
        var endpoints = await _endpointRepository.GetEndpointsByApplicationIdsAsync(ids);
        return Ok(endpoints.AsQueryable());
    }

    /// <summary>Gibt einen einzelnen Endpunkt per ID zurück.</summary>
    [EnableQuery]
    [HttpGet("Endpoints({key})")]
    public async Task<IActionResult> Get(int key)
    {
        var endpoint = await _endpointRepository.GetEndpointByIdAsync(key);
        if (endpoint == null)
            return NotFound();

        return Ok(endpoint);
    }

    /// <summary>Legt einen neuen Endpunkt an.</summary>
    [HttpPost("Endpoints")]
    public async Task<IActionResult> Post([FromBody] Endpoint entity)
    {
        var application = await _applicationRepository.GetApplicationByIdAsync(entity.ApplicationId);
        if (application == null)
            return NotFound();

        if (application.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        entity.Id = 0;
        entity.RowVersion = [];

        var saved = await _endpointRepository.AddEndpointAsync(entity);
        return Created($"/odatav4/Endpoints({saved.Id})", saved);
    }

    /// <summary>Ersetzt einen Endpunkt vollständig.</summary>
    [HttpPut("Endpoints({key})")]
    public async Task<IActionResult> Put(int key, [FromBody] Endpoint entity)
    {
        var existing = await _endpointRepository.GetEndpointByIdAsync(key);
        if (existing == null)
            return NotFound();

        if (existing.Application?.IsSystem == true)
            return StatusCode(StatusCodes.Status403Forbidden);

        existing.Name = entity.Name;
        existing.Method = entity.Method;
        existing.RelativePath = entity.RelativePath;
        existing.Body = entity.Body;
        existing.BodyMode = entity.BodyMode;
        existing.AuthenticationType = entity.AuthenticationType;
        existing.EndpointGroupId = entity.EndpointGroupId;
        existing.PreRequestScript = entity.PreRequestScript;
        existing.PostRequestScript = entity.PostRequestScript;

        var saved = await _endpointRepository.UpdateEndpointAsync(existing);
        return Ok(saved);
    }

    /// <summary>Aktualisiert einen Endpunkt partiell.</summary>
    [HttpPatch("Endpoints({key})")]
    public async Task<IActionResult> Patch(int key, [FromBody] JsonElement patch)
    {
        var existing = await _endpointRepository.GetEndpointByIdAsync(key);
        if (existing == null)
            return NotFound();

        if (existing.Application?.IsSystem == true)
            return StatusCode(StatusCodes.Status403Forbidden);

        ApplyPatch(patch, existing);

        var saved = await _endpointRepository.UpdateEndpointAsync(existing);
        return Ok(saved);
    }

    private static void ApplyPatch(JsonElement patch, Endpoint target)
    {
        foreach (var prop in patch.EnumerateObject())
        {
            switch (prop.Name.ToLowerInvariant())
            {
                case "name": target.Name = prop.Value.GetString() ?? target.Name; break;
                case "relativepath": target.RelativePath = prop.Value.GetString() ?? target.RelativePath; break;
                case "body": target.Body = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetString(); break;
                case "prerequestscript": target.PreRequestScript = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetString(); break;
                case "postrequestscript": target.PostRequestScript = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetString(); break;
                case "endpointgroupid": target.EndpointGroupId = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetInt32(); break;
                case "method":
                    if (Enum.TryParse<Schnittstellenzentrale.Core.Enums.HttpMethod>(prop.Value.GetString(), true, out var method))
                        target.Method = method;
                    break;
                case "authenticationtype":
                    if (Enum.TryParse<Schnittstellenzentrale.Core.Enums.AuthenticationType>(prop.Value.GetString(), true, out var auth))
                        target.AuthenticationType = auth;
                    break;
            }
        }
    }

    /// <summary>Löscht einen Endpunkt.</summary>
    [HttpDelete("Endpoints({key})")]
    public async Task<IActionResult> Delete(int key)
    {
        var existing = await _endpointRepository.GetEndpointByIdAsync(key);
        if (existing == null)
            return NotFound();

        if (existing.Application?.IsSystem == true)
            return StatusCode(StatusCodes.Status403Forbidden);

        await _endpointRepository.DeleteEndpointAsync(key);
        return NoContent();
    }
}
