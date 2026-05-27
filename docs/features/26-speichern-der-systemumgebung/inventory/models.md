# Datenmodell

## `SystemEnvironment`
Datei: `src/Schnittstellenzentrale.Core/Models/SystemEnvironment.cs`

| Eigenschaft | Typ | Beschreibung / Zweck |
|-------------|-----|----------------------|
| `Id` | `int` | Primärschlüssel, wird als gespeicherter Wert im `localStorage` verwendet |
| `Name` | `string` | Anzeigename der Umgebung im Dropdown |
| `Mode` | `StorageMode` | Bestimmt, ob die Umgebung Team- oder benutzerbezogen ist |
| `Owner` | `string?` | Benutzername des Eigentümers (nur bei `StorageMode.User` gesetzt) |
| `Variables` | `ICollection<EnvironmentVariable>` | Zugehörige Variablen; werden beim Laden via `Include` mitgeladen |
