# Tasks: Atomares Löschen von Anwendungsgruppen mit Inhalt (`content`-Parameter)

| # | Bereich | Aufgabe | Status | Testnachweis |
|---|---------|---------|--------|--------------|
| 1 | Datenmodell | Enum `ApplicationGroupDeleteMode` (`Detach`/`Delete`) in `src/Schnittstellenzentrale.Core/Enums/` anlegen | Offen | — |
| 2 | Logik | `IApplicationRepository.DeleteGroupAsync(int id, ApplicationGroupDeleteMode mode)` deklarieren | Offen | — |
| 3 | Logik | `ApplicationRepository.DeleteGroupAsync(id, mode)` transaktional implementieren (detach: `ApplicationGroupId=null`; delete: Endpunkte + Endpunktgruppen + Anwendungen entfernen) | Offen | — |
| 4 | Logik | `ApplicationRepository.DeleteGroupAsync(int id)` auf `Detach`-Overload delegieren | Offen | — |
| 5 | API | `ApplicationGroupsController.DeleteAsync` um `[FromQuery] string? content` erweitern (400/404/403/204 + Modus-Mapping) | Offen | — |
| 6 | API | `IApplicationApiClient.DeleteGroupAsync(int id, ApplicationGroupDeleteMode mode)` deklarieren | Offen | — |
| 7 | API | `ApplicationApiClient.DeleteGroupAsync(id, mode)` mit `?content=` implementieren | Offen | — |
| 8 | Tests | Client-Unit-Tests: `content=delete`/`content=detach`-Query-Parameter in `ApplicationApiClientTests` | Offen | — |
| 9 | Tests | Repository-Tests: `delete`-Kaskade (inkl. verschachtelter Endpunktgruppen), `detach`-Aushängen, Rollback beider Modi (`FailingSaveDbContext`) in `ApplicationRepositoryIntegrationTests` | Offen | — |
| 10 | Tests | Controller-Integrationstests: `content=delete`/`detach`/fehlend/ungültig/Systemgruppe in `ApplicationGroupsControllerIntegrationTests` | Offen | — |
| 11 | Dokumentation | `docs/help/api/api.md`: Abschnitt `DELETE /api/application-groups/{id}` mit `content`-Parameter ergänzen | Offen | — |
