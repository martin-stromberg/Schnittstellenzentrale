using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Implementierung von <see cref="IApplicationLinkService"/>.</summary>
public class ApplicationLinkService : IApplicationLinkService
{
    private readonly IApplicationLinkRepository _repository;

    /// <summary>Initialisiert eine neue Instanz von <see cref="ApplicationLinkService"/>.</summary>
    public ApplicationLinkService(IApplicationLinkRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc/>
    public Task<IList<ApplicationLink>> GetLinksAsync(int applicationId)
        => _repository.GetByApplicationIdAsync(applicationId);

    /// <inheritdoc/>
    public Task<ApplicationLink> AddLinkAsync(ApplicationLink link)
        => _repository.AddAsync(link);

    /// <inheritdoc/>
    public Task<ApplicationLink> UpdateLinkAsync(ApplicationLink link)
        => _repository.UpdateAsync(link);

    /// <inheritdoc/>
    public Task DeleteLinkAsync(int linkId)
        => _repository.DeleteAsync(linkId);
}
