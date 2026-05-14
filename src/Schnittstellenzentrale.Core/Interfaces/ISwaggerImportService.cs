using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface ISwaggerImportService
{
    Task<ImportDiff> ImportAsync(Application application);
    Task ApplyDiffAsync(ImportDiff diff);
}
