# Entscheidungen zu den offenen Fragen (requirement.md, Abschnitt „Offene Fragen")

Festgelegt durch den Bearbeitungslauf vom 19.11.2026. Da Rückfragen im Issue
asynchron wären und die Aufgabe („Erledige diese Aufgabe") eine eigenständige
Umsetzung verlangt, wurden die Vorschläge aus `requirement.md` wie folgt
beschlossen:

1. **Deployment-Branch:** Der Pages-Workflow deployt bei Push auf `staging`
   **und** `main` (jeweils mit `paths`-Filter auf die Seitenquellen) sowie per
   `workflow_dispatch`. Begründung: Das Akzeptanzkriterium verlangt eine
   öffentlich erreichbare Page; mit `staging` ist sie bereits nach dem PR-Merge
   live, mit `main` bleibt sie nach Promotion aktuell. Die Seite ist
   Präsentationsfläche, kein Release-Artefakt — ein Stand aus `staging` ist
   vertretbar.
2. **Ablageort:** Dedizierter Ordner `site/` im Repository-Root +
   Actions-Deployment (`upload-pages-artifact` mit `path: site/`). Der komplette
   `docs/`-Baum (`help/`, `features/`, `RELEASE_NOTES.md`) wird **nicht**
   veröffentlicht.
3. **Sprache:** Deutsch (einstufig, ohne Umschalter) — Issue und
   Repository-Dokumentation sind deutsch; ein Sprachumschalter ist nicht
   gefordert und kann später nachgerüstet werden.
4. **Screenshots:** Lokal/per Lauf erzeugte, eingecheckte PNGs über eine
   dedizierte Playwright-Screenshot-Klasse auf Basis der vorhandenen
   `PlaywrightServer`/`TestDatabaseSeeder`-Infrastruktur (Kestrel auf
   `http://127.0.0.1:5099`). Keine Screenshot-Generierung im Deploy-Workflow.
   Mindestansichten: Startseite/Workspaces, eine Anwendungs- oder
   Endpunktansicht, Dark-Mode-Variante.
5. **Pages-Aktivierung:** Wird in diesem Lauf bereits per `gh api`
   (Source „GitHub Actions", Admin-Token des Repository-Inhabers) erledigt;
   die Hilfe-Doku beschreibt den Schritt zusätzlich für Wiederholungsfälle.
6. **Release-Verweis:** Stabile URL `releases/latest` plus Hinweis auf das
   ZIP-Asset `Schnittstellenzentrale-vX.Y.Z.zip`; optional zusätzlich ein
   Release-Badge (shields.io). Kein `release`-Event-Trigger, keine
   Versionsnummer-Auflösung zur Laufzeit nötig.
7. **Rechtliches:** Footer-Link auf `LEGAL.md` im Repository.
