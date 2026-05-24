# Offene Aufgaben

Erstellt am: 2026-05-24
Abbruchgrund: Maximale Iterationsanzahl erreicht

Die folgenden Aufgaben konnten im automatisierten Zyklus nicht abgeschlossen werden
und müssen manuell oder in einem erneuten Lauf bearbeitet werden.

## Offene Planelemente

- [ ] `BuildRequest_ResolvesDoubleBracePlaceholdersBeforeSingleBrace` — dedizierter Test fehlt, der die Auflösungsreihenfolge `{{...}}` vor `{...}` in einem kombinierten Szenario explizit belegt. Die Einzelschritte werden durch zwei separate Tests geprüft (`BuildRequest_ResolvesDoubleBracePlaceholders` und `BuildRequest_ResolvesSingleBracePlaceholdersFromQueryParameters`), aber nicht in einem Test zusammengeführt.

## Code-Review-Befunde

- [ ] **`StorageModeServiceExtensions.cs`** — Namespace `Schnittstellenzentrale.Core.Interfaces` stimmt nicht mit dem Dateipfad `Helpers/` überein; sollte `Schnittstellenzentrale.Core.Helpers` lauten.
- [ ] **`MainLayout.razor`** — Leerer `catch`-Block in `DisposeAsync` ohne erklärenden Kommentar; die anderen leeren `catch`-Blöcke in derselben Datei sind bereits kommentiert.
- [ ] **`SystemEnvironmentRepositoryIntegrationTests.cs`** — `FixedCurrentUserService` ist eine exakte Kopie der gleichnamigen Klasse in `TestHelpers.cs`; die Klasse aus `TestHelpers` sollte als `internal` zugänglich gemacht und wiederverwendet werden.
- [ ] **`EnvironmentEditor.razor`** — `SaveAsync` (~95 Zeilen) erledigt Eingabevalidierung, Datenbankprüfung und Speichern in einem Block; die lokale Validierungslogik sollte in eine eigene Methode `ValidateInput()` ausgelagert werden.
