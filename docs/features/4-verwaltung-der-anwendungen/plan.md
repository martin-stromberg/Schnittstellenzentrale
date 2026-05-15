# Umsetzungsplan: Verwaltung der Anwendungen

## Übersicht

Es werden zwei neue Blazor-Komponenten (`ApplicationGroupEditor` und `ApplicationEditor`) als `EditForm`-basierte Inline-Formulare erstellt, analog zu `EndpointEditor`. Die bestehende Komponente `ApplicationGroupTree` wird um zwei Schaltflächen und die zugehörige Anzeigelogik für die neuen Formulare erweitert. Zusätzlich werden Integrationstests für `AddGroupAsync` und `AddApplicationAsync` in `ApplicationRepositoryIntegrationTests` ergänzt.

---

## Neue Klassen

| Klasse | Typ | Zweck |
|--------|-----|-------|
| `ApplicationGroupEditor` | Blazor-Komponente (`.razor`) | Formular zum Anlegen einer neuen `ApplicationGroup`; Pflichtfeld: `Name`; ruft nach Speichern `OnSaved` auf |
| `ApplicationEditor` | Blazor-Komponente (`.razor`) | Formular zum Anlegen einer neuen `Application`; Pflichtfelder: `Name`, `BaseUrl`; optionale Felder: `Description`, `SwaggerUrl`, `MetadataUrl`, `ApplicationGroupId`; lädt beim Initialisieren alle Gruppen für das `<select>`-Element; ruft nach Speichern `OnSaved` auf |

---

## Änderungen an bestehenden Klassen

### `ApplicationGroupTree` (Blazor-Komponente)

- **Neue Eigenschaften:**
  - `_showGroupEditor` (`bool`) — Steuert die Sichtbarkeit des `ApplicationGroupEditor`-Formulars
  - `_showApplicationEditor` (`bool`) — Steuert die Sichtbarkeit des `ApplicationEditor`-Formulars
- **Neue Methoden:**
  - `ShowGroupEditor` — Setzt `_showGroupEditor = true` und `_showApplicationEditor = false`; zeigt das Gruppenformular an
  - `ShowApplicationEditor` — Setzt `_showApplicationEditor = true` und `_showGroupEditor = false`; zeigt das Anwendungsformular an
  - `OnGroupSaved` — Wird als `OnSaved`-Callback an `ApplicationGroupEditor` übergeben; setzt `_showGroupEditor = false`, ruft `LoadDataAsync` und `StateHasChanged` auf
  - `OnApplicationSaved` — Wird als `OnSaved`-Callback an `ApplicationEditor` übergeben; setzt `_showApplicationEditor = false`, ruft `LoadDataAsync` und `StateHasChanged` auf
  - `OnEditorCancelled` — Wird als `OnCancel`-Callback an beide Editoren übergeben; setzt beide Sichtbarkeits-Flags auf `false`
- **Geänderte Methoden:** keine
- **Neue Events:** keine
- **Neue Event-Handler:** keine
- **UI-Ergänzungen:** Schaltflächen „Neue Gruppe" und „Neue Anwendung" oberhalb der Gruppenliste; bedingte Einbettung von `ApplicationGroupEditor` und `ApplicationEditor` unterhalb der Schaltflächen

### `ApplicationRepositoryIntegrationTests` (Testklasse)

- **Neue Testmethoden:** siehe Abschnitt [Tests](#tests)

---

## Umsetzungsreihenfolge

1. **`ApplicationGroupEditor` erstellen** — Abhängigkeit: nur `IApplicationRepository` (für `AddGroupAsync`) und `ISignalRNotificationService`; keine weiteren Voraussetzungen. Parameter: `EventCallback OnSaved`, `EventCallback OnCancel`.
2. **`ApplicationEditor` erstellen** — Abhängigkeit: `IApplicationRepository` (für `AddApplicationAsync` und `GetGroupsAsync`), `ISignalRNotificationService`, `IStorageModeService`, `ICurrentUserService`. Muss nach `ApplicationGroupEditor` erstellt werden, da die konzeptionelle Nähe eine konsistente Vorgehensweise sicherstellt; technisch unabhängig. Parameter: `EventCallback OnSaved`, `EventCallback OnCancel`.
3. **`ApplicationGroupTree` erweitern** — Abhängigkeit: `ApplicationGroupEditor` und `ApplicationEditor` müssen existieren, bevor sie eingebettet werden können. Schaltflächen, Sichtbarkeits-Flags und Callback-Methoden ergänzen.
4. **Integrationstests ergänzen** — Abhängigkeit: keine; kann parallel zu Schritt 1–3 erfolgen, wird hier zuletzt gelistet, da die produktiven Klassen zuerst stabil sein sollten.

---

## Tests

| Test / Hilfsmethode | Testklasse | Was wird geprüft / bereitgestellt? |
|--------------------|------------|-------------------------------------|
| `AddGroup_PersistsNewGroup` | `ApplicationRepositoryIntegrationTests` | `AddGroupAsync` legt eine Gruppe an; anschließendes `GetGroupsAsync` gibt diese Gruppe zurück |
| `AddApplication_WithGroup_PersistsApplication` | `ApplicationRepositoryIntegrationTests` | `AddApplicationAsync` legt eine Anwendung mit Gruppenreferenz an; anschließendes `GetApplicationsAsync` gibt diese Anwendung zurück |
| `AddApplication_WithoutGroup_PersistsUngroupedApplication` | `ApplicationRepositoryIntegrationTests` | `AddApplicationAsync` legt eine Anwendung ohne Gruppe an; `GetUngroupedApplicationsAsync` gibt diese Anwendung zurück |
| `AddApplication_WithStorageModeUser_SetsOwner` | `ApplicationRepositoryIntegrationTests` | Bei `StorageMode.User` wird der `Owner`-Wert der neuen Anwendung korrekt gesetzt und gefiltert zurückgegeben |

---

## Offene Punkte

1. **Darstellung der Erfassungsformulare:** Der Plan geht von Inline-Formularen aus (analog `EndpointEditor`), da die Anforderung dies als Annahme benennt. Falls modale Dialoge bevorzugt werden, muss die Sichtbarkeitslogik in `ApplicationGroupTree` entsprechend angepasst werden (Modalkomponente statt bedingter Einbettung).

2. **Gruppenauswahl im `ApplicationEditor`:** Gemäß Datenmodell ist `ApplicationGroupId` optional (`int?`). Das `<select>`-Element soll eine „Ohne Gruppe"-Option als ersten Eintrag bereitstellen (Wert `null`). Vor Implementierungsbeginn bestätigen, dass kein Pflichtfeld erwartet wird.

3. **Positionierung der Schaltflächen:** Der Plan sieht die Schaltflächen „Neue Gruppe" und „Neue Anwendung" oberhalb der Gruppenliste vor. Falls eine kontextsensitive Platzierung (z. B. „Neue Anwendung" innerhalb einer Gruppenzeile) gewünscht wird, muss die Erweiterung von `ApplicationGroupTree` entsprechend angepasst werden. In diesem Fall benötigt `ApplicationEditor` einen zusätzlichen optionalen Parameter `ApplicationGroupId` zur Vorauswahl.

4. **Bearbeiten und Löschen:** Die vorliegenden Editoren decken ausschließlich das Anlegen ab. `UpdateGroupAsync`, `UpdateApplicationAsync`, `DeleteGroupAsync` und `DeleteApplicationAsync` sind im Repository bereits vorhanden, werden in diesem Plan jedoch nicht verwendet.
