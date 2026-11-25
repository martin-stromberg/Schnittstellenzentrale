# Release Notes aktualisieren

Erstellt oder aktualisiert `docs/RELEASE_NOTES.md` im Projektstammverzeichnis. Das Dokument fasst die für die nächste Veröffentlichung angesammelten Änderungen zusammen — stichwortartig, kurz und knapp, zweisprachig (zuerst Englisch, danach Deutsch).

**Ziel:** Anwender und Administratoren vor einem Update kurz informieren: Was muss vor dem Update beachtet werden (insbesondere Breaking Changes), und was ist neu.

---

## Schritt 1: Änderungsbasis ermitteln

Prüfe, ob ein Branch `staging` existiert (lokal oder remote):

```
git show-ref --verify --quiet refs/heads/staging || git show-ref --verify --quiet refs/remotes/origin/staging
```

- **`staging` existiert:** Ermittle alle Commits, die in `staging` liegen, aber nicht im Hauptbranch (`main`):
  ```
  git log --oneline $(git merge-base main staging)..staging
  git diff --name-status $(git merge-base main staging)..staging
  ```
- **`staging` existiert nicht:** Verwende stattdessen den aktuellen Feature-Branch gegenüber `main`:
  ```
  git log --oneline $(git merge-base main HEAD)..HEAD
  git diff --name-status $(git merge-base main HEAD)..HEAD
  ```

Lies die relevanten Commit-Messages und, bei unklarem Inhalt, die zugehörigen Diffs, um den fachlichen Inhalt jeder Änderung zu verstehen.

## Schritt 2: Änderungen kategorisieren

Ordne jede identifizierte fachliche Änderung genau einer Kategorie zu:

| Kategorie | Kriterium |
|---|---|
| Wichtiger Hinweis | Breaking Change, notwendiger manueller Migrationsschritt, geänderte Konfiguration/Umgebungsvariablen, entfallene Funktion, Änderung mit Auswirkung auf bestehende Daten oder Integrationen |
| What's New | Alle übrigen fachlichen Änderungen: neue Features, Verbesserungen, Bugfixes |

**Wichtig:** Unter „Wichtige Hinweise" stehen ausschließlich Punkte, die ein Administrator vor dem Update zwingend kennen muss. Normale Bugfixes oder kleinere Verbesserungen gehören nicht dorthin. Gibt es keine derartigen Hinweise, wird dies explizit vermerkt (siehe Schritt 3).

Rein technische/interne Änderungen ohne Auswirkung auf Endanwender (z. B. reines Refactoring, Testergänzungen, CI-Anpassungen) werden weggelassen, sofern sie nicht unter „Wichtiger Hinweis" fallen.

## Schritt 3: Dokument erstellen oder aktualisieren

`docs/RELEASE_NOTES.md` beschreibt immer den vollständigen aktuellen Änderungsstand der Ermittlungsbasis aus Schritt 1 — die Datei wird bei jedem Lauf vollständig neu geschrieben (kein Anhängen). Existiert sie nicht, wird sie neu angelegt.

Struktur:

```markdown
# Release Notes

## Important Notes Before Update

- {Stichpunkt}

## What's New

- {Stichpunkt}
- {Stichpunkt}

## Wichtige Hinweise vor dem Update

- {Stichpunkt}

## Neuerungen

- {Stichpunkt}
- {Stichpunkt}
```

Gibt es keine Einträge für „Important Notes Before Update" / „Wichtige Hinweise vor dem Update", trägt der Abschnitt genau einen Punkt ein:
- Englisch: `- There are no special notices.`
- Deutsch: `- Es gibt keine besonderen Hinweise.`

**Regeln:**
- Stichpunkte kurz und knapp — ein Satzfragment pro Punkt, keine Fließtext-Absätze.
- Englischer und deutscher Abschnitt inhaltlich deckungsgleich (Übersetzung, keine unterschiedlichen Inhalte).
- Keine erfundenen Inhalte — nur tatsächlich im Diff bzw. den Commit-Messages erkennbare Änderungen.
- Reihenfolge innerhalb eines Abschnitts: neueste Änderung zuerst.
