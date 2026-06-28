using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>
/// Variante von <see cref="PlaywrightServer"/>, die eine sprachspezifische
/// <c>impressum.de.md</c>-Datei in einem eindeutigen temporären Verzeichnis anlegt
/// und <see cref="ImpressumSettings.FilePath"/> auf die Fallback-Basisdatei zeigt.
/// Dadurch wird im Test verifiziert, dass bei aktivem Deutsch-Locale die
/// sprachspezifische Datei bevorzugt wird.
/// </summary>
public class PlaywrightImpressumLanguageServer : PlaywrightServer
{
    /// <inheritdoc/>
    protected override string BindUrl => "http://127.0.0.1:5102";

    private string? _tempDir;
    private string? _fallbackFilePath;

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"sz-impressum-lang-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _fallbackFilePath = Path.Combine(_tempDir, "impressum.md");
        var germanFilePath = Path.Combine(_tempDir, "impressum.de.md");

        await File.WriteAllTextAsync(_fallbackFilePath,
            "# Impressum\n\nDies ist das Fallback-Impressum.");
        await File.WriteAllTextAsync(germanFilePath,
            "# Impressum\n\nDies ist ein deutsches Impressum.");

        try
        {
            await base.InitializeAsync();
        }
        catch
        {
            Cleanup();
            throw;
        }
    }

    /// <inheritdoc/>
    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
        var filePath = _fallbackFilePath!;
        services.PostConfigure<ImpressumSettings>(o => o.FilePath = filePath);
    }

    /// <inheritdoc/>
    public override async Task DisposeAsync()
    {
        try
        {
            await base.DisposeAsync();
        }
        finally
        {
            Cleanup();
        }
    }

    private void Cleanup()
    {
        if (_tempDir != null && Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }
}
