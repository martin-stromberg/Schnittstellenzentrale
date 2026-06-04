# Code-Review: Feature 38 – Mehrsprachigkeit DE/EN

**Branch:** `38-übersetzungen`
**Verglichen mit:** `git diff main...HEAD` + uncommitted Working-Tree-Änderungen (`git diff HEAD`)
**Scope:** Geänderte Razor-Komponenten und Ressourcendateien dieser Implementierungsrunde
**Aufwand:** High (7 Winkel × bis 6 Kandidaten, 1-Vote-Verifikation)

---

## Befunde

```json
[
  {
    "file": "src/Schnittstellenzentrale/Resources/SharedResources.resx",
    "line": 275,
    "summary": "EN-Bestätigungsmeldung in drei Delete-Dialogen ist grammatisch gebrochen: 'really delete' erscheint doppelt.",
    "failure_scenario": "ConfirmDeleteGroupDialog_Message = 'Really delete collection' + fetter Name + ConfirmDeleteGroupDialog_MessageSuffix = 'really delete?' ergibt zur Laufzeit 'Really delete collection XYZ really delete?'. Dasselbe gilt für ConfirmDeleteApplicationDialog (Zeile ~300) und ConfirmDeleteEndpointGroupDialog (Zeile ~330). Für DE-Nutzer kein Problem ('Sammlung XYZ wirklich löschen?' ist korrekt), aber englischsprachige Nutzer sehen kaputten Text. Korrekturfabian: Suffix auf '?' oder Prefix auf 'Delete collection' (ohne 'Really') ändern."
  },
  {
    "file": "src/Schnittstellenzentrale/Program.cs",
    "line": 149,
    "summary": "UseRequestLocalization wird nach UseAuthentication und UseAuthorization registriert.",
    "failure_scenario": "Microsoft empfiehlt, UseRequestLocalization vor UseAuthentication zu platzieren, damit die Kultur früh im Pipeline-Zyklus gesetzt wird. In der aktuellen Reihenfolge kann Auth-Middleware (z. B. Challenge-Meldungen oder Policy-Fehler) mit der Standard-Kultur 'en' antworten, anstatt mit der vom Browser angeforderten Kultur – auch wenn der Browser 'de' meldet. Für reine Blazor-Server-Szenarien mit IStringLocalizer im Component-Rendering ist dies meist unproblematisch, aber kein korrektes Setup laut ASP.NET Core-Dokumentation."
  }
]
```

---

## Prüfpunkte ohne Befund

| Prüfpunkt | Ergebnis |
|---|---|
| Vollständigkeit der Lokalisierung in den 7 geänderten Komponenten | Alle sichtbaren Strings über `L[...]` abgerufen; keine hartcodierten Strings übersehen. |
| Schlüsselschema `{KomponentenName}_{Rolle}` | Alle Schlüssel folgen dem Schema konsistent. |
| Symmetrie EN ↔ DE (gleiche Schlüssel in beiden resx-Dateien) | Vollständig symmetrisch; PowerShell-Diff ergab keine Abweichung. |
| Comment-Felder in resx | Alle Schlüssel besitzen einen ausgefüllten `<comment>`-Eintrag mit UI-Kontext. |
| `@inject IStringLocalizer<SharedResources> L` in allen Komponenten | Korrekt in allen 7 geänderten Komponenten vorhanden. |
| `AddLocalization()` in `Program.cs` | Registriert (Zeile 51). |
| `AddDataAnnotationsLocalization()` | Registriert mit korrektem Localizer-Provider auf `SharedResources`. |
| DataAnnotations-Standardschlüssel in resx | `[Required]`- und `[MaxLength]`-Schlüssel in beiden Sprachen vorhanden. |
| Alle in den geänderten Komponenten verwendeten Schlüssel existieren in EN- und DE-resx | Vollständig; keine fehlenden Schlüssel. |
| `string.Format(L["key"], arg)` statt `L["key", arg]` | Akzeptiertes Pattern im Codebase – kein Befund. |

---

## Nicht geprüfte Bereiche (außerhalb des Diff-Scopes)

Die folgenden Komponenten sind **nicht** Teil dieses Diff und wurden daher nicht auf Lokalisierungsvollständigkeit geprüft. Sie enthalten nach wie vor hartcodierte Strings (bekannt aus `continue.md`):

- `HealthCheckDialog.razor` – Titel-Präfix "Health-Check: " hartcodiert
- `ODataImportDialog.razor`, `SwaggerImportDialog.razor` – Titel-Strings
- `ApplicationGroupTree.razor` – "Ohne Sammlung" an zwei Stellen
- `WorkspacesLayout.razor` – Default-Endpunktname "Neuer Endpunkt"
- Alle weiteren Komponenten aus dem ursprünglichen Inventory
