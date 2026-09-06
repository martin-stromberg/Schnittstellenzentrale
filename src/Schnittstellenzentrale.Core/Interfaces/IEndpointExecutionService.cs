using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert Operationen zur Ausführung von Endpunkten.</summary>
public interface IEndpointExecutionService
{
    /// <summary>Führt den angegebenen Endpunkt asynchron aus.</summary>
    /// <param name="endpoint">Der auszuführende Endpunkt.</param>
    /// <returns>Das Ausführungsergebnis.</returns>
    Task<EndpointExecutionResult> ExecuteAsync(Endpoint endpoint);
}
