using Microsoft.AspNetCore.Mvc;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Filters;

namespace Schnittstellenzentrale.Controllers;

/// <summary>Verwaltet Endpunktgruppen.</summary>
[Route("api/endpoint-groups")]
public class EndpointGroupsController : ApiControllerBase
{
    private readonly IEndpointRepository _endpointRepository;
    private readonly ISignalRNotificationService _signalRNotificationService;

    /// <summary>Initialisiert eine neue Instanz von <see cref="EndpointGroupsController"/>.</summary>
    public EndpointGroupsController(
        ITokenStore tokenStore,
        IEndpointRepository endpointRepository,
        ISignalRNotificationService signalRNotificationService)
        : base(tokenStore)
    {
        _endpointRepository = endpointRepository;
        _signalRNotificationService = signalRNotificationService;
    }

    /// <summary>
    /// Returns all endpoint groups for the given application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <response code="200">List of endpoint groups.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    [HttpGet]
    [RequiresContextHeaders]
    [ProducesResponseType(typeof(IList<EndpointGroupResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllAsync([FromQuery] int applicationId)
    {
        var context = await ParseRequestContextAsync();
        if (context == null)
            return Unauthorized();

        var groups = await _endpointRepository.GetEndpointGroupsAsync(applicationId);
        var response = groups.Select(MapToResponse).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Returns a single endpoint group by ID.
    /// </summary>
    /// <param name="id">The endpoint group ID.</param>
    /// <response code="200">The requested endpoint group.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    /// <response code="404">Endpoint group not found.</response>
    [HttpGet("{id:int}")]
    [RequiresContextHeaders]
    [ProducesResponseType(typeof(EndpointGroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var context = await ParseRequestContextAsync();
        if (context == null)
            return Unauthorized();

        var group = await _endpointRepository.GetEndpointGroupByIdAsync(id);
        if (group == null)
            return NotFound();

        return Ok(MapToResponse(group));
    }

    /// <summary>
    /// Creates a new endpoint group.
    /// </summary>
    /// <param name="request">The endpoint group to create.</param>
    /// <response code="201">Endpoint group created successfully.</response>
    /// <response code="400">Invalid request body.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    [HttpPost]
    [RequiresContextHeaders]
    [ProducesResponseType(typeof(EndpointGroupResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateEndpointGroupRequest request)
    {
        var context = await ParseRequestContextAsync();
        if (context == null)
            return Unauthorized();

        var group = new EndpointGroup
        {
            Name = request.Name,
            ApplicationId = request.ApplicationId,
            ParentGroupId = request.ParentGroupId
        };
        var saved = await _endpointRepository.AddEndpointGroupAsync(group);

        if (context.StorageMode == StorageMode.Team)
            await _signalRNotificationService.NotifyEndpointGroupChangedAsync(saved.Id, saved.ApplicationId);

        var response = MapToResponse(saved);
        return CreatedAtAction(null, new { id = saved.Id }, response);
    }

    /// <summary>
    /// Updates an existing endpoint group.
    /// </summary>
    /// <param name="id">The endpoint group ID.</param>
    /// <param name="request">The updated endpoint group data.</param>
    /// <response code="200">Updated endpoint group.</response>
    /// <response code="400">Invalid request body.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    /// <response code="404">Endpoint group not found.</response>
    [HttpPut("{id:int}")]
    [RequiresContextHeaders]
    [ProducesResponseType(typeof(EndpointGroupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateEndpointGroupRequest request)
    {
        var context = await ParseRequestContextAsync();
        if (context == null)
            return Unauthorized();

        var existing = await _endpointRepository.GetEndpointGroupByIdAsync(id);
        if (existing == null)
            return NotFound();

        var group = new EndpointGroup
        {
            Id = id,
            Name = request.Name,
            ApplicationId = existing.ApplicationId,
            ParentGroupId = existing.ParentGroupId,
            RowVersion = request.RowVersion
        };
        var saved = await _endpointRepository.UpdateEndpointGroupAsync(group);

        if (context.StorageMode == StorageMode.Team)
            await _signalRNotificationService.NotifyEndpointGroupChangedAsync(saved.Id, saved.ApplicationId);

        return Ok(MapToResponse(saved));
    }

    /// <summary>
    /// Deletes an endpoint group.
    /// </summary>
    /// <param name="id">The endpoint group ID.</param>
    /// <response code="204">Endpoint group deleted successfully.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    /// <response code="404">Endpoint group not found.</response>
    [HttpDelete("{id:int}")]
    [RequiresContextHeaders]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var context = await ParseRequestContextAsync();
        if (context == null)
            return Unauthorized();

        var group = await _endpointRepository.GetEndpointGroupByIdAsync(id);
        if (group == null)
            return NotFound();

        var applicationId = group.ApplicationId;
        await _endpointRepository.DeleteEndpointGroupAsync(id);

        if (context.StorageMode == StorageMode.Team)
            await _signalRNotificationService.NotifyEndpointGroupChangedAsync(id, applicationId);

        return NoContent();
    }
}
