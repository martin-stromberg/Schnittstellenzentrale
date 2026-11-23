# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### review.md / recommendation-report.md (Lifecycle-Artefakte)

- **Residualer faktischer Fehler — Befund 1 nur im csproj behoben, nicht in den propagierten Stellen** — Iteration 1 stellte verifiziert fest, dass `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightServer.cs` Z. 2 nur eine **ungenutzte** `using AngleSharp;`-Direktive enthält und im Testprojekt kein AngleSharp-API-Aufruf existiert (in dieser Iteration erneut verifiziert: einziger AngleSharp-Treffer im Projekt ist genau diese using-Zeile). Der csproj-Kommentar wurde korrekt bereinigt — dieselbe falsche Behauptung „AngleSharp wird direkt in `PlaywrightServer.cs` genutzt" steht aber weiterhin an vier Stellen, die den **gelieferten Endzustand** beschreiben bzw. sie als verifiziertes Faktum ausweisen:
  - `review.md` Z. 16: behauptet, der finale Kommentar dokumentiere „Direktnutzung in `PlaywrightServer.cs` als Erhaltungsgrund" — der korrigierte Kommentar (csproj Z. 13–18) enthält das nicht mehr; die Beschreibung des gelieferten Stands ist damit falsch.
  - `review.md` Z. 37: „Direktnutzung in `Playwright/Infrastructure/PlaywrightServer.cs` (Z. 2: `using AngleSharp;`) begründet den erhaltenen Pin **faktisch korrekt**" — direkte Wiederholung des widerlegten Faktums als verifizierte Aussage; widerspricht jetzt dem korrigierten csproj-Kommentar.
  - `recommendation-report.md` Z. 35: „Pin bleibt wegen direkter Nutzung in `PlaywrightServer.cs` nötig" — falsche Begründung im Abgabe-Report des Issues.
  - `plan.md` Z. 33 („AngleSharp wird zusätzlich direkt in `PlaywrightServer.cs` genutzt — direkte Referenz bleibt unabhängig von bunit nötig"), `plan-check.md` Z. 18 und `inventory/packages.md` Z. 60 enthalten denselben Satz. Diese drei dokumentieren zwar historische Planungs-/Inventur-Stände, die Behauptung war aber auch dort schon faktisch falsch.

  Empfehlung: In `review.md` Z. 16 den Kommentar-Ist-Stand nacherzählen (Pin-Begründung über transitive Mindestanforderung, keine Direktnutzung) und Z. 37 den Satz zur Direktnutzung streichen bzw. korrigieren (ungenutzte using-Direktive ≠ Nutzung; Pin-Begründung = Versions-Pinning auf gepatchte Linie 1.10.5 über transitivem Minimum ≥ 1.10.1 aus bunit 2.15.2). In `recommendation-report.md` Z. 35 die Klammerbegründung auf die Pin-Begründung umstellen. `plan.md` Z. 33, `plan-check.md` Z. 18 und `inventory/packages.md` Z. 60 mindestens als überholte Annahme kennzeichnen oder den Satz analog streichen. Nebenbefund unverändert aus Iteration 1 außerhalb des Diff-Scopes: das verwaiste `using AngleSharp;` in `PlaywrightServer.cs` Z. 2 ist toter Code und kann bei Gelegenheit entfernt werden.

## Verifikation der Iteration-1-Befunde

- **Befund 1 (AngleSharp-Kommentar, csproj):** Behoben und inhaltlich korrekt. `Schnittstellenzentrale.Tests.csproj` Z. 13–18 nennt jetzt nur noch die tragfähige Begründung (bunit 2.15.2 fordert transitiv ≥ 1.10.1; direkte Referenz hält Auflösung auf gepatchtem 1.10.5 über dem Minimum; nearest-wins; GHSA-pgww-w46g-26qg-Verweis erhalten). Keine Direktnutzungs-Behauptung mehr im Kommentar.
- **Befund 2 (Zählwerte):** Behoben und selbst verifiziert. `grep 'Version="10\.0\.\*"'` über `src/` ergibt exakt **17 Referenzen** (Tests 4, Infrastructure 11, Hauptprojekt 2) über **13 Pakete** (Mvc.Testing, EFCore.Sqlite ×2-Projekte, EFCore.SqlServer ×2, JSInterop ×2, SignalR.Client, EFCore, EFCore.Design ×2, Configuration.Abstractions, Configuration.Binder, DI.Abstractions, Ext.Http, Pkcs, Authentication.Negotiate). Alle korrigierten Stellen konsistent: `recommendation-report.md` Z. 60 („13 Floating-`10.0.*`-Pakete (17 Referenzen)") und Z. 141 („17 Floating-Referenzen"), `review.md` Z. 22 („alle 17 Floating-Referenzen"), `inventory.md` Z. 7, `inventory/packages.md` Z. 77, `plan.md` Z. 32. Kein Restvorkommen von „18"/„14" im Floating-Kontext außerhalb des historischen `review-code.1.md` (dort korrekt als Befund-Beschreibung).

## Volle Diff-Prüfung der csproj-Änderungen (keine weiteren Befunde)

- **Versionen:** ShadcnBlazor 1.0.14→1.0.22 (Hauptprojekt); Jint 4.9.2→4.18.1, Markdig 1.2.0→1.6.0, Http.Abstractions 2.3.0→2.3.14, SignalR.Core 1.2.10→1.2.14 (Infrastructure); AngleSharp 1.5.0→1.10.5, bunit 2.7.2→2.15.2, coverlet.collector 10.0.0→10.1.1, Jint 4.9.2→4.18.1, NET.Test.Sdk 18.5.1→18.12.0, Moq 4.20.72→4.21.0 (Tests). `Microsoft.Playwright` bleibt plangemäß 1.52.0.
- **Floating/Overrides unangetastet:** Alle 17 `10.0.*`-Referenzen im Diff unverändert; `System.Security.Cryptography.Pkcs`-Override inkl. GHSA-Kommentar (Infrastructure Z. 28–37) nicht angefasst; keine neue Floating-Referenz hinzugefügt.
- **Jint synchron:** 4.18.1 in Infrastructure (Z. 8) und Tests (Z. 25); `grep` über alle `.csproj` bestätigt Jint/bunit/AngleSharp nur in diesen zwei Dateien — keine vergessene Referenz (Core enthält keine davon), kein `Directory.Packages.props`, keine `packages.lock.json`.
- **Datei-Integrität:** BOM-Status identisch zum HEAD-Blob (Infrastructure + Tests: `EF BB BF`; Hauptprojekt ohne BOM); Arbeitsdateien CRLF wie bisher; Diff beschränkt sich auf `Version`-Attribute und den Kommentarblock — keine XML-Struktur-/Whitespace-Änderungen, XML wohlgeformt.
- **Nebenwirkungen der Korrekturen:** Die Zählwert-Korrekturen in `inventory.md`, `inventory/packages.md`, `plan.md`, `recommendation-report.md`, `review.md` sind punktuell und erzeugen keine neuen Inkonsistenzen. Einzige Nebenwirkung: Die Kommentar-Korrektur hat die oben gelistete Diskrepanz zu den unkorrigiert gebliebenen Direktnutzungs-Behauptungen in den Lifecycle-Dokumenten sichtbar gemacht.

## Geprüfte Dateien

Im Diff gegen `HEAD` (Basisbranch `staging`; alle Änderungen uncommitted):

- `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`
- `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj`
- `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`
- `docs/features/task/issue-107-...-202/inventory.md`, `inventory/packages.md`, `plan.md`, `todo.md`, `recommendation-report.md`, `review.md`, `plan-check.md`, `test-results.md`, `review-usability.md`, `review-code.1.md`, `...-202-tasks.md` (Lifecycle-Artefakte)
