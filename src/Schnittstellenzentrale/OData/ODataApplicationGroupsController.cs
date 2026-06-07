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
        entity.IsSystem = false;
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

        // DB-RowVersion sichern, bevor Felder überschrieben werden.
        // Das Repository setzt OriginalValue = existing.RowVersion für den EF-Concurrency-Check.
        // Daher muss existing.RowVersion den vom Client gesendeten Wert enthalten — nicht den beim
        // Laden gelesenen DB-Wert, da sonst der Check immer erfolgreich wäre.
        var concurrencyRowVersion = entity.RowVersion.Length > 0 ? entity.RowVersion : existing.RowVersion;

        existing.Name = entity.Name;
        existing.Description = entity.Description;
        existing.Subtitle = entity.Subtitle;
        existing.IconData = entity.IconData;
        existing.RowVersion = concurrencyRowVersion;

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

        if (!ODataPatchHelper.TryApplyPatch(patch, existing, out var error))
            return BadRequest(error);

        var saved = await _applicationRepository.UpdateGroupAsync(existing);
        return Ok(saved);
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
