# Bestandsaufnahme: Atomares Löschen von Anwendungsgruppen mit Inhalt

Analyse des Löschpfads für `ApplicationGroup` (Controller → Repository → EF-Kaskaden), des `IApplicationApiClient` und der zugehörigen Tests, bezogen auf die Erweiterung von `DELETE /api/application-groups/{id}` um den `content`-Parameter.

## Zusammenfassung

- `ApplicationGroupsController.DeleteAsync` (Z. 160–186) prüft Token, lädt die Gruppe (`404`), blockiert `IsSystem` (`403`) und ruft `_applicationRepository.DeleteGroupAsync(id)` ohne Modusparameter. `NotifyTreeChangedAsync` läuft bei `StorageMode.Team` — bleibt unverändert.
- `ApplicationRepository.DeleteGroupAsync` (Z. 79–88) entfernt nur die Gruppe; enthaltene Anwendungen werden ausschließlich per DB-Regel `DeleteBehavior.SetNull` ungruppiert. **Kein Löschmodus, keine explizite Transaktion vorhanden.**
- EF-Kaskaden (`AppDbContext.cs`): `ApplicationGroup→Applications` = `SetNull`; `Application→Endpoints`/`EndpointGroups`/`Links` = `Cascade`; **`EndpointGroup→ChildGroups` = `Restrict`** — beim `delete`-Modus müssen Kind-Endpunktgruppen vor ihren Eltern entfernt werden (EF ordnet getrackte Deletes korrekt an; alle betroffenen `EndpointGroup`-Entitäten müssen geladen sein, da die DB-Kaskade über `Application` sonst am `Restrict` scheitern kann).
- `ApplicationApiClient.DeleteGroupAsync(id)` (Z. 105–112) sendet `DELETE /api/application-groups/{id}` ohne Query-Parameter; `BuildGetRequest` zeigt das vorhandene Muster für Query-Parameter-Dictionaries.
- Die UI (`WorkspacesLayout.OnDeleteGroupConfirmedAll`/`OnDeleteGroupConfirmedGroupOnly`) führt das Löschen aktuell als nicht-atomare Mehrschritt-Abfolge aus — **Umstellung ausdrücklich erst in Schritt 4**, kein Eingriff in diesem Schritt.
- `ODataApplicationGroupsController.DeleteAsync` (Z. 163) nutzt dasselbe `IApplicationRepository.DeleteGroupAsync` — Signatur muss rückwärtskompatibel bleiben oder der Aufruf angepasst werden.
- `docs/help/api/api.md` dokumentiert `DELETE /api/application-groups/{id}` bislang **gar nicht** (nur `POST`); der Abschnitt muss ergänzt werden.
- Test-Ausgangszustand: siehe [Tests](inventory/tests.md) — Baseline-Lauf dokumentiert.

## Details

- [Datenmodelle und EF-Kaskaden](inventory/models.md)
- [Logik und Abläufe](inventory/logic.md)
- [Interfaces](inventory/interfaces.md)
- [Tests und Test-Ausgangszustand](inventory/tests.md)
