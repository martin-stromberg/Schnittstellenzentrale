# Playwright-Tests — Installation und Konfiguration

## Voraussetzungen

- .NET 11.0 SDK
- PowerShell (`pwsh`) — wird für das `playwright.ps1`-Skript benötigt, das Playwright-Browser installiert
- Internetzugang beim ersten Build (Playwright lädt den Chromium-Browser herunter)

## Tests ausführen

```bash
dotnet test src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj
```

Alternativ für die gesamte Solution:

```bash
dotnet test
```

Der erste Build nach dem Klonen des Repositories lädt automatisch den Chromium-Browser herunter (MSBuild-Target `InstallPlaywright`). Dieser Schritt kann einige Minuten dauern.

Ein ungefilterter `dotnet test` umfasst beide Test-Lanes — für die reguläre Lane allein `-- --filter "Category!=OsInterface"`, für die OS-Schnittstellen-Tests `-- --filter "Category=OsInterface"` (xunit.v3/MTP: Filter als Test-App-Argument hinter `--`) (Details: [OS-Schnittstellen-Tests](../stabilitaet/os-interface-tests.md)).

## Coverage-Gate prüfen

Für die lokale Qualitätsprüfung wird der Coverage-Lauf der regulären Lane ausgeführt (die OS-Schnittstellen-Lane sammelt keine Coverage — sie ist in der CI best-effort und würde den Schwellwert instabil machen):

```bash
dotnet test --solution Schnittstellenzentrale.slnx -- --filter "Category!=OsInterface" --coverage --coverage-output-format cobertura
```

Die Qualitätssicherung prüft dabei die Line-Coverage der regulären Lane und erwartet mindestens `70 %`. Die Umsetzung bettet sich in die bestehenden UI-Tests ein, ohne die Schwelle im Code zu verändern. Relevante Abdeckungsbereiche sind zustandsabhängige Komponenten wie `ApplicationContentView`, `EnvironmentSelector`, `AppShell` und `MainLayout`, die mit gezielten bUnit-Tests abgesichert werden.

> **Hinweis:** Für die CI-Stabilisierung ist die Abdeckung der UI- und State-Pfade wichtiger als die Anzahl der Playwright-Flows. `bUnit` wird aufgrund der geringeren Laufzeit und höheren Reproduzierbarkeit daher bevorzugt.

## Playwright-Browser-Installation (MSBuild-Automatisierung)

Das Testprojekt enthält ein MSBuild-Target, das nach jedem Build automatisch `playwright install chromium` ausführt:

```xml
<Target Name="InstallPlaywright" AfterTargets="Build" Condition="'$(SkipPlaywrightInstall)' != 'true'">
  <Exec Command="pwsh -NoProfile -File &quot;$(MSBuildProjectDirectory)\bin\$(Configuration)\$(TargetFramework)\playwright.ps1&quot; install chromium" />
</Target>
```

### `SkipPlaywrightInstall`-Schalter

Das Target läuft standardmäßig bei jedem Build. Um den Installationsschritt zu überspringen — z. B. in CI-Umgebungen, in denen der Browser bereits gecacht ist — kann der Schalter gesetzt werden:

```bash
dotnet build -p:SkipPlaywrightInstall=true
dotnet test -p:SkipPlaywrightInstall=true
```

## Playwright-Traces einsehen

Nach jedem Testlauf werden Trace-Dateien im Verzeichnis `playwright-traces/` im Repo-Root abgelegt (absolut verankert über `TestPaths.FindRepoRoot()`, unabhängig vom Arbeitsverzeichnis des Testhosts). Die Dateinamen tragen einen UTC-Zeitstempel-Suffix, damit mehrere Tests derselben Klasse einander nicht überschreiben:

| Datei | Inhalt |
|---|---|
| `playwright-traces/{TestklassenName}-HHmmssfff.zip` | Trace des Testlaufs der Klasse (ein ZIP pro Testmethode) |
| `playwright-traces/SignalRSyncTests-B-HHmmssfff.zip` | Trace des zusätzlichen Browser-Kontexts B in `SignalRSyncTests` |

Trace öffnen mit dem Playwright Trace Viewer:

```bash
npx playwright show-trace playwright-traces/ApplicationCrudTests-101530123.zip
```

Alternativ: Trace-Datei auf [trace.playwright.dev](https://trace.playwright.dev) hochladen und im Browser einsehen — ohne lokale Node.js-Installation.

> **Hinweis:** Trace-Dateien werden auch bei erfolgreich bestandenen Tests geschrieben und akkumulieren durch den Zeitstempel-Suffix über Läufe hinweg — bei Bedarf manuell löschen.

## Konfiguration

| Parameter | Wert | Beschreibung |
|---|---|---|
| Testdatenbank | benannte In-Memory-SQLite (`Data Source={guid};Mode=Memory;Cache=Shared`) | Wird in `PlaywrightServer.InitializeAsync` pro Fixture angelegt und per geöffneter Anker-Verbindung offen gehalten — kein Datei-Locking zwischen Testläufen. |
| `Api:BaseUrl` | `http://127.0.0.1:5099` | Aus `appsettings.Playwright.json` (Umgebung `Playwright`); API-Aufrufe laufen ohnehin über den In-Process-Handler der `PlaywrightApiFactory`. |
| `ConnectionStrings:Default` | `Data Source=schnittstellenzentrale-playwright.db` | Aus `appsettings.Playwright.json`; wird zur Laufzeit durch die benannte In-Memory-Verbindung ersetzt. |
| `DatabaseProvider` | `SQLite` | Aus `appsettings.Playwright.json`. |
| `Urls` | `http://127.0.0.1:5099` | Aus `appsettings.Playwright.json`; `PlaywrightServer` setzt `_app.Urls` final auf die `BindUrl` der Fixture-Klasse (Ports 5099–5104). |
| Trace-Pfad | `playwright-traces/` | Im Repo-Root verankert (`TestPaths.FindRepoRoot()`); nicht konfigurierbar. |
| Chromium headless | `CI=true` → headless | `TestPaths.IsHeadless()` in `PlaywrightTestBase.InitializeAsync`; lokal läuft der Browser mit sichtbarem Fenster. |

## CI-Pipeline (GitHub Actions)

Der Job `os-interface-tests` (in `pr-staging-ci.yml` und `staging-ci.yml`) lädt die Trace-ZIPs bedingungslos als Artefakt hoch, damit auch ein fehlgeschlagener Lauf Diagnosematerial hinterlässt:

```yaml
- name: Upload Playwright traces
  if: always()
  uses: actions/upload-artifact@v7
  with:
    name: playwright-traces-pr   # bzw. playwright-traces-staging
    path: playwright-traces/
    retention-days: 14
```

> **Hinweis:** Der Upload-Pfad `playwright-traces/` ist relativ zum Repo-Root — dort verankert `PlaywrightTestBase` das Trace-Verzeichnis absolut über `TestPaths.FindRepoRoot()`, sodass der Glob unabhängig vom Testhost-Arbeitsverzeichnis greift.

## Überprüfung

Nach `dotnet test -- --filter "Category=OsInterface"` sollte die Zusammenfassung ohne Fehlschlag ausgehen.

Die OS-Schnittstellen-Tests verteilen sich auf die Playwright-Testklassen unter `Schnittstellenzentrale.Tests.Playwright` sowie die Temp-Dateisystem-Tests `ImpressumServiceTests` und `TempDirectoryCleanupTests` (Stand des Audits: 26 Klassen). Wenn Tests dieser Lane einzeln übersprungen oder gefiltert werden sollen, steht der xUnit-Trait-Filter `Category` bereit — alle Klassen der Lane tragen `[OsInterface]`:

```bash
dotnet test -- --filter "Category=OsInterface"
```

In der CI läuft diese Menge als best-effort-Job `os-interface-tests` (`continue-on-error`), getrennt vom blockierenden `build-and-test`-Job — Details in [OS-Schnittstellen-Tests](../stabilitaet/os-interface-tests.md).
