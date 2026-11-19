namespace Schnittstellenzentrale.Tests.Playwright.Infrastructure;

/// <summary>
/// Gemeinsame Pfad- und Umgebungshelfer für die Playwright-Tests.
/// </summary>
public static class TestPaths
{
    /// <summary>
    /// Ermittelt, ob Chromium im Headless-Modus gestartet werden soll.
    /// In CI-Umgebungen (CI=true) ist kein Display verfügbar, daher Headless;
    /// lokal bleibt der Browser sichtbar, damit Entwickler den Testablauf verfolgen können.
    /// </summary>
    /// <returns><c>true</c>, wenn der Browser headless gestartet werden soll.</returns>
    public static bool IsHeadless()
    {
        var ci = Environment.GetEnvironmentVariable("CI");
        return !string.IsNullOrEmpty(ci) && ci.Equals("true", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Ermittelt das Repository-Stammverzeichnis: vom Test-bin-Verzeichnis
    /// aufwärts wandern, bis die Solution-Datei gefunden ist.
    /// </summary>
    /// <returns>Der absolute Pfad des Repository-Stamms.</returns>
    /// <exception cref="DirectoryNotFoundException">Wenn kein übergeordnetes Verzeichnis die Solution-Datei enthält.</exception>
    public static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Schnittstellenzentrale.slnx")))
                return current.FullName;
            current = current.Parent;
        }
        throw new DirectoryNotFoundException(
            "Repository-Stamm nicht gefunden. Suche startete in: " + AppContext.BaseDirectory);
    }
}
