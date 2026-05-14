# Schnittstellenzentrale — Installation und Konfiguration

## Voraussetzungen

- Windows Server oder Windows-Arbeitsstation mit IIS
- .NET 9.0 Runtime (ASP.NET Core)
- Windows-Authentifizierung im IIS aktiviert (`Windows Authentication` Feature)
- Anonyme Authentifizierung im IIS deaktiviert
- SQLite (keine weitere Installation nötig) oder SQL Server

## Installationsschritte

1. Anwendung im IIS-Verzeichnis publizieren (`dotnet publish` oder Visual Studio Publish).
2. In IIS einen neuen Site oder Anwendungspool anlegen; .NET CLR-Version auf „Kein verwalteter Code" setzen (Kestrel), Framework auf `No Managed Code`.
3. Im IIS-Manager für die Site die **Windows-Authentifizierung aktivieren** und **Anonyme Authentifizierung deaktivieren**.
4. `appsettings.json` im Veröffentlichungsverzeichnis anpassen (siehe Konfiguration unten).
5. Sicherstellen, dass der Anwendungspool-Identität Schreibzugriff auf das Anwendungsverzeichnis hat (für SQLite-Datenbankdatei und Log-Verzeichnis `logs/`).
6. Anwendung starten; EF-Core-Migrationen müssen vorab manuell angewendet werden (siehe Hinweis).

> **Hinweis: Datenbankmigration**  
> Die Anwendung wendet Migrationen nicht automatisch beim Start an. Migrationen müssen manuell ausgeführt werden:  
> `dotnet ef database update --project Schnittstellenzentrale.Infrastructure --startup-project Schnittstellenzentrale`

## Konfiguration

Alle Einstellungen in `appsettings.json`:

| Parameter | Typ | Standardwert | Beschreibung |
|-----------|-----|--------------|--------------|
| `DatabaseProvider` | `string` | `"SQLite"` | Datenbankprovider: `SQLite` oder `SqlServer` |
| `ConnectionStrings:Default` | `string` | `"Data Source=schnittstellenzentrale.db"` | Verbindungszeichenfolge für den gewählten Provider |
| `HealthCheck:CooldownSeconds` | `int` | `60` | Mindestabstand in Sekunden zwischen zwei Health-Checks derselben Anwendung |
| `Serilog:MinimumLevel` | `string` | `"Information"` | Log-Level: `Verbose`, `Debug`, `Information`, `Warning`, `Error` |
| `Serilog:WriteTo[EventLog]:Args:source` | `string` | `"Schnittstellenzentrale"` | Quelle im Windows-Ereignisprotokoll |
| `Serilog:WriteTo[File]:Args:path` | `string` | `"logs/log-.txt"` | Pfad und Namensmuster der Log-Datei |
| `Serilog:WriteTo[File]:Args:retainedFileCountLimit` | `int` | `7` | Anzahl der aufzubewahrenden Tages-Log-Dateien |

**Beispiel `appsettings.json`:**

```json
{
  "DatabaseProvider": "SQLite",
  "ConnectionStrings": {
    "Default": "Data Source=schnittstellenzentrale.db"
  },
  "HealthCheck": {
    "CooldownSeconds": 60
  },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      {
        "Name": "EventLog",
        "Args": {
          "source": "Schnittstellenzentrale",
          "logName": "Application"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/log-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7
        }
      }
    ]
  },
  "AllowedHosts": "*"
}
```

**Beispiel für SQL Server:**

```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=Schnittstellenzentrale;Integrated Security=True;TrustServerCertificate=True"
  }
}
```

## Impersonation (NegotiateWithImpersonation)

Soll der Authentifizierungstyp `NegotiateWithImpersonation` verwendet werden, muss der IIS-Application-Pool unter einem Dienstkonto mit `SeImpersonatePrivilege` laufen. Standardmäßig besitzt `NetworkService` diese Berechtigung; benutzerdefinierte Dienstkonten müssen diese Berechtigung explizit erhalten.

## Überprüfung

1. Browser öffnen und zur Anwendungs-URL navigieren — bei korrekter Windows-Authentifizierung erscheint die Hauptseite ohne Login-Dialog.
2. Im Windows-Ereignisprotokoll unter „Anwendung" → Quelle „Schnittstellenzentrale" prüfen, ob Startmeldungen erscheinen.
3. Im Anwendungsverzeichnis unter `logs/` prüfen, ob Log-Dateien geschrieben werden.
4. Eine Testanwendung anlegen und einen Endpunkt ausführen.
