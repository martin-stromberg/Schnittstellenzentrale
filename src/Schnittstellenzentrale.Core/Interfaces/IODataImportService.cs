using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface IODataImportService
{
    Task<ImportDiff> ImportAsync(Application application);
    Task ApplyDiffAsync(ImportDiff diff);
}
