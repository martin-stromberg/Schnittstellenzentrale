# Abnahmeprüfung – Entwicklungsschritt 3

## Ergebnis
**Status:** Anforderung vollständig erfüllt

## Abweichungen
- Keine.

## Hinweise

### Schrittumfang (git diff Basis...HEAD)
Vier geänderte Dateien: `Microsoft.OData.Edm` 8.4.3 → 9.0.0 in `Schnittstellenzentrale.Infrastructure.csproj` (Zeile 36); neue direkte Referenzen `Microsoft.OData.Core` 9.0.0 und `Microsoft.OData.ModelBuilder` 3.0.0 samt Begründungskommentar in `Schnittstellenzentrale.csproj` (Zeilen 17–27); F5-Entscheidungsabschnitt in `decisions.md`; Release-Notes-Eintrag (EN+DE). Kein Code diff in `ODataImportService.cs` nötig — `CsdlReader.Parse(XmlReader)` (Zeile 64), `IEdmModel`, `EntitySets()`, `SchemaElements.OfType<IEdmOperation>()`, `IEdmAction` und die Vocabulary-Annotation-API in `ODataEdmModelBuilder.cs` (`EdmModel`, `EdmTerm`, `SetVocabularyAnnotation`) sind in Edm 9 unverändert; Build bestätigt Kompilierbarkeit.

### Bewertung der NU1608-Folgelösung (zusätzliche Prüfaufträge a–d)
- **(a) Begründung korrekt — verifiziert per nuspec:** `microsoft.odata.core/9.0.0-preview.4` deklariert exakte Pins `Microsoft.OData.Edm [9.0.0-preview.4]` und `Microsoft.Spatial [9.0.0-preview.4]`. Der exakte Pin kollidiert zwingend mit der direkten Edm-9.0.0-Referenz (NU1608). Aussage stimmt.
- **(b) Versionsbereiche erfüllt — verifiziert per nuspec:** `microsoft.aspnetcore.odata/10.0.0-preview.2` (einzige TFM-Gruppe: net10.0; unter net11.0 via Nearest-TFM wirksam) deklariert `Core`/`Edm`/`Spatial [9.0.0-preview.4, 10.0.0)` und `ModelBuilder [3.0.0-preview.1, 4.0.0)`. Stabile 9.0.0 bzw. 3.0.0 liegen in allen Bereichen. `Microsoft.OData.Core` 9.0.0 pinnt seinerseits `Edm [9.0.0]` und `Spatial [9.0.0]` exakt — deckungsgleich mit der direkten Referenz, kein Rest-Split. Die Lösung ist die sauberste verfügbare: die Alternative (Edm auf preview.4 pinnen oder transitive Previews tolerieren) hätte NU1608 bzw. Preview-Pins im Bestand behalten und das Issue-Ziel „stabile 9.0.0" verfehlt. Erwähnenswert: `ModelBuilder 3.0.0-preview.1` hätte mit seinem Edm-Bereich `[9.0.0-preview.3, 10.0.0)` kein NU1608 erzeugt — die direkte stabile Referenz ist hier keine Hartnotwendigkeit, aber semantisch korrekt (direkte Code-Nutzung in `OData/ODataEdmModelBuilder.cs`) und räumt die letzte Preview aus dem Stack.
- **(c) Konsistenz verifiziert:** `dotnet list package --include-transitive` über alle vier Projekte — Schnittstellenzentrale: AspNetCore.OData 10.0.0-preview.2, Core 9.0.0, ModelBuilder 3.0.0 (direkt), Edm 9.0.0, Spatial 9.0.0 (transitiv); Infrastructure: Edm 9.0.0 (direkt, kein weiterer ODL-Bedarf — Edm 9.0.0 hat keine Spatial-Abhängigkeit mehr); Tests: einheitlich Core/Edm/Spatial 9.0.0 + ModelBuilder 3.0.0; Core: kein ODL-Bezug. `project.assets.json` (libraries-Abschnitt) enthält ausschließlich die stabilen Versionen — die dort gefundenen `preview.4`-/`preview.1`-Strings sind nur die deklarierten Abhängigkeitsbereiche des AspNetCore.OData-Pakets, keine aufgelösten Versionen. Kein 8.4.x- oder preview.4-Rest. `dotnet restore --force` ohne NU1608- und sonstige Warnungen.
- **(d) Platzierung im Web-csproj ist richtig (kein Hinweis, keine Abweichung):** `ModelBuilder` wird direkt im Web-Code genutzt; `Core` ist eine reine Auflösungs-Pin-Referenz, die dort hingehört, wo der Konflikt entsteht — Web trägt `AspNetCore.OData`. Infrastructure zieht `OData.Core` transitiv gar nicht (verifiziert); eine Referenz dort wäre ein ungenutztes Paket und würde die Web-/Test-Auflösung nur indirekt über Projekt-Transitivität korrigieren — fragiler und semantisch falsch. Die Platzierung ist als Kommentar im csproj und in decisions.md sauber begründet.

### Akzeptanzkriterien im Einzelnen
- Kein Versions-Split bei `Microsoft.OData.Edm`: projektübergreifend einheitlich 9.0.0 (vorher: kompiliert 8.4.3, transitiv gelaufen 9.0.0-preview.4 — Split aufgehoben).
- `ODataControllerIntegrationTests`: **78/78 grün** (gefilterter Lauf, Dauer 2 s).
- `dotnet test --filter "Category!=OsInterface"`: **641/641 grün**, 0 Fehler, 0 übersprungen — exakt die Baseline.
- OData-Funktionsverifikation: Die 78 Integrationstests laufen über `WebApplicationFactory` gegen echte `[EnableQuery]`-Endpunkte aller vier Controller inkl. `$metadata`, `$filter`, `$select`, `$expand`; der Import-Pfad (`CsdlReader.Parse`) ist durch `ODataImportServiceTests` (49+ Assertions), `ODataImportServiceRealMetadataTests` und `ODataImportServiceIntegrationTests` im grünen Gesamtlauf abgedeckt.
- `dotnet list package --vulnerable` (alle vier Projekte, Tests inkl. transitiv): **kein Befund**.
- Strategie + Exit-Kriterium in `decisions.md` (F5-Abschnitt): Verharren auf `10.0.0-preview.2` begründet (kein stabiler 10.x/11.x-Release existiert; letzter stabiler Strang 9.x ist net8.0), Exit-Kriterium „stabiler 10.x-/11.x-Release → eigener Schritt mit voller OData-Testverifikation" dokumentiert, Alternativen und Konsequenzen (u. a. exakter Edm-Pin von Core 9.0.0 → künftige Edm-Bumps nur synchron mit Core) ausgewiesen. Release-Notes EN+DE gepflegt.

### Randnotizen (keine Abweichungen)
- Build: 0 Fehler, 35 Warnungen — ausschließlich BL0012/BL0016-Razor-Analyser-Befunde, schrittunabhängig (Bestand).
- Testlauf mit `-p:SkipPlaywrightInstall=true` ausgeführt (überspringt nur den Browser-Install-Post-Build-Schritt; OsInterface-Kategorie ist ohnehin ausgeschlossen) — ergebnisneutral.
- Folgeverantwortung beim Exit-Kriterium: `OData.Core 9.0.0` pinnt `Edm`/`Spatial` exakt auf `[9.0.0]` — Edm-/Spatial-Updates erfordern künftig ein synchrones Core-Update; in decisions.md als Konsequenz vermerkt.
