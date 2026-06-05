using Microsoft.AspNetCore.Mvc;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;
using Schnittstellenzentrale.Filters;
using Endpoint = Schnittstellenzentrale.Core.Models.Endpoint;

namespace Schnittstellenzentrale.Controllers;

/// <summary>Verwaltet Endpunkte.</summary>
[Route("api/endpoints")]
public class EndpointsController : ApiControllerBase
{
    private readonly IEndpointRepository _endpointRepository;
    private readonly ISignalRNotificationService _signalRNotificationService;

    /// <summary>Initialisiert eine neue Instanz von <see cref="EndpointsController"/>.</summary>
    public EndpointsController(
        ITokenStore tokenStore,
        IEndpointRepository endpointRepository,
        ISignalRNotificationService signalRNotificationService)
        : base(tokenStore)
    {
        _endpointRepository = endpointRepository;
        _signalRNotificationService = signalRNotificationService;
    }

    /// <summary>
    /// Returns all endpoints for the given application.
    /// </summary>
    /// <param name="applicationId">The application ID.</param>
    /// <param name="groupId">Optional endpoint group ID to filter by.</param>
    /// <response code="200">List of endpoints.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    [HttpGet]
    [RequiresContextHeaders]
    [ProducesResponseType(typeof(IList<EndpointResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllAsync([FromQuery] int applicationId, [FromQuery] int? groupId = null)
    {
        var context = await ParseRequestContextAsync();
        if (context == null)
            return Unauthorized();

        IList<Endpoint> endpoints;
        if (groupId.HasValue)
        {
            var group = await _endpointRepository.GetEndpointGroupByIdAsync(groupId.Value);
            if (group == null || group.ApplicationId != applicationId)
                return NotFound();
            endpoints = await _endpointRepository.GetByGroupIdAsync(groupId.Value);
        }
        else
        {
            endpoints = await _endpointRepository.GetEndpointsAsync(applicationId);
        }

        var response = endpoints.Select(MapToResponse).ToList();
        return Ok(response);
    }

    /// <summary>
    /// Returns a single endpoint by ID.
    /// </summary>
    /// <param name="id">The endpoint ID.</param>
    /// <response code="200">The requested endpoint.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    /// <response code="404">Endpoint not found.</response>
    [HttpGet("{id:int}")]
    [RequiresContextHeaders]
    [ProducesResponseType(typeof(EndpointResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var context = await ParseRequestContextAsync();
        if (context == null)
            return Unauthorized();

        var endpoint = await _endpointRepository.GetEndpointByIdAsync(id);
        if (endpoint == null)
            return NotFound();

        return Ok(MapToResponse(endpoint));
    }

    /// <summary>
    /// Creates a new endpoint.
    /// </summary>
    /// <param name="request">The endpoint to create.</param>
    /// <response code="201">Endpoint created successfully.</response>
    /// <response code="400">Invalid request body.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    [HttpPost]
    [RequiresContextHeaders]
    [ProducesResponseType(typeof(EndpointResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateEndpointRequest request)
    {
        var context = await ParseRequestContextAsync();
        if (context == null)
            return Unauthorized();

        var endpoint = new Endpoint
        {
            Name = request.Name,
            RelativePath = request.RelativePath,
            ApplicationId = request.ApplicationId,
            EndpointGroupId = request.EndpointGroupId,
            Method = request.Method,
            BodyMode = request.BodyMode,
            Body = request.Body,
            AuthenticationType = request.AuthenticationType,
            PreRequestScript = request.PreRequestScript,
            PostRequestScript = request.PostRequestScript
        };
        var saved = await _endpointRepository.AddEndpointAsync(endpoint);

        if (context.StorageMode == StorageMode.Team)
            await _signalRNotificationService.NotifyEndpointChangedAsync(saved.Id, saved.ApplicationId);

        var response = MapToResponse(saved);
        return CreatedAtAction(null, new { id = saved.Id }, response);
    }

    /// <summary>
    /// Updates an existing endpoint.
    /// </summary>
    /// <param name="id">The endpoint ID.</param>
    /// <param name="request">The updated endpoint data.</param>
    /// <response code="200">Updated endpoint.</response>
    /// <response code="400">Invalid request body.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    /// <response code="404">Endpoint not found.</response>
    [HttpPut("{id:int}")]
    [RequiresContextHeaders]
    [ProducesResponseType(typeof(EndpointResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateEndpointRequest request)
    {
        var context = await ParseRequestContextAsync();
        if (context == null)
            return Unauthorized();

        var existing = await _endpointRepository.GetEndpointByIdAsync(id);
        if (existing == null)
            return NotFound();

        var endpoint = new Endpoint
        {
            Id = id,
            Name = request.Name,
            RelativePath = request.RelativePath,
            ApplicationId = existing.ApplicationId,
            EndpointGroupId = request.EndpointGroupId,
            Method = request.Method,
            BodyMode = request.BodyMode,
            Body = request.Body,
            AuthenticationType = request.AuthenticationType,
            PreRequestScript = request.PreRequestScript,
            PostRequestScript = request.PostRequestScript,
            RowVersion = request.RowVersion
        };

        var saved = await _endpointRepository.UpdateEndpointAsync(endpoint);

        foreach (var h in existing.Headers)
            await _endpointRepository.DeleteHeaderAsync(h.Id);
        foreach (var h in request.Headers)
            await _endpointRepository.AddHeaderAsync(new EndpointHeader
            {
                Key = h.Key,
                Value = h.Value,
                EndpointId = id
            });

        foreach (var p in existing.QueryParameters)
            await _endpointRepository.DeleteQueryParameterAsync(p.Id);
        foreach (var p in request.QueryParameters)
            await _endpointRepository.AddQueryParameterAsync(new EndpointQueryParameter
            {
                Key = p.Key,
                Value = p.Value,
                EndpointId = id
            });

        var updated = await _endpointRepository.GetEndpointByIdAsync(id);

        if (context.StorageMode == StorageMode.Team)
            await _signalRNotificationService.NotifyEndpointChangedAsync(updated.Id, updated.ApplicationId);

        return Ok(MapToResponse(updated!));
    }

    /// <summary>
    /// Deletes an endpoint.
    /// </summary>
    /// <param name="id">The endpoint ID.</param>
    /// <response code="204">Endpoint deleted successfully.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    /// <response code="404">Endpoint not found.</response>
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

        var endpoint = await _endpointRepository.GetEndpointByIdAsync(id);
        if (endpoint == null)
            return NotFound();

        var applicationId = endpoint.ApplicationId;
        await _endpointRepository.DeleteEndpointAsync(id);

        if (context.StorageMode == StorageMode.Team)
            await _signalRNotificationService.NotifyEndpointChangedAsync(id, applicationId);

        return NoContent();
    }

    /// <summary>
    /// Adds a header to an endpoint.
    /// </summary>
    /// <param name="request">The header to add.</param>
    /// <response code="201">Header added successfully.</response>
    /// <response code="400">Invalid request body.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    [HttpPost("headers")]
    [RequiresContextHeaders]
    [ProducesResponseType(typeof(EndpointKeyValueResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddHeaderAsync([FromBody] AddEndpointKeyValueRequest request)
    {
        var context = await ParseRequestContextAsync();
        if (context == null)
            return Unauthorized();

        var header = new EndpointHeader
        {
            Key = request.Key,
            Value = request.Value,
            EndpointId = request.EndpointId
        };
        var saved = await _endpointRepository.AddHeaderAsync(header);

        return CreatedAtAction(null, new { id = saved.Id }, MapToResponse(saved));
    }

    /// <summary>
    /// Deletes a header from an endpoint.
    /// </summary>
    /// <param name="id">The header ID.</param>
    /// <response code="204">Header deleted successfully.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    /// <response code="404">Header not found.</response>
    [HttpDelete("headers/{id:int}")]
    [RequiresContextHeaders]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHeaderAsync(int id)
    {
        var context = await ParseRequestContextAsync();
        if (context == null)
            return Unauthorized();

        var header = await _endpointRepository.GetHeaderByIdAsync(id);
        if (header == null)
            return NotFound();

        await _endpointRepository.DeleteHeaderAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Adds a query parameter to an endpoint.
    /// </summary>
    /// <param name="request">The query parameter to add.</param>
    /// <response code="201">Query parameter added successfully.</response>
    /// <response code="400">Invalid request body.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    [HttpPost("query-parameters")]
    [RequiresContextHeaders]
    [ProducesResponseType(typeof(EndpointKeyValueResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddQueryParameterAsync([FromBody] AddEndpointKeyValueRequest request)
    {
        var context = await ParseRequestContextAsync();
        if (context == null)
            return Unauthorized();

        var parameter = new EndpointQueryParameter
        {
            Key = request.Key,
            Value = request.Value,
            EndpointId = request.EndpointId
        };
        var saved = await _endpointRepository.AddQueryParameterAsync(parameter);

        return CreatedAtAction(null, new { id = saved.Id }, MapToResponse(saved));
    }

    /// <summary>
    /// Deletes a query parameter from an endpoint.
    /// </summary>
    /// <param name="id">The query parameter ID.</param>
    /// <response code="204">Query parameter deleted successfully.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    /// <response code="404">Query parameter not found.</response>
    [HttpDelete("query-parameters/{id:int}")]
    [RequiresContextHeaders]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQueryParameterAsync(int id)
    {
        var context = await ParseRequestContextAsync();
        if (context == null)
            return Unauthorized();

        var parameter = await _endpointRepository.GetQueryParameterByIdAsync(id);
        if (parameter == null)
            return NotFound();

        await _endpointRepository.DeleteQueryParameterAsync(id);
        return NoContent();
    }
}
