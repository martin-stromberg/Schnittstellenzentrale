using Microsoft.AspNetCore.Mvc;
using Schnittstellenzentrale.Core.Contracts;
using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Controllers;

/// <summary>Provides read access to system environments.</summary>
[Route("api/system-environments")]
public class SystemEnvironmentsController : ApiControllerBase
{
    private readonly ISystemEnvironmentRepository _repository;

    /// <summary>Initialisiert eine neue Instanz von <see cref="SystemEnvironmentsController"/>.</summary>
    public SystemEnvironmentsController(ITokenStore tokenStore, ISystemEnvironmentRepository repository)
        : base(tokenStore)
    {
        _repository = repository;
    }

    /// <summary>Returns a single system environment by ID including its variables.</summary>
    /// <param name="id">The system environment ID.</param>
    /// <response code="200">The requested system environment.</response>
    /// <response code="401">Missing or invalid bearer token.</response>
    /// <response code="404">System environment not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SystemEnvironmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id)
    {
        var newToken = await ValidateTokenAndSetResponseHeaderAsync();
        if (newToken == null)
            return Unauthorized();

        var environment = await _repository.GetByIdAsync(id);
        if (environment == null)
            return NotFound();

        return Ok(MapToResponse(environment));
    }

    private static SystemEnvironmentResponse MapToResponse(SystemEnvironment env) => new()
    {
        Id = env.Id,
        Name = env.Name,
        Mode = (int)env.Mode,
        Owner = env.Owner,
        Description = env.Description,
        Variables = env.Variables.Select(v => new EnvironmentVariableResponse
        {
            Id = v.Id,
            Name = v.Name,
            Value = v.Value,
            IsValueMasked = v.IsValueMasked
        }).ToList()
    };
}
