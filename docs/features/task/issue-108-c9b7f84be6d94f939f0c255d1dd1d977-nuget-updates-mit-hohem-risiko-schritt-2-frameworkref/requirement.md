# Anforderung – Schritt 2: F6 – SignalR.Client ins Web-Projekt + FrameworkReference-Entscheidung

**Quelle:** Projekt `issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko` (GitHub-Issue #108, „NuGet-Updates mit hohem Risiko"), Entwicklungsschritt 2 von 9. Projektleitung-Auftrag: Kundenanforderung wörtlich übernommen, gewählte Variante dokumentierte Annahme.

**Kontext:** Schritt 1 ist gemergt — die Lösung läuft auf `net11.0`, SDK 11.0.100, Floating-Baseline `11.0.*` (`Microsoft.AspNetCore.SignalR.Client` steht derzeit als `11.0.*` in `Schnittstellenzentrale.Infrastructure.csproj`).

## Ziel

Die Paket-/Referenzstruktur von `Schnittstellenzentrale.Infrastructure` wird bereinigt:

1. `<FrameworkReference Include="Microsoft.AspNetCore.App" />` wird in `Schnittstellenzentrale.Infrastructure.csproj` aufgenommen (**gewählte Variante: volle FrameworkReference-Variante**, dokumentierte Annahme der Projektleitung).
2. Die beiden Legacy-PackageReferences `Microsoft.AspNetCore.Http.Abstractions 2.3.0` und `Microsoft.AspNetCore.SignalR.Core 1.2.10` werden aus `Schnittstellenzentrale.Infrastructure.csproj` entfernt — `Hub`/`IHubContext<THub>` (in `SignalRNotificationService<THub>`) und `IHttpContextAccessor` (in `WindowsCurrentUserService`) liegen vollständig im Shared Framework.
3. `Microsoft.AspNetCore.SignalR.Client` (`11.0.*`) wird aus `Schnittstellenzentrale.Infrastructure.csproj` **nach** `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj` (Web-Projekt) **verschoben** — das Paket ist nicht Teil des Shared Frameworks und wird nur im Web-Projekt genutzt (`Components/Layout/AppShell.razor`, `Components/Shared/ApplicationGroupTree.razor`).
4. Die Architektur-Entscheidung wird mit Begründung in `docs/projects/task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko/decisions.md` (neu anzulegen) dokumentiert.
5. Die Verträglichkeit des bunit-Testprojekts mit der transitiven `FrameworkReference` wird per Build-/Test-Check verifiziert; csproj-Konventionen (`GenerateDocumentationFile`, `WarningsAsErrors` etc.) bleiben unverändert.

## Begründung der gewählten Variante (aus dem Auftrag)

- `Hub`/`IHubContext<THub>` in `SignalRNotificationService<THub>` und `IHttpContextAccessor` in `WindowsCurrentUserService` liegen vollständig im Shared Framework `Microsoft.AspNetCore.App`.
- Zwei Legacy-Paketlinien (`Http.Abstractions 2.3.0`, `SignalR.Core 1.2.10`) entfallen.
- Der faktische Major-Sprung `SignalR.Core` 1.2.10 → Shared-Framework-Version wird sauber vollzogen statt als Paketlinie weiterverwaltet.

## Akzeptanzkriterien

- `dotnet build` fehlerfrei.
- `dotnet test --filter "Category!=OsInterface"` vollständig grün (inkl. `TopBarTests`).
- `SignalR.Client`-Referenz liegt im Web-csproj, nicht mehr in Infrastructure.
- Keine `Http.Abstractions`-/`SignalR.Core`-PackageReference mehr in Infrastructure.
- Testprojekt baut und läuft gegen die transitive `FrameworkReference`.
- Entscheidung mit Begründung in `decisions.md` dokumentiert.

## Rahmenbedingungen / Scope

- OsInterface-/Playwright-Lane **nicht** erforderlich — nur reguläre Lane (`Category!=OsInterface`).
- Reale E2E-Abnahme: App real unter Kestrel starten, `GET /` → HTTP 200 genügt als Nachweis (SignalR-Client-Nutzung ist Browser-seitig).
- Kein Issue-Kommentar, kein Merge, kein Push, kein Pull Request. Endzustand: Lifecycle vollständig inkl. Abschlusscommit `feat: ...` auf dem Schritt-Branch.
- Keine Scope-Erweiterung: keine weiteren Paketverschiebungen, keine Versionsänderungen an anderen Referenzen.

## Betroffene Bereiche

- `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj` (FrameworkReference rein, 3 PackageReferences raus)
- `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj` (SignalR.Client rein)
- `docs/projects/task/issue-108-c9b7f84be6d94f939f0c255d1dd1d977-nuget-updates-mit-hohem-risiko/decisions.md` (neu)
- Indirekt: `src/Schnittstellenzentrale.Tests` (transitive FrameworkReference, bunit-Verträglichkeit)

## Nicht-Ziele

- Keine Änderung an `Microsoft.AspNetCore.SignalR.Client`-Versionslinie (bleibt `11.0.*` floating).
- Keine Migration auf zentrales Paketmanagement, keine Lock-Dateien (F7, Schritt 9).
- Keine Änderung an OData/Serilog/OpenApi/Playwright/xunit (eigene Schritte).
