# Abnahmeprüfung – Entwicklungsschritt 8

## Ergebnis
**Status:** Anforderung vollständig erfüllt

## Abweichungen
- Lifecycle in Eigenregie (keine Unteragenten startbar) — als Abweichung im Schritt-`todo.md` dokumentiert; Abnahme erfolgt anhand der im feat-Commit `ec7c46c` enthaltenen Nachweise.

## Hinweise
- **Diff-Umfang:** `git diff 9f634d2~1..HEAD`-Summe umfasst ausschließlich die geplanten Bereiche: `Schnittstellenzentrale.Tests.csproj` (xunit→xunit.v3, `OutputType=Exe`, MTP-Extensions), Trait-Infrastruktur unter `Infrastructure/Testing/` (`CategoryTraitDiscoverer.cs` entfernt, Attribute auf `Xunit.v3.ITraitAttribute`), `IAsyncLifetime`-/`using`-Anpassungen in Testdateien, die drei CI-Workflows, `TestDiscovery.ps1`/`Retry-FlakyTests.ps1`, Doku (`CLAUDE.md`, `README.md`, `os-interface-tests.md`, `playwright-tests/*`), `RELEASE_NOTES.md` (EN+DE) und `decisions.md` (F2-Eintrag). **Kein Produktcode geändert** — konsistent mit dem reinen Infrastruktur-Scope.
- **csproj:** `xunit.v3` **4.0.3** (letztes stabiles Release, bringt `xunit.v3.mtp-v2`), `xunit`/`xunit.abstractions`/`coverlet.collector` entfernt; `Microsoft.Testing.Extensions.CodeCoverage` 18.12.0 + `Microsoft.Testing.Extensions.TrxReport` 2.5.3 neu; `xunit.runner.visualstudio` 4.0.1 und `Microsoft.NET.Test.Sdk` 18.5.1 bewusst behalten (IDE-Discovery) — Entscheidung in `decisions.md` begründet.
- **Filterselektion empirisch verifiziert:** `--filter "Category!=OsInterface"` → **641/641 grün** (~26 s, Baseline-paritätisch), `--filter "Category=OsInterface"` → **94/94 grün** (~2 min, lokales Gate erfüllt); Compound-Filter `(Category=Flaky)&(FullyQualifiedName~…)` und `--list-tests` unter MTP geprüft; leere Treffermenge = Exitcode 8 (für `TestDiscovery.ps1` belanglos, dokumentiert).
- **Coverage/TRX-Artefakte:** `--coverage --coverage-output-format cobertura` erzeugt valide Cobertura mit `line-rate="0.7251"` → **72,5 % ≥ 70-%-Gate**; `--report-trx` schreibt `test-results.trx`/`test-results-os-interface.trx` ins explizite `--results-directory` — die Retry-/Analyzer-Pipeline (`-PrimaryTrx`, `*-flaky-retry.trx`-Konvention) bleibt funktionsfähig.
- **Trait-Migration korrekt:** `OsInterface`/`Flaky`/`OsInterfaceFact`/`OsInterfaceTheory` implementieren `Xunit.v3.ITraitAttribute` (`GetTraits()` liefert `("Category", wert)`); `TestTraitReader` deckt weiterhin rohe `[Trait("Category", …)]`-Schreibweise via `CustomAttributeData` inkl. Vererbungskette ab. Konventions-Meta-Tests (`FlakyConventionTests`, `TraitParityTests`) laufen innerhalb der 641 grün.
- **API-Migration vollständig:** alle `IAsyncLifetime`-Implementierungen auf `ValueTask` (inkl. `PlaywrightImpressum*Server`-Overrides), `ITestOutputHelper`-Usings auf `Xunit`-Namespace; Build warnungsfrei, `xunit.analyzers` ohne Befund.
- **CI-Konsistenz:** Alle `dotnet test`-Aufrufe in Workflows und Skripten nutzen die `--`-Trennung; `--results-directory` überall gesetzt, wo TRX-Pfade konsumiert werden. Filterausdrücke byte-identisch zur Vorgängerfassung — kein Selektionsdrift.
- **Screenshots:** Die regenerierten `site/screenshots/*.png` (Nebeneffekt des OsInterface-Laufs, vgl. Randnotiz Schritt 7) wurden zurückgesetzt — kein ungewollter Diff im Commit.
- **Vulnerability-Scan:** `dotnet list package --vulnerable` (Solution) → keine anfälligen Pakete in allen vier Projekten.
- **Doku synchron:** `RELEASE_NOTES.md` EN+DE faktisch korrekt (Versionen, MTP-`--`-Form, Extension-Pakete, Trait-Umbau); `os-interface-tests.md` dokumentiert ITraitAttribute-Muster, MTP-Exitcodes und neue Kommandoform; README/CLAUDE.md zeigen migrierte Beispielbefehle.
- **Lifecycle-Konvention:** Schritt-Artefakte im `docs:`-Commit `9f634d2` dokumentiert und wie vorgesehen im feat-Commit entfernt — Nachweise in der Git-Historie. `steps.md` zeigt Schritt 8 noch als „In Arbeit" (Abschluss-Kennzeichnung erfolgt auf dem Projekt-Branch nach dem Merge).
