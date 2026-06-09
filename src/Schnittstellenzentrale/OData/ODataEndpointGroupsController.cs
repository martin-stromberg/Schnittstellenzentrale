using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
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
        var user = AuthenticatedUser;
        if (user == null)
            return Unauthorized();

        var storageMode = ParseStorageMode();
        var applications = await _applicationRepository.GetApplicationsAsync(storageMode, user);
        var applicationIds = new HashSet<int>(applications.Select(a => a.Id));
        var allGroups = await _endpointRepository.GetAllEndpointGroupsAsync();
        var groups = allGroups.Where(g => applicationIds.Contains(g.ApplicationId)).ToList();
        return Ok(groups.AsQueryable());
    }

    /// <summary>Gibt eine einzelne Endpunktgruppe per ID zurück.</summary>
    [EnableQuery]
    [HttpGet("EndpointGroups({key})")]
    public async Task<IActionResult> Get(int key)
    {
        var user = AuthenticatedUser;
        if (user == null)
            return Unauthorized();

        var group = await _endpointRepository.GetEndpointGroupByIdAsync(key);
        if (group == null)
            return NotFound();

        var storageMode = ParseStorageMode();
        var applications = await _applicationRepository.GetApplicationsAsync(storageMode, user);
        if (!applications.Any(a => a.Id == group.ApplicationId))
            return StatusCode(StatusCodes.Status403Forbidden);

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

        if (existing.Application == null)
            return NotFound();

        if (existing.Application.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        if (entity.RowVersion.Length == 0)
            return BadRequest("RowVersion ist erforderlich.");

        existing.Name = entity.Name;
        existing.ParentGroupId = entity.ParentGroupId;
        existing.RowVersion = entity.RowVersion;

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

        if (existing.Application == null)
            return NotFound();

        if (existing.Application.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        var parentGroupIdProp = patch.EnumerateObject()
            .Cast<System.Text.Json.JsonProperty?>()
            .FirstOrDefault(p => p!.Value.Name.Equals("parentgroupid", StringComparison.OrdinalIgnoreCase));

        if (parentGroupIdProp.HasValue && parentGroupIdProp.Value.Value.ValueKind == JsonValueKind.Number)
        {
            var parentGroupId = parentGroupIdProp.Value.Value.GetInt32();
            var parentGroup = await _endpointRepository.GetEndpointGroupByIdAsync(parentGroupId);
            if (parentGroup == null || parentGroup.ApplicationId != existing.ApplicationId)
                return BadRequest("Die angegebene ParentGroup gehört nicht zur selben Anwendung.");
        }

        var rowVersion = ODataPatchHelper.TryExtractRowVersion(patch);
        if (rowVersion == null)
            return BadRequest("rowVersion is required for PATCH");

        ApplyPatch(patch, existing);
        existing.RowVersion = rowVersion;

        try
        {
            var saved = await _endpointRepository.UpdateEndpointGroupAsync(existing);
            return Ok(saved);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("Die Endpunktgruppe wurde zwischenzeitlich geändert. Bitte die Seite neu laden.");
        }
    }

    private static void ApplyPatch(JsonElement patch, EndpointGroup target)
    {
        foreach (var prop in patch.EnumerateObject())
        {
            switch (prop.Name.ToLowerInvariant())
            {
                case "name": target.Name = prop.Value.GetString() ?? target.Name; break;
                case "parentgroupid":
                    if (prop.Value.ValueKind == JsonValueKind.Null) { target.ParentGroupId = null; break; }
                    if (prop.Value.ValueKind == JsonValueKind.Number) { target.ParentGroupId = prop.Value.GetInt32(); }
                    break;
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

        if (existing.Application == null)
            return NotFound();

        if (existing.Application.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        await _endpointRepository.DeleteEndpointGroupAsync(key);
        return NoContent();
    }
}
