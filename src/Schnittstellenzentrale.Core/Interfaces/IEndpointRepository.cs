using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface IEndpointRepository
{
    Task<IList<Endpoint>> GetEndpointsAsync(int applicationId);
    Task<IList<Endpoint>> GetEndpointsByApplicationIdsAsync(IEnumerable<int> applicationIds);
    Task<Endpoint?> GetEndpointByIdAsync(int id);
    Task<IList<Endpoint>> GetEndpointByNameAsync(int applicationId, string name);
    Task<Endpoint> AddEndpointAsync(Endpoint endpoint);
    Task<Endpoint> UpdateEndpointAsync(Endpoint endpoint);
    Task DeleteEndpointAsync(int id);

    Task<IList<EndpointGroup>> GetEndpointGroupsAsync(int applicationId);
    Task<EndpointGroup?> GetEndpointGroupByIdAsync(int id);
    Task<EndpointGroup> AddEndpointGroupAsync(EndpointGroup group);
    Task<EndpointGroup> UpdateEndpointGroupAsync(EndpointGroup group);
    Task DeleteEndpointGroupAsync(int id);

    Task<EndpointHeader> AddHeaderAsync(EndpointHeader header);
    Task DeleteHeaderAsync(int id);

    Task<EndpointQueryParameter> AddQueryParameterAsync(EndpointQueryParameter parameter);
    Task DeleteQueryParameterAsync(int id);
}
