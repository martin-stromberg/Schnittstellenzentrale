using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface IApplicationLinkRepository
{
    Task<IList<ApplicationLink>> GetByApplicationIdAsync(int applicationId);
    Task<ApplicationLink> AddAsync(ApplicationLink link);
    Task<ApplicationLink> UpdateAsync(ApplicationLink link);
    Task DeleteAsync(int linkId);
}
