# Abnahmeprüfung – Entwicklungsschritt 2

## Ergebnis
**Status:** Anforderung vollständig erfüllt

## Abweichungen
- [ ] Keine.

## Hinweise

### Verifizierte Schrittartefakte
- `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj:23` enthält `<FrameworkReference Include="Microsoft.AspNetCore.App" />`; die PackageReferences `Microsoft.AspNetCore.Http.Abstractions 2.3.0`, `Microsoft.AspNetCore.SignalR.Core 1.2.10` und `Microsoft.AspNetCore.SignalR.Client` sind entfernt (Diff gegen Basisbranch geprüft).
- `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj:21` enthält `Microsoft.AspNetCore.SignalR.Client 11.0.*` (aufgelöst: 11.0.0, plus transitiv `SignalR.Client.Core`/`Common`/`Protocols.Json` 11.0.0).
- `decisions.md` dokumentiert Entscheidung, Begründung, verworfene Minimalvariante und Konsequenzen nachvollziehbar; die Nutzungsbehauptungen wurden verifiziert (`Hub`/`IHubContext` nur in `SignalRNotificationService.cs:1`, `IHttpContextAccessor` nur in `WindowsCurrentUserService.cs:2`, `HubConnection`/`HubConnectionBuilder` nur in `AppShell.razor` und `ApplicationGroupTree.razor`).
- `dotnet build`: 0 Fehler; `dotnet restore --force --no-cache`: keine NU-Warnungen (insb. kein NU1510).
- `dotnet test --filter "Category!=OsInterface"`: **641/641 bestanden** (Baseline exakt erreicht, inkl. `TopBarTests`).
- `dotnet list package --vulnerable` (Lösung, alle 4 Projekte): **kein Befund**.
- Testprojekt baut und läuft gegen transitive `FrameworkReference` (SignalR-Server-Typen in `EndpointHubTests_Subscriptions.cs`, `PlaywrightSignalRServer.cs` kompilieren; `SignalR.Client` kommt transitiv über die Web-`ProjectReference`).

### Bewertung der NU1510-Folgebereinigung (6 zusätzlich entfernte PackageReferences)
**Technisch korrekt und sinnvoll, keine schädliche Scope-Überschreitung.** Die betroffenen Typen werden in Infrastructure real genutzt (`IHttpClientFactory` in `SwaggerImportService`/`ODataImportService`/`EndpointExecutionService`/`HealthCheckService`; `IJSRuntime` in `ThemeService`/`StorageModeService`; `IConfiguration`/`IServiceCollection` in `HealthCheckService`/`DatabaseProviderFactory`) und kompilieren weiterhin — sie kommen nachweislich aus dem Shared Framework. In der transitiven Paketliste von Infrastructure tauchen alle sechs Pakete nicht mehr auf (NuGet-Pruning auf das Framework-Asset). Hätten die Referenzen bestanden, wären dauerhafte NU1510-Warnungen die Folge gewesen. Entfernung ist in `decisions.md` (Konsequenzen) und im csproj-Kommentar transparent begründet. Die Alternative — verbleibende, als redundant markierte Referenzen — wäre schlechter gewesen.

### Pkcs-Pin-Verlust (GHSA-846f-f764-76mx high / GHSA-2fmv-rv2c-59x7 moderate)
**Kein Sicherheitsverlust.** Beide Advisories betreffen auf der .NET-11-Linie nur `11.0.0-preview.1` bis `11.0.0-rc.1`; gepatcht ab `11.0.0-rc.2`. Die Ziel-Runtime ist .NET 11 GA (installiert: `Microsoft.AspNetCore.App 11.0.0`, enthaltenes `System.Security.Cryptography.Pkcs.dll` FileVersion 11.0.26.53018) — bereits auf der gepatchten Linie; verwundbar wären nur Preview-Runtimes, die produktiv nicht relevant sind. EF.SqlServer 11.0.0 deklariert weiterhin `Pkcs >= 10.0.13` als Untergrenze; dort, wo das Paket nicht geprunt wird (Testprojekt, kein direkter `FrameworkReference`), löst es exakt als `10.0.13` = gepatchte Version auf. Die Begründung in `decisions.md` ist plausibel und ehrlich — sie behauptet keine konkreten Versionsnummern, sondern beschreibt korrekt den Patch-Pfad über die Runtime-Installation.

### Einschränkungen / Nebenbefunde (keine Abweichungen)
- `dotnet list package --vulnerable` bewertet nur NuGet-Pakete, keine Framework-Assets — die Pkcs-Sicherheit folgt der installierten Runtime. Auf Maschinen mit Runtime < 11.0.0-rc.2 (nur Previews) wäre sie ungepatcht; mit dem SDK-Pin `11.0.x` realistisch nicht erreichbar.
- Konsistenz-Hinweis: Das Testprojekt behält `Microsoft.JSInterop` als direkte PackageReference (11.0.0) und löst `Pkcs` als Paket `10.0.13` auf (kein Pruning ohne direkte `FrameworkReference`). Löst keine NU1510-Warnung aus und ist sicherheitlich unbedenklich, aber asymmetrisch zur Infrastructure-Bereinigung — ggf. in einem Folgeschritt harmonisieren.
- Netto-Diff zum Basisbranch: nur 2 csproj-Dateien, `decisions.md` (neu) und `RELEASE_NOTES.md` (+2 Zeilen). Die im Schritt-Commit gelöschten Dateien waren branch-interne Arbeitsdokumente unter `docs/features/task/…-schritt-2-frameworkref/` (im Basisdiff ohnehin nicht vorhanden).
