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

        existing.Name = entity.Name;
        existing.Description = entity.Description;
        existing.BaseUrl = entity.BaseUrl;
        existing.InterfaceUrl = entity.InterfaceUrl;
        existing.InterfaceType = Application.DetectInterfaceType(entity.InterfaceUrl);
        existing.Owner = entity.Owner;
        existing.ApplicationGroupId = entity.ApplicationGroupId;
        existing.Subtitle = entity.Subtitle;
        existing.IconData = entity.IconData;

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

        if (!TryApplyPatch(patch, existing, out var error))
            return BadRequest(error);

        existing.InterfaceType = Application.DetectInterfaceType(existing.InterfaceUrl);

        var saved = await _applicationRepository.UpdateApplicationAsync(existing);
        return Ok(saved);
    }

    // Bleibt privat: Die Signatur (bool + out error für IconData-Base64-Validierung) unterscheidet sich
    // von den void-ApplyPatch-Methoden der anderen OData-Controller und eignet sich nicht für eine
    // gemeinsame protected-Methode in ODataControllerBase.
    private static bool TryApplyPatch(JsonElement patch, Application target, out string? error)
    {
        error = null;
        foreach (var prop in patch.EnumerateObject())
        {
            switch (prop.Name.ToLowerInvariant())
            {
                case "name": target.Name = prop.Value.GetString() ?? target.Name; break;
                case "description": target.Description = prop.Value.GetString() ?? target.Description; break;
                case "baseurl": target.BaseUrl = prop.Value.GetString() ?? target.BaseUrl; break;
                case "interfaceurl": target.InterfaceUrl = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetString(); break;
                case "owner": target.Owner = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetString(); break;
                case "applicationgroupid": target.ApplicationGroupId = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetInt32(); break;
                case "subtitle": target.Subtitle = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetString(); break;
                case "icondata":
                    if (prop.Value.ValueKind == JsonValueKind.Null)
                    {
                        target.IconData = null;
                    }
                    else
                    {
                        var raw = prop.Value.GetString();
                        if (raw == null)
                        {
                            error = "IconData muss ein gültiger Base64-String sein.";
                            return false;
                        }
                        try
                        {
                            target.IconData = Convert.FromBase64String(raw);
                        }
                        catch (FormatException)
                        {
                            error = "IconData muss ein gültiger Base64-String sein.";
                            return false;
                        }
                    }
                    break;
            }
        }
        return true;
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
