using Microsoft.OpenApi;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

/// <summary>Definiert Operationen zum Swagger-Import.</summary>
public interface ISwaggerImportService
{
    /// <summary>Importiert Swagger-Metadaten für eine Anwendung.</summary>
    /// <param name="application">Die zu importierende Anwendung.</param>
    /// <returns>Die ermittelte Importdifferenz.</returns>
    Task<ImportDiff> ImportAsync(Application application);

    /// <summary>Synchronisiert die Endpunkte einer Anwendung aus einem bereits vorliegenden OpenAPI-Dokument und wendet die Differenz an.</summary>
    /// <param name="document">Das OpenAPI-Dokument.</param>
    /// <param name="applicationId">Die ID der Zielanwendung.</param>
    /// <returns>Die ermittelte Importdifferenz; bei gesetzter <see cref="ImportDiff.ErrorMessage"/> wurde nichts angewendet.</returns>
    Task<ImportDiff> SyncFromDocumentAsync(OpenApiDocument document, int applicationId);

    /// <summary>Wendet eine zuvor ermittelte Importdifferenz an.</summary>
    /// <param name="diff">Die anzuwendende Importdifferenz.</param>
    Task ApplyDiffAsync(ImportDiff diff);
}
