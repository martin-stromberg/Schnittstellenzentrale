using System.Globalization;
using Microsoft.Extensions.Options;
using Schnittstellenzentrale.Infrastructure.Services;

namespace Schnittstellenzentrale.Tests.Services;

/// <summary>Unit-Tests für <see cref="ImpressumService"/>.</summary>
public class ImpressumServiceTests : IDisposable
{
    private readonly string _tempDir;

    /// <summary>Legt ein temporäres Verzeichnis für Testdateien an.</summary>
    public ImpressumServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private static ImpressumService CreateService(string filePath)
    {
        var options = Options.Create(new ImpressumSettings { FilePath = filePath });
        return new ImpressumService(options);
    }

    // --- IsAvailable ---

    /// <summary>IsAvailable_DateiVorhanden_GibtTrueZurueck</summary>
    [Fact]
    public void IsAvailable_DateiVorhanden_GibtTrueZurueck()
    {
        var path = Path.Combine(_tempDir, "impressum.md");
        File.WriteAllText(path, "# Impressum");

        var service = CreateService(path);

        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        try
        {
            Assert.True(service.IsAvailable());
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    /// <summary>IsAvailable_DateiFehlt_GibtFalseZurueck</summary>
    [Fact]
    public void IsAvailable_DateiFehlt_GibtFalseZurueck()
    {
        var path = Path.Combine(_tempDir, "nicht_vorhanden.md");

        var service = CreateService(path);

        Assert.False(service.IsAvailable());
    }

    // --- GetContentAsHtmlAsync ---

    /// <summary>GetContentAsHtmlAsync_MarkdownWirdKorrektGerendert</summary>
    [Fact]
    public async Task GetContentAsHtmlAsync_MarkdownWirdKorrektGerendert()
    {
        var path = Path.Combine(_tempDir, "impressum.md");
        File.WriteAllText(path, "# Titel");

        var service = CreateService(path);

        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        try
        {
            var html = await service.GetContentAsHtmlAsync();
            Assert.Contains("<h1>Titel</h1>", html);
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    /// <summary>GetContentAsHtmlAsync_DateiFehlt_WirftException</summary>
    [Fact]
    public async Task GetContentAsHtmlAsync_DateiFehlt_WirftException()
    {
        var path = Path.Combine(_tempDir, "nicht_vorhanden.md");

        var service = CreateService(path);

        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        try
        {
            await Assert.ThrowsAsync<FileNotFoundException>(
                () => service.GetContentAsHtmlAsync());
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    // --- Pfadauflösung ---

    /// <summary>Pfadaufloesung_LeerFilePath_VerwendetBaseDirectory</summary>
    [Fact]
    public void Pfadaufloesung_LeerFilePath_VerwendetBaseDirectory()
    {
        var service = CreateService(string.Empty);
        var expectedPath = Path.Combine(AppContext.BaseDirectory, "impressum.md");

        // Datei existiert nicht — IsAvailable() gibt false zurück, aber der aufgelöste
        // Pfad kann über das Verhalten von IsAvailable() indirekt geprüft werden:
        // Legen wir die Datei am erwarteten Ort an, muss IsAvailable() true liefern.
        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        try
        {
            File.WriteAllText(expectedPath, "test");
            Assert.True(service.IsAvailable());
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
            if (File.Exists(expectedPath))
                File.Delete(expectedPath);
        }
    }

    /// <summary>Pfadaufloesung_RelativerFilePath_WirdRelativZuBaseDirectoryAufgeloest</summary>
    [Fact]
    public void Pfadaufloesung_RelativerFilePath_WirdRelativZuBaseDirectoryAufgeloest()
    {
        const string relativePath = "mein_impressum.md";
        var expectedPath = Path.GetFullPath(relativePath, AppContext.BaseDirectory);

        var service = CreateService(relativePath);

        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        try
        {
            File.WriteAllText(expectedPath, "test");
            Assert.True(service.IsAvailable());
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
            if (File.Exists(expectedPath))
                File.Delete(expectedPath);
        }
    }

    /// <summary>Pfadaufloesung_AbsoluterFilePath_WirdDirektVerwendet</summary>
    [Fact]
    public void Pfadaufloesung_AbsoluterFilePath_WirdDirektVerwendet()
    {
        var absolutePath = Path.Combine(_tempDir, "absolut.md");
        File.WriteAllText(absolutePath, "# Absolut");

        var service = CreateService(absolutePath);

        Assert.True(service.IsAvailable());
    }

    // --- Sprachspezifische Dateiauflösung ---

    /// <summary>IsAvailable_SprachspezifischeDateiVorhanden_GibtTrueZurueck</summary>
    [Fact]
    public void IsAvailable_SprachspezifischeDateiVorhanden_GibtTrueZurueck()
    {
        var fallbackPath = Path.Combine(_tempDir, "impressum.md");
        var languagePath = Path.Combine(_tempDir, "impressum.de.md");
        File.WriteAllText(fallbackPath, "# Fallback");
        File.WriteAllText(languagePath, "# Deutsch");

        var service = CreateService(fallbackPath);

        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("de");
        try
        {
            Assert.True(service.IsAvailable());
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    /// <summary>IsAvailable_SprachspezifischeDateiFehlt_FallbackVorhanden_GibtTrueZurueck</summary>
    [Fact]
    public void IsAvailable_SprachspezifischeDateiFehlt_FallbackVorhanden_GibtTrueZurueck()
    {
        var fallbackPath = Path.Combine(_tempDir, "impressum.md");
        File.WriteAllText(fallbackPath, "# Fallback");

        var service = CreateService(fallbackPath);

        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("fr");
        try
        {
            Assert.True(service.IsAvailable());
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    /// <summary>IsAvailable_BeideVariantenFehlen_GibtFalseZurueck</summary>
    [Fact]
    public void IsAvailable_BeideVariantenFehlen_GibtFalseZurueck()
    {
        var path = Path.Combine(_tempDir, "impressum.md");

        var service = CreateService(path);

        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("de");
        try
        {
            Assert.False(service.IsAvailable());
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    /// <summary>IsAvailable_NeutralesSprachkuerzel_VerwendetFallback</summary>
    [Fact]
    public void IsAvailable_NeutralesSprachkuerzel_VerwendetFallback()
    {
        var fallbackPath = Path.Combine(_tempDir, "impressum.md");
        File.WriteAllText(fallbackPath, "# Fallback");

        var service = CreateService(fallbackPath);

        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.InvariantCulture;
        try
        {
            Assert.True(service.IsAvailable());
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    /// <summary>GetContentAsHtmlAsync_SprachspezifischeDateiVorhanden_LiestSprachspezifischeDatei</summary>
    [Fact]
    public async Task GetContentAsHtmlAsync_SprachspezifischeDateiVorhanden_LiestSprachspezifischeDatei()
    {
        var fallbackPath = Path.Combine(_tempDir, "impressum.md");
        var languagePath = Path.Combine(_tempDir, "impressum.de.md");
        File.WriteAllText(fallbackPath, "# Fallback-Inhalt");
        File.WriteAllText(languagePath, "# Deutscher Inhalt");

        var service = CreateService(fallbackPath);

        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("de");
        try
        {
            var html = await service.GetContentAsHtmlAsync();
            Assert.Contains("Deutscher Inhalt", html);
            Assert.DoesNotContain("Fallback-Inhalt", html);
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    /// <summary>GetContentAsHtmlAsync_SprachspezifischeDateiFehlt_LiestFallbackDatei</summary>
    [Fact]
    public async Task GetContentAsHtmlAsync_SprachspezifischeDateiFehlt_LiestFallbackDatei()
    {
        var fallbackPath = Path.Combine(_tempDir, "impressum.md");
        File.WriteAllText(fallbackPath, "# Fallback-Inhalt");

        var service = CreateService(fallbackPath);

        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("fr");
        try
        {
            var html = await service.GetContentAsHtmlAsync();
            Assert.Contains("Fallback-Inhalt", html);
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    /// <summary>ResolvePath_SprachspezifischeDateiVorhanden_GibtSprachspezifischenPfadZurueck</summary>
    [Fact]
    public async Task ResolvePath_SprachspezifischeDateiVorhanden_GibtSprachspezifischenPfadZurueck()
    {
        var fallbackPath = Path.Combine(_tempDir, "impressum.md");
        var languagePath = Path.Combine(_tempDir, "impressum.de.md");
        File.WriteAllText(fallbackPath, "# Fallback");
        File.WriteAllText(languagePath, "# Deutsch");

        var service = CreateService(fallbackPath);

        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("de");
        try
        {
            Assert.True(service.IsAvailable());
            var html = await service.GetContentAsHtmlAsync();
            Assert.Contains("Deutsch", html);
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    /// <summary>ResolvePath_SprachspezifischeDateiFehlt_GibtFallbackPfadZurueck</summary>
    [Fact]
    public async Task ResolvePath_SprachspezifischeDateiFehlt_GibtFallbackPfadZurueck()
    {
        var fallbackPath = Path.Combine(_tempDir, "impressum.md");
        File.WriteAllText(fallbackPath, "# Fallback");

        var service = CreateService(fallbackPath);

        var originalCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("de");
        try
        {
            Assert.True(service.IsAvailable());
            var html = await service.GetContentAsHtmlAsync();
            Assert.Contains("Fallback", html);
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }
}
