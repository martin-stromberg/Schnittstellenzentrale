using System.Globalization;
using Markdig;
using Microsoft.Extensions.Options;
using Schnittstellenzentrale.Core.Interfaces;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Implementierung von <see cref="IImpressumService"/>: Pfadauflösung, Dateiverfügbarkeitsprüfung und Markdown-Rendering.</summary>
public class ImpressumService : IImpressumService
{
    private readonly string _resolvedPath;
    private readonly string _baseDir;
    private readonly string _baseName;

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

        _baseDir = Path.GetDirectoryName(_resolvedPath) ?? AppContext.BaseDirectory;
        _baseName = Path.GetFileNameWithoutExtension(_resolvedPath);
    }

    /// <inheritdoc/>
    public bool IsAvailable()
    {
        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        return File.Exists(ResolvePath(language));
    }

    /// <inheritdoc/>
    public async Task<string> GetContentAsHtmlAsync()
    {
        var language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        var markdown = await File.ReadAllTextAsync(ResolvePath(language));
        return Markdown.ToHtml(markdown);
    }

    private string ResolvePath(string language)
    {
        if (!string.IsNullOrEmpty(language) && !CultureInfo.CurrentUICulture.Equals(CultureInfo.InvariantCulture))
        {
            var candidate = Path.Combine(_baseDir, $"{_baseName}.{language}.md");
            if (File.Exists(candidate))
                return candidate;
        }
        return _resolvedPath;
    }
}
