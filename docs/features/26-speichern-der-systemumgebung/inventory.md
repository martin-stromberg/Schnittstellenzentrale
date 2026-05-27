# Bestandsaufnahme: Speichern der Systemumgebungsauswahl im localStorage

Analysiert wurde der Bereich der Umgebungsauswahl und -persistenz, bezogen auf die Anforderung, die gewählte Systemumgebung modusabhängig im `localStorage` zu speichern und beim nächsten Aufruf wiederherzustellen.

## Zusammenfassung

- `EnvironmentSelector.razor` enthält `OnSelectionChanged` bereits vollständig implementiert: schreibt `localStorage.setItem` bei Auswahl und `localStorage.removeItem` bei Abwahl oder fehlender Umgebung; verwendet `LocalStorageKeys.SelectedEnvironmentId(mode)`
- `MainLayout.razor` enthält `RestoreEnvironmentFromLocalStorageAsync(StorageMode)` bereits vollständig implementiert: liest `localStorage`, lädt Umgebung per `GetByIdAsync`, setzt aktive Umgebung oder bereinigt veralteten Eintrag; JSException wird stumm abgefangen
- `MainLayout.OnAfterRenderAsync` ruft `RestoreEnvironmentFromLocalStorageAsync` beim ersten Render bereits auf
- `MainLayout.OnStorageModeChanged` ruft `RestoreEnvironmentFromLocalStorageAsync` bei Moduswechsel bereits auf
- `LocalStorageKeys.SelectedEnvironmentId(StorageMode)` ist als statische Methode bereits vorhanden und gibt den modusabhängigen Schlüssel zurück (z. B. `selectedEnvironmentId_Team`)
- `IActiveEnvironmentService` und `ActiveEnvironmentService` sind vorhanden; `SetActiveEnvironment` wird korrekt aufgerufen
- `ISystemEnvironmentRepository.GetByIdAsync` ist vorhanden und liefert `null` bei nicht gefundener Umgebung — Fallback-Logik ist damit abgedeckt
- `StorageMode`-Enum mit `Team` und `User` ist vorhanden und bestimmt den Schlüsselnamen
- Bestehende Tests in `EnvironmentSelectorTests` und `MainLayoutTests` haben JS-Interop-Mocks für `localStorage`-Operationen eingerichtet, prüfen aber den `localStorage`-Schreib-/Löschaufruf selbst nicht und testen `RestoreEnvironmentFromLocalStorageAsync` noch nicht direkt
- Kein `LocalStorageKeys`-Test vorhanden

## Details

- [Datenmodell](inventory/models.md)
- [Logik](inventory/logic.md)
- [Enums](inventory/enums.md)
- [Interfaces](inventory/interfaces.md)
- [Tests](inventory/tests.md)
