using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Bietet Zugriff auf Anwendungsverknüpfungen.</summary>
public interface IApplicationLinkRepository
{
    /// <summary>Ruft die Verknüpfungen einer Anwendung ab.</summary>
    /// <param name="applicationId">ID der Anwendung.</param>
    /// <returns>Die Liste der Verknüpfungen.</returns>
    Task<IList<ApplicationLink>> GetByApplicationIdAsync(int applicationId);

    /// <summary>Erstellt eine neue Anwendungsverknüpfung.</summary>
    /// <param name="link">Die anzulegende Verknüpfung.</param>
    /// <returns>Die erstellte Verknüpfung.</returns>
    Task<ApplicationLink> AddAsync(ApplicationLink link);

    /// <summary>Aktualisiert eine Anwendungsverknüpfung.</summary>
    /// <param name="link">Die aktualisierte Verknüpfung.</param>
    /// <returns>Die aktualisierte Verknüpfung.</returns>
    Task<ApplicationLink> UpdateAsync(ApplicationLink link);

    /// <summary>Löscht eine Anwendungsverknüpfung.</summary>
    /// <param name="linkId">ID der zu löschenden Verknüpfung.</param>
    Task DeleteAsync(int linkId);
}
