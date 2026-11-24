# Schnittstellenzentrale

[![Release](https://img.shields.io/github/actions/workflow/status/martin-stromberg/Schnittstellenzentrale/release.yml?label=Release)](https://github.com/martin-stromberg/Schnittstellenzentrale/actions/workflows/release.yml)
![.NET](https://img.shields.io/badge/.NET-11.0-512BD4?logo=dotnet&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-11-512BD4?logo=dotnet&logoColor=white)
![xUnit](https://img.shields.io/badge/Tests-xUnit%20%2B%20Playwright-green?logo=xunit&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows%20%2F%20IIS-0078D4?logo=windows&logoColor=white)

Blazor Server-Anwendung zur zentralen Verwaltung lokaler Webservice-Endpunkte. Sie ermöglicht das Anlegen, Gruppieren und direkte Ausführen von HTTP-Endpunkten mit verschiedenen Authentifizierungstypen, den Import aus Swagger/OpenAPI- und OData-Definitionen sowie Echtzeit-Synchronisation im Teammodus über SignalR.

## Features

- **Workspaces** – Anwendungen in Sammlungen organisieren, Endpunkte anlegen und direkt ausführen; laufende Anfragen zeigen sofort einen Status, und das letzte Ergebnis bleibt beim Wechsel zwischen Endpunkten pro Sitzung sichtbar
- **Environments** – Systemumgebungen mit Variablentabellen verwalten
- **History** – Vollständige Aufrufhistorie mit Zeitraumfilter und Top-5-Auswertung je Anwendung
- **Import** – Endpunkte aus Swagger/OpenAPI- oder OData-`$metadata`-Definitionen importieren (Diff-Vorschau mit selektiver Übernahme); der jeweilige Import-Button erscheint in der Anwendungsdetailansicht, sobald eine passende Interface-URL hinterlegt ist. Post-Request-Skripte verwenden `sz.execute(name)` mit Boolean-Rueckgabe; nach einem unmittelbar erfolgreichen Authenticate-Aufruf kann `sz.repeat()` den aktuellen Endpunkt erneut ausfuehren.
- **OData v4-API** – vollständiger OData v4-Service unter `/odatav4` mit CSDL-Metadaten-Dokument (`/odatav4/$metadata`); exponiert alle vier Kernobjekte als Entity-Sets mit CRUD-Zugriff, `$filter`, `$select`, `$expand`, `$orderby`, `$top` und `$skip`
  - **OData-Authentifizierung** – Tokens via `GET` oder `POST /odatav4/Authenticate()` (Windows/Negotiate)
  - **OData-Metadaten-Import** – Importiert Entity-Sets und Operationen aus fremden OData-Services; erzeugt je Entity-Set fünf Endpunkte (GET, POST, PUT/PATCH/DELETE mit `{key}`-Platzhalter); unterstützt proprietäre CSDL-Annotationen: `x-sz-bearer-token` (Token-Hinterlegung), `x-sz-auth-type` (Authentifizierungstyp), `x-sz-post-request-script` (Post-Request-Skript), `x-sz-header-{name}` (Custom-Header)
  - **API-Endpunkt für Import-Anwendung** – `POST /api/applications/{id}/odata-import/apply` bzw. `POST /api/applications/{id}/swagger-import/apply` wenden Import-Diffs auf Anwendungen an (mit Token-, Existenz-, Typ- und Aggregat-Validierung)
  - **Storage-Mode-Header** – `X-Storage-Mode` wird von OData-Controllern ausgewertet (`Team`/`User`) zur optionalen Datenbereichs-Filterung
- **Authentifizierungstypen** – None, Basic, Negotiate, BearerToken, NegotiateWithImpersonation (Credentials im Windows Credential Manager)
- **Team-/Benutzermodus** – Umschaltung zwischen globalem und benutzerspezifischem Speicherbereich; die gewählte Einstellung wird im `localStorage` des Browsers gespeichert und beim nächsten Seitenaufruf automatisch wiederhergestellt
- **Echtzeit-Synchronisation** – SignalR-basiertes Broadcasting von Änderungen an alle verbundenen Sitzungen
- **Icons und Links** – Sammlungen und Anwendungen können mit Icon (PNG/JPEG) und URL-Links versehen werden
- **Health-Check** – Erreichbarkeit einer Anwendungs-URL prüfen (mit konfigurierbarem Cooldown)
- **Mehrsprachigkeit** – Benutzeroberfläche auf Deutsch und Englisch; Sprachauswahl automatisch über den `Accept-Language`-Header des Browsers (Englisch ist Standard und Fallback)
- **Impressum** – Optionale Impressumsseite unter `/impressum`; Inhalt wird aus einer Markdown-Datei (`impressum.md`) im Programmverzeichnis eingelesen und als HTML gerendert. Existiert die Datei nicht, bleibt der Navigationseintrag ausgeblendet — aktivierbar ohne Neustart durch einfaches Ablegen der Datei

## Voraussetzungen

- Windows Server oder Windows-Arbeitsstation mit IIS
- .NET 11.0 Runtime (ASP.NET Core)
- IIS: Windows-Authentifizierung aktiviert, Anonyme Authentifizierung deaktiviert
- SQLite (keine weitere Installation) oder SQL Server

## Installation

1. Bevorzugt das ZIP-Asset `Schnittstellenzentrale-vX.Y.Z.zip` aus dem passenden GitHub Release herunterladen und in das IIS-Verzeichnis entpacken. Alternativ Anwendung selbst publizieren:
   ```
   dotnet publish src/Schnittstellenzentrale/Schnittstellenzentrale.csproj -c Release -o publish/
   ```
2. IIS-Site anlegen; Anwendungspool auf `.NET CLR-Version: Kein verwalteter Code` setzen.
3. In IIS Windows-Authentifizierung aktivieren und Anonyme Authentifizierung deaktivieren.
4. `appsettings.json` im Veröffentlichungsverzeichnis anpassen (siehe Konfiguration).
5. Schreibzugriff des Anwendungspools auf das Veröffentlichungsverzeichnis sicherstellen (SQLite-Datei, `logs/`).
6. Anwendung starten — EF-Core-Migrationen werden automatisch beim Start angewendet (`Database.MigrateAsync`).

## Konfiguration

Alle Einstellungen in `appsettings.json`:

| Parameter | Typ | Standardwert | Beschreibung |
|-----------|-----|--------------|--------------|
| `DatabaseProvider` | `string` | `"SQLite"` | `SQLite` oder `SqlServer` |
| `ConnectionStrings:Default` | `string` | `"Data Source=schnittstellenzentrale.db"` | Verbindungszeichenfolge |
| `HealthCheck:CooldownSeconds` | `int` | `60` | Mindestabstand in Sekunden zwischen zwei Health-Checks derselben Anwendung |
| `Upload:MaxIconSizeBytes` | `int` | `524288` | Maximale Icon-Dateigröße in Bytes (512 KB) |
| `History:DefaultPageSize` | `int` | `50` | Einträge pro Seite in der Aufrufhistorie |
| `Serilog:MinimumLevel` | `string` | `"Information"` | Log-Level: `Verbose`, `Debug`, `Information`, `Warning`, `Error` |
| `Impressum:FilePath` | `string` | `""` (leer) | Pfad zur Impressum-Markdown-Datei; leer = `AppContext.BaseDirectory/impressum.md`. Relativer Pfad wird relativ zu `AppContext.BaseDirectory` aufgelöst, absoluter Pfad direkt verwendet. |

**Beispiel (SQLite):**

```json
{
  "DatabaseProvider": "SQLite",
  "ConnectionStrings": {
    "Default": "Data Source=schnittstellenzentrale.db"
  },
  "HealthCheck": { "CooldownSeconds": 60 },
  "Upload": { "MaxIconSizeBytes": 524288 },
  "History": { "DefaultPageSize": 50 },
  "Serilog": {
    "MinimumLevel": "Information",
    "WriteTo": [
      { "Name": "EventLog", "Args": { "source": "Schnittstellenzentrale", "logName": "Application" } },
      { "Name": "File", "Args": { "path": "logs/log-.txt", "rollingInterval": "Day", "retainedFileCountLimit": 7 } }
    ]
  },
  "Impressum": {
    "FilePath": ""
  }
}
```

**SQL Server:**

```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=Schnittstellenzentrale;Integrated Security=True;TrustServerCertificate=True"
  }
}
```

## Releases

GitHub Releases enthalten das veröffentlichte ZIP-Asset `Schnittstellenzentrale-vX.Y.Z.zip`. Releases entstehen automatisch bei Push nach `main`, wenn Conventional Commits eine neue Version ergeben, oder manuell durch einen Tag im Format `vX.Y.Z`.

## API

Die Schnittstellenzentrale stellt zwei API-Oberflächen bereit:

- **REST-API** unter `/api` — Token über `POST /authenticate`
- **OData v4-API** unter `/odatav4` — Token über `GET /odatav4/authenticate` oder `POST /odatav4/authenticate`

Die Authentifizierung erfolgt mittels Windows-Authentifizierung (Negotiate), die ein kurzlebiges Bearer-Token liefert. Dieses Token wird bei jedem Folgeaufruf im `Authorization`-Header übergeben und nach jeder erfolgreichen Anfrage rotiert (`X-New-Token`-Response-Header).

### REST-API (`/api`)

| Ressource | Beschreibung |
|-----------|--------------|
| `POST /authenticate` | Bearer-Token beziehen (Windows/Negotiate) |
| `/api/application-groups` | CRUD für `ApplicationGroup`; `DELETE` unterstützt `?content=delete` (atomar mit Inhalt) bzw. `?content=detach` (Standard: Anwendungen werden ungruppiert) |
| `/api/applications` | CRUD für `Application` |
| `POST /api/applications/{id}/import` | Import-Diff berechnen (Swagger oder OData, je nach `InterfaceType`) |
| `POST /api/applications/{id}/odata-import/apply` | OData-Import-Diff auf Anwendung anwenden |
| `POST /api/applications/{id}/swagger-import/apply` | Swagger-Import-Diff auf Anwendung anwenden |
| `/api/endpoint-groups` | CRUD für `EndpointGroup` |
| `/api/endpoints` | CRUD für `Endpoint` inkl. Header- und Query-Parameter-Routen |

Alle Endpunkte erfordern Bearer-Token im `Authorization`-Header. Unterstützt `X-Storage-Mode` (`Team`/`User`) und `X-Owner` für benutzermodus-abhängige Filterung. Mutierende Zugriffe lösen im Team-Modus über die zentrale `INotificationPolicy` jeweils genau eine SignalR-Benachrichtigung an alle verbundenen Clients aus — einheitlich für REST, OData, Import-Apply und den Startabgleich.

### OData v4-API (`/odatav4`)

| Ressource | Beschreibung |
|-----------|--------------|
| `GET /odatav4/Authenticate()`<br/>`POST /odatav4/Authenticate()` | Bearer-Token beziehen (Windows/Negotiate) |
| `GET /odatav4/$metadata` | CSDL-Metadaten-Dokument (öffentlich zugänglich) |
| `GET/POST /odatav4/Applications`<br/>`GET/PUT/PATCH/DELETE /odatav4/Applications({id})` | CRUD für `Application` |
| `GET/POST /odatav4/ApplicationGroups`<br/>`GET/PUT/PATCH/DELETE /odatav4/ApplicationGroups({id})` | CRUD für `ApplicationGroup` |
| `GET/POST /odatav4/Endpoints`<br/>`GET/PUT/PATCH/DELETE /odatav4/Endpoints({id})` | CRUD für `Endpoint` |
| `GET/POST /odatav4/EndpointGroups`<br/>`GET/PUT/PATCH/DELETE /odatav4/EndpointGroups({id})` | CRUD für `EndpointGroup` |

**Authentifizierung & Token:** Alle Daten-Endpunkte erfordern Bearer-Token im `Authorization`-Header. Token können via `GET /odatav4/Authenticate()` oder `POST /odatav4/Authenticate()` (Windows/Negotiate) bezogen werden. Nach jeder erfolgreichen Anfrage wird ein neuer Token im `X-New-Token`-Response-Header bereitgestellt.

**Abfrageoptionen:** Auf Collection-Endpunkten werden `$filter`, `$select`, `$expand`, `$orderby`, `$top`, `$skip` und `$count` unterstützt.

**Storage-Mode-Filter:** Der `X-Storage-Mode`-Header (`Team` oder `User`, Standard: `User`) filtert die Datensätze nach Speichermodus-Zugehörigkeit. Wird nicht gesetzt, liefert die API standardmäßig Benutzer-Datensätze.

**Besonderheiten:** `Id` und `RowVersion` sind bei POST/PUT/PATCH nicht schreibbar. Systemeinträge (`IsSystem = true`) sind vor Änderungen und Löschungen geschützt (403). PATCH-Anfragen akzeptieren nur die zu ändernden Felder; `null`-Werte setzen Felder auf ihren leeren Zustand.

**OData-Metadaten-Import:** Das unter `/odatav4/$metadata` exponierte CSDL-Dokument wird auch intern für den OData-Import verwendet. Externe OData-Services können importiert werden. Der Import erzeugt je Entity-Set fünf Endpunkte (GET, POST, PUT/PATCH/DELETE mit `{key}`-Platzhalter) sowie einen Endpunkt je OData-Aktion und -Funktion. Unterstützte CSDL-Annotationen: `x-sz-bearer-token` (Token-Hinterlegung), `x-sz-auth-type` (Authentifizierungstyp), `x-sz-post-request-script` (Post-Request-Skript), `x-sz-header-{name}` (Custom-Header). In Post-Request-Skripten liefert `sz.execute(name)` einen Boolean; nach einem unmittelbar erfolgreichen Authenticate-Aufruf kann `sz.repeat()` den aktuellen Endpunkt erneut ausfuehren. Für Authenticate-Endpunkte selbst gilt diese Wiederholungslogik nicht.

Ausführliche Dokumentation: [docs/help/api/odata-api.md](docs/help/api/odata-api.md)

## Projektstruktur

```
src/
├── Schnittstellenzentrale/          # Blazor Server App (Einstiegspunkt)
│   ├── Components/                  # Razor-Komponenten (Pages, Layout, Shared)
│   ├── wwwroot/                     # Statische Assets, CSS (sz-*-System)
│   └── Program.cs
├── Schnittstellenzentrale.Core/     # Domänenmodell, Interfaces, Services
├── Schnittstellenzentrale.Infrastructure/  # EF Core, Repositories, Migrationen
└── Schnittstellenzentrale.Tests/    # Unit-, Integrations- und Playwright-E2E-Tests
```

**Technologien:** ASP.NET Core 11 · Blazor Server · Entity Framework Core 11 · ShadcnBlazor · SignalR · Microsoft.AspNetCore.OData 10 Preview · Serilog · Markdig · xUnit · Playwright

## Tests

**Unit- und Integrationstests** (reguläre Lane, deterministisch — ohne Browser):

```
dotnet test -- --filter "Category!=OsInterface"
```

**OS-Schnittstellen-Tests** (Playwright-E2E, echte Kestrel-Bindung, Temp-Dateisystem — Chromium wird beim ersten Build automatisch heruntergeladen; in der CI best-effort als eigener Job `os-interface-tests`):

```
dotnet test -- --filter "Category=OsInterface"
```

Das Testprojekt läuft auf xunit.v3 und `dotnet test` im Microsoft.Testing.Platform-Modus (Opt-in via `test.runner` in `global.json`): `--filter` bleibt VSTest-Syntax, wird aber als Test-App-Argument hinter `--` übergeben.

**Coverage-Lauf** (lokale Abfrage der Line-Coverage der regulären Lane — die OS-Lane sammelt in der CI keine Coverage):

```
dotnet test --solution Schnittstellenzentrale.slnx -- --filter "Category!=OsInterface" --coverage --coverage-output-format cobertura
```

Beim ersten Build nach dem Klonen lädt das MSBuild-Target `InstallPlaywright` den Chromium-Browser herunter. Zum Überspringen in CI-Umgebungen:

```
dotnet test -p:SkipPlaywrightInstall=true
```

Trace-Dateien der Playwright-Tests werden nach jedem Lauf unter `playwright-traces/` im Repo-Root abgelegt (auch bei Erfolg; Dateinamen `{TestName}-HHmmssfff.zip` mit UTC-Zeitstempel-Suffix). Einsehen mit:

```
npx playwright show-trace playwright-traces/ApplicationCrudTests-101530123.zip
```

Das Repository nutzt ein lokales Qualitätsgate für 70 % Line Coverage in der regulären Lane; die bUnit-Tests werden gezielt erweitert, damit der CI-Lauf stabil durchläuft. Abgedeckte E2E-Szenarien: Startseite, Anwendungs-CRUD, Endpunkt-Ausführung, Swagger-Import, OData-Import, Health-Check, Speichermoduswechsel, SignalR-Echtzeitsynchronisation, Aktivitätsprotokoll.

## Git-Hooks

Das Repository enthält versionierte Git-Hooks im Verzeichnis `.githooks/` (Voraussetzung: Python 3 und das .NET SDK). Einmalig pro Klon aktivieren:

```
.githooks\install-hooks.cmd    # Windows
./.githooks/install-hooks.sh   # Linux/macOS
```

Das Skript setzt `core.hooksPath` auf `.githooks`. Alternativ manuell: `git config --local core.hooksPath .githooks`.

**pre-commit** blockiert direkte Commits auf `main`/`staging` und prüft u. a. resx-Konsistenz und fehlende Übersetzungsschlüssel, XML-Dokumentation (`GenerateDocumentationFile`, CS1591 als Fehler, keine `#pragma warning disable` für XML-Doc-Codes, vollständige `<param>`/`<returns>`-Tags) sowie hartcodierte UI-Strings in Razor-Dateien. Weitere Checks (verwaiste Komponenten, Throw-Stubs, Enum-Testabdeckung) warnen beim Commit.

**pre-push** blockiert direkte Pushes auf `main`/`staging` und prüft das gesamte Repository streng (`--all --strict`): keine `NotImplementedException`/Throw-only-Stubs, keine verwaisten Razor-Komponenten, alle `public`/`internal`-Enum-Werte müssen in Tests vorkommen.

## Automatisierte Review-Workflows

Drei zeitgesteuerte GitHub-Actions-Workflows unter `.github/workflows/` erzeugen automatisch Review-Issues mit einer `@softwarewerkstatt`-Erwähnung als Bearbeitungsauftrag:

- **Monatliches Architektur- und Sicherheitsreview** (`monthly-architecture-security-review.yml`, 1. des Monats)
- **Wöchentliche NuGet-Paketprüfung** (`weekly-nuget-review.yml`, montags)
- **Jährliches Compliance- und Copyright-Review** (`yearly-compliance-review.yml`, 1. Januar)

Jeder Workflow prüft vor dem Anlegen über ein dediziertes Label, ob bereits ein offenes Issue desselben Typs existiert, und verzichtet in diesem Fall auf ein Duplikat. Alle drei Workflows können zusätzlich manuell über `workflow_dispatch` ausgelöst werden.

Details: [docs/help/review-automatisierung/index.md](docs/help/review-automatisierung/index.md)

## GitHub Page

Die öffentliche Präsentationsseite der Anwendung ist unter https://martin-stromberg.github.io/Schnittstellenzentrale/ erreichbar. Ihre Quellen liegen im Ordner `site/` (reines HTML/CSS/JS ohne Build-Tool, eingecheckte Screenshots); das Deployment übernimmt der Workflow `.github/workflows/pages.yml` bei Push auf `staging`/`main` mit Änderungen an den Seitenquellen oder manuell per `workflow_dispatch`. Details zum Pflege- und Aktualisierungsweg: [docs/help/github-page/index.md](docs/help/github-page/index.md).

## Rechtliches

Rechtliche Hinweise und die Lizenzbeschreibung liegen in [LEGAL.md](LEGAL.md). Im Repository-Stamm ist keine separate `LICENSE`-Datei hinterlegt.
