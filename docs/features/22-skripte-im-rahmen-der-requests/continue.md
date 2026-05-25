# Offene Aufgaben

Erstellt am: 2026-05-25
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

Keine.

## Code-Review-Befunde

- [ ] **EndpointExecutionService.cs — Fehlerbehandlung**: Bei einem Fehler im Post-Request-Skript wird `result.ErrorMessage` gesetzt, aber `result.Success` bleibt `true`, wenn der HTTP-Call erfolgreich war. Der Aufrufer kann einen Post-Skript-Fehler nicht zuverlässig erkennen. Empfehlung: `result.Success = false` bei `!postResult.Success` setzen.
- [ ] **EndpointExecutionService.cs — Kopplung**: Das `ExecuteEndpoint`-Lambda in `BuildScriptContext` (17 Zeilen) enthält Repository-Lookup, Fehlerprüfung und rekursiven Aufruf inline. Empfehlung: In eine eigene private Methode `ExecuteEndpointByNameAsync` auslagern.
- [ ] **EndpointScriptRunner.cs — Fehlerbehandlung**: `sz.execute` blockiert via `GetAwaiter().GetResult()` synchron. Eine `AggregateException` aus dem Task wird von Jints `catch (JavaScriptException)` nicht erfasst, sodass die Fehlerursache verloren gehen kann. Empfehlung: `AggregateException` explizit entpacken oder Fehlerfall direkt im Ergebnisobjekt signalisieren.
- [ ] **EndpointExecutionServiceTests.cs — Toter Code**: In `SzExecute_RekursionsschutzGreiftBeimDrittenAufruf` wird ein Mock-Service via `CreateService` erstellt, der niemals verwendet wird. Empfehlung: Den ungenutzten Block entfernen.
- [ ] **EndpointExecutionServiceTests.cs — Doppelter Code**: Fünf verschiedene Endpunkt-Factory-Methoden plus mehrere inline-Konstruktionen wiederholen denselben Boilerplate. Empfehlung: Eine einzige flexible `CreateEndpoint`-Überladung mit optionalen Parametern.
- [ ] **EndpointExecutionServiceTests.cs — Testqualität**: `PreScript_SetsEnvironmentVariable_VariableAvailableInRequest` prüft nur den Skript-Aufruf, nicht die beschriebene Auswirkung (Variable im Request verfügbar). Empfehlung: Testinhalt und -name in Einklang bringen.
