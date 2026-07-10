# Detailinventar: CI/CD und Release-Konfiguration

## Vorhandene CI/CD-Dateien

Es existiert keine `.github/`-Struktur. Damit sind keine GitHub-Actions-Workflows, keine wiederverwendbaren Actions und keine Repository-lokalen Action-Konfigurationen vorhanden.

Gefundene Release-Konfigurationen:

| Datei/Typ | Status |
|---|---|
| `.github/workflows/*.yml` | Nicht vorhanden |
| `.github/workflows/*.yaml` | Nicht vorhanden |
| `.releaserc` | Nicht vorhanden |
| `release.config.js` / `release.config.cjs` | Nicht vorhanden |
| `package.json` / `package-lock.json` | Nicht vorhanden |
| `global.json` | Nicht vorhanden |

## GitHub-/Release-Bezug

- `origin` zeigt auf ein GitHub-Repository. Fuer neue Workflow-Dateien duerfen keine lokalen Zugangsdaten aus der Remote-URL uebernommen werden.
- Lokale Tags: keine Tags gefunden.
- Branch-Kontext: aktueller Branch ist `task/issue-56-df6e502b63904e7f811488c2b8a93637-bereitstellen-der-fertigen-anw`; `main` und `origin/main` zeigen im Checkout auf denselben Commit.

## Commit-Historie

Die aktuelle Historie enthaelt bereits fuer Semantic Release relevante Conventional-Commit-Typen:

- `feat: ...`
- `docs: ...`
- `plan: ...`

Die Anforderung definiert `feat`, `fix` und Breaking-Change-Markierungen als releaserelevant. Andere Typen sollen keinen Release erzeugen. Das passt zu Semantic Release mit Conventional Commits, muss aber konfiguriert werden.

## Bestehende Dokumentation mit CI-Bezug

`docs/help/playwright-tests/installation.md` enthaelt einen Beispielschritt fuer GitHub Actions, der Playwright-Traces bei Fehlern als Artefakt hochlaedt:

```yaml
- uses: actions/upload-artifact@v4
  if: failure()
  with:
    name: playwright-traces
    path: src/Schnittstellenzentrale.Tests/bin/Debug/net9.0/playwright-traces/
```

Das ist kein vorhandener Workflow, aber ein Hinweis auf erwartbare CI-Artefaktmuster. Der Pfad ist aktuell `net9.0`-bezogen und muesste bei einer Umstellung auf `net10.0` angepasst werden.

## Auswirkungen auf die Umsetzung

- Eine neue `.github/workflows/release.yml` ist erforderlich.
- Fuer Semantic Release ist eine neue Konfigurationsdatei erforderlich, vorzugsweise `release.config.cjs` oder `.releaserc`.
- Fuer reproduzierbare Node-Abhaengigkeiten ist ein neues `package.json` plus Lockdatei sinnvoll.
- Der Workflow muss auf `push` nach `main` reagieren.
- Fuer den Tag-Fallback muss der Workflow auch auf `push` von Tags nach Muster `v*.*.*` reagieren oder ein zweiter Workflow eingefuehrt werden.
- Release-Erstellung und Asset-Upload brauchen `permissions: contents: write`.
- Bei nicht releaserelevanten Commits muss der Build-/Release-Teil nach der Versionsermittlung abbrechen bzw. uebersprungen werden.

## Zu klaerende Flow-Variante

Der automatische Semantic-Release-Pfad und der manuelle Tag-Pfad haben unterschiedliche Semantik:

- Automatisch: Semantic Release berechnet die naechste Version, erstellt Tag/Release Notes und sollte nur bei releaserelevanten Commits fortfahren.
- Tag-Fallback: Der gepushte Tag ist bereits die harte Version. Der Workflow darf nicht versuchen, denselben Tag erneut zu erzeugen, muss aber Release/Asset fuer genau diese Version erstellen oder aktualisieren.

Planungsempfehlung: Im Workflow frueh zwischen Branch-Run und Tag-Run unterscheiden und eine gemeinsame Publish-/ZIP-Strecke nutzen. Der Release-Schritt kann danach getrennt behandelt werden.
