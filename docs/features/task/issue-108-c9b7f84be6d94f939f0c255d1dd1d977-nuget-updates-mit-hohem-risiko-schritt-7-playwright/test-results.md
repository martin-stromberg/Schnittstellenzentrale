# Testergebnisse — Schritt 7: Microsoft.Playwright → 1.65.1

**Status: Keine Fehler**

Datum der Läufe: 2026 (lokaler Rechner, Windows, .NET SDK 11.0.100, Release-Konfiguration).

## Reguläre Lane (blockierend)

```
dotnet test Schnittstellenzentrale.slnx -c Release --filter "Category!=OsInterface" --logger "trx;LogFileName=test-results-s7-regular.trx"
```

- **641/641 bestanden** (Baseline 641), 0 fehlgeschlagen, 0 übersprungen — 22 s.
- TRX: `src/Schnittstellenzentrale.Tests/TestResults/test-results-s7-regular.trx` (641× `outcome="Passed"`, 0× Failed — verifiziert per grep).

## OS-Schnittstellen-Lane (reale E2E-Abnahme dieses Schritts)

```
dotnet test Schnittstellenzentrale.slnx -c Release --filter "Category=OsInterface" --logger "trx;LogFileName=test-results-os-interface.trx"
```

- **94/94 bestanden** in 2:04 min, 0 fehlgeschlagen — alle 24 Playwright-Testklassen mit echtem Chromium (neue Revision 1255 / Chrome for Testing 159.0.8154.6) + echter Kestrel-Bindung (Ports 5099–5104).
- TRX: `src/Schnittstellenzentrale.Tests/TestResults/test-results-os-interface.trx` (94× `outcome="Passed"`, 0× Failed — verifiziert per grep).
- Kein einziger Test brach durch das Update 1.52.0 → 1.65.1 — das statische Audit (keine entfernten/veränderten APIs im Repo) wurde empirisch bestätigt. Kein Flaky-Retry nötig, kein `[Flaky]` vergeben.

## Reale End-to-End-Abnahme

Die OsInterface-Lane **ist** die reale End-to-End-Abnahme des Schritts: Die Tests fahren die Anwendung real (Kestrel 5099–5104, Blazor Server, echtes Chromium mit UI-Interaktionen — Anwendungs-CRUD, Dialoge, Navigation, Inplace-Editing, SignalR-Sync über zwei Browser-Kontexte, Icon-Upload, Impressum, OData-/Swagger-Import, Screenshot-Generatoren). Auslöser → sichtbare Wirkung wurde je Szenario bis zur Assertion durchgespielt; alle 94 Szenarien bestanden.

Zusätzlich verifiziert: `playwright.ps1 install --list` zeigt `chromium-1255` + `chromium_headless_shell-1255` + `ffmpeg-1013` + `winldd-1007` der Playwright-Version **1.65.1** zugeordnet (BrowserVersion 159.0.8154.6, „Chrome for Testing") — neue Binaries installiert und lauffähig (implizit durch die grüne Lane bewiesen).

## Sichtprüfung der Abnahme-Screenshots

Nicht anwendbar — die Abnahme besteht aus den automatisierten E2E-Tests (keine manuell angefertigten Abnahme-Screenshots). Die von `SiteScreenshotTests` regenerierten Site-PNGs (4 Dateien) wurden stichprobenartig gesichtet: keine Layout-Defekte, nur Rendering-Deltas des neuen Browsers (Schrift-/AA-Feinheiten).

## CI-Cache-Befund

`pr-staging-ci.yml` und `staging-ci.yml` enthalten **keinen** `actions/cache`-Eintrag für `ms-playwright` (einziger Cache: `cache: npm` im Docs-Job). Die Browser-Installation erfolgt je CI-Build frisch über das `InstallPlaywright`-Target — **kein Cache zu invalidieren**.

## Lokales Gate (Merge-Pflicht — für den PR-Text)

> Da die OsInterface-CI-Lane best-effort (`continue-on-error`) läuft, gilt für diesen Schritt als verbindliches Gate: `dotnet test --filter "Category=OsInterface"` musste **lokal vollständig grün** laufen, bevor der PR gemergt wird. **Nachweis erbracht:** 94/94 grün am {Datum des Laufs}; TRX `src/Schnittstellenzentrale.Tests/TestResults/test-results-os-interface.trx` (im PR zu verlinken/beizufügen).

## `dotnet list package --vulnerable`

Keine anfälligen Pakete in allen 4 Projekten (Schnittstellenzentrale, .Core, .Infrastructure, .Tests).

## Fehlgeschlagene Tests

Keine.
