# Speichermodus — Beschreibung

## Zweck

Der Speichermodus steuert, welcher Datensatz der Anwendung aktiv ist: geteilte Teamdaten (**Team**) oder benutzerspezifische Daten (**Benutzer**). Damit können mehrere Nutzer auf demselben Server eigene Konfigurationen pflegen, ohne die gemeinsamen Teamdaten zu beeinflussen.

Bisher wurde der gewählte Modus nur im Arbeitsspeicher gehalten und fiel bei jedem Seitenneuladen auf den Standardwert **Team** zurück. Mit dieser Erweiterung wird der Modus im `localStorage` des Browsers gespeichert und beim nächsten Aufruf automatisch wiederhergestellt.

## Funktionsweise

Der aktuell aktive Modus wird vom `StorageModeService` verwaltet. Er liest beim ersten Render den zuletzt gespeicherten Wert aus dem `localStorage` des Browsers, setzt `CurrentMode` entsprechend und löst bei einer Änderung das Event `OnModeChanged` aus. Wählt der Benutzer einen neuen Modus über das Dropdown-Menü in der Titelleiste, schreibt `StorageModeService` den neuen Wert sofort (fire-and-forget) zurück in den `localStorage`.

Die Wiederherstellung der zuletzt aktiven Systemumgebung (`RestoreEnvironmentFromLocalStorageAsync`) erfolgt sequenziell nach der Moduswiederherstellung, damit der korrekte Umgebungsschlüssel (`selectedEnvironmentId_Team` bzw. `selectedEnvironmentId_User`) verwendet wird.

## Beispiele

- Der Benutzer wählt erstmals den Modus **Benutzer** und lädt die Seite neu: Die Anwendung startet direkt im Modus **Benutzer**, ohne dass der Benutzer die Auswahl wiederholen muss.
- Ein Teammitglied arbeitet standardmäßig mit dem Modus **Team**: Die Anwendung startet immer im Modus **Team**, da das der gespeicherte Wert ist.
- Kein gespeicherter Wert vorhanden (z. B. erster Besuch): Die Anwendung startet im Standardmodus **Team**.

## Einschränkungen

- Die Präferenz ist browserspezifisch und gerätespezifisch; sie wird nicht zwischen verschiedenen Browsern oder Geräten synchronisiert.
- Der gespeicherte Wert wird bei Browserstart sofort eingelesen, ohne einen Seitenflackerer zu verhindern (anders als beim Dark Mode, der bereits vor dem ersten Blazor-Render wirksam werden kann).
- Standardfallback ist **Team**, wenn kein oder ein ungültiger Wert in `localStorage` vorhanden ist.
