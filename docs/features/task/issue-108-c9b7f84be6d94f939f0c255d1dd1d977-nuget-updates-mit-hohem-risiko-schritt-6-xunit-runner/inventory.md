# Bestandsaufnahme

## Ausgangslage

- Branch: `task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko-schritt-6-xunit-runner`
- Basisbranch: `task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko`
- Working tree: sauber, SDK `11.0.100`, Testprojekt targetet `net11.0`.
- Schritte 1–5 des Issues #108 sind bereits gemergt.

## Relevante Dateien

| Datei | Rolle |
|-------|-------|
| `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj` | Einzige zu ändernde Datei; enthält `xunit.runner.visualstudio` 3.1.5 (Zeile 33), `xunit` 2.9.3 (Zeile 32), `coverlet.collector` 10.0.0 (Zeile 19), `Microsoft.NET.Test.Sdk` 18.5.1 |
| `src/Schnittstellenzentrale.Tests/Infrastructure/Testing/` | Trait-Infrastruktur auf xunit-v2-Xunit.Sdk-Basis — Details: [inventory/trait-infrastruktur.md](inventory/trait-infrastruktur.md) |
| `.github/workflows/pr-staging-ci.yml` | Reguläre Lane `--filter "Category!=OsInterface"` + `--collect:"XPlat Code Coverage"` + 70-%-Threshold; best-effort-Lane `--filter "Category=OsInterface"` |
| `.github/workflows/staging-ci.yml` | Gleiche Lane-Struktur wie pr-staging-ci |
| `.github/workflows/release.yml` | `dotnet test --filter "Category!=OsInterface"` |
| `docs/help/stabilitaet/os-interface-tests.md` | Doku zur OS-Interface-Lane |

## Fakten zur Trait-Infrastruktur

- `CategoryTraitDiscoverer : ITraitDiscoverer` (Xunit.Sdk) emittiert
  `("Category", wert)`-Trait-Paare; referenziert per `[TraitDiscoverer(...)]`.
- Marker-Attribute `OsInterfaceAttribute`, `OsInterfaceFactAttribute`,
  `OsInterfaceTheoryAttribute`, `FlakyAttribute` implementieren `ITraitAttribute`.
- 24 Dateien tragen `[OsInterface]` auf Klassenebene; `ImpressumPageTests.cs`
  enthält zwei markierte Klassen → **25 OsInterface-markierte Testklassen**
  (22 Playwright-E2E-Klassen + `TempDirectoryCleanupTests` +
  `ImpressumServiceTests` + weitere). Keine methodenbezogenen
  `OsInterfaceFact`/`OsInterfaceTheory`-Verwendungen außerhalb der
  Infrastruktur.
- `TraitParityTests` (reguläre Lane) erzwingt, dass jede Klasse, die OS-Ressourcen
  anrührt, ein OsInterface-Trait trägt — schützt die Selektion strukturell.

## Baseline

- Vorgegebene Baseline der regulären Lane: **641 Tests** (aus der Anforderung).
- OsInterface-Testmenge: aus `--list-tests` zu ermitteln und gegen die 25
  markierten Klassen zu plausibilisieren.

## Risiken / Abhängigkeiten

- `xunit.runner.visualstudio` 4.x ist der Runner für die xunit.v3-Linie, muss aber
  weiterhin v2-Testassemblies (xunit 2.9.3) ausführen und deren
  `ITraitDiscoverer`-Extensibilität respektieren — genau das prüft dieser Schritt.
- Kein packages.lock.json vorhanden (geprüft: kein Lockfile im Repo) → Restore
  zieht die neue Version direkt.
- `Microsoft.NET.Test.Sdk` 18.5.1 ist aktuell genug für Runner 4.x (VSTest-
  Protokoll unverändert).
