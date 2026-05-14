using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface IApplicationRepository
{
    Task<IList<ApplicationGroup>> GetGroupsAsync(StorageMode storageMode, string owner);
    Task<ApplicationGroup?> GetGroupByIdAsync(int id);
    Task<ApplicationGroup> AddGroupAsync(ApplicationGroup group);
    Task<ApplicationGroup> UpdateGroupAsync(ApplicationGroup group);
    Task DeleteGroupAsync(int id);

    Task<IList<Application>> GetApplicationsAsync(StorageMode storageMode, string owner);
    Task<IList<Application>> GetUngroupedApplicationsAsync(StorageMode storageMode, string owner);
    Task<Application?> GetApplicationByIdAsync(int id);
    Task<Application> AddApplicationAsync(Application application);
    Task<Application> UpdateApplicationAsync(Application application);
    Task DeleteApplicationAsync(int id);
}
