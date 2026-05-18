# Umsetzungsplan: Systemeintrag für eigene API beim Programmstart

## Übersicht

Das Feature erweitert `ApplicationGroup` und `Application` um ein `IsSystem`-Flag und legt beim Programmstart automatisch einen Systemeintrag für die Schnittstellenzentrale-API an bzw. aktualisiert dessen URLs. Systemeinträge sind über die REST-API nicht löschbar, nicht umbenennenbar und nicht bearbeitbar; über die Blazor-UI sind sie zusätzlich nicht per Drag & Drop verschiebbar. Betroffen sind das Datenmodell (`Core`), das Repository-Interface (`Core`), die EF-Core-Konfiguration und Migration (`Infrastructure`), die Startup-Routine (`Program.cs`), beide Controller, zwei Response-DTOs sowie drei Blazor-Komponenten und die Testabdeckung.

---

## Programmabläufe

### Startup: Systemeintrag anlegen oder aktualisieren

1. `Program.cs` ruft `EnsureDatabaseInitializedAsync` auf (bereits vorhanden).
2. `Program.cs` ruft anschließend `SystemEntryInitializer.InitializeAsync(app.Services, builder.Configuration)` auf.
3. `SystemEntryInitializer.InitializeAsync` erstellt einen `IServiceScope` aus dem `IServiceProvider`.
4. Aus dem Scope wird `IApplicationRepository` aufgelöst.
5. Der Initializer liest `Api:BaseUrl` aus `IConfiguration`; ist der Wert leer oder fehlt er, wird eine Warnung per Serilog geloggt und die Methode kehrt ohne Fehler zurück.
6. Über `IApplicationRepository.GetSystemGroupAsync` wird direkt nach der Gruppe mit `IsSystem == true` gesucht.
7. **Gruppe fehlt:** `IApplicationRepository.AddGroupAsync` legt eine neue `ApplicationGroup` mit `Name = "Schnittstellenzentrale"` und `IsSystem = true` an.
8. **Gruppe vorhanden:** Keine Änderung an der Gruppe nötig (Name und `IsSystem`-Flag sind durch diesen Prozess unveränderlich).
9. Innerhalb der gefundenen oder neu angelegten Gruppe wird nach einer `Application` mit `IsSystem == true` gesucht (aus den geladenen Navigationseigenschaften der Gruppe).
10. **Anwendung fehlt:** `IApplicationRepository.AddApplicationAsync` legt eine neue `Application` an (`Name = "Schnittstellenzentrale"`, `IsSystem = true`, `BaseUrl = {Api:BaseUrl}`, `InterfaceUrl = {Api:BaseUrl}/swagger/v1/swagger.json`, `ApplicationGroupId = <ID der Gruppe>`).
11. **Anwendung vorhanden, URL weicht ab:** `IApplicationRepository.UpdateApplicationAsync` aktualisiert `BaseUrl` und `InterfaceUrl`.
12. **Anwendung vorhanden, URL identisch:** Keine Aktion.
13. Jede Ausnahme innerhalb von Schritt 4–12 wird abgefangen, per Serilog als Fehler geloggt und verschluckt, damit der Programmstart nicht blockiert wird.

Beteiligte Klassen/Komponenten: `SystemEntryInitializer`, `Program`, `IApplicationRepository`, `ApplicationRepository`, `ApplicationGroup`, `Application`

---

### API: DELETE auf Systemeintrag abweisen

#### Gruppe löschen

1. `ApplicationGroupsController.DeleteAsync` lädt die Gruppe per `IApplicationRepository.GetGroupByIdAsync`.
2. Ist die Gruppe nicht gefunden, wird `404 Not Found` zurückgegeben (bereits vorhanden).
3. Ist `group.IsSystem == true`, wird `403 Forbidden` zurückgegeben.
4. Andernfalls wird `IApplicationRepository.DeleteGroupAsync` aufgerufen (bereits vorhanden).

Beteiligte Klassen/Komponenten: `ApplicationGroupsController`, `IApplicationRepository`

#### Anwendung löschen

1. `ApplicationsController.DeleteAsync` lädt die Anwendung per `IApplicationRepository.GetApplicationByIdAsync`.
2. Ist die Anwendung nicht gefunden, wird `404 Not Found` zurückgegeben (bereits vorhanden).
3. Ist `application.IsSystem == true`, wird `403 Forbidden` zurückgegeben.
4. Andernfalls wird `IApplicationRepository.DeleteApplicationAsync` aufgerufen (bereits vorhanden).

Beteiligte Klassen/Komponenten: `ApplicationsController`, `IApplicationRepository`

---

### API: UPDATE auf Systemeintrag abweisen

#### Gruppe umbenennen

1. `ApplicationGroupsController.UpdateAsync` lädt die Gruppe per `IApplicationRepository.GetGroupByIdAsync`.
2. Ist die Gruppe nicht gefunden, wird `404 Not Found` zurückgegeben (bereits vorhanden).
3. Ist `group.IsSystem == true`, wird `403 Forbidden` zurückgegeben.
4. Andernfalls wird die Aktualisierung durchgeführt (bereits vorhanden).

Beteiligte Klassen/Komponenten: `ApplicationGroupsController`, `IApplicationRepository`

#### Anwendung bearbeiten

1. `ApplicationsController.UpdateAsync` lädt die Anwendung per `IApplicationRepository.GetApplicationByIdAsync`.
2. Ist die Anwendung nicht gefunden, wird `404 Not Found` zurückgegeben (bereits vorhanden).
3. Ist `application.IsSystem == true`, wird `403 Forbidden` zurückgegeben.
4. Andernfalls wird die Aktualisierung durchgeführt (bereits vorhanden).

Beteiligte Klassen/Komponenten: `ApplicationsController`, `IApplicationRepository`

---

### UI: Kontextmenü-Aktionen für Systemeinträge deaktivieren

1. `ApplicationGroupContextMenu` empfängt den Parameter `Group` (vom Typ `ApplicationGroup`, der nun `IsSystem` trägt).
2. Wenn `Group.IsSystem == true`, wird das `disabled`-Attribut auf die Schaltflächen „Umbenennen" und „Löschen" gesetzt.

Beteiligte Klassen/Komponenten: `ApplicationGroupContextMenu`

1. `ApplicationContextMenu` empfängt den Parameter `Application` (vom Typ `Application`, der nun `IsSystem` trägt).
2. Wenn `Application.IsSystem == true`, wird das `disabled`-Attribut auf die Schaltflächen „Bearbeiten" und „Löschen" gesetzt.

Beteiligte Klassen/Komponenten: `ApplicationContextMenu`

---

### UI: Drag & Drop für Systemanwendungen sperren

1. `ApplicationGroupTree.OnDragStart` prüft, ob `application.IsSystem == true`.
2. Ist dies der Fall, setzt die Methode `_draggedApplication` **nicht** und bricht ab — die Drag-Operation hat damit keinen Effekt beim Drop.
3. Ergänzend wird das `draggable`-Attribut im Razor-Template per Ausdruck auf `"false"` gesetzt, wenn `application.IsSystem == true`.
4. `ApplicationGroupTree.OnDrop` prüft nicht auf die Zielgruppe — normale Anwendungen dürfen per Drag & Drop in die Systemgruppe verschoben und auch wieder heraus bewegt werden.

Beteiligte Klassen/Komponenten: `ApplicationGroupTree`

---

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `SystemEntryInitializer` | Statische Klasse | Startup-Routine: legt Systemgruppe und -anwendung an oder aktualisiert deren URLs beim Programmstart |

---

## Änderungen an bestehenden Klassen

### `ApplicationGroup` (Datenmodellklasse)

- **Neue Eigenschaften:** `IsSystem` (`bool`) — kennzeichnet den Eintrag als systemseitig verwaltet; Default `false`

---

### `Application` (Datenmodellklasse)

- **Neue Eigenschaften:** `IsSystem` (`bool`) — kennzeichnet den Eintrag als systemseitig verwaltet; Default `false`

---

### `IApplicationRepository` (Interface)

- **Neue Methoden:** `GetSystemGroupAsync` — gibt die Gruppe mit `IsSystem == true` zurück oder `null`, wenn keine existiert; keine Parameter; Rückgabewert `Task<ApplicationGroup?>`

---

### `ApplicationRepository` (Repository)

- **Neue Methoden:** `GetSystemGroupAsync` — implementiert `IApplicationRepository.GetSystemGroupAsync`; filtert direkt per Datenbankabfrage nach `IsSystem == true`; lädt Navigationseigenschaft `Applications` mit; Rückgabewert `Task<ApplicationGroup?>`

---

### `AppDbContext` (Datenbankschicht)

- **Geänderte Methoden:** `OnModelCreating` — konfiguriert `IsSystem` für `ApplicationGroup` und `Application` mit `HasDefaultValue(false)` (kein `IsRequired`)

---

### `ApplicationGroupResponse` (DTO)

- **Neue Eigenschaften:** `IsSystem` (`bool`) — ermöglicht dem Frontend die clientseitige Auswertung des Systemstatus

---

### `ApplicationResponse` (DTO)

- **Neue Eigenschaften:** `IsSystem` (`bool`) — ermöglicht dem Frontend die clientseitige Auswertung des Systemstatus

---

### `ApplicationGroupsController` (Controller)

- **Geänderte Methoden:** `DeleteAsync` — Guard-Prüfung auf `IsSystem` vor dem Löschen; gibt `403 Forbidden` zurück, wenn `true`
- **Geänderte Methoden:** `UpdateAsync` — Guard-Prüfung auf `IsSystem` vor der Aktualisierung; gibt `403 Forbidden` zurück, wenn `true`
- **Geänderte Methoden:** `MapToResponse` — überträgt `IsSystem` in `ApplicationGroupResponse`

---

### `ApplicationsController` (Controller)

- **Geänderte Methoden:** `DeleteAsync` — Guard-Prüfung auf `IsSystem` vor dem Löschen; gibt `403 Forbidden` zurück, wenn `true`
- **Geänderte Methoden:** `UpdateAsync` — Guard-Prüfung auf `IsSystem` vor der Aktualisierung; gibt `403 Forbidden` zurück, wenn `true`
- **Geänderte Methoden:** Inline-Mapping in `GetByIdAsync`, `GetAllAsync`, `GetUngroupedAsync` — `IsSystem` wird in `ApplicationResponse` übertragen

---

### `Program` (Startup)

- **Geänderte Methoden:** Hauptmethode — Aufruf von `SystemEntryInitializer.InitializeAsync(app.Services, builder.Configuration)` nach `EnsureDatabaseInitializedAsync`

---

### `ApplicationGroupContextMenu` (Blazor-Komponente)

- **Geänderte Methoden:** Razor-Template — `disabled`-Attribut auf Schaltflächen „Umbenennen" und „Löschen" gesetzt, wenn `Group.IsSystem == true`

---

### `ApplicationContextMenu` (Blazor-Komponente)

- **Geänderte Methoden:** Razor-Template — `disabled`-Attribut auf Schaltflächen „Bearbeiten" und „Löschen" gesetzt, wenn `Application.IsSystem == true`

---

### `ApplicationGroupTree` (Blazor-Komponente)

- **Geänderte Methoden:** `OnDragStart` — Guard: wenn `application.IsSystem == true`, wird `_draggedApplication` nicht gesetzt
- **Geänderte Methoden:** Razor-Template — `draggable`-Attribut per Ausdruck auf `"false"` gesetzt, wenn `application.IsSystem == true`

---

## Datenbankmigrationen

| Migrationsname | Betroffene Tabellen/Spalten | Beschreibung der Änderung |
|----------------|----------------------------|---------------------------|
| `AddIsSystemToApplicationGroupAndApplication` | `ApplicationGroups.IsSystem`, `Applications.IsSystem` | Fügt die Spalte `IsSystem` (bool, DB-Default `false`, nullable) zu beiden Tabellen hinzu |

---

## Validierungsregeln

Keine. Das Feld `IsSystem` ist in keinem Request-DTO enthalten und wird ausschließlich intern gesetzt; eine Benutzereingabe findet nicht statt.

---

## Konfigurationsänderungen

Keine. Der Schlüssel `Api:BaseUrl` ist bereits in `appsettings.json` vorhanden und wird unverändert vom `SystemEntryInitializer` gelesen.

---

## Seiteneffekte und Risiken

- **Bestehende Integrationstests:** Die Erweiterung von `ApplicationGroupResponse` und `ApplicationResponse` um `IsSystem` ändert das Serialisierungsformat. Tests, die den Response-Body per JSON deserialisieren und dabei das Modell vollständig binden, müssen `IsSystem` kennen. Tests, die nur einzelne Felder prüfen, sind nicht betroffen.
- **`ControllerTestFactory`:** Die In-Memory-Datenbank wird über `AppDbContext` aufgebaut; nach der Migration muss das Schema auch dort `IsSystem` kennen. Da `CreateInMemoryDbContext` das Schema aus dem Modell ableitet (nicht aus Migrationsdateien), ist kein manueller Eingriff nötig — das neue Feld ist nach Modellerweiterung automatisch vorhanden.
- **`IApplicationRepository`-Konsumenten:** Das Hinzufügen von `GetSystemGroupAsync` zum Interface erfordert eine Implementierung in allen Klassen, die das Interface implementieren. Im Produktionscode ist das ausschließlich `ApplicationRepository`; in Tests wird `IApplicationRepository` nicht gemockt (stattdessen wird die echte Implementierung über `ControllerTestFactory` eingebunden).
- **Drag & Drop in die Systemgruppe:** Das Verschieben normaler Anwendungen in die Systemgruppe per Drag & Drop wird durch dieses Feature nicht gesperrt. Normale Anwendungen dürfen in die Systemgruppe hinein- und wieder herausbewegt werden.

---

## Umsetzungsreihenfolge

1. `ApplicationGroup` — Eigenschaft `IsSystem` (`bool`, Default `false`) hinzufügen
2. `Application` — Eigenschaft `IsSystem` (`bool`, Default `false`) hinzufügen
3. `AppDbContext.OnModelCreating` — `IsSystem` für beide Entitäten konfigurieren
4. EF-Core-Migration `AddIsSystemToApplicationGroupAndApplication` erstellen und anwenden
5. `IApplicationRepository` — Methode `GetSystemGroupAsync` hinzufügen
6. `ApplicationRepository` — `GetSystemGroupAsync` implementieren
7. `ApplicationGroupResponse` — Eigenschaft `IsSystem` hinzufügen
8. `ApplicationResponse` — Eigenschaft `IsSystem` hinzufügen
9. `ApplicationGroupsController.MapToResponse` — `IsSystem` übertragen
10. `ApplicationsController` — `IsSystem` in allen Response-Mappings übertragen
11. `ApplicationGroupsController.DeleteAsync` und `UpdateAsync` — `IsSystem`-Guard hinzufügen
12. `ApplicationsController.DeleteAsync` und `UpdateAsync` — `IsSystem`-Guard hinzufügen
13. `SystemEntryInitializer` — neue statische Klasse mit `InitializeAsync` anlegen
14. `Program.cs` — Aufruf von `SystemEntryInitializer.InitializeAsync` einfügen
15. `ApplicationGroupContextMenu` — Schaltflächen für `IsSystem`-Gruppen deaktivieren
16. `ApplicationContextMenu` — Schaltflächen für `IsSystem`-Anwendungen deaktivieren
17. `ApplicationGroupTree.OnDragStart` — Guard für `IsSystem`-Anwendungen einbauen; `draggable`-Attribut im Template anpassen
18. Neue Tests in `SystemEntryInitializerTests` anlegen
19. `ApplicationGroupsControllerIntegrationTests` — 403-Tests ergänzen
20. `ApplicationsControllerIntegrationTests` — 403-Tests ergänzen
21. `ApplicationContextMenuTests` — Tests für deaktivierte Schaltflächen ergänzen

---

## Tests

### Neue Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `InitializeAsync_WhenGroupAndApplicationMissing_CreatesBoth` | `SystemEntryInitializerTests` | Gruppe und Anwendung werden neu angelegt, wenn beide fehlen |
| `InitializeAsync_WhenGroupExistsButApplicationMissing_CreatesApplication` | `SystemEntryInitializerTests` | Nur die Anwendung wird angelegt, wenn die Gruppe bereits existiert |
| `InitializeAsync_WhenUrlDiffers_UpdatesBaseUrlAndInterfaceUrl` | `SystemEntryInitializerTests` | `BaseUrl` und `InterfaceUrl` werden aktualisiert, wenn sie von `Api:BaseUrl` abweichen |
| `InitializeAsync_WhenUrlMatches_MakesNoChanges` | `SystemEntryInitializerTests` | Kein Datenbankzugriff zum Schreiben, wenn URLs bereits korrekt sind |
| `InitializeAsync_IsIdempotent_OnRepeatedCall` | `SystemEntryInitializerTests` | Wiederholter Aufruf erzeugt keine Duplikate |
| `InitializeAsync_WhenDbThrows_DoesNotPropagateException` | `SystemEntryInitializerTests` | Datenbankfehler werden abgefangen und geloggt; kein Programmabbruch |
| `InitializeAsync_WhenBaseUrlMissing_SkipsAndLogs` | `SystemEntryInitializerTests` | Fehlender `Api:BaseUrl`-Wert führt zu Warnung statt Ausnahme |
| `DeleteApplicationGroup_WithSystemGroup_Returns403` | `ApplicationGroupsControllerIntegrationTests` | DELETE auf Systemgruppe liefert 403 |
| `PutApplicationGroup_WithSystemGroup_Returns403` | `ApplicationGroupsControllerIntegrationTests` | PUT auf Systemgruppe liefert 403 |
| `DeleteApplication_WithSystemApplication_Returns403` | `ApplicationsControllerIntegrationTests` | DELETE auf Systemanwendung liefert 403 |
| `PutApplication_WithSystemApplication_Returns403` | `ApplicationsControllerIntegrationTests` | PUT auf Systemanwendung liefert 403 |
| `Bearbeiten_Deaktiviert_WennIsSystem` | `ApplicationContextMenuTests` | Schaltfläche „Bearbeiten" ist deaktiviert, wenn `Application.IsSystem == true` |
| `Löschen_Deaktiviert_WennIsSystem` | `ApplicationContextMenuTests` | Schaltfläche „Löschen" ist deaktiviert, wenn `Application.IsSystem == true` |

### Betroffene bestehende Tests

| Test / Testklasse | Grund der Anpassung |
|-------------------|---------------------|
| `GetApplicationGroupById_WithValidId_Returns200` in `ApplicationGroupsControllerIntegrationTests` | Response enthält neu `IsSystem`; Deserialisierungsmodell muss das Feld kennen, sofern der Body vollständig verglichen wird |
| `GetApplicationById_WithValidId_Returns200WithAllFields` in `ApplicationsControllerIntegrationTests` | Testname impliziert vollständige Feldprüfung; `IsSystem` muss im erwarteten Objekt gesetzt sein |

---

## Offene Punkte

Keine.
