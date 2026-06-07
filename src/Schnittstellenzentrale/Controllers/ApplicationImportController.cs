using Microsoft.AspNetCore.Mvc;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Controllers;

/// <summary>Stellt den Import-Endpunkt für Anwendungen bereit.</summary>
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

    /// <summary>Berechnet den Import-Diff für die angegebene Anwendung. Der Import-Service wird anhand des Interface-Typs der Anwendung ausgewählt.</summary>
    /// <param name="id">Die Anwendungs-ID.</param>
    /// <response code="200">Der berechnete ImportDiff.</response>
    /// <response code="401">Token fehlt oder ist ungültig.</response>
    /// <response code="404">Anwendung nicht gefunden.</response>
    /// <response code="422">Der Interface-Typ der Anwendung unterstützt keinen Import.</response>
    [HttpPost("{id:int}/import")]
    [ProducesResponseType(typeof(ImportDiff), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ImportAsync(int id)
    {
        var newToken = await ValidateTokenAndSetResponseHeaderAsync();
        if (newToken == null)
            return Unauthorized();

        var application = await _applicationRepository.GetApplicationByIdAsync(id);
        if (application == null)
            return NotFound();

        if (application.InterfaceType == InterfaceType.Rest)
        {
            var diff = await _swaggerImportService.ImportAsync(application);
            return Ok(diff);
        }

        if (application.InterfaceType == InterfaceType.OData)
        {
            var diff = await _odataImportService.ImportAsync(application);
            return Ok(diff);
        }

        return UnprocessableEntity(new { errorMessage = "Interface-Typ nicht unterstützt" });
    }
}
