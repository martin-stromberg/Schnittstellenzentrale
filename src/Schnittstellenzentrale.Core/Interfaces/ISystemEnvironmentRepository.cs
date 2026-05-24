using Schnittstellenzentrale.Core.Enums;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface ISystemEnvironmentRepository
{
    Task<IList<SystemEnvironment>> GetEnvironmentsAsync(StorageMode mode, string? owner);
    Task<SystemEnvironment?> GetByIdAsync(int id);
    Task<SystemEnvironment> AddAsync(SystemEnvironment systemEnvironment);
    Task<SystemEnvironment> UpdateAsync(SystemEnvironment systemEnvironment);
    Task DeleteAsync(int id);
}
