# Dark Mode — Beschreibung

## Zweck

Der Dark Mode erlaubt es dem Benutzer, zwischen einem hellen und einem dunklen Farbschema zu wechseln. Das schont bei schlechten Lichtverhältnissen die Augen und entspricht einer weit verbreiteten Benutzererwartung an moderne Webanwendungen.

## Funktionsweise

Das aktive Farbschema wird über das HTML-Attribut `data-bs-theme` am `<html>`-Element gesteuert. Bootstrap 5.3 wertet dieses Attribut nativ aus und passt alle Standard-Komponenten entsprechend an. Projektspezifische Farben sind als CSS Custom Properties in `app.css` definiert und wechseln automatisch mit dem Attribut.

Beim ersten Seitenaufruf liest `theme-init.js` den zuletzt gespeicherten Wert aus dem `localStorage` des Browsers und setzt `data-bs-theme` noch vor dem ersten Rendern der Blazor-Anwendung. Dadurch erscheint die Seite sofort im gewünschten Farbschema, ohne kurzzeitig im falschen Theme aufzublitzen (Flash of Unstyled Content).

Innerhalb der laufenden Sitzung verwaltet `ThemeService` das aktive Farbschema als Scoped-Service. Er informiert alle abonnierten Komponenten per `OnThemeChanged`-Event, wenn der Benutzer das Theme wechselt, sodass diese sich neu rendern.

Die gewählte Einstellung wird über `localStorage` dauerhaft im Browser gespeichert. Bei einem erneuten Besuch wird dieselbe Einstellung wiederhergestellt, ohne dass eine Anmeldung oder serverseitige Speicherung erforderlich ist.

## Beispiele

- Der Benutzer öffnet die Anwendung zum ersten Mal: Das helle Farbschema (Light) ist voreingestellt.
- Der Benutzer klickt in der Titelleiste auf das Mond-Icon (`☽`): Die Anwendung wechselt sofort in den dunklen Modus; alle Seiten und Navigationsbestandteile erscheinen in dunklen Farbtönen.
- Der Benutzer schliesst den Browser und öffnet die Anwendung erneut: Der dunkle Modus ist noch aktiv, da die Präferenz im `localStorage` gespeichert wurde.
- Der Benutzer klickt auf das Sonnen-Icon (`☀`): Die Anwendung kehrt zum hellen Modus zurück.

## Einschränkungen

- Die Präferenz ist browserspezifisch und gerätespezifisch; sie wird nicht zwischen verschiedenen Browsern oder Geräten synchronisiert.
- Das initiale Farbschema richtet sich ausschliesslich nach dem gespeicherten `localStorage`-Wert; die Betriebssystem- oder Browser-Einstellung (`prefers-color-scheme`) wird nicht automatisch ausgelesen.
- Standardfallback ist `Light`, wenn kein Wert in `localStorage` vorhanden ist.
