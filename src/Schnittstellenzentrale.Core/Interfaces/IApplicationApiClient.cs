using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert API-Operationen für Anwendungen, Gruppen, Endpunkte und Umgebungen.</summary>
public interface IApplicationApiClient
{
    /// <summary>Ruft die Anwendungsgruppen ab.</summary>
    /// <param name="storageMode">Der Speichermodus.</param>
    /// <param name="owner">Der Eigentümer.</param>
    /// <returns>Die Liste der Anwendungsgruppen.</returns>
    Task<IList<ApplicationGroup>> GetGroupsAsync(StorageMode storageMode, string owner);

    /// <summary>Ruft eine Anwendungsgruppe anhand der ID ab.</summary>
    /// <param name="id">ID der Anwendungsgruppe.</param>
    /// <returns>Die gefundene Gruppe oder <see langword="null"/>.</returns>
    Task<ApplicationGroup?> GetGroupByIdAsync(int id);

    /// <summary>Erstellt eine neue Anwendungsgruppe.</summary>
    /// <param name="group">Die anzulegende Gruppe.</param>
    /// <returns>Die erstellte Gruppe.</returns>
    Task<ApplicationGroup> AddGroupAsync(ApplicationGroup group);

    /// <summary>Aktualisiert eine Anwendungsgruppe.</summary>
    /// <param name="group">Die aktualisierte Gruppe.</param>
    /// <returns>Die aktualisierte Gruppe.</returns>
    Task<ApplicationGroup> UpdateGroupAsync(ApplicationGroup group);

    /// <summary>Löscht eine Anwendungsgruppe.</summary>
    /// <param name="id">ID der zu löschenden Gruppe.</param>
    Task DeleteGroupAsync(int id);

    /// <summary>Ruft die nicht gruppierten Anwendungen ab.</summary>
    /// <param name="storageMode">Der Speichermodus.</param>
    /// <param name="owner">Der Eigentümer.</param>
    /// <returns>Die Liste der nicht gruppierten Anwendungen.</returns>
    Task<IList<Application>> GetUngroupedApplicationsAsync(StorageMode storageMode, string owner);

    /// <summary>Ruft eine Anwendung anhand der ID ab.</summary>
    /// <param name="id">ID der Anwendung.</param>
    /// <returns>Die gefundene Anwendung oder <see langword="null"/>.</returns>
    Task<Application?> GetApplicationByIdAsync(int id);

    /// <summary>Erstellt eine neue Anwendung.</summary>
    /// <param name="application">Die anzulegende Anwendung.</param>
    /// <returns>Die erstellte Anwendung.</returns>
    Task<Application> AddApplicationAsync(Application application);

    /// <summary>Aktualisiert eine Anwendung.</summary>
    /// <param name="application">Die aktualisierte Anwendung.</param>
    /// <returns>Die aktualisierte Anwendung.</returns>
    Task<Application> UpdateApplicationAsync(Application application);

    /// <summary>Löscht eine Anwendung.</summary>
    /// <param name="id">ID der zu löschenden Anwendung.</param>
    Task DeleteApplicationAsync(int id);

    /// <summary>Ruft die Endpunktgruppen einer Anwendung ab.</summary>
    /// <param name="applicationId">ID der Anwendung.</param>
    /// <returns>Die Liste der Endpunktgruppen.</returns>
    Task<IList<EndpointGroup>> GetEndpointGroupsAsync(int applicationId);

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

    /// <summary>Ruft die Endpunkte einer Anwendung ab.</summary>
    /// <param name="applicationId">ID der Anwendung.</param>
    /// <param name="endpointGroupId">Optionale ID der Endpunktgruppe.</param>
    /// <returns>Die Liste der Endpunkte.</returns>
    Task<IList<Endpoint>> GetEndpointsAsync(int applicationId, int? endpointGroupId = null);

    /// <summary>Ruft einen Endpunkt anhand der ID ab.</summary>
    /// <param name="id">ID des Endpunkts.</param>
    /// <returns>Der gefundene Endpunkt oder <see langword="null"/>.</returns>
    Task<Endpoint?> GetEndpointByIdAsync(int id);

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

    /// <summary>Fügt einem Endpunkt einen Header hinzu.</summary>
    /// <param name="header">Der hinzuzufügende Header.</param>
    /// <returns>Der erstellte Header.</returns>
    Task<EndpointHeader> AddHeaderAsync(EndpointHeader header);

    /// <summary>Löscht einen Endpunkt-Header.</summary>
    /// <param name="id">ID des zu löschenden Headers.</param>
    Task DeleteHeaderAsync(int id);

    /// <summary>Fügt einem Endpunkt einen Abfrageparameter hinzu.</summary>
    /// <param name="parameter">Der hinzuzufügende Parameter.</param>
    /// <returns>Der erstellte Parameter.</returns>
    Task<EndpointQueryParameter> AddQueryParameterAsync(EndpointQueryParameter parameter);

    /// <summary>Löscht einen Abfrageparameter.</summary>
    /// <param name="id">ID des zu löschenden Parameters.</param>
    Task DeleteQueryParameterAsync(int id);

    /// <summary>Ruft eine Systemumgebung anhand der ID ab.</summary>
    /// <param name="id">ID der Umgebung.</param>
    /// <returns>Die gefundene Umgebung oder <see langword="null"/>.</returns>
    Task<SystemEnvironment?> GetEnvironmentByIdAsync(int id);

    /// <summary>Importiert Metadaten für eine Anwendung.</summary>
    /// <param name="applicationId">ID der Anwendung.</param>
    /// <returns>Die Importdifferenz.</returns>
    Task<ImportDiff> ImportMetadataAsync(int applicationId);

    /// <summary>Wendet einen OData-Importunterschied an.</summary>
    /// <param name="applicationId">ID der Anwendung.</param>
    /// <param name="diff">Der anzuwendende Unterschied.</param>
    Task ApplyODataDiffAsync(int applicationId, ImportDiff diff);
}
