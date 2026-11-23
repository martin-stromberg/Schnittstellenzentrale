# Stabilität & Fehlerbehandlung

Die Schnittstellenzentrale härtet ihre Testinfrastruktur gegen Flaky-Tests: Umgebungssensitive Tests sind per `Category=OsInterface`-Trait markiert und laufen getrennt von der blockierenden regulären Lane in einem best-effort-CI-Job, Cleanup-Pfade sind fehlertolerant, und eine TRX-Auswertung macht Flake-Ereignisse über Läufe hinweg sichtbar.

## Inhalt

- [OS-Schnittstellen-Tests](os-interface-tests.md)
