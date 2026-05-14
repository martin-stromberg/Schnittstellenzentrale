using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface IEndpointExecutionService
{
    Task<EndpointExecutionResult> ExecuteAsync(Endpoint endpoint);
}
