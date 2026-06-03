using Schnittstellenzentrale.Core.Interfaces;
using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Infrastructure.Services;

/// <summary>Scoped-Service, der die aktuell aktive <see cref="SystemEnvironment"/> und die materialisierten Variablen hält.</summary>
public class ActiveEnvironmentService : IActiveEnvironmentService
{
    /// <inheritdoc/>
    public SystemEnvironment? ActiveEnvironment { get; private set; }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> ActiveVariables { get; private set; } = new Dictionary<string, string>();

    /// <inheritdoc/>
    public event Action? OnActiveEnvironmentChanged;

    /// <inheritdoc/>
    public event Action? OnEnvironmentListChanged;

    /// <inheritdoc/>
    public void SetActiveEnvironment(SystemEnvironment? environment)
    {
        ActiveEnvironment = environment;
        ActiveVariables = environment != null
            ? environment.Variables.ToDictionary(v => v.Name, v => v.Value)
            : new Dictionary<string, string>();
        OnActiveEnvironmentChanged?.Invoke();
    }

    /// <inheritdoc/>
    public void NotifyEnvironmentListChanged() => OnEnvironmentListChanged?.Invoke();
}
