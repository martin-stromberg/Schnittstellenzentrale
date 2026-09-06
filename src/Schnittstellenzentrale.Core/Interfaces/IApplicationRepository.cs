using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert anwendungs- und gruppenbezogene Abfrage- und Änderungsoperationen.</summary>
public interface IApplicationRepository
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

    /// <summary>Ruft die Systemgruppe ab.</summary>
    /// <returns>Die Systemgruppe oder <see langword="null"/>.</returns>
    Task<ApplicationGroup?> GetSystemGroupAsync();

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

    /// <summary>Ruft die Anwendungen ab.</summary>
    /// <param name="storageMode">Der Speichermodus.</param>
    /// <param name="owner">Der Eigentümer.</param>
    /// <returns>Die Liste der Anwendungen.</returns>
    Task<IList<Application>> GetApplicationsAsync(StorageMode storageMode, string owner);

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

    /// <summary>Ruft die Anzahl der Anwendungen in einer Gruppe ab.</summary>
    /// <param name="groupId">ID der Gruppe.</param>
    /// <returns>Die Anzahl der Anwendungen.</returns>
    Task<int> GetApplicationCountByGroupAsync(int groupId);

    /// <summary>Ruft die Anzahl der Endpunkte in einer Gruppe ab.</summary>
    /// <param name="groupId">ID der Gruppe.</param>
    /// <returns>Die Anzahl der Endpunkte.</returns>
    Task<int> GetEndpointCountByGroupAsync(int groupId);
}
