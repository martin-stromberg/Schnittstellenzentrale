# Logik

## `Program` (Middleware-Konfiguration)
Datei: `src/Schnittstellenzentrale/Program.cs`

Relevante Methode: `BuildWebApplicationAsync`

| Abschnitt | Zustand |
|---|---|
| `AddLocalization()` | Nicht vorhanden |
| `AddDataAnnotationsLocalization()` | Nicht vorhanden |
| `UseRequestLocalization()` | Nicht vorhanden |
| `AcceptLanguageHeaderRequestCultureProvider` | Nicht konfiguriert |

Der Middleware-Stack enthält aktuell: `UseAuthentication`, `UseAuthorization`, `UseAntiforgery`, `UseStaticFiles`/`MapStaticAssets`, `MapControllers`, `MapHub`, `MapRazorComponents`. Es fehlen die Lokalisierungs-Middleware und -Services.

---

## Razor-Komponenten mit hartcodierten UI-Texten

### `ApplicationEditor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationEditor.razor`

Hartcodierte Texte (deutsch):
- Titel: `"Neue Anwendung"`, `"Anwendung bearbeiten"`, `"Anwendung anlegen"`
- Labels: `"Name"`, `"Basis-URL"`, `"Beschreibung"`, `"Schnittstellen-URL"`, `"Sammlung"`
- Option: `"Ohne Sammlung"`
- Hints: `"REST (Swagger/OpenAPI)"`, `"OData"`, `"Typ nicht erkannt"`
- Buttons: `"Speichern"`, `"Abbrechen"`
- Fehlermeldung (Nutzer sichtbar): `$"Fehler beim Laden der Gruppen: {ex.Message}"`, `$"Speichern fehlgeschlagen: {ex.Message}"`

### `ApplicationGroupEditor`
Datei: `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupEditor.razor`

Hartcodierte Texte (deutsch):
- Titel: `"Neue Sammlung"`, `"Sammlung anlegen"`
- Label: `"Name"`
- Buttons: `"Speichern"`, `"Abbrechen"`
- Fehlermeldung (Nutzer sichtbar): `$"Speichern fehlgeschlagen: {ex.Message}"`

### `ConfirmDeleteGroupDialog`
Datei: `src/Schnittstellenzentrale/Components/Shared/ConfirmDeleteGroupDialog.razor`

Hartcodierte Texte (deutsch):
- Dialog-Titel: `"Sammlung löschen"`
- Nachricht: `"Sammlung <strong>@Group.Name</strong> wirklich löschen?"`, `"Die Sammlung enthält <strong>@Group.Applications.Count</strong> Anwendung(en)."`
- Buttons: `"Mitlöschen"`, `"Nur Sammlung löschen"`, `"Abbrechen"`

### `ConfirmDeleteApplicationDialog`
Datei: `src/Schnittstellenzentrale/Components/Shared/ConfirmDeleteApplicationDialog.razor`

Hartcodierte Texte (deutsch):
- Dialog-Titel: `"Anwendung löschen"`
- Nachricht: `"Anwendung <strong>@Application.Name</strong> wirklich löschen?"`
- Buttons: `"Löschen"`, `"Abbrechen"`

### `ConfirmDeleteEndpointGroupDialog`
Datei: `src/Schnittstellenzentrale/Components/Shared/ConfirmDeleteEndpointGroupDialog.razor`

Hartcodierte Texte (deutsch):
- Dialog-Titel: `"Ordner löschen"`
- Nachricht: `"Ordner <strong>@Group.Name</strong> wirklich löschen?"`, Warnung mit Endpunkt-Anzahl
- Buttons: `"Löschen"`, `"Abbrechen"`

### `RenameGroupDialog`
Datei: `src/Schnittstellenzentrale/Components/Shared/RenameGroupDialog.razor`

Hartcodierte Texte (deutsch):
- Dialog-Titel: `"Sammlung umbenennen"`
- Label: `"Name"`
- Buttons: `"Speichern"`, `"Abbrechen"`
- Fehlermeldung (Nutzer sichtbar): `$"Speichern fehlgeschlagen: {ex.Message}"`

### `RenameEndpointGroupDialog`
Datei: `src/Schnittstellenzentrale/Components/Shared/RenameEndpointGroupDialog.razor`

Hartcodierte Texte (deutsch):
- Dialog-Titel: `"Ordner umbenennen"`
- Label: `"Name"`
- Buttons: `"Speichern"`, `"Abbrechen"`
- Inline-Validierung (Nutzer sichtbar): `"Der Name darf nicht leer sein."`
- Fehlermeldung (Nutzer sichtbar): `$"Speichern fehlgeschlagen: {ex.Message}"`

### `CreateEndpointGroupDialog`
Datei: `src/Schnittstellenzentrale/Components/Shared/CreateEndpointGroupDialog.razor`

Hartcodierte Texte (deutsch):
- Dialog-Titel: `"Ordner anlegen"`
- Label: `"Name"`
- Buttons: `"Anlegen"`, `"Abbrechen"`
- Inline-Validierung (Nutzer sichtbar): `"Der Name darf nicht leer sein."`
- Fehlermeldung (Nutzer sichtbar): `$"Anlegen fehlgeschlagen: {ex.Message}"`

### `ConcurrencyWarningDialog`
Datei: `src/Schnittstellenzentrale/Components/Shared/ConcurrencyWarningDialog.razor`

Hartcodierte Texte (deutsch):
- Dialog-Titel: `"Schreibkonflikt erkannt"`
- Nachricht: `"Die Daten wurden zwischenzeitlich von einem anderen Benutzer geändert. Möchten Sie Ihre Änderungen trotzdem speichern und die anderen Änderungen überschreiben?"`
- Buttons: `"Überschreiben"`, `"Abbrechen"`

### `EndpointPage`
Datei: `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor`

Hartcodierte Texte (deutsch):
- Badge: `"geändert"`
- Buttons: `"Speichern"`, `"Anfrage senden"`
- Inline-Validierung (Nutzer sichtbar): `"Der Name darf nicht leer sein."`
- Tabs (Request): `"Autorisierung"`, `"Headers"`, `"Query-Parameter"`, `"Body"`, `"Pre-Request-Skript"`, `"Post-Request-Skript"`
- Response-Bereich: `"Antwort"`, `"Status:"`, `"Dauer:"`, `"Größe:"`, `" Bytes"`
- Tabs (Response): `"Body"`, `"Headers"`
- Placeholders: `"Endpunktname"`, `"Relativer Pfad"`
- `confirm()`-Dialog (Nutzer sichtbar): `"Es gibt ungespeicherte Änderungen. Trotzdem verlassen?"`
- Fehlermeldung (Nutzer sichtbar): `"Endpunkt konnte nicht geladen werden."`, `$"Speichern fehlgeschlagen: {ex.Message}"`

### `ImportDialog`
Datei: `src/Schnittstellenzentrale/Components/Shared/ImportDialog.razor`

Hartcodierte Texte (deutsch):
- Status-Texte: `"Keine Änderungen gefunden."`, `"Neue Endpunkte (@Diff.NewEndpoints.Count)"`, `"Geänderte Endpunkte (@Diff.ChangedEndpoints.Count)"`, `"Entfernte Endpunkte (@Diff.RemovedEndpoints.Count)"`
- Buttons: `"Übernehmen"`, `"Abbrechen"`
- Fehlermeldung (Nutzer sichtbar): `"Übernahme fehlgeschlagen: Ein anderer Benutzer hat die Daten gleichzeitig geändert. Bitte laden Sie die Seite neu."`, `$"Übernahme fehlgeschlagen: {ex.Message}"`

### `HealthCheckDialog`
Datei: `src/Schnittstellenzentrale/Components/Shared/HealthCheckDialog.razor`

Hartcodierte Texte (deutsch):
- Status-Meldungen: `"Health-Check wurde übersprungen (Cooldown aktiv)."`, `"Die Anwendung ist erreichbar."`, `"Die Anwendung ist nicht erreichbar."`
- Buttons: `"Anwendung entfernen"`, `"Schließen"`

### `EnvironmentEditor`
Datei: `src/Schnittstellenzentrale/Components/Shared/EnvironmentEditor.razor`

Hartcodierte Texte (deutsch):
- Sektion: `"Variablen"`
- Tabellen-Header: `"Name"`, `"Wert"`
- Leerzustand: `"Keine Variablen definiert."`
- Tooltips: `"Wert anzeigen"`, `"Wert maskieren"`
- Button: `"+ Variable hinzufügen"`
- Buttons: `"Speichern"`, `"Abbrechen"`
- Inline-Validierung (Nutzer sichtbar): `"Name darf nicht leer sein."`, `"Name darf maximal 200 Zeichen lang sein."`, div. Variablen-Fehlermeldungen, `"Eine Umgebung mit diesem Namen existiert bereits."`
- Fehlermeldung (Nutzer sichtbar): `$"Speichern fehlgeschlagen: {ex.Message}"`

### `EnvironmentManagementOverlay`
Datei: `src/Schnittstellenzentrale/Components/Shared/EnvironmentManagementOverlay.razor`

Hartcodierte Texte (deutsch):
- Titel: `"Systemumgebungen verwalten"`
- Nachricht: `"Umgebung '<strong>@_confirmDeleteEnvironment.Name</strong>' wirklich löschen?"`
- Buttons: `"Löschen"`, `"Abbrechen"`, `"Neu"`, `"Bearbeiten"`, `"Löschen"`
- Leerzustand: `"Keine Umgebungen vorhanden."`
- Fehlermeldung (Nutzer sichtbar): `$"Löschen fehlgeschlagen: {ex.Message}"`

### `WorkspacesSidebar`
Datei: `src/Schnittstellenzentrale/Components/Shared/WorkspacesSidebar.razor`

Hartcodierte Texte (deutsch):
- Buttons: `"+ Neue Sammlung"`, `"+ Neue Anwendung"`
- Footer-Link: `"Impressum"`

### `EnvironmentsSidebar`
Datei: `src/Schnittstellenzentrale/Components/Shared/EnvironmentsSidebar.razor`

Hartcodierte Texte (deutsch):
- Button: `"+ Neue Umgebung"`
- Tooltip: `"Löschen"`
- Placeholder: `"Name der Umgebung"`
- Buttons: `"Anlegen"`, `"Abbrechen"`

### `TopBar`
Datei: `src/Schnittstellenzentrale/Components/Layout/TopBar.razor`

Hartcodierte Texte (deutsch/gemischt):
- Label: `"Modus:"`
- Optionen: `"Team"`, `"Benutzer"`
- Tabs: `"Workspaces"`, `"Environments"`, `"History"` (englisch)
- Links: Titel `"Einstellungen"`, `"Hilfe"` (per `title`-Attribut)

### `AppShell`
Datei: `src/Schnittstellenzentrale/Components/Layout/AppShell.razor`

Hartcodierte Texte (deutsch):
- Fehlermeldung: `"Ein unbehandelter Fehler ist aufgetreten."`
- Link: `"Neu laden"`

### `LinksManager`
Datei: `src/Schnittstellenzentrale/Components/Shared/LinksManager.razor`

Hartcodierte Texte (deutsch):
- Sektion: `"Verwaltbare Links"`
- Button: `"+ Link hinzufügen"`
- Placeholders: `"URL (https://...)"`, `"Beschriftung (optional, max. 200 Zeichen)"`, `"Beschriftung (optional)"`
- Buttons: `"Speichern"`, `"Abbrechen"`

### `RequestAuthPanel`
Datei: `src/Schnittstellenzentrale/Components/Shared/RequestAuthPanel.razor`

Hartcodierte Texte (deutsch):
- Label: `"Authentifizierungstyp"`, `"Benutzername:Passwort"`, `"Bearer-Token"`
- Hints: `"Wird im Windows Credential Manager gespeichert."`
- Placeholders: `"benutzer:passwort"`, `"Token"`

### `EmptyContentView`
Datei: `src/Schnittstellenzentrale/Components/Shared/EmptyContentView.razor`

Hartcodierter Text (deutsch):
- `"Wählen Sie eine Sammlung oder Anwendung aus."`
