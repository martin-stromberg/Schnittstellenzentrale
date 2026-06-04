namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>
/// Variante von <see cref="PlaywrightServer"/>, die vor dem App-Start eine temporäre
/// <c>impressum.md</c>-Datei im <see cref="AppContext.BaseDirectory"/> anlegt,
/// damit <c>IImpressumService.IsAvailable()</c> während der Tests <c>true</c> zurückgibt.
/// </summary>
public class PlaywrightImpressumServer : PlaywrightServer
{
    /// <inheritdoc/>
    protected override string BindUrl => "http://127.0.0.1:5101";

    private string? _impressumFilePath;

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        _impressumFilePath = Path.Combine(AppContext.BaseDirectory, "impressum.md");
        await File.WriteAllTextAsync(_impressumFilePath,
            "# Impressum\n\nDies ist ein Test-Impressum.");

        try
        {
            await base.InitializeAsync();
        }
        catch
        {
            if (File.Exists(_impressumFilePath))
                File.Delete(_impressumFilePath);
            throw;
        }
    }

    /// <inheritdoc/>
    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();

        if (_impressumFilePath != null && File.Exists(_impressumFilePath))
            File.Delete(_impressumFilePath);
    }
}
