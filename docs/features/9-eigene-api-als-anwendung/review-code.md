# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### ApplicationGroupsController.cs / ApplicationsController.cs (beide Controller)

- **Inkonsistente Token-Validierungsstrategie** — `GetAllAsync()` in beiden Controllern verwendet `ParseRequestContextAsync()`, das Token, StorageMode und Owner in einem Schritt liest. Alle anderen Aktionen (GetById, Create, Update, Delete) rufen `ValidateTokenAndSetResponseHeaderAsync()` direkt auf und ermitteln `StorageMode` danach separat via `ParseStorageMode()`. Für einen Leser ist unklar, wann welche Variante korrekt ist.

  Empfehlung: Aktionen, die `StorageMode` benötigen (Create, Update, Delete), ebenfalls auf `ParseRequestContextAsync()` umstellen. Aktionen, die StorageMode nicht benötigen (GetById), können die direkte Variante behalten; dies sollte dann per Kommentar begründet sein.

### ApplicationResponse.cs (`InterfaceType`)

- **Typverlust im DTO** — Das Feld `InterfaceType` ist in `ApplicationResponse` als `int` deklariert (Zeile 12), während das Domänenmodell den Enum `InterfaceType` verwendet. API-Konsumenten erhalten einen rohen Integer ohne Semantik. Die Integrationstests belegen das Problem explizit mit `(int)InterfaceType.Rest` (`ApplicationsControllerIntegrationTests.cs`, Zeile 226).

  Empfehlung: `InterfaceType` in `ApplicationResponse` als Enum-Typ `InterfaceType` deklarieren (der Enum liegt bereits im Core-Projekt). Alternativ als `string` serialisieren, damit der Vertrag selbstdokumentierend ist.

### ApplicationGroupsControllerIntegrationTests.cs / ApplicationsControllerIntegrationTests.cs

- **Massiver doppelter Code** — Das Muster „Token holen → POST absetzen → neuen Token aus `X-New-Token`-Header lesen → damit den nächsten Request stellen" wiederholt sich inline in mindestens 8 Testmethoden (z. B. `GetApplicationGroups_WithValidToken_Returns200WithList`, `PutApplicationGroup_WithValidRequest_Returns200AndRotatesToken`, `DeleteApplicationGroup_WithValidId_Returns204AndRotatesToken` und die entsprechenden Anwendungstests). Der Boilerplate-Block umfasst jeweils 8–10 Zeilen.

  Empfehlung: In `ControllerTestFactory` eine Hilfsmethode ergänzen, z. B. `CreateApplicationGroupAsync(HttpClient client, string token, string name)` und `CreateApplicationAsync(HttpClient client, string token, ...)`, die das POST inklusive Token-Rotation kapseln und das Ergebnis-DTO sowie den neuen Token zurückgeben. Die Testmethoden werden dadurch erheblich kürzer.

- **Fehlende Testabdeckung: 401 für GetById** — Für `GET /api/applications/{id}` und `GET /api/application-groups/{id}` fehlt jeweils ein Test, der ohne Token einen 401 zurückbekommt. Alle anderen Endpunkte haben diesen Fall abgedeckt.

  Empfehlung: Je einen Test `GetApplicationById_WithoutToken_Returns401` und `GetApplicationGroupById_WithoutToken_Returns401` ergänzen.

### Program.cs

- **Toter Code (unnötige Hilfsmethode)** — `MigrateDatabaseAsync(AppDbContext dbContext)` (Zeile 110) enthält ausschließlich `await dbContext.Database.MigrateAsync()` und wird nur von `EnsureDatabaseInitializedAsync` aufgerufen. Die Methode erzeugt keine Abstraktion und keinen Mehrwert.

  Empfehlung: `MigrateDatabaseAsync` entfernen und `await dbContext.Database.MigrateAsync()` direkt in `EnsureDatabaseInitializedAsync` aufrufen.

### ApplicationGroupTree.razor (`OnModeChanged`)

- **Doppelte Verantwortlichkeit für `_errorMessage`** — `LoadDataAsync()` setzt `_errorMessage = null` und im Fehlerfall `_errorMessage = $"Fehler beim Laden der Daten: {ex.Message}"` selbst (Zeilen 104/110). `OnModeChanged` setzt `_errorMessage` im äußeren catch zusätzlich nochmals (Zeile 138) mit einer anderen Formulierung. Beide Stellen sind für denselben Zustand zuständig; welche Nachricht erscheint, hängt davon ab, wo die Exception auftritt.

  Empfehlung: `LoadDataAsync()` die Exceptions nach oben weitergeben lassen und das Setzen von `_errorMessage` ausschließlich dem Aufrufer übertragen, oder umgekehrt `LoadDataAsync()` als einzige Stelle für Fehlerbehandlung etablieren und den äußeren catch in `OnModeChanged` entfernen.

## Geprüfte Dateien

- `src/Schnittstellenzentrale.Core/Contracts/ApplicationGroupResponse.cs`
- `src/Schnittstellenzentrale.Core/Contracts/ApplicationResponse.cs`
- `src/Schnittstellenzentrale.Core/Contracts/CreateApplicationRequest.cs`
- `src/Schnittstellenzentrale.Core/Interfaces/IApplicationRepository.cs`
- `src/Schnittstellenzentrale.Core/Models/Application.cs`
- `src/Schnittstellenzentrale.Core/Models/ApplicationGroup.cs`
- `src/Schnittstellenzentrale.Infrastructure/Data/AppDbContext.cs`
- `src/Schnittstellenzentrale.Infrastructure/Data/Migrations/AppDbContextModelSnapshot.cs`
- `src/Schnittstellenzentrale.Infrastructure/Data/Migrations/20260518000000_AddIsSystemToApplicationGroupAndApplication.cs`
- `src/Schnittstellenzentrale.Infrastructure/Data/Migrations/20260518000000_AddIsSystemToApplicationGroupAndApplication.Designer.cs`
- `src/Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs`
- `src/Schnittstellenzentrale.Tests/Components/ApplicationContextMenuTests.cs`
- `src/Schnittstellenzentrale.Tests/Helpers/ControllerTestFactory.cs`
- `src/Schnittstellenzentrale.Tests/Integration/ApplicationGroupsControllerIntegrationTests.cs`
- `src/Schnittstellenzentrale.Tests/Integration/ApplicationsControllerIntegrationTests.cs`
- `src/Schnittstellenzentrale/Components/Shared/ApplicationContextMenu.razor`
- `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupContextMenu.razor`
- `src/Schnittstellenzentrale/Components/Shared/ApplicationGroupTree.razor`
- `src/Schnittstellenzentrale/Controllers/ApiControllerBase.cs`
- `src/Schnittstellenzentrale/Controllers/ApplicationGroupsController.cs`
- `src/Schnittstellenzentrale/Controllers/ApplicationsController.cs`
- `src/Schnittstellenzentrale/Program.cs`
