namespace Schnittstellenzentrale.Core.Helpers;

/// <summary>Erstellt aufgelöste URLs aus einem Pfad-Template und einer Liste von Parametern.</summary>
public static class EndpointUrlBuilder
{
    /// <summary>
    /// Gibt die aufgelöste URL zurück: Platzhalter im <paramref name="path"/> werden durch den zugehörigen Parameterwert ersetzt;
    /// verbleibende Parameter werden als Query-String angehängt.
    /// Einträge mit leerem Key werden übersprungen.
    /// </summary>
    public static string Resolve(string path, IEnumerable<(string Key, string Value)> parameters)
    {
        var queryParams = new List<(string Key, string Value)>();

        foreach (var (key, value) in parameters)
        {
            if (string.IsNullOrWhiteSpace(key))
                continue;

            if (path.Contains("{" + key + "}"))
                path = path.Replace("{" + key + "}", Uri.EscapeDataString(value));
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
