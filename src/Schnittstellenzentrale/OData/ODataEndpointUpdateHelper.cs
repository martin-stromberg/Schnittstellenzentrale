using System.Text.Json;
using System.Text.Json.Serialization;
using Endpoint = Schnittstellenzentrale.Core.Models.Endpoint;

namespace Schnittstellenzentrale.OData;

/// <summary>Hilfsmethoden zum Einlesen von Endpunkt-Update-Bodies inklusive mitgelieferter Kind-Mengen (<c>headers</c>/<c>queryparameters</c>).</summary>
internal static class ODataEndpointUpdateHelper
{
    private static readonly JsonSerializerOptions EndpointJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    /// <summary>Deserialisiert ein JSON-Dokument in ein <see cref="Endpoint"/>-Objekt.</summary>
    /// <param name="document">Das JSON-Dokument.</param>
    /// <param name="endpoint">Das deserialisierte Objekt.</param>
    /// <param name="error">Die Fehlermeldung.</param>
    /// <returns><c>true</c> wenn erfolgreich; <c>false</c> mit Fehlermeldung bei ungültigem JSON.</returns>
    public static bool TryDeserializeEndpoint(JsonElement document, out Endpoint? endpoint, out string? error)
    {
        endpoint = null;
        error = null;

        if (document.ValueKind != JsonValueKind.Object)
        {
            error = "Der Endpunkt-Body muss ein JSON-Objekt sein.";
            return false;
        }

        try
        {
            endpoint = document.Deserialize<Endpoint>(EndpointJsonOptions);
        }
        catch (JsonException ex)
        {
            error = $"Ungültiger Endpunkt-Body: {ex.Message}";
            return false;
        }

        if (endpoint == null)
        {
            error = "Ungültiger Endpunkt-Body.";
            return false;
        }

        return true;
    }

    /// <summary>Prüft, ob eine Eigenschaft (Groß-/Kleinschreibung ignoriert) im JSON-Objekt vorhanden ist.</summary>
    /// <param name="document">Das JSON-Objekt.</param>
    /// <param name="propertyName">Der Eigenschaftsname.</param>
    /// <returns><c>true</c> wenn die Eigenschaft vorhanden ist, sonst <c>false</c>.</returns>
    public static bool ContainsProperty(JsonElement document, string propertyName)
    {
        if (document.ValueKind != JsonValueKind.Object)
            return false;

        foreach (var prop in document.EnumerateObject())
        {
            if (string.Equals(prop.Name, propertyName, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    /// <summary>Liest eine Kind-Menge (<c>headers</c>/<c>queryparameters</c>) als Liste von Key/Value-Paaren aus einem Array-Element.</summary>
    /// <param name="element">Das JSON-Element (Array, <c>null</c> oder leer).</param>
    /// <param name="propertyName">Der Eigenschaftsname für Fehlermeldungen.</param>
    /// <param name="entries">Die gelesenen Key/Value-Paare; <c>null</c> bei Fehler.</param>
    /// <param name="error">Die Fehlermeldung.</param>
    /// <returns><c>true</c> wenn erfolgreich; <c>false</c> mit Fehlermeldung bei ungültigem Typ.</returns>
    public static bool TryReadChildren(JsonElement element, string propertyName, out List<KeyValuePair<string, string>>? entries, out string? error)
    {
        entries = null;
        error = null;

        if (element.ValueKind == JsonValueKind.Null)
        {
            entries = [];
            return true;
        }

        if (element.ValueKind != JsonValueKind.Array)
        {
            error = $"'{propertyName}' muss ein Array aus {{key, value}}-Objekten sein.";
            return false;
        }

        var result = new List<KeyValuePair<string, string>>();
        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object)
            {
                error = $"'{propertyName}'-Einträge müssen Objekte mit 'key' und 'value' sein.";
                return false;
            }

            string? key = null;
            string? value = null;
            foreach (var member in item.EnumerateObject())
            {
                if (string.Equals(member.Name, "key", StringComparison.OrdinalIgnoreCase))
                {
                    if (member.Value.ValueKind != JsonValueKind.String)
                    {
                        error = $"'{propertyName}'-Einträge benötigen ein String-Feld 'key'.";
                        return false;
                    }
                    key = member.Value.GetString();
                }
                else if (string.Equals(member.Name, "value", StringComparison.OrdinalIgnoreCase))
                {
                    if (member.Value.ValueKind != JsonValueKind.String && member.Value.ValueKind != JsonValueKind.Null)
                    {
                        error = $"'{propertyName}'-Einträge benötigen ein String-Feld 'value'.";
                        return false;
                    }
                    value = member.Value.ValueKind == JsonValueKind.Null ? null : member.Value.GetString();
                }
            }

            if (key == null)
            {
                error = $"'{propertyName}'-Einträge benötigen ein String-Feld 'key'.";
                return false;
            }

            result.Add(new KeyValuePair<string, string>(key, value ?? string.Empty));
        }

        entries = result;
        return true;
    }
}
