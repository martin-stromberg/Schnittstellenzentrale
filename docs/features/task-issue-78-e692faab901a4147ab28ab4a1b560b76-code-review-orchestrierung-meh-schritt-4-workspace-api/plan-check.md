# Plan-Prüfung – Workspace-Oberfläche auf API-Pfad umstellen (Issue #89)

Geprüft: `plan.md` gegen `requirement.md` und `inventory.md` (inkl. Detaildokumente).

## Abdeckung der Anforderung

| Anforderungselement | Im Plan adressiert | Fundstelle |
|---------------------|--------------------|------------|
| Alle Endpunkt-/Endpunktgruppen-Operationen über `IApplicationApiClient` | ✅ | WP2, WP3 — Zuordnungstabelle in `inventory/workspaces-layout.md` deckt alle 9 Repository-Aufrufe ab |
| Bestätigungs- und Zählabfragen über API | ✅ | `HandleDeleteEndpointGroupRequested` in WP2 |
| `IEndpointRepository` entfällt vollständig aus der Komponente | ✅ | WP1 (Injektion entfernen) + Verifikation in WP4 |
| Komponenteneigene Notify-Entscheidungen entfallen | ✅ | WP1 entfernt `IStorageModeService`/`ISignalRNotificationService`; alle 5 Notify-Blöcke entfallen |
| Gruppen-Löschen über einzigen Endpunkt mit `content`-Parameter | ✅ | WP1: `Delete` für „Alles löschen", `Detach` für „Nur Gruppe löschen" — korrekte Enum-Richtung (Enum: `Detach`→`detach`, `Delete`→`delete`) |
| Mehrstufige UI-Abläufe entfallen | ✅ | Vorabladen + Schleifen entfallen explizit |
| Aktivitätsprotokoll bleibt | ✅ | WP1/WP2 vermerken Erhalt von `ActivityLogService.Log`; Injektion `IActivityLogService` bleibt |
| Reine Lesezugriffe anderer Komponenten unberührt | ✅ | Scope-Ausschluss `CollectionContentView.razor` + Verifikation in WP4 |
| Nur ein produktiver Ausführungspfad, keine Doppelbenachrichtigungen | ✅ | Controller-Notify ist einzige Notify-Quelle |
| Keine neuen Tests, bestehende grün | ✅ | WP4 führt beide Test-Suiten aus; Tests-Inventar bestätigt Abdeckung aller Flüsse |

## Kritische Punkte

1. **Pfadrekonstruktion nach Endpunkt-Anlage:** Die API liefert keine Navigationsobjekte — Plan beschreibt korrekte Rekonstruktion via `GetApplicationByIdAsync`/`GetGroupByIdAsync`/`GetEndpointGroupByIdAsync`. Die Pfadstruktur `[ApplicationGroup?, Application, EndpointGroup?, Endpoint]` entspricht dem Ist-Zustand (nur direkte Parent-Gruppe). ✅
2. **`EndpointGroup.Application` ist `null!`-Property:** Der gemappte `EndpointGroup` aus der API hat `Application = null` — wird in `WorkspacesLayout` nach Umstellung nirgends dereferenziert (nur `Id`, `Name`, `ApplicationId`, `ParentGroupId` genutzt). ✅
3. **Selektions-Rücksetzung beim Endpunktgruppen-Löschen** hängt an `_deleteTargetEndpoints`/`_deleteTargetAllGroups` — beide werden weiterhin über die API befüllt; Felder bleiben nötig. Im Plan via „Selektions-Rücksetzung … bleibt" erfasst. ✅
4. **`OnDeleteGroupConfirmedAll` lud die Gruppe frisch** (`GetGroupByIdAsync`), um aktuelle `Applications` zu erhalten — entfällt ersatzlos, da der Server atomar arbeitet. ✅
5. **`HttpMethod.GET` beim Endpunkt-Anlegen:** `CreateEndpointRequest` enthält `Method` — wird übertragen; `Core.Enums.HttpMethod` vs. `System.Net.Http.HttpMethod`-Ambiguität in der Razor-Datei bereits durch `Core.Enums.HttpMethod.GET`-Qualifizierung gelöst. ✅

## Testbedarf

- Alle Akzeptanzszenarien sind durch bestehende Playwright-Tests abgedeckt (siehe `inventory/tests.md`): Gruppen-CRUD beider Varianten (`GroupCrudTests`), Endpunktgruppen-CRUD inkl. Zählwarnung (`EndpointGroupCrudTests`), SignalR (`SignalRSyncTests`), Layout (`LayoutSmokeTests`).
- UI-Fluss ist betroffen → E2E-Ausführung verpflichtend (Schritt 10).
- Keine neuen Tests geplant — konform mit der Anforderung.

## Status: Plan vollständig
