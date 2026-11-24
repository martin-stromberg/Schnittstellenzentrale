# Bestandsaufnahme – Schritt 2: F6 – SignalR.Client + FrameworkReference

Stand: Branch `task/issue-108-…-schritt-2-frameworkref`, Baseline = gemergter Schritt 1 (net11.0, SDK 11.0.100, Floating `11.0.*`). Es existieren **keine** `Directory.Build.props`/`Directory.Packages.props`/`NuGet.config`/`packages.lock.json` — Versionen stehen direkt in den csproj-Dateien.

## Projektdateien (Ist-Zustand)

### `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj` (SDK `Microsoft.NET.Sdk`, net11.0)

Relevante Zeilen:

| Zeile | Eintrag | Schicksal |
|-------|---------|-----------|
| 10 | `PackageReference Microsoft.AspNetCore.Http.Abstractions 2.3.0` | entfernen → durch Shared Framework abgedeckt |
| 11 | `PackageReference Microsoft.AspNetCore.SignalR.Client 11.0.*` | ins Web-csproj verschieben |
| 12 | `PackageReference Microsoft.AspNetCore.SignalR.Core 1.2.10` | entfernen → durch Shared Framework abgedeckt |
| – | `FrameworkReference Microsoft.AspNetCore.App` | **neu aufnehmen** |

PropertyGroup (unverändert bleiben): `TargetFramework net11.0`, `ImplicitUsings`, `Nullable`, `GenerateDocumentationFile true`, `WarningsAsErrors …;CS1591`.

Weitere PackageReferences (Jint, Markdig, EF Core 11.0.*, Microsoft.Extensions.* 11.0.*, Microsoft.JSInterop, Microsoft.OData.Edm, Microsoft.OpenApi, SQLitePCLRaw, System.Security.Cryptography.Pkcs) bleiben unberührt.

### `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj` (SDK `Microsoft.NET.Sdk.Web`, net11.0)

- `Microsoft.AspNetCore.SignalR.Client 11.0.*` **neu aufnehmen** (alphabetisch zwischen `Microsoft.AspNetCore.Authentication.Negotiate` und `Microsoft.AspNetCore.OData` einordnen).
- Web-SDK zieht `Microsoft.AspNetCore.App` implizit — keine FrameworkReference nötig.

### `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj` (SDK `Microsoft.NET.Sdk`, net11.0)

- Keine direkte Änderung nötig. Referenziert alle drei Produktivprojekte per `ProjectReference` → `SignalR.Client` fließt transitiv über das Web-Projekt, `Microsoft.AspNetCore.App` transitiv über Web- (implizit) und Infrastructure-Projekt (neu).
- bunit 2.7.2 rendert `AppShell` und `ApplicationGroupTree` (verwenden `HubConnection`) — Verträglichkeit mit der transitiven FrameworkReference per Build/Test zu verifizieren.
- Achtung: `InstallPlaywright`-Target läuft `AfterTargets="Build"` — für schnelle Builds ggf. `-p:SkipPlaywrightInstall=true`.

### `src/Schnittstellenzentrale.Core/Schnittstellenzentrale.Core.csproj`

- Keine ASP.NET-Core-Abhängigkeit (`Microsoft.AspNetCore.*`-usings: 0). Unberührt.

## Code-Nutzung (verifiziert per Volltextsuche)

| Typ/Namespace | Datei | Herkunft nach Umbau |
|---------------|-------|---------------------|
| `Microsoft.AspNetCore.SignalR` (`Hub`, `IHubContext<THub>`, `Clients.Group().SendAsync`) | `src/Schnittstellenzentrale.Infrastructure/Services/SignalRNotificationService.cs` | Shared Framework `Microsoft.AspNetCore.App` (`Microsoft.AspNetCore.SignalR.Core.dll`) |
| `Microsoft.AspNetCore.Http` (`IHttpContextAccessor`) | `src/Schnittstellenzentrale.Infrastructure/Services/WindowsCurrentUserService.cs` | Shared Framework (`Microsoft.AspNetCore.Http.Abstractions.dll` ist Teil des Shared Frameworks) |
| `Microsoft.AspNetCore.SignalR.Client` (`HubConnection`, `HubConnectionBuilder`, `HubConnectionState`) | `src/Schnittstellenzentrale/Components/Layout/AppShell.razor`, `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor` | NuGet-Paket `Microsoft.AspNetCore.SignalR.Client` im **Web**-csproj (nicht im Shared Framework enthalten) |

**Keine weiteren** `Microsoft.AspNetCore.*`-usings in Infrastructure (nur die beiden Dateien; `ODataImportService` nutzt `Microsoft.OData.Edm`, nicht AspNetCore). Keine `Microsoft.AspNetCore.*`-usings in Core.

Im Testprojekt wird `Microsoft.AspNetCore.SignalR.Client` **nicht** direkt referenziert (`TopBarTests` mockt `ISignalRNotificationService`; „HubConnection" taucht nur im Testnamen/Kommentar auf). Die Client-Assembly fließt dennoch transitiv über die ProjectReference auf das Web-Projekt ein (bunit-Rendering von `AppShell`/`ApplicationGroupTree` benötigt sie zur Laufzeit).

`EndpointHub` (`src/Schnittstellenzentrale/Hubs/EndpointHub.cs`) liegt im Web-Projekt — vom Umbau unberührt.

## Test-/Abnahme-Infrastruktur

- Reguläre Lane: `dotnet test --filter "Category!=OsInterface"` (blockierend, inkl. `TopBarTests`, `AppShellTests`).
- Playwright/OsInterface-Lane: nicht erforderlich für diesen Schritt (vom Auftrag ausgenommen).
- Reale E2E-Abnahme: `dotnet run`/`dotnet.exe <app.dll>` unter Kestrel, `GET /` → HTTP 200 (SignalR-Client läuft browserseitig — HTTP-200-Start genügt laut Auftrag).
- `Program.cs` nutzt `AddHttpContextAccessor`/SignalR-Registrierung (Web-Seite, unverändert).

## Risiken / zu verifizierende Punkte

1. **bunit-Verträglichkeit:** Testprojekt (`Microsoft.NET.Sdk`, nicht Web) muss gegen die transitive `FrameworkReference Microsoft.AspNetCore.App` bauen und laufen — FrameworkReference ist transitiv, das Web-Projekt liefert sie ohnehin schon implizit mit; Risiko gering, per Build+Test zu belegen.
2. **Major-Sprung SignalR.Core 1.2.10 → 11.0.x (Shared Framework):** API-Brüche bei `Hub`/`IHubContext<THub>`/`SendAsync` theoretisch möglich — die verwendete Oberfläche (`Clients.Group(string).SendAsync(string, params object[])`) ist seit Core-3.x stabil. Build legt Brüche sofort offen.
3. **Transitive Runtime für Tests:** Nach dem Verschieben muss `SignalR.Client` im Test-Output weiter vorhanden sein (kommt über Web-ProjectReference transitiv).

## Detaildokumente

- [inventory/csproj-diff-plan.md](inventory/csproj-diff-plan.md) — konkrete Zeilen-für-Zeilen-Umbaupunkte der beiden csproj-Dateien.
