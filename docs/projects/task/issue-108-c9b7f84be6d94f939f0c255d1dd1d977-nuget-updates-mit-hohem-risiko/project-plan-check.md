# Projektplan-Prüfung – .NET-11-Upgrade + NuGet-Updates mit hohem Risiko (Issue #108)

Geprüft: `project-plan.md` gegen `requirement.md` und `inventory.md`; technische Stichproben gegen den Repo-Stand (csproj-Referenzen, `global.json`, CI-Workflows, genannte Codestellen).

## Ergebnis

**Status:** Projektplan vollständig

## Abgleich Anforderung ↔ Entwicklungsschritte

| Anforderung (requirement.md) | Abdeckende(r) Schritt(e) | Abdeckungsgrad |
|---|---|---|
| Teil A: TFM `net11.0` in allen 4 Projekten | Schritt 1 | voll |
| Teil A: `global.json` → .NET-11-SDK | Schritt 1 (`10.0.301` → `11.0.100`, `rollForward` beibehalten) | voll |
| Teil A: 9 CI-Pins `10.0.x` → `11.0.x` | Schritt 1 (alle Fundstellen inkl. `build-and-package/action.yml` benannt) | voll |
| Teil A: harte net10-Nennungen (`--framework net10.0` in `action.yml:57`, `PlaywrightTestBase.cs:27`-Kommentar, README, Hilfsdoku) | Schritt 1 | voll |
| Teil A: Umgang mit 17 `10.0.*`-Floatings (gekoppelt an F7) | Schritt 1 (Heben auf `11.0.*` unter dokumentierter F7-Arbeitsannahme „Floating beibehalten"; Nacharbeit in Schritt 9 bei Nicht-Default) | voll |
| F1: `Microsoft.Playwright` 1.52.0 → 1.65.1 | Schritt 7 (inkl. Pflicht-Sichtung Zwischenreleases 1.53–1.64, ~400 Aufrufe, `InstallPlaywright`-Target/Browser, CI-Cache, lokales Merge-Gate für best-effort-Lane) | voll |
| F2: `xunit` 2.9.3 → `xunit.v3`, Runner → 4.x | Schritt 6 (Runner-Zwischenschritt) + Schritt 8 (`OutputType Exe`, `IAsyncLifetime`→`ValueTask`, `Xunit.Abstractions`-Wegfall, `Xunit.Sdk`-Trait-System-Neuaufbau, MTP-/`dotnet test`-/Coverlet-Verhalten, Doku-Anpassung) | voll |
| F3: `Microsoft.OpenApi` → 3.x + `Swashbuckle.AspNetCore` → 11.x, **ein gemeinsamer PR** | Schritt 5 (ausdrücklich ein Schritt; Begründung 2/3-Mix über Core-`OpenApiDocument`-Signatur und `ISwaggerProvider`-Kopplung des `SystemEndpointSyncService`) | voll |
| F4: `Serilog.AspNetCore` → 11.0.0 | Schritt 4 (Changelog-Prüfung, Sink-Kompatibilität File 7.0.0/EventLog 4.0.0, gezielte Log-Verifikation gegen stillen Ausfall) | voll |
| F5: `Microsoft.OData.Edm` → 9.0.0 + `AspNetCore.OData`-Strategie | Schritt 3 (Edm 9.0.0, Fallback 8.4.4, `ODataImportService`-API-Check, Strategie-Entscheidung mit Exit-Kriterium in `decisions.md`) | voll |
| F6: `FrameworkReference Microsoft.AspNetCore.App` bzw. Minimalvariante | Schritt 2 (Default: volle Variante — `Http.Abstractions 2.3.0` + `SignalR.Core 1.2.10` entfernen; verbindlich in beiden Varianten: `SignalR.Client` ins Web-Projekt **verschieben** — Issue-Korrektur korrekt übernommen; bunit-Verträglichkeits-Check) | voll |
| F7: Floating vs. Lock/CPM (Prozess-Entscheidung) | Schritt 9 (alle drei Varianten a/b/c benannt, Default (a) begründet, Nicht-Default-Umsetzung im Schritt vorgesehen) | voll |
| F8: npm-/Actions-Pins außerhalb NuGet-Scope | Schritt 9 (Default: separates Issue/Skizze; wird nicht implementiert — Nicht-Ziel respektiert) | voll |
| Entscheidungen dokumentieren (F5, F6, F7, F8) | Schritte 2, 3, 9 → `decisions.md` + Issue-Kommentare; Konsolidierung als Akzeptanzkriterium in Schritt 9 | voll |
| Ablauf: Testinfrastruktur getrennt + sequenziell (Runner → Playwright → xunit.v3 zuletzt) | Schritte 6 → 7 → 8 mit passenden Abhängigkeiten | voll |
| Ablauf: F3 und F4 als getrennte PRs | Schritte 4 und 5 getrennt | voll |
| Ablauf: kein Sammel-PR; jeder Schritt eigener PR gegen `staging` | alle Schritte („PR gegen `staging`" jeweils in den Akzeptanzkriterien) | voll |
| Ablauf: Abschluss-PR mit `Closes #108` | Schritt 9 | voll |
| Konfiguration: keine `packages.lock.json`/CPM ohne F7-Entscheidung | Schritt 9 (Default = reine Dokumentation; Lock/CPM nur bei abweichender Wahl) | voll |
| Konfiguration: verbindliches F1-Absicherungs-Vorgehen trotz best-effort-Lane | Schritt 7 (lokales Gate als Merge-Pflicht, TRX-Verlinkung im PR) | voll |
| Nicht-Ziele (kein Sammel-PR, keine F8-Umsetzung, keine Scope-Erweiterung, staging-Konvention, kein eigenmächtiger OData-Ersatz) | überall respektiert; OData-Default „auf preview.2 verharren" | voll |
| Offene Fragen 1–7 aus requirement.md | alle adressiert: OF1→Schritt 3, OF2→Schritt 2, OF3→Schritt 9, OF4→Schritt 7, OF5→Schritt 8, OF6→Schritt 9, OF7→Vorgehensentscheidung 1; zusätzlich Abschnitt „Offene Punkte" mit begründeten Annahmen | voll |

## Abhängigkeitsprüfung

Abhängigkeitsgraph: 1 → {2, 3, 4, 5, 6}; 6 → 7 → 8; 9 → {2–8}. Alle Verweise zeigen ausschließlich auf frühere Schritte, der Graph ist zyklenfrei und fachlich sinnvoll:

- Testinfrastruktur-Schritte sind strikt sequenziell verkettet (6→7→8) — exakt die Requirement-Vorgabe, damit die absichernden Tests nicht aus zwei Richtungen verändert werden.
- Schritt 3 (F5) hängt korrekt an Schritt 1, da die preview.2-Lauffähigkeit unter net11 dort verifiziert wird.
- Schritt 7 hängt nur explizit an 6; die Abhängigkeit zu 1 ergibt sich transitiv — ausreichend.
- Paket-Kopplungen werden nicht verletzt: OpenApi↔Swashbuckle liegt zwingend in einem gemeinsamen Schritt (5); `SignalR.Client`-Verschiebung (Schritt 2) folgt auf das Floating-Heben in Schritt 1 und ist im Schritttext konsistent formuliert („`10.0.*` bzw. `11.0.*` nach Schritt 1").

Abweichung vom Requirement-Ablauf 1 („Entscheidungen zuerst"): Der Plan legt das .NET-11-Basisupgrade bewusst an den Anfang und führt die Entscheidungen als dokumentierte Annahmen mit Defaults in den jeweiligen Schritten (2, 3, 9). Dies ist transparent begründet (Vorgehensentscheidung 1, Behandlungsstand im Abschnitt „Offene Punkte") und beantwortet zugleich die offene Frage 7 — fachlich vertretbar, keine Lücke.

## Fehlende oder unvollständige Punkte

Keine.

## Hinweise

1. **Akzeptanzkriterium `dotnet list package --vulnerable` fehlt in Schritt 8 (und 6).** Das Requirement fordert es „pro umgesetztem Update/Migration (F1–F5)" — F2 gehört dazu. In Schritten 1, 3, 4, 5, 7 enthalten; in 6/8 nur implizit über das CI-Security-Scan-Gate abgedeckt. Explizite Ergänzung wäre konsistenter (nicht blockierend).
2. **Zielversionsnummern sind Stichtagsangaben** (`Microsoft.OpenApi 3.11.1`, `Swashbuckle.AspNetCore 11.0.1`, `xunit.runner.visualstudio 4.0.1`, `xunit.v3 4.0.3`) und gegen den Repo-Stand nicht verifizierbar. Der Plan mildert das durch die Klausel „Zielversion(en) zum Umsetzungszeitpunkt (erneut) konkretisieren" ab — ausreichend.
3. **Hart kodierte Baseline „641 Tests"** in den Akzeptanzkriterien der Schritte 1 und 6: falls frühere Schritte die Testanzahl verändern (z. B. Schritt 5-Fixtures), sollte die Baseline jeweils neu gemessen statt wörtlich übernommen werden.
4. **Bewusste Abweichung von Ablauf-Punkt 1** („Entscheidungen zuerst — sie rahmen alle weiteren Updates"): Die F5-/F7-Entscheidungen fallen erst in Schritt 3 bzw. 9 formal, nachdem Schritte 1–2 bereits auf Annahmen umgesetzt haben. Da Annahmen und Übersteuerbarkeit dokumentiert sind, bleibt das Risiko klein; der Abschluss-PR sollte die getroffenen Annahmen wie angekündigt transparent machen.
5. **Übergabefähigkeit ist gegeben:** Jeder Schritt trägt Ziel/Umfang mit konkreten Dateipfaden und Zeilenbezügen, prüfbare Akzeptanzkriterien (aus dem Requirement ableitbar), Abhängigkeiten und betroffene Bereiche — eigenständig als Lifecycle-Eingabe verständlich. Einzige externe Referenz (`inventory.md` für die Aufruf-Zählung in Schritt 7) ist mit vollem Pfad angegeben.
