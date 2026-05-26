# Aktivitätsprotokoll — Beschreibung

## Zweck

Das Aktivitätsprotokoll gibt dem Benutzer einen Überblick darüber, welche Aktionen während seiner aktuellen Sitzung durchgeführt wurden. Es zeigt HTTP-Anfragen, Skriptausführungen, Entitätsoperationen (Anlage, Umbenennung, Verschieben) sowie Kontextwechsel (Modus, Umgebung) in Echtzeit an.

## Funktionsweise

Alle relevanten Aktionen rufen `IActivityLogService.Log(category, message, details?)` auf. Der Dienst legt für jeden Aufruf einen `ActivityLogEntry` mit aktuellem Zeitstempel, Kategorie, Nachrichtentext und optionalen Details an und informiert die `ActivityLogPanel`-Komponente über das `OnEntryAdded`-Event.

Das `ActivityLogPanel` ist über den Button „Protokoll" (Symbol `bi-list-ul`) in der oberen Leiste erreichbar. Es kann zwischen zwei Anzeigemodi umgeschaltet werden:

- **Dock**: Das Panel erscheint am unteren Bildschirmrand und verkleinert den Inhaltsbereich entsprechend. Die Höhe ist per Drag-Handle am oberen Panelrand stufenlos einstellbar.
- **Overlay**: Das Panel erscheint als halbtransparentes Fenster über dem Inhaltsbereich, ohne das Layout zu verschieben.

Anzeigemodus und Panelhöhe werden im `localStorage` des Browsers gespeichert und beim nächsten Öffnen wiederhergestellt.

Die Protokolleinträge werden **nicht in der Datenbank persistiert**. Das Protokoll lebt ausschließlich im Arbeitsspeicher des Blazor-Circuits und endet mit dem Schließen der Browser-Verbindung oder einem manuellen Leeren.

## Kategorien und Icons

| Kategorie | Icon | Bedeutung |
|-----------|------|-----------|
| `EntityCreated` | `bi-plus-circle` | Entität wurde angelegt (Gruppe, Anwendung, Ordner, Endpunkt) |
| `EntityModified` | `bi-pencil` | Entität wurde umbenannt oder bearbeitet |
| `EntityMoved` | `bi-arrows-move` | Anwendung wurde per Drag & Drop verschoben |
| `ContextSwitched` | `bi-arrow-left-right` | Speichermodus oder Systemumgebung wurde gewechselt |
| `EndpointExecuted` | `bi-lightning` | HTTP-Request wurde erfolgreich ausgeführt |
| `ScriptExecuted` | `bi-code-slash` | JavaScript-Skript wurde gestartet |
| `ScriptConsoleOutput` | `bi-terminal` | Ausgabe von `sz.console.write(text)` im Skript |
| `HttpError` | `bi-exclamation-triangle` | HTTP-Fehlerantwort (4xx oder 5xx) |
| `InternalError` | `bi-bug` | Unbehandelte Ausnahme in einem Service |

## Beispiele

**Erfolgreicher HTTP-Request:**
```
14:35:12  ⚡  GET https://api.example.com/data — 200
          [Details: Request: ... / Response: ...]
```

**Skript-Konsolenausgabe:**
```
14:35:12  >_  Antwort verarbeitet.
```

**Ordner angelegt:**
```
14:36:01  ✚  Ordner angelegt: Stammdaten
```

**Umgebungswechsel:**
```
14:37:45  ↔  Umgebung gewechselt: Produktion
```

## Einschränkungen

- Das Protokoll ist pro Blazor-Circuit (Browser-Verbindung) isoliert. Zwei Browser-Tabs desselben Benutzers führen separate Protokolle.
- Es gibt keine Obergrenze für die Anzahl der Einträge. Das Protokoll wächst bis zum manuellen Leeren oder bis zum Ende der Browser-Verbindung.
- Werte von als maskiert markierten Umgebungsvariablen (`IsValueMasked == true`) erscheinen in den Details von `EndpointExecuted`-Einträgen als `***`.
- HTTP-Fehlerantworten (4xx/5xx) enthalten im Protokoll nur Statuscode und Reason Phrase — keinen Response-Body.
