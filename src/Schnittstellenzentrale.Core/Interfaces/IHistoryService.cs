using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert Operationen für den Aufrufverlauf.</summary>
public interface IHistoryService
{
    /// <summary>Fügt einen Eintrag zum Aufrufverlauf hinzu.</summary>
    /// <param name="entry">Der hinzuzufügende Verlaufseintrag.</param>
    Task AddEntryAsync(EndpointCallHistoryEntry entry);

    /// <summary>Ruft eine paginierte Liste von Verlaufseinträgen ab.</summary>
    /// <param name="filter">Filter für die Abfrage.</param>
    /// <param name="page">Seitennummer.</param>
    /// <param name="pageSize">Seitengröße.</param>
    /// <returns>Tupel aus den Einträgen der Seite und der Gesamtanzahl.</returns>
    Task<(IList<EndpointCallHistoryEntry>, int)> GetPagedAsync(HistoryFilter filter, int page, int pageSize);

    /// <summary>Ruft die am häufigsten aufgerufenen Endpunkte ab.</summary>
    /// <param name="applicationId">ID der Anwendung.</param>
    /// <param name="count">Anzahl der zurückzugebenden Endpunkte.</param>
    /// <returns>Die Liste der am häufigsten aufgerufenen Endpunkte.</returns>
    Task<IList<TopEndpointResult>> GetTopEndpointsAsync(int applicationId, int count);
}

/// <summary>Filter für die Abfrage von Aufrufverlaufsdaten.</summary>
/// <param name="ApplicationId">Optionale Anwendungs-ID.</param>
/// <param name="EndpointId">Optionale Endpunkt-ID.</param>
/// <param name="From">Optionales Startdatum.</param>
/// <param name="To">Optionales Enddatum.</param>
/// <returns>Neuer Filterdatensatz.</returns>
public record HistoryFilter(int? ApplicationId, int? EndpointId, DateTime? From, DateTime? To);

/// <summary>Repräsentiert einen am häufigsten aufgerufenen Endpunkt.</summary>
/// <param name="EndpointId">ID des Endpunkts.</param>
/// <param name="RelativePath">Relativer Pfad des Endpunkts.</param>
/// <param name="HttpMethod">Verwendete HTTP-Methode.</param>
/// <param name="CallCount">Anzahl der Aufrufe.</param>
/// <returns>Neuer Ergebnisdatensatz.</returns>
public record TopEndpointResult(int EndpointId, string? RelativePath, string? HttpMethod, int CallCount);
