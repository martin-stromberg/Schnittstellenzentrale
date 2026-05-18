# Datenmodell

## `ApplicationGroup`
Datei: `src/Schnittstellenzentrale.Core/Models/ApplicationGroup.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Pflichtfeld, max. 200 Zeichen (laut `AppDbContext`) |
| `RowVersion` | `byte[]` | Concurrency-Token für optimistisches Sperren |
| `Applications` | `ICollection<Application>` | Navigationseigenschaft zu den zugehörigen Anwendungen |

---

## `Application`
Datei: `src/Schnittstellenzentrale.Core/Models/Application.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|---|---|---|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Pflichtfeld, max. 200 Zeichen (laut `AppDbContext`) |
| `Description` | `string` | Optionale Beschreibung |
| `BaseUrl` | `string` | Pflichtfeld, max. 500 Zeichen (laut `AppDbContext`) |
| `InterfaceUrl` | `string?` | Optionale Schnittstellen-URL (Swagger/OData), max. 500 Zeichen |
| `InterfaceType` | `InterfaceType` | Wird aus `InterfaceUrl` abgeleitet via `DetectInterfaceType` |
| `Owner` | `string?` | Benutzername (Windows-Domain), max. 256 Zeichen; gesetzt im User-Modus |
| `ApplicationGroupId` | `int?` | Fremdschlüssel auf `ApplicationGroup`, nullable |
| `ApplicationGroup` | `ApplicationGroup?` | Navigationseigenschaft zur Gruppe |
| `RowVersion` | `byte[]` | Concurrency-Token für optimistisches Sperren |
| `Endpoints` | `ICollection<Endpoint>` | Navigationseigenschaft zu den Endpunkten |
| `EndpointGroups` | `ICollection<EndpointGroup>` | Navigationseigenschaft zu den Endpunkt-Gruppen |

Statische Hilfsmethode: `DetectInterfaceType(string? url)` — leitet `InterfaceType` anhand der URL-Inhalte ab.

Instanzmethode: `Clone()` — erstellt eine flache Kopie ohne Navigationseigenschaften `Endpoints` und `EndpointGroups`.

---

## `AppDbContext`
Datei: `src/Schnittstellenzentrale.Infrastructure/Data/AppDbContext.cs`

Relevante Konfigurationen aus `OnModelCreating`:

- `ApplicationGroup.Name`: `IsRequired()`, `HasMaxLength(200)`
- `ApplicationGroup.RowVersion`: `IsConcurrencyToken()`
- `ApplicationGroup` → `Application`: `OnDelete(DeleteBehavior.SetNull)`
- `Application.Name`: `IsRequired()`, `HasMaxLength(200)`
- `Application.BaseUrl`: `IsRequired()`, `HasMaxLength(500)`
- `Application.InterfaceUrl`: `HasMaxLength(500)`
- `Application.Owner`: `HasMaxLength(256)`
- `Application.RowVersion`: `IsConcurrencyToken()`

`SaveChangesAsync` / `SaveChanges` rufen `UpdateRowVersions()` auf, das für alle geänderten Entities `RowVersion` auf `Guid.NewGuid().ToByteArray()` setzt.
