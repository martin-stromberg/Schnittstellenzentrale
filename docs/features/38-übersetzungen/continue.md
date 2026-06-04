# Offene Aufgaben

Erstellt am: 2026-06-04
Abbruchgrund: Kein Fortschritt zwischen den letzten zwei Iterationen (Iteration 1: 7 Befunde, Iteration 2: 10 Befunde)

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

(keine — Plan-Review hat Status `Vollständig umgesetzt`)

## Code-Review-Befunde

- [ ] **[Hoch] `ContentHeader.razor`** — Strings "Untertitel hinzufügen" (Placeholder), "Name darf nicht leer sein." (Validierungsfehler), "Nur PNG- und JPEG-Dateien sind erlaubt." und "Datei ist zu groß (max. X KB)." (Upload-Fehlermeldungen) noch hartcodiert. Localizer ist bereits injiziert, Schlüssel fehlen in SharedResources.resx.
- [ ] **[Hoch] `EnvironmentContentView.razor`** — "Abbrechen" (Button), "Beschreibung hinzufügen…" (Placeholder), "Variablen" (Label), "Name darf nicht leer sein." und "Name darf maximal 200 Zeichen lang sein." (Inline-Validierung) noch hartcodiert. Localizer ist bereits injiziert.
- [ ] **[Mittel] `EnvironmentsSidebar.razor`** — Inline-Validierungsfehler in `ConfirmCreateAsync`: "Name ist ein Pflichtfeld." und "Name darf maximal 200 Zeichen lang sein." noch hartcodiert.
- [ ] **[Mittel] `ApplicationCard.razor`** — Button-Labels "Swagger-Import", "OData-Import", "Health-Check" sowie Labels "Basis-URL:", "Metadaten-URL:" / "Swagger-URL:" noch hartcodiert. Localizer ist bereits injiziert, Schlüssel fehlen in SharedResources.resx.
- [ ] **[Mittel] `ApplicationContentView.razor`** — Vollständig unlokalisiert (kein Localizer injiziert). Betroffen: "Swagger-Import", "Health-Check", "Beschreibung", "Keine Beschreibung vorhanden.", "Basis-URL", "Swagger / OData URL", "KPI", "Anzahl Endpunkte".
- [ ] **[Mittel] `CollectionContentView.razor`** — Vollständig unlokalisiert (kein Localizer injiziert). Betroffen: "+ Neue Anwendung", "KPI", "Anzahl Anwendungen", "Anzahl Endpunkte", Tabellen-Header "Methode", "Endpunkt", "Beschreibung", "Aktion", title="Ausführen", "Keine Endpunkte vorhanden."
- [ ] **[Mittel] `FolderContentView.razor`** — Vollständig unlokalisiert (kein Localizer injiziert). Betroffen: Abschnittsüberschrift "Endpunkte in »Name«", Tabellen-Header "Methode", "Endpunkt", "Beschreibung", "Aktion", title="Ausführen".
- [ ] **[Niedrig] `HealthCheckDialog.razor`** — Titel-Präfix "Health-Check: " hartcodiert in `Title="@($"Health-Check: {Application.Name}")"`.
- [ ] **[Niedrig] `ODataImportDialog.razor` und `SwaggerImportDialog.razor`** — Titel-Strings "OData-Import-Vorschau" und "Swagger-Import-Vorschau" hartcodiert. Lösungsansatz: Ressourcen-Schlüssel als Parameter übergeben oder ImportDialog lokalisiert den Titel selbst.
- [ ] **[Niedrig] `ApplicationGroupTree.razor`** — "Ohne Sammlung" an zwei Stellen hartcodiert (Zeile 47: CollapsibleSection-Titel, Zeile 426: Activity-Log-Fallback). Localizer ist bereits injiziert, Schlüssel fehlen in SharedResources.resx.
- [ ] **[Niedrig] `WorkspacesLayout.razor`** — Default-Endpunktname "Neuer Endpunkt" bei `HandleCreateEndpointRequested` hartcodiert (Zeile 287). Wird sofort nach Endpunkt-Erstellung angezeigt.
- [ ] **[Niedrig] `LocalizationTests.cs`** — Assertion auf generischen Text "Reload" ist fragil. Robustere Alternative: spezifischerer String oder `data-testid`-Attribut auf dem Reload-Link.
