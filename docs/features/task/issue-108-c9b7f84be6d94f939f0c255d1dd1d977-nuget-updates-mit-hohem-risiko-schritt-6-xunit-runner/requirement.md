# Übersetzte Anforderung

## Original (Kundenanforderung, wörtlich)

xunit.runner.visualstudio → 4.0.1 (risikoarmer Zwischenschritt)

Ziel/Umfang: In `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`
nur `xunit.runner.visualstudio` von 3.1.5 auf 4.0.1 heben; xunit 2.9.3 bleibt
unverändert. Der Schritt prüft vor der eigentlichen v3-Migration risikoarm, ob
`dotnet test` mit der dokumentierten Filter-Syntax `--filter "Category!=OsInterface"`
und `--filter "Category=OsInterface"` weiterhin korrekt selektiert (die
[OsInterface]-/[Flaky]-Trait-Infrastruktur unter `Infrastructure/Testing/` basiert
noch auf der xunit-v2-Xunit.Sdk-Extensibilität) und ob die Coverage-Lane
(`--collect:"XPlat Code Coverage"` mit coverlet.collector, 70-%-Gate in der CI)
unverändert funktioniert.

Akzeptanzkriterien: `dotnet test --filter "Category!=OsInterface"` vollständig grün
(641-Test-Baseline); `dotnet test --filter "Category=OsInterface"` selektiert
weiterhin die Trait-markierten Tests (Ausführung selbst best-effort — die SELEKTION
muss stimmen, d. h. der Filter muss die richtige Testmenge finden, nicht dass alle
Playwright-Tests grün sind); Coverage-Collection läuft ohne Fehler.

## Strukturierte Anforderung

### Kontext

- Übergeordnetes Vorhaben: Issue #108 — NuGet-Updates mit hohem Risiko, schrittweise
  Migration (Schritte 1–5 bereits gemergt: net11.0, SDK 11.0.100, FrameworkReference
  in Infrastructure, OData stabil, Serilog 11, OpenApi 3.11.1 + Swashbuckle 11.0.1).
- Dieser Schritt 6 ist ein **risikoarmer Zwischenschritt** vor der eigentlichen
  xunit-v3-Migration: Er soll früh verifizieren, ob der Test-Runner 4.0.1 mit der
  vorhandenen xunit-v2-Trait-Extensibilität kompatibel bleibt.

### Fachlicher Scope

- **Verarbeitungseinheit:** `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`
- **Änderung:** `PackageReference xunit.runner.visualstudio` Version `3.1.5` → `4.0.1`
- **Explizit NICHT geändert:** `xunit` bleibt `2.9.3` (keine v3-Migration),
  `coverlet.collector` bleibt `10.0.0`, alle übrigen Referenzen unverändert,
  keine Änderungen an der Trait-Infrastruktur (`Infrastructure/Testing/`),
  keine CI-YAML-Änderungen.

### Akzeptanzkriterien (prüfbar)

1. `dotnet test --filter "Category!=OsInterface"` läuft vollständig grün;
   erwartete Testzahl: 641 (Baseline).
2. `dotnet test --filter "Category=OsInterface" --list-tests` listet exakt die
   Trait-markierten OsInterface-Tests (Selektion stimmt; Ausführung nicht nötig).
3. `dotnet test --filter "Category!=OsInterface" --collect:"XPlat Code Coverage"`
   läuft ohne Fehler und erzeugt eine Coverage-Datei.

### Rahmenbedingungen

- Kein Issue-Kommentar, kein Merge/Push/PR.
- Endzustand: Lifecycle vollständig inkl. Abschlusscommit `feat: ...` auf dem
  Schritt-Branch.
- Git-Hooks und CLAUDE.md-Konventionen beachten.
