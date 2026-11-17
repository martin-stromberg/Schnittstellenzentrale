# Detail: Testlandschaft

## Bestehende Abdeckung (muss ohne neue Tests grün bleiben)

### Playwright (`src/Schnittstellenzentrale.Tests/Playwright/`, via `PlaywrightServer`, Port 5099)

- `GroupCrudTests.cs`: `DeleteGroup_Mitloeschen_EntferntGruppeUndAnwendung`, `DeleteGroup_NurSammlungLoeschen_EntferntNurGruppe`, `DeleteGroup_Abbrechen_GruppeBleibtErhalten`, `RenameGroup_AktualisiertNameImBaum` — deckt beide Lösch-Varianten des `ConfirmDeleteGroupDialog` ab.
- `EndpointGroupCrudTests.cs`: `DeleteEndpointGroup_OhneEndpunkte_LoeschtOrdnerAusBaum`, `DeleteEndpointGroup_MitEndpunkten_ZeigtWarnhinweis` (Zählabfrage), `RenameEndpointGroup_AktualisiertNameImBaum`, `RenameEndpointGroup_Abbrechen_BehaeltOriginalenNamen`.
- `SignalRSyncTests.cs`: `BrowserA_CreatesApp_BrowserB_ReceivesViaSignalR` — Team-Notify-Pfad.
- `LayoutSmokeTests.cs`: Layout-Kollaps-Prüfung via `BoundingBoxAsync()`.
- `ApplicationCrudTests.cs`, `NavigationTests.cs`, `TreeCollapseTests.cs`: allgemeine Workspace-Flüsse.

### Komponententests (bUnit, `src/Schnittstellenzentrale.Tests/Components/`)

- `WorkspacesLayout` wird nur gestubbt (`AppShellTests.cs:67`, `TopBarTests.cs:72` via `ComponentFactories.AddStub<WorkspacesLayout>()`); kein Test rendert die Komponente direkt → voraussichtlich keine Testanpassung nötig, höchstens falls ein Test `IEndpointRepository` für das Layout registriert (nicht der Fall).
- Kein Komponententest injiziert `IEndpointRepository` für `WorkspacesLayout`.

### Integrations-/Unit-Tests

- `ApplicationGroupsControllerIntegrationTests.cs`: `content`-Parameter-Tests (Schritt 3) — unverändert.
- `ApplicationApiClientTests` / `ApplicationApiClientIntegrationTests.cs`: `DeleteGroupAsync` mit `content=delete|detach` — unverändert.
- `EndpointRepositoryIntegrationTests.cs`: Repository bleibt bestehen (andere Konsumenten) — unverändert.

## Test-Befehle

- Unit/Integration/Komponenten: `dotnet test --filter "FullyQualifiedName!~Playwright"`
- Playwright: `dotnet test --filter "FullyQualifiedName~Playwright"` (PlaywrightServer startet Kestrel selbst, Port 5099)

## Erwartete Anpassungen

- Keine neuen Tests erforderlich (Anforderung: „ohne neue Tests grün").
- Mögliche Anpassung: falls ein Test-Setup `IEndpointRepository` exklusiv für `WorkspacesLayout` bereitstellt — laut Suche nicht der Fall; Verifikation im Implementierungsschritt.
