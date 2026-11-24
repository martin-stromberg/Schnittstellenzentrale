# Umsetzungsplan — Schritt 7: Microsoft.Playwright 1.52.0 → 1.65.1

## Scope-Klärung

- **Verarbeitungseinheit:** NuGet-Paketupdate `Microsoft.Playwright` im Testprojekt inkl. Verifikation aller Playwright-Tests.
- **Auslösendes Ereignis:** Kundenanforderung F1 (Issue #108, Schritt 7).
- **Erwartete Wirkung:** Paket auf 1.65.1, passende Chromium-Binaries lokal installiert, beide Test-Lanes nachweisbar grün, Breaking-Changes-Sichtung dokumentiert.
- **Scope-Ausschlüsse (bewusst, keine Nutzerbestätigung nötig — technisch begründet):**
  - Keine Migration auf neue APIs (Screencast, `Locator.Visible`, `FrameLocator()` ohne Selektor, Tracing-`AriaSnapshots`) — reines Versionsupdate, keine Scope-Erweiterung.
  - Keine `[Flaky]`-Neuvergabe ohne dokumentiertes Ereignis (Repo-Konvention).
  - Keine CI-Workflow-Änderung — Browser-Installation läuft über das bestehende Build-Target; es existiert kein Browser-Cache in der CI (Befund inventory).

## Umsetzungsschritte

1. `Schnittstellenzentrale.Tests.csproj`: `Microsoft.Playwright` 1.52.0 → **1.65.1**.
2. `dotnet restore` + `dotnet build Schnittstellenzentrale.slnx -c Release` — löst `InstallPlaywright`-Target aus → neue Chromium-Binaries (Chromium 159.x) nach `%LOCALAPPDATA%/ms-playwright/`; Kompilierbarkeit beweist zugleich, dass keine entfernte API verwendet wird.
3. Falls Compile-/Laufzeit-Brüche auftreten: betroffene Aufrufe in `Playwright/` bzw. `Infrastructure/` anpassen (erwartet: keine — Audit-Befund).
4. Reguläre Lane: `dotnet test -c Release --filter "Category!=OsInterface"` → 641 Tests grün.
5. OsInterface-Lane: `dotnet test -c Release --filter "Category=OsInterface"` → grün. Bei Einzelfehlschlägen: Fehlerbild analysieren — echter Bruch → Aufruf anpassen; Flake → dokumentieren, ggf. `scripts/Retry-FlakyTests.ps1` einmalig.
6. `dotnet list package --vulnerable` → ohne Befund.

## Akzeptanzszenarien (überprüfbar)

- **A1 (Happy Path):** `dotnet test --filter "Category!=OsInterface"` → 641/641 grün (Baseline).
- **A2 (reale E2E-Abnahme):** `dotnet test --filter "Category=OsInterface"` lokal vollständig grün — 24 Playwright-Testklassen mit echtem Chromium (neue Revision) + Kestrel 5099–5104. Alternativ dokumentierte best-effort-Bewertung je Befund; echte Brüche werden vorab behoben.
- **A3:** `%LOCALAPPDATA%/ms-playwright/` enthält die zu 1.65.1 passende Chromium-Revision (Install-Output `playwright.ps1 install chromium` bzw. `install --list` Nachweis); Tests laufen tatsächlich mit neuem Browser (implizit durch A2).
- **A4:** CI-Cache-Befund dokumentiert: kein `actions/cache` für `ms-playwright` in den Workflows → keine Invalidierung nötig; Browser-Installation je Lauf frisch über Build-Target.
- **A5:** `dotnet list package --vulnerable` → keine verwundbaren Pakete.

## Lokales Gate (Merge-Pflicht, für PR-Text)

> Da die OsInterface-CI-Lane best-effort (`continue-on-error`) läuft, gilt als verbindliches Gate: `dotnet test --filter "Category=OsInterface"` muss **lokal vollständig grün** gelaufen sein, bevor der PR gemergt wird. Ergebnis/TRX sind im PR zu verlinken. Nachweis für diesen Schritt: `test-results.md` (Lauf-Datum, Testzahlen, TRX-Pfad).

## Risiken

- Einzelne OsInterface-Tests können durch die Chrome-for-Testing-Umstellung (1.57) oder Accessible-Name-Änderungen (1.65) brechen → dann gezielte Anpassung der betroffenen Locator/Assertions.
- Erhöhter Zeitbedarf: Browser-Download (~150–200 MB) + OsInterface-Laufzeit (~24 Klassen).

## Offene Punkte

Keine.

## Dateien

- `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj` (Version-Bump)
- ggf. `src/Schnittstellenzentrale.Tests/Playwright/**` (nur bei echten Brüchen)
- `docs/help/stabilitaet/os-interface-tests.md` (Versionsstand/Gate-Hinweis)
- `docs/RELEASE_NOTES.md` (Eintrag)
