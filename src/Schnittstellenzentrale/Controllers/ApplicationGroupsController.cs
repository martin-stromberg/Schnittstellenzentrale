using Microsoft.AspNetCore.Mvc;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Filters;

namespace Schnittstellenzentrale.Controllers;

/// <summary>
/// Manages application groups.
/// </summary>
[Route("api/application-groups")]
public class ApplicationGroupsController : ApiControllerBase
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly ISignalRNotificationService _signalRNotificationService;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ApplicationGroupsController"/>.</summary>
    public ApplicationGroupsController(
        ITokenStore tokenStore,
        IApplicationRepository applicationRepository,
        ISignalRNotificationService signalRNotificationService)
        : base(tokenStore)
    {
        _applicationRepository = applicationRepository;
        _signalRNotificationService = signalRNotificationService;
    }

    /// <summary>
    /// Returns all application groups for the given storage mode and owner.
    /// </summary>
    /// <remarks>Requires the <c>X-Storage-Mode</c> and <c>X-Owner</c> request headers.</remarks>
    /// <response code="200">List of application groups.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    [HttpGet]
    [RequiresContextHeaders(includeOwner: true)]
    [ProducesResponseType(typeof(IList<ApplicationGroupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllAsync()
    {
        var context = await ParseRequestContextAsync();
        if (context == null)
            return Unauthorized();

        var groups = await _applicationRepository.GetGroupsAsync(context.StorageMode, context.Owner);

        var response = groups.Select(MapToResponse).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Returns a single application group by ID.
    /// </summary>
    /// <param name="id">The application group ID.</param>
    /// <response code="200">The requested application group.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    /// <response code="404">Application group not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApplicationGroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var newToken = await ValidateTokenAndSetResponseHeaderAsync();
        if (newToken == null)
            return Unauthorized();

        var group = await _applicationRepository.GetGroupByIdAsync(id);
        if (group == null)
            return NotFound();

        return Ok(MapToResponse(group));
    }

    /// <summary>
    /// Creates a new application group.
    /// </summary>
    /// <param name="request">The application group to create.</param>
    /// <response code="201">Application group created successfully.</response>
    /// <response code="400">Invalid request body.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    [HttpPost]
    [RequiresContextHeaders]
    [ProducesResponseType(typeof(ApplicationGroupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateApplicationGroupRequest request)
    {
        var newToken = await ValidateTokenAndSetResponseHeaderAsync();
        if (newToken == null)
            return Unauthorized();

        var storageMode = ParseStorageMode();

        var group = new ApplicationGroup { Name = request.Name };
        var saved = await _applicationRepository.AddGroupAsync(group);

        if (storageMode == StorageMode.Team)
            await _signalRNotificationService.NotifyTreeChangedAsync();

        var response = MapToResponse(saved);
        return CreatedAtAction(null, new { id = saved.Id }, response);
    }

    /// <summary>
    /// Updates an existing application group.
    /// </summary>
    /// <param name="id">The application group ID.</param>
    /// <param name="request">The updated application group data.</param>
    /// <response code="200">Updated application group.</response>
    /// <response code="400">Invalid request body.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    /// <response code="404">Application group not found.</response>
    [HttpPut("{id:int}")]
    [RequiresContextHeaders]
    [ProducesResponseType(typeof(ApplicationGroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateApplicationGroupRequest request)
    {
        var newToken = await ValidateTokenAndSetResponseHeaderAsync();
        if (newToken == null)
            return Unauthorized();

        var storageMode = ParseStorageMode();

        var group = await _applicationRepository.GetGroupByIdAsync(id);
        if (group == null)
            return NotFound();

        if (group.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        group.Name = request.Name;
        var saved = await _applicationRepository.UpdateGroupAsync(group);

        if (storageMode == StorageMode.Team)
            await _signalRNotificationService.NotifyTreeChangedAsync();

        return Ok(MapToResponse(saved));
    }

    /// <summary>
    /// Deletes an application group.
    /// </summary>
    /// <param name="id">The application group ID.</param>
    /// <response code="204">Application group deleted successfully.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    /// <response code="404">Application group not found.</response>
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

        var group = await _applicationRepository.GetGroupByIdAsync(id);
        if (group == null)
            return NotFound();

        if (group.IsSystem)
            return StatusCode(StatusCodes.Status403Forbidden);

        await _applicationRepository.DeleteGroupAsync(id);

        if (storageMode == StorageMode.Team)
            await _signalRNotificationService.NotifyTreeChangedAsync();

        return NoContent();
    }

}
