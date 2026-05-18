# Anforderung: Systemeintrag für eigene API beim Programmstart

## Fachliche Zusammenfassung

Die Entitäten `ApplicationGroup` und `Application` werden um ein neues boole'sches Datenbankfeld `IsSystem` (Default `false`) erweitert. Beim Programmstart im Shared-Modus wird nach der Datenbankinitialisierung automatisch geprüft, ob eine Gruppe und eine Anwendung mit dem Namen „Schnittstellenzentrale" und `IsSystem = true` existieren; fehlende Einträge werden angelegt, abweichende URL-Felder werden aktualisiert. Systemeinträge sind über die API und per Drag & Drop nicht löschbar und nicht verschiebbar; das Feld `IsSystem` ist weder über Benutzer-DTOs setzbar noch über die UI zugänglich.

---

## Betroffene Klassen und Komponenten

### Datenmodell — zu erweitern (`Schnittstellenzentrale.Core`)

| Klasse | Änderung |
|---|---|
| `ApplicationGroup` | Neues Feld `IsSystem` (`bool`, Default `false`). |
| `Application` | Neues Feld `IsSystem` (`bool`, Default `false`). |

### Datenbankschicht — zu erweitern (`Schnittstellenzentrale.Infrastructure`)

| Artefakt | Änderung |
|---|---|
| `AppDbContext.OnModelCreating` | `IsSystem` für `ApplicationGroup` und `Application` konfigurieren (kein `IsRequired`, DB-Default `false`). |
| EF-Core-Migration | Neue Migration für `IsSystem`-Spalten auf beiden Tabellen. |

### Startup-Routine — neu zu erstellen (`Schnittstellenzentrale`)

| Artefakt | Beschreibung |
|---|---|
| `SystemEntryInitializer` (neue statische Klasse oder Service) | Enthält `InitializeAsync(IServiceProvider services, IConfiguration configuration)`. Wird in `Program.cs` nach `EnsureDatabaseInitializedAsync()` aufgerufen. Legt Gruppe und Anwendung im Shared-Modus an oder aktualisiert URL-Felder. Fehler werden per Serilog geloggt; Ausnahmen werden abgefangen, damit der Programmstart nicht blockiert wird. |

### DI / Startup (`Schnittstellenzentrale`)

| Artefakt | Änderung |
|---|---|
| `Program.cs` | Aufruf von `SystemEntryInitializer.InitializeAsync` nach `EnsureDatabaseInitializedAsync`. Zugriff auf `Api:BaseUrl` aus `IConfiguration`. |

### API-Schicht — zu erweitern (`Schnittstellenzentrale`)

| Artefakt | Änderung |
|---|---|
| `ApplicationGroupsController.DeleteAsync` | Prüft `group.IsSystem`; gibt `403 Forbidden` zurück, wenn `true`. |
| `ApplicationsController.DeleteAsync` | Prüft `application.IsSystem`; gibt `403 Forbidden` zurück, wenn `true`. |
| `ApplicationGroupsController.UpdateAsync` | Optional: `IsSystem`-Einträge für Namens-/URL-Änderungen durch Benutzer sperren (`403`). |
| `ApplicationsController.UpdateAsync` | Optional: analog. |

### DTOs — zu prüfen / unveränderlich zu lassen (`Schnittstellenzentrale.Core.Contracts`)

| Klasse | Änderung |
|---|---|
| `CreateApplicationGroupRequest` | `IsSystem` wird **nicht** hinzugefügt. |
| `UpdateApplicationGroupRequest` | `IsSystem` wird **nicht** hinzugefügt. |
| `CreateApplicationRequest` | `IsSystem` wird **nicht** hinzugefügt. |
| `UpdateApplicationRequest` | `IsSystem` wird **nicht** hinzugefügt. |
| `ApplicationGroupResponse` | `IsSystem` kann hinzugefügt werden, damit das Frontend die Schutzlogik ableiten kann. |
| `ApplicationResponse` | `IsSystem` kann hinzugefügt werden (analog). |

### UI-Komponenten (Blazor) — zu erweitern (`Schnittstellenzentrale`)

| Komponente | Änderung |
|---|---|
| `ApplicationGroupContextMenu` | Löschen- und Umbenennen-Aktion deaktivieren, wenn `Group.IsSystem == true`. |
| `ApplicationContextMenu` | Löschen- und Bearbeiten-Aktion deaktivieren, wenn `Application.IsSystem == true`. |
| `ApplicationGroupTree` | Drag-Start für Anwendungen mit `IsSystem == true` verhindern (`draggable="false"` oder Guard in `OnDragStart`); Drag-Drop-Handler `OnDrop` prüft ebenfalls `IsSystem`. |

### Konfiguration

| Schlüssel | Ebene | Beschreibung |
|---|---|---|
| `Api:BaseUrl` | `appsettings.json` | Bereits vorhanden (eingeführt in Feature 7). Wird vom `SystemEntryInitializer` gelesen, um `BaseUrl` und `InterfaceUrl` der Systemanwendung zu setzen bzw. zu aktualisieren. |

### Tests — neu zu erstellen (`Schnittstellenzentrale.Tests`)

| Artefakt | Beschreibung |
|---|---|
| `SystemEntryInitializerTests` | Integrationstests: Gruppe/Anwendung wird angelegt wenn fehlend; URL wird aktualisiert wenn abweichend; Wiederholter Aufruf ist idempotent; Fehler beim DB-Zugriff blockiert Programmstart nicht. |
| `ApplicationGroupsControllerIntegrationTests` (Erweiterung) | DELETE auf Systemeintrag liefert `403`. |
| `ApplicationsControllerIntegrationTests` (Erweiterung) | DELETE auf Systemeintrag liefert `403`. |

---

## Implementierungsansatz

- `IsSystem` wird als reguläres EF-Core-Feld mit `HasDefaultValue(false)` konfiguriert. Eine neue EF-Core-Migration erzeugt die Spalten auf beiden Tabellen.
- Der `SystemEntryInitializer` arbeitet direkt auf `IApplicationRepository` (bzw. `AppDbContext` via `IServiceScope`), ohne den `IApplicationApiClient` zu verwenden — ein Loopback-HTTP-Aufruf beim Start wäre fehleranfällig und nicht notwendig.
- Die Startup-Logik prüft den `StorageMode` nicht zur Laufzeit über `IStorageModeService`, sondern liest die Datenbank immer im Shared-Kontext (`StorageMode.Team`, leerer Owner) — analog zur bisherigen Konvention in `ApplicationRepository`.
- Die `DeleteAsync`-Controller-Methoden erhalten eine Guard-Prüfung unmittelbar nach dem Laden der Entität: `if (group.IsSystem) return StatusCode(403)`.
- Der `ApplicationGroupTree` erhält einen Guard in `OnDragStart`: Wenn `_draggedApplication.IsSystem == true`, wird `_draggedApplication` nicht gesetzt (Drop hat dann keinen Effekt). Ergänzend kann `draggable` per Razor-Ausdruck auf `false` gesetzt werden.
- `ApplicationGroupContextMenu` und `ApplicationContextMenu` erhalten einen Parameter `IsReadOnly` (oder prüfen direkt `Group.IsSystem` / `Application.IsSystem`), der Schaltflächen deaktiviert (`disabled`-Attribut).
- `IsSystem` wird aus den Request-DTOs ferngehalten; das Feld kann in den Response-DTOs (`ApplicationGroupResponse`, `ApplicationResponse`) ergänzt werden, damit das Frontend es auslesen kann.

---

## Konfiguration

| Schlüssel | Ebene | Beschreibung |
|---|---|---|
| `Api:BaseUrl` | `appsettings.json` | Bereits vorhanden. Wird vom `SystemEntryInitializer` gelesen. Wert bestimmt `Application.BaseUrl` und `Application.InterfaceUrl` (`{Api:BaseUrl}/swagger/v1/swagger.json`) des Systemeintrags. |

---

## Offene Fragen

1. **Schutzebene für Update-Operationen:** Sollen PUT-Requests auf Systemeinträge (`ApplicationGroupsController.UpdateAsync`, `ApplicationsController.UpdateAsync`) ebenfalls mit `403` abgewiesen werden, oder ist ausschließlich DELETE gesperrt? Die Anforderung nennt explizit Löschen und Verschieben; Umbenennen wird in der Abgrenzung erwähnt, aber kein HTTP-Statuscode spezifiziert.

2. **Response-DTO-Erweiterung um `IsSystem`:** Soll `IsSystem` in `ApplicationGroupResponse` und `ApplicationResponse` exponiert werden, damit das Frontend die Schutzlogik clientseitig ableiten kann? Ohne dieses Feld müsste das Frontend nach dem Versuch einer gesperrten Aktion auf den Fehler reagieren.

3. **Verhalten bei fehlendem `Api:BaseUrl`:** Was soll der `SystemEntryInitializer` tun, wenn `Api:BaseUrl` in `appsettings.json` fehlt oder leer ist? Optionen: (a) Anlegen des Eintrags überspringen und warnen, (b) Eintrag ohne `BaseUrl` anlegen (verletzt `IsRequired` auf `BaseUrl`), (c) Programmstart mit Fehler abbrechen.

4. **Gruppenname als eindeutiger Bezeichner:** Die Suche nach dem Systemeintrag erfolgt anhand von `Name == "Schnittstellenzentrale"` und `IsSystem == true`. Falls ein Benutzer zuvor manuell eine Gruppe gleichen Namens angelegt hat (ohne `IsSystem`), entstehen zwei Einträge. Soll der `SystemEntryInitializer` ausschließlich nach `IsSystem == true` suchen (Name ist dann nur beim Anlegen relevant), oder ist der Name der eindeutige Schlüssel?

5. **Frontend-Schutz: Sichtbarkeit oder Deaktivierung:** Sollen Löschen- und Bearbeiten-Schaltflächen für Systemeinträge vollständig ausgeblendet oder nur deaktiviert werden? Ausblenden verhindert visuelle Verwirrung, erschwert aber das Erkennen des Systemcharakters.

6. **Drag & Drop — Zielgruppe ist Systemeintrag:** Darf eine normale Anwendung per Drag & Drop in die Systemgruppe verschoben werden? Die Anforderung spricht nur davon, dass Systemeinträge selbst nicht verschoben werden können; das Verschieben von Nicht-System-Anwendungen in die Systemgruppe ist nicht explizit adressiert.
