using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.OData;

/// <summary>OData-CRUD-Controller für den Entity-Set <c>ApplicationGroups</c>.</summary>
[Route("odatav4")]
public class ODataApplicationGroupsController : ODataControllerBase
{
    private readonly IApplicationRepository _applicationRepository;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ODataApplicationGroupsController"/>.</summary>
    public ODataApplicationGroupsController(ITokenStore tokenStore, IApplicationRepository applicationRepository)
        : base(tokenStore)
    {
        _applicationRepository = applicationRepository;
    }

    /// <summary>Gibt alle Anwendungsgruppen zurück.</summary>
    [EnableQuery]
    [HttpGet("ApplicationGroups")]
    public async Task<IActionResult> Get()
    {
        var groups = await _applicationRepository.GetGroupsAsync(StorageMode.Team, string.Empty);
        return Ok(groups.AsQueryable());
    }

    /// <summary>Gibt eine einzelne Anwendungsgruppe per ID zurück.</summary>
    [EnableQuery]
    [HttpGet("ApplicationGroups({key})")]
    public async Task<IActionResult> Get(int key)
    {
        var group = await _applicationRepository.GetGroupByIdAsync(key);
        if (group == null)
            return NotFound();

        return Ok(group);
    }

    /// <summary>Legt eine neue Anwendungsgruppe an.</summary>
    [HttpPost("ApplicationGroups")]
    public async Task<IActionResult> Post([FromBody] ApplicationGroup entity)
    {
        entity.Id = 0;
        entity.RowVersion = [];

        var saved = await _applicationRepository.AddGroupAsync(entity);
        return Created($"/odatav4/ApplicationGroups({saved.Id})", saved);
    }

    /// <summary>Ersetzt eine Anwendungsgruppe vollständig.</summary>
    [HttpPut("ApplicationGroups({key})")]
    public async Task<IActionResult> Put(int key, [FromBody] ApplicationGroup entity)
    {
        var existing = await _applicationRepository.GetGroupByIdAsync(key);
        if (existing == null)
            return NotFound();

        if (existing.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        existing.Name = entity.Name;
        existing.Description = entity.Description;
        existing.Subtitle = entity.Subtitle;
        existing.IconData = entity.IconData;

        var saved = await _applicationRepository.UpdateGroupAsync(existing);
        return Ok(saved);
    }

    /// <summary>Aktualisiert eine Anwendungsgruppe partiell.</summary>
    [HttpPatch("ApplicationGroups({key})")]
    public async Task<IActionResult> Patch(int key, [FromBody] JsonElement patch)
    {
        var existing = await _applicationRepository.GetGroupByIdAsync(key);
        if (existing == null)
            return NotFound();

        if (existing.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        ApplyPatch(patch, existing);

        var saved = await _applicationRepository.UpdateGroupAsync(existing);
        return Ok(saved);
    }

    private static void ApplyPatch(JsonElement patch, ApplicationGroup target)
    {
        foreach (var prop in patch.EnumerateObject())
        {
            switch (prop.Name.ToLowerInvariant())
            {
                case "name": target.Name = prop.Value.GetString() ?? target.Name; break;
                case "description": target.Description = prop.Value.GetString() ?? target.Description; break;
                case "subtitle": target.Subtitle = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetString(); break;
            }
        }
    }

    /// <summary>Löscht eine Anwendungsgruppe.</summary>
    [HttpDelete("ApplicationGroups({key})")]
    public async Task<IActionResult> Delete(int key)
    {
        var existing = await _applicationRepository.GetGroupByIdAsync(key);
        if (existing == null)
            return NotFound();

        if (existing.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        await _applicationRepository.DeleteGroupAsync(key);
        return NoContent();
    }
}
