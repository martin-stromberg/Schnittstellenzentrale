using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert Operationen zur Integritätsprüfung.</summary>
public interface IHealthCheckService
{
    /// <summary>Führt eine Integritätsprüfung für eine Anwendung aus.</summary>
    /// <param name="application">Die zu prüfende Anwendung.</param>
    /// <returns>Das Ergebnis der Prüfung oder <see langword="null"/>.</returns>
    Task<bool?> CheckAsync(Application application);
}
