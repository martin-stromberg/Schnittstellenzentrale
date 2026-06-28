### Fachliche Zusammenfassung

Beim Wechsel zwischen Endpunkten in der Anwendung soll das zuletzt erzeugte Ausführungsergebnis eines Endpunkts erhalten bleiben und beim erneuten Öffnen dieses Endpunkts wieder angezeigt werden. Betroffen ist die Ergebnisanzeige der Endpunkt-Bearbeitung in `EndpointPage`, die aktuell beim Laden eines anderen `Endpoint.Id` durch Zurücksetzen von `_result` geleert wird. Das Verhalten soll pro Endpunkt gelten, damit Anwender zwischen mehreren Endpunkten navigieren können, ohne die letzte Response erneut ausführen zu müssen.

### Betroffene Klassen und Komponenten

- UI-Komponente: `EndpointPage` für Laden eines Endpunkts, Senden einer Anfrage und Anzeigen von `_result`
- UI-Komponenten: `ResponseBodyPanel` und `ResponseHeadersPanel` als bestehende Darstellung des gespeicherten Ergebnisses
- Logik/State: voraussichtlich ein UI-seitiger Cache für `EndpointExecutionResult` je `Endpoint.Id` oder eine vergleichbare Erweiterung innerhalb von `EndpointPage`
- Service: `IEndpointExecutionService` / `EndpointExecutionService` nur indirekt betroffen, da das Ergebnis weiterhin über `ExecuteAsync` erzeugt wird
- Datenklasse: `EndpointExecutionResult` als zu speicherndes Ergebnisobjekt
- Tests: Komponenten- oder Integrationstests für `EndpointPage`-Navigation und Ergebniswiederherstellung

### Implementierungsansatz

`EndpointPage.OnParametersSetAsync()` ist der relevante Erweiterungspunkt, weil dort ein Wechsel der `Endpoint.Id` erkannt und das lokale Modell neu geladen wird. Vor dem Wechsel sollte das vorhandene `_result` dem bisherigen Endpunkt zugeordnet gespeichert werden; nach dem Laden des neuen Endpunkts sollte ein vorhandenes letztes Ergebnis für diese `Endpoint.Id` wieder in `_result` gesetzt werden. `SendRequestAsync()` sollte nach erfolgreicher Ausführung das neue `EndpointExecutionResult` ebenfalls im Cache aktualisieren.

Als naheliegender Ansatz reicht ein UI-seitiger In-Memory-Zustand, zum Beispiel ein `Dictionary<int, EndpointExecutionResult>` in `EndpointPage` oder ein kleiner scoped Service, falls der Zustand Komponentenwechsel oder Seitenwechsel innerhalb der Anwendung überdauern soll. Annahme: "zwischen Endpunkten hin und her" meint den Wechsel innerhalb derselben laufenden Anwendungssitzung, nicht eine Persistierung über Browser-Reloads oder Neustarts hinweg. Das bestehende Rendering der Ergebnis-Sektion kann unverändert bleiben, solange `_result` beim Endpunktwechsel passend wiederhergestellt wird.

### Konfiguration

Keine fachliche Konfiguration ableitbar. Das Verhalten sollte standardmäßig aktiv sein.

### Offene Fragen

- Soll das letzte Ergebnis nur innerhalb derselben `EndpointPage`-Instanz erhalten bleiben oder auch nach Navigation auf andere Seiten, Browser-Reload oder Anwendungsneustart?
- Soll der Ergebnis-Cache pro Benutzer/Browser-Sitzung getrennt sein, falls mehrere Anwender oder Tabs parallel arbeiten?
- Soll das gespeicherte Ergebnis verworfen werden, wenn der Endpunkt gespeichert, geändert oder gelöscht wird?
- Soll nur das letzte sichtbare Ergebnis pro Endpunkt gespeichert werden oder auch Fehlermeldungen aus fehlgeschlagenen Ausführungen?
