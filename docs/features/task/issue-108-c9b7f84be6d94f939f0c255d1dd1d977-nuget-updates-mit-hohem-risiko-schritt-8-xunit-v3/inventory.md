# Bestandsaufnahme – xunit.v3-Migration

## Testprojekt

`src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`:
- `TargetFramework net11.0`, kein `OutputType` (also Library — v3 braucht `Exe`)
- `ImplicitUsings` + `<Using Include="Xunit" />` (bleibt korrekt: `Fact`,
  `Theory`, `ITestOutputHelper`, `TraitAttribute` liegen in v3 unter `Xunit`)
- Pakete: `xunit` 2.9.3 (→ `xunit.v3` 4.0.3), `xunit.runner.visualstudio` 4.0.1
  (neueste stabil, unterstützt v3 — bleibt), `Microsoft.NET.Test.Sdk` 18.5.1,
  `coverlet.collector` 10.0.0, `bunit` 2.7.2, `Moq` 4.20.72,
  `Microsoft.AspNetCore.Mvc.Testing` 11.0.*, `Microsoft.Playwright` 1.65.1
- `InstallPlaywright`-Target (`AfterTargets=Build`, `playwright.ps1 install chromium`)
- bunit 2.7.2 hat **keine** xunit-Abhängigkeit (project.assets.json verifiziert) —
  bricht nicht unter v3. Moq ebenfalls xunit-frei.

## xunit-Nutzung im Testprojekt

- `[Fact]`: 704 Vorkommen, `[Theory]`: 8, `[Collection]`: 23 — mechanisch
  kompatibel (gleiche Attributnamen/Namespace `Xunit` in v3).
- `using Xunit.Abstractions;` in 22 Dateien (`ITestOutputHelper`) → in v3 liegt
  `ITestOutputHelper` in `Xunit` (xunit.v3.extensibility.core) → usings löschen.
- `using Xunit.Sdk;` nur in `Infrastructure/Testing/` (ITraitDiscoverer,
  ITraitAttribute, TraitDiscoverer-Attribut) → kompletter Neubau.
- `IAsyncLifetime` in genau 4 Dateien (siehe requirement) — v3-Signatur:
  `ValueTask InitializeAsync()` / `ValueTask DisposeAsync()` (erbt IAsyncDisposable).
- Kein `async void`, keine rohen `[Trait(...)]` außerhalb der Meta-Tests/Doku.
- `IClassFixture`/`ICollectionFixture`/`CollectionDefinition`: 17 Dateien —
  Namen identisch in v3.
- `MemberData`/`InlineData`/`ClassData`: 4 Dateien — identisch.

## Trait-Infrastruktur `Infrastructure/Testing/` (v2, zu ersetzen)

| Datei | v2-Rolle | v3-Ersatz |
|-------|----------|-----------|
| `CategoryTraitDiscoverer.cs` | `ITraitDiscoverer`, liest `Category`-Named-Argument | **entfällt** — Attribute implementieren `Xunit.v3.ITraitAttribute` selbst (`GetTraits()`) |
| `OsInterfaceAttribute.cs` | `Attribute, ITraitAttribute` + `[TraitDiscoverer]` | `Attribute, Xunit.v3.ITraitAttribute` mit `GetTraits() => [("Category","OsInterface")]` |
| `FlakyAttribute.cs` | dto. für `Flaky` | dto. |
| `OsInterfaceFactAttribute.cs` | `FactAttribute, ITraitAttribute` + `[TraitDiscoverer]` | `FactAttribute, Xunit.v3.ITraitAttribute` |
| `OsInterfaceTheoryAttribute.cs` | `TheoryAttribute, ITraitAttribute` | `TheoryAttribute, Xunit.v3.ITraitAttribute` |
| `TestTraitReader.cs` | Reflection-Reader paritätisch zu v2-Discoverern | anpassen: `[Trait]` weiter per `CustomAttributeData`-Ctor-Args; Kategorie-Attribute per `ITraitAttribute.GetTraits()` |
| `TraitParityTests.cs` | Meta-Test: Playwright-Klassen ⇒ OsInterface | bleibt, Kommentar anpassen (`Theory` erbt weiter `Fact`) |
| `FlakyConventionTests.cs` | Meta-Test: Flaky ⇒ OsInterface | bleibt |
| `TestCategories.cs` | Konstanten | unverändert |

## Filter-/Coverage-Verbraucher (müssen funktional erhalten bleiben)

- `.github/workflows/staging-ci.yml` + `pr-staging-ci.yml`:
  `dotnet test --filter "Category!=OsInterface" --collect:"XPlat Code Coverage"
  --logger trx` (blockierend, speist 70-%-Gate) und
  `dotnet test --filter "Category=OsInterface" --logger trx` (best-effort-Lane).
- `.github/workflows/release.yml`: `Category!=OsInterface` mit `-p:SkipPlaywrightInstall=true`.
- `scripts/Retry-FlakyTests.ps1` + `scripts/lib/TestDiscovery.ps1`:
  `dotnet test --no-build --list-tests --filter "Category=Flaky"` und Re-Run
  `--filter "(Category=Flaky)&(FullyQualifiedName~...)"` mit TRX-Logger.
- `CLAUDE.md`: dokumentiert die Filterkommandos.
- `docs/help/stabilitaet/os-interface-tests.md`: Doku der Lane-Trennung.

## v3-Fakten (recherchiert)

- Neueste stabil: **xunit.v3 4.0.3** (11/2026); `xunit.runner.visualstudio` 4.0.1
  ist aktuellster Adapter und läuft mit v3-Assemblies.
- v3-Testprojekte sind Exe (MTP-fähig); `dotnet test` läuft weiter über
  Microsoft.NET.Test.Sdk + Adapter. **Empirisch zu verifizieren:** ob
  `--filter "Category=OsInterface"` unter SDK 11 unverändert die Trait-Menge
  selektiert, oder ob MTP-Modus/Neue-Syntax greift (`--filter-query`,
  `[name!=value]`-Trait-Syntax der v3-Query-Filter-Language).
- `ITraitDiscoverer`, `IAttributeInfo`, `xunit.abstractions` entfallen;
  `ITraitAttribute` (`Xunit.v3`) liefert `IReadOnlyCollection<KeyValuePair>` via
  `GetTraits()` — ein Attribut kann mehrere Traits liefern, auf Methoden, Klassen,
  Assembly anwendbar.
- `IAsyncLifetime` erbt `IAsyncDisposable`, beide Methoden `ValueTask`.
- `ITestOutputHelper` → Namespace `Xunit`.
- `TraitAttribute` bleibt (v3, auch auf Assembly erlaubt);
  `TheoryAttribute : FactAttribute` bleibt.

## Risiken

- Filter-Semantik `Category=…` unter `dotnet test` (VSTest-Bridge vs. MTP) —
  wird empirisch geklärt; funktionale Anforderung = selektierbare Teilmenge.
- coverlet.collector ist VSTest-Datacollector — greift nur, wenn `dotnet test`
  im VSTest-Modus läuft. Falls MTP erzwungen wird: Ausweichoption
  `Microsoft.Testing.Extensions.CodeCoverage`/coverlet-MTP-Extension prüfen.
- Assert-API ist Obermenge von 2.9, aber neue Überladungen können
  Ambiguitäten/Verhaltensänderungen bringen — Compile + voller Testlauf entscheidet.
