# Datenmodell

## `Endpoint`
Datei: `src/Schnittstellenzentrale.Core/Models/Endpoint.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename des Endpunkts |
| `Method` | `HttpMethod` | HTTP-Methode (GET, POST, …) |
| `RelativePath` | `string` | Relativer Pfad inklusive Pfad-Platzhalter |
| `Body` | `string?` | Request-Body (optional) |
| `BodyMode` | `BodyMode` | Body-Format (None, Json, Xml, PlainText) |
| `AuthenticationType` | `AuthenticationType` | Authentifizierungsart |
| `ApplicationId` | `int` | FK zur zugehörigen Anwendung |
| `Application` | `Application` | Navigationseigenschaft |
| `EndpointGroupId` | `int?` | FK zur optionalen Endpunktgruppe |
| `EndpointGroup` | `EndpointGroup?` | Navigationseigenschaft |
| `RowVersion` | `byte[]` | Optimistisches Concurrency-Token |
| `Headers` | `ICollection<EndpointHeader>` | Request-Header |
| `QueryParameters` | `ICollection<EndpointQueryParameter>` | Query-/Pfad-Parameter |

Die Felder `PreRequestScript` (`string?`) und `PostRequestScript` (`string?`) sind **noch nicht vorhanden**.

---

## `EndpointExecutionResult`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointExecutionResult.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Success` | `bool` | Gibt an, ob der Request erfolgreich war |
| `StatusCode` | `int?` | HTTP-Statuscode der Antwort |
| `RequestDetails` | `string?` | Kurzdarstellung der gesendeten Anfrage |
| `ResponseBody` | `string?` | Antwort-Body als Text |
| `ErrorMessage` | `string?` | Fehlermeldung (Verbindungsfehler, Exception) |
| `ResponseHeaders` | `IDictionary<string, string>?` | Antwort-Header |
| `DurationMs` | `long?` | Ausführungsdauer in Millisekunden |
| `ResponseSizeBytes` | `long?` | Größe des Antwort-Body in Bytes |

`ErrorMessage` ist bereits vorhanden und wird von der UI (`EndpointPage`) als `alert-danger` angezeigt — nutzbar für Skript-Fehler. Das Feld könnte für Pre- und Post-Skript-Fehler genutzt werden.

---

## `SystemEnvironment`
Datei: `src/Schnittstellenzentrale.Core/Models/SystemEnvironment.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Name der Umgebung |
| `Mode` | `StorageMode` | Speichermodus (User/Team) |
| `Owner` | `string?` | Besitzer bei User-Modus |
| `Variables` | `ICollection<EnvironmentVariable>` | Zugehörige Variablen |

---

## `EnvironmentVariable`
Datei: `src/Schnittstellenzentrale.Core/Models/EnvironmentVariable.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Variablenname |
| `Value` | `string` | Variablenwert |
| `IsValueMasked` | `bool` | Gibt an, ob der Wert maskiert angezeigt wird |
| `SystemEnvironmentId` | `int` | FK zur zugehörigen `SystemEnvironment` |
| `SystemEnvironment` | `SystemEnvironment?` | Navigationseigenschaft |
