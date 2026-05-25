# Endpunkte

Endpunkte sind einzelne HTTP-Anfragen, die einer Anwendung zugeordnet sind. Sie können Pfad-Platzhalter (`{name}`), Query-Parameter und optionale JavaScript-Skripte enthalten. Pre-Request-Skripte werden vor dem Senden ausgeführt und können Umgebungsvariablen setzen oder andere Endpunkte aufrufen; Post-Request-Skripte verarbeiten die Antwort. Die Schnittstellenzentrale analysiert den relativen Pfad automatisch, trennt Platzhalter von regulären Query-Parametern und zeigt die aufgelöste URL im Pfadfeld an.

## Inhalt

- [Beschreibung](beschreibung.md)
- [Technischer Ablauf](ablauf-technisch.md)
- [Ablauf für Anwender](ablauf-anwender.md)
- [API](api.md)
- [Business Rules](business-rules.md)
