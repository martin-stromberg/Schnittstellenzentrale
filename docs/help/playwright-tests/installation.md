# Playwright-Tests — Installation und Konfiguration

## Voraussetzungen

- .NET 9.0 SDK
- PowerShell (`pwsh`) — wird für das `playwright.ps1`-Skript benötigt, das Playwright-Browser installiert
- Internetzugang beim ersten Build (Playwright lädt den Chromium-Browser herunter)

## Tests ausführen

```
dotnet test src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj
```

Alternativ für die gesamte Solution:

```
dotnet test
```

Der erste Build nach dem Klonen des Repositories lädt automatisch den Chromium-Browser herunter (MSBuild-Target `InstallPlaywright`). Dieser Schritt kann einige Minuten dauern.

## Playwright-Browser-Installation (MSBuild-Automatisierung)

Das Testprojekt enthält ein MSBuild-Target, das nach jedem Build automatisch `playwright install chromium` ausführt:

```xml
<Target Name="InstallPlaywright" AfterTargets="Build" Condition="'$(SkipPlaywrightInstall)' != 'true'">
  <Exec Command="pwsh -NoProfile -File &quot;$(MSBuildProjectDirectory)\bin\$(Configuration)\$(TargetFramework)\playwright.ps1&quot; install chromium" />
</Target>
```

### `SkipPlaywrightInstall`-Schalter

Das Target läuft standardmäßig bei jedem Build. Um den Installationsschritt zu überspringen — z. B. in CI-Umgebungen, in denen der Browser bereits gecacht ist — kann der Schalter gesetzt werden:

```
dotnet build -p:SkipPlaywrightInstall=true
dotnet test -p:SkipPlaywrightInstall=true
```

## Playwright-Traces einsehen

Nach jedem Testlauf werden Trace-Dateien im Verzeichnis `playwright-traces/` relativ zum Testausführungsverzeichnis abgelegt:

| Datei | Inhalt |
|---|---|
| `playwright-traces/{TestklassenName}.zip` | Trace der Testklasse (alle Tests der Klasse) |
| `playwright-traces/SignalRSyncTests-A.zip` | Trace von Browser-Kontext A in `SignalRSyncTests` |
| `playwright-traces/SignalRSyncTests-B.zip` | Trace von Browser-Kontext B in `SignalRSyncTests` |

Trace öffnen mit dem Playwright Trace Viewer:

```
npx playwright show-trace playwright-traces/ApplicationCrudTests.zip
```

Alternativ: Trace-Datei auf [trace.playwright.dev](https://trace.playwright.dev) hochladen und im Browser einsehen — ohne lokale Node.js-Installation.

> **Hinweis:** Trace-Dateien werden auch bei erfolgreich bestandenen Tests geschrieben. Sie müssen manuell gelöscht werden oder vor dem nächsten Testlauf überschrieben.

## Konfiguration

| Parameter | Wert | Beschreibung |
|---|---|---|
| SQLite-Dateiname | `schnittstellenzentrale-tests.db` | Fest codiert in `PlaywrightTestFactory`. Die Datei wird im Testausführungsverzeichnis angelegt. |
| `Api:BaseUrl` | Dynamisch (`Server.BaseAddress`) | Wird in `PlaywrightTestFactory.CreateHost` auf die tatsächliche Kestrel-Adresse gesetzt; kein Eintrag in `appsettings.json` nötig. |
| `ConnectionStrings:Default` | `Data Source=schnittstellenzentrale-tests.db` | Wird in `PlaywrightTestFactory.ConfigureWebHost` gesetzt; überschreibt die Produktionskonfiguration. |
| `DatabaseProvider` | `SQLite` | Wird in `PlaywrightTestFactory.ConfigureWebHost` gesetzt. |
| Trace-Pfad | `playwright-traces/` | Relativ zum Testausführungsverzeichnis; nicht konfigurierbar. |
| Chromium headless | `true` | Fest codiert in `PlaywrightTestBase.InitializeAsync`; kein sichtbares Browserfenster. |

## CI-Pipeline (GitHub Actions)

Damit Traces bei Fehlern als Build-Artefakte verfügbar sind, muss ein Upload-Schritt konfiguriert werden:

```yaml
- uses: actions/upload-artifact@v4
  if: failure()
  with:
    name: playwright-traces
    path: src/Schnittstellenzentrale.Tests/bin/Debug/net9.0/playwright-traces/
```

> **Hinweis:** Der genaue Pfad hängt von der Konfiguration und dem Arbeitsverzeichnis des CI-Runners ab. Die Datei wird vom Testprozess relativ zu seinem Arbeitsverzeichnis geschrieben, das in der Regel `bin/Debug/net9.0/` entspricht.

## Überprüfung

Nach `dotnet test` sollte die Ausgabe zeigen:

```
Passed! - Failed: 0, Passed: 11, Skipped: 0
```

Die elf Playwright-Tests verteilen sich auf sieben Testklassen. Wenn Playwright-Tests einzeln übersprungen oder gefiltert werden sollen, kann der xUnit-Trait-Filter verwendet werden:

```
dotnet test --filter "FullyQualifiedName~Playwright"
```
