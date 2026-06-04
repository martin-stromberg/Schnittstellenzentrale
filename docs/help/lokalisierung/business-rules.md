# Mehrsprachigkeit DE/EN — Business Rules

## Englisch als Standardsprache und Fallback

**Beschreibung:** Englisch ist die Default-Kultur. Alle UI-Texte, die dem Anwender sichtbar sind,
mussen in Englisch verfugbar sein; Deutsch ist eine optionale Erganzung.

**Bedingungen:**
- Kein `Accept-Language`-Header im Request.
- `Accept-Language`-Header enthalt keine der unterstutzten Kulturen (`en`, `de`).
- Ein Schlüssel existiert in `SharedResources.de.resx` nicht.

**Verhalten:**
- Wenn eine der obigen Bedingungen erfullt ist: englischer Text wird angezeigt.
- Wenn `Accept-Language: de` gesetzt ist und der Schlüssel in `SharedResources.de.resx` vorhanden
  ist: deutschen Text anzeigen.

**Umsetzung:** `RequestLocalizationOptions.SetDefaultCulture("en")` in `Program.BuildWebApplicationAsync`;
`IStringLocalizer<SharedResources>` fallt bei fehlendem Schlüssel automatisch auf die neutrale
Kultur (= `SharedResources.resx`) zuruck.

---

## Kein manueller Sprachumschalter

**Beschreibung:** Die Anwendung bietet keine UI-Option zum Wechseln der Sprache. Es gibt kein
Cookie, keine Session-Variable und keine Benutzereinstellung fur die Kultur.

**Bedingungen:** Immer.

**Verhalten:**
- Die Sprache wird ausschliesslich aus dem `Accept-Language`-Header des Browsers ermittelt.
- Mochte ein Anwender die Sprache wechseln, muss er die Sprache seines Browsers andern und die
  Seite neu laden.

**Umsetzung:** Nur `AcceptLanguageHeaderRequestCultureProvider` ist aktiv; `CookieRequestCultureProvider`
und `QueryStringRequestCultureProvider` sind nicht konfiguriert und daher nicht wirksam.

---

## Ein resx-Paket pro Projekt

**Beschreibung:** Es wird pro Projekt exakt eine resx-Dateigruppe verwendet. Komponentenindividuelle
`.resx`-Dateien sind nicht erlaubt.

**Bedingungen:** Immer, wenn neue UI-Texte in Razor-Komponenten eingebunden werden.

**Verhalten:**
- Neue Schlüssel werden immer in `SharedResources.resx` / `SharedResources.de.resx` (Hauptprojekt)
  eingefugt — niemals in einer separaten komponentenspezifischen Ressourcen-Datei.
- Alle Razor-Komponenten verwenden dasselbe `IStringLocalizer<SharedResources>`.

**Umsetzung:** Konvention in `CLAUDE.md` dokumentiert; ein PostToolUse-Hook warnt vor abweichenden
Ressourcen-Dateien.

---

## Exception-Texte sind immer Englisch

**Beschreibung:** Fehlermeldungen, die uber `throw` ausgelost werden, und alle Serilog-Log-Ausgaben
sind immer auf Englisch — unabhangig von der aktiven Kultur des Anwenders.

**Bedingungen:** Immer.

**Verhalten:**
- Wenn eine Exception geworfen wird: englische Fehlermeldung.
- Wenn ein Fehler geloggt wird: englische Log-Meldung.
- Nur Fehlermeldungen, die dem Anwender direkt uber ein UI-Element (`_errorMessage`, `<ValidationMessage>`)
  angezeigt werden, sind lokalisiert.

**Umsetzung:** Konvention (nicht technisch erzwungen); alle `throw new Exception(...)` im Code
verwenden englische Strings.

---

## Schlüsselschema und Pflicht-Comment

**Beschreibung:** Jeder Ressourcen-Schlüssel folgt dem Schema `{KomponentenName}_{Rolle}` und hat
einen ausgefullten `Comment`-Eintrag in der `.resx`, der den UI-Kontext beschreibt.

**Bedingungen:** Bei jedem neuen Schlüssel.

**Verhalten:**
- Schlüssel ohne passendes Schema oder ohne `Comment` sind nicht konventionskonform.
- Gultige Rollen-Suffixe (Beispiele): `SaveButton`, `CancelButton`, `DeleteButton`, `Title`,
  `TitleNew`, `TitleEdit`, `Label_{Feld}`, `Placeholder_{Feld}`, `Tooltip_{Aktion}`, `EmptyState`,
  `Message`, `Error_{Typ}`.

**Umsetzung:** Konvention in `CLAUDE.md` dokumentiert; keine automatische Prufung zur Laufzeit.

---

## DataAnnotations-Lokalisierung uber SharedResources

**Beschreibung:** `[Required]`, `[MaxLength]` und `[Range]`-Attribute verwenden keine explizite
`ErrorMessage`; die Fehlermeldungen werden stattdessen uber `AddDataAnnotationsLocalization()`
aus `SharedResources` bezogen.

**Bedingungen:** Wenn ein Contract-Modell in einem Blazor-Formular (`EditForm`) validiert wird und
der Anwender einen Validierungsfehler ausgelost hat.

**Verhalten:**
- ASP.NET Core sucht den Validierungsfehler-Text als Schlüssel in `SharedResources` (konfigurierter
  zentraler Provider).
- Standard-Keys der DataAnnotations (z. B. `"The field {0} is required."`) sind in
  `SharedResources.de.resx` mit deutschen Texten uberschrieben.

**Umsetzung:** `AddDataAnnotationsLocalization()` in `Program.BuildWebApplicationAsync` mit
`DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(SharedResources))`.
