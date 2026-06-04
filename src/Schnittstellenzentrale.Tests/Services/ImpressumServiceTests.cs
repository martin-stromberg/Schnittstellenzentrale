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

        Assert.True(service.IsAvailable());
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
        var html = await service.GetContentAsHtmlAsync();

        Assert.Contains("<h1>Titel</h1>", html);
    }

    /// <summary>GetContentAsHtmlAsync_DateiFehlt_WirftException</summary>
    [Fact]
    public async Task GetContentAsHtmlAsync_DateiFehlt_WirftException()
    {
        var path = Path.Combine(_tempDir, "nicht_vorhanden.md");

        var service = CreateService(path);

        await Assert.ThrowsAsync<FileNotFoundException>(
            () => service.GetContentAsHtmlAsync());
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
        try
        {
            File.WriteAllText(expectedPath, "test");
            Assert.True(service.IsAvailable());
        }
        finally
        {
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

        try
        {
            File.WriteAllText(expectedPath, "test");
            Assert.True(service.IsAvailable());
        }
        finally
        {
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
}
