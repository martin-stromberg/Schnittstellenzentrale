using Markdig;
using Microsoft.Extensions.Options;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Implementierung von <see cref="IImpressumService"/>: Pfadauflösung, Dateiverfügbarkeitsprüfung und Markdown-Rendering.</summary>
public class ImpressumService : IImpressumService
{
    private readonly string _resolvedPath;

    /// <summary>Initialisiert den Service und löst den konfigurierten Dateipfad auf.</summary>
    public ImpressumService(IOptions<ImpressumSettings> options)
    {
        var filePath = options.Value.FilePath;

        if (string.IsNullOrEmpty(filePath))
            _resolvedPath = Path.Combine(AppContext.BaseDirectory, "impressum.md");
        else if (Path.IsPathRooted(filePath))
            _resolvedPath = filePath;
        else
            _resolvedPath = Path.GetFullPath(filePath, AppContext.BaseDirectory);
    }

    /// <inheritdoc/>
    public bool IsAvailable() => File.Exists(_resolvedPath);

    /// <inheritdoc/>
    public async Task<string> GetContentAsHtmlAsync()
    {
        var markdown = await File.ReadAllTextAsync(_resolvedPath);
        return Markdown.ToHtml(markdown);
    }
}
