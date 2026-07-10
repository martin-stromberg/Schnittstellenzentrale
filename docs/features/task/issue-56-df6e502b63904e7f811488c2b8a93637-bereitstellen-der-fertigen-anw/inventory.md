# Bestandsaufnahme: Bereitstellen der fertigen Anwendung

Hinweis zur Ausfuehrung: Der Lifecycle-Schritt 4 wurde in dieser Umgebung direkt durch Codex ausgefuehrt, da keine Unteragenten verfuegbar sind.

## Kurzfazit

Das Repository enthaelt aktuell keine GitHub-Actions-Workflows und keine Semantic-Release-Konfiguration. Die Anwendung ist eine ASP.NET-Core/Blazor-Server-Anwendung mit vier .NET-Projekten, die alle auf `net9.0` zielen. Die Anforderung verlangt dagegen einen .NET-10-Publish; lokal ist ein .NET-10-SDK vorhanden, die Projektdateien und Dokumentation sind aber noch auf .NET 9 ausgerichtet.

Der zu publishende Projektpfad ist eindeutig `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`. Das README dokumentiert bereits einen manuellen Publish nach `publish/`:

```powershell
dotnet publish src/Schnittstellenzentrale/Schnittstellenzentrale.csproj -c Release -o publish/
```

## Detaildokumente

- [CI/CD und Release-Konfiguration](inventory/ci-cd-release.md)
- [.NET-Projekte und Publish-Bestand](inventory/dotnet-publish.md)

## Relevante Repository-Struktur

| Pfad | Bedeutung fuer die Anforderung |
|---|---|
| `Schnittstellenzentrale.slnx` | Solution mit allen vier Projekten unter `src/`. |
| `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj` | Haupt-Webprojekt und Publish-Ziel. |
| `src/Schnittstellenzentrale.Core/Schnittstellenzentrale.Core.csproj` | Domain-/Core-Bibliothek, referenziert vom Webprojekt und Infrastructure. |
| `src/Schnittstellenzentrale.Infrastructure/Schnittstellenzentrale.Infrastructure.csproj` | Infrastruktur-/EF-Core-Bibliothek, referenziert vom Webprojekt. |
| `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj` | xUnit-, Integration- und Playwright-Tests. |
| `README.md` | Dokumentiert .NET 9, Installation, Publish-Befehl und Testbefehle. |
| `docs/help/schnittstellenzentrale/installation.md` | Installationsdokumentation fuer IIS und .NET-9-Runtime. |
| `docs/help/playwright-tests/installation.md` | Test-/CI-Hinweise fuer Playwright-Artefakte. |
| `.gitignore` | Ignoriert `publish/`, Build-Artefakte, `node_modules/`, Logs und Datenbankdateien. |

## CI/CD-Bestand

- Keine `.github/`-Struktur vorhanden.
- Keine GitHub-Actions-Workflow-Dateien vorhanden.
- Keine bestehende Release-Pipeline vorhanden.
- Keine `.releaserc`, `release.config.js`, `release.config.cjs`, `package.json` oder Lockdatei vorhanden.
- Keine vorhandenen Git-Tags im lokalen Checkout.
- Die Git-Historie enthaelt bereits Conventional-Commit-aehnliche Nachrichten wie `feat: ...`, `docs: ...` und `plan: ...`; damit ist die Commit-Historie grundsaetzlich fuer Semantic Release auswertbar.

## .NET- und Publish-Bestand

- Alle Projektdateien verwenden `TargetFramework` = `net9.0`.
- Es gibt kein `global.json`.
- Lokal ist das SDK `10.0.301` installiert.
- Das Webprojekt nutzt `Microsoft.NET.Sdk.Web` und referenziert Core und Infrastructure.
- Das Testprojekt enthaelt ein `InstallPlaywright`-Target, das nach Builds Chromium installiert, sofern `SkipPlaywrightInstall` nicht `true` ist.
- Fuer reine Release-Publish-Jobs muss das Testprojekt nicht gebaut werden, wenn direkt das Webprojekt publiziert wird.

## Abgleich mit der Anforderung

| Anforderung | Bestand | Konsequenz fuer Planung |
|---|---|---|
| Workflow bei Push auf `main` | Nicht vorhanden | Neue Datei unter `.github/workflows/` anlegen. |
| Manueller Tag-Fallback `vX.Y.Z` | Nicht vorhanden | Workflow muss zusaetzlich auf passende Tags reagieren oder einen separaten Pfad fuer Tag-Runs enthalten. |
| Semantic Release | Nicht vorhanden | Konfiguration und Node/npm-Bootstrap ergaenzen, ohne bestehende Buildstruktur zu stoeren. |
| GitHub Release mit Notes | Nicht vorhanden | Semantic-Release-/GitHub- oder GitHub-CLI-Flow festlegen. |
| ZIP aus komplettem Publish-Verzeichnis | Manuelles Publish nach `publish/` dokumentiert | CI-Publish-Verzeichnis eindeutig definieren, z. B. `artifacts/publish`, danach ZIP erzeugen. |
| .NET-10-Publish | Projektdateien `net9.0`, lokales SDK 10 vorhanden | Klaeren/planen, ob TargetFrameworks auf `net10.0` angehoben werden oder nur `setup-dotnet` SDK 10 nutzt. Ein echter `net10.0`-Publish erfordert Projektumstellung. |
| Keine Releases bei nicht releaserelevanten Commits | Keine Konfiguration | Semantic Release muss so eingebunden werden, dass Publish/ZIP/Release-Upload bei "no release" uebersprungen werden. |

## Risiken und offene technische Entscheidungen

- **.NET-10-Widerspruch:** Die Anforderung fordert .NET 10, Bestand und README nennen .NET 9. Ein Workflow mit .NET-10-SDK kann `net9.0` bauen, erfuellt aber keinen echten `net10.0`-Publish. Fuer Akzeptanzkriterium 6 ist wahrscheinlich eine TargetFramework-Anhebung auf `net10.0` noetig.
- **Package-Management fuer Semantic Release:** Es gibt derzeit kein Node-Projekt. Entweder werden `package.json`/Lockdatei eingefuehrt oder Semantic Release wird ad hoc per `npx` ausgefuehrt. Reproduzierbarkeit spricht fuer versionierte npm-Konfiguration.
- **Tag-Fallback vs. Semantic-Release-Automatik:** Semantic Release erzeugt normalerweise selbst Tags. Der manuelle `vX.Y.Z`-Fallback muss so getrennt werden, dass ein existierender Tag nicht erneut erzeugt werden muss und trotzdem Release Notes/Asset-Upload reproduzierbar funktionieren.
- **Asset-Ersetzung:** Bei Tag-Runs soll ein Release erstellt oder aktualisiert werden. Der Plan muss festlegen, ob bestehende Assets mit gleichem Namen ersetzt werden.
- **Berechtigungen:** Der Workflow benoetigt mindestens `contents: write` fuer Releases/Tags. Projektspezifische Secrets sind nach aktuellem Bestand nicht erforderlich.
- **Remote-Sicherheit:** Der lokale `origin` ist ein GitHub-HTTPS-Remote mit eingebetteten Zugangsdaten. Diese Information sollte nicht in neue Konfigurationsdateien uebernommen werden; Actions nutzt stattdessen `GITHUB_TOKEN`.

## Empfohlene naechste Planungsgrundlagen

- Hauptprojekt fuer Release-Publish: `src/Schnittstellenzentrale/Schnittstellenzentrale.csproj`.
- Artefaktname: naheliegend `Schnittstellenzentrale-v<version>.zip`.
- Publish-Ausgabepfad im Workflow: besser unter `artifacts/publish` oder `${{ runner.temp }}/publish` statt dem gitignorierten Root-`publish/`.
- ZIP-Inhalt: Dateien aus dem Publish-Verzeichnis direkt auf ZIP-Wurzelebene packen.
- Test-/Build-Pruefung vor Release: mindestens `dotnet publish ... -c Release`; optional vorher `dotnet test --filter "FullyQualifiedName!~Playwright" -p:SkipPlaywrightInstall=true`, falls die Release-Pipeline auch Tests absichern soll.
