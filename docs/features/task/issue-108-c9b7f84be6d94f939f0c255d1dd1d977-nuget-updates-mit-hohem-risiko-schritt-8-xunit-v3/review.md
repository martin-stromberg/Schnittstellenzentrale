# Plan-Review – Schritt 8 (xunit.v3-Migration)

**Modus:** Selbstreview (keine Unteragenten startbar — Abweichung dokumentiert in `todo.md`).

## Abgleich Plan ↔ Umsetzung

| Planpunkt | Umsetzung | Status |
|---|---|---|
| `xunit` → `xunit.v3` 4.0.3 | `xunit.v3` 4.0.3 (bringt `xunit.v3.mtp-v2` mit), `xunit.abstractions` entfällt | OK |
| `OutputType Exe` im Testprojekt | gesetzt inkl. Kommentar (MTP-App, Test-Assembly ist eigenständiger Runner) | OK |
| MTP-Reporter-Pakete | `Microsoft.Testing.Extensions.CodeCoverage` 18.12.0 + `TrxReport` 2.5.3; `coverlet.collector` entfernt | OK |
| `xunit.runner.visualstudio` / `Microsoft.NET.Test.Sdk` | beide bewusst beibehalten (IDE-Discovery, siehe decisions) | OK |
| `IAsyncLifetime` → `ValueTask` | `PlaywrightServer` + Overrides, `PlaywrightTestBase`, `EndpointExecutionIntegrationTests` | OK |
| `Xunit.Abstractions` → `Xunit` | `using Xunit.Abstractions;` in allen betroffenen Dateien entfernt (ITestOutputHelper liegt jetzt unter `Xunit`) | OK |
| Trait-Infrastruktur auf `Xunit.v3.ITraitAttribute` | `CategoryTraitDiscoverer.cs` gelöscht; `OsInterface`/`Flaky` implementieren `ITraitAttribute`; `OsInterfaceFact`/`OsInterfaceTheory` erben `Fact`/`Theory` + `ITraitAttribute`; `TestTraitReader` liest `[Trait]` per `CustomAttributeData` (inkl. Vererbungskette) und `GetTraits()` | OK |
| CI-Workflows auf MTP-CLI | `staging-ci.yml`, `pr-staging-ci.yml`, `release.yml`: Filter/Reporter hinter `--`, `--results-directory` explizit, TRX via `--report-trx`, Coverage via `--coverage --coverage-output-format cobertura` | OK |
| Retry-/Discovery-Skripte | `TestDiscovery.ps1` (`--list-tests -- --filter`), `Retry-FlakyTests.ps1` (`--filter` + `--report-trx` hinter `--`, `--results-directory`) | OK |
| Dokumentation | CLAUDE.md, README.md, `docs/help/stabilitaet/os-interface-tests.md`, `docs/help/playwright-tests/*` migriert; RELEASE_NOTES ergänzt | OK |
| Verifikationsprobes | Filterselektion (641/94), Compound-Filter, TRX, Cobertura, Exit-Code-8-Verhalten empirisch geprüft | OK |

## Abweichungen vom Plan

- `Microsoft.Testing.Extensions.TrxReport` wurde zusätzlich referenziert — im Plan implizit (TRX-Artefakte waren Pflicht), empirisch nötig.
- `--filter` bleibt VSTest-Syntax (im Plan als offene Frage markiert → empirisch bestätigt, inkl. Compound-Filter `&`).

## Restrisiken / offene Punkte

- MTP-Exitcode 8 bei leerer Filterauswahl: für `TestDiscovery.ps1` belanglos (leere Ausgabe → leere Menge), dokumentiert.
- `xunit.runner.visualstudio` bleibt für IDE-Test-Discovery referenziert; sollte die IDE-Sicht stören, wäre das ein Follow-up.
- Ergebnis: **Plan vollständig umgesetzt, keine Blocker.**
