# Bestandsaufnahme — Schritt 7: Microsoft.Playwright 1.52.0 → 1.65.1

## Ausgangszustand

- `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj:29` — `Microsoft.Playwright` **1.52.0**; `TargetFramework net11.0`; `InstallPlaywright`-Target (Zeile 39–41): `pwsh bin\$(Configuration)\net11.0\playwright.ps1 install chromium`, abschaltbar via `-p:SkipPlaywrightInstall=true`.
- Playwright-Testcode ausschließlich unter `src/Schnittstellenzentrale.Tests/Playwright/` — 22 Testdateien + `Infrastructure/` (15 Dateien).
- Alle 24 Playwright-Testklassen tragen `[OsInterface]` (best-effort-CI-Lane); reguläre Lane: 641 Tests Baseline.
- Lokale Umgebung: .NET SDK 11.0.100, pwsh 7.6.10, installierte Browser-Revisionen chromium-1169…1255 unter `%LOCALAPPDATA%/ms-playwright/`.

## Release-Notes-Sichtung 1.53–1.65.1

Vollständige Tabelle und Detailprüfung: [inventory/release-notes-befund.md](inventory/release-notes-befund.md).

**Zusammenfassung:** Releases 1.53, 1.54, 1.61–1.63 ohne Breaking Changes. Breaking Changes in 1.55 (Extension-MV2), 1.56 (`BackgroundPage`), 1.57 (`Page.Accessibility` entfernt, Chrome-for-Testing-Builds), 1.58 (`_react`/`_vue`/`:light`-Selektoren, `Devtools`-Option), 1.59 (macOS-14-WebKit), 1.60 (`Handle`-Option `ExposeBindingAsync`, Docker-Image), 1.64 (versteckte iframes = hidden), 1.65 (5 Punkte: inert-Warten, RFC-6265-Cookies, ungerenderter Text in Accessible Names, `ParamsString`, Proxy-Credentials).

**Keine der entfernten APIs wird im Repo verwendet** (grep-Audit: `BackgroundPage`, `Page.Accessibility`, `_react`, `_vue`, `:light`, `Devtools`, `ExposeBinding`, `APIRequestContext`, `ParamsString`, `AriaSnapshot`, `Cookie` → jeweils 0 Treffer). Die 1.65-Verhaltensänderungen (inert, Cookies, iframe, ungerenderter Text) treffen keine im Repo vorhandenen Muster — App nutzt kein `inert`, keine Cookies, keine iframes; einziges `<details>` (ActivityLogPanel) wird nicht per Role-Name adressiert. Restrisiko: Accessible-Name-Berechnung einzelner `GetByRole`-Locators sowie Chrome-for-Testing-Umstellung — verifiziert durch den verbindlichen lokalen OsInterface-Lauf.

## Betroffene Dateien (geplant)

- `src/Schnittstellenzentrale.Tests/Schnittstellenzentrale.Tests.csproj` — PackageReference-Version (einzige sicher geplante Änderung).
- Test-/Infrastruktur-Dateien unter `Playwright/` — nur falls der lokale Lauf echte (nicht-flaky) Brüche zeigt.
- `docs/help/stabilitaet/os-interface-tests.md` — ggf. Ergänzung (Playwright-Version, lokales Gate).
- `docs/RELEASE_NOTES.md` — Eintrag Schritt 7.

## Constraints

- Kein Push/PR/Merge; Endzustand = Abschlusscommit auf diesem Branch.
- OsInterface-Lane ist die reale E2E-Abnahme (echtes Chromium + Kestrel) — Budget für Build + Browser-Download + ~24 Testklassen einplanen.
- Lokales Gate dokumentieren: `dotnet test --filter "Category=OsInterface"` lokal grün = Merge-Pflicht (im PR-Text festzuhalten).
