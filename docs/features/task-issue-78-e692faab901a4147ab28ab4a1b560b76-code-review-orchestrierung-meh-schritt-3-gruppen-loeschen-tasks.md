# Tasks: Atomares Löschen von Anwendungsgruppen mit Inhalt (`content`-Parameter)

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | Datenmodell | Enum `ApplicationGroupDeleteMode` (`Detach`/`Delete`) in `src/Schnittstellenzentrale.Core/Enums/` anlegen | Erledigt | `ApplicationApiClientTests.DeleteGroupAsync_WithDeleteMode_SendsContentQueryParam` (nutzt das Enum) |
| 2 | Logik | `IApplicationRepository.DeleteGroupAsync(int id, ApplicationGroupDeleteMode mode)` deklarieren | Erledigt | `ApplicationRepositoryIntegrationTests.DeleteGroup_WithDeleteMode_DeletesApplicationsEndpointsAndGroups` |
| 3 | Logik | `ApplicationRepository.DeleteGroupAsync(id, mode)` transaktional implementieren (detach: `ApplicationGroupId=null`; delete: Endpunkte + Endpunktgruppen + Anwendungen entfernen) | Erledigt | `DeleteGroup_WithDeleteMode_DeletesApplicationsEndpointsAndGroups`, `DeleteGroup_WithDetachMode_KeepsApplicationsUngrouped`, `DeleteGroup_WithDeleteMode_OnSaveFailure_RollsBackEverything`, `DeleteGroup_WithDetachMode_OnSaveFailure_RollsBackEverything` |
| 4 | Logik | `ApplicationRepository.DeleteGroupAsync(int id)` auf `Detach`-Overload delegieren | Erledigt | `DeleteGroup_SetsMemberApplicationsGroupless` (bestehend), `DeleteApplicationGroup_WithoutContent_BehavesLikeDetach` |
| 5 | API | `ApplicationGroupsController.DeleteAsync` um `[FromQuery] string? content` erweitern (400/404/403/204 + Modus-Mapping) | Erledigt | `DeleteApplicationGroup_WithContentDelete_RemovesGroupAndApplications`, `..._WithContentDetach_...`, `..._WithoutContent_...`, `..._WithInvalidContent_Returns400`, `..._WithDeleteModeAndSystemGroup_Returns403` |
| 6 | API | `IApplicationApiClient.DeleteGroupAsync(int id, ApplicationGroupDeleteMode mode)` deklarieren | Erledigt | `ApplicationApiClientTests.DeleteGroupAsync_WithDeleteMode_SendsContentQueryParam` |
| 7 | API | `ApplicationApiClient.DeleteGroupAsync(id, mode)` mit `?content=` implementieren | Erledigt | `DeleteGroupAsync_WithDeleteMode_SendsContentQueryParam`, `DeleteGroupAsync_WithDetachMode_SendsContentQueryParam` |
| 8 | Tests | Client-Unit-Tests: `content=delete`/`content=detach`-Query-Parameter in `ApplicationApiClientTests` | Erledigt | Selbst |
| 9 | Tests | Repository-Tests: `delete`-Kaskade (inkl. verschachtelter Endpunktgruppen), `detach`-Aushängen, Rollback beider Modi (`FailingSaveDbContext`) in `ApplicationRepositoryIntegrationTests` | Erledigt | Selbst |
| 10 | Tests | Controller-Integrationstests: `content=delete`/`detach`/fehlend/ungültig/Systemgruppe in `ApplicationGroupsControllerIntegrationTests` | Erledigt | Selbst |
| 11 | Dokumentation | `docs/help/api/api.md`: Abschnitt `DELETE /api/application-groups/{id}` mit `content`-Parameter ergänzen | Erledigt | Kein direkter Test |
