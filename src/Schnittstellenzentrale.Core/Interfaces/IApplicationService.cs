using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert anwendungsspezifische Verwaltungsoperationen.</summary>
public interface IApplicationService
{
    /// <summary>Aktualisiert den Namen einer Anwendung.</summary>
    /// <param name="applicationId">ID der Anwendung.</param>
    /// <param name="name">Neuer Name.</param>
    Task UpdateNameAsync(int applicationId, string name);

    /// <summary>Aktualisiert den Untertitel einer Anwendung.</summary>
    /// <param name="applicationId">ID der Anwendung.</param>
    /// <param name="subtitle">Neuer Untertitel.</param>
    Task UpdateSubtitleAsync(int applicationId, string? subtitle);

    /// <summary>Aktualisiert das Icon einer Anwendung.</summary>
    /// <param name="applicationId">ID der Anwendung.</param>
    /// <param name="iconData">Binäre Icon-Daten.</param>
    Task UpdateIconAsync(int applicationId, byte[] iconData);
}
