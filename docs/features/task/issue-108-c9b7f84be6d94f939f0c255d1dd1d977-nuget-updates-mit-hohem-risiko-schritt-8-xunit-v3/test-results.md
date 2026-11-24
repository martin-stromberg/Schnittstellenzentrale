# Testergebnisse – Schritt 8 (xunit.v3-Migration)

**Modus:** Vollständige lokale Läufe durch den ausführenden Agenten (keine Unteragenten).

## E2E-Abnahme

**Nicht anwendbar** — der Schritt ändert ausschließlich Test-Infrastruktur; kein Produktcode, keine UI. Als Ersatzabnahme gilt der Nachweis, dass beide Test-Lanes unter xunit.v3/MTP unverändert vollständig selektiert werden und grün laufen (siehe unten).

## Reguläre Lane (blockierend)

```powershell
dotnet test --project src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj `
  -p:SkipPlaywrightInstall=true `
  --results-directory src/Schnittstellenzentrale.Tests/TestResults `
  -- --filter "Category!=OsInterface" `
     --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml `
     --report-trx --report-trx-filename test-results.trx
```

**Ergebnis:** `erfolgreich [+641/x0/?0]` — **641/641 grün** (Baseline: 641), 0 fehlgeschlagen, 0 übersprungen, ~26 s. Artefakte: `test-results.trx` + `coverage.cobertura.xml` (valide Cobertura).

**Coverage:** `line-rate="0.7251"` → **72,5 % Line-Coverage ≥ 70-%-Gate** ✓

## OS-Schnittstellen-Lane (best-effort; lokales Gate)

```powershell
dotnet test --project src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj `
  -p:SkipPlaywrightInstall=true `
  --results-directory src/Schnittstellenzentrale.Tests/TestResults `
  -- --filter "Category=OsInterface" --report-trx --report-trx-filename test-results-os-interface.trx
```

**Ergebnis:** `erfolgreich [+94/x0/?0]` — **94/94 grün** (Baseline: 94), ~2 min. Artefakt: `test-results-os-interface.trx`. Zwei erwartete `TempDirectoryCleanup`-Restfehler-Meldungen (beabsichtigte Lock-Testartefakte, best-effort).

## Selektions-/Filterverifikation (Probes)

- `--filter "Category=OsInterface"` → exakt 94 Tests; `Category!=OsInterface` → exakt 641 (Parität zur VSTest-Selektion unter xunit 2.9.3).
- Compound-Filter `(Category=Flaky)&(FullyQualifiedName~…)` funktioniert unverändert (VSTest-Syntax hinter `--`).
- Leere Treffermenge → MTP-Exitcode 8 (in `TestDiscovery.ps1` belanglos: leere Ausgabe → leere Menge).
- `--list-tests` liefert FQN-Liste für die Flaky-Discovery.

## Paket-Sicherheitscheck

`dotnet list package --vulnerable` → **keine anfälligen Pakete** in allen vier Projekten.

## Build

Warnungsfreier Build der Solution nach Migration (Debug; die `xunit.analyzers` aus xunit.v3 melden keine Befunde).

## Fazit

Alle Akzeptanzkriterien erfüllt: beide Lanes vollständig grün mit identischer Testmenge, Coverage-Gate ≥70 % über MTP-Extension erzeugt, TRX-Artefakte für Retry-/Analyzer-Pipeline vorhanden, keine Vulnerabilities.
