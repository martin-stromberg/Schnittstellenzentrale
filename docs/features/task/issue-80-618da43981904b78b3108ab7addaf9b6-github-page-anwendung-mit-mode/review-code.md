# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### ablauf-technisch.md

- **Inkonsistenz Dokumentation ↔ Workflow** — Zeile 23 dokumentiert den `paths`-Filter des Deploy-Triggers als „`site/**`, `.github/workflows/pages.yml` und `scripts/verify-site.mjs`". Der tatsächliche Filter in `.github/workflows/pages.yml` (Zeilen 25–32) wurde im Zuge der Befund-Behebung erweitert und umfasst zusätzlich `scripts/verify-site*.mjs` (Glob statt Einzeldatei), `docs/help/github-page/**`, `docs/help/index.md`, `README.md` und `docs/RELEASE_NOTES.md`. Die Hilfedatei beschreibt damit einen veralteten Trigger-Umfang.

  Empfehlung: Zeile 23 an den tatsächlichen `paths`-Filter anpassen (alle sieben Einträge auflisten oder mit Verweis auf die vom Verify-Skript geprüften Doku-Artefakte umschreiben).

## Geprüfte Dateien

- `.github/workflows/pages.yml`
- `site/index.html`
- `site/site.css`
- `site/theme.js`
- `site/favicon.png` (Existenz/Referenz)
- `site/screenshots/*.png` (Existenz/Referenz; `.gitkeep` entfernt)
- `scripts/verify-site.mjs`
- `scripts/verify-site.test.mjs`
- `src/Schnittstellenzentrale.Tests/Playwright/SiteScreenshotTests.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/SitePageTests.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightTestBase.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/TestDatabaseSeeder.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/TestPaths.cs`
- `docs/help/github-page/index.md`
- `docs/help/github-page/beschreibung.md`
- `docs/help/github-page/ablauf-technisch.md`
- `docs/help/index.md`
- `docs/RELEASE_NOTES.md`
- `README.md`

## Nachprüfung der Befunde aus review-code.1.md

Alle sechs Befunde des ersten Laufs sind korrekt behoben:

1. **`SiteScreenshotTests.ExpandGroupAsync`** (Zeilen 120–130): Jetzt parametrisiert mit `groupName` + `expectedChildText`; der Kindknoten wird innerhalb der Gruppen-Sektion (`section.Locator(...)`) gesucht. Aufrufe: `("Kundendienste", "Kundenportal API")` und `("Interne Services", "Benachrichtigungsdienst")` — jede Expansion wird gegen das eigene Kind verifiziert.
2. **`TestDatabaseSeeder.SeedShowcaseDataAsync`** (Zeilen 51–63): Zerlegt in `ClearNonSystemDataAsync`, `SeedGroupsAndApplicationsAsync`, `SeedEndpointsAsync`, `SeedEnvironmentsAsync`; Reihenfolge und öffentliches Verhalten (Scope, Repositories, Aufräumen vor Seeding) unverändert — Idempotenz gewahrt.
3. **`TestPaths.cs`**: Neue gemeinsame Helper-Klasse mit `IsHeadless()` und `FindRepoRoot()`; Grep bestätigt, dass die Duplikate aus `SitePageTests`, `SiteScreenshotTests` und `PlaywrightTestBase` entfernt sind und alle drei Stellen die Helper nutzen.
4. **`pages.yml` `paths`-Filter** (Zeilen 25–32): Um `docs/help/github-page/**`, `docs/help/index.md`, `README.md`, `docs/RELEASE_NOTES.md` und `scripts/verify-site*.mjs` erweitert; Kommentarkopf (Zeilen 6–9) beschreibt den erweiterten Trigger konsistent.
5. **`verify-site.mjs` Containment-Check** (Zeilen 36–39, 102–107): `isWithinDirectory(siteDir, resolve(siteDir, reference))` via `relative()` — kanonischer Ansatz, der `..`-Ausbrüche und absolute Pfade (auch fremde Laufwerke unter Windows) korrekt ablehnt; neuer Testfall „fails when an asset reference escapes the site directory" in `verify-site.test.mjs` (Zeilen 141–157) legt eine existierende Datei außerhalb von `site/` an und erwartet den Fehlschlag.
6. **`site/screenshots/.gitkeep`**: Gelöscht; das Verzeichnis enthält nur noch die vier echten PNGs.

## Anmerkungen (keine Befunde)

- `node scripts/verify-site.mjs` läuft lokal fehlerfrei (Exit 0); `node --test scripts/verify-site.test.mjs`: 11/11 Tests grün.
- `TestPaths` ist in der kompilierten Test-DLL vorhanden (Build-Artefakt aktuell); XML-Doku-Pflicht in allen neuen öffentlichen Membern erfüllt.
- `isWithinDirectory`-Randfälle geprüft: Kandidat gleich `siteDir` (relativ `""`) landet korrekt im „fehlt/leer"-Zweig; scheme-URLs, Anker und Protokoll-relative Referenzen werden weiterhin vorab ausgefiltert.
- Der externe shields.io-Badge in `site/index.html` wird von `localAssetReferences` bewusst übersprungen und in `SitePageTests` per Route-Abort abgefangen — keine Netzwerkabhängigkeit im Test.

## Nachträgliche Auflösung (Orchestrator, 19.11.2026)

Der eine Befund des zweiten Laufs — veraltete `paths`-Filter-Beschreibung in
`docs/help/github-page/ablauf-technisch.md` Zeile 23 — wurde behoben: die Zeile
listet jetzt exakt die sieben Filter-Einträge aus `.github/workflows/pages.yml`
(`site/**`, `.github/workflows/pages.yml`, `scripts/verify-site*.mjs`,
`docs/help/github-page/**`, `docs/help/index.md`, `README.md`,
`docs/RELEASE_NOTES.md`). Direkt gegen den Workflow verifiziert. Damit sind alle
Befunde aus beiden Review-Läufen aufgelöst.
