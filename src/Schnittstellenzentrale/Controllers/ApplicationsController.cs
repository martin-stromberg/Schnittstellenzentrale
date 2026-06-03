using Microsoft.AspNetCore.Mvc;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Filters;

namespace Schnittstellenzentrale.Controllers;

/// <summary>
/// Manages applications.
/// </summary>
[Route("api/applications")]
public class ApplicationsController : ApiControllerBase
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly ISignalRNotificationService _signalRNotificationService;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ApplicationsController"/>.</summary>
    public ApplicationsController(
        ITokenStore tokenStore,
        IApplicationRepository applicationRepository,
        ISignalRNotificationService signalRNotificationService)
        : base(tokenStore)
    {
        _applicationRepository = applicationRepository;
        _signalRNotificationService = signalRNotificationService;
    }

    /// <summary>
    /// Returns all applications for the given storage mode and owner.
    /// </summary>
    /// <remarks>Requires the <c>X-Storage-Mode</c> and <c>X-Owner</c> request headers.</remarks>
    /// <response code="200">List of applications.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    [HttpGet]
    [RequiresContextHeaders(includeOwner: true)]
    [ProducesResponseType(typeof(IList<ApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllAsync()
    {
        var context = await ParseRequestContextAsync();
        if (context == null)
            return Unauthorized();

        var applications = await _applicationRepository.GetApplicationsAsync(context.StorageMode, context.Owner);

        var response = applications.Select(MapToResponse).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Returns all applications that are not assigned to any group.
    /// </summary>
    /// <remarks>Requires the <c>X-Storage-Mode</c> and <c>X-Owner</c> request headers.</remarks>
    /// <response code="200">List of ungrouped applications.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    [HttpGet("ungrouped")]
    [RequiresContextHeaders(includeOwner: true)]
    [ProducesResponseType(typeof(IList<ApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUngroupedAsync()
    {
        var context = await ParseRequestContextAsync();
        if (context == null)
            return Unauthorized();

        var applications = await _applicationRepository.GetUngroupedApplicationsAsync(context.StorageMode, context.Owner);

        var response = applications.Select(MapToResponse).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Returns a single application by ID.
    /// </summary>
    /// <param name="id">The application ID.</param>
    /// <response code="200">The requested application.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    /// <response code="404">Application not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var newToken = await ValidateTokenAndSetResponseHeaderAsync();
        if (newToken == null)
            return Unauthorized();

        var application = await _applicationRepository.GetApplicationByIdAsync(id);
        if (application == null)
            return NotFound();

        return Ok(MapToResponse(application));
    }

    /// <summary>
    /// Creates a new application.
    /// </summary>
    /// <param name="request">The application to create.</param>
    /// <response code="201">Application created successfully.</response>
    /// <response code="400">Invalid request body.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    [HttpPost]
    [RequiresContextHeaders]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateApplicationRequest request)
    {
        var newToken = await ValidateTokenAndSetResponseHeaderAsync();
        if (newToken == null)
            return Unauthorized();

        var storageMode = ParseStorageMode();

        var application = new Application();
        ApplyRequestToApplication(application, request);

        var saved = await _applicationRepository.AddApplicationAsync(application);

        if (storageMode == StorageMode.Team)
            await _signalRNotificationService.NotifyTreeChangedAsync();

        var response = MapToResponse(saved);
        return CreatedAtAction(null, new { id = saved.Id }, response);
    }

    /// <summary>
    /// Updates an existing application.
    /// </summary>
    /// <param name="id">The application ID.</param>
    /// <param name="request">The updated application data.</param>
    /// <response code="200">Updated application.</response>
    /// <response code="400">Invalid request body.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    /// <response code="404">Application not found.</response>
    [HttpPut("{id:int}")]
    [RequiresContextHeaders]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateApplicationRequest request)
    {
        var newToken = await ValidateTokenAndSetResponseHeaderAsync();
        if (newToken == null)
            return Unauthorized();

        var storageMode = ParseStorageMode();

        var application = await _applicationRepository.GetApplicationByIdAsync(id);
        if (application == null)
            return NotFound();

        if (application.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        ApplyRequestToApplication(application, request);

        var saved = await _applicationRepository.UpdateApplicationAsync(application);

        if (storageMode == StorageMode.Team)
            await _signalRNotificationService.NotifyTreeChangedAsync();

        return Ok(MapToResponse(saved));
    }

    /// <summary>
    /// Deletes an application.
    /// </summary>
    /// <param name="id">The application ID.</param>
    /// <response code="204">Application deleted successfully.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    /// <response code="404">Application not found.</response>
    [HttpDelete("{id:int}")]
    [RequiresContextHeaders]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var newToken = await ValidateTokenAndSetResponseHeaderAsync();
        if (newToken == null)
            return Unauthorized();

        var storageMode = ParseStorageMode();

        var application = await _applicationRepository.GetApplicationByIdAsync(id);
        if (application == null)
            return NotFound();

        if (application.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        await _applicationRepository.DeleteApplicationAsync(id);

        if (storageMode == StorageMode.Team)
            await _signalRNotificationService.NotifyTreeChangedAsync();

        return NoContent();
    }

    private static void ApplyRequestToApplication(Application application, UpdateApplicationRequest request)
    {
        application.Name = request.Name;
        application.BaseUrl = request.BaseUrl;
        application.Description = request.Description ?? string.Empty;
        application.InterfaceUrl = request.InterfaceUrl;
        application.InterfaceType = Application.DetectInterfaceType(request.InterfaceUrl);
        application.ApplicationGroupId = request.ApplicationGroupId;
        application.Owner = request.Owner;
    }
}
