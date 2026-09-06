using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert endpunktbezogene Abfrage- und Änderungsoperationen.</summary>
public interface IEndpointRepository
{
    /// <summary>Ruft die Endpunkte einer Anwendung ab.</summary>
    /// <param name="applicationId">ID der Anwendung.</param>
    /// <returns>Die Liste der Endpunkte.</returns>
    Task<IList<Endpoint>> GetEndpointsAsync(int applicationId);

    /// <summary>Ruft alle Endpunkte ab.</summary>
    /// <returns>Die Liste aller Endpunkte.</returns>
    Task<IList<Endpoint>> GetAllEndpointsAsync();

    /// <summary>Ruft die Endpunkte für die angegebenen Anwendungs-IDs ab.</summary>
    /// <param name="applicationIds">IDs der Anwendungen.</param>
    /// <returns>Die Liste der Endpunkte.</returns>
    Task<IList<Endpoint>> GetEndpointsByApplicationIdsAsync(IEnumerable<int> applicationIds);

    /// <summary>Ruft die Endpunkte einer Endpunktgruppe ab.</summary>
    /// <param name="endpointGroupId">ID der Endpunktgruppe.</param>
    /// <returns>Die Liste der Endpunkte.</returns>
    Task<IList<Endpoint>> GetByGroupIdAsync(int endpointGroupId);

    /// <summary>Ruft einen Endpunkt anhand der ID ab.</summary>
    /// <param name="id">ID des Endpunkts.</param>
    /// <returns>Der gefundene Endpunkt oder <see langword="null"/>.</returns>
    Task<Endpoint?> GetEndpointByIdAsync(int id);

    /// <summary>Ruft Endpunkte anhand des Namens ab.</summary>
    /// <param name="applicationId">ID der Anwendung.</param>
    /// <param name="name">Name des Endpunkts.</param>
    /// <returns>Die Liste der gefundenen Endpunkte.</returns>
    Task<IList<Endpoint>> GetEndpointByNameAsync(int applicationId, string name);

    /// <summary>Erstellt einen neuen Endpunkt.</summary>
    /// <param name="endpoint">Der anzulegende Endpunkt.</param>
    /// <returns>Der erstellte Endpunkt.</returns>
    Task<Endpoint> AddEndpointAsync(Endpoint endpoint);

    /// <summary>Aktualisiert einen Endpunkt.</summary>
    /// <param name="endpoint">Der aktualisierte Endpunkt.</param>
    /// <returns>Der aktualisierte Endpunkt.</returns>
    Task<Endpoint> UpdateEndpointAsync(Endpoint endpoint);

    /// <summary>Löscht einen Endpunkt.</summary>
    /// <param name="id">ID des zu löschenden Endpunkts.</param>
    Task DeleteEndpointAsync(int id);

    /// <summary>Ruft die Endpunktgruppen einer Anwendung ab.</summary>
    /// <param name="applicationId">ID der Anwendung.</param>
    /// <returns>Die Liste der Endpunktgruppen.</returns>
    Task<IList<EndpointGroup>> GetEndpointGroupsAsync(int applicationId);

    /// <summary>Ruft alle Endpunktgruppen ab.</summary>
    /// <returns>Die Liste aller Endpunktgruppen.</returns>
    Task<IList<EndpointGroup>> GetAllEndpointGroupsAsync();

    /// <summary>Ruft eine Endpunktgruppe anhand der ID ab.</summary>
    /// <param name="id">ID der Endpunktgruppe.</param>
    /// <returns>Die gefundene Gruppe oder <see langword="null"/>.</returns>
    Task<EndpointGroup?> GetEndpointGroupByIdAsync(int id);

    /// <summary>Erstellt eine neue Endpunktgruppe.</summary>
    /// <param name="group">Die anzulegende Gruppe.</param>
    /// <returns>Die erstellte Gruppe.</returns>
    Task<EndpointGroup> AddEndpointGroupAsync(EndpointGroup group);

    /// <summary>Aktualisiert eine Endpunktgruppe.</summary>
    /// <param name="group">Die aktualisierte Gruppe.</param>
    /// <returns>Die aktualisierte Gruppe.</returns>
    Task<EndpointGroup> UpdateEndpointGroupAsync(EndpointGroup group);

    /// <summary>Löscht eine Endpunktgruppe.</summary>
    /// <param name="id">ID der zu löschenden Gruppe.</param>
    Task DeleteEndpointGroupAsync(int id);

    /// <summary>Ruft einen Header anhand der ID ab.</summary>
    /// <param name="id">ID des Headers.</param>
    /// <returns>Der gefundene Header oder <see langword="null"/>.</returns>
    Task<EndpointHeader?> GetHeaderByIdAsync(int id);

    /// <summary>Erstellt einen neuen Endpunkt-Header.</summary>
    /// <param name="header">Der anzulegende Header.</param>
    /// <returns>Der erstellte Header.</returns>
    Task<EndpointHeader> AddHeaderAsync(EndpointHeader header);

    /// <summary>Löscht einen Endpunkt-Header.</summary>
    /// <param name="id">ID des zu löschenden Headers.</param>
    Task DeleteHeaderAsync(int id);

    /// <summary>Ruft einen Abfrageparameter anhand der ID ab.</summary>
    /// <param name="id">ID des Parameters.</param>
    /// <returns>Der gefundene Parameter oder <see langword="null"/>.</returns>
    Task<EndpointQueryParameter?> GetQueryParameterByIdAsync(int id);

    /// <summary>Erstellt einen neuen Abfrageparameter.</summary>
    /// <param name="parameter">Der anzulegende Parameter.</param>
    /// <returns>Der erstellte Parameter.</returns>
    Task<EndpointQueryParameter> AddQueryParameterAsync(EndpointQueryParameter parameter);

    /// <summary>Löscht einen Abfrageparameter.</summary>
    /// <param name="id">ID des zu löschenden Parameters.</param>
    Task DeleteQueryParameterAsync(int id);
}
