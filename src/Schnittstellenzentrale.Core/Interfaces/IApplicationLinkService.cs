using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert Operationen für Anwendungsverknüpfungen.</summary>
public interface IApplicationLinkService
{
    /// <summary>Ruft die Verknüpfungen einer Anwendung ab.</summary>
    /// <param name="applicationId">ID der Anwendung.</param>
    /// <returns>Die Liste der Verknüpfungen.</returns>
    Task<IList<ApplicationLink>> GetLinksAsync(int applicationId);

    /// <summary>Erstellt eine neue Anwendungsverknüpfung.</summary>
    /// <param name="link">Die anzulegende Verknüpfung.</param>
    /// <returns>Die erstellte Verknüpfung.</returns>
    Task<ApplicationLink> AddLinkAsync(ApplicationLink link);

    /// <summary>Aktualisiert eine Anwendungsverknüpfung.</summary>
    /// <param name="link">Die aktualisierte Verknüpfung.</param>
    /// <returns>Die aktualisierte Verknüpfung.</returns>
    Task<ApplicationLink> UpdateLinkAsync(ApplicationLink link);

    /// <summary>Löscht eine Anwendungsverknüpfung.</summary>
    /// <param name="linkId">ID der zu löschenden Verknüpfung.</param>
    Task DeleteLinkAsync(int linkId);
}
