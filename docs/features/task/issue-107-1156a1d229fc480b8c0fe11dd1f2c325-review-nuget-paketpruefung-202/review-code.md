# Code-Review

## Ergebnis

**Status:** Keine Befunde

## Verifikation der Iteration-2-Restbefunde (alle behoben)

Die in Iteration 2 monierte falsche Direktnutzungs-Behauptung („AngleSharp wird direkt in `PlaywrightServer.cs` genutzt") wurde an allen Stellen korrigiert. Selbst gesucht über `rg` nach `PlaywrightServer`, `Direktnutzung`, `direkt genutzt`, `used directly` im Feature-Verzeichnis und in den `.csproj`:

- `review.md` Z. 16: beschreibt den gelieferten Kommentar jetzt faktisch korrekt (bunit 2.15.2, transitive Anforderung ≥ 1.10.1, GHSA-pgww-w46g-26qg-Verweis, Erhaltungsgrund = Pin auf 1.10.5 über dem transitiven Minimum) — keine Direktnutzungs-Behauptung mehr.
- `review.md` Z. 37: stellt jetzt korrekt fest, dass die `using AngleSharp;`-Direktive in `PlaywrightServer.cs` (Z. 2) ungenutzt ist und den Override nicht trägt.
- `recommendation-report.md` Z. 35: Klammerbegründung auf Pin-Begründung umgestellt (bunit zieht selbst ≥ 1.10.1; Pin hält Auflösung auf 1.10.5 über dem Minimum).
- `plan.md` Z. 33: Direktnutzungs-Satz ersetzt durch nearest-wins-Begründung plus explizitem Hinweis, dass die using-Direktive ungenutzt ist.
- `plan-check.md` Z. 18: Erhaltungsgrund auf Pin-Funktion umgestellt.
- `inventory/packages.md` Z. 60: Teilsatz „zusätzlich direkt genutzt in `PlaywrightServer.cs`" gestrichen.
- `inventory/code-usage.md` Z. 56 und `inventory/tests.md` Z. 55: beide bereinigt — using-Direktive als ungenutzt ausgewiesen, Override der Pin-Funktion zugeordnet.
- Verbleibende Treffer der falschen Formulierung existieren nur noch in `review-code.1.md`/`review-code.2.md` — dort korrekt als historische Befund-Beschreibung.

Faktische Korrektheit der korrigierten Texte verifiziert:

- Einziger AngleSharp-Treffer im Testprojekt-Code bleibt `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightServer.cs` Z. 2 (`using AngleSharp;`, kein API-Aufruf) — die Aussage „ungenutzt" stimmt.
- `obj/project.assets.json` des Testprojekts bestätigt: `bunit/2.15.2` deklariert `AngleSharp: 1.10.1` (transitives Minimum), aufgelöst `AngleSharp/1.10.5` — Kommentar- und Dokumentaussagen korrekt.
- Einzige verbliebene „direkt genutzt"-Aussage (`Microsoft.OData.Edm` in `inventory/packages.md` Z. 49) ist ein anderer, zutreffender Sachverhalt: `CsdlReader.Parse` in `Infrastructure/Services/ODataImportService.cs` Z. 64 verifiziert.

## Konsistenz der Korrekturen (keine neuen Inkonsistenzen)

- Zählwerte konsistent: `grep 'Version="10\.0\.\*"'` über `src/` ergibt exakt **17 Referenzen**; alle korrigierten Stellen (`inventory.md`, `inventory/packages.md` Z. 77, `plan.md` Z. 32) sprechen von 17 Referenzen / 13 Paketen. Keine Restvorkommen von „18"/„14" im Floating-Kontext außerhalb der historischen `review-code.1.md`-Befundbeschreibung.
- `inventory/packages.md` Z. 60 referenziert weiter „csproj Z. 13–16" — korrekt als Zeilenbezug auf den dokumentierten Inventur-Vorzustand (alte Kommentarzeilen im Snapshot mit deklariertem AngleSharp 1.5.0/bunit 2.7.2).
- `plan.md` Z. 58 verwendet weiter die Zeilennummern der Planungsvorlage (Vor-Änderungs-Stand) — für ein Planungsdokument konsistent.
- `todo.md`-Abhakungen (Schritte 5c–10) decken sich mit den vorhandenen Artefakten (`review.md`, `review-usability.md`, `test-results.md`, `review-code.{1,2}.md`).

## Kurzverifikation der csproj-Diffs

- **Versionen korrekt:** ShadcnBlazor 1.0.14→1.0.22 (Hauptprojekt Z. 24); Jint 4.9.2→4.18.1, Markdig 1.2.0→1.6.0, Http.Abstractions 2.3.0→2.3.14, SignalR.Core 1.2.10→1.2.14 (Infrastructure Z. 8–12); AngleSharp 1.5.0→1.10.5, bunit 2.7.2→2.15.2, coverlet.collector 10.0.0→10.1.1, Jint 4.9.2→4.18.1, NET.Test.Sdk 18.5.1→18.12.0, Moq 4.20.72→4.21.0 (Tests Z. 19–32). `Microsoft.Playwright` bleibt 1.52.0.
- **Floats unangetastet:** Keine der 17 `10.0.*`-Referenzen im Diff berührt; keine neue Floating-Referenz hinzugefügt.
- **Pkcs-Override unangetastet:** `System.Security.Cryptography.Pkcs` `10.0.*` inkl. GHSA-Kommentarblock (Infrastructure Z. 28–37) nicht im Diff.
- **Jint synchron:** 4.18.1 in Infrastructure (Z. 8) und Tests (Z. 25); `grep` über alle `.csproj` zeigt Jint/bunit/AngleSharp nur in diesen zwei Dateien.
- **Diff-Scope sauber:** Änderungen beschränken sich auf `Version`-Attribute und den AngleSharp-Kommentarblock (Tests Z. 13–18); Kommentar nennt nur noch die tragfähige Pin-Begründung.

## Geprüfte Dateien

Im Diff gegen `HEAD` (Basisbranch `staging`; alle Änderungen uncommitted):

- `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`
- `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj`
- `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj`
- `docs/features/task/issue-107-...-202/inventory.md`, `inventory/packages.md`, `inventory/code-usage.md`, `inventory/tests.md`, `plan.md`, `plan-check.md`, `todo.md`, `recommendation-report.md`, `review.md`, `review-code.1.md`, `review-code.2.md` (Lifecycle-Artefakte)
