# OS-Schnittstellen-Tests

Reguläre Tests sind deterministische bUnit-, Service- und In-Memory-Integrationstests ohne echte Betriebssystem-Schnittstellen. Sie dürfen weder Browser-Automation, echte Socket-Bindungen noch lockanfällige Dateisystem-Szenarien benötigen und bleiben die blockierende CI-Lane.

OS-Schnittstellen-Tests prüfen genau diese Umgebungskontakte. Sie bleiben erhalten, werden aber getrennt markiert (`Category=OsInterface`) und laufen in der CI als eigener best-effort-Job `os-interface-tests` — ihre Fehlschläge blockieren weder PR-Merge noch Staging-Promotion.

## Klassifizierungskriterium

Maßgeblich ist, ob die **Assertion** eines Tests am OS-Verhalten des Beteiligten hängt — nicht, ob irgendwo eine Datei angefasst wird. Repo-Heuristik (Audit: `docs/features/task/issue-100-*/inventory/audit-os-interface.md`):

Als OS-Schnittstelle gilt:

- Playwright/Browser-Automation (echtes Chromium)
- echte Kestrel-Bindung inkl. festem Port (`PlaywrightServer`, Ports 5099–5104)
- `WebApplicationFactory`-Szenarien mit echtem HTTP-Roundtrip (Socket)
- Temp-Dateisystem (`Path.GetTempPath`, `sz-impressum-*`, `playwright-traces/`, Repo-Arbeitsverzeichnis)
- SignalR-Live-Roundtrips über die gebundene Adresse (WebSocket)
- Upload-Tests mit echten Dateien

Als regulär gilt (bleibt blockierend):

- In-Memory-SQLite (`Data Source=:memory:`)
- bUnit-Komponententests
- `WebApplicationFactory`/`TestServer` ohne echten Socket (In-Process-Handler)
- gemockte `HttpMessageHandler`-Tests
- reine `Task.Delay`-Zeitabhängigkeiten ohne OS-Ressource

Aktuell getaggte Klassen (Stand des Audits): alle 24 Testklassen unter `Schnittstellenzentrale.Tests.Playwright`, `Services/ImpressumServiceTests` (echtes Temp-FS + Schreibzugriffe ins `AppContext.BaseDirectory`) und `Helpers/TempDirectoryCleanupTests` (echtes Temp-FS).

## Kategorie

OS-nahe Tests tragen xUnit-kompatibel:

```csharp
[Trait("Category", "OsInterface")]
```

Im Testprojekt `Schnittstellenzentrale.Tests` stehen dafür zentrale Attribute bereit (`Infrastructure/Testing/`):

```csharp
[OsInterface]        // Klasse oder Methode — Standard: Klassenebene
[OsInterfaceFact]    // einzelne Fact-Methode in sonst regulärer Klasse
[OsInterfaceTheory]  // einzelne Theory-Methode
```

Konvention: `[OsInterface]` auf **Klassen**ebene (eine Zeile pro Klasse, deckt alle Methoden). Methodenebene nur für Einzelmethoden-Ausnahmen. `[OsInterface]` nicht gleichzeitig auf Klasse und Methode derselben Klasse (funktional harmlos, aber konventionswidrig). Die Meta-Tests akzeptieren die Attribut- und die rohe `[Trait("Category", "OsInterface")]`-Schreibweise gleichermaßen.

## `Flaky`-Kategorie

`Flaky` ist eine reine Markierungs-Kategorie für nachweislich umgebungsflaky OS-nahe Tests. Sie berechtigt zum einmaligen automatischen Retry in der best-effort-Lane.

Vergaberegeln:

- `Flaky` wird **nur in Kombination mit `OsInterface`** vergeben — nie in der regulären Lane (abgesichert durch `FlakyConventionTests`, der die eigene Test-Assembly scannt).
- `Flaky` wird nur bei einem **dokumentierten Flake-Ereignis** (konkreter CI-/Lokal-Fehlschlag mit Fehlerbild) oder einem Analyzer-Befund aus `Analyze-FlakyTestResults.ps1` vergeben.
- Die Markierung wird entfernt, sobald die Ursache behoben ist.

Schreibweisen:

```csharp
// Klassenmarkierung
[OsInterface]
[Flaky]

// Methodenmarkierung
[OsInterfaceFact]
[Flaky]

// oder roh (Trait-Argumente müssen const sein)
[Fact]
[Trait("Category", "OsInterface")]
[Trait("Category", "Flaky")]
```

## Lokale Befehle

```powershell
# Nur reguläre Tests (Standard, blockierend)
dotnet test src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj --filter "Category!=OsInterface"

# Nur OS-Schnittstellen-Tests
dotnet test src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj --filter "Category=OsInterface"

# Nur Flaky-markierte Tests
dotnet test src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj --filter "Category=Flaky"
```

## Automatischer Retry (Flaky)

Der automatische Einmal-Retry greift ausschließlich für `Flaky`-markierte Tests und existiert nur in der best-effort-Lane — die blockierende Lane bleibt retry-frei.

**In der CI** (`os-interface tests`-Job in `pr-staging-ci.yml` und `staging-ci.yml`): Schlägt der Testschritt fehl, läuft `Retry flaky tests once` (`if: failure()`) `scripts/Retry-FlakyTests.ps1`:

1. Primär-TRX parsen, fehlgeschlagene Testnamen einsammeln (Theory-Datenzeilen werden auf den Methoden-FQN normalisiert).
2. Flaky-Menge via `dotnet test --no-build --list-tests --filter "Category=Flaky"` ermitteln.
3. Schnittmenge leer → „keine Flaky-Fehlschläge", Exit 0 (kein Retry; bei Build-Fehlschlag/fehlendem TRX ebenso).
4. Sonst genau ein gefilterter Re-Run: `--filter "(Category=Flaky)&(FullyQualifiedName~...)"` → Retry-TRX `<primaername>-flaky-retry.trx`.
5. Auswertung recovered/persistent: recovered wird als `::notice::`, persistent als `::warning::` annotiert; persistente Fehlschläge lassen den Schritt mit Exit 1 fehlschlagen (der Job bleibt durch `continue-on-error` nicht-blockierend).

Beide TRX-Dateien (Primär und Retry) wandern über das Artefakt-Glob `test-results-os-interface*.trx` ins Upload-Artefakt — der Erstfehlschlag bleibt sichtbar.

**Lokal** — alternativ manuell:

```powershell
dotnet test src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj --filter "Category=OsInterface" --logger "trx;LogFileName=lauf.trx"
./scripts/Retry-FlakyTests.ps1 -TestProject src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj -PrimaryTrx src/Schnittstellenzentrale.Tests/TestResults/lauf.trx
```

## Flake-Statistik und Quarantäne

`scripts/Analyze-FlakyTestResults.ps1` wertet gesammelte TRX-Verzeichnisse aus: Lauf-Gruppierung über die Namenskonvention `<name>.trx` (Primär) / `<name>-flaky-retry.trx` (Retry desselben Laufs). Als Flake-Ereignis zählt Fehlschlag-im-Primär + grün-im-Retry desselben Laufs oder Fehlschlag→grün in aufeinanderfolgenden Primär-Läufen. Beides sind **getrennte Signale**, die addiert werden: Ein fail in Lauf N−1 gefolgt von fail+retry-grün in Lauf N zählt zwei Flake-Ereignisse — das Übergangs- und das Same-Run-Recovery-Symptom sind unabhängige Hinweise auf Instabilität (die Doppelzählung ist bewusst gewählt). Ein durch den Retry geretteter Lauf gilt für die Sequenz selbst als grün, sodass keine dritte Zählung durch den Folgelauf entsteht. Primär-Outcomes außerhalb Passed/Failed (`NotExecuted`, Discovery-Ausfall) gelten als fehlende Beobachtung: Sie zählen weder als Ausführung noch als grüner Folgelauf und unterbrechen die Fehlschlag→grün-Sequenz nicht.

```powershell
# Lokale Läufe
./scripts/Analyze-FlakyTestResults.ps1 -Path src/Schnittstellenzentrale.Tests/TestResults

# Inkl. CI-Historie (Artefakt-Kette)
./scripts/Download-OsInterfaceTrxHistory.ps1 -WorkflowFile pr-staging-ci.yml -ArtifactName test-results-os-interface-pr
./scripts/Analyze-FlakyTestResults.ps1 -Path src/Schnittstellenzentrale.Tests/TestResults, trx-history -CandidatesFile flaky-quarantine-candidates.json
```

Klassifikation pro Test (Fenster: letzte `-MaxRuns 20` Primär-Läufe):

- `Quarantaene-Kandidat`: ≥ 2 Flake-Ereignisse ODER Fehlrate ≥ 20 % bei ≥ 5 Ausführungen → in der CI wird automatisch ein dedupliziertes Issue „Quarantaene: `<test-fqn>`" angelegt (`New-FlakyQuarantineIssue.ps1`; Dedupe über offene Issues mit identischem Titel, Marker `<!-- flaky-quarantine:<fqn> -->` im Body).
- `beobachten`: ≥ 1 Flake-Ereignis ODER Fehlrate ≥ 10 %.
- `stabil`: sonst.

Die Schwellwerte sind Startwerte und kalibrierbar (Skript-Parameter `-QuarantineMinFlakeEvents`, `-QuarantineMinFailureRate`, `-QuarantineMinExecutions`, `-WatchMinFailureRate`). In der CI läuft der Analyzer als `if: always()`-Report-Schritt über dem TestResults-Verzeichnis des Laufs plus der per `Download-OsInterfaceTrxHistory.ps1` geholten Historie (Artefakt-Kette, Exit immer 0). Download- und Quarantäne-Schritte sind best-effort: Bei Fork-PRs ist `issues: write`/`GH_TOKEN` wirkungslos — die Skripte protokollieren dann eine Warnung und enden mit Exit 0.

## CI-Verhalten

Die Workflows `.github/workflows/pr-staging-ci.yml` und `.github/workflows/staging-ci.yml` führen die Testmenge in getrennten Jobs:

- Job `build & test`: blockierend; der `Test`-Schritt läuft mit `Category!=OsInterface` und liefert als einzige Lane die Coverage für das 70-%-Gate.
- Job `os-interface tests`: best-effort (`continue-on-error`, 45 min, eigene Permissions `contents:read` + `issues:write` + `actions:read`); `Test OS interface` läuft mit `Category=OsInterface` **ohne** Coverage-Collection, gefolgt von `Retry flaky tests once` (`if: failure()`), `Download OS-interface TRX history` + `Analyze flaky test results` + `Create quarantine issues` (alle `if: always()`/best-effort).
- Artefakte: `test-results-pr`/`test-results-staging` (regulär), `test-results-os-interface-*` (OS-Lane, inkl. `*-flaky-retry.trx`), `playwright-traces-*` (Trace-ZIPs, im OS-Job — `PlaywrightTestBase` schreibt über `TestPaths.FindRepoRoot()` absolut in den Repo-Root, Dateinamen mit UTC-Zeitstempel-Suffix `{TestName}-HHmmssfff.zip`).
- `version`/`prerelease` in `staging-ci.yml` hängen **nicht** am OS-Job — ein OS-Fehlschlag darf die Release-Kette nie blockieren.

Reguläre Fehler gelten als Regressionsfehler. OS-Schnittstellen-Fehler werden sichtbar gemacht, blockieren aber nicht automatisch.

## Infrastruktur-Härtung

- `PlaywrightServer` pollt nach `StartAsync` den echten Kestrel-Socket (`WaitForServerReadyAsync`, 500-ms-Intervall, 60-s-Deadline — **jede** HTTP-Antwort gilt als „ready", auch 404/Redirects). Die Init-Phasen (Anchor-DB, `BuildWebApplicationAsync`, `StartAsync`, `WaitForServerReady`, `OnAfterStartAsync`, Swagger-Warmup) tragen Marker; Fehler werden mit Phase/BindUrl/letztem Fehler erneut geworfen. Der feste Port 5099 (+ Unterklassen 5100–5104) bleibt bewusst — die sechs Collections serialisieren pro Fixture und die Ports sind gestaffelt.
- `PlaywrightTestBase` setzt explizite Default-Timeouts (`SetDefaultTimeout(30 s)`, `SetDefaultNavigationTimeout(60 s)` — Blazor-Server-Erstaufruf kann unter CI-Last >30 s liegen), stoppt Tracing mit 30-s-Budget (verwaiste Stop-Tasks werden per `OnlyOnFaulted`-Continuation beobachtet) und führt die komplette Dispose-Kette best-effort mit Fehlersammlung — kein Assert-Overlay mehr. Diagnosen gehen über `ITestOutputHelper` in den TRX-`<Output>`-Block.
- `TempDirectoryCleanup` (`Helpers/`): best-effort-Löschung mit 5 Versuchen und linearem Backoff (100 ms × Versuch); Restfehler nur nach `Console.Error`. Wird in allen `sz-impressum-*`-Cleanup-Pfaden, `ImpressumServiceTests.Dispose` und `IconUploadTests` (eindeutige `sz-icon-{guid}`-Namen) eingesetzt.

## Bekannte Fehlerbilder

- Port 5099–5104 bereits belegt (zweiter lokaler Testlauf, verwaister Kestrel-Prozess) → `WaitForServerReady`-Timeout mit Phase/BindUrl im Fehlertext.
- Blazor-Server-Erstaufruf (SignalR-Connect + Circuit) kann unter CI-Last >30 s dauern → Navigationen haben 60-s-Budget; Einzelaktionen 30 s.
- `Tracing.StopAsync` kann hängen (kein CancellationToken) → 30-s-Budget; das ZIP wird ggf. verspätet geschrieben.
- `sz-impressum-*`-/Trace-Datei-Löschung kann durch Locks oder Read-only-Attribute scheitern → `TempDirectoryCleanup` mit Retry/Backoff, Restfehler nur geloggt.
- Diagnose: Trace-ZIPs pro Test unter `playwright-traces/` im Repo-Root (CI-Artefakt `playwright-traces-pr`/`-staging`), einsehbar mit `npx playwright show-trace <datei>.zip`; Server-/Dispose-Diagnosen im TRX-`<Output>`-Block des Tests.

## Env-Flags und Retry

Es gibt **keine** Skip-Flags per Umgebungsvariable (`*_SKIP_*` o. ä.): Alle Playwright-Tests laufen mit headless Chromium erfolgreich in der CI — eine Unterscheidung lokal/CI ist nicht nötig.

Retry ist nicht zur Stabilisierung regulärer Tests vorgesehen. Fachliche Assertion-Fehler regulärer Tests werden nicht kaschiert. Ausnahme: der oben beschriebene `Flaky`-Einmal-Retry — er existiert nur in der best-effort-Lane, weil `Flaky` nur zusammen mit `OsInterface` vergeben wird.

**Job-Rerun-Konvention** (komplementär zum Trait-Retry): Schlägt der `os-interface tests`-Job ohne auswertbares TRX fehl (Runner-Abbruch, Testhost-Crash — der Retry-Schritt findet dann keine Fehlschläge und endet neutral), ist genau ein `gh run rerun --job <job-id>` nur für diesen Job erlaubt — niemals `gh run rerun --failed` (das würde blockierende Jobs mit wiederholen). Die numerische Job-ID kommt aus `gh run view --job <id>` bzw. `gh run view <run-id> --json jobs`. Bleibt der Fehler bestehen, tritt er wiederholt auf oder meldet der Analyzer eine Schwellwert-Überschreitung, wird ein Quarantäne-Issue angelegt und das Fehlerbild unter „Bekannte Fehlerbilder" dokumentiert (inkl. Verweis auf Issue und Analyzer-Befund). Fehler in der regulären Lane werden niemals wiederholt — sie gelten immer als Regression.

## Playwright-Versionsupdates

Bei `Microsoft.Playwright`-Updates (insbesondere Major-Sprüngen über mehrere Releases) gilt wegen der best-effort-CI-Lane ein **lokales Gate als Merge-Pflicht**: `dotnet test --filter "Category=OsInterface"` muss lokal vollständig grün gelaufen sein, bevor der PR gemergt wird; Ergebnis/TRX sind im PR zu verlinken. Vorab sind die Release-Notes aller Zwischenreleases auf entfernte/geänderte APIs zu sichten und gegen die Aufrufe unter `Playwright/` zu prüfen. Die Browser-Installation erfolgt automatisch über das `InstallPlaywright`-MSBuild-Target (`playwright.ps1 install chromium`, abschaltbar via `-p:SkipPlaywrightInstall=true`); ein Browser-Cache existiert in der CI nicht.

## Audit-Heuristik und Meta-Tests

Klassifizierungskriterium für **neue** Tests: „echte OS-Berührung ⇒ `[OsInterface]`" (Kriterienliste oben). Wiederholbare Suchheuristik für ungetaggte Kandidaten:

```bash
grep -rnE 'Path\.GetTempPath|Playwright|WebApplicationFactory|Kestrel|Listen|localhost|Directory\.Delete|Process\.Start' src/Schnittstellenzentrale.Tests --include='*.cs'
```

Treffer ohne `OsInterface`-Markierung fachlich bewerten: `ControllerTestFactory`/`TestServer` (In-Process, kein Socket), `Data Source=:memory:` und Mock-`HttpMessageHandler` sind Fehl-Positive und bleiben regulär.

Zwei Meta-Tests (reguläre Lane, `Infrastructure/Testing/`):

- `FlakyConventionTests` — erzwingt `Flaky` nur mit `OsInterface` (beide Schreibweisen).
- `TraitParityTests` — jede Testklasse mit `Playwright` im `FullName` und ≥1 Testmethode muss `Category=OsInterface` tragen (Parität zum früheren Namensfilter `FullyQualifiedName~Playwright`).

**Restrisiko, transparent:** `TraitParityTests` erfasst nur FQN-Parität — ein neuer umgebungssensitiver Test **ohne** „Playwright" im Namen landet unerkannt in der blockierenden Lane (so wie `ImpressumServiceTests` nur per Audit gefunden wurde). Bei echter OS-Berührung daher manuell `[OsInterface]` setzen; ein späterer härterer Meta-Test auf `Path.GetTempPath`-/Kestrel-Heuristiken ist als Follow-up denkbar.
