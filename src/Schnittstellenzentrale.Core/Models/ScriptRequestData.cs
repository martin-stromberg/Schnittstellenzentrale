using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

namespace Schnittstellenzentrale.Core.Models;

/// <summary>Snapshot der Request-Felder für die Skriptausführung.</summary>
public class ScriptRequestData
{
    /// <summary>Die aufgelöste oder rohe URL des Requests.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Die HTTP-Methode des Requests.</summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>Die Request-Header.</summary>
    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

    /// <summary>Der Request-Body (optional).</summary>
    public string? Body { get; set; }

    /// <summary>Parst den Body als JSON und gibt ein traversierbares Objekt zurück.</summary>
    public object? AsJson()
    {
        if (string.IsNullOrEmpty(Body))
            return null;
        try
        {
            var element = JsonSerializer.Deserialize<JsonElement>(Body);
            return ConvertJsonElement(element);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Ungültiger JSON-Body: {ex.Message}", ex);
        }
    }

    /// <summary>Parst den Body als XML und gibt eine verschachtelte Objektstruktur zurück.</summary>
    public object? AsXml()
    {
        if (string.IsNullOrEmpty(Body))
            return null;
        try
        {
            var doc = XDocument.Parse(Body);
            return ConvertXmlToObject(doc.Root);
        }
        catch (XmlException ex)
        {
            throw new InvalidOperationException($"Ungültiger XML-Body: {ex.Message}", ex);
        }
    }

    internal static object? ConvertJsonElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject()
                .ToDictionary(p => p.Name, p => ConvertJsonElement(p.Value)),
            JsonValueKind.Array => element.EnumerateArray()
                .Select(ConvertJsonElement)
                .ToList(),
            JsonValueKind.String => (object?)element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            _ => null
        };
    }

    internal static object? ConvertXmlToObject(XElement? element)
    {
        if (element == null)
            return null;

        var children = element.Elements().ToList();
        if (children.Count == 0)
            return (object)element.Value;

        var dict = new Dictionary<string, object?>();
        foreach (var child in children)
        {
            var childValue = ConvertXmlToObject(child);
            if (dict.ContainsKey(child.Name.LocalName))
            {
                if (dict[child.Name.LocalName] is List<object?> list)
                    list.Add(childValue);
                else
                    dict[child.Name.LocalName] = new List<object?> { dict[child.Name.LocalName], childValue };
            }
            else
            {
                dict[child.Name.LocalName] = childValue;
            }
        }
        return dict;
    }
}
