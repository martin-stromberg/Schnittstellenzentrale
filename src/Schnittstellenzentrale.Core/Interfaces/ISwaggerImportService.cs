using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert Operationen zum Swagger-Import.</summary>
public interface ISwaggerImportService
{
    /// <summary>Importiert Swagger-Metadaten für eine Anwendung.</summary>
    /// <param name="application">Die zu importierende Anwendung.</param>
    /// <returns>Die ermittelte Importdifferenz.</returns>
    Task<ImportDiff> ImportAsync(Application application);

    /// <summary>Wendet eine zuvor ermittelte Importdifferenz an.</summary>
    /// <param name="diff">Die anzuwendende Importdifferenz.</param>
    Task ApplyDiffAsync(ImportDiff diff);
}
