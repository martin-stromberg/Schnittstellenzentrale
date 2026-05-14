# Anforderung: Schnittstellenzentrale – Initiale Anwendung

## Fachliche Zusammenfassung

Die Anwendung „Schnittstellenzentrale" wird als neue Blazor Server App für den IIS-Betrieb im lokalen Netzwerk erstellt. Sie dient als zentrale Verwaltungsoberfläche für Webservice-Endpunkte der lokalen Servermaschine: Benutzer können Anwendungen mit ihren Endpunkten anlegen, gruppieren, konfigurieren und direkt aus der UI heraus ausführen. Die Datenhaltung erfolgt wahlweise in SQLite oder SQL Server; der Zugriff wird ausschließlich über Windows-Authentifizierung gesichert. Änderungen im Teammodus werden über SignalR in Echtzeit an alle betroffenen Sitzungen übertragen.

---

## Betroffene Klassen und Komponenten

### Datenmodellklassen (neu)

| Klasse | Beschreibung |
|---|---|
| `ApplicationGroup` | Gruppe, der Anwendungen zugeordnet werden können (optional). |
| `Application` | Webservice-Anwendung mit Name, Beschreibung, Basis-URL, optionaler Swagger-URL und optionaler `$metadata`-URL. Gehört zu maximal einer `ApplicationGroup`. |
| `EndpointGroup` | Untergruppe innerhalb einer Anwendung zur Organisation von Endpunkten. |
| `Endpoint` | HTTP-Endpunkt einer Anwendung mit Methode, relativem Pfad, Headern, Query-Parametern, Body und Authentifizierungstyp. Gehört zu einer `Application` und optional zu einer `EndpointGroup`. |
| `EndpointHeader` | Schlüssel-Wert-Paar für Header eines Endpunkts. |
| `EndpointQueryParameter` | Schlüssel-Wert-Paar für Query-Parameter eines Endpunkts. |
| `StorageMode` | Enum: `Team`, `User` — steuert den Speicherbereich (global vs. benutzerspezifisch). |
| `HttpMethod` | Enum: `GET`, `POST`, `PUT`, `DELETE`, `PATCH`, `HEAD`, `OPTIONS`. |
| `AuthenticationType` | Enum: `None`, `Basic`, `Negotiate`, `BearerToken`, `NegotiateWithImpersonation`. |

### Services / Logikklassen (neu)

| Klasse / Interface | Beschreibung |
|---|---|
| `IApplicationRepository` / `ApplicationRepository` | CRUD-Operationen für `Application` und `ApplicationGroup`, storageMode-bewusst. |
| `IEndpointRepository` / `EndpointRepository` | CRUD-Operationen für `Endpoint`, `EndpointGroup`, Header und Query-Parameter. |
| `IEndpointExecutionService` / `EndpointExecutionService` | Führt einen Endpunkt per `HttpClient` aus; wählt Authentifizierungsstrategie anhand `AuthenticationType`; löst bei Fehlern ggf. Health-Check aus. |
| `ISwaggerImportService` / `SwaggerImportService` | Ruft Swagger-Definition ab, erstellt/aktualisiert Endpunkte, liefert Diff-Vorschau. |
| `IODataImportService` / `ODataImportService` | Ruft `$metadata` ab, erstellt/aktualisiert Endpunkte analog zu `SwaggerImportService`. |
| `IHealthCheckService` / `HealthCheckService` | Prüft Erreichbarkeit einer Anwendung (Swagger-URL oder `$metadata`-URL); verhindert redundante Prüfungen durch Cooldown-Mechanismus. |
| `ICredentialService` / `WindowsCredentialService` | Liest und schreibt Passwörter/Tokens über den Windows Credential Manager (DPAPI / `CredentialCache`). |
| `IStorageModeService` / `StorageModeService` | Verwaltet den aktiven `StorageMode` pro Benutzer-Sitzung und löst Neuladen aus. |
| `ISignalRNotificationService` / `SignalRNotificationService` | Sendet Änderungsbenachrichtigungen im Teammodus an alle verbundenen Clients über einen SignalR-Hub. |
| `DatabaseProviderFactory` | Erstellt den korrekten EF-Core-`DbContext` abhängig vom konfigurierten Datenbanktyp (`SQLite` / `SqlServer`). |

### Interfaces

- `IApplicationRepository`
- `IEndpointRepository`
- `IEndpointExecutionService`
- `ISwaggerImportService`
- `IODataImportService`
- `IHealthCheckService`
- `ICredentialService`
- `IStorageModeService`
- `ISignalRNotificationService`

### SignalR

- `EndpointHub` — SignalR-Hub für Live-Updates im Teammodus; Clients abonnieren einen Bereich (z. B. Anwendungs-ID oder Gruppe).

### EF Core / Datenbankschicht

- `AppDbContext` — EF-Core-Kontext mit allen Entitäten; Provider wird per `DatabaseProviderFactory` injiziert.
- Migrationen für SQLite und SQL Server (getrennte Migrationsprojekte oder bedingtes Anwenden empfohlen).

### UI-Komponenten (Blazor)

| Komponente | Beschreibung |
|---|---|
| `MainLayout` | Hauptlayout mit Modus-Umschalter (`StorageMode`-Auswahlbox). |
| `ApplicationGroupTree` | Baumansicht aller Gruppen und Anwendungen, standardmäßig zugeklappt. |
| `ApplicationCard` | Detailansicht einer Anwendung mit Aktionen (Swagger/OData-Import, Health-Check). |
| `EndpointList` | Listet Endpunkte einer Anwendung gruppiert in `EndpointGroup`s, standardmäßig zugeklappt. |
| `EndpointEditor` | Formular zum Anlegen/Bearbeiten eines Endpunkts (Methode, Pfad, Header, Query-Parameter, Body, Auth). |
| `EndpointExecutionPanel` | Führt einen Endpunkt aus, zeigt Request/Response an, triggert Health-Check bei Verbindungsfehler. |
| `SwaggerImportDialog` | Zeigt Diff-Vorschau von Swagger-Änderungen; ermöglicht selektives Übernehmen. |
| `ODataImportDialog` | Analog zu `SwaggerImportDialog` für OData-`$metadata`. |
| `HealthCheckDialog` | Zeigt Health-Check-Ergebnis; bietet Option zum Entfernen der Anwendung. |
| `ConcurrencyWarningDialog` | Warnt bei Schreibkonflikt im Teammodus; Benutzer kann überschreiben oder abbrechen. |

### Tests (vorgesehen)

- Unit-Tests für `EndpointExecutionService` (Authentifizierungsstrategie-Auswahl, Fehlerbehandlung)
- Unit-Tests für `SwaggerImportService` / `ODataImportService` (Diff-Logik)
- Unit-Tests für `HealthCheckService` (Cooldown-Logik)
- Integrationstests für Repository-Schicht (SQLite In-Memory)

---

## Implementierungsansatz

### Projektstruktur

Empfohlen wird eine Solution mit folgenden Projekten:

- `Schnittstellenzentrale` — Blazor Server App (Einstiegspunkt, IIS-Hosting)
- `Schnittstellenzentrale.Core` — Domänenmodell, Interfaces, Enums
- `Schnittstellenzentrale.Infrastructure` — EF Core, Repositories, externe Dienste
- `Schnittstellenzentrale.Tests` — Unit- und Integrationstests

### Authentifizierung

Windows-Authentifizierung wird über `IIS Windows Authentication` + `Negotiate`-Middleware konfiguriert. Kein `[Authorize]`-Rollenfilter notwendig — jeder authentifizierte Benutzer erhält vollen Zugriff.

Für Impersonation wird `WindowsIdentity.RunImpersonated` verwendet, ausgelöst durch die Benutzerauswahl im `EndpointEditor`.

### Datenbankprovider-Auswahl

In `appsettings.json` wird ein Schlüssel `DatabaseProvider` mit den Werten `SQLite` oder `SqlServer` definiert. `DatabaseProviderFactory` liest diesen beim Start aus und registriert den passenden EF-Core-Provider. Ein Neustart ist erforderlich, da die DI-Registrierung nur einmalig beim Start erfolgt.

### Team-/Benutzermodus

`StorageModeService` hält den aktiven Modus pro Blazor-Circuit (Scoped DI). Beim Moduswechsel werden alle abhängigen Komponenten via `StateHasChanged` neu gerendert und Daten aus dem entsprechenden Repository neu geladen. Im `Team`-Modus werden Schreiboperationen nach dem Persistieren über `EndpointHub` an alle abonnierten Clients gebroadcastet.

### Concurrency

Da das Modell „last write wins" vorschreibt, wird kein pessimistisches Sperren implementiert. Ein `RowVersion`/`Timestamp`-Feld in den Entitäten ermöglicht das Erkennen von Konflikten (optimistic concurrency). Bei Konflikt wird `ConcurrencyWarningDialog` angezeigt; der Benutzer kann die Warnung ignorieren und explizit mit einem Force-Save überschreiben.

*Annahme: EF Core Optimistic Concurrency mit `[Timestamp]`-Attribut wird als Mechanismus verwendet.*

### Swagger- und OData-Import

`SwaggerImportService` nutzt `NSwag` oder `Microsoft.OpenApi` zur Deserialisierung der Swagger/OpenAPI-Definition. `ODataImportService` parst das XML-`$metadata`-Dokument via `Microsoft.OData.Edm`. Beide Services liefern ein `ImportDiff`-Objekt (neu, geändert, entfernt) für die Vorschau-Dialoge.

### Health-Check-Cooldown

`HealthCheckService` speichert den Zeitstempel des letzten Checks pro Anwendung (in-memory oder in der DB). Ein konfigurierbarer Schwellenwert (z. B. 60 Sekunden) verhindert wiederholte Checks in kurzer Zeit.

*Annahme: Cooldown-Dauer ist konfigurierbar über `appsettings.json`.*

### Logging

Serilog wird als Logging-Provider registriert (`UseSerilog()`). Konfiguration in `appsettings.json`:

```json
"Serilog": {
  "MinimumLevel": "Information",
  "WriteTo": [
    { "Name": "EventLog", "Args": { "source": "Schnittstellenzentrale", "logName": "Application" } },
    { "Name": "File", "Args": { "path": "logs/log-.txt", "rollingInterval": "Day", "retainedFileCountLimit": 7 } }
  ]
}
```

---

## Konfiguration

| Schlüssel | Ebene | Beschreibung |
|---|---|---|
| `DatabaseProvider` | `appsettings.json` | `SQLite` oder `SqlServer` |
| `ConnectionStrings:Default` | `appsettings.json` | Verbindungszeichenfolge für den gewählten Provider |
| `Serilog:MinimumLevel` | `appsettings.json` | Log-Level (Verbose, Debug, Information, Warning, Error) |
| `Serilog:WriteTo[File].Args.retainedFileCountLimit` | `appsettings.json` | Aufbewahrungsdauer der Log-Dateien (Standard: 7) |
| `HealthCheck:CooldownSeconds` | `appsettings.json` | Mindestabstand zwischen Health-Checks pro Anwendung *(Annahme)* |

Alle Konfigurationsänderungen erfordern einen Neustart der Anwendung.

---

## Offene Fragen

1. **Datenbankmigration bei erstem Start:** Sollen EF-Core-Migrationen automatisch beim Anwendungsstart angewendet werden (`Database.Migrate()`), oder wird ein separater Migrationsprozess beim Deployment erwartet?

2. **Benutzerspezifische Speicherung:** Wird der aktuelle Windows-Benutzername (`WindowsIdentity.GetCurrent().Name`) als Schlüssel für benutzerspezifische Daten verwendet, oder soll eine andere Identifikation genutzt werden?

3. **Credential Service – Scope:** Werden Credentials (Passwörter/Tokens) benutzerspezifisch (DPAPI Current User) oder maschinenübergreifend (DPAPI Local Machine) gespeichert? Gilt dies für alle Authentifizierungstypen?

4. **Impersonation – Sicherheitskontext:** Läuft der IIS-Application-Pool unter einem Dienstkonto? Falls ja, ist sicherzustellen, dass der Dienstkontext Impersonation erlaubt (`SeImpersonatePrivilege`).

5. **SignalR-Scope für Live-Updates:** Auf welcher Granularität werden Änderungen gebroadcastet — pro Anwendung, pro Gruppe oder global? Dies beeinflusst die Hub-Gruppenstruktur.

6. **OData-Endpunktgenerierung:** Welche OData-Operationen sollen aus `$metadata` als Endpunkte angelegt werden (EntitySets, Actions, Functions)? Sollen alle oder nur ausgewählte Typen importiert werden?

7. **Health-Check-Definition:** Was gilt als „kürzlich durchgeführt"? Ist ein konfigurierbarer Cooldown (z. B. 60 Sekunden) ausreichend, oder wird ein fester Wert bevorzugt?

8. **SQLite-Dateipfad:** Wo soll die SQLite-Datenbankdatei abgelegt werden (Anwendungsverzeichnis, `App_Data`, konfigurierter Pfad)?

9. **Mehrsprachigkeit:** Ist die Benutzeroberfläche ausschließlich auf Deutsch, oder soll eine Lokalisierungsinfrastruktur vorbereitet werden?

10. **Blazor-Rendermode:** Wird ausschließlich Blazor Server (kein WebAssembly, kein Auto-Render-Mode) verwendet? Dies ist relevant für die SignalR-Verbindungsstrategie.
