using System.Text.Json;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.OData;

/// <summary>Gemeinsame Patch-Hilfsmethoden für OData-Controller.</summary>
internal static class ODataPatchHelper
{
    /// <summary>Wendet einen JSON-Patch auf eine <see cref="Application"/> an.</summary>
    /// <returns><c>true</c> wenn erfolgreich; <c>false</c> mit Fehlermeldung bei ungültigem Feldwert.</returns>
    public static bool TryApplyPatch(JsonElement patch, Application target, out string? error)
    {
        error = null;
        foreach (var prop in patch.EnumerateObject())
        {
            switch (prop.Name.ToLowerInvariant())
            {
                case "name": target.Name = prop.Value.GetString() ?? target.Name; break;
                case "description": target.Description = prop.Value.GetString() ?? target.Description; break;
                case "baseurl": target.BaseUrl = prop.Value.GetString() ?? target.BaseUrl; break;
                case "interfaceurl": target.InterfaceUrl = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetString(); break;
                case "owner": target.Owner = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetString(); break;
                case "applicationgroupid": target.ApplicationGroupId = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetInt32(); break;
                case "subtitle": target.Subtitle = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetString(); break;
                case "icondata":
                    if (!TryApplyIconData(prop.Value, v => target.IconData = v, out error))
                        return false;
                    break;
            }
        }
        return true;
    }

    /// <summary>Wendet einen JSON-Patch auf eine <see cref="ApplicationGroup"/> an.</summary>
    /// <returns><c>true</c> wenn erfolgreich; <c>false</c> mit Fehlermeldung bei ungültigem Feldwert.</returns>
    public static bool TryApplyPatch(JsonElement patch, ApplicationGroup target, out string? error)
    {
        error = null;
        foreach (var prop in patch.EnumerateObject())
        {
            switch (prop.Name.ToLowerInvariant())
            {
                case "name": target.Name = prop.Value.GetString() ?? target.Name; break;
                case "description": target.Description = prop.Value.GetString() ?? target.Description; break;
                case "subtitle": target.Subtitle = prop.Value.ValueKind == JsonValueKind.Null ? null : prop.Value.GetString(); break;
                case "icondata":
                    if (!TryApplyIconData(prop.Value, v => target.IconData = v, out error))
                        return false;
                    break;
            }
        }
        return true;
    }

    /// <summary>Liest den <c>IconData</c>-Wert aus einem JSON-Patch und setzt ihn auf dem Zielobjekt.</summary>
    /// <returns><c>true</c> wenn erfolgreich; <c>false</c> mit Fehlermeldung bei ungültigem Base64.</returns>
    public static bool TryApplyIconData(JsonElement prop, Action<byte[]?> setter, out string? error)
    {
        error = null;
        if (prop.ValueKind == JsonValueKind.Null)
        {
            setter(null);
            return true;
        }

        var raw = prop.GetString();
        if (raw == null)
        {
            error = "IconData muss ein gültiger Base64-String sein.";
            return false;
        }

        try
        {
            setter(Convert.FromBase64String(raw));
            return true;
        }
        catch (FormatException)
        {
            error = "IconData muss ein gültiger Base64-String sein.";
            return false;
        }
    }
}
