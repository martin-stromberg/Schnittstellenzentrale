# Abnahmeprüfung – Entwicklungsschritt 3

## Ergebnis

**Status:** Anforderung vollständig erfüllt

## Hinweise

- Controller: `src/Schnittstellenzentrale/Controllers/ApplicationGroupsController.cs:170` – `[FromQuery] string? content` wird vor der Existenzprüfung ausgewertet; fehlender/leerer Wert und `detach` (case-insensitiv) → `Detach`, `delete` → `Delete`, sonst `400`. `404` für fehlende Gruppe (Zeile 193-194) und `403` für Systemgruppen (Zeile 196-197) unverändert; SignalR-Benachrichtigung im Team-Modus unverändert (Zeile 201-202).
- Repository: `src/Schnittstellenzentrale.Infrastructure/Repositories/ApplicationRepository.cs:85-121` – beide Modi laufen in einer expliziten Transaktion (`BeginTransactionAsync`/`CommitAsync`). `Delete` entfernt Endpunkte, Endpunktgruppen (inkl. verschachtelter Kindgruppen, EF ordnet Löschungen topologisch – `ParentGroupId` steht auf `DeleteBehavior.Restrict`, AppDbContext.cs:114-117) und Anwendungen; `Detach` setzt `ApplicationGroupId = null`. Der bisherige parameterlose `DeleteGroupAsync(int id)` delegiert auf `Detach` – entspricht dem bisherigen Verhalten (FK `Application.ApplicationGroupId` steht auf `DeleteBehavior.SetNull`, AppDbContext.cs:80-83).
- API-Client: `src/Schnittstellenzentrale/Services/ApplicationApiClient.cs:115-124` – neuer Overload `DeleteGroupAsync(int id, ApplicationGroupDeleteMode mode)` sendet `DELETE /api/application-groups/{id}?content={delete|detach}`; Interface `IApplicationApiClient` um den Overload erweitert (`src/Schnittstellenzentrale.Core/Interfaces/IApplicationApiClient.cs:34-37`). Enum `ApplicationGroupDeleteMode` neu in `src/Schnittstellenzentrale.Core/Enums/ApplicationGroupDeleteMode.cs`.
- Tests: Client-Unit-Tests verifizieren URL inkl. `?content=delete`/`detach` (`ApplicationApiClientTests.cs:308-349`); Repository-Tests decken Kaskade inkl. verschachtelter Endpunktgruppen, Detach-Umhängen sowie Rollback beider Modi bei Save-Fehlschlag über `FailingSaveDbContext` ab (`ApplicationRepositoryIntegrationTests.cs:420-564`); Controller-Integrationstests decken `content=delete`, `detach`, fehlenden Parameter (Detach-Verhalten), ungültigen Wert (`400`) und Systemgruppe (`403`) ab (`ApplicationGroupsControllerIntegrationTests.cs:427-548`).
- Dokumentation: `docs/help/api/api.md` um Abschnitt `DELETE /api/application-groups/{id}` mit `content`-Parameter ergänzt; README-Endpunktabelle und `docs/RELEASE_NOTES.md` aktualisiert.
- Testsuite: `dotnet test --filter "FullyQualifiedName!~Playwright"` → 588/588 bestanden.
- UI-Löschdialog unverändert – gemäß Vorgabe bewusst nicht Teil dieses Schritts (Schritt 4).
