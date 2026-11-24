# Plan-Prüfung — Schritt 7: Microsoft.Playwright → 1.65.1

**Status: Plan vollständig**

Geprüft gegen `requirement.md` und `inventory.md` (inkl. `inventory/release-notes-befund.md`).

## Prüfpunkte

| Kriterium | Befund |
|-----------|--------|
| Anforderungsabdeckung | Alle F1-Punkte abgedeckt: Version-Bump, Release-Notes-Sichtung 1.53–1.64 (erledigt, dokumentiert), ~600 Aufrufe geprüft (API-Inventar + entfernte-APIs-Audit, 0 Treffer), Infrastruktur-Dateien benannt, Browser-Neuinstallation via Build-Target, CI-Cache-Befund, lokales Gate als Merge-Pflicht, alle 5 Akzeptanzkriterien als Szenarien A1–A5. |
| Breaking-Changes-Vollständigkeit | Alle Releases 1.53.0–1.65.1 einzeln gesichtet (gh release view, playwright-dotnet). Entfernte APIs vollständig gegen Repo grept — keine Nutzung. Verhaltensänderungen (inert, Cookies, iframes, ungerenderter Text, Proxy) gegen App- und Test-Code geprüft — keine Muster vorhanden. |
| Testbedarf | Reguläre Lane (641-Baseline) + OsInterface-Lane als reale E2E-Abnahme; Flaky-Konvention benannt; Retry-Pfad dokumentiert. |
| Scope-Ausschlüsse | Vorhanden und technisch begründet (keine neuen APIs, keine Flaky-Vergabe ohne Ereignis, keine CI-Änderung). Keine `offen`-Einträge. |
| Offene Punkte | Keine — Implementierung darf starten. |
| Risiken | Benannt (Chrome-for-Testing, Accessible Names, Zeitbudget) mit Reaktionsweg. |

## Hinweis

Plan deckt den Fall „echte Brüche trotz sauberem Audit" explizit ab (Schritt 3 + Risiken). Die verbindliche Entscheidungsgrundlage ist der lokale OsInterface-Lauf.
