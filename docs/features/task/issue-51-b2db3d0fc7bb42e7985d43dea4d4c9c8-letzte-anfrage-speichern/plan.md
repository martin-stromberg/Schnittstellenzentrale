# Umsetzungsplan: Letzte Anfrage je Endpunkt speichern

## Ziel

Beim Wechsel zwischen Endpunkten innerhalb desselben UI-Scopes soll das zuletzt erzeugte `EndpointExecutionResult` je `Endpoint.Id` erhalten bleiben und beim erneuten Anzeigen dieses Endpunkts wieder in der bestehenden Response-Sektion erscheinen. Der Zustand ist bewusst UI-seitig und fluechtig: kein Speichern in Datenbank, History oder Browser-Storage, keine Wiederherstellung nach Browser-Reload oder Anwendungsneustart.

## Entscheidungen

- Der Cache wird als scoped Service umgesetzt, nicht als Dictionary-Feld in `EndpointPage`. Damit bleibt das Ergebnis innerhalb derselben Blazor-/bUnit-Service-Scope erhalten, auch wenn `EndpointPage` zwischenzeitlich aus dem Renderbaum entfernt und spaeter neu erzeugt wird.
- Gespeichert wird das komplette `EndpointExecutionResult`, inklusive Fehlerergebnissen mit `ErrorMessage`. Alles, was die UI aktuell anzeigen kann, wird damit wiederhergestellt.
- Ein neues Ausfuehrungsergebnis ersetzt immer den Cache-Eintrag fuer dieselbe `Endpoint.Id`.
- Beim Wechsel auf einen Endpunkt ohne Cache-Eintrag bleibt `_result` `null`; es darf kein Ergebnis eines anderen Endpunkts angezeigt werden.
- Keine Invalidierung bei Editieren oder Speichern des Endpunkts in diesem Schritt. Die Anforderung verlangt die Wiederanzeige des letzten Ausfuehrungsergebnisses; fachliche Regeln fuer Verwerfen bei geaenderten Endpunktdaten sind nicht Teil dieses Plans.

## Betroffene Dateien

| Datei | Aenderung |
|-------|-----------|
| `src/Schnittstellenzentrale.Core/Interfaces/IEndpointExecutionResultCache.cs` | Neues Interface fuer UI-seitigen Ergebnis-Cache. |
| `src/Schnittstellenzentrale.Infrastructure/Services/EndpointExecutionResultCache.cs` | Neue scoped Implementierung mit Dictionary `int -> EndpointExecutionResult`. |
| `src/Schnittstellenzentrale/Program.cs` | Registrierung `AddScoped<IEndpointExecutionResultCache, EndpointExecutionResultCache>()`. |
| `src/Schnittstellenzentrale/Components/Shared/EndpointPage.razor` | Cache injizieren, Ergebnis bei Endpunktwechsel wiederherstellen und nach Ausfuehrung aktualisieren. |
| `src/Schnittstellenzentrale.Tests/Components/EndpointPageTests.cs` | bUnit-Registrierung und neue Tests fuer Wiederherstellung, Trennung und Aktualisierung. |

## Implementierungsschritte

1. Neues Interface `IEndpointExecutionResultCache` in `Schnittstellenzentrale.Core.Interfaces` anlegen.
   - `EndpointExecutionResult? Get(int endpointId)`
   - `void Set(int endpointId, EndpointExecutionResult result)`
   - Optional `void Remove(int endpointId)` nur aufnehmen, wenn die Implementierung oder Tests es direkt benoetigen.

2. Neue Implementierung `EndpointExecutionResultCache` in `Schnittstellenzentrale.Infrastructure.Services` anlegen.
   - Intern `Dictionary<int, EndpointExecutionResult>` verwenden.
   - `Get` gibt `null` zurueck, wenn kein Eintrag existiert.
   - `Set` ersetzt vorhandene Eintraege.
   - Keine Kopie des DTO erzwingen; `EndpointExecutionResult` wird heute direkt von der UI konsumiert und nicht mutiert.

3. Service in `Program.cs` registrieren.
   - Registrierung bei den anderen scoped UI-/Session-Services einordnen:
     `builder.Services.AddScoped<IEndpointExecutionResultCache, EndpointExecutionResultCache>();`
   - Sicherstellen, dass die notwendigen Namespaces bereits vorhanden sind oder ergaenzt werden.

4. `EndpointPage.razor` erweitern.
   - `@inject IEndpointExecutionResultCache ExecutionResultCache` ergaenzen.
   - In `OnParametersSetAsync()` beim erkannten `Endpoint.Id`-Wechsel:
     - Nach `LoadModelFromParameter()`, Dirty-/Fehler-Reset und Guard-Aufraeumen `_result = ExecutionResultCache.Get(Endpoint.Id);` setzen.
     - Den bisherigen pauschalen Reset `_result = null` entfernen oder durch diese Cache-Wiederherstellung ersetzen.
   - In `SendRequestAsync()` nach erfolgreichem `ExecuteAsync`:
     - Ergebnis einer lokalen Variable zuweisen.
     - `_result = result;`
     - `ExecutionResultCache.Set(_model.Id, result);`
   - Wenn `GetEndpointByIdAsync(_model.Id)` `null` liefert, keinen Cache aktualisieren.

5. bUnit-Testsetup anpassen.
   - In `EndpointPageTests` einen echten Cache-Service registrieren, z. B. `Services.AddSingleton<IEndpointExecutionResultCache, EndpointExecutionResultCache>();` innerhalb des bUnit-Kontexts. Singleton ist im Testkontext ausreichend, produktiv bleibt der Service scoped.
   - Falls die Implementierung in `Infrastructure.Services` liegt, passenden `using` ergaenzen.

6. Neue bUnit-Tests in `EndpointPageTests` ergaenzen.
   - `Endpunktwechsel_StelltLetztesErgebnisWiederHer`:
     - Endpunkt A rendern und ausfuehren; Mock liefert Response `"response-a"`.
     - Per `cut.SetParametersAndRender(...)` auf Endpunkt B wechseln.
     - Zurueck auf Endpunkt A wechseln.
     - Erwartung: Response-Sektion sichtbar und enthaelt `"response-a"`.
   - `Endpunktwechsel_ZeigtKeinFremdesErgebnis`:
     - Endpunkt A ausfuehren.
     - Auf Endpunkt B wechseln, ohne B auszufuehren.
     - Erwartung: keine `.sz-endpoint-response` oder zumindest kein `"response-a"`.
   - `ErneuteAusfuehrung_AktualisiertGespeichertesErgebnis`:
     - Endpunkt A ausfuehren; erster Mock-Return `"old-response"`.
     - Endpunkt A erneut ausfuehren; zweiter Mock-Return `"new-response"`.
     - Auf B wechseln und zurueck auf A.
     - Erwartung: `"new-response"` sichtbar, `"old-response"` nicht mehr sichtbar.
   - Optionaler vierter Test, falls Aufwand gering:
     - Fehlerresultat mit `ErrorMessage` cachen und nach Endpunktwechsel wieder anzeigen.

7. Testdaten im Test sauber trennen.
   - `CreateEndpoint` so erweitern oder ueberladen, dass unterschiedliche `Id`, `Name` und `RelativePath` fuer A/B erzeugt werden koennen.
   - `GetEndpointByIdAsync(id)` fuer jede verwendete Id explizit mocken.
   - `ExecuteAsync` ueber `SetupSequence` oder ueber Argumentpruefung auf `Endpoint.Id` konfigurieren.

## Akzeptanzkriterien

- Nach Ausfuehrung von Endpunkt A, Wechsel zu Endpunkt B und Rueckkehr zu Endpunkt A wird das letzte Ergebnis von A wieder angezeigt.
- Endpunkt B zeigt kein Ergebnis von A, solange B nicht selbst ausgefuehrt wurde.
- Wird ein Endpunkt erneut ausgefuehrt, ersetzt das neue Ergebnis das bisher gespeicherte Ergebnis fuer diese `Endpoint.Id`.
- Bestehende Response-Komponenten (`ResponseBodyPanel`, `ResponseHeadersPanel`) bleiben unveraendert nutzbar.
- Es gibt bUnit-Tests fuer Wiederherstellung, Nicht-Vermischung und Aktualisierung.

## Testplan

Fokussiert ausfuehren:

```powershell
dotnet test src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj --filter EndpointPageTests
```

Bei unerwarteten Seiteneffekten danach vollstaendig ausfuehren:

```powershell
dotnet test src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj
```

## Risiken und Hinweise

- Der Cache ist fluechtig und scoped. Das ist passend fuer "im selben UI-Scope", loest aber bewusst keine Wiederherstellung nach Reload, neuem Browser-Tab oder App-Neustart.
- Ergebnisse koennen nach spaeteren Endpunkt-Aenderungen veraltet wirken. Falls fachlich gewuenscht, sollte eine separate Invalidierungsanforderung geplant werden.
- Falls bUnit `SetParametersAndRender` dieselbe Komponenteninstanz wiederverwendet, deckt das die direkte Endpunktnavigation ab. Die scoped Service-Wahl deckt zusaetzlich den Fall ab, dass `EndpointPage` im echten Layout entfernt und neu erzeugt wird.

## Offene Punkte

Keine.
