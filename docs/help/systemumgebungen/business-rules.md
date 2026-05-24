# Systemumgebungen — Business Rules

## Namenseindeutigkeit von Umgebungen

**Beschreibung:** Zwei Umgebungen dürfen im gleichen Modus und für den gleichen Benutzer nicht denselben Namen tragen.

**Bedingungen:**
- Gilt pro `StorageMode` (`Team` oder `User`) und `Owner`.
- Im Teammodus ist `Owner` immer `null`; Eindeutigkeit gilt damit pro Teamumgebung.
- Im Benutzermodus ist `Owner` der Windows-Benutzername; zwei verschiedene Benutzer dürfen Umgebungen mit gleichem Namen haben.

**Verhalten:**
- UI-seitige Prüfung: `EnvironmentEditor.SaveAsync` lädt alle Umgebungen des aktuellen Modus via `GetEnvironmentsAsync` und prüft, ob ein Eintrag mit gleichem Namen (case-insensitive) und abweichender ID existiert → Fehlermeldung „Eine Umgebung mit diesem Namen existiert bereits."
- Datenbankseitig: Unique-Constraint auf (`Name`, `Mode`, `Owner`) verhindert Duplikate auch bei gleichzeitigen Schreibzugriffen.

**Umsetzung:** `EnvironmentEditor.SaveAsync` (UI-Prüfung), `AppDbContext.OnModelCreating` (Datenbankconstraint)

---

## Namenseindeutigkeit von Variablen

**Beschreibung:** Innerhalb einer Umgebung darf kein Variablenname zweimal vorkommen.

**Bedingungen:**
- Prüfung erfolgt auf der lokalen, noch nicht gespeicherten Liste von `EnvironmentVariable`-Einträgen.
- Leere Variablennamen werden separat abgewiesen (eigene Validierungsregel).

**Verhalten:**
- `EnvironmentEditor.SaveAsync` gruppiert alle Variablen mit nicht-leerem Namen nach `Name` per LINQ. Enthält eine Gruppe mehr als einen Eintrag, wird die Fehlermeldung „Variablenname '{name}' ist mehrfach vorhanden." angezeigt; Speichern wird blockiert.
- Datenbankebene: Unique-Constraint auf (`Name`, `SystemEnvironmentId`).

**Umsetzung:** `EnvironmentEditor.SaveAsync` (LINQ-Prüfung), `AppDbContext.OnModelCreating` (Datenbankconstraint)

---

## Owner-Zuweisung im Benutzermodus

**Beschreibung:** Im Benutzermodus wird der `Owner` einer neuen Umgebung automatisch auf den aktuellen Windows-Benutzernamen gesetzt.

**Bedingungen:**
- Gilt nur bei `StorageMode.User`.
- Der Anwender kann `Owner` nicht manuell eingeben.

**Verhalten:**
- `SystemEnvironmentRepository.AddAsync` setzt `systemEnvironment.Owner = _currentUserService.GetCurrentUserName()` bei `Mode == StorageMode.User`.
- Beim Laden werden Benutzerumgebungen über `ApplyOwnerFilter` gefiltert: nur Einträge mit passendem `Owner` werden zurückgegeben.

**Umsetzung:** `SystemEnvironmentRepository.AddAsync`, `SystemEnvironmentRepository.ApplyOwnerFilter`

---

## Platzhalterauflösung: fehlende Variable ergibt leeren String

**Beschreibung:** Ist ein `{{variablenname}}`-Platzhalter in einem Endpunktfeld vorhanden, aber in der aktiven Umgebung nicht definiert, wird er durch einen leeren String ersetzt — kein Fehler, keine Fehlermeldung.

**Bedingungen:**
- Gilt für alle auflösbaren Felder (Basis-URL, relativer Pfad, Header, Query-Parameter, Bearer-Token, Body).
- Gilt auch, wenn gar keine Umgebung aktiv ist (leeres `ActiveVariables`-Dictionary).

**Verhalten:**
- `EndpointExecutionService.ResolvePlaceholders` verwendet `variables.TryGetValue(name, out var value)`. Schlägt die Suche fehl, wird ein leerer String zurückgegeben.
- Die Anfrage wird dennoch abgesendet.

**Umsetzung:** `EndpointExecutionService.ResolvePlaceholders`

---

## Reihenfolge der Platzhalterauflösung

**Beschreibung:** `{{...}}`-Platzhalter werden vor `{...}`-Pfadparameter-Platzhaltern aufgelöst.

**Bedingungen:**
- Beide Syntaxen können in denselben Feldern vorkommen.
- Da `{{...}}` zwei geschwungene Klammern verwendet und `{...}` eine, gibt es keine Überschneidung bei der Regex-Erkennung.

**Verhalten:**
- `EndpointExecutionService.BuildRequest` ruft `ResolvePlaceholders` mit `DoubleBracePlaceholderRegex` (`\{\{([^}]+)\}\}`) auf, bevor `EndpointUrlBuilder.Resolve` die `{...}`-Platzhalter ersetzt.
- Ergibt die `{{...}}`-Auflösung einen Wert, der selbst einen `{...}`-Platzhalter enthält, wird dieser im zweiten Schritt ebenfalls aufgelöst.

**Umsetzung:** `EndpointExecutionService.BuildRequest`, `EndpointExecutionService.ResolvePlaceholders`

---

## Rückfall bei gelöschter aktiver Umgebung

**Beschreibung:** Wird die aktuell aktive Umgebung gelöscht — entweder durch den eigenen Benutzer oder (im Team-Modus) durch einen anderen Client — fällt die Auswahl sofort auf „keine Umgebung" zurück.

**Bedingungen:**
- Gilt im Team-Modus für alle verbundenen Clients via SignalR.
- Gilt im eigenen Client unmittelbar nach dem Bestätigen des Löschdialogs.

**Verhalten:**
- Nach dem Löschen: `OnEnvironmentChanged` in `MainLayout` prüft, ob die aktive Umgebung noch in der Datenbank existiert (`GetByIdAsync`). Ist sie nicht mehr vorhanden, wird `SetActiveEnvironment(null)` und der `localStorage`-Eintrag entfernt.

**Umsetzung:** `MainLayout.OnEnvironmentChanged`, `ActiveEnvironmentService.SetActiveEnvironment`

---

## Maskierung von Variablenwerten

**Beschreibung:** Variablenwerte können in der UI als Passwortfeld dargestellt werden, um sensible Daten nicht im Klartext anzuzeigen. Der tatsächliche Wert wird unverändert gespeichert und bei Requests übertragen.

**Bedingungen:**
- `IsValueMasked = true`: Das Eingabefeld für den Wert hat `type="password"`; der Wert erscheint nicht im DOM-Inhalt.
- `IsValueMasked = false`: Das Eingabefeld hat `type="text"`; der Wert ist sichtbar.
- In der Datenbank und beim HTTP-Request wird der Wert immer im Klartext verwendet — keine serverseitige Verschlüsselung.

**Umsetzung:** `EnvironmentEditor` (Rendering des Eingabefelds), `EnvironmentVariable.IsValueMasked`
