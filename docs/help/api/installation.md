# REST-API — Installation und Konfiguration

## Voraussetzungen

- Die Schnittstellenzentrale läuft als ASP.NET Core-Anwendung mit aktivierter Windows-Authentifizierung (Negotiate).
- `builder.Services.AddControllers()` und `app.MapControllers()` sind in `Program.cs` registriert.
- `builder.Services.AddSingleton<ITokenStore, TokenStore>()` ist registriert.
- `builder.Services.AddHttpClient<IApplicationApiClient, ApplicationApiClient>(...)` ist registriert.

Diese Registrierungen sind ab Version dieses Features im `Program.cs` der Hauptanwendung enthalten und müssen nicht manuell nachgepflegt werden.

## Konfiguration

In `appsettings.json` muss der Abschnitt `Api` vorhanden sein:

```json
{
  "Api": {
    "BaseUrl": "https://localhost:5001"
  }
}
```

| Parameter | Typ | Standardwert | Beschreibung |
|-----------|-----|--------------|--------------|
| `Api:BaseUrl` | `string` | `https://localhost:5001` | Basis-URL der eigenen REST-API; wird vom `ApplicationApiClient` als `HttpClient.BaseAddress` verwendet. Im Produktivbetrieb auf die tatsächliche Serveradresse anpassen (z. B. `https://meinserver/schnittstellenzentrale`). |

## Überprüfung

1. Anwendung starten.
2. Mit einem HTTP-Client (z. B. curl oder Postman) `POST /authenticate` an die konfigurierte Basis-URL senden:
   ```
   curl -X POST https://localhost:5001/authenticate --negotiate -u :
   ```
3. Die Antwort muss `200 OK` mit einem JSON-Objekt `{ "token": "<GUID>" }` enthalten.
4. Mit dem erhaltenen Token `POST /api/application-groups` aufrufen:
   ```
   curl -X POST https://localhost:5001/api/application-groups \
     -H "Authorization: Bearer <token>" \
     -H "X-Storage-Mode: Team" \
     -H "Content-Type: application/json" \
     -d '{"name":"TestGruppe"}'
   ```
5. Die Antwort muss `201 Created` mit einer befüllten `ApplicationGroupResponse` enthalten.
