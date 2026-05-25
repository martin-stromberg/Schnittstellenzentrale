using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Services;

internal static class ImportDiffCalculator
{
    internal static ImportDiff Calculate(IList<Core.Models.Endpoint> existing, IList<Core.Models.Endpoint> imported)
    {
        var diff = new ImportDiff();
        if (!TryBuildDictionary(existing, out var existingByKey, out var existingDuplicateKey))
            return new ImportDiff { ErrorMessage = $"Duplizierter Endpoint-Schlüssel im Bestand: {existingDuplicateKey}" };

        if (!TryBuildDictionary(imported, out var importedByKey, out var importedDuplicateKey))
            return new ImportDiff { ErrorMessage = $"Duplizierter Endpoint-Schlüssel im Import: {importedDuplicateKey}" };

        foreach (var kvp in importedByKey)
        {
            if (!existingByKey.ContainsKey(kvp.Key))
                diff.NewEndpoints.Add(kvp.Value);
            else
            {
                var existingEndpoint = existingByKey[kvp.Key];
                if (HasChanged(existingEndpoint, kvp.Value))
                    diff.ChangedEndpoints.Add(MergeExistingIdentity(existingEndpoint, kvp.Value));
            }
        }

        foreach (var kvp in existingByKey)
        {
            if (!importedByKey.ContainsKey(kvp.Key))
                diff.RemovedEndpoints.Add(kvp.Value);
        }

        return diff;
    }

    private static bool TryBuildDictionary(
        IEnumerable<Core.Models.Endpoint> endpoints,
        out Dictionary<string, Core.Models.Endpoint> dictionary,
        out string? duplicateKey)
    {
        dictionary = [];
        duplicateKey = null;

        foreach (var endpoint in endpoints)
        {
            var key = BuildKey(endpoint);
            if (!dictionary.TryAdd(key, endpoint))
            {
                duplicateKey = key;
                return false;
            }
        }

        return true;
    }

    private static string BuildKey(Core.Models.Endpoint endpoint) => $"{endpoint.Method}:{endpoint.RelativePath}";

    private static bool HasChanged(Core.Models.Endpoint existing, Core.Models.Endpoint imported)
    {
        return existing.Name != imported.Name
            || existing.Body != imported.Body
            || existing.AuthenticationType != imported.AuthenticationType
            || existing.PreRequestScript != imported.PreRequestScript
            || existing.PostRequestScript != imported.PostRequestScript;
    }

    private static Core.Models.Endpoint MergeExistingIdentity(Core.Models.Endpoint existing, Core.Models.Endpoint imported)
    {
        return new Core.Models.Endpoint
        {
            Id = existing.Id,
            Name = imported.Name,
            Method = imported.Method,
            RelativePath = imported.RelativePath,
            Body = imported.Body,
            AuthenticationType = imported.AuthenticationType,
            ApplicationId = imported.ApplicationId,
            EndpointGroupId = existing.EndpointGroupId,
            RowVersion = existing.RowVersion,
            Headers = existing.Headers,
            QueryParameters = existing.QueryParameters,
            PreRequestScript = imported.PreRequestScript,
            PostRequestScript = imported.PostRequestScript
        };
    }
}
