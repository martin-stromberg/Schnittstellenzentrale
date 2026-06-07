using Microsoft.AspNetCore.Mvc;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Controllers;

/// <summary>Stellt Import-Endpunkte für Anwendungen bereit (Swagger, OData).</summary>
[Route("api/applications")]
public class ApplicationImportController : ApiControllerBase
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly ISwaggerImportService _swaggerImportService;
    private readonly IODataImportService _odataImportService;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ApplicationImportController"/>.</summary>
    public ApplicationImportController(
        ITokenStore tokenStore,
        IApplicationRepository applicationRepository,
        ISwaggerImportService swaggerImportService,
        IODataImportService odataImportService)
        : base(tokenStore)
    {
        _applicationRepository = applicationRepository;
        _swaggerImportService = swaggerImportService;
        _odataImportService = odataImportService;
    }

    /// <summary>Berechnet den Swagger-Import-Diff für die angegebene Anwendung.</summary>
    /// <param name="id">Die Anwendungs-ID.</param>
    /// <response code="200">Der berechnete ImportDiff.</response>
    /// <response code="401">Token fehlt oder ist ungültig.</response>
    /// <response code="404">Anwendung nicht gefunden.</response>
    [HttpPost("{id:int}/import/swagger")]
    [ProducesResponseType(typeof(ImportDiff), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ImportSwaggerAsync(int id)
    {
        var newToken = await ValidateTokenAndSetResponseHeaderAsync();
        if (newToken == null)
            return Unauthorized();

        var application = await _applicationRepository.GetApplicationByIdAsync(id);
        if (application == null)
            return NotFound();

        var diff = await _swaggerImportService.ImportAsync(application);
        return Ok(diff);
    }

    /// <summary>Berechnet den OData-Import-Diff für die angegebene Anwendung.</summary>
    /// <param name="id">Die Anwendungs-ID.</param>
    /// <response code="200">Der berechnete ImportDiff.</response>
    /// <response code="401">Token fehlt oder ist ungültig.</response>
    /// <response code="404">Anwendung nicht gefunden.</response>
    [HttpPost("{id:int}/import/odata")]
    [ProducesResponseType(typeof(ImportDiff), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ImportODataAsync(int id)
    {
        var newToken = await ValidateTokenAndSetResponseHeaderAsync();
        if (newToken == null)
            return Unauthorized();

        var application = await _applicationRepository.GetApplicationByIdAsync(id);
        if (application == null)
            return NotFound();

        var diff = await _odataImportService.ImportAsync(application);
        return Ok(diff);
    }
}
