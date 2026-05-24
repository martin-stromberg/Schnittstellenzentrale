# Umsetzungsplan: Verwaltung von Systemumgebungen mit Variablen

## Übersicht

Es werden zwei neue Datenmodellklassen (`SystemEnvironment`, `EnvironmentVariable`), ein Repository, ein Scoped-Service sowie drei Blazor-Komponenten eingeführt. Der `EndpointExecutionService` wird um die `{{Variablenname}}`-Platzhalterauflösung erweitert, und `MainLayout` erhält eine Auswahlbox sowie ein Zahnrad-Icon zur Umgebungsverwaltung. Betroffen sind die Projekte `Schnittstellenzentrale.Core`, `Schnittstellenzentrale.Infrastructure`, `Schnittstellenzentrale` (Blazor) und `Schnittstellenzentrale.Tests`.

---

## Programmabläufe

### Umgebung anlegen

1. Benutzer öffnet das Verwaltungsoverlay via Zahnrad-Icon im Header (`MainLayout` → `EnvironmentManagementOverlay.OpenAsync()`).
2. `EnvironmentManagementOverlay` ruft `ISystemEnvironmentRepository.GetEnvironmentsAsync(currentMode, owner)` auf und zeigt die Umgebungsliste alphabetisch nach `Name` sortiert an.
3. Benutzer klickt auf „Neu" — `EnvironmentManagementOverlay` öffnet `EnvironmentEditor` ohne Vorbelegung.
4. Benutzer gibt Namen und optional Variablen (`EnvironmentVariable`-Einträge) ein.
5. `EnvironmentEditor` prüft Namenseindeutigkeit per `ISystemEnvironmentRepository.GetEnvironmentsAsync` (UI-seitig, vor dem Speichern).
6. `Owner` wird via `ICurrentUserService.GetCurrentUserName()` gesetzt, wenn `Mode == StorageMode.User` — User-Umgebungen sind nur für den eigenen Benutzer sichtbar.
7. Bei Eindeutigkeit: `ISystemEnvironmentRepository.AddAsync(systemEnvironment)` wird aufgerufen.
8. Im Team-Modus: `ISignalRNotificationService.NotifyEnvironmentChangedAsync()` benachrichtigt andere Clients.
9. `EnvironmentManagementOverlay` aktualisiert die Liste; `EnvironmentSelector` wird via `EventCallback` oder `StateHasChanged` neu gerendert.

Beteiligte Klassen/Komponenten: `MainLayout`, `EnvironmentManagementOverlay`, `EnvironmentEditor`, `ISystemEnvironmentRepository`, `SystemEnvironmentRepository`, `ICurrentUserService`, `ISignalRNotificationService`, `SignalRNotificationService`

---

### Umgebung bearbeiten (inkl. Umbenennen)

1. Benutzer wählt eine vorhandene Umgebung in `EnvironmentManagementOverlay` aus.
2. `EnvironmentManagementOverlay` öffnet `EnvironmentEditor` mit den Daten der gewählten `SystemEnvironment` vorbelegt — dasselbe Formular wie beim Anlegen.
3. Benutzer ändert Name oder Variablen.
4. `EnvironmentEditor` prüft Namenseindeutigkeit (eigene ID ausschließen) und Variablennamens-Eindeutigkeit per LINQ auf der lokalen Liste.
5. `ISystemEnvironmentRepository.UpdateAsync(systemEnvironment)` wird aufgerufen.
6. Im Team-Modus: `ISignalRNotificationService.NotifyEnvironmentChangedAsync()` wird ausgelöst.
7. `MainLayout` empfängt die SignalR-Benachrichtigung und lädt die aktualisierte Umgebung via `GetByIdAsync`; falls die aktive Umgebung betroffen ist, aktualisiert `MainLayout` `IActiveEnvironmentService`.
8. `EnvironmentSelector` wird aktualisiert.

Beteiligte Klassen/Komponenten: `EnvironmentManagementOverlay`, `EnvironmentEditor`, `ISystemEnvironmentRepository`, `IActiveEnvironmentService`, `ISignalRNotificationService`, `MainLayout`

---

### Umgebung löschen

1. Benutzer wählt eine Umgebung in `EnvironmentManagementOverlay` aus und klickt auf „Löschen".
2. `EnvironmentManagementOverlay` zeigt einen Bestätigungsdialog.
3. Nach Bestätigung: `ISystemEnvironmentRepository.DeleteAsync(id)` wird aufgerufen (Cascade Delete löscht zugehörige `EnvironmentVariable`-Einträge automatisch).
4. Im Team-Modus: `ISignalRNotificationService.NotifyEnvironmentChangedAsync()` benachrichtigt andere Clients.
5. Alle verbundenen Clients empfangen die Benachrichtigung über SignalR. `MainLayout` prüft, ob die gelöschte Umgebung die aktive ist, und setzt `IActiveEnvironmentService` via `SetActiveEnvironment(null)` sofort auf `null`; der `localStorage`-Eintrag wird gelöscht.
6. `EnvironmentSelector` und `EnvironmentManagementOverlay` werden aktualisiert.

Beteiligte Klassen/Komponenten: `EnvironmentManagementOverlay`, `ISystemEnvironmentRepository`, `IActiveEnvironmentService`, `MainLayout`, `ISignalRNotificationService`

---

### Aktive Umgebung wählen

1. Benutzer wählt eine Umgebung in `EnvironmentSelector` (Dropdown im Header).
2. `EnvironmentSelector` speichert die ID via `IJSRuntime` in `localStorage` unter dem Schlüssel `selectedEnvironmentId_{mode}` (`Team` oder `User`).
3. `EnvironmentSelector` ruft `IActiveEnvironmentService.SetActiveEnvironment(systemEnvironment)` auf.
4. `IActiveEnvironmentService` aktualisiert `ActiveEnvironment` und materialisiert `ActiveVariables` als `IReadOnlyDictionary<string, string>`.

Beteiligte Klassen/Komponenten: `EnvironmentSelector`, `IActiveEnvironmentService`, `ActiveEnvironmentService`, `IJSRuntime`

---

### Moduswechsel mit Wiederherstellung der aktiven Umgebung

1. Benutzer wechselt `StorageMode` über den bestehenden Schalter in `MainLayout`.
2. `MainLayout.OnStorageModeChanged` setzt den neuen Modus via `StorageModeService.SetMode`.
3. `MainLayout` liest via `IJSRuntime` den `localStorage`-Wert `selectedEnvironmentId_{neuermodus}`.
4. Existiert eine gespeicherte ID: `ISystemEnvironmentRepository.GetByIdAsync(id)` prüft, ob die Umgebung noch vorhanden ist.
5. Existiert sie noch: `IActiveEnvironmentService.SetActiveEnvironment(systemEnvironment)` setzt die aktive Umgebung.
6. Existiert sie nicht mehr: `IActiveEnvironmentService.SetActiveEnvironment(null)` setzt leere Auswahl; `localStorage`-Eintrag wird gelöscht.
7. `EnvironmentSelector` wird mit den Umgebungen des neuen Modus neu befüllt (alphabetisch nach `Name` sortiert) und zeigt die wiederhergestellte (oder leere) Auswahl.

Beteiligte Klassen/Komponenten: `MainLayout`, `StorageModeService`, `IActiveEnvironmentService`, `ISystemEnvironmentRepository`, `EnvironmentSelector`, `IJSRuntime`

---

### SignalR-Aktualisierung der aktiven Umgebung im Team-Modus

1. Ein anderer Client führt eine CRUD-Operation auf `SystemEnvironment` durch; `NotifyEnvironmentChangedAsync()` wird ausgelöst.
2. `MainLayout` empfängt die SignalR-Benachrichtigung `EnvironmentChanged`.
3. `MainLayout` lädt die Umgebungsliste via `GetEnvironmentsAsync` neu und aktualisiert `EnvironmentSelector`.
4. Falls die aktive Umgebung gelöscht wurde: `MainLayout` setzt `IActiveEnvironmentService` sofort auf `null` und löscht den `localStorage`-Eintrag (sofortiger Rückfall, kein Neuladen erforderlich).
5. Falls die aktive Umgebung bearbeitet wurde: `MainLayout` lädt die aktualisierte Umgebung via `GetByIdAsync` und aktualisiert `IActiveEnvironmentService` mit den neuen Werten.

Beteiligte Klassen/Komponenten: `MainLayout`, `ISignalRNotificationService`, `SignalRNotificationService`, `IActiveEnvironmentService`, `ISystemEnvironmentRepository`

---

### Platzhalterauflösung beim Ausführen eines Endpunkts

1. `EndpointExecutionService.ExecuteAsync` wird aufgerufen.
2. `BuildRequest` ruft `IActiveEnvironmentService.ActiveVariables` ab (kein Datenbankzugriff zur Laufzeit).
3. `BuildRequest` wendet `ResolvePlaceholders(input, activeVariables)` nacheinander auf: Basis-URL (`Application.BaseUrl`), relativer Pfad (`Endpoint.RelativePath`), jeden Header-Namen (`EndpointHeader.Key`), jeden Header-Wert (`EndpointHeader.Value`), jeden Query-Parameter-Namen (`EndpointQueryParameter.Key`), jeden Query-Parameter-Wert (`EndpointQueryParameter.Value`), Bearer-Token und Body (`Endpoint.Body`).
4. Nach der `{{...}}`-Auflösung greift die bestehende `{...}`-Auflösung via `EndpointUrlBuilder.Resolve`.
5. Der fertig aufgelöste Request wird abgesendet.

Beteiligte Klassen/Komponenten: `EndpointExecutionService`, `IActiveEnvironmentService`, `EndpointUrlBuilder`

---

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `SystemEnvironment` | Datenmodellklasse | Systemumgebung mit `Name`, `Mode`, `Owner` und Variablenliste |
| `EnvironmentVariable` | Datenmodellklasse | Variable innerhalb einer `SystemEnvironment` mit `Name`, `Value` und `IsValueMasked` |
| `ISystemEnvironmentRepository` | Interface | CRUD-Kontrakt für `SystemEnvironment`-Persistierung |
| `SystemEnvironmentRepository` | Klasse | Implementierung von `ISystemEnvironmentRepository` via `AppDbContext` |
| `IActiveEnvironmentService` | Interface | Kontrakt für den Scoped-Service zur aktiven Umgebung |
| `ActiveEnvironmentService` | Klasse | Scoped-Service; hält `ActiveEnvironment` und materialisiertes `ActiveVariables`-Dictionary |
| `EnvironmentSelector` | Blazor-Komponente | Dropdown im Header zur Wahl der aktiven Umgebung |
| `EnvironmentManagementOverlay` | Blazor-Komponente | Modales Overlay zur CRUD-Verwaltung von Umgebungen |
| `EnvironmentEditor` | Blazor-Komponente | Einheitliches Formular für Anlegen und Bearbeiten einer Umgebung inkl. Variablenliste |
| `SystemEnvironmentRepositoryIntegrationTests` | Testklasse | Integrationstests für `SystemEnvironmentRepository` |

---

## Änderungen an bestehenden Klassen

### `AppDbContext` (Klasse, `Schnittstellenzentrale.Infrastructure`)

- **Neue Eigenschaften:** `SystemEnvironments` (`DbSet<SystemEnvironment>`) — Zugriff auf Systemumgebungen; `EnvironmentVariables` (`DbSet<EnvironmentVariable>`) — Zugriff auf Variablen
- **Geänderte Methoden:** `OnModelCreating` — Konfiguration der neuen Entitäten hinzufügen: Unique-Constraint auf `SystemEnvironment` (`Name` + `Mode` + `Owner`); Unique-Constraint auf `EnvironmentVariable` (`Name` + `SystemEnvironmentId`); Cascade Delete von `SystemEnvironment` auf `EnvironmentVariable`

---

### `ISignalRNotificationService` (Interface, `Schnittstellenzentrale.Core`)

- **Neue Methoden:** `NotifyEnvironmentChangedAsync()` — wird nach Schreiboperationen auf `SystemEnvironment` ausschließlich im Team-Modus aufgerufen; kein Parameter (analog zu den bestehenden Notify-Methoden)

---

### `SignalRNotificationService<THub>` (Klasse, `Schnittstellenzentrale.Infrastructure`)

- **Neue Methoden:** `NotifyEnvironmentChangedAsync()` — Implementierung von `ISignalRNotificationService.NotifyEnvironmentChangedAsync`; sendet eine SignalR-Nachricht `EnvironmentChanged` an die relevante Gruppe (Konvention analog zu `NotifyApplicationChangedAsync`)

---

### `EndpointExecutionService` (Klasse, `Schnittstellenzentrale.Infrastructure`)

- **Neue Eigenschaften:** Konstruktorparameter `IActiveEnvironmentService activeEnvironmentService` — Abhängigkeit für den Zugriff auf `ActiveVariables`
- **Neue Methoden:** `ResolvePlaceholders(string input, IReadOnlyDictionary<string, string> variables)` — ersetzt alle `{{name}}`-Vorkommen via `Regex.Replace` mit Muster `\{\{([^}]+)\}\}`; fehlende Variable ergibt leeren String; gibt `null`-sicher einen String zurück
- **Geänderte Methoden:** `BuildRequest` — ruft `ResolvePlaceholders` für Basis-URL, `RelativePath`, Header-Namen/-Werte, Query-Parameter-Namen/-Werte, Bearer-Token und `Body` auf; die Aufrufe erfolgen vor dem bestehenden `EndpointUrlBuilder.Resolve`-Aufruf

---

### `MainLayout` (Blazor-Komponente, `Schnittstellenzentrale`)

- **Neue Eigenschaften:** Injizierte Abhängigkeiten: `IActiveEnvironmentService`, `IJSRuntime`, `ISystemEnvironmentRepository`
- **Neue Methoden:** `RestoreEnvironmentFromLocalStorageAsync(StorageMode mode)` — liest `localStorage`-Eintrag, prüft Existenz via Repository, setzt aktive Umgebung oder fällt auf leere Auswahl zurück
- **Geänderte Methoden:** `OnInitialized` — abonniert zusätzlich das SignalR-Ereignis `EnvironmentChanged`; `OnAfterRenderAsync` — ruft `RestoreEnvironmentFromLocalStorageAsync` beim ersten Render auf; `OnStorageModeChanged` — ruft `RestoreEnvironmentFromLocalStorageAsync` für den neuen Modus auf
- **Neue Methoden:** `OnEnvironmentChanged` — Handler für das SignalR-Ereignis: lädt Umgebungsliste neu; setzt `IActiveEnvironmentService` sofort auf `null` wenn die aktive Umgebung gelöscht wurde; aktualisiert `IActiveEnvironmentService` wenn die aktive Umgebung bearbeitet wurde
- **Geänderte Methoden:** `Dispose` — meldet SignalR-Abonnement für `EnvironmentChanged` zusätzlich ab

---

### `TestHelpers` (Klasse, `Schnittstellenzentrale.Tests`)

- **Neue Methoden:** `ExecuteWithTwoSystemEnvironmentRepositoriesAsync(Func<SystemEnvironmentRepository, SystemEnvironmentRepository, Task>)` — analog zu `ExecuteWithTwoContextsAsync`, aber für `SystemEnvironmentRepository`; ermöglicht Concurrency-Tests

---

### `EndpointExecutionServiceTests` (Testklasse, `Schnittstellenzentrale.Tests`)

- **Geänderte Methoden:** Bestehende Tests, die `EndpointExecutionService` direkt instanziieren, müssen den neuen Konstruktorparameter `IActiveEnvironmentService` (als Mock ohne aktive Umgebung) erhalten.

---

## Datenbankmigrationen

| Migrationsname | Betroffene Tabellen/Spalten | Beschreibung der Änderung |
|----------------|----------------------------|---------------------------|
| `AddSystemEnvironments` | `SystemEnvironments`, `EnvironmentVariables` | Neue Tabellen anlegen; Unique-Constraints und Cascade-Delete-Fremdschlüssel konfigurieren |

---

## Validierungsregeln

| Feld / Objekt | Regel | Fehlerfall |
|---------------|-------|------------|
| `SystemEnvironment.Name` | Eindeutig pro `Mode` + `Owner` | Fehlermeldung im `EnvironmentEditor`: „Eine Umgebung mit diesem Namen existiert bereits." |
| `SystemEnvironment.Name` | Nicht leer | Speichern im `EnvironmentEditor` blockiert |
| `EnvironmentVariable.Name` | Eindeutig pro `SystemEnvironment` (LINQ-Prüfung auf lokaler Liste) | Fehlermeldung in der Variablentabelle des `EnvironmentEditor` |
| `EnvironmentVariable.Name` | Nicht leer | Zeile in der Variablentabelle kann nicht gespeichert werden |

---

## Konfigurationsänderungen

Keine.

---

## Seiteneffekte und Risiken

- **`EndpointExecutionService`-Konstruktor:** Der neue Pflichtparameter `IActiveEnvironmentService` ändert die Konstruktorsignatur. Alle DI-Registrierungen sind automatisch betroffen; direkte Instanziierungen in Tests müssen angepasst werden.
- **`ISignalRNotificationService`-Interface:** Das neue Interface-Mitglied `NotifyEnvironmentChangedAsync` erzwingt eine Implementierung in allen bestehenden Klassen und Test-Mocks, die dieses Interface implementieren.
- **`AppDbContext.OnModelCreating`:** Erweiterungen müssen bestehende Konfigurationen unangetastet lassen; die Migration muss auf einem sauberen Schema getestet werden.
- **`MainLayout`-Rendering:** Die neuen Injektionen (`IJSRuntime`, `ISystemEnvironmentRepository`) und der `localStorage`-Zugriff in `OnAfterRenderAsync` müssen mit dem bestehenden Theme-Initialisierungsablauf koordiniert werden, um doppelte Render-Zyklen zu vermeiden.
- **`IsValueMasked` (Maskierung):** Der Variablenwert wird unverändert in der Datenbank gespeichert und beim Request verwendet. Nur die UI-Darstellung maskiert den Wert mit Sternchen. Es liegt kein Schutz durch serverseitige Verschlüsselung vor — dies ist bewusste Entscheidung und kein Defizit.

---

## Umsetzungsreihenfolge

1. Datenmodellklassen `SystemEnvironment` und `EnvironmentVariable` anlegen (`Schnittstellenzentrale.Core`)
2. `ISystemEnvironmentRepository` anlegen (`Schnittstellenzentrale.Core`)
3. `IActiveEnvironmentService` anlegen (`Schnittstellenzentrale.Core`)
4. `ISignalRNotificationService` um `NotifyEnvironmentChangedAsync` erweitern (`Schnittstellenzentrale.Core`)
5. `AppDbContext` um `DbSet`-Eigenschaften und `OnModelCreating`-Konfiguration erweitern (`Schnittstellenzentrale.Infrastructure`)
6. EF-Core-Migration `AddSystemEnvironments` erstellen und anwenden (`Schnittstellenzentrale.Infrastructure`)
7. `SystemEnvironmentRepository` implementieren (`Schnittstellenzentrale.Infrastructure`) — analog zu `ApplicationRepository` inkl. `ApplyOwnerFilter` und `ICurrentUserService`
8. `ActiveEnvironmentService` implementieren (`Schnittstellenzentrale.Infrastructure`) — analog zu `StorageModeService`
9. `SignalRNotificationService` um `NotifyEnvironmentChangedAsync` erweitern (`Schnittstellenzentrale.Infrastructure`)
10. `EndpointExecutionService` um `IActiveEnvironmentService`-Abhängigkeit und `ResolvePlaceholders`-Methode erweitern sowie `BuildRequest` anpassen (`Schnittstellenzentrale.Infrastructure`)
11. DI-Registrierungen für `ISystemEnvironmentRepository`, `SystemEnvironmentRepository`, `IActiveEnvironmentService`, `ActiveEnvironmentService` hinzufügen (`Schnittstellenzentrale`)
12. Blazor-Komponente `EnvironmentEditor` erstellen (`Schnittstellenzentrale`)
13. Blazor-Komponente `EnvironmentManagementOverlay` erstellen (`Schnittstellenzentrale`)
14. Blazor-Komponente `EnvironmentSelector` erstellen (`Schnittstellenzentrale`)
15. `MainLayout` erweitern: `EnvironmentSelector` und Zahnrad-Icon integrieren, `localStorage`-Logik implementieren, SignalR-Abonnement für `EnvironmentChanged` aufbauen, `Dispose` erweitern (`Schnittstellenzentrale`)
16. `TestHelpers` um `ExecuteWithTwoSystemEnvironmentRepositoriesAsync` erweitern (`Schnittstellenzentrale.Tests`)
17. Bestehende `EndpointExecutionServiceTests` anpassen (neuer Konstruktorparameter) (`Schnittstellenzentrale.Tests`)
18. `SystemEnvironmentRepositoryIntegrationTests` erstellen (`Schnittstellenzentrale.Tests`)
19. Neue `EndpointExecutionServiceTests`-Szenarien für `{{...}}`-Auflösung hinzufügen (`Schnittstellenzentrale.Tests`)
20. Playwright-E2E-Test für Umgebungsvariablen-Auflösung erstellen (`Schnittstellenzentrale.Tests`)
21. Playwright-E2E-Test für Sichtbarkeits-Toggles (Maskierung) erstellen (`Schnittstellenzentrale.Tests`)

---

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `AddEnvironment_PersistsEnvironment` | `SystemEnvironmentRepositoryIntegrationTests` | Neue Umgebung wird korrekt gespeichert |
| `AddEnvironment_WithDuplicateName_ThrowsConstraintException` | `SystemEnvironmentRepositoryIntegrationTests` | Unique-Constraint auf `Name` + `Mode` + `Owner` wird durchgesetzt |
| `DeleteEnvironment_CascadesVariables` | `SystemEnvironmentRepositoryIntegrationTests` | Cascade Delete entfernt alle `EnvironmentVariable`-Einträge |
| `GetEnvironments_WithStorageModeUser_ReturnsOnlyOwnedEnvironments` | `SystemEnvironmentRepositoryIntegrationTests` | Owner-Filterung im User-Modus — nur eigene Umgebungen sichtbar |
| `GetEnvironments_WithStorageModeTeam_ReturnsAllTeamEnvironments` | `SystemEnvironmentRepositoryIntegrationTests` | Keine Filterung im Team-Modus |
| `UpdateEnvironment_PersistsChanges` | `SystemEnvironmentRepositoryIntegrationTests` | Änderungen an Umgebung und Variablen werden gespeichert |
| `BuildRequest_ResolvesDoubleBracePlaceholdersBeforeSingleBrace` | `EndpointExecutionServiceTests` | `{{var}}`-Auflösung erfolgt vor `{pfad}`-Auflösung |
| `BuildRequest_MissingVariable_ReplacesWithEmptyString` | `EndpointExecutionServiceTests` | Fehlende Variable ergibt leeren String |
| `BuildRequest_NoActiveEnvironment_ReplacesAllDoubleBracePlaceholdersWithEmptyString` | `EndpointExecutionServiceTests` | Ohne aktive Umgebung werden alle `{{...}}` durch leere Strings ersetzt |
| `BuildRequest_ResolvesPlaceholdersInBaseUrl` | `EndpointExecutionServiceTests` | `{{...}}`-Auflösung in Basis-URL |
| `BuildRequest_ResolvesPlaceholdersInRelativePath` | `EndpointExecutionServiceTests` | `{{...}}`-Auflösung in relativem Pfad |
| `BuildRequest_ResolvesPlaceholdersInHeaderNamesAndValues` | `EndpointExecutionServiceTests` | `{{...}}`-Auflösung in Header-Name und -Wert |
| `BuildRequest_ResolvesPlaceholdersInQueryParameterNamesAndValues` | `EndpointExecutionServiceTests` | `{{...}}`-Auflösung in Query-Parameter-Name und -Wert |
| `BuildRequest_ResolvesPlaceholdersInBearerToken` | `EndpointExecutionServiceTests` | `{{...}}`-Auflösung im Bearer-Token |
| `BuildRequest_ResolvesPlaceholdersInBody` | `EndpointExecutionServiceTests` | `{{...}}`-Auflösung im Body |
| `UmgebungMitVariable_Aktivieren_EndpunktSendetAufgeloestUrl` | `EndpointExecutionTests` (Playwright) | E2E: Umgebung mit Variable anlegen, aktivieren, Endpunkt ausführen, aufgelöste URL in Antwortanzeige prüfen |
| `MaskierterWert_IstNichtImKlartextImDomSichtbar` | `EnvironmentManagementTests` (Playwright, neu) | E2E: Maskierter Variablenwert erscheint nicht im Klartext im DOM; nach Klick auf Auge-Icon wird Wert sichtbar; Wert in DB bleibt unverändert |
| `ExecuteWithTwoSystemEnvironmentRepositoriesAsync` | `TestHelpers` | Hilfsmethode für Concurrency-Tests mit zwei `SystemEnvironmentRepository`-Instanzen |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| Alle Tests in `EndpointExecutionServiceTests` | `EndpointExecutionService`-Konstruktor erhält neuen Parameter `IActiveEnvironmentService`; alle Instanziierungen müssen einen Mock (ohne aktive Umgebung) übergeben |
| Alle Mocks von `ISignalRNotificationService` in Tests | Interface erhält neue Methode `NotifyEnvironmentChangedAsync`; Mocks müssen diese implementieren (leere Implementierung genügt, wenn nicht explizit geprüft) |

---

## Offene Punkte

Keine.
