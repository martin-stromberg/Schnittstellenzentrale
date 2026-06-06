using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.OData;

/// <summary>OData-CRUD-Controller für den Entity-Set <c>EndpointGroups</c>.</summary>
[Route("odatav4")]
public class ODataEndpointGroupsController : ODataControllerBase
{
    private readonly IEndpointRepository _endpointRepository;
    private readonly IApplicationRepository _applicationRepository;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ODataEndpointGroupsController"/>.</summary>
    public ODataEndpointGroupsController(
        ITokenStore tokenStore,
        IEndpointRepository endpointRepository,
        IApplicationRepository applicationRepository)
        : base(tokenStore)
    {
        _endpointRepository = endpointRepository;
        _applicationRepository = applicationRepository;
    }

    /// <summary>Gibt alle Endpunktgruppen zurück.</summary>
    [EnableQuery]
    [HttpGet("EndpointGroups")]
    public async Task<IActionResult> Get()
    {
        var groups = await _endpointRepository.GetAllEndpointGroupsAsync();
        return Ok(groups.AsQueryable());
    }

    /// <summary>Gibt eine einzelne Endpunktgruppe per ID zurück.</summary>
    [EnableQuery]
    [HttpGet("EndpointGroups({key})")]
    public async Task<IActionResult> Get(int key)
    {
        var group = await _endpointRepository.GetEndpointGroupByIdAsync(key);
        if (group == null)
            return NotFound();

        return Ok(group);
    }

    /// <summary>Legt eine neue Endpunktgruppe an.</summary>
    [HttpPost("EndpointGroups")]
    public async Task<IActionResult> Post([FromBody] EndpointGroup entity)
    {
        var application = await _applicationRepository.GetApplicationByIdAsync(entity.ApplicationId);
        if (application == null)
            return NotFound();

        if (application.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        entity.Id = 0;
        entity.RowVersion = [];

        var saved = await _endpointRepository.AddEndpointGroupAsync(entity);
        return Created($"/odatav4/EndpointGroups({saved.Id})", saved);
    }

    /// <summary>Ersetzt eine Endpunktgruppe vollständig.</summary>
    [HttpPut("EndpointGroups({key})")]
    public async Task<IActionResult> Put(int key, [FromBody] EndpointGroup entity)
    {
        var existing = await _endpointRepository.GetEndpointGroupByIdAsync(key);
        if (existing == null)
            return NotFound();

        if (existing.Application?.IsSystem == true)
            return StatusCode(StatusCodes.Status403Forbidden);

        existing.Name = entity.Name;
        existing.ParentGroupId = entity.ParentGroupId;

        var saved = await _endpointRepository.UpdateEndpointGroupAsync(existing);
        return Ok(saved);
    }

    /// <summary>Aktualisiert eine Endpunktgruppe partiell.</summary>
    [HttpPatch("EndpointGroups({key})")]
    public async Task<IActionResult> Patch(int key, [FromBody] JsonElement patch)
    {
        var existing = await _endpointRepository.GetEndpointGroupByIdAsync(key);
        if (existing == null)
            return NotFound();

        if (existing.Application?.IsSystem == true)
            return StatusCode(StatusCodes.Status403Forbidden);

        ApplyPatch(patch, existing);

        var saved = await _endpointRepository.UpdateEndpointGroupAsync(existing);
        return Ok(saved);
    }

    private static void ApplyPatch(JsonElement patch, EndpointGroup target)
    {
        foreach (var prop in patch.EnumerateObject())
        {
            switch (prop.Name.ToLowerInvariant())
            {
                case "name": target.Name = prop.Value.GetString() ?? target.Name; break;
                case "parentgroupid": target.ParentGroupId = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetInt32(); break;
            }
        }
    }

    /// <summary>Löscht eine Endpunktgruppe.</summary>
    [HttpDelete("EndpointGroups({key})")]
    public async Task<IActionResult> Delete(int key)
    {
        var existing = await _endpointRepository.GetEndpointGroupByIdAsync(key);
        if (existing == null)
            return NotFound();

        if (existing.Application?.IsSystem == true)
            return StatusCode(StatusCodes.Status403Forbidden);

        await _endpointRepository.DeleteEndpointGroupAsync(key);
        return NoContent();
    }
}
