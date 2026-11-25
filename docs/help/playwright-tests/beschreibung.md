# Playwright-Tests — Beschreibung

## Zweck

Die Playwright-Tests sind browsergesteuerte End-to-End-Regressionstests für die Schnittstellenzentrale. Sie ergänzen die vorhandenen Unit- und Integrationstests um eine vollständige UI-Ebene und sichern zugleich die CI-Qualitätssicherung gegen ein fehlschlagendes Coverage-Gate ab.

## Funktionsweise

Die Tests starten die Anwendung über `PlaywrightServer` als echten Kestrel-HTTP-Server auf festen Ports (5099–5104); API-Aufrufe des Blazor-Servers laufen über den In-Process-`TestServer` der `PlaywrightApiFactory`. Windows-Authentifizierung wird durch den `TestAuthHandler` ersetzt, der jeden Request als `TEST\testuser` akzeptiert. Als Datenbasis dient eine benannte Shared-Cache-In-Memory-SQLite pro Server-Fixture, die vor jedem Test zurückgesetzt wird.

Playwright steuert einen Chromium-Browser (headless unter `CI=true`, lokal mit sichtbarem Fenster) und navigiert die Benutzeroberfläche über ARIA-Rollen, CSS-Selektoren und Label-basierte Lokalisierung. Alle Tests zeichnen Traces auf (Screenshots, DOM-Snapshots, Quelltexte); die Trace-Datei wird nach dem Test unter `playwright-traces/` abgelegt.

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

Die Tabelle zeigt die Kernabläufe; inzwischen existieren weitere Playwright-Testklassen (Impressum-Varianten, OData-Import/-Ausführung, Layout-Smoke, Icon-Upload, Navigation, Site-Screenshots u. a.) — Stand des Audits: 24 Klassen unter `Schnittstellenzentrale.Tests.Playwright`, alle mit `[OsInterface]` markiert (siehe [OS-Schnittstellen-Tests](../stabilitaet/os-interface-tests.md)).

## Coverage-Gate und UI-Abdeckung

Das Feature „Coverage Gate schlägt fehl“ ist kein produktiver Funktionszuwachs, sondern eine Stabilisierung der bestehenden Qualitätssicherung. Die Zielvorgabe bleibt eine Line-Coverage von mindestens `70 %` in der regulären Lane; die Umsetzung fokussiert sich auf bisher un- oder unterabgedeckte UI- und State-Pfade, damit der CI-Lauf regelfest bleibt.

Die relevanten Ergänzungen befinden sich in `src/Schnittstellenzentrale.Tests/Components/` und betreffen insbesondere:

- `ApplicationContentViewTests` — Abdeckung von Swagger-/OData-Import-Fehlern, erfolgreichem Import und zusätzlichen Branches.
- `EnvironmentSelectorTests` — Abdeckung für leere Listen, gelöschte Auswahl, lokale Speicherung und Null-Fälle bei fehlenden Umgebungen.
- `AppShellTests` — Sicherung der Initialisierungsreihenfolge und der Wiederherstellung aus `localStorage` bei unvollständigem State.
- `MainLayoutTests` — Prüfung von Moduswechseln, Speicherrecovery und korrektem Wiederherstellen der aktiven Umgebung.
- `TestMockFactory.CreateCoverageScenarioDependencies` — zentrale Hilfsfunktion für gemeinsame Mocks und Daten in Coverage-Testszenarien.

## Besonderheiten

**Auth-Mock:** `TestAuthHandler` ersetzt die Windows-Authentifizierung vollständig. Er akzeptiert jeden Request ohne Prüfung und stellt einen festen `ClaimsPrincipal` mit dem Namen `TEST\testuser` bereit. `ICurrentUserService` ist ebenfalls gemockt und gibt konstant `TEST\testuser` zurück, da `WindowsCurrentUserService` sonst den realen Windows-Benutzer des Testprozesses liefern würde.

**SQLite-Testdatenbank:** Die Tests verwenden eine benannte Shared-Cache-In-Memory-SQLite (`Data Source={guid};Mode=Memory;Cache=Shared`), die pro Server-Fixture angelegt und per geöffneter Anker-Verbindung am Leben gehalten wird — kein Datei-Locking zwischen Testläufen. `TestDatabaseSeeder.ResetAsync` leert den Datenbestand vor jedem Test deterministisch per `DELETE FROM` über alle Modelltabellen (`EnsureDeletedAsync` wäre auf `Mode=Memory`-DSNs wirkungslos — die offene Anker-Verbindung hält die DB am Leben; Hintergrund: Issue #118).

**SignalR-Tests mit zwei Browser-Kontexten:** `SignalRSyncTests` läuft gegen `PlaywrightSignalRServer`, eine `PlaywrightServer`-Unterklasse (Port 5100), die `SignalRNotificationService<EndpointHub>` echt registriert und den `ApiNotificationProxy` der `PlaywrightApiFactory` nach dem Start auf den echten `IHubContext` verdrahtet. Der Test öffnet zwei unabhängige `IBrowserContext`-Instanzen über denselben `IBrowser`, wechselt beide in den Team-Modus und prüft, ob Browser B die von Browser A angelegte Anwendung ohne manuelles Neuladen sieht.

**Trace-Aufzeichnung:** Jeder Test aktiviert das Playwright-Tracing mit `Screenshots = true`, `Snapshots = true` und `Sources = true`. Die Trace-Datei (`playwright-traces/{TestName}-HHmmssfff.zip`, im Repo-Root verankert über `TestPaths.FindRepoRoot()`, UTC-Zeitstempel-Suffix gegen Überschreiben) wird nach jedem Testlauf geschrieben — unabhängig davon, ob der Test bestanden hat oder fehlgeschlagen ist. Die Datei kann mit dem Playwright Trace Viewer eingesehen werden (siehe [Installation & Konfiguration](installation.md)).

**xUnit-Parallelisierung:** Die Playwright-Tests verteilen sich auf sechs xUnit-Collections mit je eigener Server-Fixture auf gestaffelten Ports (5099–5104): `"Playwright"` für die meisten Testklassen sowie `"PlaywrightImpressum"`, `"PlaywrightImpressumFallback"`, `"PlaywrightImpressumLanguage"`, `"PlaywrightODataEndpoint"` und `"PlaywrightSignalR"` für die Szenarien mit abweichender Server-Konfiguration. `SitePageTests` läuft bewusst ohne Collection und Server-Fixture (statische Seite per Route-Interception). Innerhalb einer Collection läuft immer nur eine Testklasse auf einmal — Fixture, Port und In-Memory-DB werden seriell geteilt.

## Einschränkungen

- Playwright-Tests laufen sequenziell innerhalb ihrer Collection; gleichzeitige Ausführung mit anderen Playwright-Tests ist ausgeschlossen.
- Die Server-Fixtures binden feste Ports (5099–5104). Ein zweiter paralleler Testlauf (z. B. in zwei Terminals) scheitert an der Port-Belegung (`WaitForServerReady`-Timeout mit BindUrl im Fehlertext).
- Die Trace-Dateien werden immer geschrieben und müssen manuell bereinigt werden; es gibt kein automatisches Cleanup bei erfolgreichem Testlauf.
- Der Chromium-Browser muss vorab installiert sein (wird über das MSBuild-Target `InstallPlaywright` automatisiert, siehe [Installation & Konfiguration](installation.md)).
