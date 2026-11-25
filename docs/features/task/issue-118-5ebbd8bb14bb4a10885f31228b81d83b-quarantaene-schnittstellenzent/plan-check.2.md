Plan lückenhaft

# Plan-Check (2. Runde) – Issue #118: Quarantäne `ODataEndpointExecutionTests`

Geprüft am 2026-11-25 gegen `requirement.md`, `inventory.md`, `inventory/ci-evidence.md`,
`plan.md` (überarbeiteter Stand) und den Code (EF Core / Microsoft.Data.Sqlite 11.0.0 im
NuGet-Cache bestätigt). **Ergebnis vorab:** Der überarbeitete Plan hat alle vier Lücken
aus `plan-check.1.md` korrekt abgearbeitet — die Root-Cause-Mechanik in `plan.md` ist
jetzt konsistent, widerspruchsfrei und deckt sich mit den dokumentierten
Decompile-/Reflexions-/Repro-Belegen. Der Fix (`DELETE FROM`-Wipe + `ExpandWithRetryAsync`)
ist korrekt und vollständig beschrieben. Die verbleibenden Lücken sind **zwei
nicht bereinigte Reststellen in den Inventory-Artefakten**, die weiterhin die
falsifizierte „Reset meist wirksam"-Schlussfolgerung behaupten und über §4.4 in den
Issue-Kommentar (Akzeptanzkriterium) propagieren können, sowie kleinere Ungenauigkeiten.

## Geprüfte Kriterien

### 1. Abdeckung der Anforderungen (requirement.md + Akzeptanzkriterien)

| Anforderung | Befund |
|---|---|
| FA1 Fehlerbild sichern (Run-Links, Schritt, Meldung, Aufeinanderfolge) | **Erfüllt** — `ci-evidence.md` enthält Runs 45434716126 + 45543708029, Zeile 47, Strict-Mode-Verstoß, aufeinanderfolgend im pr-staging-ci-Fenster; Plan §4.4 plant den Issue-Kommentar mit genau diesen Angaben. |
| FA2 Ursache klassifizieren | **Erfüllt** — „Testisolations-/Infrastrukturdefekt, kein Produktdefekt, kein klassischer Flake" ist jetzt mit belastbarer Mechanik begründet (§2: deterministischer No-op + DOM-/Render-Race). |
| FA3 Ursache beheben oder Test härten | **Erfüllt** — deterministischer Wipe (§4.1) plus `ExpandWithRetryAsync`-Härtung (§4.2); die Issue-Beschreibung benennt das Muster als „naheliegende erste Maßnahme". |
| FA4 Entscheidungsregel | **Erfüllt** — kein `[Flaky]`, korrekt begründet (behebbarer Isolationsdefekt; Vergaberegeln verlangen dokumentierten Flake). |
| FA5 Dokumentation neues Fehlerbild | **Erfüllt** — §4.3 liefert jetzt den korrekten Eintragstext für „Bekannte Fehlerbilder" (Einfügepunkt `os-interface-tests.md` nach Z. 161 verifiziert — Abschnitt endet dort, Z. 155-161). |

### 2. Technische Korrektheit (Stichproben gegen den Code verifiziert)

**(a) Korrigierte Root-Cause-Mechanik — jetzt konsistent und widerspruchsfrei:** ✅

- `plan.md` §2 beschreibt die No-op-Mechanik exakt so, wie `plan-check.1.md` sie
  falsifiziert/verifiziert hat: `DataSource` leer bei `Mode=Memory` → der
  `ClearAllPools`/`File.Delete`-Zweig wird nicht betreten; else-if-Zweig unerreichbar,
  weil `finally` die Verbindung schließt; `Mode=Memory` non-pooled
  (`IsNonPooled=True` per Reflexion auf 11.0.0); Anchor-Connection hält die benannte
  Shared-Cache-DB (`PlaywrightServer.cs:69-72` — Zeilen exakt verifiziert) bis
  Collection-Ende am Leben. Intern konsistent: kein Widerspruch mehr zu den
  Laufzeit-Belegen (`Rows after EnsureDeleted: 1`, `no such table` erst nach
  Anchor-Dispose).
- Intermittenz jetzt korrekt als DOM-/Render-Race an `ODataEndpointExecutionTests.cs:47`
  (Zeile verifiziert: ungefiltertes `Expect(.sz-tree-item-btn HasText "SZ-OData-E2E")
  .ToBeVisibleAsync()`; Pendant Z. 135) beschrieben, inkl. Trace-Beleg
  `ODataEndpointExecutionTests-091350503.zip` (Alt-Apps im DOM eines *grünen* zweiten
  Facts) und `.First`-Kaschierung. Erklärt die CI-Statistik („meist grün, gelegentlich
  rot, aufeinanderfolgend") konsistent.
- Formulierungs-Nitpick (unkritisch): §2 schreibt „die Duplikat-Zeile des
  **Alt-Eintrags**" gelänge per Re-Render ins DOM — tatsächlich war die Alt-Zeile
  bereits seit Seitenlade im DOM (ci-evidence §4a); das Re-Render hängt die Zeile der
  **neu angelegten** Anwendung an. Mechanik und Schlussfolgerung bleiben richtig.

**(b) Geplanter `DELETE FROM`-Wipe — korrekt bedacht:** ✅

- `db.Model.GetEntityTypes() → GetTableName()` deckt alle **10 DbSets** ab (verifiziert
  in `src/Schnittstellenzentrale.Infrastructure/Data/AppDbContext.cs`, Z. 14-41:
  ApplicationGroups, Applications, EndpointGroups, Endpoints, EndpointHeaders,
  EndpointQueryParameters, SystemEnvironments, EnvironmentVariables, ApplicationLinks,
  EndpointCallHistory). `__EFMigrationsHistory` nicht im Modell — korrekt außen vor
  (existiert unter `EnsureCreated` ohnehin nicht).
- `PRAGMA foreign_keys` als per-Connection-Pragma auf explizit geöffneter Verbindung
  (`OpenConnectionAsync`), außerhalb einer Transaktion — **korrekt**: bei non-pooled
  Verbindungen ginge die Einstellung zwischen zwei `ExecuteSqlRawAsync`-Aufrufen sonst
  verloren.
- `sqlite_sequence`-Nicht-Reset benannt und akzeptabel (§6). System-Re-Seed korrekt
  durchdacht: `SystemEntryInitializer.InitializeAsync` ist idempotent
  (`SystemEntryInitializer.cs:19-66` — legt Gruppe/App nur bei Fehlen an, verifiziert),
  `SeedSystemEndpointsAsync` legt `POST /authenticate` bedingungslos an
  (`TestDatabaseSeeder.cs:224-230` verifiziert) → nach Wipe exakt einmal statt
  akkumulierend (Nebenbefund, wird mitgeheilt — korrekt erkannt).
- Auch der veraltete Code-Kommentar an `SeedShowcaseDataAsync` (Z. 41-50, der
  „nicht zuverlässig"-Satz) wird in Schritt 1 zum Aktualisieren eingeplant. ✅

**(c) `SQLITE_LOCKED` vs. `SQLITE_BUSY` — korrekt für Shared-Cache:** ✅ (mit Einschränkung)

- Die Unterscheidung in §6 stimmt: `SQLITE_BUSY` (Datei-/DB-Locks fremder Verbindungen,
  vom Busy-Handler/`busy_timeout` abgefedert) vs. `SQLITE_LOCKED` (Shared-Cache-
  Tabellenlocks desselben Caches, nicht vom Busy-Handler behandelt).
- Einschränkung: `SqliteException.SqliteErrorCode` liefert in Microsoft.Data.Sqlite den
  **erweiterten** Fehlercode — der Shared-Cache-Fall wäre `SQLITE_LOCKED_SHAREDCACHE`
  (262), ein `== 6`-Vergleich würde ihn verfehlen. Betrifft nur die bewusst nicht
  eingeplante (YAGNI-)Contingency-Retry — Empfehlung, keine Lücke.

**(d) `ExpandWithRetryAsync`-Referenzen und Umsetzungsschritt — korrekt:** ✅ (mit Zählfehler)

- `TreeCollapseTests.cs:143-160` verifiziert: `private static async Task
  ExpandWithRetryAsync(ILocator waitTarget, ILocator clickTarget)`, bis zu 3 Versuche,
  `WaitForAsync(Timeout=5_000)`, `TimeoutException` → `clickTarget.ClickAsync()`,
  finaler Default-Wait. Signatur/Mechanik exakt wie referenziert.
- Die fünf gelisteten Doppelklick-Paare (Z. 59-60, 78-79, 98-99, 155-156, 165-166)
  sind im Code exakt so vorhanden — **aber der Plan nennt sie „sechs".** Tatsächlich
  sind es fünf Paare (3 im ersten Fact, 2 im zweiten). Die Einzelklicks Z. 84, 104, 171
  sind keine Paare und zurecht nicht gelistet. Zählfehler ohne Umsetzungsrisiko, aber
  falsch.

**(e) Reststellen der alten Mechanik — vorhanden:** ❌ (die verbleibende Lücke)

In `plan.md` selbst ist **keine** Reststelle mehr vorhanden (alle `ClearAllPools`-
Erwähnungen stehen jetzt im korrekten „wird nie erreicht"-Kontext). Aber zwei
Inventory-Artefakte behaupten weiterhin die von `plan-check.1.md` falsifizierte
Schlussfolgerung — wörtlich:

- `inventory/ci-evidence.md:152`: „… der Datenbank-Reset ist also **meist wirksam**,
  aber nicht zuverlässig" — widerspricht `plan.md` §2 („wirkt nie", deterministischer
  No-op) und der eigenen Repro-Beleglage. Das war exakt plan-check.1-Befund (c).
- `inventory.md:182`: „… → Reset **meist wirksam, intermittierend unwirksam**." —
  dieselbe falsifizierte Interpretation.

Relevant, weil §4.4 den Issue-Kommentar „mit den fehlenden Angaben aus
`inventory/ci-evidence.md` + `plan-check.md`" komponieren lässt: Die stale Sätze sind
eine Propagationsquelle für genau die falsche Klassifikation, deren Beseitigung
Gegenstand der Überarbeitung war. (`inventory.md:181` dagegen ist unkritisch — dort
steht „nicht zuverlässig" als wörtliches Zitat des Code-Kommentars `TestDatabaseSeeder.cs:46-49`,
das als Code-Fakt stimmt und in §4.1 zum Aktualisieren eingeplant ist.)

### 3. Umsetzungsschritte: Vollständigkeit, Reihenfolge, Abhängigkeiten

- Schritt 1 (`TestDatabaseSeeder.cs`, Z. 28-39 verifiziert als `ResetAsync`-Range):
  `EnsureDeletedAsync` entfernen, `EnsureCreatedAsync` behalten (frische DB → Schema;
  bestehende → HasTables-No-op — korrekt), neuer modellgetriebener Wipe, Kommentar-
  Begründung, `SeedShowcaseDataAsync`-Kommentar aktualisieren + dortigen
  `ClearNonSystemDataAsync`-Aufruf bevorzugt entfernen. Vollständig inkl. zwei
  begründeter verworfener Alternativen (Repository-Wipe deckt direkt nur 3 Tabellen;
  `EndpointCallHistory`-`SetNull` an `AppDbContext.cs:188,192` verifiziert;
  `DROP TABLE`-Variante verworfen). ✅
- Schritt 2 (Härtung): Helper-Signatur, Ersatzstellen und Toggle-Semantik-Begründung
  korrekt; Abhängigkeit zu Schritt 1 keine (unabhängig, richtig eingeordnet als „kein
  Hauptfix"). ✅
- Schritte 3-4 (Stabilitätsdoku, Issue-Kommentar): Inhalte konkret und jetzt mit
  korrekter Mechanik. ✅
- Reihenfolge sinnvoll; keine unbenannten impliziten Abhängigkeiten. Kleinigkeit:
  Kein Schritt korrigiert die stale Inventory-Sätze (s. Lücken).

### 4. Teststrategie

- MTP-Syntax korrekt: `global.json` `test.runner=Microsoft.Testing.Platform`
  verifiziert; Filter/Reports hinter `--` (`--filter`, `--report-trx`,
  `--report-trx-filename`) entsprechen der xunit.v3-Konvention. ✅
- 3–5 Wiederholungsläufe + TRX-`startTime`-Reihenfolgenprüfung: sinnvoll; kein Orderer
  im Projekt (grep → keine Treffer), Fact-Reihenfolge variiert tatsächlich. ✅
- **Verpflichtender voller OsInterface-Lauf (§5.4) ist benannt und begründet** — korrekt,
  weil der Reset bisher nie wirkte und verdeckte Alt-Daten-Abhängigkeiten erstmals
  sichtbar werden können. ✅
- Intermittenz-Argumentation jetzt sauber: „der bisherige Fehler ist nach dem Fix
  deterministisch ausgeschlossen … '3× grün' allein ist nicht der Wirkbeweis" — genau
  die von plan-check.1 geforderte Umformulierung. ✅

### 5. Nebenwirkungen

- Breitenwirkung auf alle **24 Playwright-Klassen** benannt (verifiziert: 22 Dateien,
  `ImpressumPageTests.cs` und `ImpressumPageWithLanguageTests.cs` je 2 Klassen = 24). ✅
- Reguläre Lane inkl. `FlakyConventionTests`/`TraitParityTests` adressiert (nur
  Test-Infrastruktur geändert — plausibel). ✅
- `sqlite_sequence`, `SeedSystemEndpointsAsync`-Neuanlage, Analyzer-Fenster-Normalisierung,
  `SQLITE_LOCKED`-Restfenster — benannt. ✅

## Gefundene Lücken

1. **Stale „meist wirksam"-Schlussfolgerungen in den Inventory-Artefakten bereinigen**
   (Pflicht vor dem Issue-Kommentar):
   - `inventory/ci-evidence.md:152` — Satz „der Datenbank-Reset ist also meist wirksam,
     aber nicht zuverlässig" durch den verifizierten Befund ersetzen: der Reset wirkt
     **nie** (`EnsureDeletedAsync` = deterministischer No-op auf `Mode=Memory`); die 4
     grünen Läufe in Reihenfolge GetEndpoints→Registrieren erklären sich über die
     DOM-/Render-Race (Alt-Daten waren auch dort im DOM — Beleg: lokaler Trace
     `ODataEndpointExecutionTests-091350503.zip`).
   - `inventory.md:182` — dieselbe falsifizierte Interpretation analog korrigieren.
   - Alternativ (minimal): je ein Nachtrags-Vermerk „Korrigiert durch plan-check/plan:
     Reset wirkt deterministisch nie". Wichtig ist, dass die Artefakte die
     Root-Cause-Klassifikation im Issue-Kommentar (§4.4, Akzeptanzkriterium 1) nicht
     konterkarieren.
2. **Zählfehler in `plan.md` §4.2:** „sechs blinden Doppelklick-Paare" → **fünf**
   (Z. 59-60, 78-79, 98-99, 155-156, 165-166 — Liste selbst ist korrekt und vollständig).

## Empfehlungen

- Wording in `plan.md` §2 präzisieren: das Re-Render hängt die Zeile der **neu
  angelegten** Anwendung an (die Alt-Zeile stand bereits seit Seitenlade im DOM).
- Beim Contingency-Hinweis in §6 (`SqliteErrorCode == 6`) ggf. auf den erweiterten Code
  `SQLITE_LOCKED_SHAREDCACHE` (262) bzw. `SqliteExtendedErrorCode` verweisen — nur
  relevant, falls die (bewusst nicht eingeplante) Retry je umgesetzt wird.
- Bei Umsetzung die Empfehlung aus `plan-check.1.md` aufgreifen: optionaler
  Diagnose-Log (Zeilenzählen pro Tabelle nach dem Wipe) im TRX-`<Output>` — erleichtert
  künftige Isolations-Diagnosen.
