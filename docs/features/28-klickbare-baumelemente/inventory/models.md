# Datenmodell – Bestandsaufnahme

## `ApplicationGroup`
Datei: `src/Schnittstellenzentrale.Core/Models/ApplicationGroup.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename der Gruppe (wird als `Title` an `CollapsibleSection` übergeben) |
| `IsSystem` | `bool` | Kennzeichnet systemseitig verwaltete Gruppen |
| `RowVersion` | `byte[]` | Optimistisches Sperren |
| `Applications` | `ICollection<Application>` | Untergeordnete Anwendungen |

---

## `EndpointGroup`
Datei: `src/Schnittstellenzentrale.Core/Models/EndpointGroup.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel |
| `Name` | `string` | Anzeigename des Ordners (wird in `RenderEndpointGroup` als `@group.Name` gerendert) |
| `ApplicationId` | `int` | Fremdschlüssel zur übergeordneten Anwendung |
| `Application` | `Application` | Navigationseigenschaft |
| `ParentGroupId` | `int?` | Fremdschlüssel zur übergeordneten Endpunktgruppe (null = Wurzelordner) |
| `ParentGroup` | `EndpointGroup?` | Navigationseigenschaft zur übergeordneten Gruppe |
| `RowVersion` | `byte[]` | Optimistisches Sperren |
| `Endpoints` | `ICollection<Endpoint>` | Direkt zugeordnete Endpunkte |
| `ChildGroups` | `ICollection<EndpointGroup>` | Untergeordnete Gruppen |

Hinweis: `ApplicationGroupTree` verwendet `ParentGroupId` zur Unterscheidung von Wurzel- und Kindordnern. Es existiert kein Zustandsfeld, das angibt, ob ein Ordner auf- oder zugeklappt ist.
