# Bestandsaufnahme: GitHub-Page-Präsentationsseite (Issue #80)

Analysiert wurden die Bereiche GitHub-Actions-Workflows, CSS-/Theme-System, Playwright-Infrastruktur (für Screenshot-Erzeugung), Dokumentationsstruktur und wiederverwendbare Assets — bezogen auf die Anforderung einer statischen GitHub-Pages-Präsentationsseite (`requirement.md`). Es gibt keine betroffenen C#-Domänenklassen; die Anforderung betrifft ausschließlich neue statische Web-Artefakte und einen neuen Deployment-Workflow.

## Zusammenfassung

- **Keine GitHub-Pages-Basis vorhanden:** Kein Pages-Workflow, kein `site/`- oder `docs/page/`-Ordner, keine `_config.yml`/`CNAME`/`.nojekyll`, keine `pages:`-Permission oder `github-pages`-Environment irgendwo in `.github/` — die Seite und ihr Deployment müssen vollständig neu erzeugt werden. Siehe [Workflows](inventory/workflows.md).
- **Workflow-Konventionen klar erkennbar:** 10 bestehende Workflows mit einheitlichem Muster (englischer Kommentarkopf, minimale `permissions`, `concurrency`-Gruppe, `timeout-minutes`, `ubuntu-latest` für Orchestrierung / `windows-latest` für .NET, Actions `@v6`/`@v7`, `pwsh`-Shell). Siehe [Workflows](inventory/workflows.md).
- **Design-System wiederverwendbar:** Vollständige shadcn-Token (`oklch`, light + `.dark`) in `shadcn/default-theme.css`, eigene `sz-*`-Klassen und -Variablen in `app.css`, `.dark`-Mechanik über `theme.js` mit `localStorage`-Schlüssel `colorScheme`. Auffällig: Die Sidebar-Gradient-Variablen (`--sz-sidebar-bg-*`) sind definiert, werden aber im aktuellen Code nicht mehr referenziert. `prefers-color-scheme` wird in der App **nicht** ausgewertet — ohne gespeicherte Wahl gilt immer Light. Siehe [Design-System](inventory/design-system.md).
- **Playwright-Infrastruktur einsatzbereit für Screenshots:** `PlaywrightServer` startet die echte App per Kestrel auf `http://127.0.0.1:5099` mit Test-Auth und benannter In-Memory-SQLite-DB; `TestDatabaseSeeder.ResetAsync()` liefert reproduzierbaren Datenstand (allerdings minimal: nur Systemgruppe + ein Systemendpunkt). `Page.ScreenshotAsync()` wird bisher nirgends genutzt — nur Playwright-Tracing-Screenshots. Siehe [Playwright-Infrastruktur](inventory/playwright.md).
- **Inhaltsquellen vorhanden:** `README.md` (Features-Liste, Badges, Release-Asset-Name `Schnittstellenzentrale-vX.Y.Z.zip`), `docs/help/` mit 15 Themengebieten und `index.md`-Konvention, `docs/RELEASE_NOTES.md` (bilingual EN/DE), `LEGAL.md`. Einziges Bild-Asset: `favicon.png` (32×32). Kein `LICENSE`-File im Repo-Root. Siehe [Dokumentation und Inhalte](inventory/docs-and-content.md).

**Test-Ausgangszustand:** Beide Testsuiten wurden am 19.11.2026 ausgeführt und sind vollständig grün — 654/654 Unit-/Integrationstests und 64/64 Playwright-Tests bestanden, keine Fehlschläge, keine übersprungenen Tests. Nachweis: [Tests](inventory/tests.md).

## Details

- [Workflow-Konventionen (.github/workflows/)](inventory/workflows.md)
- [Design-System (app.css, shadcn, Theme-Mechanik)](inventory/design-system.md)
- [Playwright-Infrastruktur (Screenshot-Basis)](inventory/playwright.md)
- [Dokumentation, Inhaltsquellen und Assets](inventory/docs-and-content.md)
- [Tests und Test-Ausgangszustand](inventory/tests.md)
