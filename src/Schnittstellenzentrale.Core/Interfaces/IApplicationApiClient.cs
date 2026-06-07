using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface IApplicationApiClient
{
    Task<IList<ApplicationGroup>> GetGroupsAsync(StorageMode storageMode, string owner);
    Task<ApplicationGroup?> GetGroupByIdAsync(int id);
    Task<ApplicationGroup> AddGroupAsync(ApplicationGroup group);
    Task<ApplicationGroup> UpdateGroupAsync(ApplicationGroup group);
    Task DeleteGroupAsync(int id);
    Task<IList<Application>> GetUngroupedApplicationsAsync(StorageMode storageMode, string owner);
    Task<Application?> GetApplicationByIdAsync(int id);
    Task<Application> AddApplicationAsync(Application application);
    Task<Application> UpdateApplicationAsync(Application application);
    Task DeleteApplicationAsync(int id);
    Task<IList<EndpointGroup>> GetEndpointGroupsAsync(int applicationId);
    Task<EndpointGroup?> GetEndpointGroupByIdAsync(int id);
    Task<EndpointGroup> AddEndpointGroupAsync(EndpointGroup group);
    Task<EndpointGroup> UpdateEndpointGroupAsync(EndpointGroup group);
    Task DeleteEndpointGroupAsync(int id);
    Task<IList<Endpoint>> GetEndpointsAsync(int applicationId, int? endpointGroupId = null);
    Task<Endpoint?> GetEndpointByIdAsync(int id);
    Task<Endpoint> AddEndpointAsync(Endpoint endpoint);
    Task<Endpoint> UpdateEndpointAsync(Endpoint endpoint);
    Task DeleteEndpointAsync(int id);
    Task<EndpointHeader> AddHeaderAsync(EndpointHeader header);
    Task DeleteHeaderAsync(int id);
    Task<EndpointQueryParameter> AddQueryParameterAsync(EndpointQueryParameter parameter);
    Task DeleteQueryParameterAsync(int id);
    Task<SystemEnvironment?> GetEnvironmentByIdAsync(int id);
    Task<ImportDiff> ImportMetadataAsync(int applicationId);
}
