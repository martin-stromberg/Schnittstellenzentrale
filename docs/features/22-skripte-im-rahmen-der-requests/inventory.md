# Bestandsaufnahme: Skripte im Rahmen der Requests — Persistierung von Umgebungsvariablen

Analysiert wurde der Code rund um die Skriptausführung bei Endpunkt-Requests, bezogen auf die Anforderung, dass `sz.environment.set` Variablenänderungen bei aktiver Systemumgebung auch in der Datenbank persistiert.

## Zusammenfassung

- `EndpointScriptRunner.BuildEnvironmentObject` enthält das `sz.environment.set`-Lambda; es aktualisiert den In-Memory-Zustand via `context.EnvironmentService.SetActiveEnvironment`, schreibt aber **nicht** in die Datenbank.
- `ScriptContext` besitzt derzeit **keine** Eigenschaft `ISystemEnvironmentRepository`; `EndpointExecutionService.BuildScriptContext` injiziert dieses Repository folglich nicht.
- `IActiveEnvironmentService.SetActiveEnvironment` ist synchron; eine async-Variante existiert nicht.
- `ISystemEnvironmentRepository.UpdateAsync` ist vollständig implementiert und berücksichtigt beim Update auch `IsValueMasked`-Felder bestehender Variablen.
- Beim Neuaufbau der Variablenliste in `sz.environment.set` werden neue `EnvironmentVariable`-Objekte ohne `Id` und ohne `IsValueMasked`-Übernahme konstruiert — der ursprüngliche Maskierungsstatus geht damit verloren.
- Das Muster für blockierende async-Aufrufe aus synchronen Jint-Lambdas ist mit `sz.execute` bereits etabliert (`Task.Run(...).GetAwaiter().GetResult()`).
- Testabdeckung für `sz.environment.set` existiert nur für den Fall **ohne** aktive Systemumgebung; der Persistierungsfall (aktive Umgebung vorhanden) ist noch nicht getestet.
- Kein Integrationstest prüft nach Skriptausführung den Datenbankstand der Umgebungsvariablen.

## Details

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Interfaces](inventory/interfaces.md)
- [Tests](inventory/tests.md)
