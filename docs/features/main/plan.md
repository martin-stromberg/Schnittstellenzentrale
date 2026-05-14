# Umsetzungsplan: Schnittstellenzentrale – Initiale Anwendung

## Übersicht

Es wird eine neue Blazor Server App für den IIS-Betrieb erstellt, die als zentrale Verwaltungsoberfläche für lokale Webservice-Endpunkte dient. Da kein Quellcode existiert, sind alle Klassen, Interfaces, Services, UI-Komponenten und Tests vollständig neu zu erstellen. Die Solution besteht aus vier Projekten: `Schnittstellenzentrale` (Blazor Server), `Schnittstellenzentrale.Core` (Domäne), `Schnittstellenzentrale.Infrastructure` (EF Core, Services) und `Schnittstellenzentrale.Tests`.

---

## Neue Klassen

### Enums (`Schnittstellenzentrale.Core`)

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `StorageMode` | Enum | Steuert den Speicherbereich: `Team` (global) oder `User` (benutzerspezifisch). |
| `HttpMethod` | Enum | HTTP-Methoden: `GET`, `POST`, `PUT`, `DELETE`, `PATCH`, `HEAD`, `OPTIONS`. |
| `AuthenticationType` | Enum | Authentifizierungstypen: `None`, `Basic`, `Negotiate`, `BearerToken`, `NegotiateWithImpersonation`. |

### Datenmodellklassen (`Schnittstellenzentrale.Core`)

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `ApplicationGroup` | Datenmodellklasse | Gruppe, der Anwendungen zugeordnet werden können (optional). |
| `Application` | Datenmodellklasse | Webservice-Anwendung mit Name, Beschreibung, Basis-URL, optionaler Swagger-URL, optionaler `$metadata`-URL und Zugehörigkeit zu einer `ApplicationGroup`. |
| `EndpointGroup` | Datenmodellklasse | Untergruppe innerhalb einer Anwendung zur Organisation von Endpunkten. |
| `Endpoint` | Datenmodellklasse | HTTP-Endpunkt einer Anwendung mit Methode, relativem Pfad, Headern, Query-Parametern, Body und Authentifizierungstyp. Gehört zu einer `Application` und optional zu einer `EndpointGroup`. |
| `EndpointHeader` | Datenmodellklasse | Schlüssel-Wert-Paar für Header eines Endpunkts. |
| `EndpointQueryParameter` | Datenmodellklasse | Schlüssel-Wert-Paar für Query-Parameter eines Endpunkts. |
| `ImportDiff` | Datenmodellklasse | Ergebnisobjekt eines Swagger- oder OData-Imports: enthält neue, geänderte und entfernte Endpunkte für die Diff-Vorschau. |

### Interfaces (`Schnittstellenzentrale.Core`)

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `IApplicationRepository` | Interface | CRUD-Operationen für `Application` und `ApplicationGroup`, storageMode-bewusst. |
| `IEndpointRepository` | Interface | CRUD-Operationen für `Endpoint`, `EndpointGroup`, `EndpointHeader` und `EndpointQueryParameter`. |
| `IEndpointExecutionService` | Interface | Ausführung eines Endpunkts per `HttpClient`. |
| `ISwaggerImportService` | Interface | Import von Endpunkten aus einer Swagger/OpenAPI-Definition. |
| `IODataImportService` | Interface | Import von Endpunkten aus einem OData-`$metadata`-Dokument. |
| `IHealthCheckService` | Interface | Erreichbarkeitsprüfung einer Anwendung mit Cooldown-Mechanismus. |
| `ICredentialService` | Interface | Lesen und Schreiben von Passwörtern/Tokens über den Windows Credential Manager. |
| `IStorageModeService` | Interface | Verwaltung des aktiven `StorageMode` pro Benutzer-Sitzung. |
| `ISignalRNotificationService` | Interface | Senden von Änderungsbenachrichtigungen im Teammodus über SignalR. |

### EF Core / Infrastruktur (`Schnittstellenzentrale.Infrastructure`)

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `AppDbContext` | Klasse | EF-Core-Kontext mit allen Entitäten; Provider wird per `DatabaseProviderFactory` injiziert. |
| `DatabaseProviderFactory` | Klasse | Liest `DatabaseProvider` aus `appsettings.json` und registriert den passenden EF-Core-Provider (`SQLite` oder `SqlServer`). |
| `ApplicationRepository` | Klasse | Implementiert `IApplicationRepository`. |
| `EndpointRepository` | Klasse | Implementiert `IEndpointRepository`. |
| `EndpointExecutionService` | Klasse | Implementiert `IEndpointExecutionService`; wählt Authentifizierungsstrategie anhand `AuthenticationType`; löst bei Verbindungsfehlern ggf. einen Health-Check aus. |
| `SwaggerImportService` | Klasse | Implementiert `ISwaggerImportService`; nutzt `Microsoft.OpenApi` zur Deserialisierung; liefert `ImportDiff`. |
| `ODataImportService` | Klasse | Implementiert `IODataImportService`; parst `$metadata` via `Microsoft.OData.Edm`; liefert `ImportDiff`. |
| `HealthCheckService` | Klasse | Implementiert `IHealthCheckService`; speichert Zeitstempel des letzten Checks pro Anwendung in-memory; Cooldown konfigurierbar über `HealthCheck:CooldownSeconds`. |
| `WindowsCredentialService` | Klasse | Implementiert `ICredentialService` via Windows Credential Manager (DPAPI). |
| `StorageModeService` | Klasse | Implementiert `IStorageModeService`; Scoped DI (pro Blazor-Circuit). |
| `SignalRNotificationService` | Klasse | Implementiert `ISignalRNotificationService`; kommuniziert mit `EndpointHub`. |

### SignalR (`Schnittstellenzentrale`)

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `EndpointHub` | Klasse | SignalR-Hub für Live-Updates im Teammodus; Clients abonnieren einen Bereich (Anwendungs-ID oder Gruppe). |

### UI-Komponenten (`Schnittstellenzentrale`)

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `MainLayout` | Blazor-Komponente | Hauptlayout mit `StorageMode`-Umschalter. |
| `ApplicationGroupTree` | Blazor-Komponente | Baumansicht aller Gruppen und Anwendungen, standardmäßig zugeklappt. |
| `ApplicationCard` | Blazor-Komponente | Detailansicht einer Anwendung mit Aktionen (Swagger/OData-Import, Health-Check). |
| `EndpointList` | Blazor-Komponente | Listet Endpunkte einer Anwendung gruppiert in `EndpointGroup`s, standardmäßig zugeklappt. |
| `EndpointEditor` | Blazor-Komponente | Formular zum Anlegen/Bearbeiten eines Endpunkts (Methode, Pfad, Header, Query-Parameter, Body, Auth). |
| `EndpointExecutionPanel` | Blazor-Komponente | Führt einen Endpunkt aus, zeigt Request/Response an, triggert Health-Check bei Verbindungsfehler. |
| `SwaggerImportDialog` | Blazor-Komponente | Zeigt `ImportDiff`-Vorschau von Swagger-Änderungen; ermöglicht selektives Übernehmen. |
| `ODataImportDialog` | Blazor-Komponente | Analog zu `SwaggerImportDialog` für OData-`$metadata`. |
| `HealthCheckDialog` | Blazor-Komponente | Zeigt Health-Check-Ergebnis; bietet Option zum Entfernen der Anwendung. |
| `ConcurrencyWarningDialog` | Blazor-Komponente | Warnt bei Schreibkonflikt im Teammodus; Benutzer kann überschreiben (Force-Save) oder abbrechen. |

---

## Änderungen an bestehenden Klassen

Keine — es existiert kein Quellcode. Alle Klassen sind neu.

---

## Umsetzungsreihenfolge

Die Reihenfolge berücksichtigt Abhängigkeiten: Enums vor Datenmodellklassen, Interfaces vor Implementierungen, Datenbankschicht vor Services, Services vor UI.

1. **Solution und Projektstruktur anlegen** — Solution mit den vier Projekten `Schnittstellenzentrale`, `Schnittstellenzentrale.Core`, `Schnittstellenzentrale.Infrastructure`, `Schnittstellenzentrale.Tests` erstellen; Projektreferenzen und NuGet-Pakete (EF Core, Serilog, SignalR, Microsoft.OpenApi, Microsoft.OData.Edm) konfigurieren.
2. **Enums anlegen** (`Schnittstellenzentrale.Core`) — `StorageMode`, `HttpMethod`, `AuthenticationType`; Abhängigkeit: müssen vor den Datenmodellklassen existieren.
3. **Datenmodellklassen anlegen** (`Schnittstellenzentrale.Core`) — `ApplicationGroup`, `Application`, `EndpointGroup`, `Endpoint`, `EndpointHeader`, `EndpointQueryParameter`, `ImportDiff`; Reihenfolge: `ApplicationGroup` vor `Application`, `Application` vor `EndpointGroup`/`Endpoint`, `Endpoint` vor `EndpointHeader`/`EndpointQueryParameter`.
4. **Interfaces anlegen** (`Schnittstellenzentrale.Core`) — alle `I*`-Interfaces; Abhängigkeit: setzen Datenmodellklassen und Enums voraus.
5. **`AppDbContext` anlegen** (`Schnittstellenzentrale.Infrastructure`) — Entitätskonfigurationen, `[Timestamp]`/`RowVersion`-Felder für Optimistic Concurrency, Fluent API für Beziehungen.
6. **`DatabaseProviderFactory` anlegen** (`Schnittstellenzentrale.Infrastructure`) — Auswertung von `DatabaseProvider` aus `appsettings.json`, Registrierung des EF-Core-Providers.
7. **EF-Core-Migrationen erstellen** — initiale Migration für SQLite und SQL Server.
8. **`ApplicationRepository` und `EndpointRepository` implementieren** (`Schnittstellenzentrale.Infrastructure`) — CRUD-Methoden, StorageMode-Logik.
9. **`WindowsCredentialService` implementieren** (`Schnittstellenzentrale.Infrastructure`) — DPAPI-Zugriff via Windows Credential Manager.
10. **`HealthCheckService` implementieren** (`Schnittstellenzentrale.Infrastructure`) — HTTP-Erreichbarkeitsprüfung, In-Memory-Cooldown-Tracking.
11. **`EndpointExecutionService` implementieren** (`Schnittstellenzentrale.Infrastructure`) — `HttpClient`-Aufruf, Authentifizierungsstrategien je `AuthenticationType`, Impersonation via `WindowsIdentity.RunImpersonated`, Auslösung des Health-Checks bei Verbindungsfehlern; Abhängigkeit: `IHealthCheckService`, `ICredentialService`.
12. **`SwaggerImportService` implementieren** (`Schnittstellenzentrale.Infrastructure`) — OpenAPI-Deserialisierung, Diff-Berechnung, Rückgabe als `ImportDiff`.
13. **`ODataImportService` implementieren** (`Schnittstellenzentrale.Infrastructure`) — `$metadata`-Parsing, Diff-Berechnung analog zu `SwaggerImportService`.
14. **`StorageModeService` implementieren** (`Schnittstellenzentrale.Infrastructure`) — Scoped-Verwaltung des aktiven `StorageMode`.
15. **`EndpointHub` anlegen** (`Schnittstellenzentrale`) — SignalR-Hub, Gruppen-Abonnement-Mechanismus.
16. **`SignalRNotificationService` implementieren** (`Schnittstellenzentrale.Infrastructure`) — Broadcast von Änderungen über `EndpointHub` im Teammodus; Abhängigkeit: `EndpointHub` muss vorab existieren.
17. **Anwendungskonfiguration** (`Schnittstellenzentrale`) — `appsettings.json` mit `DatabaseProvider`, `ConnectionStrings:Default`, `Serilog`, `HealthCheck:CooldownSeconds`; IIS-Windows-Authentifizierung konfigurieren; Serilog registrieren; DI-Registrierungen in `Program.cs`.
18. **UI-Komponenten implementieren** (`Schnittstellenzentrale`) — in dieser Reihenfolge: `MainLayout` → `ApplicationGroupTree` → `ApplicationCard` → `EndpointList` → `EndpointEditor` → `EndpointExecutionPanel` → `SwaggerImportDialog` → `ODataImportDialog` → `HealthCheckDialog` → `ConcurrencyWarningDialog`; Abhängigkeit: alle Services müssen registriert sein.

---

## Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `Execute_WithAuthTypeNone_SendsRequestWithoutCredentials` | `EndpointExecutionServiceTests` | Kein Authentifizierungs-Header wird gesetzt bei `AuthenticationType.None`. |
| `Execute_WithAuthTypeBasic_SendsBasicAuthHeader` | `EndpointExecutionServiceTests` | Korrekter `Authorization: Basic`-Header wird aus gespeicherten Credentials gebildet. |
| `Execute_WithAuthTypeNegotiate_UsesNegotiateHandler` | `EndpointExecutionServiceTests` | `Negotiate`-Handler wird ausgewählt. |
| `Execute_WithAuthTypeBearerToken_SendsBearerHeader` | `EndpointExecutionServiceTests` | Korrekter `Authorization: Bearer`-Header wird aus gespeichertem Token gebildet. |
| `Execute_WithAuthTypeNegotiateWithImpersonation_RunsImpersonated` | `EndpointExecutionServiceTests` | `WindowsIdentity.RunImpersonated` wird aufgerufen. |
| `Execute_OnConnectionError_DoesNotCallHealthCheck` | `EndpointExecutionServiceTests` | Bei Verbindungsfehler wird `IHealthCheckService.CheckAsync` nicht aufgerufen (Verantwortung liegt in der UI). |
| `Import_NewSwaggerDefinition_ReturnsCorrectDiff` | `SwaggerImportServiceTests` | Neue Endpunkte werden korrekt als „neu" klassifiziert. |
| `Import_ChangedSwaggerOperation_ReturnsChangedInDiff` | `SwaggerImportServiceTests` | Geänderte Operationen werden korrekt als „geändert" klassifiziert. |
| `Import_RemovedSwaggerOperation_ReturnsRemovedInDiff` | `SwaggerImportServiceTests` | Entfernte Operationen werden korrekt als „entfernt" klassifiziert. |
| `Import_NewODataMetadata_ReturnsCorrectDiff` | `ODataImportServiceTests` | Neue OData-Endpunkte werden korrekt als „neu" klassifiziert. |
| `Import_ChangedODataMetadata_ReturnsChangedInDiff` | `ODataImportServiceTests` | Geänderte OData-Operationen werden korrekt als „geändert" klassifiziert. |
| `CheckAsync_WithinCooldown_DoesNotSendRequest` | `HealthCheckServiceTests` | Zweiter Check innerhalb des Cooldown-Fensters sendet keinen HTTP-Request. |
| `CheckAsync_AfterCooldownExpired_SendsRequest` | `HealthCheckServiceTests` | Nach Ablauf des Cooldowns wird ein neuer HTTP-Request gesendet. |
| `CheckAsync_UnreachableUrl_ReturnsFalse` | `HealthCheckServiceTests` | Nicht erreichbare URL führt zu negativem Ergebnis. |
| `CreateSqliteContext_ReturnsSqliteDbContext` | `DatabaseProviderFactoryTests` | `DatabaseProviderFactory` registriert SQLite-Provider korrekt. |
| `CreateSqlServerContext_ReturnsSqlServerDbContext` | `DatabaseProviderFactoryTests` | `DatabaseProviderFactory` registriert SQL Server-Provider korrekt. |
| `GetApplications_WithStorageModeUser_ReturnsOnlyUserData` | `ApplicationRepositoryIntegrationTests` | Im Benutzermodus werden nur benutzerspezifische Datensätze zurückgegeben (SQLite In-Memory). |
| `GetApplications_WithStorageModeTeam_ReturnsTeamData` | `ApplicationRepositoryIntegrationTests` | Im Teammodus werden globale Datensätze zurückgegeben (SQLite In-Memory). |
| `SaveEndpoint_ConcurrentWrite_DetectsConflict` | `EndpointRepositoryIntegrationTests` | Optimistic Concurrency-Konflikt wird bei gleichzeitigem Schreiben korrekt erkannt (SQLite In-Memory). |
| `CreateInMemoryDbContext` | `TestHelpers` | Stellt einen `AppDbContext` mit SQLite In-Memory-Provider für Integrationstests bereit. |

---

## Offene Punkte

1. **Datenbankmigration bei erstem Start:** Sollen EF-Core-Migrationen automatisch beim Anwendungsstart angewendet werden (`Database.Migrate()`), oder wird ein separater Migrationsprozess beim Deployment erwartet? Die Antwort beeinflusst den Startcode in `Program.cs`.

2. **Benutzerspezifische Speicherung:** Wird `WindowsIdentity.GetCurrent().Name` als Schlüssel für benutzerspezifische Daten im Repository verwendet, oder soll eine andere Identifikation genutzt werden? Beeinflusst Repository-Filterlogik.

3. **Credential Service – Scope:** Werden Credentials benutzerspezifisch (DPAPI Current User) oder maschinenübergreifend (DPAPI Local Machine) gespeichert? Gilt dies für alle `AuthenticationType`-Werte?

4. **Impersonation – Sicherheitskontext:** Läuft der IIS-Application-Pool unter einem Dienstkonto mit `SeImpersonatePrivilege`? Ohne diese Berechtigung kann `WindowsIdentity.RunImpersonated` nicht genutzt werden.

5. **SignalR-Granularität:** Auf welcher Ebene werden Änderungen gebroadcastet — pro Anwendung, pro Gruppe oder global? Beeinflusst die Hub-Gruppenstruktur in `EndpointHub` und `SignalRNotificationService`.

6. **OData-Endpunktgenerierung:** Welche OData-Operationstypen sollen aus `$metadata` importiert werden (EntitySets, Actions, Functions, alle)? Beeinflusst `ODataImportService`.

7. **SQLite-Dateipfad:** Wo soll die SQLite-Datenbankdatei abgelegt werden (Anwendungsverzeichnis, `App_Data`, konfigurierter Pfad)? Beeinflusst `ConnectionStrings:Default` und ggf. IIS-Berechtigungen.

8. **Mehrsprachigkeit:** Wird ausschließlich Deutsch verwendet, oder soll eine Lokalisierungsinfrastruktur (`IStringLocalizer`) vorbereitet werden? Beeinflusst alle UI-Komponenten.

9. **Blazor-Rendermode:** Wird ausschließlich Blazor Server verwendet (kein WebAssembly, kein Auto-Render-Mode)? Beeinflusst die SignalR-Verbindungsstrategie und die Lebensdauer der Scoped-Services.
