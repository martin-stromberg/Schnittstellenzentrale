using System.Text.Json;
using System.Xml.Linq;

namespace Schnittstellenzentrale.Core.Models;

/// <summary>Snapshot der HTTP-Antwort für die Skriptausführung.</summary>
public class ScriptResponseData
{
    /// <summary>Der Antwort-Body (optional).</summary>
    public string? Body { get; set; }

    /// <summary>Die Antwort-Header.</summary>
    public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

    /// <summary>Parst den Body als JSON und gibt ein traversierbares Objekt zurück.</summary>
    public object? AsJson()
    {
        if (string.IsNullOrEmpty(Body))
            return null;
        var element = JsonSerializer.Deserialize<JsonElement>(Body);
        return ScriptRequestData.ConvertJsonElement(element);
    }

    /// <summary>Parst den Body als XML und gibt eine verschachtelte Objektstruktur zurück.</summary>
    public object? AsXml()
    {
        if (string.IsNullOrEmpty(Body))
            return null;
        var doc = XDocument.Parse(Body);
        return ScriptRequestData.ConvertXmlToObject(doc.Root);
    }
}
