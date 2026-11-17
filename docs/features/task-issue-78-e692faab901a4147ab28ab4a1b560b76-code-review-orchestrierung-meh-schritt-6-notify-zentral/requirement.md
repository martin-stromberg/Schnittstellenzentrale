# Übersetzte Anforderung

Issue-Bezug: #91 (Teilschritt 6 von Issue #78 – Code-Review-Orchestrierung)

## Ausgangslage

- Die Entscheidung, ob nach einer mutierenden Änderung eine SignalR-Benachrichtigung ausgelöst wird, ist heute dezentral verteilt: Jeder REST-Controller prüft selbst `StorageMode == Team`, die OData-Controller benachrichtigen gar nicht, und die Endpunkt-Detailseite (`EndpointPage.razor`) feuert nach dem Speichern eine zweite, redundante Benachrichtigung zusätzlich zur Controller-Benachrichtigung.
- Folge: Inkonsistente Abdeckung (OData-Pfade benachrichtigen nie), doppelte Benachrichtigung beim UI-Speichern, und eine in vielen Dateien wiederholte Policy-Prüfung.

## Ziel / fachliche Wirkung

- Die Auslösepolitik wird an **einer zentralen Stelle** definiert: **genau eine** Benachrichtigung pro mutierendem Vorgang und **nur im Team-Speichermodus**.
- Diese Policy wird **einheitlich in allen mutierenden API-Pfaden** angewendet (REST-Controller, OData-Controller, Import-Apply-Endpunkte).
- Jede mutierende Änderung — egal ob über REST-API, OData-API, Oberfläche, Import oder Startabgleich — löst im Team-Modus genau eine Benachrichtigung aus; im Benutzermodus (User) keine.

## Konkreter Umfang

1. **Zentrale Policy**: Eine zentrale Komponente kapselt die Entscheidung „benachrichtigen oder nicht" (Team-Modus-Prüfung) und bildet die mutierende Operation auf die passende Benachrichtigungsart ab:
   - Anwendungen und Anwendungsgruppen → Baum-Benachrichtigung (`NotifyTreeChangedAsync`)
   - Endpunkte → `NotifyEndpointChangedAsync(endpointId, applicationId)`
   - Endpunktgruppen → `NotifyEndpointGroupChangedAsync(endpointGroupId, applicationId)`
2. **OData-Nachrüstung**: Alle mutierenden Operationen aller vier OData-Entity-Sets (Post/Put/Patch/Delete auf `Applications`, `ApplicationGroups`, `Endpoints`, `EndpointGroups`) lösen im Team-Modus dieselbe Benachrichtigungsart aus wie ihre REST-Pendants.
3. **REST-Vereinheitlichung**: Die REST-Controller verwenden die zentrale Policy statt lokaler `if (Team)`-Prüfungen; bislang nicht benachrichtigende mutierende REST-Pfade (Header-/Query-Parameter-Add/Delete auf Endpoints, Import-Apply) benachrichtigen ebenfalls.
4. **Startabgleich**: Der `SystemEndpointSyncService` löst nach tatsächlich angewendeten Änderungen genau eine Benachrichtigung aus.
5. **Dedup**: `EndpointPage.razor` feuert nach erfolgreichem Speichern keine eigene `NotifyEndpointChangedAsync`-Nachricht mehr (der REST-Controller benachrichtigt bereits).
6. **Transport unverändert**: SignalR-Hub, Gruppen und Ereignisnamen bleiben unverändert; es wird ausschließlich die Auslösepolitik vereinheitlicht.

## Akzeptanzkriterien

- Mutierende OData-Operationen aller vier Entity-Sets senden im Team-Modus (`X-Storage-Mode: Team`) genau eine passende Benachrichtigung; ohne Team-Header keine.
- REST-Endpunkte behalten bzw. erhalten dieselbe Policy (genau eine Benachrichtigung, nur Team-Modus), inklusive Header-/Query-Parameter- und Import-Apply-Pfaden.
- Beim Speichern eines Endpunkts aus der Oberfläche wird nur noch eine `EndpointChanged`-Benachrichtigung ausgelöst (Controller-seitig).
- Integrations-Tests verifizieren die Benachrichtigungen auf den mutierenden OData-Pfaden inhaltlich (bisher wurde der Service nur technisch durch einen Mock abgefangen).
- Bestehende Unit-, Integrations- und Playwright-SignalR-Tests bleiben grün.

## Nicht-Ziele

- Keine Änderung am Benachrichtigungs-Transport (Hub, Gruppennamen, Eventnamen, Payloads).
- Keine neuen Benachrichtigungsarten.
- Keine Änderung an Umgebungs-Benachrichtigungen (`EnvironmentChanged`) — Umgebungen gehören nicht zu den vier Entity-Sets dieser Anforderung.
