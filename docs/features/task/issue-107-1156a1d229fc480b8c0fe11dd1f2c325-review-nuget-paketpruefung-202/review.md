# Plan-Review

## Ergebnis

**Status:** Offene Aufgaben vorhanden

## Umgesetzte Planelemente

- [x] Recherche + finale Risikoeinstufung (Umsetzungsreihenfolge Schritt 1) — Release-Notes-Befunde je Kandidat in `recommendation-report.md` dokumentiert (Begründungsspalten); Zusatzfakten ermittelt: kein stabiler/neuerer `Microsoft.AspNetCore.OData`-10.x-Release (preview.2 = neuestes 10.x), `Microsoft.OData.Edm` 8.4.4 existiert, `xunit.runner.visualstudio` 4.x führt xUnit ≥ 1.9.2 aus
- [x] Batch A — `ShadcnBlazor` 1.0.14 → 1.0.22 — in `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj` (Z. 24) gesetzt
- [x] Batch A — `Microsoft.AspNetCore.Http.Abstractions` 2.3.0 → 2.3.14 — in `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj` (Z. 10) gesetzt
- [x] Batch A — `Microsoft.AspNetCore.SignalR.Core` 1.2.10 → 1.2.14 — in `Schnittstellenzentrale.Infrastructure.csproj` (Z. 12) gesetzt
- [x] Batch B — `Markdig` 1.2.0 → 1.6.0 — `Schnittstellenzentrale.Infrastructure.csproj` (Z. 9)
- [x] Batch B — `Jint` 4.9.2 → 4.18.1 — in beiden Projekten synchron gesetzt (`Infrastructure` Z. 8, `Tests` Z. 26)
- [x] Batch B — `bunit` 2.7.2 → 2.15.2 — `Schnittstellenzentrale.Tests.csproj` (Z. 21)
- [x] Batch B — `AngleSharp` 1.5.0 → 1.10.5 inkl. Kommentar-Update — `Schnittstellenzentrale.Tests.csproj` (Z. 13–20): Kommentar nennt neue bunit-Version 2.15.2, neue transitive Anforderung ≥ 1.10.1, GHSA-pgww-w46g-26qg-Verweis beibehalten, Erhaltungsgrund = direkter Pin auf 1.10.5 über dem transitiven Minimum ≥ 1.10.1 dokumentiert
- [x] Batch B — `coverlet.collector` 10.0.0 → 10.1.1 — `Schnittstellenzentrale.Tests.csproj` (Z. 22), `PrivateAssets`/`IncludeAssets` erhalten
- [x] Batch B — `Microsoft.NET.Test.Sdk` 18.5.1 → 18.12.0 — `Schnittstellenzentrale.Tests.csproj` (Z. 31)
- [x] Batch B — `Moq` 4.20.72 → 4.21.0 — `Schnittstellenzentrale.Tests.csproj` (Z. 33)
- [x] `Microsoft.Playwright` 1.52.0 → 1.65.1 — **plangemäß nicht umgesetzt**: Release-Notes-Recherche ergab dokumentierte Breaking Changes → Umstufung auf „hoch" gemäß Plan-Regel („Recherche und Risiko-Finalisierung" Schritt 3 / Akzeptanzszenario 4); Referenz bleibt 1.52.0, Befund inkl. konkreter Bruchliste (Locator-Inertness, Cookie-RFC-6265, Accessible Names/`GetByRole`, `ParamsString`, Proxy-Credentials) im Report dokumentiert
- [x] Transitive AngleSharp-/AngleSharp.Css-Auflösung nach bunit-Update geprüft — gegen `project.assets.json` verifiziert: bunit 2.15.2 fordert `AngleSharp` ≥ 1.10.1 und `AngleSharp.Css` ≥ 1.4.1; aufgelöst AngleSharp 1.10.5, AngleSharp.Css 1.4.1 (Beta-Abhängigkeit beseitigt), AngleSharp.Diffing 1.1.1 → Kommentar-Wahrheit gegeben
- [x] Designentscheidung „Floating `10.0.*` unangetastet" — alle 17 Floating-Referenzen im Diff unberührt
- [x] Designentscheidung „`System.Security.Cryptography.Pkcs`-Override unverändert" — Version `10.0.*` und Begründungs-Kommentar (GHSA-846f-f764-76mx, GHSA-2fmv-rv2c-59x7) in `Schnittstellenzentrale.Infrastructure.csproj` Z. 28–37 exakt unverändert
- [x] Designentscheidung „Legacy-Pakete: Patch statt Ablösung" — Patch-Updates umgesetzt; `FrameworkReference`-Ablösung als hoch/Architekturänderung im Report dokumentiert
- [x] Designentscheidung „OData-Split unverändert" — `Microsoft.AspNetCore.OData` 10.0.0-preview.2 und `Microsoft.OData.Edm` 8.4.3 unangetastet; kohärente Empfehlung (Edm → 9.0.0 stabil, SemVer > 9.0.0-preview.4) als hoch dokumentiert
- [x] Lokale Verifikation pro Batch — laut `recommendation-report.md`: Release-Build mit `TreatWarningsAsErrors` 0 Warnungen, `dotnet test --filter "Category!=OsInterface"` 641/641, `--vulnerable --include-transitive` 0 Befunde — für beide Batches
- [x] `recommendation-report.md` erzeugt — gefordertes Ergebnisformat vollständig: Paket | aktuell → Ziel | Update-Art | Risiko + Begründung; alle drei Gruppen (umgesetzt / hoch-aufgeschoben / keine Aktion), transitive Highlights, npm-/Actions-Pin-Hinweise und vollständiger Folge-Issue-Entwurf (8 Abschnitte, ohne `automated-nuget-review`-Label) enthalten

## Offene Aufgaben

- [ ] Folge-Issue für hoch/aufgeschobene Empfehlungen auf GitHub anlegen (Umsetzungsreihenfolge Schritt 5) — im Report liegt nur der **Entwurf** vor; das Issue selbst wurde noch nicht erstellt. Fehlt vollständig.
- [ ] PR gegen `staging` öffnen mit Empfehlungstabelle im Body + Folge-Issue-Verlinkung und Checks fail-fast überwachen (Umsetzungsreihenfolge Schritt 6) — kein PR vorhanden; Änderungen liegen uncommitted im Arbeitsverzeichnis. Fehlt vollständig.

## Hinweise

1. **Umstufung `Microsoft.Playwright` ist plan- und anforderungskonform:** Der Plan stufte das Update zunächst als „mittel" ein, sah aber explizit vor, dass ein Breaking-Change-Befund in den Release Notes zur Umstufung auf „hoch" führt (Plan Z. 45, Akzeptanzszenario 4). `requirement.md` schließt die automatische Umsetzung von hoch-Risiko-Updates aus. Der Report begründet die Umstufung mit konkreten Brüchen aus den 1.65-Release-Notes, die die OsInterface-Tests plausibel treffen (`GetByRole` mit Name, `ClickAsync`, `FillAsync`). Keine Planabweichung — die Tasks-Datei wurde entsprechend angepasst.
2. **AngleSharp-Override-Kommentar korrekt aktualisiert:** Behauptung „bunit 2.15.2 fordert transitiv AngleSharp ≥ 1.10.1" wurde gegen `src/Schnittstellenzentrale.Tests/obj/project.assets.json` verifiziert (bunit-Dependencies: AngleSharp 1.10.1, AngleSharp.Css 1.4.1). Der erhaltene Pin wird durch die transitive Mindestanforderung (≥ 1.10.1 aus bunit 2.15.2) und das Festhalten auf der gepatchten 1.10.5 begründet; die `using AngleSharp;`-Direktive in `PlaywrightServer.cs` (Z. 2) ist ungenutzt (kein API-Aufruf) und trägt den Override nicht. Plan-Check-Hinweis 1 (AngleSharp fehlte in der Recherche-Aufzählung) wurde berücksichtigt — der Report enthält eine eigene AngleSharp-Zeile mit Release-Notes-Begründung.
3. **Overrides/Floating unangetastet:** `System.Security.Cryptography.Pkcs`-Override (Version + Kommentar) und alle `10.0.*`-Floats sind im Diff exakt unverändert; `Microsoft.OData.Edm` 8.4.3 und `Microsoft.AspNetCore.OData` 10.0.0-preview.2 ebenso. Diff beschränkt sich auf die geplanten Versionsangaben + den AngleSharp-Kommentarblock — Scope-Ausschluss 7 eingehalten.
4. **Verifikationszahlen stammen aus dem Report:** 641/641 Tests und 0 Vulnerabilities wurden von diesem Review nicht erneut ausgeführt, sondern der dokumentierten Verifikationstabelle in `recommendation-report.md` entnommen. Die aufgelösten Zielversionen in `project.assets.json` bestätigen die behaupteten Updates allerdings unabhängig.
5. **Reihenfolge-Abhängigkeit:** Das Folge-Issue sollte vor dem PR angelegt werden, damit der PR-Body es verlinken kann (Plan-Schritt 6 verlangt „Folge-Issue verlinken").
