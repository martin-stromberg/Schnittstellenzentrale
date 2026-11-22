# Flaky-Test-Härtung: umgebungssensitive Tests, Deadlines/Cleanup und Retry-Konvention

**Quelle:** GitHub-Issue #100 („Flaky-Test-Härtung: umgebungssensitive Tests, Deadlines/Cleanup und Retry-Konvention (Vorlage: Softwareschmiede#428)") — vollständiger Text in `issue.md` im Repository-Root.
**Blaupause:** `martin-stromberg/Softwareschmiede` Issue #428, Umsetzung in PR #432 (gemergt 2026-11-20, drei komplette CI-Durchläufe verifiziert grün).

## Ziel / Zweck

Die in der Softwareschmiede-Blaupause beschlossene Härtung gegen Flaky-Tests soll projektübergreifend ausgerollt werden; dieses Issue ist die angepasste Variante für dieses Repository. Umgebungssensitive Tests sollen identifiziert, markiert und aus der blockierenden CI-Lane ausgelagert werden; Deadlines, Cleanup und Retry-Verhalten sollen so gehärtet werden, dass Flakes „selten, lokalisiert, selbstdiagnostizierend" auftreten.

## Ausgangslage / Befund

Die CI führt Tests aus, hat aber noch keine Trait-basierte `OsInterface`-Kategorie. Konkreter Stand:

- Beide Test-Workflows (`pr-staging-ci.yml`, `staging-ci.yml`) trennen heute rein namensbasiert: Die blockierende Stufe läuft mit `--filter "FullyQualifiedName!~Playwright"`, die Playwright-Stufe mit `FullyQualifiedName~Playwright` und `continue-on-error` — aber **als Step im selben `build-and-test`-Job**, nicht als eigenständiger Job.
- Im gesamten Testprojekt (`src/Schnittstellenzentrale.Tests`, ~105 Testdateien) gibt es **keinen einzigen `[Trait]`** — neue umgebungssensitive Tests ohne „Playwright" im Namen landen unbemerkt in der blockierenden Lane.
- Bereits heute laufen umgebungssensitive Nicht-Playwright-Tests blockierend mit (Beispiele: `ImpressumServiceTests` mit `Path.GetTempPath` + `Directory.Delete`, Upload-Tests mit echten Temp-Dateien).
- `PlaywrightServer` bindet einen festen Port (`http://127.0.0.1:5099`); die `sz-impressum-*`-Tempverzeichnisse werden mit schlichtem `Directory.Delete` ohne Retry/Backoff aufgeräumt.
- **Coverage-Verflechtung:** Die Playwright-Coverage fließt bei Erfolg in das 70%-Line-Coverage-Gate ein. Ein Auslagern in einen separaten best-effort-Job verändert die Coverage-Herleitung — ohne Artefakt-Merge wird der Schwellwert instabil oder unvollständig.
- Es existieren weder `scripts/`-Helfer noch eine TRX-Auswertung oder Retry-Konvention.

**Anlass:** In `martin-stromberg/Softwareschmiede` führte ein flaky eingeschränkter Prozess-Test zu nicht reproduzierbaren CI-Fehlschlägen in der regulären, blockierenden Test-Lane (Issue `martin-stromberg/Softwareschmiede#428`, Umsetzung in PR `martin-stromberg/Softwareschmiede#432` — drei komplette CI-Durchläufe verifiziert grün). Die dort beschlossene Härtung soll projektübergreifend ausgerollt werden; dieses Issue ist die angepasste Variante für dieses Repository.

## Funktionale Anforderungen

1. **Audit:** Die Test-Dateien sind auf umgebungssensitive Tests zu prüfen. Repo-angepasste Heuristik: Playwright/Browser-Automation (Chromium), echte Kestrel-Bindung inkl. festem Port 5099 (`PlaywrightServer`), `WebApplicationFactory`-Szenarien mit echtem HTTP-Roundtrip, Temp-Dateisystem (`Path.GetTempPath`, `sz-impressum-*`, `playwright-traces`), SignalR-Live-Roundtrips über die gebundene Adresse, Upload-Tests mit echten Dateien. **Explizit nicht** umgebungssensitiv: In-Memory-SQLite-Integrationstests (`Data Source=:memory:`) und reine bUnit-/Service-Tests — sie bleiben deterministisch in der blockierenden Lane. (Offener Punkt: finale Abgrenzungsliste im Audit-Ergebnis dokumentieren.)

2. **Trait-Infrastruktur:** `[Trait("Category", "OsInterface")]` ist für umgebungssensitive Tests einzuführen; der `Flaky`-Trait ist als zweite Markierung **nur** in Kombination mit `OsInterface` zulässig — per Meta-Test abgesichert. Zusätzlich ist ein Meta-Test zu erstellen, der Trait-Parität erzwingt: jede Testklasse, die der bisherige Namensfilter (`FullyQualifiedName~Playwright`) erfasst hätte, muss `OsInterface` tragen (verhindert stille Regression beim Filterwechsel).

3. **Lane-Trennung:** Umgebungssensitive Tests sind in einen **eigenen Job** auszulagern (`continue-on-error` auf Job-Ebene, großzügiges `timeout-minutes`), nicht als Step im Blocker-Job. Die reguläre Lane filtert `Category!=OsInterface` und bleibt deterministisch/blockierend. Die Änderung ist in **beiden** Workflows (`pr-staging-ci.yml` **und** `staging-ci.yml`) identisch umzusetzen — sonst driftet die Konvention auseinander. Der bisherige FQN-Namensfilter wird durch den Trait-Filter ersetzt.

4. **Deadline-Angleichung:** Host-/Polling-Deadlines müssen größer als das Kind-/Phasen-Budget sein (Referenzfehler aus der Blaupause: Host 30 s vs. Kind 90 s → eingebaute Flake-Quelle). Repo-konkret: Server-Readiness-Waits in `PlaywrightServer` (Start-up-Poll bis Port 5099 antwortet), Page-/Assertion-Timeouts und Trace-Stop-Budgets in `PlaywrightTestBase` gegen das Gesamt-Timeout des Jobs prüfen; Diagnose-Ausgabe (Phase/State/Error) beibehalten bzw. einführen.

5. **Cleanup-Härtung:** In Fixture-`Dispose`/`finally` sind laufende Browser-/Server-Ressourcen geordnet zu beenden (`StopAsync`/`DisposeAsync` bereits vorhanden — Fehlerpfade prüfen), `Directory.Delete` für `sz-impressum-*`/`playwright-traces` ist mit Retry/Backoff zu versehen, Restfehler sind nur zu protokollieren statt zu werfen — ein Cleanup-Fehler darf den eigentlichen Assert nie als `AggregateException` überdecken. Blaupause: `TempDirectoryCleanup`-Helfer aus PR #432.

6. **Retry-/Quarantäne-Konvention:** `Flaky`-Markierung nur mit `OsInterface` (Meta-Test, s. o.); genau **ein** automatisierter Re-Run nur der markierten Fehlschläge in der best-effort-Lane; dokumentierte `gh run rerun --job <id>`-Regel nur für den OS-Job; **kein** Blanket-Retry der Gesamtsuite.

7. **Flake-Statistik:** Eine TRX-Auswertung (Fehlschlag-then-Rerun-grün-Rate pro Test) ist als Report-Schritt einzurichten; bei Schwellwertüberschreitung ist ein Quarantäne-Issue zu erzeugen. TRX-Dateien liegen bereits als Artefakte (`test-results*.trx`) vor. Offene Punkte: Wo wird die Statistik über Runs hinweg persistiert (Artefakt-Kette vs. Committed-Datei vs. Issue-Kommentare)? Welcher Schwellwert löst Quarantäne aus?

8. **Coverage-Integrität:** Es ist festzulegen, wie die OS-Job-Coverage weiterhin ins 70%-Gate einfließt (Artefakt-Upload aus dem OS-Job + Merge via ReportGenerator im auswertenden Schritt), damit der Schwellwert nicht nicht-deterministisch von einer best-effort-Lane abhängt — alternativ bewusst dokumentieren, dass OS-Coverage nicht mehr ins Gate zählt, und Threshold-Herleitung anpassen. (Offener Punkt: Entscheidung Merge vs. Ausschluss.)

9. **Port-Strategie:** Offener Punkt — festen Port 5099 beibehalten (einfach, dokumentiert) oder auf dynamischen Port umstellen (robuster gegen Kollisionen auf geteilten Runnern, erfordert URL-Übergabe an den Browser-Kontext).

10. **Doku:** Die Konvention ist in `CLAUDE.md` (Testkommando `FullyQualifiedName!~Playwright` → Trait-Filter) und `docs/help/playwright-tests/` zu aktualisieren; Blaupause: `docs/help/stabilitaet/os-interface-tests.md`.

11. **Verifikation:** Mindestens drei komplette CI-Durchläufe (je Workflow) sind selbstständig zu wiederholen — ein einzelner grüner Lauf gilt nicht als Nachweis. Zu beachten ist `concurrency.cancel-in-progress`: Wiederholungen seriell auf demselben Branch oder über separate Läufe auslösen.

## Offene Punkte (vor Umsetzung klären)

1. **Job-Platzierung:** OS-Lane als zusätzlicher Job in den beiden bestehenden Workflows oder eigener Workflow? (Empfehlung Blaupause: gleicher Workflow, eigener Job.)
2. **Coverage-Behandlung:** OS-Coverage per Artefakt mergen oder aus dem 70%-Gate ausnehmen?
3. **Flake-Schwellwert und Statistik-Persistenz** (s. Maßnahme Flake-Statistik).
4. **Port 5099:** fest vs. dynamisch.
5. **Audit-Ergebnis:** Welche Integration-Tests außerhalb von `Playwright/` fallen konkret unter `OsInterface`? Kandidaten sind Tests mit Temp-Dateisystem und echten HTTP-Roundtrips; In-Memory-SQLite-Tests nicht.

## Akzeptanzkriterien

- Die blockierende Lane (`Category!=OsInterface`) enthält nachweislich keinen umgebungssensitiven Test; der Meta-Test garantiert Trait-Parität zum bisherigen Namensfilter.
- Ein Fehlschlag im OS-Job blockiert weder PR-Merge noch Staging-Promotion; ein Fehlschlag in der regulären Lane weiterhin schon.
- `Flaky`-Trait existiert nur in Kombination mit `OsInterface` (Meta-Test grün).
- Cleanup-Fehler überdecken keine Test-Asserts (kein `AggregateException`-Masking mehr).
- TRX-Flake-Auswertung läuft als Report-Schritt und erzeugt bei Schwellwertverletzung ein Quarantäne-Issue.
- Mindestens drei vollständig grüne CI-Durchläufe je Workflow liegen vor.
- Doku (`CLAUDE.md`, `docs/help/playwright-tests/`) beschreibt die neue Filter-Konvention.

## Nicht-Ziele / Abgrenzung

- In-Memory-SQLite-Integrationstests (`Data Source=:memory:`) sind **explizit nicht** umgebungssensitiv und bleiben deterministisch in der blockierenden Lane.
- Reine bUnit-/Service-Tests sind **explizit nicht** umgebungssensitiv und bleiben deterministisch in der blockierenden Lane.
- Es gibt **kein** Blanket-Retry der Gesamtsuite.
- Echte UI-/Prozess-Tests werden nie vollständig deterministisch — Ziel ist „selten, lokalisiert, selbstdiagnostizierend"; wo möglich ist Logik gegen Fakes statt gegen echte Prozesse zu testen.
