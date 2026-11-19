# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### SiteScreenshotTests.cs (SiteScreenshotTests)

- **Testqualität** — `ExpandGroupAsync` (Zeilen 120–128): Der Sichtbarkeits-Erwartungswert ist hartkodiert auf `"Kundenportal API"`. Beim Aufklappen der Gruppe `"Interne Services"` (Aufruf aus `ExpandShowcaseTreeAsync`, Zeile 104) ist dieser Button bereits sichtbar, bevor der Klick überhaupt ausgewertet ist — `ClickUntilVisibleAsync` kehrt sofort zurück, ohne die Expansion der tatsächlich angeforderten Gruppe zu verifizieren. Die Methode prüft damit nur für `"Kundendienste"` das benannte Verhalten.

  Empfehlung: Das erwartete Element auf die aufzuklappende Gruppe beziehen — z. B. einen zweiten Parameter für den erwarteten Kindknoten (`"Benachrichtigungsdienst"` für `"Interne Services"`) oder einen Locator innerhalb von `section` verwenden, der erst nach dem Aufklappen existiert.

### TestDatabaseSeeder.cs (TestDatabaseSeeder)

- **God-Methode** — `SeedShowcaseDataAsync` (Zeilen 51–181, ~130 Zeilen): Die Methode erledigt fünf konzeptuell getrennte Aufgaben hintereinander: Aufräumen der Nicht-System-Daten, Anlegen der Gruppen, Anlegen der Anwendungen, Anlegen der Endpunktgruppen/-punkte und Anlegen der Umgebungen.

  Empfehlung: In private Hilfsmethoden zerlegen, z. B. `ClearNonSystemDataAsync()`, `SeedGroupsAndApplicationsAsync()`, `SeedEndpointsAsync(Application portalApp, Application orderApp)` und `SeedEnvironmentsAsync()` — die Aufräumphase ist zudem fachlich eigenständig und verdient einen eigenen Namen.

### SiteScreenshotTests.cs / SitePageTests.cs / PlaywrightTestBase.cs

- **Doppelter Code** — `FindRepoRoot()` ist nahezu identisch in `SiteScreenshotTests` (Zeilen 160–171) und `SitePageTests` (Zeilen 181–192) implementiert; `IsHeadless()` in `SitePageTests` (Zeilen 134–138) dupliziert die private Logik aus `PlaywrightTestBase` (Zeilen 37–41). Bei einer dritten Verwendungsstelle oder einer Änderung (z. B. weitere CI-Variable) driften die Kopien auseinander.

  Empfehlung: Eine gemeinsame Hilfsklasse in `Playwright/Infrastructure` anlegen (z. B. `static class TestPaths` mit `FindRepoRoot()` und `IsHeadless()`) und aus beiden Testklassen sowie der Basisklasse verwenden.

### pages.yml

- **Inkonsistenz Trigger ↔ Prüfumfang** — Der `paths`-Filter (Zeilen 24–27) deckt nur `site/**`, die Workflow-Datei und `scripts/verify-site.mjs` ab. `verify-site.mjs` prüft aber zusätzlich `docs/help/github-page/**`, `docs/help/index.md`, `README.md` (`## GitHub Page`), `docs/RELEASE_NOTES.md` und liegt in der Logik auch `scripts/verify-site.test.mjs` nahe. Ein Commit, der nur ein geprüftes Doku-Artefakt umbenennt/entkernt (z. B. den README-Abschnitt entfernt), löst weder Workflow noch Verifikation aus — der nächste Site-Deploy schlägt dann mit einem scheinbar zusammenhanglosen Verify-Fehler fehl.

  Empfehlung: Die vom Verify-Skript geprüften Pfade in den `paths`-Filter aufnehmen (`docs/help/github-page/**`, `docs/help/index.md`, `README.md`, `docs/RELEASE_NOTES.md`, `scripts/verify-site*.mjs`). Ein dadurch ausgelöstes Redeploy desselben `site/`-Stands ist unproblematisch.

### verify-site.mjs

- **Fehlende Validierung** — `localAssetReferences`/`verifySiteArtifacts` (Zeilen 93–98): Referenzen mit `..` (z. B. `src="../README.md"`) werden per `join(siteDir, normalize(reference))` außerhalb von `site/` aufgelöst. Existiert die Datei im Repository, gilt der Check als bestanden, obwohl das Pages-Artefakt nur `site/` enthält und die referenzierte Datei deployed nicht erreichbar wäre — falsch-negative Freigabe.

  Empfehlung: Referenzen ablehnen, deren normalisierter Pfad mit `..` beginnt bzw. `resolve(siteDir, reference)` nicht innerhalb von `siteDir` liegt.

### site/screenshots/.gitkeep

- **Toter Code** — Die Datei ist überflüssig, seit echte PNGs (`home-light.png` u. a.) eingecheckt sind; `.gitkeep` hatte nur die Aufgabe, das leere Verzeichnis zu versionieren.

  Empfehlung: Datei löschen.

## Geprüfte Dateien

- `.github/workflows/pages.yml`
- `site/index.html`
- `site/site.css`
- `site/theme.js`
- `site/favicon.png` (Existenz/Referenz)
- `site/screenshots/.gitkeep`, `site/screenshots/*.png` (Existenz/Referenz)
- `scripts/verify-site.mjs`
- `scripts/verify-site.test.mjs`
- `src/Schnittstellenzentrale.Tests/Playwright/SiteScreenshotTests.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/SitePageTests.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightTestBase.cs`
- `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/TestDatabaseSeeder.cs`
- `docs/help/github-page/index.md`
- `docs/help/github-page/beschreibung.md`
- `docs/help/github-page/ablauf-technisch.md`
- `docs/help/index.md`
- `docs/RELEASE_NOTES.md`
- `README.md`

## Anmerkungen (keine Befunde)

- Workflow-Korrektheit bestätigt: `environment`/`url` als Job-Level-Keys, `id: deployment` am `deploy-pages`-Step, Permissions `pages: write`/`id-token: write`/`actions: read`, `concurrency`-Gruppe `pages` mit `cancel-in-progress: false` — entspricht dem Starter-Workflow. Alle gepinnten Action-Majors existieren und sind aktuell (`checkout@v7`, `setup-node@v7`, `configure-pages@v6`, `upload-pages-artifact@v5`, `deploy-pages@v5`; Stand 2026-11).
- `site/index.html` nutzt ausschließlich relative Asset-Pfade (Pages-Unterverzeichnis `/Schnittstellenzentrale/` tauglich), keine `http://`-Links, kein Inline-JS mit Nutzereingaben. Inline-Theme-Init ist bewusst vor dem Stylesheet, um Theme-Flackern zu vermeiden.
- `site/site.css` verwendet keine `.sz-*`-Klassen — die Seite ist bewusst eigenständig (PostToolUse-Hook für `.sz-*` greift nur auf das App-Projekt). Alle in `SiteScreenshotTests` verwendeten `.sz-*`-Selektoren existieren in der App (`ApplicationGroupTree.razor`, `CollapsibleSection.razor`).
- `node scripts/verify-site.mjs` läuft lokal fehlerfrei (Exit 0); `node --test scripts/verify-site.test.mjs`: 10/10 Tests grün.
- `dotnet build` des Testprojekts: 0 Warnungen, 0 Fehler — CS1591-Doku-Pflicht ist in allen neuen öffentlichen Membern erfüllt.
- Seeder-Idempotenz: `SeedShowcaseDataAsync` räumt Nicht-System-Gruppen (kaskadierend via `DeleteGroupAsync` mit `ApplicationGroupDeleteMode.Delete`), ungruppierte Anwendungen und sämtliche Umgebungen vorher weg; System-Einträge bleiben durch `IsSystem`-Filter erhalten.
