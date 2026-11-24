# Anforderung (übersetzt)

## Ausgangslage

Issue #108, Entwicklungsschritt 7 (letzter Schritt der High-Risk-NuGet-Update-Serie). Schritte 1–6 sind gemergt: net11.0, SDK 11.0.100, FrameworkReference, OData stabil, Serilog 11, Microsoft.OpenApi 3.11.1 + Swashbuckle 11.0.1, xunit.runner.visualstudio 4.0.1 (xunit bleibt 2.9.3).

## F1 – Microsoft.Playwright → 1.65.1

### Ziel und Umfang

- `Microsoft.Playwright` im Testprojekt `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj` von **1.52.0 auf 1.65.1** heben.
- Vor der Umsetzung Release-Notes/Breaking Changes aller Zwischenreleases **1.53–1.64** vollständig sichten (bisher nur 1.65 dokumentiert) und gegen die ~600 Locator-/Action-Aufrufe in `src/Schnittstellenzentrale.Tests/Playwright/` prüfen (Locator 316, ClickAsync 212, Expect 150, ToBeVisibleAsync 117, GetByRole 106, FillAsync 62 u. a.).
- Betroffene Aufrufe und Infrastruktur (`PlaywrightTestBase`, `PlaywrightServer`, `PlaywrightApiFactory`, `PlaywrightSignalRServer`) anpassen, soweit nötig.
- Das `InstallPlaywright`-Target (`playwright.ps1 install chromium`, abschaltbar via `-p:SkipPlaywrightInstall=true`) installiert automatisch die zum neuen Paket passenden Browser — lokale Browser-Installation erneuern und ggf. den CI-Cache invalidieren.
- Da die OsInterface-CI-Lane nur best-effort läuft (`continue-on-error`), ist als verbindliches Absicherungs-Vorgehen ein **lokales Gate als Merge-Pflicht** zu definieren und im PR festzuhalten: `dotnet test --filter "Category=OsInterface"` muss lokal vollständig grün gelaufen sein, bevor der PR gemergt wird (Ergebnis/TRX im PR verlinken).

### Akzeptanzkriterien

1. `dotnet test --filter "Category!=OsInterface"` vollständig grün (641-Test-Baseline).
2. Lokaler Lauf `dotnet test --filter "Category=OsInterface"` grün — bzw. dokumentierte best-effort-Bewertung mit Begründung je Befund.
3. Neue Chromium-Binaries über das Build-Target installiert und lauffähig.
4. CI-Cache-Befund beurteilt/dokumentiert.
5. `dotnet list package --vulnerable` ohne Befund.

### Hinweise/Randbedingungen

- Die OsInterface-Lane lokal ausführen ist die reale E2E-Abnahme des Schritts: `PlaywrightServer` (Kestrel Port 5099) + Chromium starten; Browser werden beim Testbuild automatisch installiert (InstallPlaywright-Target).
- Bekannte 1.65-Breaking-Changes: Locator-Aktionen warten auf nicht-inerte Elemente; Cookie-Matching RFC 6265; ungerenderter Text fällt aus Accessible Names/Aria-Snapshots (GetByRole mit Name); ParamsString wird angehängt statt ersetzt; Proxy-Credentials nur an Proxy-Challenges. `ToMatchAriaSnapshotAsync` wird im Repo nicht genutzt (0 Treffer).
- Flaky-Konvention des Repos beachten (`Flaky`-Trait nur mit `OsInterface`, Einmal-Retry via `scripts/Retry-FlakyTests.ps1`, siehe `docs/help/stabilitaet/os-interface-tests.md`). Bei echten (nicht flaky) Brüchen betroffene Aufrufe anpassen.
- Lokales Gate als Merge-Pflicht im Abschlussbericht dokumentieren (für den PR-Text).
- Kein Issue-Kommentar, kein Merge/Push/PR. Endzustand: Lifecycle vollständig inkl. Abschlusscommit `feat: ...` auf dem Schritt-Branch.
