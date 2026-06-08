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
    private readonly IStorageModeService _storageModeService;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ODataApplicationGroupsController"/>.</summary>
    public ODataApplicationGroupsController(ITokenStore tokenStore, IApplicationRepository applicationRepository, IStorageModeService storageModeService)
        : base(tokenStore)
    {
        _applicationRepository = applicationRepository;
        _storageModeService = storageModeService;
    }

    /// <summary>Gibt alle Anwendungsgruppen zurück.</summary>
    [EnableQuery]
    [HttpGet("ApplicationGroups")]
    public async Task<IActionResult> Get()
    {
        var groups = await _applicationRepository.GetGroupsAsync(_storageModeService.CurrentMode, AuthenticatedUser);
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
