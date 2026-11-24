# Anforderung (übersetzt)

## Titel

F2 – Migration des Testprojekts von xunit 2.9.3 auf xunit.v3 (Zielversion: neueste stabil, 4.0.3)

## Kontext

Schritt 8 einer mehrstufigen NuGet-Update-Story (Issue #108). Schritte 1–7 sind gemergt:
net11.0, SDK 11.0.100, FrameworkReference, Serilog 11, OpenApi 3.11.1 + Swashbuckle 11.0.1,
xunit.runner.visualstudio 4.0.1, Playwright 1.65.1.

Baselines: 641 reguläre Tests (`Category!=OsInterface`), 94 OsInterface-Tests, 78
ODataControllerIntegrationTests, Coverage-Gate ≥ 70 % (nur reguläre Lane).

## Fachlicher Auftrag

Vollständige Migration von `src/Schnittstellenzentrale.Tests` auf xunit.v3:

1. `OutputType=Exe` im Test-csproj; Paket `xunit` 2.9.3 → `xunit.v3` 4.0.3;
   Microsoft.NET.Test.Sdk-/MTP-Konfiguration anpassen.
2. `IAsyncLifetime`-Signaturen auf `ValueTask` umstellen (4 Dateien:
   `Integration/EndpointExecutionIntegrationTests.cs`,
   `Playwright/Infrastructure/PlaywrightServer.cs`,
   `Playwright/Infrastructure/PlaywrightTestBase.cs`,
   `Playwright/SitePageTests.cs`).
3. `Xunit.Abstractions`/`ITestOutputHelper` (22 Dateien) auf v3-Pendants migrieren;
   `<Using Include="Xunit" />` prüfen.
4. Die v2-Trait-Infrastruktur unter `Infrastructure/Testing/` gegen die
   v3-Extensibilität NEU bauen — Semantik identisch: die OsInterface-Teilmenge muss
   unter `dotnet test` zuverlässig selektierbar bleiben (Konvention in CLAUDE.md,
   CI-Filtern, `scripts/Retry-FlakyTests.ps1`). Funktionale Anforderung ist die
   selektierbare Teilmenge, nicht zwingend dieselbe Befehlssyntax.
5. coverlet.collector/Coverage-Gate (70 %) muss weiter funktionieren.
6. Mechanisch: 704 `[Fact]`, 8 `[Theory]`, 23 `[Collection]`-Nutzungen betroffen.
7. `CLAUDE.md` und `docs/help/stabilitaet/os-interface-tests.md` auf migrierte
   Kommandos aktualisieren.

## Akzeptanzkriterien

- `dotnet test --filter "Category!=OsInterface"` vollständig grün (641 erwartet).
- `dotnet test --filter "Category=OsInterface"` liefert exakt die Trait-markierte
  Teilmenge (94 erwartet) und läuft lokal grün (lokales Gate wie in Schritt 7).
- Coverage-Collection funktioniert (70-%-Gate erhalten).
- `scripts/Retry-FlakyTests.ps1`-Kommandos verifiziert/angepasst.
- `CLAUDE.md` + `docs/help/stabilitaet/os-interface-tests.md` aktualisiert.
- `dotnet list package --vulnerable` sauber.

## Scope-Ausschlüsse

- Kein Issue-Kommentar, kein Merge/Push/PR.
- Keine Scope-Ausweitung auf andere Projekte/Pakete; nur minimal-konsistente
  Dritt-Abhängigkeits-Anpassungen falls zwingend (z. B. alternativer
  Coverage-Collector).
