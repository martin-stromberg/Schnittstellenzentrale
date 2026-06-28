# Komponenten- und Ablaufdetails

## EndpointPage

`EndpointPage` ist die zentrale Komponente fuer Bearbeiten und Ausfuehren eines Endpunkts.

Wichtige Felder und Services:

- `IEndpointExecutionService ExecutionService` ist in `EndpointPage.razor:2` injiziert.
- `IEndpointExecutionResultCache ExecutionResultCache` ist in `EndpointPage.razor:3` injiziert.
- `_errorMessage` haelt allgemeine UI-Fehler (`EndpointPage.razor:148`).
- `_result` haelt das letzte sichtbare Ausfuehrungsergebnis (`EndpointPage.razor:149`).
- Ein Running-/Loading-Feld existiert aktuell nicht in der Felderliste `EndpointPage.razor:143-157`.

## Rendering vor der Ausfuehrung

Die Kopfzeile enthaelt:

- Namensfeld und Speichern-Button (`EndpointPage.razor:17-23`);
- Fehleranzeige fuer `_errorMessage` (`EndpointPage.razor:26-29`);
- Methode, Pfad und Senden-Button (`EndpointPage.razor:36-45`).

Der Senden-Button:

```razor
<button class="sz-btn-primary sz-btn-send" @onclick="SendRequestAsync">@L["EndpointPage_SendButton"]</button>
```

Fundstelle: `EndpointPage.razor:44`.

Der Button ist nur ueber die Identifikationsklasse `.sz-btn-send` markiert, aber nicht an einen Ausfuehrungszustand gebunden.

## Rendering nach der Ausfuehrung

Der Antwortbereich wird nur bei `_result != null` gerendert:

- `EndpointPage.razor:95-131`
- Metadaten: Status, Dauer, Groesse (`EndpointPage.razor:99-109`)
- Fehler im Ergebnis: `_result.ErrorMessage` (`EndpointPage.razor:112-115`)
- Body/Header-Tabs (`EndpointPage.razor:116-129`)

Das bedeutet: Ohne Ergebnis gibt es keinen Response-Bereich und keinen Zwischenzustand.

## SendRequestAsync

Der aktuelle Ablauf:

1. Dirty-State speichern (`EndpointPage.razor:478-483`).
2. Endpunkt neu laden (`EndpointPage.razor:485`).
3. Fehler setzen und abbrechen, wenn kein Endpunkt geladen wird (`EndpointPage.razor:486-490`).
4. `Application` ergaenzen (`EndpointPage.razor:492-495`).
5. `ExecutionService.ExecuteAsync(refreshed)` awaiten (`EndpointPage.razor:497`).
6. `_result` und Cache setzen (`EndpointPage.razor:498-499`).

Der laufende Zeitraum ist vor allem Schritt 5, kann aber auch Speichern und Nachladen umfassen. Fuer die Anforderung ist entscheidend, dass der Status unmittelbar nach dem Ausloesen sichtbar wird. Deshalb sollte ein Running-Flag vor dem ersten laengeren Await gesetzt werden, nicht erst direkt vor `ExecuteAsync`.

## Fehlerpfade

Bekannte Fehlerpfade:

- Speichern kann `_errorMessage` setzen (`EndpointPage.razor:374-377`).
- Nachladen kann `_errorMessage` setzen (`EndpointPage.razor:486-490`).
- Der Service kann ein Ergebnis mit `ErrorMessage` liefern, das im Antwortbereich erscheint (`EndpointPage.razor:112-115`).
- Allgemeine Exceptions aus `ExecutionService.ExecuteAsync` werden im Service intern in ein Ergebnis gewandelt (`EndpointExecutionService.cs:125-135`), mit Ausnahme von `OperationCanceledException`, die weitergeworfen wird (`EndpointExecutionService.cs:121-124`).

Folgerung: Ein UI-Running-Flag sollte in `try/finally` verwaltet werden, damit es auch bei Pre-Skript-Fehlern, HTTP-Fehlern, Post-Skript-Fehlern, Ladefehlern und Exceptions nicht stehen bleibt.

## Ergebnis-Cache

Beim Laden eines anderen Endpunkts wird das letzte Ergebnis aus dem Cache geholt:

- `EndpointPage.razor:166-173`
- Cache-Implementierung: `EndpointExecutionResultCache.cs:9-17`

Ein laufender Status sollte nicht in diesen Ergebnis-Cache geschrieben werden, weil er kein Ergebnis ist und nicht beim Endpunktwechsel wiederhergestellt werden darf.

## Ausfuehrungsservice

`EndpointExecutionService` ist fuer die fachliche Ausfuehrung zustaendig:

- Oeffentliche Einstiegsmethode: `EndpointExecutionService.cs:67-71`
- interne Ausfuehrungssteuerung: `EndpointExecutionService.cs:81-197`
- Pre-Skript vor HTTP: `EndpointExecutionService.cs:101-110`
- HTTP-Ausfuehrung: `EndpointExecutionService.cs:113-120`
- Post-Skript nach HTTP: `EndpointExecutionService.cs:138-154`
- Logging und History: `EndpointExecutionService.cs:156-190`
- Request-Building: `EndpointExecutionService.cs:309-355`

Fuer eine reine UI-Statusanzeige sollte dieser Service nicht erweitert werden. Er liefert bereits ein abgeschlossenes `EndpointExecutionResult`.

## Nebenlaeufigkeit und Mehrfachklick

Aktuell verhindert nichts einen zweiten Klick auf `Anfrage senden`, waehrend die erste Anfrage laeuft. Da der Handler async ist und der Button nicht deaktiviert wird, koennen mehrere Ausfuehrungen parallel gestartet werden. Das kann zu Race Conditions in `_result` und `ExecutionResultCache.Set(...)` fuehren: das zuletzt zurueckkehrende Ergebnis gewinnt, nicht zwingend die zuletzt gestartete Anfrage.

Die Anforderung fragt offen, ob ein erneutes Ausloesen verhindert werden soll. Aus technischer Sicht waere `disabled="@_isExecuting"` die einfachste Absicherung; ohne diese Sperre sollte zumindest ein eindeutiger Status fuer parallele Ausfuehrungen bedacht werden.
