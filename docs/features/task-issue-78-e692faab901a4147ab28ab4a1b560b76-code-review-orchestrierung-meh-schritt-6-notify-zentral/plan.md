# Umsetzungsplan: Zentrale SignalR-Benachrichtigungs-Policy

Issue-Bezug: #91 · Basisbranch: `task/issue-78-e692faab901a4147ab28ab4a1b560b76-code-review-orchestrierung-meh`

## Scope-Klärung

**Verarbeitungseinheit**: Ein mutierender Vorgang = ein erfolgreicher mutierender API-Request (REST oder OData) bzw. ein abgeschlossener Hintergrundvorgang (Import-Apply, Startabgleich). Pro Vorgang wird **genau eine** Benachrichtigung ausgelöst.

**Auslösende Ereignisse**: POST/PUT/PATCH/DELETE auf den vier Entity-Sets, REST-Kindmutationen (Header/Query-Parameter), Import-Apply-Endpunkte, `SystemEndpointSyncService` bei tatsächlichen Änderungen.

**Erwartete Wirkung**: Im Team-Speichermodus (`X-Storage-Mode: Team`) wird nach erfolgreicher Persistierung genau eine Benachrichtigung gesendet (Baum-Benachrichtigung für Anwendungen/Anwendungsgruppen/Import/Abgleich; entitätsspezifisch für Endpunkte/Endpunktgruppen). Im Benutzermodus und bei fehlgeschlagenen Vorgängen (4xx/5xx vor Persistierung) keine.

**Policy-Mapping** (einmalig zentral definiert):

| Mutation | Benachrichtigung |
|----------|------------------|
| Application (jede) | `NotifyTreeChangedAsync` |
| ApplicationGroup (jede) | `NotifyTreeChangedAsync` |
| Endpoint (inkl. Header/Query-Parameter-Kindmutationen) | `NotifyEndpointChangedAsync(endpointId, applicationId)` |
| EndpointGroup (jede) | `NotifyEndpointGroupChangedAsync(endpointGroupId, applicationId)` |
| Import-Apply / Startabgleich (mehrentitäts-übergreifend) | `NotifyTreeChangedAsync` (eine einzige Nachricht) |

**Umgang mit nicht vorhandenen Objekten**: Existiert die Ziel-Entität nicht, endet der Vorgang mit `NotFound` vor Persistierung → keine Benachrichtigung (unverändertes Verhalten, keine neuen Objekte erzeugen).

### Scope-Ausschlüsse

Keine — die Anforderung benennt Transport-Fixiertheit und die vier Entity-Sets explizit; Umgebungs-Benachrichtigungen (`EnvironmentChanged`) sind kein Scope-Ausschluss im Sinne einer Nutzererwartung, sondern fachlich eigenständiger Mechanismus außerhalb der vier genannten Entity-Sets.

### Entscheidungen (aus Anforderung abgeleitet)

- **Header-/Query-Parameter-REST-Pfade** benachrichtigen neu: Sie sind mutierende REST-Pfade auf dem Endpunkt-Aggregat; die fachliche Wirkung („jede mutierende Änderung") verlangt Abdeckung. Ein UI-Create-Flow (Endpoint + Header + Parameter) erzeugt dabei je API-Call eine Nachricht — jeder Call ist ein eigener mutierender Vorgang.
- **Import-Apply** benachrichtigt einmalig per `TreeChanged` (Diff kann viele Entitäten betreffen; genau eine Nachricht pro Vorgang), nur wenn der Diff tatsächliche Änderungen enthält.
- **Startabgleich**: kein Request-Kontext → Benachrichtigung ohne StorageMode-Prüfung, da ausschließlich freigegebene Systemdaten betroffen sind; nur bei tatsächlich angewendeten Änderungen (Diff nicht leer, kein `ErrorMessage`).
- **`NotifyApplicationChangedAsync`/`NotifyGroupChangedAsync`** bleiben ungenutzt (kein Client-Handler, Transport unverändert).

## Architektur

Neue zentrale Policy-Komponente, die die einzige Stelle mit der Team-Modus-Entscheidung ist:

- `src/Schnittstellenzentrale.Core/Interfaces/INotificationPolicy.cs` (neu):

```csharp
public interface INotificationPolicy
{
    Task NotifyTreeMutationAsync(StorageMode storageMode);
    Task NotifyEndpointMutationAsync(StorageMode storageMode, int endpointId, int applicationId);
    Task NotifyEndpointGroupMutationAsync(StorageMode storageMode, int endpointGroupId, int applicationId);
    Task NotifyBackgroundMutationAsync(); // Hintergrundvorgänge ohne Request-Kontext (Startabgleich)
}
```

- `src/Schnittstellenzentrale.Infrastructure/Services/NotificationPolicy.cs` (neu): implementiert das Interface, delegiert an `ISignalRNotificationService`; Team-Prüfung genau einmal hier.
- `Program.cs`: `AddScoped<INotificationPolicy, NotificationPolicy>()` neben der bestehenden `ISignalRNotificationService`-Registrierung.

## Umsetzungsschritte

1. `INotificationPolicy` + `NotificationPolicy` anlegen; in `Program.cs` registrieren.
2. REST-Controller auf Policy umstellen (`ISignalRNotificationService`-Feld/Ktor-Param durch `INotificationPolicy` ersetzen):
   - `ApplicationsController`, `ApplicationGroupsController` → `NotifyTreeMutationAsync(context.StorageMode)` / `storageMode`
   - `EndpointsController` → `NotifyEndpointMutationAsync(...)` in Create/Update/Delete; **neu** in `AddHeaderAsync`, `DeleteHeaderAsync`, `AddQueryParameterAsync`, `DeleteQueryParameterAsync` (applicationId über `EndpointId` → `GetEndpointByIdAsync` auflösen)
   - `EndpointGroupsController` → `NotifyEndpointGroupMutationAsync(...)`
3. `ApplicationImportController`: `INotificationPolicy` injizieren; nach erfolgreichem `ApplyDiffAsync` im Team-Modus `NotifyTreeMutationAsync(storageMode)` aufrufen, sofern `diff.NewEndpoints/ChangedEndpoints/RemovedEndpoints` nicht leer sind; `ParseStorageMode()` ergänzen.
4. OData-Controller: `INotificationPolicy` in alle vier Controller injizieren; nach erfolgreicher Persistierung in Post/Put/Patch/Delete die passende Policy-Methode mit `ParseStorageMode()` aufrufen.
5. `SystemEndpointSyncService`: `INotificationPolicy` aus dem Scope auflösen; nach `SyncFromDocumentAsync` bei nicht-leerem Diff ohne `ErrorMessage` `NotifyBackgroundMutationAsync()` aufrufen.
6. `EndpointPage.razor`: `OnSaveSuccessAsync` — SignalR-Aufruf (Z. 445–446) und `@inject ISignalRNotificationService` (Z. 7) entfernen.
7. `ControllerTestFactory`: Mock exponieren (`public Mock<ISignalRNotificationService> SignalRNotificationMock { get; }`), damit Integrationstests verifizieren können.
8. Bestehende Tests anpassen:
   - `ODataControllerNullApplicationTests`: direkte `new ODataEndpointsController(...)`/`new ODataEndpointGroupsController(...)`-Aufrufe um `Mock<INotificationPolicy>`-Parameter ergänzen.
   - `SystemEndpointSyncServiceTests`: `INotificationPolicy`-Mock im `ServiceCollection` registrieren (Service löst die Policy per `GetRequiredService` auf); ggf. Verify für nicht-leeren Diff.
   - `EndpointPageTests`/`AppShellTests`/`TopBarTests`: `ISignalRNotificationService`-Registrierungen bleiben unverändert (werden vom Production-Code nicht mehr benötigt, schaden aber nicht).
9. Tests:
   - Neu `tests/Integration/ODataNotificationIntegrationTests.cs`: für alle vier Entity-Sets je mutierende Operation mit `X-Storage-Mode: Team` → `Verify` auf genau einen korrekten Notify-Aufruf; ohne Team-Header → `Verify(Times.Never)`. Pro Test `SignalRNotificationMock.Invocations.Clear()`.
   - Neu `tests/Unit/NotificationPolicyTests.cs`: Team → inner aufgerufen; User → nie; Background → immer.
   - REST-Neubenetachrichtigungen (Header-Add/Delete, Import-Apply) in passende bestehende Integrations-Testdateien aufnehmen.

## Akzeptanzszenarien

**Happy Path (externer Auslöser → sichtbare Wirkung)**
1. Client im Team-Modus sendet `POST /odatav4/Applications` → Anwendung persistiert, genau eine `TreeChanged`-Benachrichtigung; ein zweiter Client sieht die Anwendung ohne Reload (bestehender Playwright-Test deckt REST-Pendant).
2. `PATCH /odatav4/Endpoints({id})` mit Team-Header → genau eine `EndpointChanged`-Benachrichtigung mit `(endpointId, applicationId)`.
3. Derselbe Request ohne `X-Storage-Mode`-Header → Persistierung erfolgt, **keine** Benachrichtigung.
4. UI-Speichern eines Endpunkts im Team-Modus → genau eine `EndpointChanged`-Nachricht (nur Controller-seitig).
5. `POST /api/applications/{id}/odata-import/apply` mit nicht-leerem Diff im Team-Modus → genau eine `TreeChanged`-Nachricht.
6. Startabgleich mit Änderungen → genau eine `TreeChanged`-Nachricht.

**Fehler-/Randpfade**
- Mutation auf nicht vorhandene Entität → `NotFound`, keine Benachrichtigung.
- Mutation auf Systementität → `403`, keine Benachrichtigung.
- PATCH ohne `rowVersion` → `400`, keine Benachrichtigung.
- Apply mit leerem Diff → `204`, keine Benachrichtigung.
- User-Modus (`X-Storage-Mode: User` oder fehlender Header) → Persistierung erfolgt, keine Benachrichtigung.

## Testplan

- Unit: `NotificationPolicyTests` (Policy-Mapping + Team-Gate + Background).
- Integration: `ODataNotificationIntegrationTests` (16 mutierende OData-Ops × Team/User); Ergänzungen für Header-/Parameter- und Import-Apply-Benachrichtigung auf REST.
- Playwright: `SignalRSyncTests` bleibt grün (reale E2E-Abnahme über Kestrel-Server — UI-Fluss „App anlegen in Browser A → sichtbar in Browser B").
- Vollsuite: `CI=true dotnet test --filter "FullyQualifiedName!~Playwright"` sowie Playwright-Teilmenge.

## Offene Punkte

Keine.
