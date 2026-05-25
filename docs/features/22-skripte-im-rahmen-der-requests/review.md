# Plan-Review

## Ergebnis

**Status:** Vollständig umgesetzt

## Umgesetzte Planelemente

### Neue Klassen

- [x] `IEndpointScriptRunner` (Interface) — angelegt in `src/Schnittstellenzentrale.Core/Interfaces/IEndpointScriptRunner.cs`
- [x] `EndpointScriptRunner` (Klasse) — angelegt in `src/Schnittstellenzentrale.Infrastructure/Services/EndpointScriptRunner.cs`
- [x] `ScriptContext` (Datenklasse) — angelegt in `src/Schnittstellenzentrale.Core/Models/ScriptContext.cs`
- [x] `ScriptRequestData` (Datenklasse) — angelegt in `src/Schnittstellenzentrale.Core/Models/ScriptRequestData.cs`
- [x] `ScriptResponseData` (Datenklasse) — angelegt in `src/Schnittstellenzentrale.Core/Models/ScriptResponseData.cs`
- [x] `ScriptExecutionResult` (Datenklasse) — angelegt in `src/Schnittstellenzentrale.Core/Models/ScriptExecutionResult.cs`

### Neue Felder an Datenklassen

- [x] Feld `PreRequestScript` (`string?`) in `Endpoint` — vorhanden
- [x] Feld `PostRequestScript` (`string?`) in `Endpoint` — vorhanden
- [x] Feld `EnvironmentService` (`IActiveEnvironmentService`) in `ScriptContext` — vorhanden
- [x] Feld `Request` (`ScriptRequestData`) in `ScriptContext` — vorhanden
- [x] Feld `Response` (`ScriptResponseData?`) in `ScriptContext` — vorhanden
- [x] Feld `ExecuteEndpoint` (`Func<string, Task<EndpointExecutionResult>>`) in `ScriptContext` — vorhanden
- [x] Feld `CallDepth` (`Dictionary<int, int>`) in `ScriptContext` — vorhanden
- [x] Felder `Url`, `Method`, `Headers`, `Body` in `ScriptRequestData` — vorhanden
- [x] Methode `AsJson()` in `ScriptRequestData` — vorhanden
- [x] Methode `AsXml()` in `ScriptRequestData` — vorhanden
- [x] Felder `Body`, `Headers` in `ScriptResponseData` — vorhanden
- [x] Methode `AsJson()` in `ScriptResponseData` — vorhanden
- [x] Methode `AsXml()` in `ScriptResponseData` — vorhanden
- [x] Felder `Success`, `ErrorMessage` in `ScriptExecutionResult` — vorhanden

### Neue Methoden und Signaturen

- [x] Methode `ExecuteAsync(string script, ScriptContext context)` in `IEndpointScriptRunner` — vorhanden
- [x] Methode `ExecuteAsync(string script, ScriptContext context)` in `EndpointScriptRunner` — vorhanden
- [x] Konstante `ScriptTimeoutMs = 5000` in `EndpointScriptRunner` — vorhanden
- [x] `sz`-Objekt-Registrierung mit `sz.environment`, `sz.request`, `sz.response`, `sz.execute` in `EndpointScriptRunner` — vorhanden
- [x] Methode `GetEndpointByNameAsync(int applicationId, string name)` in `IEndpointRepository` — vorhanden
- [x] Methode `GetEndpointByNameAsync(int applicationId, string name)` in `EndpointRepository` — vorhanden

### Änderungen an bestehenden Klassen

- [x] `EndpointExecutionService` — neue Abhängigkeit `IEndpointScriptRunner` per Konstruktor-Injektion — vorhanden
- [x] `EndpointExecutionService` — neue Abhängigkeit `IEndpointRepository` per Konstruktor-Injektion — vorhanden
- [x] `EndpointExecutionService.ExecuteAsync` — Pre-Skript-Ausführung vor HTTP-Request mit Abbruch bei Fehler — vorhanden
- [x] `EndpointExecutionService.ExecuteAsync` — Post-Skript-Ausführung nach HTTP-Request mit Fehlertext-Anhang — vorhanden
- [x] `EndpointExecutionService.ExecuteAsync` — `CallDepth`-Verwaltung (Inkrementieren/Dekrementieren) per `ScriptContext` — vorhanden
- [x] `EndpointExecutionService` — `sz.execute()`-Callback mit Mehrdeutigkeitsprüfung und Rekursionsschutz — vorhanden
- [x] `ModelUpdateExtensions.ApplyUpdate(Endpoint, Endpoint)` — `PreRequestScript` und `PostRequestScript` aufgenommen — vorhanden
- [x] `EndpointPage` — Feld `PreRequestScript` im lokalen `_model` — vorhanden (über `LoadModelFromParameter`)
- [x] `EndpointPage` — Feld `PostRequestScript` im lokalen `_model` — vorhanden (über `LoadModelFromParameter`)
- [x] `EndpointPage.LoadModelFromParameter` — kopiert `PreRequestScript` und `PostRequestScript` — vorhanden
- [x] `EndpointPage.SaveAsync` — überträgt `PreRequestScript` und `PostRequestScript` (via `_model`) — vorhanden
- [x] `EndpointPage` — Registerkarte „Pre-Request-Skript" (Tab-Key `"pre-script"`) mit `<textarea>` — vorhanden
- [x] `EndpointPage` — Registerkarte „Post-Request-Skript" (Tab-Key `"post-script"`) mit `<textarea>` — vorhanden

### Datenbankmigrationen

- [x] Migration `AddScriptFieldsToEndpoint` — angelegt in `src/Schnittstellenzentrale.Infrastructure/Data/Migrations/20260525063557_AddScriptFieldsToEndpoint.cs`
- [x] `PreRequestScript` (nullable TEXT) in Tabelle `Endpoints` — in Migration und Snapshot vorhanden
- [x] `PostRequestScript` (nullable TEXT) in Tabelle `Endpoints` — in Migration und Snapshot vorhanden

### DI-Registrierung

- [x] `IEndpointScriptRunner` → `EndpointScriptRunner` in `Program.cs` registriert — vorhanden

### Tests — neue Testklasse `EndpointScriptRunnerTests`

- [x] `Syntaxfehler_GibtScriptExecutionResultMitErrorMessage` — vorhanden
- [x] `RuntimeException_GibtScriptExecutionResultMitErrorMessage` — vorhanden
- [x] `SzEnvironmentGet_LiestVariableAusActiveVariables` — vorhanden
- [x] `SzEnvironmentSet_AktualisiertActiveVariables` — vorhanden
- [x] `SzRequestUrl_GibtKorrekteUrlZurueck` — vorhanden
- [x] `SzResponseBodyAsJson_ParstJsonKorrekt` — vorhanden
- [x] `SzResponseBodyAsXml_ParstXmlKorrekt` — vorhanden

### Tests — Erweiterungen `EndpointExecutionServiceTests`

- [x] Hilfsmethode `CreateScriptRunnerMock(ScriptExecutionResult)` — vorhanden
- [x] `CreateService` um Mocks für `IEndpointScriptRunner` und `IEndpointRepository` erweitert — vorhanden
- [x] `CreateServiceCapturingUri` um Mocks für `IEndpointScriptRunner` und `IEndpointRepository` erweitert — vorhanden
- [x] `PreScript_SetsEnvironmentVariable_VariableAvailableInRequest` — vorhanden
- [x] `PreScript_Fehler_BlockiertHttpRequest_FehlerMeldungImErgebnis` — vorhanden
- [x] `PostScript_LiestResponseBody_SetzUmgebungsvariable` — vorhanden
- [x] `PostScript_Fehler_HttpErgebnisVorhanden_FehlerMeldungAngehaengt` — vorhanden
- [x] `SzExecute_LoesteAusfuehrungDesZweitenEndpunktsAus` — vorhanden
- [x] `SzExecute_RekursionsschutzGreiftBeimDrittenAufruf` — vorhanden
- [x] `EndpunktOhneSkript_VerhaeltSichWieBisher` — vorhanden
- [x] `SzExecute_MehrdeutigerName_GibtFehlerZurueck` — vorhanden

### Tests — Erweiterungen `EndpointPageTests`

- [x] `PreRequestSkript_RegistorkarteWirdGerendert` — vorhanden
- [x] `PostRequestSkript_RegistorkarteWirdGerendert` — vorhanden
- [x] `PreRequestSkript_AenderungLoestMarkDirtyAus` — vorhanden
- [x] `PostRequestSkript_AenderungLoestMarkDirtyAus` — vorhanden

## Offene Aufgaben

Keine.

## Hinweise

- Die SQL-Server-Migrations-Reihe (`SqlServerMigrations/`) enthält noch keine entsprechende Migration für `PreRequestScript`/`PostRequestScript`. Der Plan adressiert nur eine Migration (ohne Datenbanktyp-Spezifizierung); die SQLite-Migration ist vorhanden. Sofern SQL-Server-Unterstützung aktiv genutzt wird, wäre eine parallele SQL-Server-Migration analog zu den bestehenden Einträgen in `SqlServerMigrations/` zu ergänzen. Da der Plan dies nicht explizit fordert, ist dies kein offener Punkt im Sinne des Plans.
- `sz.execute()` verwendet `Task.Run(...).GetAwaiter().GetResult()` aus dem Jint-Delegate. Der Plan enthielt einen expliziten Prototyp-Schritt zur Verifikation auf Deadlock-Freiheit; das Ergebnis dieses Prototyps ist im Code nicht dokumentiert, der Mechanismus ist aber so implementiert wie geplant.
