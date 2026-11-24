using Microsoft.Playwright;
using Schnittstellenzentrale.Tests.Infrastructure.Testing;
using Schnittstellenzentrale.Tests.Playwright.Infrastructure;

namespace Schnittstellenzentrale.Tests.Playwright;

/// <summary>
/// Smoke-Tests der statischen Präsentationsseite unter <c>site/</c>.
/// Läuft bewusst ohne <see cref="Infrastructure.PlaywrightServer"/> und ohne
/// <c>"Playwright"</c>-Collection: Die statischen Dateien werden per
/// Route-Interception unter der Pseudo-Origin <c>http://site.test/</c> ausgeliefert.
/// </summary>
[OsInterface]
public class SitePageTests : IAsyncLifetime
{
    private const string SiteOrigin = "http://site.test";

    private IPlaywright _playwright = null!;
    private IBrowser _browser = null!;
    private IBrowserContext _context = null!;
    private IPage _page = null!;

    /// <inheritdoc/>
    public async ValueTask InitializeAsync()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = TestPaths.IsHeadless()
        });
        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            Locale = "de-DE",
            ExtraHTTPHeaders = new Dictionary<string, string> { ["Accept-Language"] = "de-DE,de;q=0.9" }
        });
        // Explizite Default-Timeouts analog PlaywrightTestBase: Aktionen 30 s,
        // Navigationen 60 s (weit unter dem 45-min-Job-Timeout der OS-Lane).
        _context.SetDefaultTimeout(30_000);
        _context.SetDefaultNavigationTimeout(60_000);
        await _context.RouteAsync("**/*", ServeSiteFileAsync);
        _page = await _context.NewPageAsync();
    }

    /// <inheritdoc/>
    /// <remarks>Komplett best-effort: Jeder Dispose-Schritt läuft einzeln, Fehler werden
    /// gesammelt und protokolliert — die Methode wirft nie, damit ein Cleanup-Fehler den
    /// eigentlichen Test-Assert nicht überdeckt.</remarks>
    public async ValueTask DisposeAsync()
    {
        var errors = new List<Exception>();

        try { await _context.DisposeAsync(); }
        catch (Exception error) { errors.Add(error); }

        try { await _browser.DisposeAsync(); }
        catch (Exception error) { errors.Add(error); }

        try { _playwright.Dispose(); }
        catch (Exception error) { errors.Add(error); }

        foreach (var error in errors)
        {
            Console.Error.WriteLine(
                $"SitePageTests.DisposeAsync: Cleanup-Fehler (best-effort, nicht werfend): {error}");
        }
    }

    /// <summary>Alle Pflichtsektionen der Seite sind sichtbar (Hero, Feature-Kacheln, Galerie, Download, Footer).</summary>
    [Fact]
    public async Task Index_ZeigtAllePflichtsektionen()
    {
        await _page.GotoAsync(SiteOrigin);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        await Assertions.Expect(_page).ToHaveTitleAsync(new System.Text.RegularExpressions.Regex("Schnittstellenzentrale"));
        await Assertions.Expect(_page.Locator("#hero h1")).ToBeVisibleAsync();
        await Assertions.Expect(_page.Locator(".feature-card")).ToHaveCountAsync(9);
        await Assertions.Expect(_page.Locator("#screenshots")).ToBeVisibleAsync();
        await Assertions.Expect(_page.Locator("#download")).ToBeVisibleAsync();
        await Assertions.Expect(_page.Locator(".site-footer")).ToBeVisibleAsync();
    }

    /// <summary>Alle Galerie-Bilder sind tatsächlich geladen (naturalWidth &gt; 0).</summary>
    [Fact]
    public async Task Screenshots_SindGeladen()
    {
        await _page.GotoAsync(SiteOrigin);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await _page.WaitForFunctionAsync(
            "() => [...document.querySelectorAll('.gallery-item img')].every(i => i.complete)");

        var images = _page.Locator(".gallery-item img");
        var count = await images.CountAsync();
        Assert.True(count >= 4, $"Mindestens 4 Galerie-Bilder erwartet, gefunden: {count}");

        for (var i = 0; i < count; i++)
        {
            var naturalWidth = await images.Nth(i).EvaluateAsync<int>("img => img.naturalWidth");
            Assert.True(naturalWidth > 0, $"Galerie-Bild {i} ist nicht geladen (naturalWidth = 0).");
        }
    }

    /// <summary>Der Download-Link zeigt auf releases/latest; der Footer verlinkt Repository, docs/help und LEGAL.md.</summary>
    [Fact]
    public async Task DownloadLink_ZeigtAufLatestRelease()
    {
        await _page.GotoAsync(SiteOrigin);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var downloadLinks = _page.Locator("#download a[href*='releases/latest']");
        Assert.True(await downloadLinks.CountAsync() >= 1, "Kein Download-Link auf releases/latest gefunden.");

        var footer = _page.Locator(".site-footer");
        Assert.True(await footer.Locator("a[href*='github.com/martin-stromberg/Schnittstellenzentrale']").CountAsync() >= 1,
            "Kein Repository-Link im Footer gefunden.");
        await Assertions.Expect(footer.Locator("a[href*='docs/help']")).ToHaveCountAsync(1);
        await Assertions.Expect(footer.Locator("a[href*='LEGAL.md']")).ToHaveCountAsync(1);
    }

    /// <summary>Klick auf den Theme-Toggle setzt .dark und persistiert die Wahl über einen Reload hinweg.</summary>
    [Fact]
    public async Task DarkToggle_SchaltetUm_UndPersistiert()
    {
        await _page.GotoAsync(SiteOrigin);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var toggle = _page.Locator("#theme-toggle");
        await Assertions.Expect(toggle).ToBeVisibleAsync();
        await toggle.ClickAsync();

        await Assertions.Expect(_page.Locator("html.dark")).ToHaveCountAsync(1);
        var stored = await _page.EvaluateAsync<string>("() => localStorage.getItem('colorScheme')");
        Assert.Equal("Dark", stored);

        await _page.ReloadAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Assertions.Expect(_page.Locator("html.dark")).ToHaveCountAsync(1);
    }

    /// <summary>Im Mobile-Viewport (390×844) entsteht kein horizontaler Scrollbalken und Kerninhalte bleiben sichtbar.</summary>
    [Fact]
    public async Task MobileViewport_KeinHorizontalScroll()
    {
        await _page.SetViewportSizeAsync(390, 844);
        await _page.GotoAsync(SiteOrigin);
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var scrollWidth = await _page.EvaluateAsync<int>("() => document.documentElement.scrollWidth");
        var clientWidth = await _page.EvaluateAsync<int>("() => document.documentElement.clientWidth");
        Assert.True(scrollWidth <= clientWidth,
            $"Horizontaler Overflow: scrollWidth {scrollWidth} > clientWidth {clientWidth}");

        await Assertions.Expect(_page.Locator("#hero h1")).ToBeVisibleAsync();
        await Assertions.Expect(_page.Locator(".feature-card").First).ToBeVisibleAsync();
    }

    private async Task ServeSiteFileAsync(IRoute route)
    {
        var url = new Uri(route.Request.Url);
        if (!url.Host.Equals("site.test", StringComparison.OrdinalIgnoreCase))
        {
            // Externe Referenzen (z. B. shields.io-Badge) werden bewusst abgebrochen —
            // der Test darf nicht von echtem Netzwerkzugriff abhängen.
            await route.AbortAsync();
            return;
        }

        var relative = url.AbsolutePath.TrimStart('/');
        if (string.IsNullOrEmpty(relative))
            relative = "index.html";

        var path = Path.Combine(SiteDirectory(), relative.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(path))
        {
            await route.FulfillAsync(new RouteFulfillOptions { Status = 404 });
            return;
        }

        await route.FulfillAsync(new RouteFulfillOptions
        {
            Path = path,
            ContentType = ContentTypeFor(Path.GetExtension(path))
        });
    }

    private static string ContentTypeFor(string extension) => extension.ToLowerInvariant() switch
    {
        ".html" => "text/html",
        ".css" => "text/css",
        ".js" => "text/javascript",
        ".png" => "image/png",
        ".svg" => "image/svg+xml",
        _ => "application/octet-stream"
    };

    private static string SiteDirectory() => Path.Combine(TestPaths.FindRepoRoot(), "site");
}
