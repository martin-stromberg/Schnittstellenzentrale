# Playwright-Tests — Beschreibung

## Zweck

Die Playwright-Tests sind browsergesteuerte End-to-End-Regressionstests für die Schnittstellenzentrale. Sie ergänzen die vorhandenen Unit- und Integrationstests um eine vollständige UI-Ebene: Ein echter Chromium-Browser interagiert mit der laufenden Anwendung und stellt sicher, dass die sieben wichtigsten Benutzerabläufe funktionieren — von der Startseite über CRUD-Operationen bis hin zur SignalR-Echtzeitsynchronisation.

## Funktionsweise

Die Tests starten die Anwendung intern über `WebApplicationFactory<Program>` als echten Kestrel-HTTP-Server. Windows-Authentifizierung wird durch den `TestAuthHandler` ersetzt, der jeden Request als `TEST\testuser` akzeptiert. Als Datenbasis dient eine dedizierte SQLite-Datei `schnittstellenzentrale-tests.db`, die vor jedem Test vollständig zurückgesetzt wird.

Playwright steuert einen headless Chromium-Browser und navigiert die Benutzeroberfläche über ARIA-Rollen, CSS-Selektoren und Label-basierte Lokalisierung. Alle Tests zeichnen Traces auf (Screenshots, DOM-Snapshots, Quelltexte); die Trace-Datei wird nach dem Test unter `playwright-traces/` abgelegt.

## Abgedeckte Testklassen

| Testklasse | Abgedeckter Ablauf |
|---|---|
| `HomePageTests` | Startseite aufrufen; Systemgruppe „Schnittstellenzentrale" und Endpunkte im Baum prüfen |
| `ApplicationCrudTests` | Anwendung anlegen, umbenennen und löschen |
| `EndpointExecutionTests` | Endpunkt auswählen und ausführen; HTTP-2xx-Response im UI prüfen |
| `SwaggerImportTests` | Swagger-Import-Dialog öffnen, Import bestätigen, Endpunkte im Baum prüfen |
| `HealthCheckTests` | Health-Check-Dialog öffnen und Status-Meldung prüfen |
| `StorageModeTests` | Speichermodus zwischen „Team" und „User" wechseln |
| `SignalRSyncTests` | Zwei Browser-Kontexte: Browser A legt Anwendung an, Browser B empfängt die Änderung per SignalR ohne Reload |

## Besonderheiten

**Auth-Mock:** `TestAuthHandler` ersetzt die Windows-Authentifizierung vollständig. Er akzeptiert jeden Request ohne Prüfung und stellt einen festen `ClaimsPrincipal` mit dem Namen `TEST\testuser` bereit. `ICurrentUserService` ist ebenfalls gemockt und gibt konstant `TEST\testuser` zurück, da `WindowsCurrentUserService` sonst den realen Windows-Benutzer des Testprozesses liefern würde.

**SQLite-Testdatenbank:** Die Tests verwenden eine Datei-SQLite-Datenbank (`schnittstellenzentrale-tests.db`) statt einer In-Memory-Datenbank, weil Playwright als separater Prozess läuft und damit keinen Zugriff auf eine In-Process-Datenbank hätte. Die Datei wird vor jedem Test durch `TestDatabaseSeeder.ResetAsync` gelöscht und neu angelegt.

**SignalR-Tests mit zwei Browser-Kontexten:** `SignalRSyncTests` verwendet `PlaywrightSignalRFactory`, eine Unterklasse von `PlaywrightTestFactory`, bei der der `ISignalRNotificationService`-Mock entfernt wird. Stattdessen ist der echte `SignalRNotificationService<EndpointHub>` aktiv. Der Test öffnet zwei unabhängige `IBrowserContext`-Instanzen über denselben `IBrowser`, wechselt beide in den Team-Modus und prüft, ob Browser B die von Browser A angelegte Anwendung ohne manuelles Neuladen sieht.

**Trace-Aufzeichnung:** Jeder Test aktiviert das Playwright-Tracing mit `Screenshots = true`, `Snapshots = true` und `Sources = true`. Die Trace-Datei (`playwright-traces/{TestName}.zip`) wird nach jedem Testlauf geschrieben — unabhängig davon, ob der Test bestanden hat oder fehlgeschlagen ist. Die Datei kann mit dem Playwright Trace Viewer eingesehen werden (siehe [Installation & Konfiguration](installation.md)).

**xUnit-Parallelisierung:** Alle Playwright-Tests außer `SignalRSyncTests` gehören zur xUnit-Collection `"Playwright"` und teilen sich eine gemeinsame `PlaywrightTestFactory`-Instanz. `SignalRSyncTests` gehört zur separaten Collection `"PlaywrightSignalR"`. Innerhalb einer Collection läuft immer nur eine Testklasse auf einmal, um Dateikonflikte auf der SQLite-Datenbankdatei zu verhindern.

## Einschränkungen

- Playwright-Tests laufen sequenziell innerhalb ihrer Collection; gleichzeitige Ausführung mit anderen Playwright-Tests ist ausgeschlossen.
- Die SQLite-Datei `schnittstellenzentrale-tests.db` wird im Arbeitsverzeichnis des Testprozesses angelegt. Parallel laufende Testläufe (z. B. in zwei Terminals) können in Dateikonflikten resultieren.
- Die Trace-Dateien werden immer geschrieben und müssen manuell bereinigt werden; es gibt kein automatisches Cleanup bei erfolgreichem Testlauf.
- Der Chromium-Browser muss vorab installiert sein (wird über das MSBuild-Target `InstallPlaywright` automatisiert, siehe [Installation & Konfiguration](installation.md)).
