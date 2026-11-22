# Umsetzungsplan – Flaky-Test-Härtung (Issue #100)

**Quelle:** `requirement.md` (GitHub-Issue #100)
**Bestandsaufnahme:** `inventory.md` + `inventory/*.md`
**Blaupause:** `martin-stromberg/Softwareschmiede` Issue #428 / PR #432 (Klon: `C:/Users/Martin/AppData/Local/Temp/sws-blueprint`, Commit `ed6e65a`)
**Branch:** `task/issue-100-18b1cd7a6803448da4f44fa321e3d057-flaky-test-haertung-umgebungss`

---

## 1. Zielbild

Das Testprojekt `src/Schnittstellenzentrale.Tests` (xUnit **2.9.3**, ein Testprojekt) erhält eine
trait-basierte Trennung in zwei Lanes:

- **Reguläre Lane** (`Category!=OsInterface`): deterministische bUnit-, Service- und
  In-Memory-Integrationstests; bleibt blockierend und alleinige Quelle des 70-%-Coverage-Gates.
- **OS-Schnittstellen-Lane** (`Category=OsInterface`): alle 24 Playwright-Testklassen
  (Chromium + echte Kestrel-Bindung auf den Ports 5099–5104, `sz-impressum-*`-Tempdirs,
  `playwright-traces`) sowie `Services/ImpressumServiceTests` (echtes Temp-Dateisystem,
  Schreibzugriffe ins `AppContext.BaseDirectory`). Läuft in einem **eigenen Job**
  `os-interface-tests` mit Job-Level `continue-on-error: true` — Fehlschläge blockieren
  weder PR-Merge noch Staging-Promotion.

Infrastruktur-Härtung: Server-Readiness-Poll mit expliziter Deadline, explizite
Playwright-Default-Timeouts, gebudgetetes Trace-Stop, fehlertolerante Dispose-Ketten
(kein Assert-Overlay mehr), `TempDirectoryCleanup`-Helfer mit Retry/Backoff.

Retry-/Statistik-Konvention: `Flaky`-Trait (nur in Kombination mit `OsInterface`, per
Meta-Test abgesichert) berechtigt zum einmaligen automatischen Re-Run in der OS-Lane;
die TRX-Auswertung über ~20 Läufe erzeugt Quarantäne-Kandidaten, für die automatisiert
ein dedupliziertes GitHub-Issue angelegt wird. Kein Blanket-Retry.

---

## 2. Scope-Klärung

### Im Scope

1. Trait-Infrastruktur (xUnit-v2-`TraitDiscoverer`-Muster) + Tagging der 25 Testklassen.
2. Meta-Tests `FlakyConventionTests` + `TraitParityTests`.
3. Neuer CI-Job `os-interface-tests` in `pr-staging-ci.yml` **und** `staging-ci.yml`
   (identisch, nur Artefakt-Suffixe `-pr`/`-staging` und Workflow-Name für den
   History-Download unterscheiden sich); Umbau `build-and-test`; `release.yml`-Filter.
4. Deadline-/Timeout-Härtung in `PlaywrightServer`, `PlaywrightTestBase`, `SitePageTests`.
5. Cleanup-Härtung via neuem `TempDirectoryCleanup`-Helfer + eigene Unit-Tests.
6. Skripte: `Retry-FlakyTests.ps1`, `Analyze-FlakyTestResults.ps1`, `lib/Trx.ps1`,
   `lib/TestDiscovery.ps1`, `Download-OsInterfaceTrxHistory.ps1`,
   `New-FlakyQuarantineIssue.ps1`.
7. Kleinigkeiten: `IconUploadTests` eindeutige Temp-Dateinamen; `playwright-traces/`
   in `.gitignore`.
8. Doku: `docs/help/stabilitaet/os-interface-tests.md` (neu), `docs/help/index.md`,
   `CLAUDE.md`, `README.md`, `.claude/css-conventions.md`, betroffene Teile von
   `docs/help/playwright-tests/installation.md`.
9. Coverage-Nachmessung der regulären Lane nach dem Tagging.
10. CI-Verifikation: ≥3 komplette `pr-staging-ci.yml`-Durchläufe; `staging-ci.yml`
    verbindlich als Post-Merge-Verifikation (deklarierte Abweichung von FR 11, s. §7).

### Scope-Ausschlüsse (bewusst, mit Status)

| Ausschluss | Status / Begründung |
|---|---|
| Kein dynamischer Port für `PlaywrightServer` | **Entschieden (fest 5099–5104):** Die 6 Collections serialisieren bereits pro Collection-Fixture und sind port-gestaffelt; ein dynamischer Port erforderte die URL-Übergabe an alle Browser-Kontexte — unverhältnismäßiger Umbau. Wird in der Doku dokumentiert (s. Entscheidung 4). |
| Kein Blanket-Retry der Gesamtsuite | **Entschieden (Issue-Vorgabe):** Einmal-Retry nur für `Flaky`-markierte Tests, nur in der OS-Lane. Reguläre Lane bleibt retry-frei. |
| Kein Umbau der xUnit-Collections/Parallelisierung | Die 6 CollectionDefinitions bleiben wie sie sind; die Port-Staffelung trägt die Parallelausführung bereits. |
| Kein `SOFTWARESCHMIEDE_SKIP_*`-Flag-Konzept | Die Blaupause braucht Skip-Flags für ConPTY/FlaUI auf headless Runnern; hier laufen alle Playwright-Tests headless-Chromium bereits erfolgreich in der CI. Keine analogen Flags nötig — Kommentare in den übernommenen Skripten werden entsprechend generisch angepasst. |
| Kein zweites Testprojekt / kein Compile-Link der Meta-Tests | Die Blaupause verteilt `FlakyConventionTests` per `<Compile Include>` auf zwei Projekte; hier gibt es genau ein Testprojekt. |
| Kein Artefakt-Merge der OS-Lane-Coverage | **Entschieden (Entscheidung 2):** OS-Lane sammelt keine Coverage. Merge-Fallback nur als dokumentierter Plan-B, falls die reguläre Lane unter 70 % fiele. |
| Keine Umstellung auf xUnit v3 | Das Repo bleibt bei xUnit 2.9.3; die Blaupausen-Attribute (v3-API `ITraitAttribute`) werden auf das v2-`TraitDiscoverer`-Muster übersetzt, nicht das Framework gewechselt. |
| Keine Korrektur aller veralteten `docs/help/playwright-tests/`-Inhalte | Nur die von dieser Änderung betroffenen Stellen (Filter-Konvention, Coverage-Aussage). Die übrigen Bestands-Diskrepanzen (`PlaywrightTestFactory`, Datei-SQLite, „11 Tests in 7 Klassen") sind Alt-Schulden und kein Teil dieses Issues. |

---

## 3. Entscheidungen zu offenen Punkten

### Entscheidung 1 – Job-Platzierung: eigener Job `os-interface-tests` in den beiden bestehenden Workflows

- Eigener Job in `pr-staging-ci.yml` **und** `staging-ci.yml` (nicht eigener Workflow,
  nicht Step im Blocker-Job). Begründung: Blaupausen-Empfehlung; die Blaupause begründet
  zusätzlich ausdrücklich, warum die OS-Lane nicht im Blocker-Job laufen darf — ihre
  Laufzeit zählte dort gegen das Job-Timeout und ein Abbruch ließ selbst grüne PR-Checks
  rot werden / blockierte die Release-Kette.
- Job-Eigenschaften: `needs: detect-backmerge`, gleiche `if:`-Backmerge-Bedingung wie
  `build-and-test`, `runs-on: windows-latest`, `continue-on-error: true`,
  `timeout-minutes: 45` (Playwright-Suite deutlich langsamer als die Unit-Test-Lane;
  >30 min Puffer über Server-Readiness-Deadline und Timeouts).
- `staging-ci.yml`: `version`/`prerelease` bekommen `os-interface-tests` **nicht** in
  `needs` — ein OS-Fehlschlag darf die Pre-Release-Kette nie blockieren (Kommentar aus
  der Blaupause sinngemäß übernehmen).

### Entscheidung 2 – Coverage: OS-Lane wird aus dem 70-%-Gate ausgenommen

- Die OS-Lane führt `dotnet test` **ohne** `--collect` aus; `build-and-test` sammelt
  Coverage nur noch aus der regulären Lane (ReportGenerator-Pfad bleibt unverändert,
  der Glob `src/Schnittstellenzentrale.Tests/TestResults/**/coverage.cobertura.xml`
  greift weiterhin — es liegt dann nur noch eine Cobertura-Datei vor).
- Begründung: Blaupausen-Entscheidung; das Gate soll nicht nicht-deterministisch von
  einer best-effort-Lane abhängen. Lokal ist die reguläre Lane allein
  (`FullyQualifiedName!~Playwright`) mit **72,82 % Line-Coverage ≥ 70 %** gemessen.
- **Aber:** Durch das Tagging wandert `ImpressumServiceTests` (15 `[Fact]`s, deckt den
  `ImpressumService` inkl. Pfadauflösung/Sprachvarianten ab) in die OS-Lane — ihr
  Coverage-Beitrag entfällt. **Pflichtschritt im Plan:** nach dem Tagging die reguläre
  Lane mit `Category!=OsInterface` erneut messen und den Wert dokumentieren.
- **Harter Blocker / Fallback:** Fiele die reguläre Lane unter 70 %, wird als Plan-B der
  Artefakt-Merge umgesetzt (OS-Job lädt `coverage.cobertura.xml` als Artefakt hoch;
  `build-and-test` holt es via `actions/download-artifact` und ReportGenerator mergt
  beide — `needs:`-Kopplung wäre dann nötig, macht das Gate aber wieder von der
  best-effort-Lane abhängig → nur als Notlösung).

### Entscheidung 3 – Flake-Statistik: Blaupausen-Schwellwerte + Artefakt-Ketten-Persistenz

- Schwellwerte = Blaupausen-Startwerte (kalibrierbar als Skript-Parameter):
  Fenster `MaxRuns=20`; **Quarantäne-Kandidat**: ≥2 Flake-Ereignisse ODER ≥20 % Fehlrate
  bei ≥5 Ausführungen; **beobachten**: ≥1 Flake-Ereignis oder ≥10 % Fehlrate.
- Persistenz = **Artefakt-Kette** (keine committed Datei, keine Issue-Kommentare):
  ein best-effort-CI-Schritt lädt die `test-results-os-interface-*`-Artefakte der
  letzten ~15 Läufe **desselben Workflows** via `gh run list`/`gh run download` nach
  `trx-history/`; Fehler (fehlende Artefakte, API-Fehler) werden toleriert — der Schritt
  schlägt nie (`continue-on-error: true` + fehlertolerantes Skript). Der Analyzer wertet
  `src/Schnittstellenzentrale.Tests/TestResults` **und** `trx-history/` gemeinsam aus.
- Quarantäne-Issue: bei „Quarantaene-Kandidat" legt ein pwsh-Schritt per
  `gh issue create` ein Issue an — **dedupliziert** primär über die Titel-Suche
  `gh issue list --search "in:title \"Quarantäne: <fqn>\"" --state open` (robuster als
  die Volltext-Suche, die an Satzzeichen tokenisiert); der Body trägt zusätzlich den
  stabilen Marker `<!-- flaky-quarantine:<fqn> -->` als Beleg/Fallback im Issue-Text.
  Neu angelegt wird nur, wenn kein **offenes** Issue existiert (geschlossenes oder
  nicht existierendes Issue → neu).
- Fork-PR-Permissions: `issues: write`/`GH_TOKEN` ist bei PRs aus Forks wirkungslos
  (GITHUB_TOKEN dort read-only) — Quarantäne- und History-Download-Schritte laufen
  explizit **best-effort**: Skripte protokollieren den Fehler (Warnung) und enden mit
  Exit 0 statt zu werfen; `gh run download` bleibt ebenfalls tolerant (s. AP 4).
- Permissions: der OS-Job erhält `permissions: contents: read` (Checkout),
  `issues: write` (Issue anlegen/suchen), `actions: read` (`gh run list/download`).
  Job-Level-Permissions ersetzen die Workflow-Permissions — daher explizit vollständig
  setzen. `GH_TOKEN: ${{ github.token }}` auf Job-Env-Ebene.

### Entscheidung 4 – Port 5099: fest beibehalten

- `BindUrl` bleibt `http://127.0.0.1:5099` (+ Unterklassen 5100–5104). Begründung: die
  6 Collections serialisieren Tests bereits pro Fixture, die Ports sind bewusst
  gestaffelt; ein dynamischer Port (Port 0 + Rückgabe der effektiven URL an alle
  `PlaywrightTestBase`-Kontexte) wäre ein unverhältnismäßiger Umbau ohne realen
  Kollisionsbefund. Neu: der **Readiness-Poll** (AP 5) macht eine echte
  Nicht-Erreichbarkeit des festen Ports jetzt sichtbar statt implizit beim ersten
  `Page.GotoAsync` zu scheitern.

### Entscheidung 5 – Audit-Ergebnis: `inventory/audit-os-interface.md` ist final

- **40 OsInterface-Dateien:** 22 Playwright-Testdateien (24 Testklassen) + 17
  `Playwright/Infrastructure`-Dateien + `Services/ImpressumServiceTests.cs`.
- Getaggt werden nur **Testklassen** (die 24 Playwright-Klassen + `ImpressumServiceTests`
  = 25 `[OsInterface]`-Markierungen); die 17 Infrastructure-Dateien enthalten keine
  `[Fact]`/`[Theory]` und brauchen keinen Trait.
- Alle 17 `Integration/`-Dateien bleiben regulär (`WebApplicationFactory`=TestServer
  in-process ohne Socket; `Data Source=:memory:` deterministisch). Die Randbeobachtungen
  (`Task.Delay`-Zeitabhängigkeiten in `TokenStoreTests`, `ApplicationGroupsControllerIntegrationTests`,
  `EndpointExecutionServiceTests`) bleiben bewusst regulär — kein OS-Bezug.
- Zusatz aus dieser Planung: `TempDirectoryCleanupTests` (neu, AP 6) wird ebenfalls
  `[OsInterface]` getaggt — sie rührt echtes Temp-Dateisystem an und fällt damit unter
  dieselbe Heuristik wie `ImpressumServiceTests`.

### Ergänzende Planungsentscheidungen

**D1 – xUnit-v2-Variante: Convenience-Attribute + ein `CategoryTraitDiscoverer`, `[Trait]` bleibt erlaubt.**
Die Blaupausen-Attribute nutzen xUnit v3 (`Xunit.v3`, `ITraitAttribute.GetTraits`) und sind
nicht übernehmbar. Für xUnit 2.9.3 wird stattdessen das dokumentierte v2-Muster gewählt:
Attribute tragen `[TraitDiscoverer("Schnittstellenzentrale.Tests.Infrastructure.Testing.CategoryTraitDiscoverer", "Schnittstellenzentrale.Tests")]`;
ein einziger `CategoryTraitDiscoverer : Xunit.Sdk.ITraitDiscoverer` liest per
`IAttributeInfo.GetNamedArgument<string>("Category")` die `Category`-Property des jeweiligen
Attributs (v2 liest dort Property-Werte via Reflection — kein Ctor-Arg nötig).
**Kritisch:** Jedes Attribut implementiert zusätzlich `Xunit.Sdk.ITraitAttribute`
(memberloses Marker-Interface aus `xunit.core.dll`) — xUnit 2.9.3 sammelt Trait-Attribute
ausschließlich über `GetCustomAttributes(typeof(ITraitAttribute))`; ohne das Interface
erzeugt der Discoverer nie Traits und der `Category`-Filter findet 0 Tests. Die
v3-`GetTraits()`-Methode der Blaupause entfällt damit.
Attribute: `OsInterfaceAttribute` (Klasse+Methode), `OsInterfaceFactAttribute : FactAttribute`,
`OsInterfaceTheoryAttribute : TheoryAttribute` (Test-Discovery über vererbtes
`XunitTestCaseDiscoverer`, kein Zusatz nötig), `FlakyAttribute` (Klasse+Methode).
`TestCategories` liefert die const-Strings, damit rohes `[Trait("Category", TestCategories.OsInterface)]`
weiterhin erlaubt bleibt (Trait-Argumente müssen const sein — sind sie).
**Tagging-Strategie:** primär `[OsInterface]` auf **Klassen**ebene (eine Zeile pro Klasse,
deckt alle Methoden ab); `[OsInterfaceFact]`/`[OsInterfaceTheory]` für Einzelmethoden-Fälle;
rohes `[Trait]` bleibt zulässig. Die Meta-Tests erkennen **beide** Schreibweisen
(`TraitAttribute`-Instanzen direkt + Attribute mit `Category`-Property — identisch zur
Discoverer-Semantik, ohne xunit-interne `IAttributeInfo`-Wraps).

**D2 – CI-Detailentscheidungen:**
- OS-Test-Step läuft auf `Schnittstellenzentrale.slnx` mit `-c Release --no-build`
  (konsistent zum Blocker-Job; der Release-Build triggert wie bisher das
  `InstallPlaywright`-Target → Chromium-Installation; kein separater Install-Step).
- `Retry-FlakyTests.ps1` wird mit `-Configuration Release` aufgerufen (Skript-Default
  ist `Debug`).
- TRX-Name `test-results-os-interface.trx`; Upload-Glob `test-results-os-interface*.trx`
  deckt die Retry-Datei `test-results-os-interface-flaky-retry.trx` ab.
- `playwright-traces/`-Upload wandert von `build-and-test` in den OS-Job (Traces werden
  relativ zum Repo-Root des `dotnet test`-Laufs geschrieben — in der OS-Lane).

**D3 – Skript-Varianten: eigene Skriptdateien statt Inline-Steps.**
`Download-OsInterfaceTrxHistory.ps1` und `New-FlakyQuarantineIssue.ps1` werden als eigene
Dateien unter `scripts/` angelegt (lokal testbar, YAML bleibt dünn — analog zur
Blaupausen-Struktur). Übergabe Analyse→Quarantäne: der Analyzer erhält einen neuen
optionalen Parameter `-CandidatesFile <pfad>` und schreibt die Quarantäne-Kandidaten als
JSON (Erweiterung gegenüber der Blaupause, die nur Markdown-Report erzeugt) — das
Quarantäne-Skript parst JSON statt Markdown.

**D4 – `playwright-traces/` in `.gitignore` aufnehmen.**
Lokale Läufe erzeugen Trace-ZIPs im Repo-Root; der Artefakt-Upload erfolgt aus dem
CI-Workspace und ist davon unberührt → Eintrag ohne Nachteil.

**D5 – Diagnose-Kanal: `ITestOutputHelper` in `PlaywrightTestBase`.**
Die Basisklasse erhält `ITestOutputHelper` per Konstruktor (xUnit v2 injiziert sie in
Testklassen automatisch); alle 23 abgeleiteten Klassen bekommen den zusätzlichen
Ctor-Parameter mechanisch mitgezogen. Dispose-/Timeout-Diagnosen landen damit im
TRX-`<Output>`-Block statt im nicht-kapturierten `Console`. Serverseitige Phasenfehler
(`PlaywrightServer` ist Collection-Fixture, kein Output-Helper möglich) werden über
angereicherte Exception-Texte (Phase/State/letzter Fehler) transportiert.

---

## 4. Arbeitspakete (Checkliste)

### AP 1 – Trait-Infrastruktur (xUnit v2)

Neues Verzeichnis `src/Schnittstellenzentrale.Tests/Infrastructure/Testing/`:

- [ ] `TestCategories.cs` — `public const string OsInterface = "OsInterface"`,
      `public const string Flaky = "Flaky"` (Blaupause `TestCategories.cs` 1:1).
- [ ] `CategoryTraitDiscoverer.cs` — `public sealed class CategoryTraitDiscoverer : Xunit.Sdk.ITraitDiscoverer`
      (Interface aus `xunit.core.dll`, **nicht** `Xunit.Abstractions` — dort existiert
      kein Trait-Typ); Signatur
      `IEnumerable<KeyValuePair<string,string>> GetTraits(IAttributeInfo traitAttribute)`
      (`IAttributeInfo` aus `Xunit.Abstractions`); liest
      `traitAttribute.GetNamedArgument<string>("Category")` und liefert
      `("Category", <wert>)`.
- [ ] **Pflicht:** Alle vier Convenience-Attribute implementieren zusätzlich
      `Xunit.Sdk.ITraitAttribute` (memberloses Marker-Interface aus `xunit.core.dll`).
      xUnit 2.9.3 sammelt Trait-Attribute ausschließlich über
      `GetCustomAttributes(typeof(ITraitAttribute))` (filtert per `IsAssignableFrom`) —
      ohne das Interface wird dem Discoverer nie ein Attribut vorgelegt, es entsteht kein
      `Category`-Trait und `--filter "Category=OsInterface"` findet 0 Tests (stiller
      Totalausfall der Lane-Trennung).
- [ ] `OsInterfaceAttribute.cs` — `: Attribute, Xunit.Sdk.ITraitAttribute`,
      `[TraitDiscoverer("...CategoryTraitDiscoverer", "Schnittstellenzentrale.Tests")]`,
      `AttributeUsage(Class|Method)`, `public string Category => TestCategories.OsInterface;`
- [ ] `OsInterfaceFactAttribute.cs` — wie oben, aber `: FactAttribute, Xunit.Sdk.ITraitAttribute`,
      `AttributeUsage(Method)`.
- [ ] `OsInterfaceTheoryAttribute.cs` — wie oben, aber `: TheoryAttribute, Xunit.Sdk.ITraitAttribute`
      (vorsorglich — aktuell kein `[Theory]`-Anwender in der OS-Lane).
- [ ] `FlakyAttribute.cs` — `: Attribute, Xunit.Sdk.ITraitAttribute`,
      `[TraitDiscoverer("...CategoryTraitDiscoverer", "Schnittstellenzentrale.Tests")]`,
      `AttributeUsage(Class|Method)`, `Category => TestCategories.Flaky`, XML-Doku mit
      Vergaberegeln (nur mit OsInterface, nur bei dokumentiertem Flake, entfernen nach Fix).
- [ ] XML-Doku an allen öffentlichen Typen (`CS1591` ist `WarningsAsErrors` im Testprojekt).
- [ ] Smoke-Check: `dotnet test --list-tests --filter "Category=OsInterface"` listet
      nach dem Tagging exakt die getaggten Tests.

### AP 2 – Meta-Tests

- [ ] `Infrastructure/Testing/TestTraitReader.cs` — `internal static`, liest effektive
      Traits eines Members/Types: (a) `TraitAttribute`-Instanzen → `(Name, Value)`;
      (b) Attribute mit lesbarer `Category`-Property → `("Category", wert)` — spiegelt die
      `CategoryTraitDiscoverer`-Semantik. Plus Hilfsfunktion `HasOsInterface(MemberInfo)`.
- [ ] `Infrastructure/Testing/FlakyConventionTests.cs` — Assembly-Scan (Blaupause
      `FlakyConventionTests` adaptiert, mit `TestTraitReader`, plain `Assert` — das Repo
      hat **kein** FluentAssertions): jeder Typ/jede Methode mit `Category=Flaky` muss
      `Category=OsInterface` tragen (Klassen-Trait vererbt auf Methoden mitzählen).
- [ ] `Infrastructure/Testing/TraitParityTests.cs` — jeder Typ der Test-Assembly, dessen
      `FullName` `"Playwright"` enthält **und** der ≥1 Methode mit `FactAttribute`-Derivat
      besitzt, muss `Category=OsInterface` tragen — klassenweise oder auf jeder
      Testmethode. Verstoßliste in der Assert-Meldung ausgeben.
- [ ] Beide Meta-Testklassen bleiben bewusst **regulär** (kein OsInterface-Tag).

### AP 3 – Tagging der 25 Testklassen

`[OsInterface]` auf Klassenebene + `using Schnittstellenzentrale.Tests.Infrastructure.Testing;`:

- [ ] Alle 22 Dateien in `src/Schnittstellenzentrale.Tests/Playwright/` (24 Klassen —
      `ImpressumPageTests.cs` und `ImpressumPageWithLanguageTests.cs` enthalten je 2 Klassen;
      `SitePageTests.cs` mitzählen, obwohl ohne Collection).
- [ ] `src/Schnittstellenzentrale.Tests/Services/ImpressumServiceTests.cs` (einzige
      OsInterface-Datei außerhalb des Playwright-Namespace — reine FQN-Parität erfasst sie
      nicht, manuelle Zuordnung laut Audit).
- [ ] Kein Tagging in `Playwright/Infrastructure/` (keine Testmethoden), keine
      `Flaky`-Markierung beim initialen Rollout (gibt keinen dokumentierten Flake).
- [ ] Verbotsregel: `[OsInterface]` nicht gleichzeitig auf Klasse **und** Methode
      derselben Klasse — doppelte Trait-Einträge (funktional harmlos, Multi-Value-Dict,
      aber konventionswidrig); Klassenebene ist der Standard, Methodenebene nur für
      Einzelmethoden-Ausnahmen in sonst regulären Klassen.

### AP 4 – Retry-/Analyse-Skripte

- [ ] `scripts/lib/Trx.ps1` — 1:1 aus Blaupause.
- [ ] `scripts/lib/TestDiscovery.ps1` — 1:1 aus Blaupause (`Get-FlakyTestNames`, generisch).
- [ ] `scripts/Retry-FlakyTests.ps1` — Blaupause adaptiert: `param(TestProject, PrimaryTrx,
      Configuration="Debug", RetryTrxFileName)`; Beispiel-Pfade auf dieses Repo;
      Kommentar zu „Skip-Flag" generisch formulieren (keine `SOFTWARESCHMIEDE_SKIP_*`).
- [ ] `scripts/Analyze-FlakyTestResults.ps1` — Blaupause adaptiert:
      `SOFTWARESCHMIEDE_SKIP_*`-Erwähnungen → generisch („Skip/`NotExecuted`"); Empfehlungs-
      Text-Pfad `docs/help/stabilitaet/os-interface-tests.md` stimmt; **neu**:
      `-CandidatesFile`-Parameter → schreibt Quarantäne-Kandidaten als JSON-Array
      (`[{test, executions, failures, flakeEvents, failureRate}]`).
- [ ] `scripts/Download-OsInterfaceTrxHistory.ps1` — neu:
      `param(WorkflowFile, ArtifactName, Destination="trx-history", MaxRuns=15)`;
      `gh run list --workflow <file> --limit <n> --status completed --json databaseId`
      → pro Run `gh run download <id> -n <ArtifactName> -D <Destination>/<id>`;
      jeder Download einzeln try/catch; Exit immer 0.
- [ ] `scripts/New-FlakyQuarantineIssue.ps1` — neu: `param(CandidatesFile)`; liest JSON;
      pro Kandidat `gh issue list --search "in:title \"Quarantäne: <fqn>\"" --state open`;
      kein Treffer → `gh issue create --title "Quarantäne: <fqn>" --body <report+marker>`
      (Marker `<!-- flaky-quarantine:<fqn> -->` im Body als Beleg/Fallback); offener
      Treffer → Skip + Log. **Best-effort mit Log statt Wurf:** bei PRs aus Forks ist
      `issues: write`/`GH_TOKEN` wirkungslos (GITHUB_TOKEN read-only) — `gh`-Fehler
      werden toleriert (Warnung ausgeben, Exit 0), damit Fork-PRs keinen roten Step
      trotz funktionierendem Rest zeigen.
- [ ] Lokale Tests der Skripte gegen synthetische TRX-Dateien (s. Verifikation).

### AP 5 – Deadline-/Timeout-Härtung

- [ ] `Playwright/Infrastructure/PlaywrightServer.cs`:
      - Nach `await _app.StartAsync()` (Z. 135): `WaitForServerReadyAsync()` — Poll per
        `HttpClient` GET gegen `BindUrl` (echter Socket, nicht der In-Process-Handler),
        Intervall 500 ms, **Deadline 60 s** (> jeder Phasenbudget-Anteil, ≪ Job-Timeout
        45 min). **Akzeptanzkriterium: JEDE empfangene HTTP-Antwort** vom gebundenen
        Socket gilt als „ready" — Status egal (auch 404/3xx-Redirects zählen; die
        Readiness-Frage ist „Kestrel-Listener + Middleware-Pipeline beantworten
        Requests", nicht „Route existiert/2xx"). Nur Verbindungsfehler und
        Request-Timeouts zählen als nicht-bereit → Retry bis zur Deadline. Bei Timeout:
        Exception mit Phase, BindUrl, letztem Fehler.
      - Phasen-Diagnose: die Init-Phasen (Anchor-DB, `BuildWebApplicationAsync`,
        `StartAsync`, `OnAfterStartAsync`, Swagger-Warmup) bekommen Phasen-Marker;
        Fehler werden mit Phasen-Label erneut geworfen (Startfehler bleiben werfend —
        nur Dispose ist fehlertolerant).
      - `DisposeAsync` (Z. 158–167): `StopAsync`/`DisposeAsync`/`_apiFactory`/
        `_anchorConnection` je einzeln best-effort, Fehler gesammelt → `Console.Error`,
        **nie werfen**.
- [ ] `Playwright/Infrastructure/PlaywrightTestBase.cs`:
      - Ctor: `ITestOutputHelper`-Parameter (D5); alle ~23 abgeleiteten Ctors anpassen.
      - `InitializeAsync`: nach `NewContextAsync` explizit
        `Context.SetDefaultTimeout(30_000)` (Playwright-Default bewusst machen) und
        `Context.SetDefaultNavigationTimeout(60_000)` — Begründung: Blazor-Server-
        Erstaufruf (SignalR-Connect + Circuit) kann unter CI-Last >30 s liegen;
        60 s bleibt weit unter dem Job-Budget. Werte als dokumentierte Startwerte.
      - `DisposeAsync`: Trace-Stop via `Task.WhenAny(stopTask, Task.Delay(30_000))` —
        Budget-Überschreitung nur loggen. Die bei Timeout **verwaiste `stopTask`** wird
        beobachtet:
        `_ = stopTask.ContinueWith(t => { /* t.Exception loggen + schlucken */ }, TaskContinuationOptions.OnlyOnFaulted)` —
        sonst droht `UnobservedTaskException` und die Trace-ZIP kann verspätet unter dem
        Folge-Dispose geschrieben werden (`Tracing.StopAsync` kennt kein
        CancellationToken; `WhenAny` bleibt der richtige Grundansatz, nur die
        Restlebensdauer wird gesaubert). Trace-ZIP wird ggf. verspätet geschrieben —
        akzeptiert und dokumentiert. Danach `OnDisposingAsync`,
        `Context.DisposeAsync`, `_browser.DisposeAsync`, `_playwright.Dispose()`
        je einzeln try/catch, Fehler gesammelt → `ITestOutputHelper`/`Console.Error`;
        nie werfen (Browser-Dispose läuft unabhängig vom Trace-Stop-Ausgang weiter).
      - Zusatzkontexte: Hilfsmethode `StopTraceBestEffortAsync(context, name)` in der
        Basis; `SignalRSyncTests.OnDisposingAsync` (`SignalRSyncTests.cs:24–32`) nutzt sie.
- [ ] `Playwright/SitePageTests.cs` (kein `PlaywrightTestBase`): eigener Dispose-Pfad
      (Z. 39–44) gleiches Muster — jeder Dispose-Schritt best-effort + geloggte Fehler;
      Launch/Context ohne Timeouts → `SetDefaultTimeout` analog.

### AP 6 – Cleanup-Härtung

- [ ] `src/Schnittstellenzentrale.Tests/Helpers/TempDirectoryCleanup.cs` — neu, Blaupause
      adaptiert: `internal static void DeleteBestEffort(string root, bool normalizeAttributes = false)`
      (**ohne** `processesToKill` — hier gibt es keine Kindprozesse); 5 Versuche,
      Backoff `100 ms * attempt`, Restfehler nur `Console.Error`; optional
      `DeleteFileBestEffort(string path)` für die `File.Delete`-Stellen.
- [ ] `Helpers/TempDirectoryCleanupTests.cs` — Blaupause adaptiert (ohne Prozess-Test):
      fehlendes Verzeichnis → kein Throw; exklusiv geöffnete Datei → kein Throw;
      Read-only + `normalizeAttributes` → gelöscht; ggf. File-Variante. Klasse `[OsInterface]`
      (echtes Temp-FS, s. Entscheidung 5).
- [ ] Einsatz in allen `sz-impressum-*`-Cleanup-Pfaden: `PlaywrightImpressumServer.cs:61–65`,
      `PlaywrightImpressumLanguageServer.cs` (Cleanup ~Z. 66–70),
      `PlaywrightImpressumFallbackServer.cs` (Cleanup ~Z. 60–64) — `Directory.Delete` →
      `TempDirectoryCleanup.DeleteBestEffort`; auch im `catch` der `InitializeAsync`
      (Start-Fehlschlag) bleibt Cleanup werf-frei.
- [ ] `Services/ImpressumServiceTests.cs`: `Dispose` (Z. 20–24) → `DeleteBestEffort`;
      die beiden `File.Delete`-finallys (Z. 133–134, 157–158) → `DeleteFileBestEffort`
      bzw. try/catch → `Console.Error`.
- [ ] `Playwright/IconUploadTests.cs`: feste Temp-Namen `test-icon.png`/`test-icon.txt`
      (Z. 45, 68) → eindeutig `sz-icon-{Guid:N}.*`; `File.Delete` → best-effort.
      Nebenbei: der zweite Test (Z. 68–79) schreibt eine ungenutzte Datei — Schreiben
      entfernen oder eindeutigen Namen verwenden (Pfad ist für `FilePayload`-Upload
      ohnehin unnötig).
- [ ] `.gitignore`: `playwright-traces/` aufnehmen (D4; bei den MSTest-/`*.trx`-Einträgen
      ~Z. 64–71 einordnen).
- [ ] Randnotiz (optional/Follow-up): `Integration/EndpointExecutionIntegrationTests.cs:29–34`
      (`_factory.Dispose()` ohne Fehlerbehandlung) hat dasselbe Assert-Masking-Risiko,
      liegt aber in der regulären Lane — außerhalb des Issue-Kerns; bei Gelegenheit
      denselben best-effort-Griff ansetzen oder als Follow-up-Issue notieren.

### AP 7 – Workflow-Umbauten

**`.github/workflows/pr-staging-ci.yml`:**

- [ ] `build-and-test`: Step „Test" (Z. 144) Filter → `"Category!=OsInterface"` (Kommentar
      aktualisieren); Step „Test Playwright" (Z. 147–157) **entfernen**; Coverage-
      Report/Threshold-Steps unverändert (Glob greift weiter, nur reguläre Lane liegt vor);
      Kommentar beim Threshold anpassen (nur reguläre Lane speist Gate 5);
      Step „Upload Playwright traces" (Z. 201–208) entfernen (→ OS-Job).
- [ ] Neuer Job `os-interface-tests` (vor `back-merge-skip` einordnen):
      `needs: detect-backmerge`, `if: needs.detect-backmerge.outputs.is_backmerge != 'true'`,
      `continue-on-error: true`, `runs-on: windows-latest`, `timeout-minutes: 45`,
      `permissions: { contents: read, issues: write, actions: read }`,
      `env: GH_TOKEN: ${{ github.token }}`. Steps:
      1. `actions/checkout@v7`
      2. `actions/setup-dotnet@v6` (`10.0.x`)
      3. `dotnet restore Schnittstellenzentrale.slnx`
      4. `dotnet build Schnittstellenzentrale.slnx -c Release --no-restore` (triggert
         `InstallPlaywright` → Chromium; Kommentar wie im Blocker-Job)
      5. `dotnet test Schnittstellenzentrale.slnx -c Release --no-build --filter "Category=OsInterface" --logger "trx;LogFileName=test-results-os-interface.trx" --logger "console;verbosity=normal"` — **ohne** `--collect`
      6. `Retry flaky tests once` (`if: failure()`):
         `& scripts/Retry-FlakyTests.ps1 -TestProject src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj -PrimaryTrx src/Schnittstellenzentrale.Tests/TestResults/test-results-os-interface.trx -Configuration Release`
         (Kommentar: `if: failure()` bewusst breit — fehlendes TRX ⇒ neutral Exit 0)
      7. `Download OS-interface TRX history` (`if: always()` + `continue-on-error: true`):
         `& scripts/Download-OsInterfaceTrxHistory.ps1 -WorkflowFile pr-staging-ci.yml -ArtifactName test-results-os-interface-pr -Destination trx-history -MaxRuns 15`
      8. `Analyze flaky test results` (`if: always()`):
         `& scripts/Analyze-FlakyTestResults.ps1 -Path src/Schnittstellenzentrale.Tests/TestResults, trx-history -CandidatesFile flaky-quarantine-candidates.json`
      9. `Create quarantine issues` (`if: always()`):
         `& scripts/New-FlakyQuarantineIssue.ps1 -CandidatesFile flaky-quarantine-candidates.json`
         (no-op bei leerer/fehlender Datei; best-effort — Skript toleriert fehlende
         `issues: write`-Rechte auf Fork-PRs und endet mit Warnung/Exit 0)
      10. Upload `test-results-os-interface-pr` (`if: always()`, Pfad
          `src/Schnittstellenzentrale.Tests/TestResults/test-results-os-interface*.trx`)
      11. Upload `playwright-traces-pr` (`if: always()`, Pfad `playwright-traces/` —
          übernommen aus `build-and-test`)

**`.github/workflows/staging-ci.yml`:** identischer Umbau (Test-Step Z. 118 Filter,
„Test Playwright" Z. 121–124 entfernen, Traces-Upload Z. 167–178 → OS-Job, Artefakte mit
`-staging`-Suffix, `-WorkflowFile staging-ci.yml -ArtifactName test-results-os-interface-staging`).
**`version` (Z. 182) und `prerelease` (Z. 255) `needs:` unverändert lassen** — OS-Job nicht
aufnehmen; Kommentar mit Blaupausen-Begründung (OS-Fehlschlag darf die Release-Kette nie
blockieren).

**`.github/workflows/release.yml`:** Z. 106 Filter → `"Category!=OsInterface"`;
Begleitkommentar (Z. 101–102) an die Trait-Konvention anpassen.

### AP 8 – Doku

- [ ] `docs/help/stabilitaet/index.md` + `docs/help/stabilitaet/os-interface-tests.md` —
      neu; Blaupause an Repo-Realität anpassen: Klassifikationskriterium = Playwright/
      Chromium, echte Kestrel-Bindung (Ports 5099–5104), Temp-Dateisystem
      (`sz-impressum-*`, `playwright-traces`), echter SignalR-Roundtrip — statt
      ConPTY/FlaUI/Clipboard; lokale Befehle mit `Category`-Filtern; `Flaky`-Konvention
      inkl. beider Schreibweisen (`[OsInterface]`/`[OsInterfaceFact]`/`[Trait]`);
      Retry-/Rerun-Konvention (`gh run rerun --job <id>` nur OS-Job, nie `--failed`);
      Flake-Statistik + Quarantäne-Issues; **keine** `Run-AllTestsIndividually`-/
      `SOFTWARESCHMIEDE_SKIP_*`-Referenzen; „Bekannte Fehlerbilder" auf Playwright-Traces/
      `npx playwright show-trace` zeigen. Audit-Abschnitt: die Klassifikationsheuristik
      („echte OS-Berührung ⇒ `[OsInterface]`") als Kriterium für künftige Tests benennen —
      inkl. transparentem Hinweis auf das Restrisiko, dass neue umgebungssensitive Tests
      **ohne** „Playwright" im Namen vom `TraitParityTests`-Meta-Test nicht erfasst werden
      und manuell getaggt werden müssen (s. Risiken).
- [ ] `docs/help/index.md` — Eintrag `stabilitaet/` unter „Qualität und Tests".
- [ ] `CLAUDE.md` (Z. 22): `FullyQualifiedName!~Playwright` → `Category!=OsInterface`;
      Kurzhinweis auf die OS-Lane (`Category=OsInterface`, best-effort in CI).
- [ ] `README.md` (~Z. 170/176): `FullyQualifiedName!~Playwright` → `Category!=OsInterface`
      bzw. `FullyQualifiedName~Playwright` → `Category=OsInterface`; Kurzhinweis auf den
      `os-interface-tests`-Job (best-effort in CI).
- [ ] `.claude/css-conventions.md` (~Z. 40): `FullyQualifiedName!~Playwright` →
      `Category!=OsInterface`.
- [ ] `docs/help/playwright-tests/installation.md`: Filter-Konvention (~Z. 110) auf
      `Category=OsInterface` umstellen + Hinweis auf den `os-interface-tests`-Job.
- [ ] `docs/help/playwright-tests/index.md`: Coverage-Aussage anpassen (Playwright-
      Coverage fließt nicht mehr ins 70-%-Gate).

### AP 9 – Mess- und Konsolidierungsschritte

- [ ] Testmengen-Baseline vor Tagging: `dotnet test --list-tests` mit altem
      `FullyQualifiedName`-Filter (beide Richtungen, Zählen).
- [ ] Nach Tagging: `--list-tests`/`--filter "Category=OsInterface"` bzw. `!=` —
      Mengenabgleich (s. Akzeptanzszene A1).
- [ ] Coverage-Nachmessung reguläre Lane (`Category!=OsInterface`, XPlat Coverage +
      ReportGenerator) — Wert dokumentieren (Ziel ≥70 %, s. Entscheidung 2/Fallback).
- [ ] Lokal grün: `dotnet test --filter "Category!=OsInterface"`.

---

## 5. Akzeptanzszenarien (prüfbar)

- **A1 – Filter-Parität:** `--list-tests --filter "Category=OsInterface"` =
  Menge alter Filter `FullyQualifiedName~Playwright` **plus** `ImpressumServiceTests`
  (15 Methoden) **plus** `TempDirectoryCleanupTests` (neu, AP 6 — Anzahl seiner
  `[Fact]`s zum Zeitpunkt der Messung zählen); `--filter "Category!=OsInterface"` =
  alter Blocker-Filter **minus** `ImpressumServiceTests` **plus** die neuen regulären
  Meta-Tests (AP 2). Beide Zahlen dokumentiert (AP 9).
- **A2 – Meta-Tests grün:** `FlakyConventionTests` und `TraitParityTests` laufen in der
  regulären Lane und schlagen bei Verstoß (Kontrollverstoß lokal einmal verifiziert:
  Testklasse ohne Tag unter `Playwright/` → Parity-Test rot).
- **A3 – Lane-Trennung in CI:** Ein OS-Job-Fehlschlag lässt PR-Check/Gesamtlauf grün
  (`continue-on-error` auf Job-Ebene); `version`/`prerelease` in `staging-ci.yml`
  hängen nicht am OS-Job. Fehlschlag der regulären Lane bleibt blockierend.
- **A4 – Coverage ≥70 %:** `Enforce coverage threshold` in `build-and-test` grün;
  gemessener Wert der rein regulären Lane dokumentiert (≥70 %; andernfalls
  Fallback-Entscheidung greift → Blocker).
- **A5 – Retry-Konvention:** `Retry-FlakyTests.ps1` lokal gegen synthetische TRX
  getestet: (a) fehlendes TRX → Exit 0, (b) Fehlschlag ohne Flaky-Treffer → Exit 0,
  (c) Flaky-Fehlschlag → genau ein gefilterter Re-Run + korrektes TRX-Naming.
  Der Analyzer klassifiziert gegen synthetische Multi-Run-TRX-Sets korrekt
  (Quarantäne-Kandidat/beobachten/stabil) und schreibt `-CandidatesFile`-JSON.
- **A6 – Workflow-YAML valide:** beide Workflows + `release.yml` parsebar
  (`actionlint` oder `gh workflow`/`yq`-Check), OS-Job in beiden identisch bis auf
  benannte Unterschiede.
- **A7 – Cleanup-Masking weg:** `TempDirectoryCleanupTests` deckt Lock-/Missing-/
  Read-only-Fälle ab; kein `Directory.Delete`/`File.Delete` im Testprojekt mehr ohne
  best-effort-Hülle (Volltextsuche).
- **A8 – `Flaky` nur mit `OsInterface`:** Konventionstest deckt beide Schreibweisen ab;
  initiale Flaky-Menge ist leer (`--list-tests --filter "Category=Flaky"` → 0).
- **A9 – Quarantäne-Dedupe:** `New-FlakyQuarantineIssue.ps1` legt bei Kandidat nur ein
  Issue an (Dedupe primär über Titel-Suche `in:title "Quarantäne: <fqn>" --state open`);
  Wiederholungslauf mit offenem Issue → kein Duplikat; geschlossenes Issue → neu;
  simulierter Rechte-Fehler (`gh`-Fehlschlag) → Warnung + Exit 0 statt Step-Rot
  (lokal gegen Test-Repo oder mit `GH_TOKEN`-Dry-Run verifiziert).
- **A10 – Doku konsistent:** `CLAUDE.md`, `README.md`, `.claude/css-conventions.md`,
  `docs/help/stabilitaet/` + `installation.md` zeigen die Trait-Filter; kein Verweis
  mehr auf den FQN-Filter als Konvention (Volltextsuche `FullyQualifiedName` in
  Doku-Dateien).

---

## 6. Risiken

| Risiko | Bewertung / Mitigation |
|---|---|
| **Coverage-Einbruch <70 %** durch `ImpressumServiceTests`-Umzug | Mittel. Lokal 72,82 % regulär — Puffer ~2,8 pp. Nachmessung ist Pflichtschritt (AP 9); unter 70 % → Artefakt-Merge-Fallback (Entscheidung 2) oder Schwellwert-Herleitung anpassen — bewusst als Blocker behandeln, nicht still drüberbügeln. |
| **xUnit-v2-TraitDiscoverer funktioniert nicht wie erwartet** (z. B. `GetNamedArgument` liest Property nicht) | Niedrig–mittel. V2-Muster ist dokumentiert; früh mit einem einzelnen getaggten Test verifizieren (`--list-tests --filter "Category=OsInterface"`), bevor alle 25 Klassen getaggt werden. Fallback: rohes `[Trait("Category", TestCategories.OsInterface)]` + einfaches Attribut-typ-Mapping im `TestTraitReader`. |
| **Theory-/Datenzeilen-Normalisierung im Retry** (FQN mit `(arg: …)`) | Niedrig. Skripte aus der Blaupause normalisieren bereits auf Methoden-FQN; lokaler Test gegen echtes Playwright-TRX vor CI-Rollout. |
| **Readiness-Poll 60 s zu knapp** auf stark geladenem Runner | Niedrig. Startwert, kalibrierbar; Fehlertext enthält Phase/URL/letzten Fehler → Diagnose statt stillem Flake. Job-Timeout 45 min bleibt unangetastet. |
| **`timeout-minutes: 45` trotzdem zu klein** für die volle Playwright-Suite | Mittel. Bisher teilte sich die Suite das 30-min-Budget des `build-and-test`-Jobs inkl. Build — die OS-Lane allein (ohne Coverage-Collection) sollte deutlich darunter liegen; erste CI-Läufe beobachten, ggf. erhöhen. |
| **History-Download `gh run download` liefert keine Artefakte** (Retention 14 d, alte Runs ohne OS-Artefakt) | Niedrig. Schritt ist best-effort (toleriert Fehler); Statistik fällt dann auf „nur aktueller Lauf" zurück — Analyzer funktioniert auch mit einem einzelnen TRX. |
| **TraitParity-Restrisiko:** neue umgebungssensitive Tests **ohne** „Playwright" im `FullName` landen weiterhin unerkannt in der Blocker-Lane (`TraitParityTests` erzwingt nur FQN-Parität; `ImpressumServiceTests` wurde nur per Audit, nicht per Meta-Test erfasst) | Akzeptiert — **issue-konform** (Audit-Heuristik + Meta-Test exakt wie im Issue gefordert), aber transparent dokumentiert: Der Audit-Abschnitt in `docs/help/stabilitaet/os-interface-tests.md` nennt die Heuristik („echte OS-Berührung ⇒ `[OsInterface]`") als Klassifizierungskriterium für künftige Tests; optional späterer härterer Meta-Test auf `Path.GetTempPath`/Kestrel-Heuristiken als Follow-up. |
| **Fork-PRs:** `issues: write`/`GH_TOKEN` wirkungslos (GITHUB_TOKEN dort read-only) — `gh issue create` scheitert | Niedrig. Quarantäne-/Download-Schritte explizit best-effort (Warnung + Exit 0 statt Wurf) — Fork-PRs zeigen keinen roten Step trotz funktionierendem Rest. |
| **Quarantäne-Spam** bei Schwellwert-Fehlkalibrierung | Niedrig. Dedupe über Titel-Suche (`in:title "Quarantäne: <fqn>"` auf offene Issues) + Body-Marker als Beleg; nur „Quarantaene-Kandidat" löst aus; Schwellwerte = bewährte Blaupausen-Startwerte. |
| **`continue-on-error` am Job lässt OS-Regressionen unbemerkt** | Mittel. Analyzer-Step (`if: always()`) + Quarantäne-Issues + TRX/Trace-Artefakte halten Sichtbarkeit; Rerun-Konvention dokumentiert. |
| **Concurrency `cancel-in-progress`** verwirft Wiederholungsläufe bei Push | Beachtet: Verifikationsläufe seriell bzw. per `gh run rerun --job` auf denselben Run auslösen (s. Verifikation). |
| **`ITestOutputHelper`-Ctor-Umbau an 24 Klassen** | Niedrig. Mechanisch, Compiler-sicher; alle Dateien werden für das Tagging ohnehin angefasst. |
| **YAML-Drift zwischen den zwei Workflows** | Niedrig. Identischer Umbau in einem Arbeitspaket; `actionlint`-Validierung. |

---

## 7. Verifikation

### Lokal (vor Push)

- [ ] `dotnet build Schnittstellenzentrale.slnx -c Release` sauber (kein `--no-build`
      bei ersten Testläufen — Build vorher verifizieren).
- [ ] `dotnet test --filter "Category!=OsInterface"` grün; `--filter "Category=OsInterface"` lokal
      (Playwright headful) grün.
- [ ] `--list-tests`-Mengenabgleich (A1) + Meta-Tests grün (A2) + Kontrollverstoß
      einmal getestet.
- [ ] Coverage-Nachmessung dokumentiert (Entscheidung 2).
- [ ] Retry-/Analyze-/Download-/Quarantäne-Skripte gegen synthetische TRX-Verzeichnisse
      getestet (A5/A9) — Fehlschlag→Retry-grün, Fehlschlag→Folgelauf-grün, persistent.
- [ ] `dotnet format Schnittstellenzentrale.slnx --verify-no-changes --severity error` sauber
      (Gate 1); `dotnet build -c Release -p:TreatWarningsAsErrors=true` sauber (Gate 3,
      XML-Doku `CS1591`!).
- [ ] Workflow-YAMLs lokal validiert.

### CI (nach Push, auf dem PR gegen `staging`)

- [ ] ≥3 komplette `pr-staging-ci.yml`-Durchläufe auf dem PR — **seriell** wegen
      `concurrency.cancel-in-progress` (leerer Commit je Runde, oder `gh run rerun`
      auf denselben Run). Fail-fast: pro Runde auf `fail` prüfen, nicht blind warten
      (`gh pr checks --watch --fail-fast`).
- [ ] Pro Lauf prüfen: `build-and-test` grün + Coverage-Gate bestanden; `os-interface-tests`
      läuft vollständig durch (Status ggf. „failure" aber Lauf grün = erwartetes
      best-effort-Verhalten); Artefakte `test-results-os-interface-pr` +
      `playwright-traces-pr` vorhanden; Analyzer-Report im Step-Log; History-Download-
      Schritt tolerant.
- [ ] **`staging-ci.yml`: deklarierte Abweichung von FR 11.** FR 11/Akzeptanzkriterium
      verlangt ≥3 komplette Durchläufe **je Workflow**; `staging-ci.yml` triggert aber
      erst auf Push nach `staging` — d. h. **post-merge**; drei Läufe sind in diesem
      Arbeitslauf faktisch nicht abschließbar. **Post-Merge-Verifikationsplan
      (verbindlich):** die ersten ≥3 `staging-ci.yml`-Läufe nach Merge beobachten —
      OS-Job läuft vollständig durch (best-effort-Verhalten incl. `continue-on-error`
      greift), Artefakte `test-results-os-interface-staging`/`playwright-traces-staging`
      vorhanden, `version`/`prerelease` unbeeinflusst (OS-Job nicht in `needs`). Die
      Abweichung wird im Abschlussbericht/PR-Text als dokumentierte Anpassung des
      Akzeptanzkriteriums mit Nachverfolgung benannt — keine stillschweigende
      Verwässerung.
- [ ] `release.yml`-Pfad kann vor Release nicht vollständig getestet werden —
      Syntax-Validierung + Kommentar; Filter-Konsistenz ist identisch zur PR-Lane.
