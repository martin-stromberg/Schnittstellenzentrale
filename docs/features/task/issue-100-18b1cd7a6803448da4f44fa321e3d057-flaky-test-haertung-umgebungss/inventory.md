# Bestandsaufnahme – Flaky-Test-Härtung (Issue #100)

Faktische Inventur der Codebasis als Grundlage für die Trait-basierte Trennung
umgebungssensitiver Tests (`Category=OsInterface`), Deadline-/Cleanup-Härtung und
Retry-Konvention. Nur Fakten, keine Bewertung.

**Stand:** Branch `task/issue-100-18b1cd7a6803448da4f44fa321e3d057-flaky-test-haertung-umgebungss`
**Scope:** `src/Schnittstellenzentrale.Tests`, `.github/workflows`, `.github/actions`, `scripts/`, `docs/help/`, `CLAUDE.md`, `.gitignore`

## Detaildokumente

| Dokument | Inhalt |
|---|---|
| [inventory/testprojekt-struktur.md](inventory/testprojekt-struktur.md) | Verzeichnisstruktur, Paketversionen, Attribute/Traits, Test-Collections |
| [inventory/audit-os-interface.md](inventory/audit-os-interface.md) | **Kernstück:** Dateiweise Klassifikation aller 105 Dateien (OsInterface vs. regulär) mit Begründung |
| [inventory/deadlines-timeouts.md](inventory/deadlines-timeouts.md) | Server-Start, Readiness, Playwright-Timeouts, Job-Timeouts |
| [inventory/cleanup.md](inventory/cleanup.md) | Dispose-/finally-Pfade, Temp-Verzeichnis-Löschung, Fehlerbehandlung |
| [inventory/ci.md](inventory/ci.md) | Workflow-Jobs, Filter, Coverage-Gate, Artefakte, Berechtigungen |
| [inventory/sonstiges.md](inventory/sonstiges.md) | scripts/, docs/help/, .gitignore, CLAUDE.md-Konventionen |

## Kernaussagen auf einen Blick

- **Testprojekt:** 105 `.cs`-Dateien unter `src/Schnittstellenzentrale.Tests` (davon 83 Dateien mit `[Fact]`/`[Theory]`, 24 Playwright-Testklassen in 22 Dateien, 22 reine Helfer-/Infrastruktur-Dateien). Insgesamt 703 `[Fact]`/`[Theory]`-Attribute.
- **xUnit:** `xunit` **2.9.3** (xUnit v2, **nicht** v3) + `xunit.runner.visualstudio` 3.1.5 — `[Trait("Category", "OsInterface")]` ist v2-kompatibel (`--filter "Category=OsInterface"`).
- **Traits:** Kein einziges `[Trait]` im gesamten Testprojekt (verifiziert per Volltextsuche). Aktuell einzige Klassifikation: FQN-Namensfilter `FullyQualifiedName~Playwright`, der über den Namespace `Schnittstellenzentrale.Tests.Playwright` greift.
- **Collections:** 6 `[CollectionDefinition]`s, alle in `Playwright/Infrastructure/`, jeweils mit eigenem `PlaywrightServer`-Fixture und eigenem Festport (5099–5104).
- **Audit-Ergebnis:** 40 Dateien umgebungssensitiv (alle 22 Playwright-Testdateien + 17 Playwright-Infrastructure-Dateien + `Services/ImpressumServiceTests.cs` mit echtem Temp-Dateisystem und Schreibzugriffen ins `AppContext.BaseDirectory`). Alle Integration-Tests laufen über `TestServer` (In-Memory, kein echter Socket) bzw. `Data Source=:memory:` und sind nach Repo-Heuristik **nicht** OsInterface.
- **Deadlines:** `PlaywrightServer` bindet `http://127.0.0.1:5099` und ruft `StartAsync()` **ohne** Readiness-Poll auf; der einzige „Warmup" läuft über den In-Process-`TestServer`-Handler, nicht über die gebundene Adresse. Keine expliziten Page-/Assertion-Timeouts in `PlaywrightTestBase` (Playwright-Defaults greifen).
- **Cleanup:** `Directory.Delete` für `sz-impressum-*`-Tempverzeichnisse und `playwright-traces` ohne Retry/Backoff; kein `try/catch` um Dispose-Ketten — ein Cleanup-Fehler kann den eigentlichen Assert überdecken.
- **CI:** Beide Workflows (`pr-staging-ci.yml`, `staging-ci.yml`) führen Playwright als `continue-on-error`-**Step im selben `build-and-test`-Job** aus (kein eigener Job). Coverage beider Lanes wird gemeinsam von ReportGenerator gemergt und gegen 70 % geprüft. Ein dritter FQN-Filter steckt in `release.yml` (Release-Gate).
- **Lücken:** Kein `docs/help/stabilitaet/`-Ordner; `playwright-traces/` ist **nicht** in `.gitignore`; `docs/help/playwright-tests/` beschreibt eine veraltete Infrastruktur (`PlaywrightTestFactory`, Datei-SQLite `schnittstellenzentrale-tests.db`, „11 Tests in 7 Klassen").
