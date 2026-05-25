using System.Text.Json;
using System.Xml.Linq;

namespace Schnittstellenzentrale.Core.Helpers;

/// <summary>Gemeinsame Parsing-Hilfsmethoden für <c>ScriptRequestData</c> und <c>ScriptResponseData</c>.</summary>
internal static class ScriptBodyParser
{
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
