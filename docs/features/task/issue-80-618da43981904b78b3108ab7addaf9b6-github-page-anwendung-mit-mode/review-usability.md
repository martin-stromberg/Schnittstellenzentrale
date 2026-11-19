# Usability-Review

## Ergebnis

**Status:** Keine Befunde

## Befunde

Keine. Die Seite wurde real geprüft: statischer Server (`python -m http.server` auf `site/`), alle referenzierten Ressourcen liefern HTTP 200, alle externen Links (Repository, `releases/latest`, `docs/help`, `LEGAL.md`, shields.io-Badge) sind erreichbar (HTTP 200 nach Redirect). Rendering in Hell- und Dunkel-Darstellung sowie bei 375-px-Viewport per Headless-Chromium-Screenshot verifiziert; die bestehenden `SitePageTests` (5 Tests: Sektionen, Bildladung, Download-Link, Theme-Toggle inkl. Persistenz über Reload, kein horizontaler Overflow bei 390 px) laufen grün.

Anmerkungen ohne Befundcharakter:

- Der Theme-Umschalter ist ein reines Symbol (☾/☀) ohne sichtbare Textbeschriftung, trägt aber einen deutschen `title`-Tooltip und `aria-pressed` — gängiges Muster, für die Zielgruppe zumutbar.
- Fachbegriffe wie „SignalR", „CSDL-Metadaten", „CRUD", `$metadata` oder `/odatav4` sowie der Platzhalter `Schnittstellenzentrale-vX.Y.Z.zip` sind sichtbar; das Produkt ist ein Entwickler-/Administrationswerkzeug und die Begriffe stammen aus der Anforderung selbst — zielgruppengerecht, keine internen Kennungen (keine GUIDs, Ids oder Pfad-Artefakte).

## Geprüfte Interaktionen

Liste der aus der Anforderung geprüften Benutzerinteraktionen:

- Seite besuchen und Zweck der Anwendung verstehen (Hero, Klartext-Einleitung) → unauffällig
- Feature-Übersicht lesen (9 Feature-Kacheln) → unauffällig
- Screenshots ansehen (Galerie mit 4 echten App-Screenshots, hell + dunkel, Captions und Alt-Texte) → unauffällig
- Release/Download aufrufen (Hero-Button „Jetzt herunterladen" + Download-Panel „Release herunterladen" → `releases/latest`) → unauffällig
- Repository, Dokumentation (`docs/help`) und Rechtliches (`LEGAL.md`) über Header/Footer aufrufen → unauffällig
- Interne Anker-Navigation (Features/Screenshots/Download) → unauffällig
- Zwischen Hell- und Dunkel-Darstellung umschalten (Klick toggelt `.dark`, persistiert in `localStorage`, Icon und Tooltip wechseln, `prefers-color-scheme`-Fallback beim Erstbesuch) → unauffällig
- Seite auf schmalem Viewport nutzen (375 px: Umbruch der Kacheln, kein horizontaler Scroll) → unauffällig
- Sichtbare Fehlerzustände (kaputte Bildpfade, leere Sektionen, nicht ladendes Release-Badge) → unauffällig

## Geprüfte Dateien

Liste aller geprüften UI-Dateien:

- `site/index.html`
- `site/site.css`
- `site/theme.js`
- `site/favicon.png`
- `site/screenshots/home-light.png`
- `site/screenshots/application-light.png`
- `site/screenshots/endpoint-light.png`
- `site/screenshots/home-dark.png`
