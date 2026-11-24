# Plan-Review — Schritt 7: Microsoft.Playwright → 1.65.1

**Status: Vollständig umgesetzt**

| Planelement | Umsetzung | Nachweis |
|-------------|-----------|----------|
| PackageReference 1.52.0 → 1.65.1 | `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj:29` | Diff |
| Release-Notes-Sichtung 1.53–1.65.1 | `inventory/release-notes-befund.md` — alle 14 Releases einzeln dokumentiert | Artefakt |
| ~600 Aufrufe geprüft, betroffene anpassen | API-Inventar + Entfernte-APIs-Audit (0 Treffer); OsInterface-Lauf 94/94 grün → **keine Anpassung nötig** | test-results.md |
| Infrastruktur anpassen (TestBase/Server/ApiFactory/SignalRServer) | Keine betroffene API in diesen Dateien — unverändert, lauffähig | Build + Lane grün |
| Browser-Neuinstallation | chromium-1255 + headless_shell-1255 + ffmpeg-1013 (Chromium 159.0.8154.6, Chrome for Testing) in `%LOCALAPPDATA%/ms-playwright/`; via Build-Target/`playwright.ps1 install chromium` | `install --list`-Nachweis |
| CI-Cache-Befund | Kein `actions/cache` für `ms-playwright` in pr-staging-ci/staging-ci → nichts zu invalidieren | inventory, Workflow-Grep |
| Lokales Gate als Merge-Pflicht | In plan.md + test-results.md dokumentiert (PR-Text-Baustein) | Artefakte |
| Akzeptanz A1: reguläre Lane 641 grün | 641/641 grün, 22 s | test-results-s7-regular.trx |
| Akzeptanz A2: OsInterface lokal grün | 94/94 grün, 2:04 min | test-results-os-interface.trx |
| Akzeptanz A5: vulnerable ohne Befund | 4 Projekte ohne Befund | `dotnet list package --vulnerable` |

## Offene Aufgaben

Keine.

## Anmerkungen

- Nebenbefund: `SiteScreenshotTests` regenerierten `site/screenshots/*.png` (4 Dateien) mit dem neuen Chrome-for-Testing-Renderer — legitimer Nebeneffekt des Updates, wird mitcommittet.
