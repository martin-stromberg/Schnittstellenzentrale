# Konfiguration – Bestandsaufnahme

## `appsettings.json`
Datei: `src/Schnittstellenzentrale/appsettings.json`

Es ist **kein** `Impressum`-Abschnitt vorhanden. Die Datei enthält folgende Abschnitte: `Api`, `DatabaseProvider`, `ConnectionStrings`, `HealthCheck`, `Serilog`, `Upload`, `History`, `AllowedHosts`.

---

## `Program.cs`
Datei: `src/Schnittstellenzentrale/Program.cs`

`IImpressumService` und `ImpressumSettings` sind **nicht** registriert. Das Muster für Settings-Registrierung ist etabliert:

```csharp
builder.Services.Configure<UploadSettings>(builder.Configuration.GetSection("Upload"));
builder.Services.Configure<HistorySettings>(builder.Configuration.GetSection("History"));
```

---

## `HistorySettings` (Referenzmuster)
Datei: `src/Schnittstellenzentrale.Infrastructure/Services/HistorySettings.cs`

Zeigt das im Projekt verwendete Muster für eine einfache Konfigurationsklasse mit Default-Wert:

```csharp
public class HistorySettings
{
    public int DefaultPageSize { get; set; } = 50;
}
```

`ImpressumSettings` ist noch nicht vorhanden.
