namespace Schnittstellenzentrale.Core.Helpers;

/// <summary>Erstellt aufgelöste URLs aus einem Pfad-Template und einer Liste von Parametern.</summary>
public static class EndpointUrlBuilder
{
    /// <summary>
    /// Gibt die aufgelöste URL zurück: Platzhalter im <paramref name="path"/> werden durch den zugehörigen Parameterwert ersetzt;
    /// verbleibende Parameter werden als Query-String angehängt.
    /// Einträge mit leerem Key werden übersprungen.
    /// </summary>
    /// <param name="keepEmptyPlaceholders">
    /// Wenn <c>true</c>, werden Platzhalter mit leerem Wert nicht ersetzt (Anzeige-Modus).
    /// Wenn <c>false</c> (Standard), wird ein leerer Wert als leerer String eingesetzt (Request-Modus).
    /// </param>
    public static string Resolve(string path, IEnumerable<(string Key, string Value)> parameters, bool keepEmptyPlaceholders = false)
    {
        var queryParams = new List<(string Key, string Value)>();

        foreach (var (key, value) in parameters)
        {
            if (string.IsNullOrWhiteSpace(key))
                continue;

            var placeholder = "{" + key + "}";
            if (path.Contains(placeholder))
            {
                if (!keepEmptyPlaceholders || !string.IsNullOrEmpty(value))
                    path = path.Replace(placeholder, Uri.EscapeDataString(value));
            }
            else
                queryParams.Add((key, value));
        }

        if (queryParams.Count == 0)
            return path;

        var query = string.Join("&", queryParams.Select(p =>
            $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        return path + "?" + query;
    }
}
