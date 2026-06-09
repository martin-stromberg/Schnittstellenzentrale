# REST-API und OData v4-API

Die Schnittstellenzentrale stellt zwei API-Oberflächen bereit, über die externe Clients und interne Blazor-Komponenten Anwendungsgruppen, Anwendungen, Endpunktgruppen und Endpunkte verwalten können. Die Authentifizierung erfolgt über ein kurzlebiges Token-Verfahren auf Basis der Windows-Identität des aufrufenden Benutzers.

- **REST-API** unter `/api` — für die Blazor-Komponenten (via `IApplicationApiClient`) und externe HTTP-Clients; unterstützt `X-Storage-Mode`-Filterung und SignalR-Benachrichtigungen.
- **OData v4-API** unter `/odatav4` — für maschinellen Zugriff, `IODataImportService`-Integration und OData-kompatible Clients; liefert ein CSDL-Metadaten-Dokument unter `/odatav4/$metadata`.

## Inhalt

- [Beschreibung](beschreibung.md)
- [Technischer Ablauf](ablauf-technisch.md)
- [API](api.md)
- [OData v4-API](odata-api.md)
- [Installation & Konfiguration](installation.md)
- [Business Rules](business-rules.md)
