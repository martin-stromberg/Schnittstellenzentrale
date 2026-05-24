# Bestandsaufnahme: Playwright-Regressionstests für die wichtigsten UI-Abläufe

Analysiert wurde der aktuelle Codestand von `Schnittstellenzentrale.Tests`, `Schnittstellenzentrale`, `Schnittstellenzentrale.Core` und `Schnittstellenzentrale.Infrastructure` in Bezug auf die Anforderung, Playwright-gesteuerte End-to-End-Tests einzuführen.

---

## Zusammenfassung

- `WebApplicationFactory<Program>`-Infrastruktur ist vorhanden: `ControllerTestFactory` zeigt das vollständige Muster (Auth-Bypass, DB-Override, IHostedService-Entfernung, SignalR-Mock).
- `TestAuthHandler` existiert und liefert bereits den geforderten Benutzer `TEST\testuser` — kann direkt in `PlaywrightTestFactory` wiederverwendet werden.
- SQLite-In-Memory-Infrastruktur ist vorhanden; eine Datei-SQLite-Verbindung (`schnittstellenzentrale-tests.db`) ist noch nicht implementiert.
- `IDbContextFactory<AppDbContext>` wird bereits als Factory-Pattern verwendet — Umstellung auf Datei-SQLite ist ein einfacher Connection-String-Tausch.
- `AppDbContext` unterstützt `EnsureCreated()` und `MigrateAsync()` — beide Verfahren sind für `TestDatabaseSeeder` nutzbar.
- `SystemEntryInitializer.InitializeAsync()` ist vorhanden und wird für den Seed-Prozess benötigt; erfordert eine `Api:BaseUrl`-Konfiguration, die auf `Server.BaseAddress` zeigt.
- `EndpointHub` ist vollständig implementiert und unter `/hubs/endpoint` registriert; `SignalRNotificationService<THub>` ist die produktive Implementierung.
- Alle UI-Komponenten, die in den sieben Testabläufen angesprochen werden, existieren: `ApplicationGroupTree`, `ApplicationEditor`, `ApplicationCard`, `SwaggerImportDialog`, `HealthCheckDialog`, `EndpointPage`, `MainLayout` (Modus-Dropdown).
- xUnit mit `IAsyncLifetime` wird bereits in `EndpointExecutionIntegrationTests` verwendet — das Muster für `PlaywrightTestBase` ist bekannt.
- **Fehlend:** `PlaywrightTestFactory`, `PlaywrightTestBase`, `TestDatabaseSeeder` — keine dieser Klassen existiert.
- **Fehlend:** Playwright-Testklassen (`HomePageTests`, `ApplicationCrudTests`, `EndpointExecutionTests`, `SwaggerImportTests`, `HealthCheckTests`, `StorageModeTests`, `SignalRSyncTests`) — kein einziger Playwright-Test ist vorhanden.
- **Fehlend:** NuGet-Paket `Microsoft.Playwright` (und Playwright-xUnit-Adapter) — nicht in `Schnittstellenzentrale.Tests.csproj` referenziert.
- **Fehlend:** `appsettings.Testing.json` — kein separates Testing-Konfigurationsfile vorhanden; `ControllerTestFactory` setzt `UseEnvironment("Testing")`, aber keine passende Datei existiert.
- **Fehlend:** `playwright install`-Automatisierung via MSBuild oder CI-Skript.

---

## Details

- [Vorhandene Test- und Server-Infrastruktur](inventory/infrastructure.md)
- [Bestehende Tests und Hilfsmethoden](inventory/tests.md)
- [UI-Struktur: Blazor-Komponenten](inventory/ui.md)

---

## Relevante Code-Stellen

| Datei | Relevanz |
|---|---|
| `src/Schnittstellenzentrale.Tests/Helpers/ControllerTestFactory.cs` | Vorlage für `PlaywrightTestFactory`; zeigt Auth-Bypass, DB-Override, IHostedService-Entfernung, SignalR-Mock |
| `src/Schnittstellenzentrale.Tests/Helpers/TestAuthHandler.cs` | Wird unverändert in `PlaywrightTestFactory` eingehängt |
| `src/Schnittstellenzentrale.Tests/Helpers/TestHelpers.cs` | `CreateInMemoryDbContext`-Muster; für Playwright durch Datei-SQLite ersetzt |
| `src/Schnittstellenzentrale.Tests/Integration/EndpointExecutionIntegrationTests.cs` | Zeigt `IAsyncLifetime`-Muster für Factory-Lifecycle |
| `src/Schnittstellenzentrale.Tests/Integration/SystemEntryInitializerTests.cs` | Zeigt, wie `SystemEntryInitializer` im Testkontext aufgerufen wird |
| `src/Schnittstellenzentrale/Program.cs` | Registrierungsreihenfolge; `partial class Program` für `WebApplicationFactory` |
| `src/Schnittstellenzentrale/SystemEntryInitializer.cs` | Muss in `TestDatabaseSeeder` mit dynamischer `Api:BaseUrl` aufgerufen werden |
| `src/Schnittstellenzentrale/Hubs/EndpointHub.cs` | SignalR-Hub; in `SignalRSyncTests` nicht gemockt |
| `src/Schnittstellenzentrale.Infrastructure/Services/SignalRNotificationService.cs` | Produktive SignalR-Implementierung; in den meisten Playwright-Tests gemockt |
| `src/Schnittstellenzentrale.Infrastructure/Data/AppDbContext.cs` | DB-Schema; `EnsureCreated()` für `TestDatabaseSeeder` |
| `src/Schnittstellenzentrale.Infrastructure/Data/DatabaseProviderFactory.cs` | Zeigt, wie `IDbContextFactory<AppDbContext>` registriert wird |
| `src/Schnittstellenzentrale/Components/Layout/MainLayout.razor` | Modus-Dropdown für `StorageModeTests` |
| `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor` | Zentrales UI-Element für alle Tests; SignalR-Client-Logik |
| `src/Schnittstellenzentrale/Components/Shared/ApplicationCard.razor` | Öffnet Swagger-Import- und Health-Check-Dialog |
| `src/Schnittstellenzentrale/Components/Shared/SwaggerImportDialog.razor` | Ziel von `SwaggerImportTests` |
| `src/Schnittstellenzentrale/Components/Shared/HealthCheckDialog.razor` | Ziel von `HealthCheckTests` |
| `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor` | Ziel von `EndpointExecutionTests` |
| `src/Schnittstellenzentrale/Components/Shared/ApplicationEditor.razor` | Formular für `ApplicationCrudTests` |
| `src/Schnittstellenzentrale/appsettings.json` | Zeigt `Api:BaseUrl`-Konfigurationsschlüssel; muss in `PlaywrightTestFactory` überschrieben werden |
| `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj` | Ausgangspunkt für NuGet-Erweiterung um `Microsoft.Playwright` |

---

## Offene technische Fragen

1. **Auth-Bypass in Playwright-Kontext:** `TestAuthHandler` authentifiziert alle Requests im Test-Server. Da Playwright jedoch echte HTTP-Requests sendet (kein `HttpClient` aus der Factory), muss sichergestellt werden, dass der Test-Server für alle Requests — auch die von Playwright initiierten Blazor Server-Verbindungen (WebSocket/SSE) — das `"Test"`-Auth-Schema verwendet. Der Auth-Middleware-Stack in `Program.cs` verwendet `UseAuthentication()` + `UseAuthorization()`: Funktioniert `TestAuthHandler` hier reibungslos für alle Request-Typen (HTTP, WebSocket, SignalR)?

2. **Blazor Server vs. Playwright-Port:** `WebApplicationFactory` weist zufällige Ports zu. Blazor Server-Komponenten bauen zusätzlich eine SignalR-Verbindung zum Server auf (`/hubs/endpoint`). Playwright muss `Server.BaseAddress` kennen, damit Blazor die korrekte Hub-URL ableiten kann. Wie wird sichergestellt, dass `NavigationManager.BaseUri` im Testserver auf denselben Port zeigt wie `Server.BaseAddress`?

3. **Blazor Server Circuit und Playwright:** Playwright steuert den Browser; Blazor Server-Rendering läuft über einen SignalR-Circuit. `WebApplicationFactory` startet den Server normalerweise ohne echten Kestrel-Port. Für Playwright ist ein echter Port notwendig — muss `UseUrls` oder `WithWebHostBuilder(b => b.UseKestrel())` verwendet werden?

4. **`ICurrentUserService` in Tests:** `ApplicationGroupTree` ruft `CurrentUserService.GetCurrentUserName()` auf und übergibt den Owner an `GetGroupsAsync`. `WindowsCurrentUserService` liest den Windows-Benutzernamen. Im Testkontext (kein Windows-Auth) könnte dies fehlschlagen oder einen falschen Owner liefern — muss `ICurrentUserService` in `PlaywrightTestFactory` ebenfalls gemockt werden?

5. **SQLite WAL und parallele Tests:** Die Datei `schnittstellenzentrale-tests.db` könnte bei paralleler Testausführung zu Konflikten führen. Wird eine xUnit `[Collection("Playwright")]` mit Deaktivierung der Parallelisierung ausreichen, oder ist ein pro-Test-Datenbankname nötig?

6. **Blazor-spezifische JS-Interop:** `ApplicationGroupTree` führt JS-Aufrufe für Sidebar-Resize aus (`endpoint-page.js`). In Playwright-Tests mit echtem Browser sollte dies funktionieren — aber statische Dateien müssen korrekt vom Testserver ausgeliefert werden. Ist `MapStaticAssets()` im Test-Server aktiv?
