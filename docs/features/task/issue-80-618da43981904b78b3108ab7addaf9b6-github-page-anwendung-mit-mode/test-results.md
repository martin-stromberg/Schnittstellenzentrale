# Test-Ergebnisse

- **Datum:** 2026-11-19 (Lauf 2 — Wiederholung nach Code-Review-Fixes; Ergebnis Lauf 1 überschrieben)
- **Branch:** `task/issue-80-618da43981904b78b3108ab7addaf9b6-github-page-anwendung-mit-mode` (Basis `staging`)
- **Geprüfter Stand:** Uncommitted Working Tree (Implementierung Issue #80 inkl. Review-Fixes)
- **Umgebung:** Windows, Git Bash, .NET 10.0.15 (SDK 10.0.404), Node v24.15.0, Playwright 1.65.1 (npx) / Microsoft.Playwright 1.52.0, Chromium headless (`CI=true`)
- **Geänderte Dateien seit Lauf 1:** `SiteScreenshotTests.cs` (ExpandGroupAsync-Parametrisierung), `SitePageTests.cs` (Duplikate entfernt, TestPaths genutzt), `TestDatabaseSeeder.cs` (SeedShowcaseDataAsync in Hilfsmethoden zerlegt), `PlaywrightTestBase.cs` (IsHeadless/FindRepoRoot an TestPaths delegiert), neu `TestPaths.cs`, `pages.yml` (paths-Filter erweitert), `verify-site.mjs` (Containment-Check), `verify-site.test.mjs` (neuer Testfall), `site/screenshots/.gitkeep` gelöscht.

## Ergebnis

**Status:** Keine Fehler

## Fehlgeschlagene Tests

Keine.

## Testläufe

| Lauf | Befehl | Ergebnis |
|------|--------|----------|
| Build | `dotnet build Schnittstellenzentrale.slnx` | Erfolgreich, 0 Fehler, 0 Warnungen |
| Unit/Integration | `dotnet test --no-build --filter "FullyQualifiedName!~Playwright" --collect:"XPlat Code Coverage"` | 654/654 bestanden (Baseline 654 eingehalten, 0 fehlgeschlagen, 0 übersprungen) |
| Volle Playwright-Suite | `CI=true dotnet test --no-build --filter "FullyQualifiedName~Playwright"` | 73/73 bestanden (inkl. 5× `SitePageTests`, 4× `SiteScreenshotTests`; kein Regressionsbruch durch `PlaywrightTestBase`-/`TestDatabaseSeeder`-Umbau) |
| Node-Tests | `node --test scripts/verify-site.test.mjs` | 11/11 bestanden (inkl. neuem Containment-Testfall „fails when an asset reference escapes the site directory") |
| Verify-Skript (voller Lauf inkl. Doku-Block) | `node scripts/verify-site.mjs` | Exit 0 — „Seiten- und Doku-Artefakte OK" |

## E2E-Abdeckung

| Szenario | Test / Testklasse | Ergebnis |
|----------|-------------------|----------|
| Seite rendern, alle Inhalte sichtbar | `SitePageTests.Index_ZeigtAllePflichtsektionen` | Bestanden |
| Screenshot-Galerie lädt echte PNGs | `SitePageTests.Screenshots_SindGeladen` | Bestanden |
| Dark Mode umschalten + persistieren | `SitePageTests.DarkToggle_SchaltetUm_UndPersistiert` | Bestanden |
| Responsives Layout | `SitePageTests.MobileViewport_KeinHorizontalScroll` | Bestanden |
| Download-/Release-Verweis + Footer-Links | `SitePageTests.DownloadLink_ZeigtAufLatestRelease` | Bestanden |
| Screenshots aus der echten App | `SiteScreenshotTests` (4 Facts: `Workspaces_Light_Screenshot`, `ApplicationDetail_Light_Screenshot`, `EndpointPage_Light_Screenshot`, `Workspaces_Dark_Screenshot`) | Bestanden — PNGs im Lauf neu geschrieben |
| Dokumentierter Aktualisierungsweg | `verify-site.mjs` Doku-Artefakt-Block + `verify-site.test.mjs` (11 Tests) | Bestanden |
| Remote-Deploy nach Merge (öffentliche Erreichbarkeit) | `pages.yml`-Lauf + Abruf der Pages-URL | Nicht ausgeführt — nachgelagerter Schritt, erst nach Merge auf `staging` verifizierbar; kein Teil der lokalen Abnahme |

## Reale End-to-End-Abnahme

Durchgeführt über: echten statischen HTTP-Server (Node `http`, `site/` als Document Root, `http://127.0.0.1:8321`) + realem Chromium (Playwright 1.65.1, installierte Browser-Binary, headless) — nicht über das Test-Framework, sondern eigenes Abnahmeskript (19 Checks) plus visuelle Sichtprüfung der gerenderten Full-Page-Screenshots (hell/dunkel/mobil).

| Szenario (plan.md Akzeptanzszenarien) | Durchgeführt über | Ergebnis |
|---------------------------------------|-------------------|----------|
| 1 — Happy Path lokal: Seite ansehen (Vorstellung, Feature-Kacheln, Galerie mit echten App-Screenshots, Download-Verweis `releases/latest` + `Schnittstellenzentrale-vX.Y.Z.zip`-Hinweis, Footer-Links Repository/`docs/help/`/`LEGAL.md`) | Statischer Server + echter Browser; 19/19 Checks bestanden: `lang="de"`, Viewport-Meta, Hero sichtbar, 9 Feature-Kacheln + Galerie-Container (10 `[class*="feature"]`-Treffer), 5 Galerie-`img` davon 4× PNG `naturalWidth=1440` + 1 Badge-Bild, `releases/latest` + Asset-Hinweis + alle Footer-Ziele | Bestanden |
| 1 — Dark-Toggle + Persistenz | Klick auf Toggle setzt `.dark` auf `<html>`, `localStorage.colorScheme="Dark"`; Body-Hintergrund `oklch(1 0 0)` → `oklch(0.145 0 0)`; nach Reload weiterhin Dark; zweiter Klick zurück auf `Light`; visuelle Sichtprüfung Dark-Rendering korrekt | Bestanden |
| 1 — Responsivität | Viewport 390×844: `scrollWidth=390 <= clientWidth=390`, kein Horizontal-Scroll, Kerninhalte sichtbar; Sichtprüfung einspaltiges Layout korrekt | Bestanden |
| 2 — Deployment-Pfad nach Merge | Nicht lokal durchführbar (erfordert Merge auf `staging`) | Nachgelagert — Remote-Deploy ist kein Teil der lokalen Abnahme (siehe E2E-Abdeckung); `pages.yml` statisch geprüft im Lauf 1, paths-Filter seitdem erweitert und durch `verify-site.mjs`-Grünlauf abgesichert |
| 3 — Deploy ohne auslösende Seitenänderung | `paths`-Filter in `pages.yml` kontrolliert (`site/**`, `pages.yml`, `scripts/verify-site*.mjs`, `docs/help/github-page/**`, `docs/help/index.md`, `README.md`, `docs/RELEASE_NOTES.md` — seit Lauf 1 um die vom Verify-Skript geprüften Doku-Artefakte erweitert) | Statisch bestätigt; Laufzeitverhalten erst nach Merge beobachtbar (nachgelagert) |
| 4 — Fehlerfall Deploy bei kaputtem `site/` | `verify-site.mjs` läuft im Workflow **vor** `configure-pages`; Fehlerfälle durch `verify-site.test.mjs` nachgewiesen (fehlendes PNG, fehlendes `lang="de"`, fehlende Sektion, CDN-/Bootstrap-Referenz, Pfadausbruch aus `site/` [Containment, neu], fehlende Doku-Datei/Stichwort, fehlender Release-Notes-Eintrag) | Bestanden (Gate-Logik lokal verifiziert) |
| 5 — Screenshot-Regenerierung | `dotnet test --filter "FullyQualifiedName~Playwright"` (enthält `SiteScreenshotTests`) im Lauf ausgeführt; PNGs wurden in `site/screenshots/` neu geschrieben (Zeitstempel 20:57, aktualisierte Dateigrößen) | Bestanden |
| 6 — Parallele Deploys | `concurrency: { group: pages, cancel-in-progress: false }` in `pages.yml` | Statisch bestätigt; Laufzeitverhalten nachgelagert |
| 7 — Dokumentierter Aktualisierungsweg | `docs/help/github-page/{index,beschreibung,ablauf-technisch}.md` vorhanden; Link in `docs/help/index.md`; `## GitHub Page` in `README.md`; `RELEASE_NOTES.md`-Eintrag (EN+DE); `verify-site.mjs` Doku-Block grün | Bestanden |

## Screenshot-Artefakte (`site/screenshots/`)

Alle vier PNGs wurden im Playwright-Lauf neu erzeugt und sind echte App-Screenshots (visuell verifiziert): PNG-Format, 1440×900 px, nicht leer, zeigen die laufende Anwendung mit Demo-Daten:

- `home-light.png` (37 KB): Workspaces-Ansicht, aufgeklappter Baum (Kundendienste → Kundenportal API → Stammdaten mit GET/POST/PUT/DELETE-Endpunkten), hell
- `application-light.png` (58 KB): Anwendungsdetail „Kundenportal API"
- `endpoint-light.png` (46 KB): Endpunktansicht „GET /api/kunden" mit Methoden-Dropdown und Anfrage-Tabs
- `home-dark.png` (39 KB): Workspaces-Ansicht im Dark Mode

Das zuvor vorhandene `site/screenshots/.gitkeep` ist wie geplant entfernt — der Ordner enthält nun ausschließlich echte PNGs.

## Zusammenfassung

- Gesamt: 738 automatisierte Tests + 19 reale Abnahmechecks
- Bestanden: 654 (Unit/Integration) + 73 (Playwright, enthält die 9 neuen Site-Tests) + 11 (Node) + 19 (reale Abnahme)
- Fehlgeschlagen: 0
- Übersprungen: 0
- Nachgelagert (dokumentiert, nicht Teil der lokalen Abnahme): Remote-Deploy-Verifikation nach Merge auf `staging` (`pages.yml`-Lauf, HTTP-200-Abruf der Pages-URL), Laufzeitverhalten der `paths`-/`concurrency`-Filter

## Testabdeckung

**Abdeckung:** 72,8 % Zeilen (Cobertura, `XPlat Code Coverage`, nur Nicht-Playwright-Suite; 11.446/15.718 Zeilen)

Hinweis: Die Messung umfasst nur die Nicht-Playwright-Suite. UI-Code (`.razor`-Komponenten) wird überwiegend durch die Playwright-Suite ausgeübt, die nicht in dieser Cobertura-Datei enthalten ist — die vielen 0 %-Klasseneinträge betreffen daher primär Razor-Komponenten und teilweise per Playwright getestete Services. Die CI erzwingt separat 70 % Line Coverage; der Gesamtwert liegt darüber.

## Fehlende Tests

Quelle: `Dateinamen-Konvention` + Coverage-Daten (auf die neuen/geänderten Artefakte bezogen)

- Keine. Alle geänderten Artefakte sind testseitig abgedeckt: `TestPaths.cs` indirekt über `SitePageTests`/`SiteScreenshotTests` (die es nutzen) und die volle 73er-Playwright-Suite, `verify-site.mjs` (Containment-Check) über den neuen Testfall in `verify-site.test.mjs`, `pages.yml`-paths-Filter statisch geprüft. `SiteScreenshotTests`/`SitePageTests`/`TestDatabaseSeeder`/`PlaywrightTestBase` sind selbst Testinfrastruktur.
- Bestandslücke (nicht Teil dieser Änderung): zahlreiche `.razor`-Komponenten zeigen in der Unit-Test-Coverage 0 %; reale Ausübung erfolgt über Playwright/bUnit außerhalb dieser Messung.
