# Anforderung: Verwaltung der Anwendungen

## Fachliche Zusammenfassung

Die `Home`-Seite zeigt bereits die `ApplicationGroupTree`-Komponente in der linken Seitenleiste und rendert eine `ApplicationCard` bei Auswahl einer Anwendung — jedoch sind die dafür benötigten Datensätze (Anwendungen und Gruppen) mangels Erfassungsmaske noch nicht anlegbar. Diese Anforderung ergänzt die Benutzeroberfläche um die Möglichkeit, `ApplicationGroup`- und `Application`-Datensätze direkt in der Anwendung zu erstellen. Das `IApplicationRepository` und der `AppDbContext` sind bereits vollständig implementiert; es fehlen ausschließlich die UI-seitigen Erfassungskomponenten und deren Integration in die bestehende Seitenstruktur.

---

## Betroffene Klassen und Komponenten

### Datenmodellklassen

Keine neuen Klassen erforderlich. `ApplicationGroup` und `Application` sind bereits in `Schnittstellenzentrale.Core` vorhanden und vollständig modelliert.

### Services / Logikklassen

Keine neuen Services erforderlich. `IApplicationRepository` / `ApplicationRepository` decken alle benötigten Schreiboperationen (`AddGroupAsync`, `AddApplicationAsync`) bereits ab.

### UI-Komponenten (Blazor) — neu zu erstellen

| Komponente | Beschreibung |
|---|---|
| `ApplicationGroupEditor` | Formular zum Anlegen einer neuen `ApplicationGroup` (Pflichtfeld: `Name`). |
| `ApplicationEditor` | Formular zum Anlegen einer neuen `Application` (Pflichtfelder: `Name`, `BaseUrl`; optionale Felder: `Description`, `SwaggerUrl`, `MetadataUrl`, `ApplicationGroupId`). |

### UI-Komponenten (Blazor) — zu erweitern

| Komponente | Erweiterung |
|---|---|
| `ApplicationGroupTree` | Schaltflächen „Neue Gruppe" und „Neue Anwendung" hinzufügen, die `ApplicationGroupEditor` bzw. `ApplicationEditor` öffnen; nach Speichern `LoadDataAsync` erneut aufrufen und `StateHasChanged` auslösen. |

### Tests

- Integrationstests für `ApplicationRepository.AddGroupAsync` und `AddApplicationAsync` (SQLite In-Memory) — *sofern noch nicht durch bestehende Tests abgedeckt*.

---

## Implementierungsansatz

- `ApplicationGroupEditor` und `ApplicationEditor` werden als Blazor-Komponenten (`@code`-Block mit `EditForm`) implementiert, analog zu `EndpointEditor`.
- Beide Komponenten erhalten einen `EventCallback OnSaved`-Parameter, über den der Aufrufer nach erfolgreichem Speichern reagieren kann (Datenliste neu laden).
- `ApplicationEditor` lädt beim Initialisieren alle vorhandenen `ApplicationGroup`-Datensätze (über `IApplicationRepository.GetGroupsAsync`) und bietet sie in einem `<select>`-Element zur optionalen Zuordnung an. Der `StorageMode` und der `owner`-Kontext werden dabei berücksichtigt.
- Die Integration in `ApplicationGroupTree` erfolgt über modale Dialoge oder inline aufgeklappte Formularbereiche — analog zur bestehenden `CollapsibleSection`-Struktur. *(Annahme: Inline-Formulare wie bei `EndpointEditor`, nicht modale Dialoge.)*
- Schreiboperationen rufen nach dem Persistieren `ISignalRNotificationService.NotifyGroupChangedAsync` bzw. `NotifyApplicationChangedAsync` auf, sofern `StorageMode.Team` aktiv ist — konsistent mit dem bestehenden Benachrichtigungsmuster.
- Die `Owner`-Eigenschaft der neuen `Application` wird bei `StorageMode.User` mit `ICurrentUserService.GetCurrentUserName()` befüllt, bei `StorageMode.Team` bleibt sie `null`.

---

## Konfiguration

Kein zusätzlicher Konfigurationsbedarf. Die Funktion nutzt ausschließlich bestehende Konfigurationsschlüssel (`DatabaseProvider`, `ConnectionStrings:Default`).

---

## Offene Fragen

1. **Darstellung der Erfassungsformulare:** Sollen `ApplicationGroupEditor` und `ApplicationEditor` inline in der `ApplicationGroupTree`-Seitenleiste erscheinen (wie `EndpointEditor` unterhalb eines Endpunkts), oder soll ein modaler Dialog verwendet werden? Die Seitenleiste ist schmal (col-3); ein modaler Dialog könnte die Benutzererfahrung verbessern.

2. **Gruppenauswahl im `ApplicationEditor`:** Soll die Gruppe beim Anlegen einer neuen Anwendung optional sein (kein Pflichtfeld), oder muss eine Gruppe zwingend ausgewählt werden? Gemäß Datenmodell ist `ApplicationGroupId` optional (`int?`); die UI sollte eine „Ohne Gruppe"-Option bereitstellen.

3. **Pflichtfelder für `Application`:** `Description`, `SwaggerUrl` und `MetadataUrl` sind laut Datenmodell optional. Sollen im Formular Validierungshinweise für diese Felder eingeblendet werden, oder reicht eine rein serverseitige Validierung?

4. **Positionierung der Schaltflächen:** Wo genau in der `ApplicationGroupTree`-Komponente sollen „Neue Gruppe" und „Neue Anwendung" platziert werden — oberhalb der Gruppenliste, unterhalb, oder kontextsensitiv (z. B. „Neue Anwendung" innerhalb einer Gruppenzeile)?

5. **Bearbeiten und Löschen:** Die Anforderung beschreibt ausschließlich das Anlegen. Soll das Bearbeiten und Löschen von Anwendungen und Gruppen ebenfalls im Rahmen dieser Anforderung implementiert werden, oder ist das Gegenstand einer separaten Anforderung?
