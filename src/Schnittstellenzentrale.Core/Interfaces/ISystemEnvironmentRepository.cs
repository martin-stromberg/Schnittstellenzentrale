using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert Operationen zum Verwalten von Systemumgebungen.</summary>
public interface ISystemEnvironmentRepository
{
    /// <summary>Ruft die Systemumgebungen ab.</summary>
    /// <param name="mode">Der Speichermodus.</param>
    /// <param name="owner">Der Eigentümer.</param>
    /// <returns>Die Liste der Umgebungen.</returns>
    Task<IList<SystemEnvironment>> GetEnvironmentsAsync(StorageMode mode, string? owner);

    /// <summary>Ruft eine Systemumgebung anhand der ID ab.</summary>
    /// <param name="id">ID der Umgebung.</param>
    /// <returns>Die gefundene Umgebung oder <see langword="null"/>.</returns>
    Task<SystemEnvironment?> GetByIdAsync(int id);

    /// <summary>Erstellt eine neue Systemumgebung.</summary>
    /// <param name="systemEnvironment">Die anzulegende Umgebung.</param>
    /// <returns>Die erstellte Umgebung.</returns>
    Task<SystemEnvironment> AddAsync(SystemEnvironment systemEnvironment);

    /// <summary>Aktualisiert eine Systemumgebung.</summary>
    /// <param name="systemEnvironment">Die aktualisierte Umgebung.</param>
    /// <returns>Die aktualisierte Umgebung.</returns>
    Task<SystemEnvironment> UpdateAsync(SystemEnvironment systemEnvironment);

    /// <summary>Löscht eine Systemumgebung.</summary>
    /// <param name="id">ID der zu löschenden Umgebung.</param>
    Task DeleteAsync(int id);

    /// <summary>Aktualisiert eine Umgebungsvariable.</summary>
    /// <param name="environmentId">ID der Umgebung.</param>
    /// <param name="name">Name der Variable.</param>
    /// <param name="value">Neuer Wert der Variable.</param>
    Task UpdateVariableAsync(int environmentId, string name, string value);
}
