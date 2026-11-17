# Planprüfung

**Status: Plan vollständig**

Geprüft: `plan.md` gegen `requirement.md` und `inventory.md` (inkl. Detaildokumente).

## Anforderungsabdeckung

| Anforderung | Plan-Abdeckung | Status |
|-------------|----------------|--------|
| Zentrale Definition der Auslösepolitik (genau eine Nachricht, nur Team-Modus) | `INotificationPolicy`/`NotificationPolicy` als einzige Stelle mit Team-Prüfung | ✅ |
| Einheitliche Anwendung in allen mutierenden API-Pfaden | Umstellung aller REST-Controller + alle vier OData-Controller + Import-Apply | ✅ |
| OData-Nachrüstung wie REST-Pendants (Baum vs. entitätsspezifisch) | Policy-Mapping-Tabelle deckt alle 16 mutierenden OData-Ops ab | ✅ |
| Doppelte Benachrichtigung `EndpointPage` entfällt | Schritt 6 (Entfernen von Inject + Aufruf) | ✅ |
| Transport unverändert | Kein Eingriff in Hub/Gruppen/Events | ✅ |
| Fachliche Wirkung: REST, OData, UI, Import, Startabgleich | Alle fünf Quellen adressiert | ✅ |
| Integrationstests für OData-Benachrichtigungen (inhaltlich) | `ODataNotificationIntegrationTests` + exponierter Mock | ✅ |
| Bestehende Suiten grün | Anpassungen an betroffene Tests eingeplant | ✅ |

## Geprüfte Risiken und deren Abdeckung im Plan

- **Direkte Controller-Instanziierung**: `ODataControllerNullApplicationTests` instanziiert `ODataEndpointsController`/`ODataEndpointGroupsController` direkt — Konstruktor-Änderung ist als Schritt 8 im Plan verankert. ✅
- **DI-Auflösung im Sync-Service**: `SystemEndpointSyncServiceTests` baut einen eigenen `ServiceProvider` — Registrierung des `INotificationPolicy`-Mocks ist im Plan. ✅
- **Playwright-DI**: `PlaywrightApiFactory`/`PlaywrightServer` ersetzen nur `ISignalRNotificationService`; `INotificationPolicy` löst über `Program.cs`-Registrierung den Proxy auf — kompatibel. ✅
- **Parameterermittlung**: `applicationId`-Auflösung für Header-/Query-Parameter-Deletes und OData-Mutationen ist in `inventory/mutating-paths.md` dokumentiert und im Plan referenziert. ✅
- **„Genau eine Nachricht"**: Import-Apply/Startabgleich senden `TreeChanged` (eine Nachricht für mehrentitäts-übergreifende Mutation); Leer-Diff-Ausnahme ist definiert. ✅
- **Keine Benachrichtigung bei Fehlschlägen**: Alle Notify-Aufrufe erfolgen ausschließlich nach erfolgreicher Persistierung (Plan + Akzeptanzszenarien). ✅
- **E2E-Nachweis**: `SignalRSyncTests` (echter Kestrel + zwei Browser-Kontexte) bleibt der reale UI-Nachweis; Plan benennt ihn explizit. ✅

## Verifizierbarkeit

Akzeptanzszenarien sind vollständig: Happy Path vom externen Auslöser (HTTP-Request mit Headern) bis zur Wirkung (genau eine Hub-Nachricht, Client-Reload), Ausgangszustände mit fehlenden Objekten (NotFound/403/400-Pfade), Modus-Differenzierung (Team vs. User). Wiederaufnahmewege bei Async/Wartesituationen sind nicht relevant (synchrone Request-Verarbeitung).

## Ergebnis

Der Plan deckt die Anforderung vollständig ab, enthält überprüfbare Akzeptanzszenarien und berücksichtigt alle identifizierten Seiteneffekte auf Bestandstests. **Plan vollständig.**
