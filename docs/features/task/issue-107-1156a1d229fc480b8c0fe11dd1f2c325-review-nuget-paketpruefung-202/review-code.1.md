# Code-Review

## Ergebnis

**Status:** Befunde vorhanden

## Befunde

### Schnittstellenzentrale.Tests.csproj

- **Inkorekte Kommentar-Begründung (faktischer Fehler)** — Z. 16–17: Der neue AngleSharp-Override-Kommentar behauptet „AngleSharp is used directly in Playwright/Infrastructure/PlaywrightServer.cs". Verifiziert falsch: `src/Schnittstellenzentrale.Tests/Playwright/Infrastructure/PlaywrightServer.cs` enthält in Z. 2 nur eine **ungenutzte** `using AngleSharp;`-Direktive — im gesamten Testprojekt existiert kein Aufruf einer AngleSharp-API (keine Treffer für `Configuration.`, `BrowsingContext`, `HtmlParser`, `QuerySelector`, `Parse*` o. ä.). Die zweite im Kommentar genannte Begründung (Pin hält die Auflösung auf der neuesten gepatchten Linie 1.10.5 statt des transitiven Minimums 1.10.1 aus bunit 2.15.2) ist dagegen korrekt und allein tragfähig — der Override bleibt funktional und sinnvoll.

  Empfehlung: Kommentar korrigieren — die Direktnutzungs-Behauptung streichen und den Pin ausschließlich mit dem Versions-Pinning auf die gepatchte Linie begründen (z. B. „…the pin keeps the resolution on the latest patched release (1.10.5) instead of bunit's transitive minimum 1.10.1"). Nebenbefund außerhalb des Diff-Scopes: das verwaiste `using AngleSharp;` in `PlaywrightServer.cs` Z. 2 ist toter Code und kann bei Gelegenheit entfernt werden.

### recommendation-report.md / review.md (Lifecycle-Artefakte)

- **Faktischer Zählfehler** — `recommendation-report.md` Z. 60 spricht von „14 Floating-`10.0.*`-Pakete (18 Referenzen)", Z. 141 wiederholt „18 Floating-Referenzen"; `review.md` Z. 22 übernimmt „alle 18 Floating-Referenzen". Tatsächlicher Stand in der Solution: **17** `Version="10.0.*"`-Referenzen über **13** Pakete (inkl. `System.Security.Cryptography.Pkcs`; 2× `Microsoft.JSInterop`, 2× EFCore.Sqlite, 2× EFCore.SqlServer, 2× EFCore.Design, je 1× EFCore, SignalR.Client, Mvc.Testing, Authentication.Negotiate, Ext.Http, Ext.DI.Abstractions, Ext.Configuration.Binder, Ext.Configuration.Abstractions, Pkcs). Die Aussage „unverändert" und die Auflösung auf 10.0.15 sind korrekt — nur die Anzahl stimmt nicht.

  Empfehlung: Zählwerte auf „13 Pakete / 17 Referenzen" korrigieren (oder „inkl. Pkcs-Override" explizit kennzeichnen, falls nur ohne Override gemeint war — dann 12 Pakete / 16 Referenzen).

## Geprüfte Dateien

Im Diff gegen `HEAD` (Basisbranch `staging`; alle Änderungen uncommitted):

- `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj` — ShadcnBlazor 1.0.14 → 1.0.22 (existiert, aktuellste stabile Version)
- `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj` — Jint 4.9.2 → 4.18.1, Markdig 1.2.0 → 1.6.0, Http.Abstractions 2.3.0 → 2.3.14, SignalR.Core 1.2.10 → 1.2.14 (alle auf nuget.org vorhanden, jeweils aktuellste stabile Version)
- `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj` — AngleSharp 1.5.0 → 1.10.5 + Kommentar, bunit 2.7.2 → 2.15.2, coverlet.collector 10.0.0 → 10.1.1, Jint → 4.18.1, NET.Test.Sdk 18.5.1 → 18.12.0, Moq 4.20.72 → 4.21.0 (alle verifiziert)
- `docs/features/task/issue-107-...-202/recommendation-report.md`, `review.md`, `review-usability.md`, `todo.md`, `...-202-tasks.md` (Lifecycle-Artefakte)

## Verifikationsdetails (keine Befunde)

- **AngleSharp-Transitive:** `obj/project.assets.json` des Testprojekts bestätigt: bunit 2.15.2 fordert `AngleSharp >= 1.10.1`, `AngleSharp.Css >= 1.4.1`; aufgelöst `AngleSharp/1.10.5` (aktuellste stabile Version laut NuGet-Index), AngleSharp.Css 1.4.1, AngleSharp.Diffing 1.1.1. GHSA-pgww-w46g-26qg via GitHub-Advisory-API verifiziert: AngleSharp, mXSS via annotation-xml, betroffen `< 1.5.0`, gepatcht 1.5.0 — Verweis im Kommentar korrekt.
- **Overrides unangetastet:** `System.Security.Cryptography.Pkcs` `10.0.*` inkl. Begründungs-Kommentar (Infrastructure Z. 28–37) und alle 17 Floating-`10.0.*`-Einträge exakt unverändert; keine neue Floating-Referenz hinzugefügt. Auflösung der Floats auf 10.0.15 in `project.assets.json` bestätigt.
- **Konsistenz Jint:** 4.18.1 in Infrastructure (Z. 8) und Tests (Z. 26) synchron; keine weiteren Jint-/bunit-/AngleSharp-Referenzen in anderen Projekten (Core hat nur `Microsoft.OpenApi`); kein `Directory.Packages.props`, keine `packages.lock.json` — keine vergessene Referenz.
- **Datei-Integrität:** BOM in Tests- und Infrastructure-csproj identisch zum HEAD-Stand (EF BB BF), Haupt-csproj weiterhin ohne BOM; durchgehend CRLF; Diff beschränkt sich auf `Version`-Attribute und den Kommentarblock — keine Whitespace-/XML-Strukturänderungen.
- **Artefakt-Konsistenz:** `tasks.md` (17 erledigt / 2 offen: Folge-Issue + PR), `review.md` (Status „Offene Aufgaben vorhanden", dieselben zwei Punkte) und `todo.md` stimmen überein; `Microsoft.Playwright`-Nicht-Update (dokumentierte Breaking Changes → hoch) ist über alle drei Artefakte kohärent begründet. Stichprobe externer Fakten bestätigt: AspNetCore.OData 10.0.0-preview.2 ist letztes 10.x-Release, Swashbuckle 11.0.1, Serilog.AspNetCore 11.0.0, OpenApi 3.11.1, xunit.runner.visualstudio 4.0.1, OData.Edm 9.0.0.
