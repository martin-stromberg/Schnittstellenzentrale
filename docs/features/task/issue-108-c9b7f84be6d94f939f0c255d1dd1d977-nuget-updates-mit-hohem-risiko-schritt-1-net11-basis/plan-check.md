# Plan-Check – .NET-11-Basisupgrade

**Status: Plan vollständig**

Geprüft: `plan.md` gegen `requirement.md` und `inventory.md` (Schritt ohne Unteragenten — Selbstprüfung mit expliziter Abdeckungsmatrix).

## Abdeckungsmatrix Anforderung → Plan

| Anforderung | Plan-Element | Abgedeckt |
|---|---|---|
| 4 × TFM `net10.0` → `net11.0` | Schritt 2 (alle 4 Pfade benannt) | voll |
| `global.json` 10.0.301 → 11.0.100, rollForward beibehalten | Schritt 1 | voll |
| 9 × `dotnet-version '10.0.x'` → `'11.0.x'` | Schritt 5 (Fundstellen aus inventory.md: pr-staging-ci 3, staging-ci 3, security-scan 1, release 1, action.yml 1) | voll |
| `--framework net10.0` in action.yml | Schritt 5 | voll |
| 17 Floating `10.0.*` → `11.0.*` | Schritt 3 (2 + 11 + 4 = 17, deckt sich mit inventory.md Zählung) | voll |
| Pkcs-Kommentar an 11.x anpassen | Schritt 4 + Annahme A3 | voll |
| AngleSharp-Kommentar prüfen | Schritt 4 (version-neutral, unverändert) | voll |
| `PlaywrightTestBase.cs:27`-Kommentar | Schritt 6 | voll |
| README.md / installation.md ×2 | Schritt 6 | voll |
| `Microsoft.AspNetCore.OData 10.0.0-preview.2` bleibt gepinnt | Scope-Ausschluss + A4 + S3 | voll |

## Abdeckungsmatrix Akzeptanzkriterien → Verifikation

| Akzeptanzkriterium | Verifikation im Plan |
|---|---|
| `dotnet build` unter SDK 11.0.100 fehlerfrei | S1 / Schritt 7.1 |
| `dotnet test --filter "Category!=OsInterface"` grün (Baseline 641) | S2 / Schritt 7.2 |
| 78 `ODataControllerIntegrationTests` grün | S3 / Schritt 7.3 |
| `dotnet list package --vulnerable` ohne Befund | S4 / Schritt 7.4 |
| keine `10.0.*`-/`net10.0`-Nennungen außerhalb historischer Dokumente | S5 / Schritt 7.5 (+ A1 grenzt gepinnte Paketversionen ab) |
| Reale E2E-Abnahme (GET /, OData) | S6 / Schritt 7.6 |

## Risiken und Lücken (geprüft, ohne Befund)

- **NU1202-Risiko preview.2 unter net11:** im Plan als A4 adressiert (Eskalation statt eigenmächtiger Paketwechsel) — korrekt, da ein Paketwechsel Scope-Verletzung wäre.
- **Pkcs-Kommentar:** A3 vermeidet erfundene Patch-Versionsnummern — korrekt.
- **release.yml-Pin unquoted** (`dotnet-version: 10.0.x`): im Plan über „je Stil der Zeile" abgedeckt.
- **`Setup .NET 10`-Schrittnamen / action.yml-Description:** nicht wörtlich im Requirement, aber durch A2 abgedeckt — konsistent mit dem Sauberkeitskriterium.
- **OsInterface-Lane:** korrekt ausgenommen (Auftrag: nur reguläre Lane).
- **E2E:** anonym erreichbare Endpunkte verifiziert (`[AllowAnonymous]` auf `ApiControllerBase`/`ODataControllerBase`, keine Fallback-Policy in `Program.cs`) — S6 durchführbar.
- **Keine UI-Änderung:** reine Build-/CI-/Doku-Baseline → kein Usability-E2E-Szenario nötig; Sichtprüfung nicht anwendbar (keine Screenshots).

## Ergebnis

Der Plan deckt die Kundenanforderung vollständig ab; alle Akzeptanzkriterien sind überprüfbar verankert. Keine Lücken festgestellt.
