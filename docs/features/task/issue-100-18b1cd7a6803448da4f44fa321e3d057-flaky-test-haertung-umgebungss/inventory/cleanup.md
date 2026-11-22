# Cleanup-Bestand

## Dispose-/finally-Pfade in `Playwright/Infrastructure/`

### `PlaywrightTestBase.DisposeAsync` (Zeilen 97–110)

```csharp
public async Task DisposeAsync()
{
    Directory.CreateDirectory("playwright-traces");
    await Context.Tracing.StopAsync(new TracingStopOptions { Path = ... });
    await OnDisposingAsync();
    await Context.DisposeAsync();
    await _browser.DisposeAsync();
    _playwright.Dispose();
}
```

Fakten:
- **Kein try/catch/finally.** Ein Fehler in `Tracing.StopAsync` (Zeile 100) überspringt `OnDisposingAsync`, `Context.DisposeAsync`, `_browser.DisposeAsync` und `_playwright.Dispose()` — der Browser-Prozess bleibt stehen **und** die Dispose-Exception überdeckt den eigentlichen Test-Assert.
- `Tracing.StopAsync` schreibt immer einen Trace — auch bei grünem Test (bewusst, dient der CI-Diagnose via Artefakt-Upload).
- `playwright-traces` wird relativ zum Test-Arbeitsverzeichnis angelegt (in CI = Repo-Root, da `dotnet test` von dort läuft).
- `OnInitializedAsync`/`OnDisposingAsync` sind virtuelle Hooks (Zeilen 69, 113); Basisklasse ruft `OnDisposingAsync` **nach** dem eigenen Trace-Stop, **vor** Context-Dispose.

### Abgeleitete Kontexte

- `SignalRSyncTests.OnDisposingAsync` (`Playwright/SignalRSyncTests.cs:24–32`) stoppt den Trace des **zweiten** Kontexts (`SignalRSyncTests-B.zip`) und dispost `_contextB`. Wirft diese Methode, wird sie innerhalb von `DisposeAsync` (Basis-Z. 105) ausgeführt → `Context`/`_browser`/`_playwright` der Basis werden nicht mehr disposed.
- `CreateAdditionalContextAsync` (Basis, Zeilen 73–94) legt beliebig viele Kontexte an; die Basis dispost **nur** den Haupt-`Context` — jede abgeleitete Klasse muss eigene Kontexte in `OnDisposingAsync` selbst freigeben.

### `PlaywrightServer.DisposeAsync` (Zeilen 158–167)

```csharp
if (_app is not null) { await _app.StopAsync(); await _app.DisposeAsync(); }
_apiFactory?.Dispose();
_anchorConnection?.Dispose();
```

- Kein Fehlerhandling: Wirft `StopAsync`/`DisposeAsync`, werden `_apiFactory` und `_anchorConnection` nicht freigegeben; die Exception propagiert ins xUnit-Fixture-Teardown.
- Reihenfolge: Kestrel-Stop → WebApplication-Dispose → `PlaywrightApiFactory` (TestServer)-Dispose → Anchor-SQLite-Connection.

### Impressum-Server-Varianten (`sz-impressum-*`)

Identisches Muster in `PlaywrightImpressumServer` (Z. 49–65), `PlaywrightImpressumLanguageServer` (Z. 54–70), `PlaywrightImpressumFallbackServer` (Z. 48–64):

```csharp
public override async Task DisposeAsync()
{
    try { await base.DisposeAsync(); }
    finally { Cleanup(); }
}
private void Cleanup()
{
    if (_tempDir != null && Directory.Exists(_tempDir))
        Directory.Delete(_tempDir, recursive: true);
}
```

- `try/finally` garantiert Ausführung des Cleanup, **aber**: `Directory.Delete` ohne Retry/Backoff/Filter — ein gesperrtes File-Handle (AV-Scan, offene Datei) lässt die Exception aus `finally` propagieren und kann eine eventuell schon fliegende `base.DisposeAsync`-Exception **oder den Test-Assert** überdecken.
- Gleiches `Cleanup()` läuft im `catch` der `InitializeAsync` (z. B. `PlaywrightImpressumServer.cs:29–37`): Tempdir wird bei Start-Fehlschlag entfernt, Fehler beim Löschen würden den Start-Fehler überdecken.

### `PlaywrightApiFactory`

`internal sealed class` ohne eigenen Dispose-Override; wird in `PlaywrightServer.DisposeAsync` Zeile 165 disposed.

### `SitePageTests.DisposeAsync` (`Playwright/SitePageTests.cs:39–44`)

Sequenzielle `await _context.DisposeAsync(); await _browser.DisposeAsync(); _playwright.Dispose();` — kein Fehlerhandling; früher Dispose-Fehler verhindert spätere Disposes.

## Temp-Verzeichnis-/Datei-Löschungen außerhalb Infrastructure

| Stelle | Mechanismus | Fehlerbehandlung |
|---|---|---|
| `Services/ImpressumServiceTests.cs:20–24` | `Dispose()`: `if (Directory.Exists(_tempDir)) Directory.Delete(_tempDir, recursive: true)` | Keine — kein try/catch, kein Retry; Löschfehler propagiert in den Test-Teardown |
| `Services/ImpressumServiceTests.cs:133–134, 157–158` | `finally { if (File.Exists(expectedPath)) File.Delete(expectedPath); }` für Dateien in `AppContext.BaseDirectory` | `File.Delete` selbst ungeschützt; Fehler würde den Assert überdecken |
| `Playwright/IconUploadTests.cs:56–59, 84–87` | `finally { File.Delete(tempFile); }` | Ungeschützt; feste Dateinamen `test-icon.png`/`test-icon.txt` in `Path.GetTempPath` — Kollision zwischen parallelen Läufen möglich |
| `PlaywrightTestBase.cs:99` + `SignalRSyncTests.cs:26` | `Directory.CreateDirectory("playwright-traces")` | Kein Löschen — Traces akkumulieren lokal; nur per `.zip`-Überschreibung je Testklasse begrenzt |

## Sonstige Ressourcen-Freigaben

- `PlaywrightServer._anchorConnection` (`SqliteConnection`, named in-memory `Mode=Memory;Cache=Shared`, Z. 63–66) wird in `DisposeAsync` Z. 166 disposed — hält die shared In-Memory-DB für alle Verbindungen der Collection offen.
- `ControllerTestFactory.Dispose(bool)` (`Helpers/ControllerTestFactory.cs:99–104`): `base.Dispose` + `_connection.Dispose()` — `TestServer`-Fixture, `:memory:`-Connection.
- `EndpointExecutionIntegrationTests.DisposeAsync` (`Integration/EndpointExecutionIntegrationTests.cs:29–34`): `_apiClient.Dispose(); _factory.Dispose();` — ohne Fehlerbehandlung, `Dispose()` der Factory kann werfen.

## Fehlende Bestandteile (Fakt)

- Kein `TempDirectoryCleanup`-o. ä. Helfer im Testprojekt (Volltextsuche: kein Retry/Backoff um `Directory.Delete`).
- Keine `catch`-Blöcke um irgendeine `Directory.Delete`-/`File.Delete`-Stelle im Testprojekt.
- Kein Mechanismus, der `playwright-traces`- oder `sz-impressum-*`-Reste vor Laufbeginn aufräumt.
