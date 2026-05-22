# Datenmodell

## `Endpoint`
Datei: `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename des Endpunkts |
| `Method` | `HttpMethod` | HTTP-Methode (GET, POST, ...) |
| `RelativePath` | `string` | Relativer URL-Pfad |
| `Body` | `string?` | Anfrage-Body (Text) |
| `AuthenticationType` | `AuthenticationType` | Authentifizierungsart |
| `ApplicationId` | `int` | Fremdschlüssel auf `Application` |
| `Application` | `Application` | Navigationseigenschaft zur Anwendung |
| `EndpointGroupId` | `int?` | Optionaler Fremdschlüssel auf `EndpointGroup` |
| `EndpointGroup` | `EndpointGroup?` | Navigationseigenschaft zur Gruppe |
| `RowVersion` | `byte[]` | Optimistic-Concurrency-Token |
| `Headers` | `ICollection<EndpointHeader>` | HTTP-Header des Endpunkts |
| `QueryParameters` | `ICollection<EndpointQueryParameter>` | Query-Parameter des Endpunkts |

**Fehlendes Feld laut Anforderung:** `BodyMode` (`BodyMode`-Enum) ist noch nicht vorhanden.

---

## `EndpointGroup`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointGroup.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename der Gruppe |
| `ApplicationId` | `int` | Fremdschlüssel auf `Application` |
| `Application` | `Application` | Navigationseigenschaft |
| `RowVersion` | `byte[]` | Optimistic-Concurrency-Token |
| `Endpoints` | `ICollection<Endpoint>` | Enthaltene Endpunkte |

---

## `EndpointExecutionResult`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointExecutionResult.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Success` | `bool` | Gibt an, ob die Anfrage erfolgreich war |
| `StatusCode` | `int?` | HTTP-Statuscode der Antwort |
| `RequestDetails` | `string?` | Zusammenfassung der gesendeten Anfrage (Methode + URL) |
| `ResponseBody` | `string?` | Antwort-Body als Text |
| `ErrorMessage` | `string?` | Fehlerbeschreibung bei Verbindungsproblemen |

**Fehlende Felder laut Anforderung:** `ResponseHeaders` (`IDictionary<string, string>`), `DurationMs` (`long?`) und `ResponseSizeBytes` (`long?`) sind noch nicht vorhanden.

---

## `EndpointHeader`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointHeader.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Key` | `string` | Header-Name |
| `Value` | `string` | Header-Wert |
| `EndpointId` | `int` | Fremdschlüssel auf `Endpoint` |
| `Endpoint` | `Endpoint` | Navigationseigenschaft |

---

## `EndpointQueryParameter`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointQueryParameter.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Key` | `string` | Parameter-Name |
| `Value` | `string` | Parameter-Wert |
| `EndpointId` | `int` | Fremdschlüssel auf `Endpoint` |
| `Endpoint` | `Endpoint` | Navigationseigenschaft |
