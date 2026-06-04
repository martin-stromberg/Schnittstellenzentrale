namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>
/// Variante von <see cref="PlaywrightServer"/>, die vor dem App-Start sowohl eine
/// <c>impressum.md</c>-Fallback-Datei als auch eine sprachspezifische <c>impressum.de.md</c>-Datei
/// im <see cref="AppContext.BaseDirectory"/> anlegt.
/// </summary>
public class PlaywrightImpressumLanguageServer : PlaywrightServer
{
    /// <inheritdoc/>
    protected override string BindUrl => "http://127.0.0.1:5102";

    private string? _fallbackFilePath;
    private string? _germanFilePath;

    /// <inheritdoc/>
    public override async Task InitializeAsync()
    {
        _fallbackFilePath = Path.Combine(AppContext.BaseDirectory, "impressum.md");
        _germanFilePath = Path.Combine(AppContext.BaseDirectory, "impressum.de.md");

        await File.WriteAllTextAsync(_fallbackFilePath,
            "# Impressum\n\nDies ist das Fallback-Impressum.");
        await File.WriteAllTextAsync(_germanFilePath,
            "# Impressum\n\nDies ist das deutsche Impressum.");

        try
        {
            await base.InitializeAsync();
        }
        catch
        {
            if (File.Exists(_fallbackFilePath))
                File.Delete(_fallbackFilePath);
            if (File.Exists(_germanFilePath))
                File.Delete(_germanFilePath);
            throw;
        }
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
            if (_fallbackFilePath != null && File.Exists(_fallbackFilePath))
                File.Delete(_fallbackFilePath);
            if (_germanFilePath != null && File.Exists(_germanFilePath))
                File.Delete(_germanFilePath);
        }
    }
}
