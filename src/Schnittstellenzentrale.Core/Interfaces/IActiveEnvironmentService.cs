using Schnittstellenzentrale.Core.Models;

namespace Schnittstellenzentrale.Core.Interfaces;

public interface IActiveEnvironmentService
{
    SystemEnvironment? ActiveEnvironment { get; }
    IReadOnlyDictionary<string, string> ActiveVariables { get; }
    event Action? OnActiveEnvironmentChanged;
    event Action? OnEnvironmentListChanged;
    void SetActiveEnvironment(SystemEnvironment? environment);
    void NotifyEnvironmentListChanged();
}
