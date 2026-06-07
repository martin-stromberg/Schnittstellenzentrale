using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.OData;

/// <summary>OData-CRUD-Controller für den Entity-Set <c>Applications</c>.</summary>
[Route("odatav4")]
public class ODataApplicationsController : ODataControllerBase
{
    private readonly IApplicationRepository _applicationRepository;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ODataApplicationsController"/>.</summary>
    public ODataApplicationsController(ITokenStore tokenStore, IApplicationRepository applicationRepository)
        : base(tokenStore)
    {
        _applicationRepository = applicationRepository;
    }

    /// <summary>Gibt alle Anwendungen zurück.</summary>
    [EnableQuery]
    [HttpGet("Applications")]
    public async Task<IActionResult> Get()
    {
        var applications = await _applicationRepository.GetApplicationsAsync(StorageMode.Team, string.Empty);
        return Ok(applications.AsQueryable());
    }

    /// <summary>Gibt eine einzelne Anwendung per ID zurück.</summary>
    [EnableQuery]
    [HttpGet("Applications({key})")]
    public async Task<IActionResult> Get(int key)
    {
        var application = await _applicationRepository.GetApplicationByIdAsync(key);
        if (application == null)
            return NotFound();

        return Ok(application);
    }

    /// <summary>Legt eine neue Anwendung an.</summary>
    [HttpPost("Applications")]
    public async Task<IActionResult> Post([FromBody] Application entity)
    {
        entity.Id = 0;
        entity.IsSystem = false;
        entity.RowVersion = [];
        entity.InterfaceType = Application.DetectInterfaceType(entity.InterfaceUrl);

        var saved = await _applicationRepository.AddApplicationAsync(entity);
        return Created($"/odatav4/Applications({saved.Id})", saved);
    }

    /// <summary>Ersetzt eine Anwendung vollständig.</summary>
    [HttpPut("Applications({key})")]
    public async Task<IActionResult> Put(int key, [FromBody] Application entity)
    {
        var existing = await _applicationRepository.GetApplicationByIdAsync(key);
        if (existing == null)
            return NotFound();

        if (existing.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        if (entity.RowVersion.Length == 0)
            return BadRequest("RowVersion ist erforderlich.");

        existing.Name = entity.Name;
        existing.Description = entity.Description;
        existing.BaseUrl = entity.BaseUrl;
        existing.InterfaceUrl = entity.InterfaceUrl;
        existing.InterfaceType = Application.DetectInterfaceType(entity.InterfaceUrl);
        existing.Owner = entity.Owner;
        existing.ApplicationGroupId = entity.ApplicationGroupId;
        existing.Subtitle = entity.Subtitle;
        existing.IconData = entity.IconData;
        existing.RowVersion = entity.RowVersion;

        var saved = await _applicationRepository.UpdateApplicationAsync(existing);
        return Ok(saved);
    }

    /// <summary>Aktualisiert eine Anwendung partiell.</summary>
    [HttpPatch("Applications({key})")]
    public async Task<IActionResult> Patch(int key, [FromBody] JsonElement patch)
    {
        var existing = await _applicationRepository.GetApplicationByIdAsync(key);
        if (existing == null)
            return NotFound();

        if (existing.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        if (!ODataPatchHelper.TryApplyPatch(patch, existing, out var error))
            return BadRequest(error);

        existing.InterfaceType = Application.DetectInterfaceType(existing.InterfaceUrl);

        var saved = await _applicationRepository.UpdateApplicationAsync(existing);
        return Ok(saved);
    }

    /// <summary>Löscht eine Anwendung.</summary>
    [HttpDelete("Applications({key})")]
    public async Task<IActionResult> Delete(int key)
    {
        var existing = await _applicationRepository.GetApplicationByIdAsync(key);
        if (existing == null)
            return NotFound();

        if (existing.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        await _applicationRepository.DeleteApplicationAsync(key);
        return NoContent();
    }
}
