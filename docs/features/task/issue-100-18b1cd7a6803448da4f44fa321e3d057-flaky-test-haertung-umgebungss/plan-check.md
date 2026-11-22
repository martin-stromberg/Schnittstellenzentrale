# Plan-Check – Flaky-Test-Härtung (Issue #100) – Nachprüfung

**Status: `Plan vollständig`**

Alle **8 Befunde** der Vorprüfung sind korrekt und vollständig eingearbeitet — die kritischen
xUnit-v2-Defekte (`ITraitAttribute`-Marker-Interface, falscher `Xunit.Abstractions`-Namespace)
sind an allen Stellen behoben. Bei der Nachprüfung wurden jedoch **zwei kleine, neu
aufgetretene bzw. nun sichtbare Inkonsistenzen** gefunden (Formel-Lücke in A1 durch das
`[OsInterface]`-Tagging der neuen `TempDirectoryCleanupTests`; unvollständige Attribut-Spez
für `FlakyAttribute` in AP 1). Beides Ein-Zeilen-Fixes, keine kritischen Defekte.

## Ergebnis je Befund der Vorprüfung

| # | Befund | Ergebnis | Beleg in plan.md |
|---|---|---|---|
| 1 | `ITraitAttribute` an allen 4 Attributen | **Eingearbeitet** | D1 Z. 170–174 (Pflicht, mit Begründung `GetCustomAttributes(typeof(ITraitAttribute))`); AP 1 Z. 234–240 (Pflicht-Checkbox) + explizit je Attribut: `OsInterface` Z. 241, `OsInterfaceFact` Z. 244, `OsInterfaceTheory` Z. 246, `Flaky` Z. 248 — je `: …, Xunit.Sdk.ITraitAttribute` |
| 2 | `Xunit.Sdk.ITraitDiscoverer` statt `Xunit.Abstractions` | **Eingearbeitet** | D1 Z. 167; AP 1 Z. 227–229 (expliziter Hinweis „nicht `Xunit.Abstractions` — dort existiert kein Trait-Typ"); Signatur `GetTraits(IAttributeInfo)` korrekt (`IAttributeInfo` aus `Xunit.Abstractions` ist richtig). Keine Reststelle mit falscher Qualifikation (Volltextsuche). |
| 3 | `README.md` + `.claude/css-conventions.md` in Doku-Scope | **Eingearbeitet** | Scope Z. 51–53; AP 8 Z. 455–459 (`README.md` ~Z. 170/176 beide Richtungen + Hinweis auf `os-interface-tests`-Job; `css-conventions.md` ~Z. 40); A10 Z. 510–513 inkl. Volltextsuche `FullyQualifiedName` |
| 4 | FR-11-Abweichung deklariert + Post-Merge-Plan | **Eingearbeitet** | Scope Z. 55–56 („deklarierte Abweichung von FR 11"); §7 Z. 566–576: explizite Abweichungs-Deklaration, **verbindliche** Post-Merge-Verifikation „erste ≥3 `staging-ci.yml`-Läufe", Benennung im Abschlussbericht/PR. Kein „1–2 Läufe"-Relikt mehr. |
| 5 | Readiness-Poll-Akzeptanzkriterium | **Eingearbeitet** | AP 5 Z. 321–325: **jede empfangene HTTP-Antwort** = ready (Status egal, auch 404/3xx); nur Verbindungsfehler/Timeouts = Retry-Fall — exakt die empfohlene Variante, inkl. Begründung. |
| 6 | Verwaiste `StopAsync`-Task-Beobachtung | **Eingearbeitet** | AP 5 Z. 341–349: `ContinueWith(…, OnlyOnFaulted)` auf `stopTask` nach `WhenAny`-Budget; `UnobservedTaskException` + verspätete Trace-ZIP benannt; `Tracing.StopAsync` ohne CancellationToken dokumentiert. |
| 7 | Quarantäne-Dedupe + Fork-Permissions | **Eingearbeitet** | Entscheidung 3 Z. 121–130 (primär `in:title "Quarantäne: <fqn>" --state open`, Body-Marker `<!-- flaky-quarantine:<fqn> -->` als Beleg/Fallback; Fork-Best-Effort: Warnung + Exit 0); AP 4 Z. 305–312 (identisch konkretisiert); Step-Kommentar Z. 419–420; A9 Z. 505–509 (Dedupe- + Rechte-Fehler-Verifikation); Risiken Z. 528–529. |
| 8 | TraitParity-Restrisiko dokumentiert | **Eingearbeitet** | Risiken Z. 527 (eigene Zeile, issue-konform + transparent); AP 8 Z. 448–451 verpflichtet die Doku-Datei `docs/help/stabilitaet/os-interface-tests.md` auf Audit-Heuristik + Restrisiko-Hinweis. |
| 9 | Kleinigkeiten | **Eingearbeitet/abhakt** | `OsInterfaceTheory` vorsorglich vermerkt (Z. 246–247); Verbotsregel Klasse+Methode (AP 3, Z. 283–286); `EndpointExecutionIntegrationTests`-Dispose als optionale Randnotiz (AP 6, Z. 385–388); `gh run download`-Fork-Toleranz (Z. 129–130, AP 4 Z. 304 „Exit immer 0"). |

## Restlücken (klein, aber konkret behebbar)

### R1. A1-Mengenformel berücksichtigt `TempDirectoryCleanupTests` nicht — Widerspruch zu Entscheidung 5/AP 6

- **Entscheidung 5** (plan.md Z. 157–159) und **AP 6** (Z. 368–369) legen fest, dass die neu
  angelegte `TempDirectoryCleanupTests`-Klasse `[OsInterface]` getaggt wird.
- **A1** (Z. 479–482) fordert aber: `Category=OsInterface` = alter `FullyQualifiedName~Playwright`-Satz
  **plus** `ImpressumServiceTests` (15 Methoden) — punktgenaue Gleichheit.
- Tatsächliche Menge = alter Satz + 15 + m (`m` = Methoden der `TempDirectoryCleanupTests`,
  AP 6 nennt mind. 3–4 Fälle). Der Mengenabgleich in **AP 9** (Z. 469–470, „s. Akzeptanzszene A1")
  läuft damit garantiert auf eine „Abweichung", die es nicht gibt — genau die Art
  Fehlalarm, gegen den das Akzeptanzszenario geschrieben wurde.
- **Anweisung:** A1-Formel erweitern, z. B. „… plus `ImpressumServiceTests` (15 Methoden)
  **plus neu angelegte `[OsInterface]`-Testklassen, konkret `TempDirectoryCleanupTests` (m Methoden)**"
  bzw. Formel auf „Parität der **Bestands**tests" präzisieren und die neue Klasse separat
  als Addendum nennen.

### R2. `FlakyAttribute` in AP 1 ohne explizites `[TraitDiscoverer]`/`AttributeUsage` — nur über D1-Generalklausel abgedeckt

- AP 1 Z. 241–247 spezifiziert für `OsInterfaceAttribute` explizit `[TraitDiscoverer("…CategoryTraitDiscoverer", "Schnittstellenzentrale.Tests")]` + `AttributeUsage(Class|Method)`;
  `OsInterfaceFact`/`OsInterfaceTheory` ziehen per „wie oben" nach.
- `FlakyAttribute` (Z. 248–250) nennt dagegen nur `: Attribute, Xunit.Sdk.ITraitAttribute`,
  `Category => TestCategories.Flaky` und XML-Doku — `[TraitDiscoverer]` und `AttributeUsage`
  fehlen in der itemisierten Spec (abgedeckt allenfalls durch die generische D1-Formulierung
  Z. 166 „Attribute tragen `[TraitDiscoverer]`" und Z. 177 „Klasse+Methode").
- Konsequenz bei wörtlicher Umsetzung: ohne `TraitDiscoverer` emittiert xUnit v2 keinen
  `Category=Flaky`-Trait; die Reflection-basierten Meta-Tests (`TestTraitReader`) blieben grün
  und A8 (`--filter "Category=Flaky"` → 0) fängt den Defekt nicht ab — das Retry-Skript fände
  später keine Flaky-Tests im TRX.
- **Anweisung:** In der `FlakyAttribute`-Zeile von AP 1 `[TraitDiscoverer(…CategoryTraitDiscoverer…)]`
  und `AttributeUsage(Class|Method)` explizit ergänzen (analog `OsInterfaceAttribute`).

## Sonstige Nachprüfung (kein Änderungsbedarf)

- **Keine Altreste:** Volltextsuche — keine Stelle mehr mit `Xunit.Abstractions.ITraitDiscoverer`;
  keine „1–2 Staging-Pushes"-Verwässerung; `in:title`-Dedupe konsistent an allen 4 Stellen
  (Entscheidung 3, AP 4, A9, Risiken).
- **Konsistenzcheck:** 23 abgeleitete Ctor-Umbauten (D5 Z. 211 vs. AP 5 Z. 335 „~23" — 24
  Klassen − `SitePageTests` = 23, stimmig); 25 Tagging-Markierungen (Scope Z. 39, AP 3,
  Entscheidung 5) beziehen sich korrekt auf Bestandsklassen — `TempDirectoryCleanupTests`
  wird bei Erstellung getaggt (nur A1-Formel betroffen, s. R1); OS-Job ohne `--collect`
  konsistent mit Entscheidung 2 (Z. 92–95, 409).
- **Vollständigkeit gg. requirement.md:** FR 1 (Audit→Entscheidung 5), FR 2 (AP 1+2),
  FR 3 (AP 7, beide Workflows + `release.yml`), FR 4 (AP 5 Deadline-Hierarchie), FR 5
  (AP 6), FR 6 (Retry-Konvention AP 4/AP 7), FR 7 (Analyzer+Persistenz Entscheidung 3),
  FR 8 (Entscheidung 2 inkl. Fallback), FR 9 (Entscheidung 4), FR 10 (AP 8/A10), FR 11
  (§7 inkl. deklarierter Abweichung) — alles abgedeckt; alle 5 offenen Punkte entschieden;
  Akzeptanzkriterien auf A1–A10 abgebildet; Nicht-Ziele (kein Blanket-Retry, In-Memory-SQLite
  regulär) in Scope-Ausschlüssen/Entscheidung 5 gespiegelt.


---

## Abschlussvermerk (Orchestrator, 2026-11-22)

Die zwei Restlücken der Nachprüfung wurden direkt behoben:

- **R1 (A1-Mengenformel):** `TempDirectoryCleanupTests` und die neuen regulären Meta-Tests
  sind in der A1-Formel berücksichtigt (plan.md, Akzeptanzszenarien A1).
- **R2 (`FlakyAttribute`-Spec):** `[TraitDiscoverer(...)]` + `AttributeUsage(Class|Method)`
  ergänzt (plan.md, AP 1).

Damit ist der Plan vollständig.
