using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface IHistoryService
{
    Task AddEntryAsync(EndpointCallHistoryEntry entry);
    Task<(IList<EndpointCallHistoryEntry> Items, int TotalCount)> GetPagedAsync(HistoryFilter filter, int page, int pageSize);
    Task<IList<TopEndpointResult>> GetTopEndpointsAsync(int applicationId, int count);
}

public record HistoryFilter(int? ApplicationId, int? EndpointId, DateTime? From, DateTime? To);

public record TopEndpointResult(int EndpointId, string? RelativePath, string? HttpMethod, int CallCount);
