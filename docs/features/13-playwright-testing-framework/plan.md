# Umsetzungsplan: Playwright-Regressionstests für die wichtigsten UI-Abläufe

## Übersicht

Das Testprojekt `Schnittstellenzentrale.Tests` wird um eine Playwright-Infrastruktur (Factory, Basisklasse, Datenbank-Seeder) und sieben Playwright-Testklassen erweitert. Die Tests laufen gegen einen per `WebApplicationFactory` gestarteten Kestrel-Server mit echter Datei-SQLite-Datenbank, `TestAuthHandler`-Auth-Bypass und gemocktem `ISignalRNotificationService` (außer im SignalR-Test). Betroffen sind ausschließlich das Testprojekt und die Projektdatei; der Produktionscode bleibt unverändert.

---

## Programmabläufe

### Ablauf: Testserver-Start und Initialisierung

1. xUnit instanziiert `PlaywrightTestFactory` als `ICollectionFixture<PlaywrightTestFactory>`.
2. `PlaywrightTestFactory.ConfigureWebHost` ruft `builder.UseEnvironment("Testing")` auf und konfiguriert in `ConfigureTestServices`: Auth-Bypass via `TestAuthHandler`, Datei-SQLite-Connection auf `schnittstellenzentrale-tests.db`, Entfernung aller `IHostedService`-Registrierungen, Mock von `ISignalRNotificationService`, Mock von `ICurrentUserService`.
3. `PlaywrightTestFactory` überschreibt `CreateHost` und setzt die `Api:BaseUrl`-Konfiguration auf `Server.BaseAddress`, damit `SystemEntryInitializer` die korrekte URL einträgt. Da `Server.BaseAddress` erst nach `base.CreateHost(builder)` bekannt ist, wird sie unmittelbar danach in einer `ConfigureAppConfiguration`-Callback eingetragen.
4. `CreateHost` ruft nach dem Host-Start `EnsureCreated()` auf und danach `SystemEntryInitializer.InitializeAsync()` mit dem dynamischen `Api:BaseUrl`.
5. `PlaywrightTestFactory` legt `Server.BaseAddress` als public Property `BaseAddress` frei, damit `PlaywrightTestBase` die URL an `Page.GotoAsync` übergeben kann.

Beteiligte Klassen/Komponenten: `PlaywrightTestFactory`, `TestAuthHandler`, `AppDbContext`, `SystemEntryInitializer`, `ICurrentUserService`

---

### Ablauf: Test-Setup und Teardown (pro Testklasse)

1. `PlaywrightTestBase.InitializeAsync` ruft `TestDatabaseSeeder.ResetAsync` auf (löscht und re-erstellt Schema, führt Seed aus).
2. `PlaywrightTestBase.InitializeAsync` ruft `Playwright.CreateAsync()` auf, erzeugt `IBrowser` (Chromium, headless) und `IBrowserContext`.
3. `IBrowserContext.Tracing.StartAsync` wird mit `Screenshots = true`, `Snapshots = true`, `Sources = true` aufgerufen.
4. Der einzelne Test läuft.
5. `PlaywrightTestBase.DisposeAsync` prüft, ob der Test fehlgeschlagen ist:
   - Bei Fehler: `Context.Tracing.StopAsync` mit `Path = "playwright-traces/{TestName}.zip"`.
   - Bei Erfolg: `Context.Tracing.StopAsync` ohne `Path` (Trace verworfen).
6. `IBrowserContext`, `IBrowser` und `IPlaywright` werden disposed.

Beteiligte Klassen/Komponenten: `PlaywrightTestBase`, `TestDatabaseSeeder`, `IBrowser`, `IBrowserContext`

---

### Ablauf: Datenbankrücksetzung (vor jeder Testklasse)

1. `TestDatabaseSeeder.ResetAsync` erzeugt über `IDbContextFactory<AppDbContext>` einen `AppDbContext`.
2. `db.Database.EnsureDeleted()` löscht die SQLite-Datei.
3. `db.Database.EnsureCreated()` legt das Schema neu an.
4. `SystemEntryInitializer.InitializeAsync` wird mit dem `IServiceProvider` der Factory und der `Api:BaseUrl`-Konfiguration aufgerufen, um die Systemgruppe und -anwendung anzulegen.

Beteiligte Klassen/Komponenten: `TestDatabaseSeeder`, `AppDbContext`, `SystemEntryInitializer`

---

### Ablauf: SignalR-Echtzeitsynchronisation (Ablauf 7)

1. `SignalRSyncTests` erhält `PlaywrightSignalRFactory` als Fixture (Variante ohne `ISignalRNotificationService`-Mock).
2. `InitializeAsync` erzeugt zwei unabhängige `IBrowserContext`-Instanzen (`ContextA`, `ContextB`) über denselben `IBrowser`.
3. Beide Kontexte navigieren zur Startseite und wechseln den Speichermodus auf `Team`.
4. `ContextA.Page` legt über das UI eine neue Anwendung an (Formular ausfüllen, speichern).
5. `ContextB.Page` wartet via `Expect(locator).ToBeVisibleAsync()` auf den neuen Eintrag im `ApplicationGroupTree`, ohne `Page.ReloadAsync` aufzurufen.
6. Der Test gilt als bestanden, wenn die neue Anwendung in Browser B sichtbar wird.

Beteiligte Klassen/Komponenten: `SignalRSyncTests`, `PlaywrightSignalRFactory`, `EndpointHub`, `SignalRNotificationService<EndpointHub>`, `ApplicationGroupTree`

---

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `PlaywrightTestFactory` | Klasse (erbt von `WebApplicationFactory<Program>`) | Startet den Testserver mit Kestrel-Port, Auth-Bypass, Datei-SQLite und Service-Overrides für alle Playwright-Tests außer SignalR |
| `PlaywrightSignalRFactory` | Klasse (erbt von `PlaywrightTestFactory`) | Überschreibt `ConfigureTestServices` um den `ISignalRNotificationService`-Mock zu entfernen, sodass der echte `SignalRNotificationService<EndpointHub>` aktiv bleibt |
| `PlaywrightTestBase` | abstrakte Klasse (implementiert `IAsyncLifetime`) | Basisklasse für alle Playwright-Tests; verwaltet `IPlaywright`, `IBrowser`, `IBrowserContext`; aktiviert Tracing und speichert Traces bei Fehlern |
| `TestDatabaseSeeder` | Klasse | Löscht und re-erstellt `schnittstellenzentrale-tests.db`; ruft `SystemEntryInitializer.InitializeAsync` auf |
| `PlaywrightCollection` | xUnit Collection-Definition (`[CollectionDefinition]`) | Gruppiert alle Playwright-Tests in `[Collection("Playwright")]`; deaktiviert Parallelisierung zwischen Testklassen |
| `HomePageTests` | Testklasse | Ablauf 1: Startseite, Systemgruppe und Systemendpunkte im Baum prüfen |
| `ApplicationCrudTests` | Testklasse | Ablauf 2: Anwendung anlegen, bearbeiten, löschen |
| `EndpointExecutionTests` | Testklasse | Ablauf 3: Endpunkt auswählen und über `EndpointPage` ausführen |
| `SwaggerImportTests` | Testklasse | Ablauf 4: Swagger-Import-Dialog, URL eintragen, importierte Endpunkte prüfen |
| `HealthCheckTests` | Testklasse | Ablauf 5: Health-Check-Dialog öffnen und Ergebnis prüfen |
| `StorageModeTests` | Testklasse | Ablauf 6: Speichermodus zwischen `Team` und `User` wechseln |
| `SignalRSyncTests` | Testklasse | Ablauf 7: Zwei Browser-Kontexte, SignalR-Live-Update prüfen |

---

## Änderungen an bestehenden Klassen

### `Schnittstellenzentrale.Tests.csproj` (Projektdatei)

- **Neue Abhängigkeiten:** `Microsoft.Playwright` (aktuelle stabile Version) und `Microsoft.Playwright.NUnit` werden als `PackageReference` hinzugefügt. Da das Projekt xUnit verwendet, wird `Microsoft.Playwright.NUnit` nur als Quelle für den Playwright-Lifecycle-Adapter genutzt; alternativ wird das xUnit-kompatible `Playwright.xunit`-Paket geprüft. Empfehlung: `Microsoft.Playwright` ohne NUnit-Adapter verwenden und den Lebenszyklus vollständig über `IAsyncLifetime` in `PlaywrightTestBase` selbst verwalten — das vermeidet eine NUnit-Abhängigkeit im xUnit-Projekt.
- **Neues MSBuild-Target:** Ein `<Target Name="InstallPlaywright">` wird hinzugefügt, das nach dem Build `playwright install chromium` ausführt. Dies stellt sicher, dass Playwright-Browser bei `dotnet test` automatisch vorhanden sind.

### `ControllerTestFactory` (bestehende Helferklasse)

- Keine funktionale Änderung. Bleibt unverändert als Factory für Controller-Integrationstests.

---

## Datenbankmigrationen

Keine. Die Playwright-Tests verwenden `EnsureDeleted()` + `EnsureCreated()` und arbeiten gegen das bestehende EF-Core-Schema. Keine produktiven Migrationen erforderlich.

---

## Validierungsregeln

Keine. Playwright-Tests prüfen bestehende Validierungen im UI, führen aber keine neuen Validierungsregeln ein.

---

## Konfigurationsänderungen

| Eintrag | Typ | Standardwert | Zweck |
|---------|-----|--------------|-------|
| `Api:BaseUrl` (Laufzeit-Override) | `string` | Wird in `PlaywrightTestFactory.CreateHost` dynamisch auf `Server.BaseAddress` gesetzt | Stellt sicher, dass `SystemEntryInitializer` die korrekte Testserver-URL einträgt |
| `ConnectionStrings:Default` (Laufzeit-Override) | `string` | `Data Source=schnittstellenzentrale-tests.db` | Lenkt `IDbContextFactory<AppDbContext>` auf die Test-SQLite-Datei |
| `DatabaseProvider` (Laufzeit-Override) | `string` | `SQLite` | Stellt sicher, dass `DatabaseProviderFactory` SQLite verwendet |

Alle drei Overrides werden ausschließlich in `PlaywrightTestFactory.ConfigureWebHost` per `builder.ConfigureAppConfiguration` gesetzt — keine Änderung an `appsettings.json` oder `appsettings.Testing.json` notwendig.

---

## Seiteneffekte und Risiken

- **Datei-SQLite bei paralleler Ausführung:** Die Datei `schnittstellenzentrale-tests.db` kann nicht von mehreren Prozessen gleichzeitig beschrieben werden. `PlaywrightCollection` verhindert dies durch Deaktivierung der Parallelisierung innerhalb der Playwright-Testsuite. Die bestehenden Controller-Integrationstests verwenden In-Memory-SQLite und sind davon nicht betroffen.
- **`TestAuthHandler` und Blazor Server Circuit:** `TestAuthHandler` authentifiziert alle HTTP-Requests inklusive WebSocket-Handshakes für Blazor Server und SignalR. Da der Handler jeden Request ohne Prüfung als `TEST\testuser` akzeptiert, sind keine Anpassungen am Auth-Stack nötig. Voraussetzung ist, dass `UseAuthentication()` + `UseAuthorization()` im Test-Server aktiv bleiben (sie werden durch `ConfigureTestServices` nicht entfernt).
- **`ICurrentUserService`-Mock:** `WindowsCurrentUserService` liest den Windows-Benutzernamen aus dem Betriebssystem, nicht aus dem `ClaimsPrincipal`. In Playwright-Tests würde er daher den Windows-Systembenutzernamen liefern, nicht `TEST\testuser`. Um konsistente Daten sicherzustellen, wird `ICurrentUserService` in `PlaywrightTestFactory` durch einen Mock ersetzt, der konstant `TEST\testuser` zurückliefert. Bestehende Tests sind nicht betroffen (sie verwenden `ControllerTestFactory`).
- **Statische Assets:** `MapStaticAssets()` ist in `Program.cs` aktiv und wird vom Testserver übernommen. Playwright erhält damit korrekte CSS-, JS- und Blazor-Boot-Ressourcen. Kein Anpassungsbedarf.
- **`TestDatabaseSeeder` vs. `SystemEntryInitializer` in `CreateHost`:** `CreateHost` in `PlaywrightTestFactory` legt den Systemeintrag einmalig beim Serverstart an. `TestDatabaseSeeder.ResetAsync` löscht die Datenbank und ruft `SystemEntryInitializer` erneut auf. Dieses doppelte Initialisierungsmuster ist bewusst — der erste Aufruf in `CreateHost` dient der Serverbereitschaft, der Reset in `TestDatabaseSeeder` stellt den definierten Ausgangszustand vor jedem Test sicher.
- **Bestehende Tests:** Keine bestehenden Tests sind betroffen. `PlaywrightTestFactory` ist eine neue, parallele Factory. `ControllerTestFactory` und alle bestehenden Testklassen bleiben unverändert.

---

## Umsetzungsreihenfolge

1. **NuGet-Pakete hinzufügen:** `Microsoft.Playwright` in `Schnittstellenzentrale.Tests.csproj` eintragen; MSBuild-Target für `playwright install chromium` anlegen.
2. **`PlaywrightTestFactory` anlegen:** `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightTestFactory.cs` — Auth-Bypass, Datei-SQLite-Override, `IHostedService`-Entfernung, `ISignalRNotificationService`-Mock, `ICurrentUserService`-Mock, `Api:BaseUrl`-Override auf `Server.BaseAddress`.
3. **`PlaywrightSignalRFactory` anlegen:** `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightSignalRFactory.cs` — erbt von `PlaywrightTestFactory`, entfernt den `ISignalRNotificationService`-Mock wieder.
4. **`TestDatabaseSeeder` anlegen:** `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/TestDatabaseSeeder.cs` — `EnsureDeleted()`, `EnsureCreated()`, `SystemEntryInitializer.InitializeAsync`.
5. **`PlaywrightCollection` anlegen:** `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightCollection.cs` — `[CollectionDefinition("Playwright")]` mit `ICollectionFixture<PlaywrightTestFactory>`.
6. **`PlaywrightTestBase` anlegen:** `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightTestBase.cs` — `IAsyncLifetime`, Playwright-Init, Tracing-Start, Trace-Speicherung bei Fehler, `TestDatabaseSeeder`-Aufruf.
7. **`HomePageTests` anlegen:** `src/Schnittstellenzentrale.Tests/Playwright/HomePageTests.cs`.
8. **`ApplicationCrudTests` anlegen:** `src/Schnittstellenzentrale.Tests/Playwright/ApplicationCrudTests.cs`.
9. **`EndpointExecutionTests` anlegen:** `src/Schnittstellenzentrale.Tests/Playwright/EndpointExecutionTests.cs`.
10. **`SwaggerImportTests` anlegen:** `src/Schnittstellenzentrale.Tests/Playwright/SwaggerImportTests.cs`.
11. **`HealthCheckTests` anlegen:** `src/Schnittstellenzentrale.Tests/Playwright/HealthCheckTests.cs`.
12. **`StorageModeTests` anlegen:** `src/Schnittstellenzentrale.Tests/Playwright/StorageModeTests.cs`.
13. **`SignalRSyncTests` anlegen:** `src/Schnittstellenzentrale.Tests/Playwright/SignalRSyncTests.cs` — verwendet `PlaywrightSignalRFactory`, zwei `IBrowserContext`-Instanzen.

---

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `StartPage_ShowsSystemGroup` | `HomePageTests` | Systemgruppe „Schnittstellenzentrale" ist im `ApplicationGroupTree` sichtbar |
| `StartPage_ShowsOwnApiEndpoints` | `HomePageTests` | Endpunkte der Systemanwendung erscheinen nach Aufklappen im Baum |
| `CreateApplication_AppearsInTree` | `ApplicationCrudTests` | Neue Anwendung ist nach dem Speichern im Baum sichtbar |
| `EditApplication_UpdatesNameInTree` | `ApplicationCrudTests` | Geänderter Name der Anwendung erscheint im Baum |
| `DeleteApplication_DisappearsFromTree` | `ApplicationCrudTests` | Gelöschte Anwendung ist nicht mehr im Baum vorhanden |
| `ExecuteEndpoint_ReturnsSuccessResponse` | `EndpointExecutionTests` | HTTP-2xx-Response erscheint im Response-Bereich der `EndpointPage` |
| `ImportSwagger_ImportsEndpointsIntoTree` | `SwaggerImportTests` | Importierte Endpunkte sind nach Bestätigung im Baum der Systemanwendung sichtbar |
| `HealthCheck_ShowsReachableStatus` | `HealthCheckTests` | `HealthCheckDialog` zeigt „erreichbar"-Status nach Ausführung |
| `SwitchToTeamMode_ShowsTeamData` | `StorageModeTests` | Nach Wechsel auf `Team` zeigt `ApplicationGroupTree` Team-Daten |
| `SwitchBackToUserMode_ShowsUserData` | `StorageModeTests` | Nach Rückwechsel auf `User` zeigt `ApplicationGroupTree` User-Daten |
| `BrowserA_CreatesApp_BrowserB_ReceivesViaSignalR` | `SignalRSyncTests` | Browser B zeigt neue Anwendung ohne Reload, nachdem Browser A sie angelegt hat |

### Betroffene bestehende Tests

Keine.

---

## Offene Punkte

| # | Offener Punkt | Empfohlener Vorschlag |
|---|---------------|----------------------|
| 1 | **Kestrel-Port für Playwright:** `WebApplicationFactory` startet standardmäßig ohne echten Kestrel-Port (TestServer nutzt In-Process-Transport). Playwright benötigt einen echten HTTP-Port. | `PlaywrightTestFactory` überschreibt `CreateHost` und ruft zusätzlich `builder.ConfigureWebHost(b => b.UseUrls("http://127.0.0.1:0"))` auf, um Kestrel auf einem zufälligen Port zu starten. `Server.BaseAddress` liefert danach die tatsächliche Adresse. Alternativ kann `factory.CreateClient()` aufgerufen werden, um den Server zu starten, und dann `factory.Server.BaseAddress` ausgelesen werden. |
| 2 | **Auth-Bypass für Blazor Server Circuit und SignalR-WebSocket:** `TestAuthHandler` wird für alle Request-Typen eingehängt. Blazor Server verbindet sich per WebSocket mit `/_blazor` und SignalR per WebSocket mit `/hubs/endpoint`. | `TestAuthHandler` als einziges Auth-Schema ist ausreichend, weil `UseAuthentication()` alle Request-Typen (HTTP, WebSocket) behandelt. Kein separater Middleware-Bypass notwendig. Explizit prüfen, dass `AddAuthentication("Test")` ohne `.AddNegotiate()` bleibt — `RemoveAll<IConfigureOptions<AuthenticationOptions>>()` aus `ControllerTestFactory` übernehmen. |
| 3 | **SignalR im Testserver — CORS und Auth:** `EndpointHub` verwendet dasselbe Auth-Schema wie der Rest der Anwendung. In `PlaywrightSignalRFactory` muss sichergestellt werden, dass `AddSignalR()` aktiv bleibt und kein CORS-Problem entsteht, da Browser und Server auf derselben Origin (127.0.0.1) laufen. | Keine CORS-Konfiguration notwendig, da Same-Origin. `AddSignalR()` wird durch `ConfigureTestServices` nicht entfernt. Die Verbindung von `ApplicationGroupTree` zu `/hubs/endpoint` sollte ohne weitere Anpassungen funktionieren. |
| 4 | **Playwright xUnit-Adapter:** Es gibt keinen offiziellen Playwright xUnit-Adapter. `Microsoft.Playwright.NUnit` enthält NUnit-spezifische Basisklassen. | `Microsoft.Playwright` (ohne NUnit-Adapter) hinzufügen und den Playwright-Lebenszyklus vollständig über `IAsyncLifetime` in `PlaywrightTestBase` selbst verwalten. `IPlaywright`, `IBrowser` und `IBrowserContext` werden manuell erzeugt und disposed. Dies ist die sauberste Lösung für ein reines xUnit-Projekt. |
| 5 | **Datenbankisolation: pro Klasse oder pro Methode:** Jeder DB-Reset ist teuer (Datei löschen, Schema neu anlegen, Seed). | Reset pro Testklasse (in `InitializeAsync` von `PlaywrightTestBase`), nicht pro Testmethode. Die Tests innerhalb einer Klasse sind so zu gestalten, dass sie keinen gemeinsamen Zustand voraussetzen (d. h. jeder Test legt seine eigenen Daten an). |
| 6 | **`playwright install`-Automatisierung:** Browser müssen lokal und in der CI vorhanden sein. | MSBuild-Target in `Schnittstellenzentrale.Tests.csproj`, das `dotnet tool run playwright install chromium` nach dem Build ausführt (`AfterTargets="Build"`). Dies läuft bei jedem `dotnet build` und stellt sicher, dass die Browser stets aktuell sind. |
| 7 | **Trace-Speicherort in der CI:** Playwright-Traces sollen bei Fehlern als Build-Artefakte verfügbar sein. | Traces werden nach `playwright-traces/` relativ zum Testausführungsverzeichnis geschrieben. Der CI-Schritt (z. B. GitHub Actions) muss `playwright-traces/**` als Artefakt hochladen. Die Konfiguration des CI-Workflows liegt außerhalb dieses Plans und muss separat ergänzt werden. |
| 8 | **Fehlererkennung für bedingte Trace-Speicherung in xUnit:** xUnit bietet keine eingebaute API, um in `DisposeAsync` zu prüfen, ob ein Test fehlgeschlagen ist. | Einen `bool _testFailed`-Flag in `PlaywrightTestBase` führen, der in einem `try/catch` um den Testaufruf gesetzt wird — oder alternativ immer den Trace speichern und bei Erfolg löschen. Empfehlung: immer speichern, Traces nach erfolgreichem Lauf automatisch durch ein Cleanup-Target löschen, da das bedingte Speichern in xUnit ohne Framework-Hooks aufwändig ist. |
