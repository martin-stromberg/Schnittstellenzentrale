using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>In-Memory-Cache fuer das letzte Ausfuehrungsergebnis je Endpunkt im aktuellen UI-Scope.</summary>
public class EndpointExecutionResultCache : IEndpointExecutionResultCache
{
    private readonly Dictionary<int, EndpointExecutionResult> _resultsByEndpointId = [];

    /// <inheritdoc/>
    public EndpointExecutionResult? Get(int endpointId) =>
        _resultsByEndpointId.TryGetValue(endpointId, out var result) ? result : null;

    /// <inheritdoc/>
    public void Set(int endpointId, EndpointExecutionResult result) =>
        _resultsByEndpointId[endpointId] = result;
}
