using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Speichert das letzte Ausfuehrungsergebnis je Endpunkt fuer den aktuellen UI-Scope.</summary>
public interface IEndpointExecutionResultCache
{
    /// <summary>Gibt das gespeicherte Ergebnis fuer den Endpunkt zurueck, falls eines vorhanden ist.</summary>
    /// <param name="endpointId">Id des Endpunkts.</param>
    /// <returns>Das letzte Ergebnis oder <see langword="null"/>.</returns>
    EndpointExecutionResult? Get(int endpointId);

    /// <summary>Speichert oder ersetzt das Ergebnis fuer den Endpunkt.</summary>
    /// <param name="endpointId">Id des Endpunkts.</param>
    /// <param name="result">Zu speicherndes Ausfuehrungsergebnis.</param>
    void Set(int endpointId, EndpointExecutionResult result);
}
