# Abnahmeprüfung – Entwicklungsschritt 1

## Ergebnis

**Status:** Abweichungen gefunden

## Abweichungen

- [ ] `README.md:163` enthält weiterhin die harte net10-Nennung „**Technologien:** ASP.NET Core 10 · Blazor Server · Entity Framework Core 10 · … · Microsoft.AspNetCore.OData 10 Preview · …“. Die Schrittanforderung verlangt „harte net10-Nennungen korrigiert (…, README.md, …)“; in derselben Datei wurden die Badges auf .NET 11 / EF Core 11 gehoben, sodass die Datei nun intern widersprüchlich ist (Badge: EF_Core-11 vs. Fließtext: Entity Framework Core 10). Die Nennung „Microsoft.AspNetCore.OData 10 Preview“ in derselben Zeile ist dagegen korrekt (Paket bleibt auf 10.0.0-preview.2 gepinnt) und nicht Teil der Abweichung.

## Hinweise

Geprüfter Diff: `git diff task/issue-108-…-nuget-updates-mit-hohem-risiko...HEAD` — 18 geänderte Dateien, alle scope-konform (4 csproj, global.json, 4 Workflows + 1 Composite Action, PlaywrightTestBase.cs, README.md, CLAUDE.md, RELEASE_NOTES.md, 4 Hilfs-Docs). Keine scope-fremden Änderungen.

Nachgewiesene Anforderungen:

- **TFMs:** Alle 4 Projekte auf `net11.0` (`Schnittstellenzentrale.csproj:38`, `Core.csproj:8`, `Infrastructure.csproj:41`, `Tests.csproj:4`).
- **global.json:** SDK `11.0.100`, `rollForward: latestFeature` beibehalten; `dotnet --version` löst zu 11.0.100 auf (installiert: 10.0.404, 11.0.100).
- **CI-Pins:** Alle 9 `dotnet-version`-Fundstellen auf `11.0.x` (action.yml:41; pr-staging-ci.yml:81,126,226; release.yml:52; security-scan.yml:31; staging-ci.yml:74,107,202). Kein `10.0.x`-Pin verblieben.
- **Publish-Framework:** `--framework net11.0` in `build-and-package/action.yml` (Zeile 57).
- **Floating-Referenzen:** Alle 17 gezählt und auf `11.0.*` gehoben — Infrastructure 11 (SignalR.Client, EF×4, Extensions×4, JSInterop, Pkcs), Tests 4 (Mvc.Testing, EF Sqlite, EF SqlServer, JSInterop), Web 2 (Negotiate, EF.Design). Kein `10.0.*` mehr in Quellen.
- **Pkcs-Kommentar** (`Infrastructure.csproj:28-37`): Auf die 11.x-Linie angepasst, GHSA-Bezüge (846f-f764-76mx / 2fmv-rv2c-59x7) erhalten, keine erfundenen Patch-Versionsbehauptungen („every 11.x release is patched" — durch Vulnerability-Scan gedeckt).
- **AngleSharp-Kommentar** (`Tests.csproj:13-16`): Enthält keine .NET-Versionsaussage (Pin 1.5.0, GHSA-pgww-w46g-26qg) — weiterhin korrekt, keine Änderung nötig.
- **OData-Pin:** `Microsoft.AspNetCore.OData 10.0.0-preview.2` unverändert gepinnt (`Schnittstellenzentrale.csproj:16`).
- **Harte net10-Nennungen:** `PlaywrightTestBase.cs:27` (Kommentar `bin/<cfg>/net11.0/`), `docs/help/schnittstellenzentrale/installation.md:6`, `docs/help/playwright-tests/installation.md:5`, README-Badges + Laufzeitanforderung — korrigiert; README.md:163 jedoch übersehen (s. Abweichung). Zusätzlich konsistent nachgezogen: CLAUDE.md-Stackzeile, `docs/help/lokalisierung/installation.md`, `ablauf-technisch.md`, RELEASE_NOTES-Eintrag.
- **Grep-Restbefund** (`10.0.*|net10.0|10.0.x` sowie `.NET 10|ASP.NET Core 10` o. ä.): Nur noch in historischen Dokumenten (`issue.md`, `docs/RELEASE_NOTES.md` als Migrationsbeschreibung, `docs/projects/task/…` Planungs-/Inventar-Dokumente, `docs/features/…/test-results.md`), in der gitignorierten `.softwareschmiede-scm-exchange/` und in bin/obj-Artefakten. Gepinnte Folgeschritt-Pakete (preview.2, Swashbuckle 10.1.7, Serilog 10.0.0, coverlet 10.0.0) sind bewusst belassen — keine Abweichung.

Verifikationsläufe unter SDK 11.0.100 (lokal nachvollzogen):

- `dotnet build Schnittstellenzentrale.slnx` → **0 Fehler**, 33 Warnungen (BL0016/NU-Analysen, präexistent), 9,4 s.
- `dotnet test --filter "Category!=OsInterface" --no-build` → **641/641 bestanden** (Baseline 641 exakt getroffen), 18 s, net11.0.
- `dotnet test --filter "FullyQualifiedName~ODataControllerIntegrationTests" --no-build` → **78/78 bestanden**, 2 s — Lauffähigkeit des net10-only-Pakets `Microsoft.AspNetCore.OData 10.0.0-preview.2` unter net11 damit verifiziert.
- `dotnet list package --vulnerable` → **kein Befund** in allen 4 Projekten (nur NU1510-Hinweise auf präexistent überflüssige Abstractions-Referenzen).

Einschränkung: Die vom Schritt gemeldete E2E-Verifikation (GET / → 200, POST /authenticate → 200, GET /odatav4/Applications → 200 gegen laufende App) wurde nicht eigenständig wiederholt; sie ist durch die 78 grünen `ODataControllerIntegrationTests` (WebApplicationFactory, echte HTTP-Roundtrips inkl. OData-Routen) ausreichend plausibilisiert.
