# Umsetzungsplan – xunit.v3-Migration (Schritt 8)

## Scope-Klärung

- **Verarbeitungseinheit:** `src/Schnittstellenzentrale.Tests` (einzige Test-Assembly)
  plus konsumierende Kommandos (CI-Workflows, `scripts/`, `CLAUDE.md`,
  `docs/help/stabilitaet/os-interface-tests.md`).
- **Auslösende Ereignisse:** Build/Test über `dotnet test`, CI-Lanes, lokale Gates.
- **Erwartete Wirkung:** Testprojekt baut und läuft vollständig auf xunit.v3 4.0.3;
  OsInterface-Teilmenge bleibt selektierbar; Coverage-Gate unverändert.
- **Scope-Ausschlüsse:** keine. (Nur Minimalanpassung von Dritt-Abhängigkeiten,
  falls zwingend — wird im Ergebnis begründet.)

## Architekturentscheidung (Annahme, dokumentiert)

`dotnet test` bleibt im **VSTest-Modus**: `Microsoft.NET.Test.Sdk` 18.5.1 +
`xunit.runner.visualstudio` 4.0.1 bleiben referenziert; das Projekt wird
zusätzlich `Exe` (v3-Anforderung) und ist damit auch direkt als MTP-App
ausführbar (`--filter-query`, `-trait` etc.). Begründung:

- `--filter "Category=…"`, `--collect:"XPlat Code Coverage"` (coverlet.collector)
  und `--logger trx` bleiben byte-identisch → CI, Retry-Skript und Coverage-Gate
  ohne Umbau funktionsfähig.
- coverlet.collector ist ein reiner VSTest-Datacollector; Voll-MTP würde eine
  neue Coverage-Extension erfordern (unnötige Scope-Ausweitung).
- **Empirischer Checkpoint:** Falls `dotnet test` unter SDK 11 bei einem
  xunit.v3-Projekt dennoch automatisch MTP wählt und `--filter` anders
  interpretiert, wird auf die funktionierende Selektionssyntax gewechselt und
  CI/Skripte/Doku konsistent angepasst (funktionale Anforderung zählt).

## Arbeitspakete

### P1 – csproj

- `<OutputType>Exe</OutputType>` ergänzen.
- `xunit` 2.9.3 → `xunit.v3` 4.0.3.
- `xunit.runner.visualstudio` 4.0.1, `Microsoft.NET.Test.Sdk`, `coverlet.collector`
  unverändert. `<Using Include="Xunit" />` bleibt (v3 nutzt denselben Namespace).
- `dotnet restore` + `dotnet build` — erste Compile-Fehlerliste.

### P2 – Trait-Infrastruktur Neubau (`Infrastructure/Testing/`)

- `CategoryTraitDiscoverer.cs` **löschen** (ITraitDiscoverer existiert nicht mehr).
- `OsInterfaceAttribute`, `FlakyAttribute`: `Attribute, Xunit.v3.ITraitAttribute`,
  `GetTraits()` liefert `[new("Category", TestCategories.X)]`; `[TraitDiscoverer]`
  und `using Xunit.Sdk` entfernen.
- `OsInterfaceFactAttribute`: `FactAttribute, Xunit.v3.ITraitAttribute`
  (Fact bleibt in v3 ableitbar). Verifizieren, dass Traits von der Fact-Attribut-
  Instanz selbst aufgesammelt werden — sonst auf `[OsInterface]`+`[Fact]` umstellen.
- `OsInterfaceTheoryAttribute`: analog (`TheoryAttribute` erbt weiter `Fact`).
- `TestTraitReader`: `[Trait]` weiter über `CustomAttributeData`-Ctor-Args;
  Kategorie-Attribute über `ITraitAttribute.GetTraits()` statt `Category`-Property
  (Property kann als Convenience bleiben; Reader ruft bevorzugt `GetTraits()` auf).
- `TraitParityTests`/`FlakyConventionTests`: Kommentare auf v3 anpassen,
  Logik bleibt.

### P3 – IAsyncLifetime (4 Dateien)

`Task InitializeAsync()`/`Task DisposeAsync()` → `ValueTask …`; `await`-
Verhalten unverändert (ValueTask ist awaitable). In `PlaywrightServer`/
`PlaywrightTestBase` `async ValueTask`.

### P4 – ITestOutputHelper (22 Dateien)

`using Xunit.Abstractions;` entfernen — `ITestOutputHelper` liegt in v3 unter
`Xunit` (durch global using bereits sichtbar).

### P5 – Compile-Restfehler

Assert-Überladungen/entfernte APIs iterativ beheben (erwartet: wenige).

### P6 – Filter-Verifikation (empirisch)

1. `dotnet test --no-build --list-tests --filter "Category=OsInterface"` → muss 94 liefern.
2. `… --filter "Category!=OsInterface"` → 641.
3. `… --filter "Category=Flaky"` → Flaky-Teilmenge.
4. Bei Abweichung: MTP-Syntax ermitteln (`--filter-query "/[category=osinterface]"`
   o. ä.), Kommandos konsistent anpassen.

### P7 – Volle Läufe + Coverage

- `dotnet test -c Release --filter "Category!=OsInterface" --collect:"XPlat Code Coverage"`
  → grün + `coverage.cobertura.xml` wird erzeugt (Gate-Funktion nachgewiesen).
- `dotnet test -c Release --filter "Category=OsInterface"` → 94, grün (lokales Gate).
- `dotnet list package --vulnerable` sauber.

### P8 – Doku

- `CLAUDE.md`: Testabschnitt auf migrierte Kommandos (ggf. neue Syntax, sonst
  Hinweis „xunit.v3").
- `docs/help/stabilitaet/os-interface-tests.md`: Kommandos, ggf. MTP-Hinweis
  (v3-Exe direkt ausführbar), Trait-Attribut-Beschreibung auf `GetTraits()`
  aktualisieren.
- `scripts/Retry-FlakyTests.ps1`/`TestDiscovery.ps1`: nur anpassen, wenn sich
  die funktionierende Syntax ändert (P6).
- CI-Workflows: nur anpassen, wenn P6 eine Syntaxänderung ergibt.
- `docs/RELEASE_NOTES.md`: Eintrag.

## Akzeptanzszenarien

1. `dotnet test --filter "Category!=OsInterface"` (Release, mit Coverage) grün,
   ~641 Tests, Coverage-Datei vorhanden.
2. `dotnet test --filter "Category=OsInterface"` selektiert exakt die
   Trait-Teilmenge (~94) und läuft lokal grün.
3. `dotnet list package --vulnerable` ohne Funde.

## Offene Punkte

- Keine (Filter-Syntax-Frage ist als empirischer Checkpoint P6 eingeplant;
  Entscheidung liegt im Ermessen des Umsetzers, wird im Ergebnis dokumentiert).
