# Release Notes

## Important Notes Before Update

- There are no special notices.

## What's New

- Unified endpoint aggregate update: replacing an endpoint — scalar values plus the full `Headers` and `QueryParameters` sets — now runs as a single server-side operation shared by the REST update endpoint, the OData `PUT`/`PATCH` operations on `Endpoints` and the import apply paths; optimistic concurrency via `RowVersion` is unchanged.
- Atomic deletion of application groups: `DELETE /api/application-groups/{id}` accepts an optional `content` parameter — `delete` removes the group together with all contained applications, endpoints and endpoint groups in one transactional operation; `detach` (default) ungroups the applications and removes only the group. Any failure rolls back the entire operation.
- New REST endpoint `POST /api/applications/{id}/swagger-import/apply` applies a previously computed Swagger import diff to an application with full validation (token, existence, interface type, aggregate integrity, concurrency via `RowVersion`).
- Unified Swagger import execution path: imports and the startup synchronization of system applications now run through the same apply endpoint.
- Unified API execution path in the workspace area: all endpoint and endpoint-group operations (create, rename, delete, count queries) as well as deleting application groups with content now go through the REST API with consistent context headers and a single change-notification policy — removing duplicated notification logic and multi-step UI delete flows.

## Wichtige Hinweise vor dem Update

- Es gibt keine besonderen Hinweise.

## Neuerungen

- Einheitliches Endpunkt-Aggregat-Update: Das Ersetzen eines Endpunkts — Skalarwerte plus die vollständigen Mengen `Headers` und `QueryParameters` — läuft als eine gemeinsame serverseitige Operation, die vom REST-Update-Endpunkt, den OData-`PUT`/`PATCH`-Operationen auf `Endpoints` und den Import-Apply-Pfaden genutzt wird; die Optimistic-Concurrency über `RowVersion` bleibt unverändert.
- Atomares Löschen von Anwendungsgruppen: `DELETE /api/application-groups/{id}` akzeptiert den optionalen Parameter `content` — `delete` entfernt die Gruppe samt aller enthaltenen Anwendungen, Endpunkte und Endpunktgruppen in einer transaktionalen Operation; `detach` (Standard) löst die Anwendungen aus der Gruppe und entfernt nur die Gruppe. Jeder Fehlschlag setzt die gesamte Operation zurück.
- Neuer REST-Endpunkt `POST /api/applications/{id}/swagger-import/apply` wendet einen zuvor berechneten Swagger-Import-Diff mit vollständiger Validierung auf eine Anwendung an (Token, Existenz, Interface-Typ, Aggregat-Integrität, Nebenläufigkeit über `RowVersion`).
- Einheitlicher Swagger-Import-Ausführungspfad: Importe und der Startabgleich von Systemanwendungen laufen über denselben Apply-Endpunkt.
- Einheitlicher API-Ausführungspfad im Workspace-Bereich: Sämtliche Endpunkt- und Endpunktgruppen-Operationen (Anlegen, Umbenennen, Löschen, Zählabfragen) sowie das Löschen von Anwendungsgruppen mit Inhalt laufen über die REST-API mit konsistenten Kontext-Headern und einer einzigen Benachrichtigungs-Politik — doppelte Benachrichtigungslogik und mehrstufige UI-Löschabläufe entfallen.
