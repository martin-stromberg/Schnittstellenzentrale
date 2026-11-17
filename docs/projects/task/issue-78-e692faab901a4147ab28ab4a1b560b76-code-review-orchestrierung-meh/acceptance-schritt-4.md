# Abnahmeprüfung – Entwicklungsschritt 4

## Ergebnis

**Status:** Anforderung vollständig erfüllt

## Abweichungen

Keine.

## Hinweise

Geprüft anhand `git diff task/issue-78-...-code-review-orchestrierung-meh...HEAD` (Commit c7d9b59) sowie Volltext-Lektüre der geänderten Dateien.

- `WorkspacesLayout.razor` (src/Schnittstellenzentrale/Components/Layout/WorkspacesLayout.razor): Die Injektionen `IEndpointRepository`, `IStorageModeService` und `ISignalRNotificationService` sind entfernt (Zeilen 1–5); ein grep über die Datei findet keinerlei Restverweise auf Repository-, SignalR- oder StorageMode-Zugriffe.
- Sämtliche Endpunkt-/Endpunktgruppen-Operationen laufen über `IApplicationApiClient`: `AddEndpointGroupAsync` (Z. 260), `AddEndpointAsync` (Z. 280), `GetEndpointByIdAsync` (Z. 283), `UpdateEndpointGroupAsync` (Z. 318), `GetEndpointsAsync`/`GetEndpointGroupsAsync` für die Zählabfrage (Z. 331–332), `DeleteEndpointGroupAsync` (Z. 370), `DeleteEndpointAsync` (Z. 382).
- Keine komponenteneigene Notify-Entscheidung mehr: Die `StorageMode.Team`-Prüfungen und `Notify*`-Aufrufe sind vollständig entfernt; die Politik liegt im Controller-Pfad (`EndpointsController` Z. 127–128/202–203/234–235, `EndpointGroupsController` Z. 106–107/149–150/181–182, `ApplicationGroupsController` Z. 201–202).
- Gruppen-Löschen mit Inhalt: Beide Bestätigungsvarianten rufen denselben Endpunkt auf — `DeleteGroupAsync(group.Id, ApplicationGroupDeleteMode.Delete)` (Z. 219) bzw. `.Detach` (Z. 231). Der Client serialisiert den Modus als `?content=delete|detach` (ApplicationApiClient.cs Z. 115–123); der Controller mappt den Parameter korrekt (ApplicationGroupsController.cs Z. 170–188). Die mehrstufigen Abläufe (Gruppe neu laden, Anwendungen einzeln löschen/umgruppieren) sind entfallen.
- Das UI-seitige Aktivitätsprotokoll (`IActivityLogService`) ist erhalten (Z. 167, 178, 209, 261, 281, 319).
- Verhaltensänderung beobachtet: `RelativePath` neu angelegter Endpunkte ist jetzt `"/"` statt `string.Empty` (Z. 275) — plausibel im API-Kontext, fachlich unbedenklich.
- Der Breadcrumb-Pfad nach dem Endpunkt-Anlegen wird nun über drei GET-Aufrufe (`GetApplicationByIdAsync`, `GetGroupByIdAsync`, `GetEndpointGroupByIdAsync`) rekonstruiert, da API-Antworten keine Navigationseigenschaften liefern (Z. 288–304) — korrekte Umsetzung.
- Verifikation: `dotnet build` fehlerfrei (0 Warnungen); `dotnet test --filter "FullyQualifiedName!~Playwright"` — 588/588 grün; `dotnet test --filter "FullyQualifiedName~Playwright"` — 64/64 grün.
