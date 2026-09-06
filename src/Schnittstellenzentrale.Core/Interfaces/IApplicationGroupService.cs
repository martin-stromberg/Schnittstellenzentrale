namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert Operationen zum Verwalten von Anwendungsgruppen.</summary>
public interface IApplicationGroupService
{
    /// <summary>Aktualisiert den Namen einer Anwendungsgruppe.</summary>
    /// <param name="groupId">ID der Anwendungsgruppe.</param>
    /// <param name="name">Neuer Name.</param>
    Task UpdateNameAsync(int groupId, string name);

    /// <summary>Aktualisiert die Beschreibung einer Anwendungsgruppe.</summary>
    /// <param name="groupId">ID der Anwendungsgruppe.</param>
    /// <param name="description">Neue Beschreibung.</param>
    Task UpdateDescriptionAsync(int groupId, string? description);

    /// <summary>Aktualisiert den Untertitel einer Anwendungsgruppe.</summary>
    /// <param name="groupId">ID der Anwendungsgruppe.</param>
    /// <param name="subtitle">Neuer Untertitel.</param>
    Task UpdateSubtitleAsync(int groupId, string? subtitle);

    /// <summary>Aktualisiert das Icon einer Anwendungsgruppe.</summary>
    /// <param name="groupId">ID der Anwendungsgruppe.</param>
    /// <param name="iconData">Binäre Icon-Daten.</param>
    Task UpdateIconAsync(int groupId, byte[] iconData);
}
