# Anforderung: Verwaltung von Systemumgebungen mit Variablen

## Fachliche Zusammenfassung

Das System wird um ein Konzept der „Systemumgebungen" (`SystemEnvironment`) erweitert, das es Anwendern erlaubt, benannte Variablensätze zu definieren und einer aktiven Umgebung zuzuordnen. Systemumgebungen sind an den `StorageMode` (`Team` / `User`) gebunden und werden in der Datenbank persistiert. Im Header der Anwendung erscheinen neben dem bestehenden `StorageMode`-Schalter eine Auswahlbox für die aktive Umgebung sowie ein Zahnrad-Icon zur Verwaltung über ein modales Overlay. Beim Absenden eines Requests löst `EndpointExecutionService` alle `{{Variablenname}}`-Platzhalter in den relevanten Feldern (Basis-URL, relative URL, Header, Query-Parameter, Bearer-Token, Body) durch die Werte der aktiven `SystemEnvironment` auf — bevor die bestehende `{Pfadparameter}`-Auflösung greift. Die Eingabedaten der Endpunkte und Anwendungen bleiben dabei unverändert; die Auflösung erfolgt ausschließlich zur Laufzeit des Requests.

---

## Betroffene Klassen und Komponenten

### Datenmodellklassen — neu (`Schnittstellenzentrale.Core`)

| Klasse | Beschreibung |
|---|---|
| `SystemEnvironment` | Systemumgebung mit `Name` (eindeutig pro `StorageMode`), `Mode` (`StorageMode`-Enum), `Owner` (`string?`, analog zu `Application`) und einer Liste von `EnvironmentVariable`-Einträgen. |
| `EnvironmentVariable` | Variable innerhalb einer `SystemEnvironment`: `Name` (eindeutig pro Umgebung), `Value` (`string`), `IsValueMasked` (`bool` — steuert Sichtbarkeitsanzeige in der UI). Fremdschlüssel auf `SystemEnvironment`. |

### Enums — keine neuen Enums erforderlich

`StorageMode` (bereits vorhanden als `Team` / `User`) wird als Modus-Zuordnung für `SystemEnvironment` wiederverwendet.

### Datenbankschicht — zu erweitern (`Schnittstellenzentrale.Infrastructure`)

| Artefakt | Änderung |
|---|---|
| `AppDbContext` | Neue `DbSet<SystemEnvironment>` und `DbSet<EnvironmentVariable>` mit Konfiguration (Unique-Constraint auf `SystemEnvironment.Name` + `Mode` + `Owner`; Unique-Constraint auf `EnvironmentVariable.Name` + `SystemEnvironmentId`; Cascade Delete). |
| EF-Core-Migration | Neue Tabellen `SystemEnvironments` und `EnvironmentVariables`. |

### Interfaces — neu (`Schnittstellenzentrale.Core`)

| Interface | Beschreibung |
|---|---|
| `ISystemEnvironmentRepository` | CRUD-Operationen: `GetEnvironmentsAsync(StorageMode, string? owner)`, `GetByIdAsync(int id)`, `AddAsync(SystemEnvironment)`, `UpdateAsync(SystemEnvironment)`, `DeleteAsync(int id)`. Validierung auf Namenseindeutigkeit wird auf Datenbankebene (Constraint) und optional auf Service-Ebene durchgesetzt. |

### Logikklassen / Services — neu (`Schnittstellenzentrale.Infrastructure`)

| Klasse | Beschreibung |
|---|---|
| `SystemEnvironmentRepository` | Implementierung von `ISystemEnvironmentRepository` über `AppDbContext`. |

### Logikklassen / Services — zu erweitern

| Klasse | Änderung |
|---|---|
| `EndpointExecutionService` | Neue private Methode `ResolvePlaceholders(string input, IReadOnlyDictionary<string, string> variables)`: ersetzt alle `{{name}}`-Vorkommen durch den Variablenwert oder leeren String, wenn die Variable nicht definiert ist. Wird in `BuildRequest` vor der bestehenden `{Pfadparameter}`-Auflösung auf Basis-URL, relative URL, Header-Name/-Wert, Query-Parameter-Name/-Wert, Bearer-Token und Body angewendet. |

### UI-Komponenten (Blazor) — neu zu erstellen (`Schnittstellenzentrale`)

| Komponente | Beschreibung |
|---|---|
| `EnvironmentSelector` | Auswahlbox im Header: zeigt alle `SystemEnvironment`-Einträge des aktiven `StorageMode`. Leere Auswahl ist zulässig. Speichert die Auswahl via JavaScript-Interop im `localStorage` (Schlüssel: `selectedEnvironmentId_{mode}`). |
| `EnvironmentManagementOverlay` | Modales Overlay (zentriert, mit Backdrop) zur CRUD-Verwaltung von `SystemEnvironment`-Einträgen des aktiven `StorageMode`. Enthält eine Liste der Umgebungen und öffnet bei Auswahl den `EnvironmentEditor`. Bestätigungsdialog beim Löschen. |
| `EnvironmentEditor` | Inline-Formular oder Modal für Name und Variablenliste einer `SystemEnvironment`. Variablenliste ist eine editierbare Tabelle (`EnvironmentVariable`-Einträge: Name, Wert, Sichtbarkeits-Toggle per Auge-Icon). Validierung: Name eindeutig pro Modus, Variablenname eindeutig pro Umgebung. |

### UI-Komponenten (Blazor) — zu erweitern (`Schnittstellenzentrale`)

| Komponente | Änderung |
|---|---|
| `MainLayout` | Integration von `EnvironmentSelector` und Zahnrad-Icon (`EnvironmentManagementOverlay`-Trigger) rechts neben dem bestehenden `StorageMode`-Schalter. Beim Moduswechsel wird die zuletzt gewählte Umgebung des neuen Modus aus `localStorage` wiederhergestellt. Beim Löschen der aktiven Umgebung Rückfall auf „keine Umgebung ausgewählt". |

### Services / Hilfsklassen — zu erweitern oder neu

| Klasse | Beschreibung |
|---|---|
| `IActiveEnvironmentService` / `ActiveEnvironmentService` | Hält die aktuell aktive `SystemEnvironment` (inkl. aufgelöster Variablen als `IReadOnlyDictionary<string, string>`) pro Blazor-Circuit (Scoped DI). Wird von `EndpointExecutionService` abgefragt. *(Annahme: Scoped-Service analog zu `StorageModeService`.)* |

### Tests — zu erweitern/neu zu erstellen (`Schnittstellenzentrale.Tests`)

| Artefakt | Beschreibung |
|---|---|
| `SystemEnvironmentRepositoryIntegrationTests` | Szenarien: CRUD, Namenseindeutigkeit (Constraint-Verletzung), Cascade Delete bei Umgebungslöschung. |
| `EndpointExecutionServiceTests` (Erweiterung) | Szenarien: (1) `{{var}}`-Platzhalter werden vor `{pfad}`-Platzhaltern aufgelöst. (2) Fehlende Variable ergibt leeren String. (3) Keine aktive Umgebung → alle `{{...}}` durch leere Strings ersetzt. (4) Platzhalter in Basis-URL, relativer URL, Header-Name/-Wert, Query-Parametern, Bearer-Token und Body werden aufgelöst. |
| Playwright-Tests | E2E-Szenario: Umgebung mit Variable `baseUrl=https://example.com` anlegen, aktivieren, Endpunkt mit `{{baseUrl}}/api/test` senden, gesendete URL in der Antwortanzeige prüfen. |

---

## Implementierungsansatz

### Platzhalter-Auflösung (`EndpointExecutionService`)

`ResolvePlaceholders(string input, IReadOnlyDictionary<string, string> variables)` wendet `Regex.Replace` mit dem Muster `\{\{([^}]+)\}\}` auf `input` an. Für jeden Treffer wird `variables.TryGetValue(name, out var val)` aufgerufen; fehlt die Variable, wird ein leerer String eingesetzt. Da `{{...}}` und `{...}` sich syntaktisch unterscheiden, ist keine Kollision mit der bestehenden Pfadparameter-Auflösung möglich — dennoch gilt die Reihenfolge: erst `{{...}}`-Auflösung, dann `{...}`-Auflösung.

### `ActiveEnvironmentService`

Scoped-Service (analog zu `StorageModeService`). Hält `SystemEnvironment? ActiveEnvironment` und eine materialisierte `IReadOnlyDictionary<string, string> ActiveVariables` (leer wenn keine Umgebung aktiv). `EndpointExecutionService` ruft `IActiveEnvironmentService.ActiveVariables` ab; keine Datenbankabfrage zur Laufzeit des Requests erforderlich.

### localStorage-Persistierung (Client-seitig)

`EnvironmentSelector` schreibt beim Ändern der Auswahl via `IJSRuntime` in `localStorage`:
- `selectedEnvironmentId_Team` / `selectedEnvironmentId_User` (jeweilige Umgebungs-ID oder leer)

`MainLayout` liest diese Werte beim Initialisieren und nach Moduswechsel. Beim Laden wird geprüft, ob die gespeicherte ID noch existiert (Umgebung könnte inzwischen gelöscht worden sein); falls nicht, Rückfall auf leere Auswahl.

### Verwaltungsoverlay

`EnvironmentManagementOverlay` ist eine Blazor-Komponente mit einem `bool _isVisible`-Zustand. Das Zahnrad-Icon in `MainLayout` ruft eine Methode `OpenAsync()` auf. Das Overlay rendert nur Umgebungen des aktiven `StorageMode` (gefiltert via `ISystemEnvironmentRepository.GetEnvironmentsAsync`). Nach jeder CRUD-Operation wird `EnvironmentSelector` über einen `EventCallback` oder `StateHasChanged` aktualisiert.

### Validierung

- Eindeutigkeit des Umgebungsnamens pro Modus: serverseitig per Datenbank-Unique-Constraint; zusätzlich UI-seitige Prüfung vor dem Speichern über einen `ISystemEnvironmentRepository`-Aufruf, um eine aussagekräftige Fehlermeldung anzuzeigen.
- Eindeutigkeit des Variablennamens pro Umgebung: UI-seitig per `LINQ`-Prüfung auf der lokalen Liste vor dem Speichern.

### SignalR

Im `Team`-Modus werden Schreiboperationen auf `SystemEnvironment` über `ISignalRNotificationService` gemeldet, sodass andere Clients die Auswahlbox aktualisieren können. *(Annahme: Eine neue Notification-Methode `NotifyEnvironmentChangedAsync()` wird ergänzt.)*

---

## Konfiguration

Kein zusätzlicher Konfigurationsbedarf in `appsettings.json`. Die Auswahl der aktiven Umgebung und der aktive Modus werden ausschließlich client-seitig im `localStorage` persistiert.

---

## Offene Fragen

1. **`Owner`-Feld bei `StorageMode.User`:** Wird `SystemEnvironment.Owner` analog zu `Application.Owner` mit dem Windows-Benutzernamen (`ICurrentUserService.GetCurrentUserName()`) befüllt, oder sind benutzerspezifische Umgebungen global für alle Benutzer im User-Modus sichtbar?

2. **`ActiveEnvironmentService` — Aktualisierung bei Datenbankänderungen:** Wenn ein anderer Client eine Umgebung im Team-Modus bearbeitet, soll `ActiveEnvironmentService` automatisch über SignalR aktualisiert werden, oder reicht eine Aktualisierung beim nächsten Seitenneuladen?

3. **Löschen der aktiven Umgebung — SignalR-Benachrichtigung:** Wenn ein anderer Client die aktive Umgebung löscht, soll der betroffene Client sofort auf leere Auswahl zurückfallen (SignalR-gesteuert) oder erst nach dem nächsten Seitenneuladen?

4. **Sichtbarkeit von `IsValueMasked` im Netzwerkverkehr:** Maskierte Variablenwerte werden in der UI mit Sternchen dargestellt, aber beim Request unverändert übertragen. Soll der Wert auch serverseitig verschlüsselt gespeichert werden (analog zu `ICredentialService`), oder reicht die reine UI-Maskierung?

5. **`NotifyEnvironmentChangedAsync` — Granularität:** Soll die SignalR-Benachrichtigung nur im Team-Modus ausgelöst werden (analog zu den bestehenden Notify-Methoden), oder auch im User-Modus?

6. **Umgebungsname im Overlay — Editierbarkeit:** Soll das Umbenennen einer Umgebung inline in der Listenansicht erfolgen (wie `RenameEndpointGroupDialog`) oder über denselben `EnvironmentEditor`, der auch für neue Umgebungen verwendet wird?

7. **Reihenfolge der Umgebungen in der Auswahlbox:** Nach welchem Kriterium werden Umgebungen sortiert (alphabetisch nach Name, nach Erstellungsdatum, nach ID)?

8. **Playwright-Testabdeckung der Sichtbarkeits-Toggles:** Sollen maskierte Variablenwerte in Playwright-Tests explizit geprüft werden (z. B. dass der Wert nicht im DOM sichtbar ist), oder reicht ein Unit-Test für die UI-Logik?
