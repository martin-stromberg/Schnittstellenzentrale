using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Helpers;

internal static class EndpointGroupHelper
{
    internal static async Task<int?> ResolveGroupIdAsync(
        string relativePath,
        int applicationId,
        IEndpointRepository endpointRepository,
        Dictionary<(string Name, int? ParentGroupId), EndpointGroup> groupLookup)
    {
        int? parentGroupId = null;
        foreach (var segment in ParseGroupSegments(relativePath))
        {
            var key = (segment, parentGroupId);
            if (!groupLookup.TryGetValue(key, out var endpointGroup))
            {
                endpointGroup = await endpointRepository.AddEndpointGroupAsync(new EndpointGroup
                {
                    Name = segment,
                    ApplicationId = applicationId,
                    ParentGroupId = parentGroupId
                });
                groupLookup[key] = endpointGroup;
            }
            parentGroupId = endpointGroup.Id;
        }
        return parentGroupId;
    }

    internal static IEnumerable<string> ParseGroupSegments(string relativePath)
    {
        foreach (var segment in relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            if (segment.Equals("api", StringComparison.OrdinalIgnoreCase))
                continue;
            if (segment.StartsWith('{'))
                continue;
            yield return segment;
        }
    }
}
