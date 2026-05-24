# Anforderung: Playwright-Regressionstests für die wichtigsten UI-Abläufe

## Fachliche Zusammenfassung

Das bestehende Testprojekt `Schnittstellenzentrale.Tests` wird um eine neue Kategorie von End-to-End-Tests erweitert: Playwright-gesteuerte UI-Tests, die gegen einen automatisch per `WebApplicationFactory` gestarteten Server laufen. Die Windows-Authentifizierung wird durch den bereits vorhandenen `TestAuthHandler` (Custom `AuthenticationHandler`) ersetzt, der einen festen Test-Benutzer (`TEST\testuser`) liefert. Als Datenbasis dient eine dedizierte SQLite-Datei `schnittstellenzentrale-tests.db`, die vor jedem Testlauf in einen definierten Ausgangszustand gebracht wird. Die sieben zu testenden Abläufe decken die Kernfunktionen der Anwendung ab: Startseite und Systemregistrierung, CRUD für Anwendungen, Endpunktausführung, Swagger-Import, Health-Check, Speichermodus-Wechsel sowie SignalR-Echtzeitsynchronisation im Team-Modus.

---

## Betroffene Klassen und Komponenten

### Neue Infrastruktur- und Hilfsklassen (`Schnittstellenzentrale.Tests`)

| Artefakt | Beschreibung |
|---|---|
| `PlaywrightTestFactory` | Neue Klasse, abgeleitet von `WebApplicationFactory<Program>`. Ersetzt die SQLite-In-Memory-Verbindung aus `ControllerTestFactory` durch eine Dateiverbindung auf `schnittstellenzentrale-tests.db`. Hängt `TestAuthHandler` als einziges Auth-Schema ein. Entfernt `IHostedService`-Registrierungen, mockt `ISignalRNotificationService` oder lässt ihn für SignalR-Tests durchlaufen. |
| `PlaywrightTestBase` | Basisklasse (oder `IAsyncLifetime`-Fixture) für alle Playwright-Tests. Enthält Initialisierung von `IPlaywright`, `IBrowser` und `IBrowserContext`; kümmert sich um Trace-Aktivierung (`StartTracingAsync`) und Speichern von Traces bei Testfehlern. |
| `TestDatabaseSeeder` | Hilfsklasse zum Zurücksetzen und Befüllen der SQLite-Datei `schnittstellenzentrale-tests.db` vor jedem Testlauf. Führt `EnsureDeleted()` + `EnsureCreated()` (oder `Migrate()`) aus und legt definierte Seed-Daten an. |

### Neue Playwright-Testklassen (`Schnittstellenzentrale.Tests/Playwright`)

| Testklasse | Beschreibung |
|---|---|
| `HomePageTests` | Ablauf 1: Startseite aufrufen; Systemeintrag „Schnittstellenzentrale" im `ApplicationGroupTree` prüfen; erwartete Endpunkte der eigenen API im Baum verifizieren. |
| `ApplicationCrudTests` | Ablauf 2: Anwendung anlegen (Name, Base-URL, Typ); Sichtbarkeit im Baum prüfen; Anwendung bearbeiten (Name ändern); Änderung im Baum verifizieren; Anwendung löschen und Verschwinden aus dem Baum bestätigen. |
| `EndpointExecutionTests` | Ablauf 3: Endpunkt der eigenen API im Baum auswählen; Endpunkt über `EndpointExecutionPanel` / `EndpointPage` ausführen; Response im UI prüfen (HTTP 2xx). |
| `SwaggerImportTests` | Ablauf 4: Swagger-Import-Dialog für die Systemanwendung öffnen; Swagger-URL der eigenen API eintragen; Import bestätigen; importierte Endpunkte im Baum der zugehörigen Anwendung prüfen. |
| `HealthCheckTests` | Ablauf 5: Health-Check-Dialog für eine Anwendung öffnen; Health-Check ausführen; Ergebnis (Statusanzeige) im `HealthCheckDialog` verifizieren. |
| `StorageModeTests` | Ablauf 6: Speichermodus von `User` auf `Team` umstellen; Team-Daten im `ApplicationGroupTree` prüfen; zurück auf `User` wechseln und User-Daten verifizieren. |
| `SignalRSyncTests` | Ablauf 7: Zwei `IBrowserContext`-Instanzen im Team-Modus öffnen; Browser A legt Anwendung an; Browser B empfängt die Änderung per SignalR ohne manuelles Neu-Laden und zeigt die neue Anwendung im Baum. |

### Zu erweiternde Hilfsklassen (`Schnittstellenzentrale.Tests`)

| Artefakt | Änderung |
|---|---|
| `ControllerTestFactory` | Keine funktionale Änderung; `PlaywrightTestFactory` wird als eigenständige, parallele Factory hinzugefügt, die für Playwright-Szenarien optimiert ist (Datenbankdatei statt In-Memory, echter HTTP-Port). |

### Neue NuGet-Abhängigkeiten (`Schnittstellenzentrale.Tests.csproj`)

| Paket | Beschreibung |
|---|---|
| `Microsoft.Playwright` | Playwright .NET-SDK für Browser-Steuerung. |
| `Microsoft.Playwright.NUnit` oder Playwright xUnit-Adapter | xUnit-Integration für Playwright-Lebenszyklusverwaltung. |

---

## Implementierungsansatz

### Server-Start und Auth-Bypass

`PlaywrightTestFactory` leitet von `WebApplicationFactory<Program>` ab (analog zu `ControllerTestFactory`). In `ConfigureTestServices` wird:
- das komplette Authentication-Schema durch `TestAuthHandler` mit festem Benutzer `TEST\testuser` ersetzt,
- `IDbContextFactory<AppDbContext>` auf eine Datei-SQLite-Verbindung (`schnittstellenzentrale-tests.db`) umgestellt,
- `IHostedService`-Registrierungen werden entfernt (mit Ausnahme der für SignalR-Tests benötigten Hubs),
- `ISignalRNotificationService` wird in den meisten Tests gemockt (in `SignalRSyncTests` aber real durchgeleitet).

Da Playwright einen echten HTTP-Port benötigt, muss `PlaywrightTestFactory` über `CreateDefaultClient()` oder `Server.BaseAddress` den tatsächlichen Testserver-Port ermitteln und an Playwright weitergeben.

### Datenbankinitialisierung

`TestDatabaseSeeder` wird in der `IAsyncLifetime.InitializeAsync`-Methode des Fixtures aufgerufen. Er löscht und re-erstellt die SQLite-Datei (kein In-Memory, da Playwright-Prozesse eine echte Datei benötigen) und befüllt sie mit Seed-Daten (z. B. Systemgruppe/-anwendung via `SystemEntryInitializer`). Nach dem Testlauf (`DisposeAsync`) wird die Datei gelöscht oder geleert.

### Playwright-Traces

In `PlaywrightTestBase` wird Tracing per `Context.Tracing.StartAsync(new TracingStartOptions { Screenshots = true, Snapshots = true, Sources = true })` gestartet. Bei Testfehler wird der Trace per `Context.Tracing.StopAsync(new TracingStopOptions { Path = "playwright-traces/{TestName}.zip" })` gespeichert. Bei Testerfolg wird der Trace verworfen.

### SignalR-Test (Ablauf 7)

`SignalRSyncTests` erstellt zwei unabhängige `IBrowserContext`-Instanzen über denselben `IBrowser`. Beide navigieren zur Anwendung und stellen den Speichermodus auf `Team`. Browser A führt eine CRUD-Aktion durch (Anwendung anlegen). Browser B wartet via `Page.WaitForSelectorAsync` (oder `Expect(locator).ToBeVisibleAsync()`) auf die neue Anwendung im Baum, ohne die Seite manuell neu zu laden. `ISignalRNotificationService` wird in diesem Test nicht gemockt — die echte `SignalRNotificationService`-Implementierung inkl. `EndpointHub` wird durchgeschaltet.

### Testablauf-Isolation

Jeder Test erhält eine frisch initialisierte Datenbank. Da Playwright-Tests tendenziell länger laufen, werden sie als separate `[Collection]` gruppiert (xUnit `ICollectionFixture`), um parallele Ausführung mit Unit-Tests zu verhindern und Datenbankkonflikte zu vermeiden.

---

## Konfiguration

| Schlüssel | Ebene | Beschreibung |
|---|---|---|
| SQLite-Dateiname `schnittstellenzentrale-tests.db` | Testprojekt (`PlaywrightTestFactory`) | Fest codierter Dateiname im Testverzeichnis; kein `appsettings.json`-Eintrag erforderlich. |
| `Api:BaseUrl` | `appsettings.Testing.json` oder Testfactory-Override | Muss auf die dynamisch zugewiesene `Server.BaseAddress` des `PlaywrightTestFactory`-Servers zeigen, damit `SystemEntryInitializer` die korrekte URL einträgt. |
| Playwright-Trace-Pfad | Test-Code (`PlaywrightTestBase`) | Standardmäßig `playwright-traces/` relativ zum Testausführungsverzeichnis; nicht konfigurierbar (Annahme). |

---

## Offene Fragen

1. **Auth-Bypass-Einbaupunkt:** Wird `TestAuthHandler` (wie bisher in `ControllerTestFactory`) als Authentication-Schema in `ConfigureTestServices` eingehängt, oder ist ein separater Middleware-Bypass (z. B. eigene `IMiddlewareFactory`) sinnvoller? Entscheidung fällt in `/plan`.

2. **Playwright-Port-Ermittlung:** `WebApplicationFactory` weist beim Start einen zufälligen Port zu. Wie wird dieser Port zuverlässig an Playwright übergeben — über `Server.BaseAddress` oder durch explizites Binden auf einen festen Test-Port (`UseUrls`)? Zufälliger Port ist bevorzugt (kein Portkonflikt), erfordert aber korrekte Weitergabe.

3. **SignalR im Test-Server:** Benötigt der SignalR-Hub `EndpointHub` für `SignalRSyncTests` spezielle Konfiguration im Testserver (z. B. aktiviertes CORS, kein Token-Auth)? Oder funktioniert der Hub mit dem `TestAuthHandler`-Schema reibungslos?

4. **Datenbankzustand zwischen Tests:** Werden alle Playwright-Tests sequenziell mit einer gemeinsamen Datenbankdatei ausgeführt (Seed vor jeder Klasse), oder erhält jeder einzelne Test eine frisch initialisierte Datenbank (teuer, aber isolierter)? Empfehlung: Seed pro Testklasse, nicht pro Testmethode.

5. **Playwright-Browser-Installation:** `playwright install` muss einmalig ausgeführt werden. Soll dies über ein MSBuild-Target im Testprojekt automatisiert werden, oder obliegt es dem Entwickler/der Pipeline?

6. **xUnit-Parallelisierung:** Playwright-Tests sind langsam und sollten nicht parallel zu anderen Tests laufen. Soll eine eigene `[Collection("Playwright")]` mit `[DisableParallelization]` verwendet werden, oder reicht ein separates xUnit-Assembly?

7. **Trace-Speicherort in der Pipeline:** Sollen Playwright-Traces als Build-Artefakte in der CI-Pipeline veröffentlicht werden? Falls ja, muss der Pfad mit dem Artefakt-Upload-Schritt abgestimmt werden.
