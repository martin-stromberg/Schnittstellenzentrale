Plan lückenhaft

# Plan-Check – Issue #118: Quarantäne `ODataEndpointExecutionTests`

Geprüft am 2026-11-25 gegen `requirement.md`, `inventory.md` + `inventory/ci-evidence.md`,
`plan.md` und den tatsächlichen Code-/Bibliotheksstand (EF Core / Microsoft.Data.Sqlite
11.0.0, decompile + Laufzeit-Reproduktion). Der vorgeschlagene **Fix selbst ist korrekt
und umsetzbar** — die Lücke liegt in der Root-Cause-Argumentation, die in wesentlichen
mechanischen Behauptungen falsifiziert ist und sich in die geforderten Befund-/Doku-
Artefakte (§4.3 Stabilitätsdoku, §4.4 Issue-Kommentar) fortpflanzen würde.

## Geprüfte Kriterien

### 1. Abdeckung der Anforderungen (requirement.md + Akzeptanzkriterien)

| Anforderung | Befund |
|---|---|
| FA1 Fehlerbild sichern (Run-Links, Schritt, Meldung, Aufeinanderfolge) | **Erfüllt** — `inventory/ci-evidence.md` enthält alles (Runs 45434716126 + 45543708029, Z. 47, Strict-Mode-Verstoß, aufeinanderfolgend im pr-staging-ci-Fenster — verifiziert gegen die heruntergeladenen TRX). Plan §4.4 plant den Issue-Kommentar. |
| FA2 Ursache klassifizieren | **Formal erfüllt, inhaltlich fehlerhaft** — Klassifikation „Testisolations-/Infrastrukturdefekt, kein Produktdefekt, kein Flake" ist korrekt, aber die begründende Mechanik ist falsch (Details unter Kriterium 2). |
| FA3 Ursache beheben oder Test härten | **Erfüllt** — deterministischer `DELETE FROM`-Wipe + `ExpandWithRetryAsync`-Härtung; der Fix würde den Fehler tatsächlich beseitigen (s. Kriterium 2). |
| FA4 Entscheidungsregel (Fix statt `[Flaky]`) | **Erfüllt** — konsistent: behebbarer Isolationsdefekt → kein `[Flaky]`. |
| FA5 Dokumentation neues Fehlerbild | **Geplant, aber mit falscher Mechanik** — Eintrag in „Bekannte Fehlerbilder" ist vorgesehen (Akzeptanzkriterium), der vorgeschlagene Text beschreibt jedoch die widerlegte „ClearAllPools-Race"-Erklärung. |

### 2. Technische Korrektheit (Stichproben verifiziert)

**Bestätigte Plan-Behauptungen:**

- Fehlerstelle `ODataEndpointExecutionTests.cs:47` (ungefilterte Strict-Assertion,
  analog Z. 135) — verifiziert, genau so im Code.
- Blinde Chevron-Doppelklick-Paare an Z. 59-60, 78-79, 98-99, 155-156, 165-166 — verifiziert.
- `ResetAsync` (`TestDatabaseSeeder.cs:28-39`): `EnsureDeletedAsync` + `EnsureCreatedAsync`
  + `SystemEntryInitializer` (idempotent, `SystemEntryInitializer.cs:19-66` — legt nur bei
  Fehlen an) + `SeedSystemEndpointsAsync` (legt `POST /authenticate` **bedingungslos** an,
  Z. 224-230 → Duplikat-Akkumulation bei versagendem Reset bestätigt).
- `ExpandWithRetryAsync(ILocator waitTarget, ILocator clickTarget)`
  (`TreeCollapseTests.cs:143-160`, Aufrufer Z. 116-122/128-134) — Signatur, 3 Versuche,
  5-s-Warte, finaler Default-Wait: exakt wie im Plan referenziert.
- 10 DbSets im `AppDbContext` — verifiziert (inkl. `EndpointCallHistory` mit
  `OnDelete(SetNull)`, das `ClearNonSystemDataAsync` tatsächlich zurücklässt).
- 24 Playwright-Testklassen — verifiziert (22 Dateien, 24 Klassen).
- MTP-Filter hinter `--` in der Teststrategie — korrekte xunit.v3-Syntax.
- Wipe-Detail „PRAGMA foreign_keys auf explizit geöffneter Verbindung, außerhalb einer
  Transaktion" — **korrekt und wichtig**: `foreign_keys` ist ein per-Connection-Pragma;
  bei non-pooled Verbindungen ginge es zwischen zwei `ExecuteSqlRawAsync`-Aufrufen sonst
  verloren. Der Plan hat das richtig bedacht.

**Falsifizierte Plan-Behauptungen (die Lücke):**

(a) *Behauptung*: `SqliteDatabaseCreator.Delete()` liest `DataSource` = `{guid}`-Name
    (nicht leer) → `SqliteConnection.ClearAllPools()` + `File.Delete(<name>)`.
    **Realität (decompile `microsoft.entityframeworkcore.sqlite.core/11.0.0` +
    Laufzeit-Reproduktion):** `SqliteConnection.DataSource` liefert für eine geöffnete
    `Mode=Memory`-Verbindung `sqlite3_db_filename` = **`""`** (leer). Damit wird der
    `if (!string.IsNullOrEmpty(path))`-Zweig **gar nicht betreten** — `ClearAllPools`
    und `File.Delete` laufen nie. Der `else-if`-Zweig (`State == Open`) ist ebenfalls
    toter Code, weil das `finally` die Verbindung bereits geschlossen hat.
    **`Delete()` ist auf diesem DSN ein buchstäblicher No-op.**

(b) *Behauptung*: Die DB wird zerstört, sobald das Pool-Clearing die letzte physische
    Verbindung schließt; offene/ausgeliehene Verbindungen überleben das Clearing.
    **Realität:** `SqliteConnectionFactory.GetPoolGroup` markiert Verbindungen mit
    `Mode == SqliteOpenMode.Memory` als **`IsNonPooled`** (Quellcode +
    Reflexions-Probe auf 11.0.0: `IsNonPooled=True`). Es existiert **kein**
    `SqliteConnectionPool` für diesen Connection-String; `ClearAllPools()` iteriert
    Pool-Gruppen und tut dort nichts (`poolGroup.Clear()` → `_pool == null` → No-op).
    Selbst wenn der Zweig erreicht würde, könnte das „Clearing" keine einzige dieser
    Verbindungen schließen — es gibt kein Race-Fenster, weil es kein Clearing gibt.

(c) *Behauptung*: „4 von 6 Läufen mit Reihenfolge GetEndpoints→Registrieren grün →
    Reset meist wirksam, intermittierend unwirksam." **Realität (empirisch):**
    `EnsureDeletedAsync` löscht **nie** — Reproduktion mit `Mode=Memory;Cache=Shared`
    + offener Anchor: `Rows after EnsureDeleted: 1` (deterministisch); die benannte
    In-Memory-DB stirbt erst mit der letzten Verbindung (Anchor-Dispose →
    „no such table"). Zusätzlicher Beleg aus dem **lokalen Testlauf vom 25.11.**
    (beide Facts grün, Reihenfolge GetEndpoints→Registrieren): Der Basis-HTML-Snapshot
    des *zweiten* Facts (`playwright-traces/ODataEndpointExecutionTests-091350503.zip`)
    zeigt die Alt-Anwendungen `SZ-OData-E2E` und `Filter-Test-App` als
    `.sz-tree-item-btn` im DOM — die Daten waren da, der Test lief trotzdem grün.

(d) *Tatsächliche Intermittenz (fehlt im Plan):* Der Strict-Mode-Verstoß ist eine
    **DOM-/Timing-Race an Z. 47**, keine Reset-Race: `Expect(...).ToBeVisibleAsync()`
    resolved im lokalen Trace ~1,3 ms nach Start zu **einem** Element — das Re-Render
    des Baums (Neuanlage → `TreeChanged` → `LoadDataAsync`) hatte die neue
    Duplikat-Zeile noch nicht ins DOM gehängt. Danach machen `.First`-Locators und
    die Alt-App (die bereits die importierten OData-Endpunkte des ersten Facts trägt)
    den Rest des Tests gegen die *falsche* Anwendung „grün". Scheitert der erste Poll
    des Expects erst nach dem Re-Render, stehen 2 Elemente da → Sofort-Fehlschlag.
    Das erklärt die CI-Statistik (meist grün, gelegentlich rot, aufeinanderfolgend)
    besser und ist durch die lokale Trace-Auswertung gestützt.

**Nebenfolgerung (verstärkt den Fix, Plan unterschätzt sie):** Da der Reset nie wirkt,
akkumulieren Daten deterministisch über **alle** Facts **aller** Playwright-Collections
(Collection-Fixture = eine DB pro Collection; der `SeedShowcaseDataAsync`-Kommentar Z. 44-49
deutet das bereits an, und `POST /authenticate` dupliziert sich pro Fact). Der
modellgetriebene Wipe ist damit nicht nur für diesen Test, sondern für die ganze Suite
die richtige Isolation — die Plan-Begründung ist also sogar zu schwach, nicht zu stark.

### 3. Umsetzungsschritte: Vollständigkeit, Reihenfolge, Abhängigkeiten

- Schritt 1 (Wipe in `ResetAsync`) ist technisch korrekt beschrieben und vollständig:
  explizites `OpenConnectionAsync` (per-Connection-PRAGMA!), `PRAGMA foreign_keys = 0`,
  `DELETE FROM` über `db.Model.GetEntityTypes() → GetTableName()` (Distinct,
  null-gefiltert — deckt alle 10 DbSets, Tabellen ohne DbSet und
  `__EFMigrationsHistory` bleibt zu Recht außen vor), `PRAGMA foreign_keys = 1`,
  `finally`-Close. Keine impliziten Abhängigkeiten.
- `sqlite_sequence`-Nicht-Reset ist benannt und akzeptabel (kein Test asserted IDs).
- Reihenfolge und Scope-Ausschlüsse (Strict-Assertion als Tripwire behalten, keine
  Namensänderung) sind nachvollziehbar und sinnvoll.
- Schritte 2-4 (Härtung, Doku, Issue-Kommentar) sind vollständig adressiert.
- Kleinigkeit: Das Rest-Risiko beim Wipe unter Shared-Cache wäre `SQLITE_LOCKED`
  (Shared-Cache-Tabellenlocks), nicht `SQLITE_BUSY` — sachlich unwesentlich, aber für
  die Doku präzisierbar.

### 4. Teststrategie

- Realistisch ausführbar: Playwright/Chromium lokal installiert (`~/AppData/Local/
  ms-playwright/chromium-*`), MTP-Filter-Syntax hinter `--` korrekt, lokaler Lauf der
  Klasse wurde im Rahmen dieses Checks erfolgreich ausgeführt (2/2 grün, ~15 s).
- ≥3 Wiederholungsläufe + TRX-`startTime`-Reihenfolgenprüfung ist eine sinnvolle
  Intermittenz-Verifikation.
- Hinweis: Nach dem Fix ist der bisherige Fehler deterministisch ausgeschlossen
  (kein Alt-Daten-Duplikat möglich); die Wiederholungsläufe verifizieren primär die
  Abwesenheit neuer Isolations-/Timing-Probleme — das sollte in §5 so benannt werden,
  damit „3× grün" nicht als der eigentliche Wirkbeweis missverstanden wird.

### 5. Nebenwirkungen

- Globaler Wipe über alle 24 Playwright-Klassen: vom Plan adressiert (§6 Breitenwirkung,
  voller OsInterface-Lauf als Regression). Da der Reset bisher **nie** wirkte, ist das
  Risiko neu auftretender roter Tests sogar realer als vom Plan beschrieben (Tests
  könnten unbeabsichtigt auf akkumulierte Daten vertrauen — z. B. `.First`-Locators,
  die bisher die *älteste* Zeile trafen). Erhöht die Wichtigkeit von Schritt 5.4.
- `SeedSystemEndpointsAsync`-Neuanlage je Reset: korrekt erfasst (heilt die bisherige
  Akkumulation von `POST /authenticate`).
- Reguläre Lane (`Category!=OsInterface`, Meta-Tests `FlakyConventionTests`/
  `TraitParityTests`) unberührt — plausibel, da nur Test-Infrastruktur geändert wird;
  MTP-Syntax im Plan korrekt.

## Gefundene Lücken

1. **§2 „Tatsächliche Reset-Mechanik" korrigieren** (Root-Cause nicht belastbar):
   `DataSource` ist für `Mode=Memory` leer (`sqlite3_db_filename` → `""`) → `Delete()`
   erreicht weder `ClearAllPools()` noch `File.Delete()` (else-if unerreichbar, weil
   `finally` die Verbindung schließt) — `EnsureDeletedAsync` ist ein deterministischer
   No-op. Zusätzlich ist der Connection-String non-pooled (`IsNonPooled=True`), sodass
   `ClearAllPools` selbst bei Aufruf keine dieser Verbindungen schließen könnte.
2. **Intermittenz-Ursache ersetzen:** Nicht „Reset meist wirksam, Race beim Clearing",
   sondern: Reset wirkt nie → Alt-Daten liegen immer vor → Strict-Assertion an Z. 47
   ist eine DOM-/Render-Race (Expect resolved zu 1 Element, solange das Re-Render die
   Duplikat-Zeile noch nicht angehängt hat; `.First`-Locators + bereits importierte
   OData-Endpunkte auf der Alt-App kaschieren den Rest). Belege: lokaler Trace
   `playwright-traces/ODataEndpointExecutionTests-091350503.zip` (Alt-Apps im DOM eines
   *grünen* zweiten Facts; Expect resolved in ~1,3 ms) sowie die CI-Fehlertraces.
3. **§4.3 Doku-Eintrag und §4.4 Issue-Kommentar inhaltlich anpassen:** Der vorgeschlagene
   „Bekannte Fehlerbilder"-Text („`ClearAllPools` schließt nicht alle Verbindungen —
   Race") dokumentiert eine falsche Mechanik. Korrektur: „`EnsureDeletedAsync` ist auf
   `Mode=Memory`-DSNs wirkungslos (kein File-Backing, non-pooled, Anchor hält die DB);
   Daten akkumulieren deterministisch über alle Facts einer Collection; Strict-Mode-
   Verstoß tritt bei ungünstigem Render-Timing der Duplikat-Zeile auf."
4. **Kleinigkeiten:** (a) Rest-Risiko `SQLITE_LOCKED` statt `SQLITE_BUSY` benennen;
   (b) „nur 3 von 10 Tabellen" bei `ClearNonSystemDataAsync` präzisieren — Kaskaden
   decken mehr ab, die eigentlichen Lecks sind `EndpointCallHistory` (`SetNull`) und
   fehlende Abdeckung neuer Entities; (c) den Nachweis in §5 umformulieren (Fix macht
   den Alt-Daten-Pfad deterministisch unmöglich, Wiederholungsläufe prüfen auf neue
   Defekte, nicht auf den alten).

## Empfehlungen

- Lokalen Trace vom 25.11. (`ODataEndpointExecutionTests-091350503.zip`) als Beleg in
  die Artefakte aufnehmen: Er widerlegt „Reset meist wirksam" direkt — Alt-Daten waren
  auch in grünen Läufen im DOM.
- In `ResetAsync` nach dem Wipe eine Debug-Absicherung denkbar (z. B. `Assert`/
  `output`-Log mit Zeilenzählen pro Tabelle), damit ein zukünftiges Isolationsloch im
  TRX-`<Output>` sichtbar wäre — optional, kein Muss.
- Die Prüfung „welche anderen Playwright-Tests still auf Alt-Daten vertrauten" im
  vollen OsInterface-Lauf (§5.4) nicht überspringen: Mit einem echt wirksamen Reset
  können erstmals verdeckte Abhängigkeiten sichtbar werden (z. B. `.Single()`- oder
  Reihenfolgeannahmen).
