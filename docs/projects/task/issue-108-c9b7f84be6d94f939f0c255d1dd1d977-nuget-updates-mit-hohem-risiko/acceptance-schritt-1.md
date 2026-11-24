# Abnahmeprüfung – Entwicklungsschritt 1

## Ergebnis

**Status:** Anforderung vollständig erfüllt

## Abweichungen

Keine.

## Hinweise

Zweiter Durchlauf nach Korrektur. Erster Bericht: `acceptance-schritt-1.1.md`
(einzige Abweichung: „ASP.NET Core 10 · Entity Framework Core 10"-Fließtext in
`README.md:163`). Korrektur auf dem Schritt-Branch umgesetzt und verifiziert:
Grep findet keine verbleibenden „ASP.NET Core 10"-/„Entity Framework Core 10"-
Nennungen in `README.md`, `docs/help` und `src` außer dem bewusst gepinnten
„Microsoft.AspNetCore.OData 10 Preview" und historischen Dokumenten.

Aus dem ersten Durchlauf unverändert gültige Nachweise: `dotnet build` unter
SDK 11.0.100 fehlerfrei; `dotnet test --filter "Category!=OsInterface"` 641/641
grün; `ODataControllerIntegrationTests` 78/78 grün; `dotnet list package
--vulnerable` ohne Befund; alle 17 Floatings auf `11.0.*`; 9 CI-Pins auf
`11.0.x`; E2E real geprüft (GET / → 200, POST /authenticate → 200,
GET /odatav4/Applications → 200).
