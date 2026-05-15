# Enums

## `StorageMode`
Datei: `src/Schnittstellenzentrale.Core/Enums/StorageMode.cs`

| Wert | Bedeutung |
|---|---|
| `Team` | Teamweite Daten — keine Owner-Filterung, `Owner` bleibt `null` |
| `User` | Benutzerspezifische Daten — Filterung nach `Owner` (Windows-Benutzername), `Owner` wird beim Anlegen gesetzt |
