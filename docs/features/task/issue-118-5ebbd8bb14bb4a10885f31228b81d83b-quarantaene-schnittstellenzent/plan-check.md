Plan vollständig

# Plan-Check (3. Runde) – Issue #118: Quarantäne `ODataEndpointExecutionTests`

Geprüft am 2026-11-25 gegen `requirement.md`, `inventory.md`, `inventory/ci-evidence.md`,
`plan.md` (aktualisierter Stand) und den Code. **Ergebnis vorab:** Beide in
`plan-check.2.md` benannten Lücken sind geschlossen — die falsifizierte
„Reset meist wirksam"-Schlussfolgerung existiert in keinem Eingabe-Artefakt mehr, und der
Zählfehler in §4.2 ist korrigiert. Zusätzlich wurde die plan-check.2-Empfehlung zur
`SQLITE_LOCKED_SHAREDCACHE`-Präzision aufgegriffen. Der Plan ist widerspruchsfrei,
code-verifiziert und ohne weitere Klärung umsetzbar.

## Geprüfte Kriterien

### 1. Anforderungsabdeckung (requirement.md + Akzeptanzkriterien)

| Anforderung | Befund |
|---|---|
| FA1 Fehlerbild sichern (Run-Links, Schritt, Meldung, Aufeinanderfolge) | **Erfüllt** — `ci-evidence.md` liefert Runs 45434716126 + 45543708029 (Links in `inventory.md` §12), `ODataEndpointExecutionTests.cs:47`, Strict-Mode-Verstoß, aufeinanderfolgend im pr-staging-ci-Fenster; §4.4 plant den Issue-Kommentar mit genau diesen Angaben. |
| FA2 Ursache klassifizieren | **Erfüllt** — „Testisolations-/Infrastrukturdefekt, kein Produktdefekt, kein klassischer Flake" mit verifizierter Zweiteilung: deterministischer No-op-Reset + DOM-/Render-Race. Jetzt konsistent über plan.md, inventory.md und ci-evidence.md. |
| FA3 Ursache beheben oder Test härten | **Erfüllt** — deterministischer `DELETE FROM`-Wipe (§4.1) plus `ExpandWithRetryAsync`-Härtung (§4.2, Entscheidung „in diesem PR" begründet in §7). |
| FA4 Entscheidungsregel | **Erfüllt** — kein `[Flaky]`, korrekt begründet (behebbarer Isolationsdefekt; Vergaberegeln verlangen dokumentierten Flake). |
| FA5 Dokumentation neues Fehlerbild | **Erfüllt** — §4.3 liefert Eintragstext mit korrekter Mechanik; Einfügepunkt `os-interface-tests.md` nach Z. 161 verifiziert (Abschnitt „Bekannte Fehlerbilder" Z. 155-161, nächster Abschnitt Z. 163). |

### 2. Technische Korrektheit (Stichproben gegen den Code)

**(a) plan-check.2-Lücke 1 geschlossen — stale Ursachenklassifikation bereinigt:** ✅

- `inventory/ci-evidence.md:149-159` enthält jetzt die korrigierte Fassung:
  „nicht weil der Reset wirkt: `EnsureDeletedAsync` ist auf der
  Shared-Cache-In-Memory-DB ein deterministischer No-op (`DataSource` leer bei
  `Mode=Memory`; Verbindungen non-pooled), Alt-Daten sind in **allen** Läufen nach dem
  Erstfact vorhanden … Die grünen Läufe erklären sich durch eine DOM-/Render-Race" —
  inkl. transparentem Korrekturvermerk „(Korrigiert per plan-check.1.md — zuvor stand
  hier fälschlich ‚Reset meist wirksam'.)". Exakt konsistent mit `plan.md` §2.
- `inventory.md:182` sagt jetzt: „Reset = deterministischer No-op bei `Mode=Memory`
  (s. ci-evidence §5): Die Intermittenz ist eine DOM-/Render-Race an Zeile 47, kein
  Reset-Race." ✅
- Grep über alle Artefakte: kein stale Vorkommen mehr. Verbleibende
  „nicht zuverlässig"-Treffer sind unkritisch — `inventory.md:52` und `:181` zitieren
  wörtlich den real existierenden Code-Kommentar `TestDatabaseSeeder.cs:46-49`
  (verifiziert: Kommentar steht dort; als Code-Fakt korrekt attribuiert und in §4.1
  zum Aktualisieren eingeplant); `plan.md:155` ist die Aktualisierungsanweisung
  selbst. Propagationsquelle in den Issue-Kommentar (§4.4) damit beseitigt.

**(b) plan-check.2-Lücke 2 geschlossen — Zählfehler:** ✅

- `plan.md` §4.2 (Z. 179) nennt jetzt „die **fünf** blinden Doppelklick-Paare
  (Z. 59-60, 78-79, 98-99, 155-156, 165-166)". Im Code verifiziert: `appChevron`-
  Doppelklicks an Z. 59-60, 78-79, 98-99 (Fact 1) und Z. 155-156, 165-166 (Fact 2) —
  genau fünf Paare; Einzelklicks Z. 84, 104, 171 sind zurecht nicht gelistet.

**(c) plan-check.2-Empfehlung aufgegriffen — `SQLITE_LOCKED`-Präzision:** ✅

- `plan.md` §6 Rest-Risiko (Z. 280-284) enthält jetzt den Hinweis: „Der Primärcode 6
  deckt den erweiterten Code `SQLITE_LOCKED_SHAREDCACHE` (262) nicht ab — ein etwaiger
  Retry müsste zusätzlich `SqliteErrorCode == 262` bzw. auf `ExtendedErrorCode`
  prüfen." Korrekt (Microsoft.Data.Sqlite liefert erweiterte Codes in
  `SqliteErrorCode`).

**(d) Root-Cause-Mechanik und geplanter Fix — weiterhin korrekt:** ✅

- `EnsureDeletedAsync`-No-op-Mechanik (§2) unverändert konsistent mit den
  Decompile-/Reflexions-/Repro-Belegen aus plan-check.1; Anchor-Connection an
  `PlaywrightServer.cs:69-72` erneut verifiziert (`Data Source={guid};Mode=Memory;Cache=Shared`, `_anchorConnection.Open()`).
- `DELETE FROM`-Wipe: 10 DbSets in `AppDbContext.cs` Z. 14-41 verifiziert
  (inkl. `EndpointCallHistory` mit `SetNull`-FKs an Z. 188/192 — das belegte Leck des
  Repository-Wipes); per-Connection-`PRAGMA foreign_keys` auf explizit geöffneter
  Verbindung außerhalb einer Transaktion — korrekt für non-pooled Verbindungen.
- `ResetAsync`-Range `TestDatabaseSeeder.cs:28-39` verifiziert;
  `SeedSystemEndpointsAsync` legt `POST /authenticate` bedingungslos an (Z. 224-230);
  `SeedShowcaseDataAsync`-Kommentar an Z. 46-49 existiert wie beschrieben.
- `ExpandWithRetryAsync`-Referenz `TreeCollapseTests.cs:143-160` verifiziert
  (3 Versuche, 5-s-Wait, `TimeoutException` → Klick, finaler Default-Wait;
  Aufrufer Z. 116-122/128-134). Toggle-Semantik-Begründung im Plan korrekt.
- Fehlerstelle `ODataEndpointExecutionTests.cs:47` (ungefilterte Strict-Assertion,
  Pendant Z. 135) verifiziert.
- Reihenfolge `EnsureCreatedAsync` → Wipe ist aus §2 ableitbar (frische DB → Schema
  anlegen, „der Daten-Wipe ist dann ein harmloser No-op") — auf neuer DB existieren
  die Tabellen vor dem `DELETE FROM`, kein `no such table`-Risiko.

### 3. Vollständigkeit/Reihenfolge der Umsetzungsschritte

- Schritt 1 (Reset-Fix): vollständig inkl. Kommentar-Aktualisierungen und zwei
  begründeter verworfener Alternativen (Repository-Wipe: nur 3/10 Tabellen direkt +
  `EndpointCallHistory`-Leck; `DROP TABLE`: unnötiges Schema-Timing).
- Schritt 2 (Härtung): Helper-Signatur, fünf Ersatzstellen, Toggle-Semantik —
  unabhängig von Schritt 1, korrekt als „kein Hauptfix" eingeordnet.
- Schritte 3-4 (Stabilitätsdoku, Issue-Kommentar): konkrete Inhalte mit korrekter
  Mechanik; keine Stale-Quellen mehr.
- Reihenfolge sinnvoll, keine unbenannten impliziten Abhängigkeiten.

### 4. Teststrategie-Ausführbarkeit

- MTP-Syntax korrekt (Filter/Reports hinter `--`; `--report-trx`,
  `--report-trx-filename`) — xunit.v3-Konvention eingehalten.
- 3–5 Wiederholungsläufe + TRX-`startTime`-Reihenfolgenprüfung sinnvoll (kein Orderer
  im Projekt, Fact-Reihenfolge variiert — in ci-evidence §5 belegt).
- **Verpflichtender voller OsInterface-Lauf (§5 Schritt 4) benannt und sauber
  begründet:** Da der Reset bisher nie wirkte, können verdeckte
  Alt-Daten-Abhängigkeiten (`.First`-Locators, `.Single()`-Annahmen — z. B.
  `SeedFilterEndpointsAsync` Z. 206) erstmals sichtbar werden; gefundene Fälle sind
  eigenständige Isolationsdefekte. ✅
- Intermittenz-Argumentation sauber: „deterministisch ausgeschlossen … '3× grün'
  allein ist nicht der Wirkbeweis". ✅

### 5. Nebenwirkungen

- Breitenwirkung auf alle 24 Playwright-Testklassen benannt (in plan-check.2 gegen
  22 Dateien/24 Klassen verifiziert); reguläre Lane inkl. Meta-Tests adressiert.
- `sqlite_sequence`-Nicht-Reset, `POST /authenticate`-Neuanlage (Nebenbefund,
  mitgeheilt), `SQLITE_LOCKED`-Restfenster, Analyzer-Fenster-Normalisierung —
  benannt und korrekt eingeordnet.
- Scope-Ausschlüsse vollständig und begründet; bewusste Beibehaltung der
  Strict-Assertion als Isolations-Tripwire ist eine saubere Design-Entscheidung.

## Empfehlungen (optional, keine Lücken)

- **Wording-Harmonisierung:** `plan.md` §2 (Scope-Ausschluss-Tabelle, Z. 99) nennt die
  erste Doppelklick-Stelle „Z. 56-60", §4.2 korrekt „Z. 59-60" (Z. 56-58 ist die
  `appRow`-Locator-Deklaration). Beim Umsetzen an §4.2 orientieren; die Tabelle kann
  bei Gelegenheit auf „59-60" vereinheitlicht werden.
- **Explizite Reihenfolge:** In §4.1 die Reihenfolge „`EnsureCreatedAsync` →
  `ClearAllDataAsync` → `SystemEntryInitializer`/`SeedSystemEndpointsAsync`" beim
  Umsetzen einhalten (aus §2 ableitbar; stellt sicher, dass Tabellen vor dem Wipe
  existieren).
- **plan-check.1-Empfehlung weiterhin gültig:** optionaler Diagnose-Log (Zeilenzahlen
  pro Tabelle nach dem Wipe) im TRX-`<Output>` — erleichtert künftige
  Isolations-Diagnosen.
- Kleinigkeit: `plan.md` Z. 3 verweist auf „plan-check.md" als Überarbeitungsquelle —
  gemeint war `plan-check.1.md`; die Referenz ist ab dieser dritten Prüfung ohnehin
  konsistent.
