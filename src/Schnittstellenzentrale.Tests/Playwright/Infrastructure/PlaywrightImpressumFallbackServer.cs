using Microsoft.Extensions.DependencyInjection;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>
/// Variante von <see cref="PlaywrightServer"/>, die eine <c>impressum.md</c>-Datei ohne
/// sprachspezifische Variante anlegt. Damit fällt die Spracherkennung in <see cref="ImpressumService"/>
/// auf die Fallback-Datei zurück, unabhängig vom konfigurierten Thread-Locale.
/// </summary>
public class PlaywrightImpressumFallbackServer : PlaywrightServer
{
    /// <inheritdoc/>
    protected override string BindUrl => "http://127.0.0.1:5104";

    private string? _tempDir;
    private string? _impressumFilePath;

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"sz-impressum-fb-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _impressumFilePath = Path.Combine(_tempDir, "impressum.md");
        await File.WriteAllTextAsync(_impressumFilePath,
            "# Impressum\n\nDies ist das Fallback-Impressum.");

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
        var filePath = _impressumFilePath!;
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
