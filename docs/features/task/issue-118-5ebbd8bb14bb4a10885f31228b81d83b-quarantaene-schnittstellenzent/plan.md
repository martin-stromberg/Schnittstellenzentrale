# Umsetzungsplan – Issue #118: Quarantäne `ODataEndpointExecutionTests.OdataApi_Registrieren_Importieren_UndAlleEndpunkteErfolgreichAusfuehren`

Stand: 2026-11-25, überarbeitet nach `plan-check.md` (Root-Cause-Mechanik verifiziert und
korrigiert; der geplante Fix selbst wurde als korrekt bestätigt und bleibt unverändert).
Befundlage: `inventory.md` + `inventory/ci-evidence.md` (vollständig gesichert: 2
fehlschlagende Runs 45434716126 + 45543708029, identische Fehlersignatur, Trace-DOM-Beleg)
sowie Decompile-/Reflexions- und Laufzeit-Reproduktionsbefunde aus `plan-check.md`.

## 1. Ziel/Zusammenfassung

Der Playwright-Test schlägt intermittierend mit einem Strict-Mode-Verstoß fehl
(`.sz-tree-item-btn` HasText `"SZ-OData-E2E"` → 2 Elemente, `ODataEndpointExecutionTests.cs:47`,
analog Z. 135). Zwei verifizierte Mechanismen wirken zusammen:

1. **Deterministisch wirkungsloser Reset:** `TestDatabaseSeeder.ResetAsync()` leert die
   geteilte Shared-Cache-In-Memory-SQLite der Collection-Fixture **nie** —
   `EnsureDeletedAsync` ist auf `Mode=Memory`-DSNs ein buchstäblicher No-op
   (Details §2). Alt-Daten aller vorherigen Facts **aller** Collections liegen daher
   immer vor, nicht nur gelegentlich.
2. **DOM-/Render-Race als eigentlicher Intermittenz-Auslöser:** Die Strict-Assertion an
   Z. 47 resolved typischerweise in ~1 ms zu **einem** Element — bevor das Re-Render des
   Baums (Neuanlage → SignalR `TreeChanged` → `LoadDataAsync`) die Duplikat-Zeile des
   Alt-Eintrags ins DOM gehängt hat. Nur wenn das Re-Render schneller landet (CI-Last)
   resolved der Locator zu 2 Elementen → Sofort-Fehlschlag. Danach kaschieren
   `.First`-Locators und die bereits mit importierten OData-Endpunkten des Vortests
   belegte Alt-Anwendung den Rest des Tests „grün" gegen die falsche Anwendung.

Ziel ist ein **deterministischer Datenbank-Reset pro Fact** (Root-Cause-Fix in der
Test-Infrastruktur), sodass kein Fact mehr Daten eines vorherigen Facts sieht. Es liegt kein
Produktdefekt vor (Fehlschlag vor OData-Import/Endpunkt-Ausführung); es wird daher nur
Test-/Isolationsinfrastruktur geändert, plus Dokumentation des neuen Fehlerbilds in der
Stabilitätsdoku. Keine `[Flaky]`-Markierung — die Entscheidungsregel („persistente Störung →
Ursache beheben, kein `[Flaky]`") greift, weil die Ursache ein behebbarer
Testisolations-Defekt ist.

## 2. Scope-Klärung

- **Verarbeitungseinheit:** `TestDatabaseSeeder.ResetAsync()`, aufgerufen aus
  `PlaywrightTestBase.InitializeAsync` (`PlaywrightTestBase.cs:69`) vor jedem Fact aller
  Playwright-Collections (Ports 5099–5104). Der Fix wirkt damit global auf alle 24
  Playwright-Testklassen, nicht nur auf `ODataEndpointExecutionTests`.
- **Auslösendes Ereignis:** Jeder Fact nach dem ersten innerhalb einer Collection-Fixture —
  die benannte Shared-Cache-In-Memory-DB (`Data Source={guid};Mode=Memory;Cache=Shared`,
  `PlaywrightServer.cs:69-72`) enthält ab da garantiert Alt-Daten (Fact-Reihenfolge nicht
  fixiert, kein Orderer — vgl. `inventory/ci-evidence.md` §5). Ob ein Fact dennoch grün
  wird, entscheidet allein das DOM-/Render-Timing der Duplikat-Assertion, nicht der Daten-
  zustand.
- **Erwartete Wirkung:** Nach `ResetAsync` ist die DB garantiert leer bzw. enthält nur die
  definierten System-Einträge (Systemgruppe/-app + `POST /authenticate`), unabhängig davon,
  welche Verbindungen zum Reset-Zeitpunkt noch offen sind.
- **Tatsächliche Reset-Mechanik (Root Cause, verifiziert per Decompile + Reflexion +
  Laufzeit-Repro auf EF Core / Microsoft.Data.Sqlite 11.0.0):**
  `db.Database.EnsureDeletedAsync()` → EF-Core-`SqliteDatabaseCreator.Delete()` liest
  `DbConnection.DataSource`. Für eine geöffnete `Mode=Memory`-Verbindung liefert das
  `sqlite3_db_filename` = **`""` (leer)** — der `if (!string.IsNullOrEmpty(path))`-Zweig,
  der `SqliteConnection.ClearAllPools()` + `File.Delete(<path>)` enthält, wird **gar nicht
  betreten**. Auch der `else-if (State == Open)`-Zweig ist unerreichbar, weil das `finally`
  die Verbindung bereits geschlossen hat. **`Delete()` ist auf diesem DSN ein
  deterministischer No-op.**
  Zusätzlich: `SqliteConnectionFactory.GetPoolGroup` markiert Verbindungen mit
  `Mode == SqliteOpenMode.Memory` als **`IsNonPooled`** (per Reflexion auf 11.0.0 bestätigt:
  `IsNonPooled=True`). Für diesen Connection-String existiert **kein** `SqliteConnectionPool`;
  `ClearAllPools()` iteriert Pool-Gruppen und würde dort nichts tun (`_pool == null` →
  No-op). Es gibt damit **keine „Race mit ausgeliehenen Verbindungen" beim Pool-Clearing** —
  es gibt schlicht kein Clearing.
  Laufzeit-Reproduktion (2026-11-25): Mit offener Anchor-Connection überleben Daten
  `EnsureDeletedAsync` **immer** (`Rows after EnsureDeleted: 1`, deterministisch); die
  benannte Shared-Cache-DB stirbt erst beim Schließen der letzten Verbindung —
  Anchor-Dispose am Ende der Collection (`no such table` danach). `EnsureCreatedAsync`
  sieht anschließend die vorhandenen Tabellen (`Exists()`/`HasTables()` = true) und wird
  ebenfalls No-op. **Konsequenz:** Daten akkumulieren deterministisch über alle Facts
  aller Playwright-Collections; `SeedSystemEndpointsAsync` legt `POST /authenticate`
  **bedingungslos** pro Reset an (`TestDatabaseSeeder.cs:224-230`) → dieser Endpunkt
  dupliziert sich pro Fact.
- **Tatsächliche Intermittenz (verifiziert):** Der Strict-Mode-Verstoß ist eine
  **DOM-/Render-Race**, keine Reset-Race. `Expect(...).ToBeVisibleAsync()` resolved im
  lokalen Trace `playwright-traces/ODataEndpointExecutionTests-091350503.zip` ~1,3 ms nach
  Start zu **einem** Element — das Re-Render (Neuanlage → `TreeChanged` → `LoadDataAsync`)
  hatte die neue Duplikat-Zeile noch nicht ins DOM gehängt. Scheitert der erste Poll erst
  nach dem Re-Render, stehen 2 Elemente da → Sofort-Fehlschlag (kein Timeout, ~2 s
  Testlaufzeit, `inventory/ci-evidence.md` §3). Beleg, dass Alt-Daten **immer** da sind:
  Der Basis-HTML-Snapshot eines *grünen* zweiten Facts aus dem lokalen Lauf vom 25.11.
  (`ODataEndpointExecutionTests-091350503.zip`) zeigt die Alt-Anwendungen `SZ-OData-E2E`
  und `Filter-Test-App` als `.sz-tree-item-btn` im DOM. Die CI-Statistik „meist grün,
  gelegentlich rot, aufeinanderfolgend" erklärt sich ausschließlich über das Render-Timing.
- **Umgang mit noch nicht vorhandenen Objekten:** Frische DB (erster Fact nach Server-Start)
  → `EnsureCreatedAsync` legt das Schema an; der Daten-Wipe ist dann ein harmloser No-op.
  `SystemEntryInitializer.InitializeAsync` ist idempotent (legt Systemgruppe/-app nur bei
  Fehlen an, `src/Schnittstellenzentrale/SystemEntryInitializer.cs` Z. 19-66).

### Scope-Ausschlüsse

| Ausschluss | Status | Begründung |
|---|---|---|
| Produktcode-Änderung außerhalb Test-Infrastruktur | `bestätigt` | Kein Produktdefekt belegt — Fehlschlag an `ODataEndpointExecutionTests.cs:47`, vor OData-Import und Endpunkt-Ausführung (ci-evidence §6). |
| `[Flaky]`-Markierung | `bestätigt` | Entscheidungsregel der Anforderung: behebbare Ursache → fix statt markieren. `[Flaky]` wäre nur bei dokumentiertem, nicht behebbarem Flake-Befund zulässig. |
| Aufteilung des ~30-Schritte-Monolithen | `bestätigt` | In der Anforderung ausdrücklich optional; für den Fix nicht erforderlich. |
| Eindeutige Entitätsnamen pro Test / `.First`-Abschwächung der Strict-Assertions | `bestätigt` | Bewusste Wahl: Die ungefilterte Strict-Assertion an Z. 47/Z. 135 bleibt erhalten und fungiert nach dem Infrastruktur-Fix als Isolations-Tripwire — ein erneutes Alt-Daten-Leck würde sofort wieder sichtbar statt kaschiert. Namen wie `SZ-OData-E2E`/`Filter-Test-App` bleiben unverändert. |
| `ExpandWithRetryAsync` auf die blinden Chevron-Doppelklicks (Z. 56-60, 78-79, 98-99, 155-156, 165-166) | `bestätigt` | Nicht der beobachtete Fehler (Fehlschlag vor jedem Chevron-Klick), aber dokumentiertes Restrisiko (#112-Muster). In diesem PR umgesetzt — die Issue-Beschreibung benennt das Muster ausdrücklich als „naheliegende erste Maßnahme" (Bestätigung 2026-11-25 aus Anforderungstext). |
| Refaktorierung anderer Playwright-Tests (`ODataImportTests`, `EndpointExecutionTests`-Blindklicks) | `bestätigt` | Sie profitieren vom globalen Reset-Fix ohne eigene Änderung; generelle Suite-Refaktorierung ist Nicht-Ziel. |
| Änderungen an Analyzer, Schwellen, CI-Charakter | `bestätigt` | Explizites Nicht-Ziel der Anforderung. |

## 3. Akzeptanzszenarien

1. **Happy Path (beobachtete Fehlerreihenfolge):** Fact
   `OdataApi_GetEndpoints_MitContainsFilter_…` läuft zuerst und legt `SZ-OData-E2E`,
   `Filter-Test-App` und 3 geseedete Endpunkte an. Anschließend startet
   `OdataApi_Registrieren_…`: `ResetAsync` entfernt sämtliche Daten deterministisch; nach
   „Speichern" der neuen Anwendung resolved `.sz-tree-item-btn` HasText `"SZ-OData-E2E"` zu
   **genau einem** Element (Z. 47 grün); OData-Import, Chevron-Expand und die drei
   Endpunkt-Ausführungen (Authenticate/`token`, GET ApplicationGroups/`isSystem`, GET
   Applications/`isSystem`) laufen bis zum Ende durch.
2. **Umgekehrte Reihenfolge:** `Registrieren` zuerst, dann `GetEndpoints` — die gleiche
   Strict-Assertion an Z. 135 resolved ebenfalls genau ein Element; der Filter-Test läuft
   komplett grün.
3. **Wiederholbarkeit:** Mindestens 3–5 aufeinanderfolgende lokale Läufe von
   `FullyQualifiedName~ODataEndpointExecutionTests` sind vollständig grün (die
   Fact-Reihenfolge variiert natürlich über die Läufe).
4. **Globale Isolation:** Nach `ResetAsync` existieren keine Datensätze mehr — auch nicht
   in `EndpointCallHistory`, dem belegten Hauptleck des bisherigen
   `ClearNonSystemDataAsync` (dessen `SetNull`-FKs `AppDbContext.cs:188,192` lassen
   Historieneinträge mit genullten Verweisen zurück), und auch nicht in künftigen Entities
   ohne Kaskaden-/Repository-Abdeckung; `POST /authenticate` existiert danach exakt einmal.
5. **Reguläre Lane unverändert grün:** `Category!=OsInterface` inkl. Meta-Tests
   (`FlakyConventionTests`, `TraitParityTests`) bleibt bestehen.
6. **Dokumentation:** „Bekannte Fehlerbilder" in `docs/help/stabilitaet/os-interface-tests.md`
   enthält einen Eintrag für das Alt-Daten-/Strict-Mode-Duplikat mit korrekter
   Mechanik-Beschreibung und Verweis auf Issue #118.

## 4. Umsetzungsschritte

1. **`TestDatabaseSeeder.ResetAsync` deterministisch machen**
   (`src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/TestDatabaseSeeder.cs`, Z. 28-39):
   - `EnsureDeletedAsync()` entfernen (nachweislich wirkungsloser No-op auf
     `Mode=Memory`-DSNs, s. §2 — kein File-Backing, `DataSource` leer, Verbindungen
     non-pooled).
   - `EnsureCreatedAsync()` beibehalten (legt Schema auf frischer DB an; No-op auf
     bestehender).
   - Neue private Methode `ClearAllDataAsync(AppDbContext db)`: Verbindung explizit öffnen
     (`db.Database.OpenConnectionAsync()`), `PRAGMA foreign_keys = 0`, dann
     `DELETE FROM "<Tabelle>"` für jede Tabelle aus
     `db.Model.GetEntityTypes()` → `GetTableName()` (Distinct, null-gefiltert — modellgetrieben,
     deckt alle 10 DbSets plus künftige Entities ab; Tabellen ohne DbSet und
     `__EFMigrationsHistory` bleiben zu Recht außen vor), danach `PRAGMA foreign_keys = 1`;
     Verbindung im `finally` schließen. Wichtig: `foreign_keys` ist ein per-Connection-Pragma
     und muss außerhalb einer Transaktion laufen — bei non-pooled Verbindungen ginge es sonst
     zwischen zwei `ExecuteSqlRawAsync`-Aufrufen verloren (Autocommit je Aufruf ist gegeben,
     solange keine explizite Transaktion geöffnet wird).
   - Xml-Doc/Kommentar an `ResetAsync`: kurz begründen, warum nicht `EnsureDeletedAsync`
     (`EnsureDeletedAsync` ist auf `Mode=Memory`-DSNs wirkungslos: kein File-Backing →
     `Delete()` erreicht weder `ClearAllPools` noch `File.Delete`; `Mode=Memory`-Verbindungen
     sind non-pooled; die offene Anchor-Connection hält die benannte Shared-Cache-DB bis
     Collection-Ende am Leben — Daten überleben den Aufruf deterministisch).
   - Den veralteten Kommentar an `SeedShowcaseDataAsync` (Z. 44-49: „`EnsureDeletedAsync`
     wirkt nicht zuverlässig …") aktualisieren: Der Befund ist schärfer — der Reset wirkt
     **nie**, jetzt ist er deterministisch; der dortige `ClearNonSystemDataAsync`-Aufruf
     darf als defensiver Zusatz bestehen bleiben oder entfernt werden (nach dem
     vollständigen Wipe redundant — bei Umsetzung bevorzugt entfernen, damit nur eine
     Clear-Mechanik existiert).
   - Verworfene Alternative innerhalb dieses Schritts: Wipe über die Repositories
     (`ClearNonSystemDataAsync`-Muster) — verworfen, weil repository-basiert direkt nur
     **3 von 10 Tabellen** abdeckt (Nicht-System-Gruppen, ungruppierte Nicht-System-Apps,
     Umgebungen). Kaskaden decken zwar Endpunkte, Endpunktgruppen, Header und
     Query-Parameter mit ab; das belegte Hauptleck ist jedoch `EndpointCallHistory` —
     dessen `SetNull`-FKs (`AppDbContext.cs:188,192`) lassen Historienzeilen mit genullten
     Verweisen zurück — und jede künftige Entity ohne explizite Abdeckung wäre
     nachrüstpflichtig. Der modellgetriebene `DELETE FROM` ist tabellenagnostisch und
     zukunftssicher.
   - Verworfene Alternative: Schema per `DROP TABLE` aller Tabellen neu aufbauen — verworfen,
     weil `DELETE FROM` gleichwertig leert, aber Schema-Recreation-Timing (EnsureCreated vs.
     offene Server-Verbindungen) gar nicht erst zum Thema macht.

2. **`ExpandWithRetryAsync`-Muster auf `ODataEndpointExecutionTests` anwenden**
   (Entscheidung getroffen: in diesem PR umgesetzt, s. §7;
   `src/Schnittstellenzentrale.Tests/Playwright/ODataEndpointExecutionTests.cs`):
   - Privaten statischen Helper analog `TreeCollapseTests.cs:143-160` einführen
     (`waitTarget.WaitForAsync(Timeout=5_000)` → bei `TimeoutException` `clickTarget.ClickAsync()`,
     max. 3 Versuche, finaler Wait mit Default-Timeout).
   - Ersetzt die fünf blinden Doppelklick-Paare (Z. 59-60, 78-79, 98-99, 155-156, 165-166):
     waitTarget = sichtbarer Expand-Zustand (z. B. Chevron-down-Icon bzw. das erwartete
     Kindelement), clickTarget = der Chevron-Button. Der zweite Blindklick entfällt — die
     Sichtbarkeitsprüfung vor dem Klick verhindert zugleich das Zuklappen eines bereits
     expandierten Knotens (Toggle-Semantik).
   - **Kein** Hauptfix — dokumentiertes Restrisiko (verlorener Klick während
     SignalR-Reconnect, #112-Muster); die Strict-Mode-Problematik wird ausschließlich
     durch Schritt 1 beseitigt.

3. **Stabilitätsdoku ergänzen** (`docs/help/stabilitaet/os-interface-tests.md`, Abschnitt
   „Bekannte Fehlerbilder", nach Z. 161):
   - Neuer Eintrag: **Alt-Daten-Duplikat → Strict-Mode-Verstoß.** Symptom: Playwright
     `strict mode violation`, Locator resolved zu 2 Elementen (Sofort-Fehlschlag, kein
     Timeout). Ursache: `EnsureDeletedAsync` ist auf `Mode=Memory`-DSNs wirkungslos —
     kein File-Backing (`DataSource` leer → `Delete()` erreicht `ClearAllPools`/`File.Delete`
     gar nicht), `Mode=Memory`-Verbindungen sind non-pooled, und die offene
     Anchor-Connection hält die benannte Shared-Cache-DB am Leben. Daten akkumulieren
     daher **deterministisch** über alle Facts aller Collections; der Strict-Mode-Verstoß
     tritt bei ungünstigem DOM-/Render-Timing auf (die Assertion resolved zu 1 Element,
     solange das Re-Render die Duplikat-Zeile noch nicht angehängt hat; erst bei
     verspätetem Re-Render — typisch unter CI-Last — resolved sie zu 2 Elementen).
     Fix: deterministischer `DELETE FROM`-Wipe in `TestDatabaseSeeder.ResetAsync`
     (Issue #118, Runs 45434716126 + 45543708029). Diagnose-Hinweis: Trace-Snapshots
     prüfen, ob „fremde" Einträge anderer Facts im DOM stehen — beachte: Alt-Daten sind
     auch in *grünen* Läufen sichtbar (vgl. lokaler Trace
     `ODataEndpointExecutionTests-091350503.zip`).

4. **Befund-Angaben im GitHub-Issue #118 ergänzen** (Akzeptanzkriterium 1): Kommentar mit
   den fehlenden Angaben aus `inventory/ci-evidence.md` + `plan-check.md` — Run-Links
   (45434716126, 45543708029), fehlschlagender Schritt (Z. 47, erste Baum-Assertion),
   Fehlermeldung (Strict-Mode-Verstoß, 2 Elemente), Aufeinanderfolge (ja — im
   pr-staging-ci-Fenster ohne grünen Primär-Lauf dazwischen), Klassifikation
   (**Testisolations-/Infrastrukturdefekt, kein Produktdefekt, kein klassischer Flake**)
   mit korrekter Zweiteilung der Ursache: (a) `EnsureDeletedAsync` ist auf
   `Mode=Memory;Cache=Shared`-DSNs ein deterministischer No-op → Alt-Daten liegen immer
   vor; (b) die Intermittenz ist eine DOM-/Render-Race am ungefilterten Strict-Expect
   (Z. 47), keine Reset-Race. Gewählter Fix (modellgetriebener `DELETE FROM`-Wipe) und
   Verweis auf dieses Artefakt-Verzeichnis.

## 5. Teststrategie

OS-Tests benötigen Playwright/Chromium — Installation erfolgt über das
`InstallPlaywright`-MSBuild-Target beim Build (abschaltbar via
`-p:SkipPlaywrightInstall=true`).

```bash
# 1) Betroffene Testklasse — mehrfach, da Fact-Reihenfolge variiert
dotnet test src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj -- \
  --filter "FullyQualifiedName~ODataEndpointExecutionTests" \
  --report-trx --report-trx-filename issue-118-run.trx
# → 3–5 Wiederholungen; Erwartung: dauerhaft grün in beiden Fact-Reihenfolgen.
#   TRX in src/Schnittstellenzentrale.Tests/TestResults/ prüfen (beide Facts Passed,
#   ggf. startTime-Reihenfolge notieren).

# 2) Geschwister-/Kollisionskandidaten (gleiche Reset-Mechanik, andere Collection)
dotnet test src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj -- \
  --filter "FullyQualifiedName~ODataImportTests" --report-trx

# 3) Reguläre Lane (blockierend; Meta-Tests FlakyConventionTests/TraitParityTests laufen hier)
dotnet test src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj -- \
  --filter "Category!=OsInterface"

# 4) VERPFLICHTEND (nicht optional): voller OsInterface-Lauf als globale Isolations-Regression
dotnet test src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj -- \
  --filter "Category=OsInterface" --report-trx
```

**Warum Schritt 4 verpflichtend ist:** Da der Reset bisher **nie** wirkte (§2), konnten
andere Playwright-Tests still auf akkumulierte Daten vertrauen — z. B. `.First`-Locators,
die bisher die *älteste* Zeile trafen, oder `.Single()`-/Reihenfolgeannahmen. Mit dem
erstmals echt wirksamen Reset können solche verdeckten Abhängigkeiten erstmals als neue
rote Tests sichtbar werden; gefundene Fälle sind eigenständige Isolationsdefekte und
kein Grund, den Fix zurückzunehmen.

Verifikation des Fixes selbst: Der bisherige Fehler ist nach dem Fix **deterministisch
ausgeschlossen** — ohne Alt-Daten kann die Duplikat-Zeile gar nicht ins DOM gelangen. Die
≥3 Wiederholungsläufe (inkl. mindestens eines Laufs mit Reihenfolge
„GetEndpoints → Registrieren" — per TRX-`startTime` prüfen) verifizieren daher primär die
Abwesenheit **neuer** Isolations-/Timing-Probleme, nicht den alten Defekt — „3× grün"
allein ist nicht der Wirkbeweis. Zusätzlich code-seitig sicherstellen, dass
`ClearAllDataAsync` wirklich alle modellierten Tabellen leert (10 DbSets). Die
Strict-Assertion bleibt als Tripwire erhalten (siehe Scope-Ausschlüsse): Ein künftiges
Isolationsloch würde sich sofort wieder als Strict-Mode-Verstoß zeigen.

## 6. Risiken und Nebenwirkungen

- **Breitenwirkung (geschärft):** Der neue Wipe betrifft alle 24 Playwright-Testklassen.
  Da der Reset bisher **nie** wirkte, ist das Risiko neu auftretender roter Tests real:
  Tests könnten unbeabsichtigt auf akkumulierte Daten vertraut haben (`.First`-Locators
  auf die älteste Zeile, `.Single()`-/Reihenfolgeannahmen). Mitigation: Schritt 5.4
  (voller OsInterface-Lauf) ist verpflichtende lokale Regression; gefundene
  Abhängigkeiten sind eigenständige Isolationsdefekte.
- **Rest-Risiko beim Wipe selbst — `SQLITE_LOCKED`, nicht `SQLITE_BUSY`:** Schreibt ein
  sich noch abbauender Circuit des Vortests exakt während des `DELETE FROM`, ist bei
  Shared-Cache-DSNs `SQLITE_LOCKED` das einschlägige Risiko: `SQLITE_BUSY` entsteht durch
  Datei-/Datenbank-Locks anderer Verbindungen und wird vom Busy-Handler/`busy_timeout`
  abgefedert; `SQLITE_LOCKED` dagegen signalisiert Shared-Cache-Tabellen-/Schema-Locks
  innerhalb desselben Caches und wird vom Busy-Handler **nicht** behandelt (Retry auf
  derselben Verbindung möglich). Sichtbar wäre das als Reset-Fehler im Test-Output,
  nicht mehr als Strict-Mode-Verstoß. Sequenzielle Fact-Ausführung innerhalb der
  Collection plus abgeschlossener Browser-Dispose machen das Fenster klein; kein
  Retry-Mechanismus eingeplant (YAGNI — bei Auftreten: `SqliteException` mit
  `SqliteErrorCode == 6` (`SQLITE_LOCKED`) gezielt mit wenigen Retries abfedern;
  beobachten. Hinweis zur Präzision: Der Primärcode 6 deckt den erweiterten Code
  `SQLITE_LOCKED_SHAREDCACHE` (262) nicht ab — ein etwaiger Retry müsste zusätzlich
  `SqliteErrorCode == 262` bzw. auf `ExtendedErrorCode` prüfen).
- **`sqlite_sequence` wird nicht zurückgesetzt:** Auto-Increment-IDs laufen über Facts
  weiter — semantisch irrelevant (kein Test asserted auf konkrete IDs).
- **`SeedSystemEndpointsAsync`-Neuanlage je Reset:** Nach dem vollständigen Wipe wird die
  System-App je Reset neu angelegt (neue Id) und `POST /authenticate` exakt einmal
  geseedet — bisher akkumulierte sich dieser Endpunkt pro Fact, weil der Reset nie wirkte
  (Nebenbefund, wird mitgeheilt).
- **CI/Analyzer:** Keine Workflow-Änderung. Der Quarantäne-Befund normalisiert sich über die
  kommenden Primär-Läufe; bis die Fehlrate unter die Schwelle fällt, bleibt der Befund im
  Analyzer-Fenster sichtbar — das ist erwartetes Verhalten (Randbedingung der Anforderung).
- **Kein `[Flaky]`:** Begründung siehe Scope-Ausschlüsse; die Vergaberegeln verlangen einen
  dokumentierten *Flake*-Befund — hier liegt ein klassifizierter, behobener
  Isolationsdefekt vor.

## 7. Offene Punkte

Keine — Entscheidung getroffen: Die `ExpandWithRetryAsync`-Härtung (Schritt 4.2) wird
**in diesem PR** umgesetzt. Begründung: Die Issue-Beschreibung (verbindliche Anweisung,
Abschnitt „Empfohlene Schritte") benennt das Muster ausdrücklich als „naheliegende erste
Maßnahme"; die blinden Chevron-Doppelklicks sind ein dokumentiertes Restrisiko desselben
Flake-Mechanismus aus Issue #112. Ein Aufschieben als Follow-up würde das im Issue geforderte
Ziel „Test härten" nur teilweise erfüllen. (Entscheidung bestätigt 2026-11-25.)
