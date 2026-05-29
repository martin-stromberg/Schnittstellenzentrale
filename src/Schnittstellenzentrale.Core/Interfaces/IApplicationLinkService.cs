using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface IApplicationLinkService
{
    Task<IList<ApplicationLink>> GetLinksAsync(int applicationId);
    Task<ApplicationLink> AddLinkAsync(ApplicationLink link);
    Task<ApplicationLink> UpdateLinkAsync(ApplicationLink link);
    Task DeleteLinkAsync(int linkId);
}
