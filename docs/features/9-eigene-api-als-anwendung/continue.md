# Offene Aufgaben

Erstellt am: 2026-05-18
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen (jeweils 6 Befunde)

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine — Plan-Review hat Status `Vollständig umgesetzt`.

## Code-Review-Befunde

### ApplicationGroupsController.cs / ApplicationsController.cs (beide Controller)

- **Inkonsistente Token-Validierungsstrategie** — `GetAllAsync()` in beiden Controllern verwendet `ParseRequestContextAsync()`, das Token, StorageMode und Owner in einem Schritt liest. Alle anderen Aktionen (GetById, Create, Update, Delete) rufen `ValidateTokenAndSetResponseHeaderAsync()` direkt auf und ermitteln `StorageMode` danach separat via `ParseStorageMode()`. Für einen Leser ist unklar, wann welche Variante korrekt ist.

  Empfehlung: Aktionen, die `StorageMode` benötigen (Create, Update, Delete), ebenfalls auf `ParseRequestContextAsync()` umstellen. Aktionen, die StorageMode nicht benötigen (GetById), können die direkte Variante behalten; dies sollte dann per Kommentar begründet sein.

### ApplicationResponse.cs (`InterfaceType`)

- **Typverlust im DTO** — Das Feld `InterfaceType` ist in `ApplicationResponse` als `int` deklariert, während das Domänenmodell den Enum `InterfaceType` verwendet. API-Konsumenten erhalten einen rohen Integer ohne Semantik. Die Integrationstests belegen das Problem explizit mit `(int)InterfaceType.Rest`.

  Empfehlung: `InterfaceType` in `ApplicationResponse` als Enum-Typ `InterfaceType` deklarieren (der Enum liegt bereits im Core-Projekt). Alternativ als `string` serialisieren, damit der Vertrag selbstdokumentierend ist.

### ApplicationGroupsControllerIntegrationTests.cs / ApplicationsControllerIntegrationTests.cs

- **Massiver doppelter Code** — Das Muster „Token holen → POST absetzen → neuen Token aus `X-New-Token`-Header lesen → damit den nächsten Request stellen" wiederholt sich inline in mindestens 8 Testmethoden.

  Empfehlung: In `ControllerTestFactory` eine Hilfsmethode ergänzen, z. B. `CreateApplicationGroupAsync` und `CreateApplicationAsync`, die das POST inklusive Token-Rotation kapseln.

- **Fehlende Testabdeckung: 401 für GetById** — Für `GET /api/applications/{id}` und `GET /api/application-groups/{id}` fehlt jeweils ein Test ohne Token (erwartet 401).

  Empfehlung: Je einen Test `GetApplicationById_WithoutToken_Returns401` und `GetApplicationGroupById_WithoutToken_Returns401` ergänzen.

### Program.cs

- **Toter Code (unnötige Hilfsmethode)** — `MigrateDatabaseAsync(AppDbContext dbContext)` enthält ausschließlich `await dbContext.Database.MigrateAsync()` und wird nur von `EnsureDatabaseInitializedAsync` aufgerufen.

  Empfehlung: `MigrateDatabaseAsync` entfernen und `await dbContext.Database.MigrateAsync()` direkt in `EnsureDatabaseInitializedAsync` aufrufen.

### ApplicationGroupTree.razor (`OnModeChanged`)

- **Doppelte Verantwortlichkeit für `_errorMessage`** — `LoadDataAsync()` und `OnModeChanged` setzen `_errorMessage` im Fehlerfall unabhängig voneinander mit unterschiedlichen Formulierungen.

  Empfehlung: Eine der beiden Stellen als einzige Verantwortliche für `_errorMessage` festlegen und die andere entfernen.
