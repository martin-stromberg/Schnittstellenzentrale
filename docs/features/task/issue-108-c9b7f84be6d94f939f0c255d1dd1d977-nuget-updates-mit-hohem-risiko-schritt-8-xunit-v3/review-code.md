# Code-Review – Schritt 8 (xunit.v3-Migration)

**Modus:** Selbstreview (keine Unteragenten startbar).

## Geprüfte Bereiche

### Trait-Infrastruktur (`Infrastructure/Testing/`)

- `OsInterfaceAttribute`/`FlakyAttribute`: implementieren `Attribute` + `Xunit.v3.ITraitAttribute`, liefern `("Category", …)` über `GetTraits()` — deckt sich mit dem xunit.v3-Referenzmuster (TraitExtensibilityExample). `Inherited = true` bleibt erhalten, damit Subklassen-Kategorisierung greift.
- `OsInterfaceFactAttribute`/`OsInterfaceTheoryAttribute`: erben `FactAttribute`/`TheoryAttribute` und liefern das `OsInterface`-Trait — methodenscharf wie zuvor.
- `CategoryTraitDiscoverer.cs` entfernt (v2-`ITraitDiscoverer`/`Xunit.Sdk` existieren nicht mehr — vollständig gelöscht statt Leiche).
- `TestTraitReader`: zwei Erkennungspfade — (a) rohes `[Trait("Category", …)]` via `CustomAttributeData` mit Vererbungsketten-Durchlauf, (b) `ITraitAttribute`-Instanzen via `GetTraits()`. Deckt beide dokumentierten Schreibweisen; `FlakyConventionTests`/`TraitParityTests` laufen grün (Teil der 641).

### IAsyncLifetime-Migration

- Alle `InitializeAsync`/`DisposeAsync` geben `ValueTask` zurück (Interface erbt jetzt `IAsyncDisposable`). Overrides in den `PlaywrightImpressum*Server`-Subklassen konsistent angehoben — Build ohne CS0508.
- `PlaywrightTestBase.DisposeAsync` ruft weiterhin die best-effort-Dispose-Kette; Semantik unverändert (nur Rückgabetyp).

### Projektdatei

- `OutputType=Exe` gesetzt (MTP-App erzeugt eigenen Entry Point), `xunit.v3` 4.0.3, MTP-Extensions gepinnt (CodeCoverage 18.12.0, TrxReport 2.5.3), `coverlet.collector`/`xunit`/`xunit.abstractions` entfernt, `Microsoft.NET.Test.Sdk` 18.5.1 + `xunit.runner.visualstudio` 4.0.1 bewusst behalten (IDE-Discovery).

### Workflows/Skripte

- Alle `dotnet test`-Aufrufe: `dotnet test --project|--solution … -- <test-app-args>`; `--results-directory` überall explizit, wo TRX-Pfade weiterverarbeitet werden (`-PrimaryTrx` in CI-Retry-Schritt).
- Filterausdrücke byte-identisch übernommen (VSTest-Syntax läuft unter xunit.v3 unverändert) — 641/94-Selektion empirisch bestätigt.
- `Retry-FlakyTests.ps1`: Retry-Lauf schreibt `<name>-flaky-retry.trx` via `--report-trx-filename` ins explizite `--results-directory`; Auswertelogik unverändert.

### Doku-Konsistenz

- README/CLAUDE.md/help-Docs zeigen durchgehend die `--`-Form; Beispielkommandos aus `os-interface-tests.md` entsprechen den tatsächlich ausgeführten (verifizierten) Befehlen.

## Befunde

- Keine offenen Befunde. Warnungsfreier Build, beide Lanes grün (siehe `test-results.md`).
