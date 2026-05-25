# Bestandsaufnahme: Skripte für Endpunkte (Pre/Post-Request)

Analysiert wurde der Bereich rund um `Endpoint`, `EndpointExecutionService`, `IActiveEnvironmentService` und `EndpointPage` — die Klassen und Komponenten, die laut Anforderung erweitert oder neu erstellt werden sollen.

---

## Zusammenfassung

- `Endpoint` ist vollständig vorhanden; `PreRequestScript` und `PostRequestScript` fehlen noch im Modell, im DB-Schema und in `LoadModelFromParameter`/`SaveAsync` der `EndpointPage`.
- `EndpointExecutionResult` hat bereits ein `ErrorMessage`-Feld, das von der UI als `alert-danger` angezeigt wird — direkt für Skript-Fehler nutzbar.
- `EndpointExecutionService` enthält die vollständige Platzhalter-Auflösungs- und HTTP-Request-Logik; es fehlt jede Pre-/Post-Skript-Integration sowie die Abhängigkeit zu `IEndpointScriptRunner`.
- `IActiveEnvironmentService` und `ActiveEnvironmentService` sind vorhanden; granulare `get(name)`/`set(name, value)`-Methoden auf Einzelvariablen fehlen — müssen in `EndpointScriptRunner` über `ActiveVariables` und `SetActiveEnvironment` simuliert werden.
- `ModelUpdateExtensions.ApplyUpdate(Endpoint, Endpoint)` muss um die zwei neuen Skriptfelder ergänzt werden.
- Das DB-Schema hat sieben abgeschlossene Migrationen; die neue Migration für `PreRequestScript`/`PostRequestScript` fehlt.
- `EndpointPage` hat vier Request-Registerkarten (`auth`, `headers`, `query`, `body`); die zwei neuen Script-Tabs fehlen vollständig.
- Alle neu zu erstellenden Klassen (`EndpointScriptRunner`, `ScriptContext`, `ScriptRequestData`, `ScriptResponseData`, `ScriptExecutionResult`) und das Interface `IEndpointScriptRunner` existieren noch nicht.
- Die neuen Testklassen `EndpointScriptRunnerTests` sowie die erforderlichen Erweiterungen von `EndpointExecutionServiceTests` und `EndpointPageTests` fehlen noch.

---

## Details

- [Datenmodell](inventory/models.md)
- [Datenbankschicht](inventory/database.md)
- [Interfaces](inventory/interfaces.md)
- [Logik](inventory/logic.md)
- [UI-Komponenten](inventory/ui.md)
- [Tests](inventory/tests.md)
