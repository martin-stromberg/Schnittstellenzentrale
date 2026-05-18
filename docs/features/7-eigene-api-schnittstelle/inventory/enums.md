# Enums

## `StorageMode`
Datei: `src/Schnittstellenzentrale.Core/Enums/StorageMode.cs`

| Wert | Bedeutung |
|---|---|
| `Team` | Alle Applications und Gruppen sind für alle Benutzer sichtbar (kein Owner-Filter) |
| `User` | Nur eigene Applications (gefiltert nach `Owner`) und deren Gruppen sind sichtbar |

Wird als Parameter in `IApplicationRepository`-Methoden übergeben. Aktueller Wert wird von `IStorageModeService.CurrentMode` geliefert.

---

## `InterfaceType`
Datei: `src/Schnittstellenzentrale.Core/Enums/InterfaceType.cs`

| Wert | Bedeutung |
|---|---|
| `Unknown` (0) | Schnittstellentyp konnte nicht erkannt werden |
| `Rest` (1) | REST-Schnittstelle (erkannt an „swagger" oder „openapi" in der URL) |
| `OData` (2) | OData-Schnittstelle (erkannt an „$metadata" in der URL) |

Wird per `Application.DetectInterfaceType(string? url)` aus der `InterfaceUrl` abgeleitet und in `Application.InterfaceType` gespeichert.
