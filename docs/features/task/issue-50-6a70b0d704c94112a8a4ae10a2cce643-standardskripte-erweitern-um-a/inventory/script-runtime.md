# Skript-Laufzeit und `sz`-API

## Relevante Stellen

- `src/Schnittstellenzentrale.Infrastructure/Services/EndpointScriptRunner.cs`
- `src/Schnittstellenzentrale.Core/Models/ScriptContext.cs`
- `src/Schnittstellenzentrale.Core/Models/ScriptExecutionResult.cs`
- `src/Schnittstellenzentrale.Core/Models/ScriptRequestData.cs`
- `src/Schnittstellenzentrale.Core/Models/ScriptResponseData.cs`
- `src/Schnittstellenzentrale.Core/Helpers/ScriptBodyParser.cs`

## Aktueller Zustand

`EndpointScriptRunner` nutzt Jint und registriert das globale Objekt `sz` mit den Teilobjekten:

- `sz.environment.get(name)`
- `sz.environment.set(name, value)`
- `sz.request`
- `sz.response` im Post-Request-Skript
- `sz.console.write(text)`
- `sz.execute(name)`

`sz.execute(name)` ruft intern `context.ExecuteEndpoint(name)` auf und gibt derzeit ein JS-Objekt mit diesen Feldern zurueck:

- `success`
- `statusCode`
- `responseBody`
- `errorMessage`

`ScriptExecutionResult` modelliert nur den Erfolg der Skriptausfuehrung selbst, nicht den Erfolg des aufgerufenen Endpunkts.

## Relevante Folgerung fuer die Aenderung

Die neue Anforderung veraendert den Rueckgabevertrag von `sz.execute(name)` direkt und erfordert zusaetzlich ein neues API-Verhalten fuer `sz.repeat()`.

## Testabdeckung

Vorhandene Tests decken bereits ab:

- Syntax- und Runtime-Fehler in Skripten
- Zugriff auf `sz.environment`, `sz.request`, `sz.response`, `sz.console`
- Rekursionsschutz fuer verschachtelte `sz.execute`-Aufrufe

Es fehlt aktuell eine explizite Testabdeckung fuer:

- Boolean-Rueckgabewert von `sz.execute(name)`
- Wiederholung des aktuellen Endpunkts ueber `sz.repeat()`
- Ausschluss des Authenticate-Endpunkts von dieser Logik
