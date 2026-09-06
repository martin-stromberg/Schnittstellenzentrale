using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.OData;

/// <summary>OData-CRUD-Controller für den Entity-Set <c>ApplicationGroups</c>.</summary>
[Route("odatav4")]
public class ODataApplicationGroupsController : ODataControllerBase
{
    private readonly IApplicationRepository _applicationRepository;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ODataApplicationGroupsController"/>.</summary>
    /// <param name="tokenStore">Der Token-Store.</param>
    /// <param name="applicationRepository">Das Repository für Anwendungen.</param>
    public ODataApplicationGroupsController(ITokenStore tokenStore, IApplicationRepository applicationRepository)
        : base(tokenStore)
    {
        _applicationRepository = applicationRepository;
    }

    /// <summary>Gibt alle Anwendungsgruppen zurück.</summary>
    /// <returns>Das Ergebnis.</returns>
    [EnableQuery]
    [HttpGet("ApplicationGroups")]
    public async Task<IActionResult> Get()
    {
        var user = AuthenticatedUser;
        if (user == null)
            return Unauthorized();

        var storageMode = ParseStorageMode();
        var groups = await _applicationRepository.GetGroupsAsync(storageMode, user);

        if (storageMode == Core.Enums.StorageMode.User)
        {
            var systemGroup = await _applicationRepository.GetSystemGroupAsync();
            if (systemGroup != null && !groups.Any(g => g.Id == systemGroup.Id))
                groups = [.. groups, systemGroup];
        }

        return Ok(groups.AsQueryable());
    }

    /// <summary>Gibt eine einzelne Anwendungsgruppe per ID zurück.</summary>
    /// <param name="key">Der Schlüssel der Entität.</param>
    /// <returns>Das Ergebnis.</returns>
    [EnableQuery]
    [HttpGet("ApplicationGroups({key})")]
    public async Task<IActionResult> Get(int key)
    {
        var user = AuthenticatedUser;
        if (user == null)
            return Unauthorized();

        var group = await _applicationRepository.GetGroupByIdAsync(key);
        if (group == null)
            return NotFound();

        var storageMode = ParseStorageMode();
        if (!group.IsSystem)
        {
            var ownedGroups = await _applicationRepository.GetGroupsAsync(storageMode, user);
            if (!ownedGroups.Any(g => g.Id == key))
                return StatusCode(StatusCodes.Status403Forbidden);
        }

        return Ok(group);
    }

    /// <summary>Legt eine neue Anwendungsgruppe an.</summary>
    /// <param name="entity">Die zu verarbeitende Entität.</param>
    /// <returns>Das Ergebnis.</returns>
    [HttpPost("ApplicationGroups")]
    public async Task<IActionResult> Post([FromBody] ApplicationGroup entity)
    {
        var user = AuthenticatedUser;
        if (user == null)
            return Unauthorized();

        entity.Id = 0;
        entity.IsSystem = false;
        entity.RowVersion = [];

        var saved = await _applicationRepository.AddGroupAsync(entity);
        return Created($"/odatav4/ApplicationGroups({saved.Id})", saved);
    }

    /// <summary>Ersetzt eine Anwendungsgruppe vollständig.</summary>
    /// <param name="key">Der Schlüssel der Entität.</param>
    /// <param name="entity">Die zu verarbeitende Entität.</param>
    /// <returns>Das Ergebnis.</returns>
    [HttpPut("ApplicationGroups({key})")]
    public async Task<IActionResult> Put(int key, [FromBody] ApplicationGroup entity)
    {
        var existing = await _applicationRepository.GetGroupByIdAsync(key);
        if (existing == null)
            return NotFound();

        if (existing.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        if (entity.RowVersion.Length == 0)
            return BadRequest("RowVersion ist erforderlich.");

        existing.Name = entity.Name;
        existing.Description = entity.Description;
        existing.Subtitle = entity.Subtitle;
        existing.IconData = entity.IconData;
        existing.RowVersion = entity.RowVersion;

        var saved = await _applicationRepository.UpdateGroupAsync(existing);
        return Ok(saved);
    }

    /// <summary>Aktualisiert eine Anwendungsgruppe partiell.</summary>
    /// <param name="key">Der Schlüssel der Entität.</param>
    /// <param name="patch">Das JSON-Patch-Dokument.</param>
    /// <returns>Das Ergebnis.</returns>
    [HttpPatch("ApplicationGroups({key})")]
    public async Task<IActionResult> Patch(int key, [FromBody] JsonElement patch)
    {
        var existing = await _applicationRepository.GetGroupByIdAsync(key);
        if (existing == null)
            return NotFound();

        if (existing.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        if (!ODataPatchHelper.ContainsRowVersion(patch))
            return BadRequest("RowVersion ist erforderlich.");

        if (!ODataPatchHelper.TryApplyPatch(patch, existing, out var error))
            return BadRequest(error);

        try
        {
            var saved = await _applicationRepository.UpdateGroupAsync(existing);
            return Ok(saved);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("Die Anwendungsgruppe wurde zwischenzeitlich geändert. Bitte die Seite neu laden.");
        }
    }

    /// <summary>Löscht eine Anwendungsgruppe.</summary>
    /// <param name="key">Der Schlüssel der Entität.</param>
    /// <returns>Das Ergebnis.</returns>
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
